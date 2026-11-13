using System;

namespace VitalCheck.Models
{
    /// <summary>
    /// Represents a measurable fitness goal (e.g., target body weight, daily steps, weekly workout sessions).
    /// Tracks current progress, target values, deadlines, and completion statuses.
    /// </summary>
    public class FitnessGoal
    {
        public int GoalID { get; set; }
        public int ClientID { get; set; }
        public string GoalDescription { get; set; } = string.Empty;
        public decimal TargetValue { get; set; }
        public decimal CurrentValue { get; set; }
        public string Unit { get; set; } = "kg";
        public DateTime TargetDate { get; set; } = DateTime.Today.AddMonths(1);
        public string Status { get; set; } = "In Progress"; // "In Progress", "Completed", "Cancelled"
        public DateTime DateCreated { get; set; } = DateTime.Now;

        // Convenient calculated properties
        public int DaysRemaining => (TargetDate.Date - DateTime.Today).Days;
        public bool IsOverdue => DaysRemaining < 0 && Status == "In Progress";

        /// <summary>
        /// Calculates progress towards goal as an integer percentage (0 to 100).
        /// Handles both increasing goals (e.g. running 10 km) and decreasing goals (e.g. weight loss from 85kg to 75kg).
        /// </summary>
        public int CalculateProgressPercentage(decimal? initialValue = null)
        {
            if (Status == "Completed") return 100;
            if (TargetValue == 0) return 0;

            if (initialValue.HasValue && initialValue.Value != TargetValue)
            {
                decimal totalDistance = Math.Abs(TargetValue - initialValue.Value);
                decimal progressDistance = Math.Abs(CurrentValue - initialValue.Value);
                if (totalDistance == 0) return 100;
                int percent = (int)Math.Round((progressDistance / totalDistance) * 100m);
                return Math.Clamp(percent, 0, 100);
            }

            // Simple direct proportion: Current / Target
            int directPercent = (int)Math.Round((CurrentValue / TargetValue) * 100m);
            return Math.Clamp(directPercent, 0, 100);
        }

        public FitnessGoal()
        {
        }

        public FitnessGoal(int goalId, int clientId, string goalDescription, decimal targetValue, decimal currentValue,
                           string unit, DateTime targetDate, string status, DateTime dateCreated)
        {
            GoalID = goalId;
            ClientID = clientId;
            GoalDescription = goalDescription;
            TargetValue = targetValue;
            CurrentValue = currentValue;
            Unit = unit;
            TargetDate = targetDate;
            Status = status;
            DateCreated = dateCreated;
        }

        public override string ToString()
        {
            return $"{GoalDescription} ({CurrentValue}/{TargetValue} {Unit}) - [{Status}]";
        }
    }
}
