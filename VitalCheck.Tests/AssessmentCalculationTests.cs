using System;
using Xunit;
using VitalCheck.Models;

namespace VitalCheck.Tests
{
    /// <summary>
    /// Verifies core health diagnostic mathematics:
    /// 1. BMI calculation formula: BMI = Weight (kg) / Height (m)^2
    /// 2. Adult WHO BMI category classifications.
    /// 3. Pediatric safeguard (adult cutoffs must NOT apply to minors under 18).
    /// 4. Healthy weight reference range (WHO 18.5 to 24.9).
    /// 5. Basal Metabolic Rate (Mifflin-St Jeor formula).
    /// 6. Daily water intake and rule-based wellness feedback.
    /// </summary>
    public class AssessmentCalculationTests
    {
        [Theory]
        [InlineData(70, 175, 22.9)] // 70 / (1.75 * 1.75) = 22.857 -> 22.9
        [InlineData(85, 180, 26.2)] // 85 / (1.80 * 1.80) = 26.234 -> 26.2
        [InlineData(50, 160, 19.5)] // 50 / (1.60 * 1.60) = 19.531 -> 19.5
        [InlineData(95, 170, 32.9)] // 95 / (1.70 * 1.70) = 32.871 -> 32.9
        public void CalculateBMI_CalculatesCorrectly(decimal weightKg, decimal heightCm, decimal expectedBmi)
        {
            decimal actualBmi = Assessment.CalculateBMI(weightKg, heightCm);
            Assert.Equal(expectedBmi, actualBmi);
        }

        [Fact]
        public void CalculateBMI_ReturnsZero_WhenHeightOrWeightIsZeroOrNegative()
        {
            Assert.Equal(0m, Assessment.CalculateBMI(0, 175));
            Assert.Equal(0m, Assessment.CalculateBMI(70, 0));
            Assert.Equal(0m, Assessment.CalculateBMI(-10, 170));
            Assert.Equal(0m, Assessment.CalculateBMI(70, -170));
        }

        [Theory]
        [InlineData(17.5, 25, "Underweight")]
        [InlineData(18.5, 30, "Normal weight")]
        [InlineData(22.0, 45, "Normal weight")]
        [InlineData(24.9, 28, "Normal weight")]
        [InlineData(25.0, 35, "Overweight")]
        [InlineData(28.5, 50, "Overweight")]
        [InlineData(30.0, 22, "Obese")]
        [InlineData(35.5, 40, "Obese")]
        public void DetermineBMICategory_Adults_ReturnsAccurateClassification(decimal bmi, int age, string expectedCategory)
        {
            string category = Assessment.DetermineBMICategory(bmi, age);
            Assert.Equal(expectedCategory, category);
        }

        [Theory]
        [InlineData(16.0, 10)]
        [InlineData(21.0, 15)]
        [InlineData(28.0, 17)]
        public void DetermineBMICategory_MinorsUnder18_DoesNotApplyAdultClassification(decimal bmi, int age)
        {
            // Prompt requirement: "For children and teenagers, do not apply adult BMI classifications."
            string category = Assessment.DetermineBMICategory(bmi, age);

            Assert.Contains("Pediatric", category, StringComparison.OrdinalIgnoreCase);
            Assert.NotEqual("Normal weight", category);
            Assert.NotEqual("Underweight", category);
            Assert.NotEqual("Overweight", category);
            Assert.NotEqual("Obese", category);
        }

        [Fact]
        public void CalculateIdealWeightRange_ReturnsExpectedRangeFor180cm()
        {
            // For 180 cm (1.8m):
            // Min: 18.5 * 1.8^2 = 59.94 -> 59.9 kg
            // Max: 24.9 * 1.8^2 = 80.676 -> 80.7 kg
            Assessment.CalculateIdealWeightRange(180m, out decimal minWeight, out decimal maxWeight);

            Assert.Equal(59.9m, minWeight);
            Assert.Equal(80.7m, maxWeight);
        }

        [Fact]
        public void CalculateBMR_UsesMifflinStJeorFormula()
        {
            // Male: 10 * 75kg + 6.25 * 180cm - 5 * 30y + 5 = 750 + 1125 - 150 + 5 = 1730 kcal
            int bmrMale = Assessment.CalculateBMR(75m, 180m, 30, "Male");
            Assert.Equal(1730, bmrMale);

            // Female: 10 * 60kg + 6.25 * 165cm - 5 * 28y - 161 = 600 + 1031.25 - 140 - 161 = 1330 kcal
            int bmrFemale = Assessment.CalculateBMR(60m, 165m, 28, "Female");
            Assert.Equal(1330, bmrFemale);
        }

        [Theory]
        [InlineData(50, 1.8)]  // 50 * 0.035 = 1.75 -> 1.8 L
        [InlineData(70, 2.5)]  // 70 * 0.035 = 2.45 -> 2.5 L
        [InlineData(100, 3.5)] // 100 * 0.035 = 3.5 L
        public void CalculateRecommendedWater_ReturnsTargetHydration(decimal weightKg, decimal expectedLitres)
        {
            decimal water = Assessment.CalculateRecommendedWater(weightKg);
            Assert.Equal(expectedLitres, water);
        }

        [Fact]
        public void GenerateWellnessFeedback_IncludesMedicalDisclaimerAndLifestyleSections()
        {
            string feedback = Assessment.GenerateWellnessFeedback(
                age: 28,
                heightCm: 175m,
                weightKg: 72m,
                sex: "Male",
                bmi: 23.5m,
                bmiCategory: "Normal weight",
                activityLevel: "Moderately Active",
                sleepHours: 7.5m,
                waterIntake: 2.8m
            );

            // Prompt requirement: "Do not claim to diagnose medical conditions."
            Assert.Contains("does not replace professional medical diagnosis", feedback, StringComparison.OrdinalIgnoreCase);

            // Verifies rule-based sections
            Assert.Contains("BODY COMPOSITION", feedback);
            Assert.Contains("METABOLIC RATE", feedback);
            Assert.Contains("HYDRATION", feedback);
            Assert.Contains("SLEEP", feedback);
            Assert.Contains("ACTION PLAN", feedback);
        }
    }
}
