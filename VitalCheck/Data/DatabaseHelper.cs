using System;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.SqlClient;

namespace VitalCheck.Data
{
    /// <summary>
    /// DatabaseHelper handles low-level ADO.NET database operations.
    /// It provides straightforward methods for executing queries, parameterized commands,
    /// and ensuring the database and tables are properly initialized in SQL Server LocalDB.
    /// </summary>
    public static class DatabaseHelper
    {
        // Connection string pointing to SQL Server LocalDB
        private const string MasterConnectionString = @"Server=(localdb)\MSSQLLocalDB;Database=master;Integrated Security=True;TrustServerCertificate=True;Connect Timeout=30;";
        private const string DatabaseConnectionString = @"Server=(localdb)\MSSQLLocalDB;Database=VitalCheckDB;Integrated Security=True;TrustServerCertificate=True;Connect Timeout=30;";

        public static string ConnectionString => DatabaseConnectionString;

        /// <summary>
        /// Creates and returns a new openable SqlConnection object to the VitalCheckDB database.
        /// </summary>
        public static SqlConnection GetConnection()
        {
            return new SqlConnection(DatabaseConnectionString);
        }

        /// <summary>
        /// Automatically checks if VitalCheckDB and its tables exist, and creates them if needed.
        /// This ensures the application runs seamlessly out-of-the-box without manual SQL scripts.
        /// </summary>
        public static void InitializeDatabase()
        {
            // Step 1: Ensure VitalCheckDB database exists in the LocalDB instance
            using (SqlConnection masterConn = new SqlConnection(MasterConnectionString))
            {
                masterConn.Open();
                string checkDbSql = @"
                    IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'VitalCheckDB')
                    BEGIN
                        CREATE DATABASE VitalCheckDB;
                    END";

                using (SqlCommand cmd = new SqlCommand(checkDbSql, masterConn))
                {
                    cmd.ExecuteNonQuery();
                }
            }

            // Step 2: Ensure all required tables exist inside VitalCheckDB
            using (SqlConnection dbConn = new SqlConnection(DatabaseConnectionString))
            {
                dbConn.Open();

                string createTablesSql = @"
                    -- 1. Users Table (for Authentication & Role Authorization)
                    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'Users')
                    BEGIN
                        CREATE TABLE Users (
                            UserID INT IDENTITY(1,1) PRIMARY KEY,
                            FullName NVARCHAR(100) NOT NULL,
                            Email NVARCHAR(100) NOT NULL UNIQUE,
                            PasswordHash NVARCHAR(256) NOT NULL,
                            UserRole NVARCHAR(50) NOT NULL,
                            DateRegistered DATETIME NOT NULL DEFAULT GETDATE()
                        );
                    END

                    -- 2. ClientProfiles Table (Health profiles for Individuals & Instructor Clients)
                    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'ClientProfiles')
                    BEGIN
                        CREATE TABLE ClientProfiles (
                            ClientID INT IDENTITY(1,1) PRIMARY KEY,
                            UserID INT NULL,
                            InstructorID INT NULL,
                            FullName NVARCHAR(100) NOT NULL,
                            Age INT NOT NULL,
                            Sex NVARCHAR(20) NOT NULL,
                            Height DECIMAL(5,2) NOT NULL,
                            Weight DECIMAL(5,2) NOT NULL,
                            DateCreated DATETIME NOT NULL DEFAULT GETDATE(),
                            CONSTRAINT FK_ClientProfiles_User FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE SET NULL,
                            CONSTRAINT FK_ClientProfiles_Instructor FOREIGN KEY (InstructorID) REFERENCES Users(UserID) ON DELETE NO ACTION
                        );
                    END

                    -- 3. Assessments Table (Historical health and lifestyle diagnostic checkups)
                    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'Assessments')
                    BEGIN
                        CREATE TABLE Assessments (
                            AssessmentID INT IDENTITY(1,1) PRIMARY KEY,
                            ClientID INT NOT NULL,
                            AssessmentDate DATETIME NOT NULL DEFAULT GETDATE(),
                            Age INT NOT NULL,
                            Height DECIMAL(5,2) NOT NULL,
                            Weight DECIMAL(5,2) NOT NULL,
                            BMI DECIMAL(4,1) NOT NULL,
                            BMICategory NVARCHAR(50) NOT NULL,
                            ActivityLevel NVARCHAR(50) NOT NULL,
                            SleepHours DECIMAL(3,1) NOT NULL,
                            WaterIntake DECIMAL(3,1) NOT NULL,
                            Feedback NVARCHAR(MAX) NULL,
                            CONSTRAINT FK_Assessments_Client FOREIGN KEY (ClientID) REFERENCES ClientProfiles(ClientID) ON DELETE CASCADE
                        );
                    END

                    -- 4. FitnessGoals Table (Quantifiable health & fitness milestones)
                    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'FitnessGoals')
                    BEGIN
                        CREATE TABLE FitnessGoals (
                            GoalID INT IDENTITY(1,1) PRIMARY KEY,
                            ClientID INT NOT NULL,
                            GoalDescription NVARCHAR(200) NOT NULL,
                            TargetValue DECIMAL(6,2) NOT NULL,
                            CurrentValue DECIMAL(6,2) NOT NULL,
                            Unit NVARCHAR(20) NOT NULL DEFAULT 'kg',
                            TargetDate DATE NOT NULL,
                            Status NVARCHAR(30) NOT NULL DEFAULT 'In Progress',
                            DateCreated DATETIME NOT NULL DEFAULT GETDATE(),
                            CONSTRAINT FK_FitnessGoals_Client FOREIGN KEY (ClientID) REFERENCES ClientProfiles(ClientID) ON DELETE CASCADE
                        );
                    END
                ";

                using (SqlCommand cmd = new SqlCommand(createTablesSql, dbConn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Executes an INSERT, UPDATE, or DELETE SQL statement with parameters.
        /// Returns the number of rows affected.
        /// </summary>
        public static int ExecuteNonQuery(string query, SqlParameter[]? parameters = null)
        {
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }
                    return cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Executes an INSERT statement and retrieves the generated IDENTITY ID (e.g. SCOPE_IDENTITY()).
        /// </summary>
        public static int ExecuteScalarInsert(string query, SqlParameter[]? parameters = null)
        {
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }
                    object? result = cmd.ExecuteScalar();
                    if (result != null && int.TryParse(result.ToString(), out int id))
                    {
                        return id;
                    }
                    return 0;
                }
            }
        }

        /// <summary>
        /// Executes a SELECT query with parameters and fills a DataTable.
        /// </summary>
        public static DataTable ExecuteQuery(string query, SqlParameter[]? parameters = null)
        {
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        return dt;
                    }
                }
            }
        }

        /// <summary>
        /// Hashes a plain-text password using SHA-256 for secure storage in the database.
        /// </summary>
        public static string HashPassword(string plainPassword)
        {
            if (string.IsNullOrEmpty(plainPassword)) return string.Empty;

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(plainPassword));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
