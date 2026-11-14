using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using VitalCheck.Data;
using VitalCheck.Models;

namespace VitalCheck.Controllers
{
    /// <summary>
    /// AssessmentController coordinates health diagnostic checkups,
    /// executes BMI evaluations and wellness feedback generation,
    /// and logs assessments to the database.
    /// </summary>
    public class AssessmentController
    {
        /// <summary>
        /// Computes diagnostic metrics (BMI, Category, and Rule-based recommendations) in memory
        /// before committing to the database, allowing live previews in the UI.
        /// </summary>
        public Assessment CalculateDiagnostic(int clientId, int age, decimal heightCm, decimal weightKg,
                                              string activityLevel, decimal sleepHours, decimal waterIntake)
        {
            decimal bmi = Assessment.CalculateBMI(weightKg, heightCm);
            string category = Assessment.DetermineBMICategory(bmi, age);
            string feedback = Assessment.GenerateWellnessFeedback(age, bmi, category, activityLevel, sleepHours, waterIntake);

            return new Assessment(
                assessmentId: 0,
                clientId: clientId,
                assessmentDate: DateTime.Now,
                age: age,
                height: heightCm,
                weight: weightKg,
                bmi: bmi,
                bmiCategory: category,
                activityLevel: activityLevel,
                sleepHours: sleepHours,
                waterIntake: waterIntake,
                feedback: feedback
            );
        }

        /// <summary>
        /// Validates and saves an assessment record into the database.
        /// Also updates the client's profile with their current height and weight.
        /// </summary>
        public bool SaveAssessment(Assessment assessment, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (assessment.ClientID <= 0)
            {
                errorMessage = "Invalid client profile associated with assessment.";
                return false;
            }

            if (assessment.Age < 5 || assessment.Age > 120)
            {
                errorMessage = "Age must be between 5 and 120.";
                return false;
            }

            if (assessment.Height < 50 || assessment.Height > 260)
            {
                errorMessage = "Height must be between 50 cm and 260 cm.";
                return false;
            }

            if (assessment.Weight < 20 || assessment.Weight > 350)
            {
                errorMessage = "Weight must be between 20 kg and 350 kg.";
                return false;
            }

            if (assessment.SleepHours < 0 || assessment.SleepHours > 24)
            {
                errorMessage = "Daily sleep duration must be between 0 and 24 hours.";
                return false;
            }

            if (assessment.WaterIntake < 0 || assessment.WaterIntake > 15)
            {
                errorMessage = "Daily water intake must be between 0 and 15 litres.";
                return false;
            }

            try
            {
                // 1. Insert assessment record
                string insertSql = @"
                    INSERT INTO Assessments (ClientID, AssessmentDate, Age, Height, Weight, BMI, BMICategory, ActivityLevel, SleepHours, WaterIntake, Feedback)
                    VALUES (@ClientID, GETDATE(), @Age, @Height, @Weight, @BMI, @BMICategory, @ActivityLevel, @SleepHours, @WaterIntake, @Feedback);
                    SELECT SCOPE_IDENTITY();";

                SqlParameter[] parameters = {
                    new SqlParameter("@ClientID", assessment.ClientID),
                    new SqlParameter("@Age", assessment.Age),
                    new SqlParameter("@Height", assessment.Height),
                    new SqlParameter("@Weight", assessment.Weight),
                    new SqlParameter("@BMI", assessment.BMI),
                    new SqlParameter("@BMICategory", assessment.BMICategory),
                    new SqlParameter("@ActivityLevel", assessment.ActivityLevel),
                    new SqlParameter("@SleepHours", assessment.SleepHours),
                    new SqlParameter("@WaterIntake", assessment.WaterIntake),
                    new SqlParameter("@Feedback", assessment.Feedback ?? "")
                };

                int newId = DatabaseHelper.ExecuteScalarInsert(insertSql, parameters);
                if (newId > 0)
                {
                    assessment.AssessmentID = newId;

                    // 2. Synchronize current Height and Weight back into ClientProfile
                    string updateProfileSql = @"
                        UPDATE ClientProfiles
                        SET Height = @Height,
                            Weight = @Weight
                        WHERE ClientID = @ClientID";

                    SqlParameter[] syncParams = {
                        new SqlParameter("@ClientID", assessment.ClientID),
                        new SqlParameter("@Height", assessment.Height),
                        new SqlParameter("@Weight", assessment.Weight)
                    };

                    DatabaseHelper.ExecuteNonQuery(updateProfileSql, syncParams);
                    return true;
                }

                errorMessage = "Failed to save assessment to database.";
                return false;
            }
            catch (Exception ex)
            {
                errorMessage = $"Database error: {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Retrieves the entire assessment history for a client, sorted newest to oldest.
        /// </summary>
        public List<Assessment> GetAssessmentsByClient(int clientId)
        {
            List<Assessment> list = new List<Assessment>();

            string query = @"
                SELECT AssessmentID, ClientID, AssessmentDate, Age, Height, Weight, BMI, BMICategory, ActivityLevel, SleepHours, WaterIntake, Feedback
                FROM Assessments
                WHERE ClientID = @ClientID
                ORDER BY AssessmentDate DESC";

            SqlParameter[] parameters = {
                new SqlParameter("@ClientID", clientId)
            };

            DataTable dt = DatabaseHelper.ExecuteQuery(query, parameters);

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new Assessment(
                    assessmentId: Convert.ToInt32(row["AssessmentID"]),
                    clientId: Convert.ToInt32(row["ClientID"]),
                    assessmentDate: Convert.ToDateTime(row["AssessmentDate"]),
                    age: Convert.ToInt32(row["Age"]),
                    height: Convert.ToDecimal(row["Height"]),
                    weight: Convert.ToDecimal(row["Weight"]),
                    bmi: Convert.ToDecimal(row["BMI"]),
                    bmiCategory: row["BMICategory"].ToString() ?? "",
                    activityLevel: row["ActivityLevel"].ToString() ?? "",
                    sleepHours: Convert.ToDecimal(row["SleepHours"]),
                    waterIntake: Convert.ToDecimal(row["WaterIntake"]),
                    feedback: row["Feedback"].ToString() ?? ""
                ));
            }

            return list;
        }

