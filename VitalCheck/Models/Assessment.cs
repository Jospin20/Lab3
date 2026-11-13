using System;
using System.Text;

namespace VitalCheck.Models
{
    /// <summary>
    /// Represents a health diagnostic checkup record.
    /// Encapsulates BMI calculation, adult vs teen classification rules,
    /// and rule-based wellness recommendations.
    /// </summary>
    public class Assessment
    {
        public int AssessmentID { get; set; }
        public int ClientID { get; set; }
        public DateTime AssessmentDate { get; set; } = DateTime.Now;
        public int Age { get; set; }
        public decimal Height { get; set; } // in cm
        public decimal Weight { get; set; } // in kg
        public decimal BMI { get; set; }
        public string BMICategory { get; set; } = string.Empty;
        public string ActivityLevel { get; set; } = "Moderately Active";
        public decimal SleepHours { get; set; } = 8.0m;
        public decimal WaterIntake { get; set; } = 2.5m; // Litres
        public string Feedback { get; set; } = string.Empty;

        public Assessment()
        {
        }

        public Assessment(int assessmentId, int clientId, DateTime assessmentDate, int age, decimal height, decimal weight,
                          decimal bmi, string bmiCategory, string activityLevel, decimal sleepHours, decimal waterIntake, string feedback)
        {
            AssessmentID = assessmentId;
            ClientID = clientId;
            AssessmentDate = assessmentDate;
            Age = age;
            Height = height;
            Weight = weight;
            BMI = bmi;
            BMICategory = bmiCategory;
            ActivityLevel = activityLevel;
            SleepHours = sleepHours;
            WaterIntake = waterIntake;
            Feedback = feedback;
        }

        /// <summary>
        /// Calculates Body Mass Index (BMI) using the standard metric formula:
        /// BMI = Weight (kg) / [Height (m)]^2
        /// </summary>
        public static decimal CalculateBMI(decimal weightKg, decimal heightCm)
        {
            if (heightCm <= 0 || weightKg <= 0) return 0m;
            decimal heightM = heightCm / 100.0m;
            decimal bmi = weightKg / (heightM * heightM);
            return Math.Round(bmi, 1);
        }

        /// <summary>
        /// Determines the BMI category.
        /// Important medical rule: Adult BMI classifications are NOT applied to children/teens (under 18).
        /// </summary>
        public static string DetermineBMICategory(decimal bmi, int age)
        {
            if (bmi <= 0) return "N/A";

            if (age < 18)
            {
                return "Pediatric / Adolescent (Percentile-based, adult ranges do not apply)";
            }

            if (bmi < 18.5m)
            {
                return "Underweight";
            }
            else if (bmi < 25.0m)
            {
                return "Normal weight";
            }
            else if (bmi < 30.0m)
            {
                return "Overweight";
            }
            else
            {
                return "Obese";
            }
        }

        /// <summary>
        /// Rule-based wellness recommendations generated using basic C# conditional logic.
        /// Emphasizes lifestyle habits, sleep hygiene, hydration, and goal setting.
        /// Explicitly clarifies that it does not diagnose medical conditions.
        /// </summary>
        public static string GenerateWellnessFeedback(int age, decimal bmi, string bmiCategory, string activityLevel, decimal sleepHours, decimal waterIntake)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("HEALTH & WELLNESS SUMMARY");
            sb.AppendLine("----------------------------------------------------------------------");
            sb.AppendLine("* Note: VitalCheck provides general wellness guidance and does not diagnose medical conditions.");
            sb.AppendLine();

