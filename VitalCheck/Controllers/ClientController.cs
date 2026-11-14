using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using VitalCheck.Data;
using VitalCheck.Models;

namespace VitalCheck.Controllers
{
    /// <summary>
    /// ClientController manages client profiles for Fitness Instructors.
    /// It handles adding clients, searching, updating, and retrieving client profiles.
    /// </summary>
    public class ClientController
    {
        /// <summary>
        /// Retrieves all clients managed by a specific fitness instructor,
        /// with optional search filtering by full name.
        /// </summary>
        public List<ClientProfile> GetClientsByInstructor(int instructorId, string searchQuery = "")
        {
            List<ClientProfile> list = new List<ClientProfile>();

            string query = @"
                SELECT ClientID, UserID, InstructorID, FullName, Age, Sex, Height, Weight, DateCreated
                FROM ClientProfiles
                WHERE InstructorID = @InstructorID";

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                query += " AND FullName LIKE @Search";
            }

            query += " ORDER BY FullName ASC";

            List<SqlParameter> paramList = new List<SqlParameter>
            {
                new SqlParameter("@InstructorID", instructorId)
            };

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                paramList.Add(new SqlParameter("@Search", $"%{searchQuery.Trim()}%"));
            }

            DataTable dt = DatabaseHelper.ExecuteQuery(query, paramList.ToArray());

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new ClientProfile(
                    clientId: Convert.ToInt32(row["ClientID"]),
                    userId: row["UserID"] != DBNull.Value ? Convert.ToInt32(row["UserID"]) : null,
                    instructorId: row["InstructorID"] != DBNull.Value ? Convert.ToInt32(row["InstructorID"]) : null,
                    fullName: row["FullName"].ToString() ?? "",
                    age: Convert.ToInt32(row["Age"]),
                    sex: row["Sex"].ToString() ?? "Male",
                    height: Convert.ToDecimal(row["Height"]),
                    weight: Convert.ToDecimal(row["Weight"]),
                    dateCreated: Convert.ToDateTime(row["DateCreated"])
                ));
            }

            return list;
        }

        /// <summary>
        /// Retrieves a single client profile by ClientID.
        /// </summary>
        public ClientProfile? GetClientById(int clientId)
        {
            string query = @"
                SELECT ClientID, UserID, InstructorID, FullName, Age, Sex, Height, Weight, DateCreated
                FROM ClientProfiles
                WHERE ClientID = @ClientID";

            SqlParameter[] parameters = {
                new SqlParameter("@ClientID", clientId)
            };

            DataTable dt = DatabaseHelper.ExecuteQuery(query, parameters);
            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                return new ClientProfile(
                    clientId: Convert.ToInt32(row["ClientID"]),
                    userId: row["UserID"] != DBNull.Value ? Convert.ToInt32(row["UserID"]) : null,
                    instructorId: row["InstructorID"] != DBNull.Value ? Convert.ToInt32(row["InstructorID"]) : null,
                    fullName: row["FullName"].ToString() ?? "",
                    age: Convert.ToInt32(row["Age"]),
                    sex: row["Sex"].ToString() ?? "Male",
                    height: Convert.ToDecimal(row["Height"]),
                    weight: Convert.ToDecimal(row["Weight"]),
                    dateCreated: Convert.ToDateTime(row["DateCreated"])
                );
            }

            return null;
        }

        /// <summary>
        /// Adds a new client profile under an instructor.
        /// </summary>
        public bool AddClient(ClientProfile client, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (!ValidateClientInput(client, out errorMessage))
            {
                return false;
            }

            try
            {
                string insertSql = @"
                    INSERT INTO ClientProfiles (UserID, InstructorID, FullName, Age, Sex, Height, Weight, DateCreated)
                    VALUES (NULL, @InstructorID, @FullName, @Age, @Sex, @Height, @Weight, GETDATE());
                    SELECT SCOPE_IDENTITY();";

                SqlParameter[] parameters = {
                    new SqlParameter("@InstructorID", client.InstructorID.HasValue ? (object)client.InstructorID.Value : DBNull.Value),
                    new SqlParameter("@FullName", client.FullName.Trim()),
                    new SqlParameter("@Age", client.Age),
                    new SqlParameter("@Sex", client.Sex),
                    new SqlParameter("@Height", client.Height),
                    new SqlParameter("@Weight", client.Weight)
                };

                int newId = DatabaseHelper.ExecuteScalarInsert(insertSql, parameters);
                if (newId > 0)
                {
                    client.ClientID = newId;
                    return true;
                }

                errorMessage = "Failed to insert client record.";
                return false;
            }
            catch (Exception ex)
            {
                errorMessage = $"Database error: {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Updates an existing client profile.
        /// </summary>
        public bool UpdateClient(ClientProfile client, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (!ValidateClientInput(client, out errorMessage))
            {
                return false;
            }

            try
            {
                string updateSql = @"
                    UPDATE ClientProfiles
                    SET FullName = @FullName,
                        Age = @Age,
                        Sex = @Sex,
                        Height = @Height,
                        Weight = @Weight
                    WHERE ClientID = @ClientID";

                SqlParameter[] parameters = {
                    new SqlParameter("@ClientID", client.ClientID),
                    new SqlParameter("@FullName", client.FullName.Trim()),
                    new SqlParameter("@Age", client.Age),
                    new SqlParameter("@Sex", client.Sex),
                    new SqlParameter("@Height", client.Height),
                    new SqlParameter("@Weight", client.Weight)
                };

                int rows = DatabaseHelper.ExecuteNonQuery(updateSql, parameters);
                if (rows > 0)
                {
                    return true;
                }

                errorMessage = "Client profile not found or could not be updated.";
                return false;
            }
            catch (Exception ex)
            {
                errorMessage = $"Database error: {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Deletes a client profile and cascades to their assessments and goals.
        /// </summary>
        public bool DeleteClient(int clientId, out string errorMessage)
        {
            errorMessage = string.Empty;
            try
            {
                string deleteSql = "DELETE FROM ClientProfiles WHERE ClientID = @ClientID";
                SqlParameter[] parameters = {
                    new SqlParameter("@ClientID", clientId)
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
        /// Validates input values for ClientProfile.
        /// </summary>
        private static bool ValidateClientInput(ClientProfile client, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(client.FullName))
            {
                errorMessage = "Client full name is required.";
                return false;
            }

            if (client.Age < 5 || client.Age > 120)
            {
                errorMessage = "Age must be between 5 and 120.";
                return false;
            }

            if (client.Height < 50 || client.Height > 260)
            {
                errorMessage = "Height must be between 50 cm and 260 cm.";
                return false;
            }

            if (client.Weight < 20 || client.Weight > 350)
            {
                errorMessage = "Weight must be between 20 kg and 350 kg.";
                return false;
            }

            return true;
        }
    }
}
