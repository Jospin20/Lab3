using System;
using Xunit;
using VitalCheck.Models;

namespace VitalCheck.Tests
{
    /// <summary>
    /// Verifies fitness goals logic:
    /// - Progress percentage calculations
    /// - Completion state
    /// - Days remaining and overdue calculations
    /// </summary>
    public class GoalLogicTests
    {
        [Fact]
        public void CalculateProgressPercentage_StandardIncreasingGoal_ComputesAccuratePercentage()
        {
            var goal = new FitnessGoal
            {
                GoalDescription = "Daily Steps",
                Unit = "steps",
                TargetValue = 10000,
                CurrentValue = 5000,
                Status = "In Progress"
            };

            int pct = goal.CalculateProgressPercentage();
            Assert.Equal(50, pct);
        }

        [Fact]
        public void CalculateProgressPercentage_WhenCompleted_Returns100Percent()
        {
            var goal = new FitnessGoal
            {
                GoalDescription = "Drink Water",
                Unit = "L",
                TargetValue = 3.0m,
                CurrentValue = 1.5m,
                Status = "Completed"
            };

            int pct = goal.CalculateProgressPercentage();
            Assert.Equal(100, pct);
        }

        [Fact]
        public void CalculateProgressPercentage_WeightLossSpan_CalculatesProperly()
        {
            // Initial weight: 90 kg, Target weight: 80 kg, Current: 85 kg
            // Progress: 5 kg lost out of 10 kg target span = 50%
            var goal = new FitnessGoal
            {
                GoalDescription = "Target Body Weight",
                Unit = "kg",
                TargetValue = 80m,
                CurrentValue = 85m,
                Status = "In Progress"
            };

            int pct = goal.CalculateProgressPercentage(baselineStartingValue: 90m);
            Assert.Equal(50, pct);
        }

        [Fact]
        public void DaysRemaining_ComputesExpectedDays()
        {
            var goal = new FitnessGoal
            {
                TargetDate = DateTime.Today.AddDays(14),
                Status = "In Progress"
            };

            Assert.Equal(14, goal.DaysRemaining);
            Assert.False(goal.IsOverdue);
        }

        [Fact]
        public void IsOverdue_ReturnsTrue_WhenDatePassedAndStillInProgress()
        {
            var goal = new FitnessGoal
            {
                TargetDate = DateTime.Today.AddDays(-5),
                Status = "In Progress"
            };

            Assert.True(goal.IsOverdue);
        }

        [Fact]
        public void IsOverdue_ReturnsFalse_WhenCompletedEvenIfPastTargetDate()
        {
            var goal = new FitnessGoal
            {
                TargetDate = DateTime.Today.AddDays(-5),
                Status = "Completed"
            };

            Assert.False(goal.IsOverdue);
        }
    }
}
