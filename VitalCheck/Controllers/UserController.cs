using System;
using System.Data;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using VitalCheck.Data;
using VitalCheck.Models;

namespace VitalCheck.Controllers
{
    /// <summary>
    /// UserController acts as the bridge between user-facing authentication forms
    /// (LoginForm, RegisterForm) and the database operations for User accounts.
    /// It enforces input validation, hashing, duplicate prevention, and role handling.
    /// </summary>
    public class UserController
    {
        /// <summary>
        /// Registers a new user account (Individual or Instructor).
        /// If role is 'Individual', an initial linked ClientProfile is automatically created.
        /// </summary>
        public bool Register(string fullName, string email, string password, string role, out string errorMessage)
        {
            errorMessage = string.Empty;

            // 1. Validation
            if (string.IsNullOrWhiteSpace(fullName))
            {
                errorMessage = "Full Name is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                errorMessage = "Email address is required.";
                return false;
            }

            if (!IsValidEmail(email))
            {
                errorMessage = "Please enter a valid email address (e.g., user@example.com).";
                return false;
            }

            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            {
                errorMessage = "Password must be at least 6 characters long.";
                return false;
            }

            if (role != "Individual" && role != "Instructor")
            {
                errorMessage = "Please select a valid role (Individual or Instructor).";
                return false;
            }

            try
            {
                // 2. Check for duplicate email
                string checkEmailSql = "SELECT COUNT(*) FROM Users WHERE LOWER(Email) = LOWER(@Email)";
                SqlParameter[] checkParams = {
                    new SqlParameter("@Email", email.Trim())
                };

                DataTable checkDt = DatabaseHelper.ExecuteQuery(checkEmailSql, checkParams);
                if (checkDt.Rows.Count > 0 && Convert.ToInt32(checkDt.Rows[0][0]) > 0)
                {
                    errorMessage = "An account with this email address already exists. Please log in or use a different email.";
                    return false;
                }

                // 3. Hash the password
                string passwordHash = DatabaseHelper.HashPassword(password);

                // 4. Insert user record into database
                string insertUserSql = @"
                    INSERT INTO Users (FullName, Email, PasswordHash, UserRole, DateRegistered)
                    VALUES (@FullName, @Email, @PasswordHash, @UserRole, GETDATE());
                    SELECT SCOPE_IDENTITY();";

                SqlParameter[] insertParams = {
                    new SqlParameter("@FullName", fullName.Trim()),
                    new SqlParameter("@Email", email.Trim().ToLower()),
                    new SqlParameter("@PasswordHash", passwordHash),
                    new SqlParameter("@UserRole", role)
                };

                int newUserId = DatabaseHelper.ExecuteScalarInsert(insertUserSql, insertParams);

                if (newUserId <= 0)
                {
                    errorMessage = "Registration failed while saving user details.";
                    return false;
                }

                // 5. If the new user is an Individual, automatically create their ClientProfile
                if (role == "Individual")
                {
                    string insertProfileSql = @"
                        INSERT INTO ClientProfiles (UserID, InstructorID, FullName, Age, Sex, Height, Weight, DateCreated)
                        VALUES (@UserID, NULL, @FullName, 25, 'Male', 170.00, 70.00, GETDATE());";

                    SqlParameter[] profileParams = {
                        new SqlParameter("@UserID", newUserId),
                        new SqlParameter("@FullName", fullName.Trim())
                    };

                    DatabaseHelper.ExecuteNonQuery(insertProfileSql, profileParams);
                }

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = $"Database error during registration: {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Authenticates a user by email and password.
        /// Returns the User object upon success.
        /// </summary>
        public bool Login(string email, string password, out User? user, out string errorMessage)
        {
            user = null;
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                errorMessage = "Please enter both email and password.";
                return false;
            }

            try
            {
                string passwordHash = DatabaseHelper.HashPassword(password);

                string query = @"
                    SELECT UserID, FullName, Email, PasswordHash, UserRole, DateRegistered
                    FROM Users
                    WHERE LOWER(Email) = LOWER(@Email) AND PasswordHash = @PasswordHash";

                SqlParameter[] parameters = {
                    new SqlParameter("@Email", email.Trim()),
                    new SqlParameter("@PasswordHash", passwordHash)
                };

                DataTable dt = DatabaseHelper.ExecuteQuery(query, parameters);

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    user = new User(
                        userId: Convert.ToInt32(row["UserID"]),
                        fullName: row["FullName"].ToString() ?? "",
                        email: row["Email"].ToString() ?? "",
                        passwordHash: row["PasswordHash"].ToString() ?? "",
                        userRole: row["UserRole"].ToString() ?? "Individual",
                        dateRegistered: Convert.ToDateTime(row["DateRegistered"])
                    );
                    return true;
                }
                else
                {
                    errorMessage = "Invalid email or password. Please verify your credentials.";
                    return false;
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"Login error: {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Retrieves the linked ClientProfile for an individual user.
        /// </summary>
        public ClientProfile? GetClientProfileByUserId(int userId)
        {
            string query = @"
                SELECT ClientID, UserID, InstructorID, FullName, Age, Sex, Height, Weight, DateCreated
                FROM ClientProfiles
                WHERE UserID = @UserID";

            SqlParameter[] parameters = {
                new SqlParameter("@UserID", userId)
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
        /// Helper for basic email format validation.
        /// </summary>
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            // Simple and reliable standard pattern for beginner email validation
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }
    }
}
