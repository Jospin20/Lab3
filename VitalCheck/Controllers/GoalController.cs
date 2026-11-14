using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using VitalCheck.Data;
using VitalCheck.Models;

namespace VitalCheck.Controllers
{
    /// <summary>
    /// GoalController coordinates creating, tracking, updating, and completing fitness goals.
    /// Provides data retrieval and updates for both individuals and instructors.
    /// </summary>
    public class GoalController
    {
        /// <summary>
        /// Creates a new fitness goal in the database.
        /// </summary>
        public bool CreateGoal(FitnessGoal goal, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (goal.ClientID <= 0)
            {
                errorMessage = "Invalid client profile for fitness goal.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(goal.GoalDescription))
            {
                errorMessage = "Goal description is required (e.g., 'Reach target weight of 70 kg').";
                return false;
            }

            if (goal.TargetValue <= 0)
            {
                errorMessage = "Target value must be greater than zero.";
                return false;
            }

            if (goal.TargetDate.Date < DateTime.Today)
            {
                errorMessage = "Target deadline cannot be in the past.";
                return false;
            }

            try
            {
                string insertSql = @"
                    INSERT INTO FitnessGoals (ClientID, GoalDescription, TargetValue, CurrentValue, Unit, TargetDate, Status, DateCreated)
                    VALUES (@ClientID, @GoalDescription, @TargetValue, @CurrentValue, @Unit, @TargetDate, @Status, GETDATE());
                    SELECT SCOPE_IDENTITY();";

                SqlParameter[] parameters = {
                    new SqlParameter("@ClientID", goal.ClientID),
                    new SqlParameter("@GoalDescription", goal.GoalDescription.Trim()),
                    new SqlParameter("@TargetValue", goal.TargetValue),
                    new SqlParameter("@CurrentValue", goal.CurrentValue),
                    new SqlParameter("@Unit", string.IsNullOrWhiteSpace(goal.Unit) ? "kg" : goal.Unit.Trim()),
                    new SqlParameter("@TargetDate", goal.TargetDate.Date),
                    new SqlParameter("@Status", string.IsNullOrWhiteSpace(goal.Status) ? "In Progress" : goal.Status)
                };

                int newId = DatabaseHelper.ExecuteScalarInsert(insertSql, parameters);
                if (newId > 0)
                {
                    goal.GoalID = newId;
                    return true;
                }

                errorMessage = "Failed to create fitness goal.";
                return false;
            }
            catch (Exception ex)
            {
                errorMessage = $"Database error: {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Retrieves fitness goals for a client, optionally filtered by status ('All', 'In Progress', 'Completed', 'Cancelled').
        /// </summary>
        public List<FitnessGoal> GetGoalsByClient(int clientId, string statusFilter = "All")
        {
            List<FitnessGoal> list = new List<FitnessGoal>();

            string query = @"
                SELECT GoalID, ClientID, GoalDescription, TargetValue, CurrentValue, Unit, TargetDate, Status, DateCreated
                FROM FitnessGoals
                WHERE ClientID = @ClientID";

            List<SqlParameter> paramList = new List<SqlParameter>
            {
                new SqlParameter("@ClientID", clientId)
            };

            if (!string.Equals(statusFilter, "All", StringComparison.OrdinalIgnoreCase))
            {
                query += " AND Status = @Status";
                paramList.Add(new SqlParameter("@Status", statusFilter));
            }

            query += " ORDER BY TargetDate ASC";

            DataTable dt = DatabaseHelper.ExecuteQuery(query, paramList.ToArray());

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new FitnessGoal(
                    goalId: Convert.ToInt32(row["GoalID"]),
                    clientId: Convert.ToInt32(row["ClientID"]),
                    goalDescription: row["GoalDescription"].ToString() ?? "",
                    targetValue: Convert.ToDecimal(row["TargetValue"]),
                    currentValue: Convert.ToDecimal(row["CurrentValue"]),
                    unit: row["Unit"].ToString() ?? "kg",
                    targetDate: Convert.ToDateTime(row["TargetDate"]),
                    status: row["Status"].ToString() ?? "In Progress",
                    dateCreated: Convert.ToDateTime(row["DateCreated"])
                ));
            }

            return list;
        }

        /// <summary>
        /// Updates the current recorded progress value and status for a goal.
        /// </summary>
        public bool UpdateGoalProgress(int goalId, decimal newCurrentValue, string status, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                string updateSql = @"
                    UPDATE FitnessGoals
                    SET CurrentValue = @CurrentValue,
                        Status = @Status
                    WHERE GoalID = @GoalID";

                SqlParameter[] parameters = {
                    new SqlParameter("@GoalID", goalId),
                    new SqlParameter("@CurrentValue", newCurrentValue),
                    new SqlParameter("@Status", status)
                };

                int rows = DatabaseHelper.ExecuteNonQuery(updateSql, parameters);
                return rows > 0;
            }
            catch (Exception ex)
            {
                errorMessage = $"Database error: {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Marks a goal as completed.
        /// </summary>
        public bool MarkGoalCompleted(int goalId, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                string updateSql = "UPDATE FitnessGoals SET Status = 'Completed' WHERE GoalID = @GoalID";
                SqlParameter[] parameters = {
                    new SqlParameter("@GoalID", goalId)
                };

                int rows = DatabaseHelper.ExecuteNonQuery(updateSql, parameters);
                return rows > 0;
            }
            catch (Exception ex)
            {
                errorMessage = $"Database error: {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Deletes a fitness goal by GoalID.
        /// </summary>
        public bool DeleteGoal(int goalId, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                string deleteSql = "DELETE FROM FitnessGoals WHERE GoalID = @GoalID";
                SqlParameter[] parameters = {
                    new SqlParameter("@GoalID", goalId)
                };

                int rows = DatabaseHelper.ExecuteNonQuery(deleteSql, parameters);
                return rows > 0;
            }
            catch (Exception ex)
            {
                errorMessage = $"Database error: {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Gets the count of active ('In Progress') goals for an individual client or instructor's clients.
        /// </summary>
        public int GetActiveGoalsCount(int? clientId = null, int? instructorId = null)
        {
            string query;
            List<SqlParameter> pList = new List<SqlParameter>();

            if (clientId.HasValue)
            {
                query = "SELECT COUNT(*) FROM FitnessGoals WHERE ClientID = @ClientID AND Status = 'In Progress'";
                pList.Add(new SqlParameter("@ClientID", clientId.Value));
            }
            else if (instructorId.HasValue)
            {
                query = @"
                    SELECT COUNT(*) 
                    FROM FitnessGoals G
                    INNER JOIN ClientProfiles C ON G.ClientID = C.ClientID
                    WHERE C.InstructorID = @InstructorID AND G.Status = 'In Progress'";
                pList.Add(new SqlParameter("@InstructorID", instructorId.Value));
            }
            else
            {
                query = "SELECT COUNT(*) FROM FitnessGoals WHERE Status = 'In Progress'";
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