        /// <summary>
        /// Retrieves the most recent assessment for a client.
        /// </summary>
        public Assessment? GetLatestAssessment(int clientId)
        {
            string query = @"
                SELECT TOP 1 AssessmentID, ClientID, AssessmentDate, Age, Height, Weight, BMI, BMICategory, ActivityLevel, SleepHours, WaterIntake, Feedback
                FROM Assessments
                WHERE ClientID = @ClientID
                ORDER BY AssessmentDate DESC";

            SqlParameter[] parameters = {
                new SqlParameter("@ClientID", clientId)
            };

            DataTable dt = DatabaseHelper.ExecuteQuery(query, parameters);
            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                return new Assessment(
                    assessmentId: Convert.ToInt32(row["AssessmentID"]),
                    clientId: Convert.ToInt32(row["ClientID"]),
                    assessmentDate: Convert.ToDateTime(row["AssessmentDate"]),
                    age: Convert.ToInt32(row["Age"]),
                    height: Convert.ToDecimal(row["Height"]),
                    weight: Convert.ToDecimal(row["Weight"]),
                    bmi: Convert.ToDecimal(row["BMI"]),
                    bmiCategory: row["BMICategory"].ToString() ?? "",
                    activityLevel: row["ActivityLevel"].ToString() ?? "",
                    sleepHours: Convert.ToDecimal(row["SleepHours"]),
                    waterIntake: Convert.ToDecimal(row["WaterIntake"]),
                    feedback: row["Feedback"].ToString() ?? ""
                );
            }

            return null;
        }

        /// <summary>
        /// Returns total count of assessments stored for an individual or across an instructor's clients.
        /// </summary>
        public int GetTotalAssessmentsCount(int? clientId = null, int? instructorId = null)
        {
            string query;
            List<SqlParameter> pList = new List<SqlParameter>();

            if (clientId.HasValue)
            {
                query = "SELECT COUNT(*) FROM Assessments WHERE ClientID = @ClientID";
                pList.Add(new SqlParameter("@ClientID", clientId.Value));
            }
            else if (instructorId.HasValue)
            {
                query = @"
                    SELECT COUNT(*) 
                    FROM Assessments A
                    INNER JOIN ClientProfiles C ON A.ClientID = C.ClientID
                    WHERE C.InstructorID = @InstructorID";
                pList.Add(new SqlParameter("@InstructorID", instructorId.Value));
            }
            else
            {
                query = "SELECT COUNT(*) FROM Assessments";
            }

            DataTable dt = DatabaseHelper.ExecuteQuery(query, pList.ToArray());
            if (dt.Rows.Count > 0)
            {
                return Convert.ToInt32(dt.Rows[0][0]);
            }

            return 0;
        }
    }
}