            // 1. BMI & Weight Assessment Feedback
            sb.AppendLine("[Body Composition & BMI]");
            if (age < 18)
            {
                sb.AppendLine("• For children and teenagers under 18, standard adult BMI categories do not apply. Clinical evaluation relies on age-and-sex specific growth percentiles. Focus on balanced nutrition and active play.");
            }
            else
            {
                switch (bmiCategory)
                {
                    case "Underweight":
                        sb.AppendLine($"• Your calculated BMI is {bmi:F1} ({bmiCategory}). Consider incorporating nutrient-dense, calorie-rich wholesome foods and strength training. Consult a healthcare provider if experiencing unintentional weight loss.");
                        break;
                    case "Normal weight":
                        sb.AppendLine($"• Your calculated BMI is {bmi:F1} ({bmiCategory}). Congratulations on maintaining a healthy weight range! Continue balanced dietary habits and regular exercise.");
                        break;
                    case "Overweight":
                        sb.AppendLine($"• Your calculated BMI is {bmi:F1} ({bmiCategory}). A slight calorie deficit paired with 150+ minutes of moderate weekly cardio and resistance training can help guide your body to its ideal range.");
                        break;
                    case "Obese":
                        sb.AppendLine($"• Your calculated BMI is {bmi:F1} ({bmiCategory}). Consider setting gradual, sustainable fitness targets (e.g., losing 0.5 kg per week). Focus on portion control, whole foods, and consulting a physician before high-intensity workouts.");
                        break;
                    default:
                        sb.AppendLine($"• BMI calculated at {bmi:F1}.");
                        break;
                }
            }
            sb.AppendLine();

            // 2. Physical Activity Feedback
            sb.AppendLine("[Physical Activity]");
            if (activityLevel == "Sedentary")
            {
                sb.AppendLine("• Current Activity: Sedentary. Try introducing 20-30 minutes of brisk walking each day or taking standing breaks every hour to enhance cardiovascular circulation.");
            }
            else if (activityLevel == "Lightly Active")
            {
                sb.AppendLine("• Current Activity: Lightly Active. Good starting point! Adding 2 days of basic resistance exercises (bodyweight squats, push-ups) can accelerate fitness gains.");
            }
            else if (activityLevel == "Moderately Active")
            {
                sb.AppendLine("• Current Activity: Moderately Active. Excellent! You meet recommended physical activity guidelines. Ensure progressive overload if aiming for specific performance targets.");
            }
            else if (activityLevel == "Very Active")
            {
                sb.AppendLine("• Current Activity: Very Active. Outstanding commitment! Ensure you incorporate proper rest days and dynamic stretching to prevent overtraining and injury.");
            }
            sb.AppendLine();

            // 3. Sleep Hygiene Feedback
            sb.AppendLine("[Sleep Habits]");
            if (sleepHours < 7.0m)
            {
                sb.AppendLine($"• Average Sleep: {sleepHours:F1} hrs/day. You may be experiencing mild sleep debt. Strive for 7-9 hours of consistent sleep nightly to optimize muscular recovery and metabolic regulation.");
            }
            else if (sleepHours <= 9.0m)
            {
                sb.AppendLine($"• Average Sleep: {sleepHours:F1} hrs/day. Optimal sleep duration! Consistent bedtimes and waking times further strengthen circadian rhythms.");
            }
            else
            {
                sb.AppendLine($"• Average Sleep: {sleepHours:F1} hrs/day. Slightly higher than standard requirements. Ensure your sleep quality is restful and check for afternoon fatigue.");
            }
            sb.AppendLine();

            // 4. Hydration Feedback
            sb.AppendLine("[Hydration]");
            if (waterIntake < 2.0m)
            {
                sb.AppendLine($"• Daily Water: {waterIntake:F1} L/day. Below optimal hydration levels. Aim for at least 2.0 to 2.5 litres daily to boost cognitive focus and joint lubrication.");
            }
            else if (waterIntake <= 3.5m)
            {
                sb.AppendLine($"• Daily Water: {waterIntake:F1} L/day. Great hydration habits! Maintain this intake, especially on intense workout days.");
            }
            else
            {
                sb.AppendLine($"• Daily Water: {waterIntake:F1} L/day. High hydration level. Make sure you maintain sufficient electrolyte intake when consuming large volumes of water.");
            }

            return sb.ToString();
        }
    }
}
