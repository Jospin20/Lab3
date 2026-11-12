-- ===================================================================
-- VitalCheck: Health & Fitness Diagnostic
-- Database Initialization Script (VitalCheckDB)
-- Microsoft SQL Server / LocalDB
-- ===================================================================

-- 1. Create Database if not exists
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'VitalCheckDB')
BEGIN
    CREATE DATABASE VitalCheckDB;
END
GO

USE VitalCheckDB;
GO

-- 2. Create Users Table
-- Stores credentials and account roles for both Individuals and Fitness Instructors
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'Users')
BEGIN
    CREATE TABLE Users (
        UserID INT IDENTITY(1,1) PRIMARY KEY,
        FullName NVARCHAR(100) NOT NULL,
        Email NVARCHAR(100) NOT NULL UNIQUE,
        PasswordHash NVARCHAR(256) NOT NULL,
        UserRole NVARCHAR(50) NOT NULL, -- 'Individual' or 'Instructor'
        DateRegistered DATETIME NOT NULL DEFAULT GETDATE()
    );
END
GO

-- 3. Create ClientProfiles Table
-- Stores health profiles. For an Individual, UserID points to their account.
-- For an Instructor, InstructorID links clients they manage.
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'ClientProfiles')
BEGIN
    CREATE TABLE ClientProfiles (
        ClientID INT IDENTITY(1,1) PRIMARY KEY,
        UserID INT NULL,
        InstructorID INT NULL,
        FullName NVARCHAR(100) NOT NULL,
        Age INT NOT NULL,
        Sex NVARCHAR(20) NOT NULL, -- 'Male', 'Female', 'Other'
        Height DECIMAL(5,2) NOT NULL, -- in cm (e.g., 175.50)
        Weight DECIMAL(5,2) NOT NULL, -- in kg (e.g., 72.00)
        DateCreated DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_ClientProfiles_User FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE SET NULL,
        CONSTRAINT FK_ClientProfiles_Instructor FOREIGN KEY (InstructorID) REFERENCES Users(UserID) ON DELETE NO ACTION
    );
END
GO

-- 4. Create Assessments Table
-- Stores periodic health and lifestyle diagnostic assessments
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'Assessments')
BEGIN
    CREATE TABLE Assessments (
        AssessmentID INT IDENTITY(1,1) PRIMARY KEY,
        ClientID INT NOT NULL,
        AssessmentDate DATETIME NOT NULL DEFAULT GETDATE(),
        Age INT NOT NULL,
        Height DECIMAL(5,2) NOT NULL, -- cm
        Weight DECIMAL(5,2) NOT NULL, -- kg
        BMI DECIMAL(4,1) NOT NULL,
        BMICategory NVARCHAR(50) NOT NULL,
        ActivityLevel NVARCHAR(50) NOT NULL, -- 'Sedentary', 'Lightly Active', 'Moderately Active', 'Very Active'
        SleepHours DECIMAL(3,1) NOT NULL,
        WaterIntake DECIMAL(3,1) NOT NULL, -- Litres
        Feedback NVARCHAR(MAX) NULL,
        CONSTRAINT FK_Assessments_Client FOREIGN KEY (ClientID) REFERENCES ClientProfiles(ClientID) ON DELETE CASCADE
    );
END
GO

-- 5. Create FitnessGoals Table
-- Stores quantifiable targets set by individuals or instructors for their clients
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'FitnessGoals')
BEGIN
    CREATE TABLE FitnessGoals (
        GoalID INT IDENTITY(1,1) PRIMARY KEY,
        ClientID INT NOT NULL,
        GoalDescription NVARCHAR(200) NOT NULL,
        TargetValue DECIMAL(6,2) NOT NULL,
        CurrentValue DECIMAL(6,2) NOT NULL,
        Unit NVARCHAR(20) NOT NULL DEFAULT 'kg', -- 'kg', 'steps', 'hours', etc.
        TargetDate DATE NOT NULL,
        Status NVARCHAR(30) NOT NULL DEFAULT 'In Progress', -- 'In Progress', 'Completed', 'Cancelled'
        DateCreated DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_FitnessGoals_Client FOREIGN KEY (ClientID) REFERENCES ClientProfiles(ClientID) ON DELETE CASCADE
    );
END
GO
