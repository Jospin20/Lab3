using System;

namespace VitalCheck.Models
{
    /// <summary>
    /// Represents a client's core health profile.
    /// An Individual user has a linked ClientProfile (UserID is set).
    /// A Fitness Instructor can manage multiple ClientProfiles (InstructorID is set).
    /// </summary>
    public class ClientProfile
    {
        public int ClientID { get; set; }
        public int? UserID { get; set; }
        public int? InstructorID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Sex { get; set; } = "Male"; // "Male", "Female", "Other"
        public decimal Height { get; set; } // Stored in centimeters (e.g., 175.00 cm)
        public decimal Weight { get; set; } // Stored in kilograms (e.g., 72.50 kg)
        public DateTime DateCreated { get; set; } = DateTime.Now;

        // Convenient calculated property: converts height from cm to meters for BMI formula
        public decimal HeightInMeters => Height > 0 ? Height / 100.0m : 0m;

        // Formatted display text for list boxes and dropdowns
        public string DisplayText => $"{FullName} - Age: {Age}, {Sex}";

        public ClientProfile()
        {
        }

        public ClientProfile(int clientId, int? userId, int? instructorId, string fullName, int age, string sex, decimal height, decimal weight, DateTime dateCreated)
        {
            ClientID = clientId;
            UserID = userId;
            InstructorID = instructorId;
            FullName = fullName;
            Age = age;
            Sex = sex;
            Height = height;
            Weight = weight;
            DateCreated = dateCreated;
        }

        public override string ToString()
        {
            return DisplayText;
        }
    }
}
