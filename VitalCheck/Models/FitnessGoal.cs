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
        /// Handles both increasing goals (e.g. running 10 km, 10,000 steps)
        /// and decreasing goals (e.g. weight reduction from 85 kg to 75 kg).
        /// </summary>
        public int CalculateProgressPercentage(decimal? baselineStartingValue = null)
        {
            if (string.Equals(Status, "Completed", StringComparison.OrdinalIgnoreCase)) return 100;
            if (TargetValue <= 0) return 0;

            if (baselineStartingValue.HasValue && baselineStartingValue.Value != TargetValue)
            {
                decimal totalSpan = Math.Abs(TargetValue - baselineStartingValue.Value);
                decimal progressSpan = Math.Abs(CurrentValue - baselineStartingValue.Value);

                if (totalSpan <= 0) return 100;
                int pct = (int)Math.Round((progressSpan / totalSpan) * 100m);
                return Math.Clamp(pct, 0, 100);
            }

            // If decreasing target (e.g. target weight 70 kg, current 70 kg or less)
            if (CurrentValue == TargetValue)
            {
                return 100;
            }

            if (TargetValue > 0 && CurrentValue > 0)
            {
                // Standard proportional percentage
                if (CurrentValue >= TargetValue && !string.Equals(Unit, "kg", StringComparison.OrdinalIgnoreCase))
                {
                    return 100;
                }

                if (string.Equals(Unit, "kg", StringComparison.OrdinalIgnoreCase) && CurrentValue <= TargetValue)
                {
                    return 100;
                }

                int direct = (int)Math.Round((CurrentValue / TargetValue) * 100m);
                return Math.Clamp(direct, 0, 100);
            }

            return 0;
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
