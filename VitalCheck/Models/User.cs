using System;

namespace VitalCheck.Models
{
    /// <summary>
    /// Represents a system user (either an Individual or a Fitness Instructor).
    /// Used for authentication, session handling, and role-based permissions.
    /// </summary>
    public class User
    {
        public int UserID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string UserRole { get; set; } = "Individual"; // "Individual" or "Instructor"
        public DateTime DateRegistered { get; set; } = DateTime.Now;

        // Convenient check for role-based navigation and permissions
        public bool IsInstructor => string.Equals(UserRole, "Instructor", StringComparison.OrdinalIgnoreCase);
        public bool IsIndividual => string.Equals(UserRole, "Individual", StringComparison.OrdinalIgnoreCase);

        // Parameterless constructor (required for serialization and manual property setting)
        public User()
        {
        }

        // Parameterized constructor for easy object instantiation
        public User(int userId, string fullName, string email, string passwordHash, string userRole, DateTime dateRegistered)
        {
            UserID = userId;
            FullName = fullName;
            Email = email;
            PasswordHash = passwordHash;
            UserRole = userRole;
            DateRegistered = dateRegistered;
        }

        public override string ToString()
        {
            return $"{FullName} ({UserRole})";
        }
    }
}
