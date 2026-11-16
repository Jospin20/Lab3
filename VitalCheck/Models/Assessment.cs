using System;
using System.Text;

namespace VitalCheck.Models
{
    /// <summary>
    /// Represents a health diagnostic checkup record.
    /// Encapsulates BMI calculation, clinical BMR & daily caloric needs (Mifflin-St Jeor),
    /// adult vs pediatric classification, personalized hydration goals, and rule-based wellness recommendations.
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
        /// Calculates the healthy adult weight range for a given height based on WHO guidelines (BMI 18.5 to 24.9).
        /// </summary>
        public static void CalculateIdealWeightRange(decimal heightCm, out decimal minWeightKg, out decimal maxWeightKg)
        {
            if (heightCm <= 0)
            {
                minWeightKg = 0m;
                maxWeightKg = 0m;
                return;
            }

            decimal heightM = heightCm / 100.0m;
            minWeightKg = Math.Round(18.5m * heightM * heightM, 1);
            maxWeightKg = Math.Round(24.9m * heightM * heightM, 1);
        }

        /// <summary>
        /// Calculates Basal Metabolic Rate (BMR) using the clinically validated Mifflin-St Jeor formula.
        /// </summary>
        public static int CalculateBMR(decimal weightKg, decimal heightCm, int age, string sex)
        {
            if (weightKg <= 0 || heightCm <= 0 || age <= 0) return 0;

            // Base formula: 10 * weight(kg) + 6.25 * height(cm) - 5 * age(y)
            decimal baseBmr = (10m * weightKg) + (6.25m * heightCm) - (5m * age);

            if (string.Equals(sex, "Male", StringComparison.OrdinalIgnoreCase))
            {
                baseBmr += 5m;
            }
            else if (string.Equals(sex, "Female", StringComparison.OrdinalIgnoreCase))
            {
                baseBmr -= 161m;
            }
            else
            {
                baseBmr -= 78m; // Midpoint adjustment
            }

            return Math.Max(500, (int)Math.Round(baseBmr));
        }

        /// <summary>
        /// Calculates Total Daily Energy Expenditure (TDEE) based on activity multiplier.
        /// </summary>
        public static int CalculateTDEE(int bmr, string activityLevel)
        {
            decimal multiplier = activityLevel switch
            {
                "Sedentary" => 1.2m,
                "Lightly Active" => 1.375m,
                "Moderately Active" => 1.55m,
                "Very Active" => 1.725m,
                _ => 1.4m
            };

            return (int)Math.Round(bmr * multiplier);
        }

        /// <summary>
        /// Calculates personalized daily water requirement based on body mass (standard ~35ml per kg).
        /// </summary>
        public static decimal CalculateRecommendedWater(decimal weightKg)
        {
            if (weightKg <= 0) return 2.5m;
            decimal needed = weightKg * 0.035m;
            return Math.Round(Math.Clamp(needed, 1.5m, 4.5m), 1);
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
        /// Rule-based wellness recommendations generated using structured clinical & fitness rules.
        /// Emphasizes lifestyle habits, caloric balance, hydration, and goal setting.
        /// Explicitly clarifies that it provides wellness guidance and does not diagnose medical conditions.
        /// </summary>
        public static string GenerateWellnessFeedback(int age, decimal heightCm, decimal weightKg, string sex,
                                                      decimal bmi, string bmiCategory, string activityLevel,
                                                      decimal sleepHours, decimal waterIntake)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("══════════════════════════════════════════════════════════════════════════════");
            sb.AppendLine("                   VITALCHECK WELLNESS & DIAGNOSTIC REPORT                     ");
            sb.AppendLine("══════════════════════════════════════════════════════════════════════════════");
            sb.AppendLine("Notice: VitalCheck provides general wellness guidance only. It does not replace");
            sb.AppendLine("professional medical diagnosis or advice from a certified healthcare provider.");
            sb.AppendLine("──────────────────────────────────────────────────────────────────────────────");
            sb.AppendLine();

            // 1. BMI & Weight Assessment Feedback
            sb.AppendLine("1. BODY COMPOSITION & TARGET WEIGHT ANALYSIS");
            if (age < 18)
            {
                sb.AppendLine($"• Subject Age: {age} years. Standard adult BMI cutoffs do not apply to pediatric patients.");
                sb.AppendLine("• Adolescent Evaluation: Growth patterns during teenage years fluctuate significantly.");
                sb.AppendLine("  Focus on whole foods, sufficient dietary calcium, protein, and physical play.");
            }
            else
            {
                CalculateIdealWeightRange(heightCm, out decimal minWeight, out decimal maxWeight);
                sb.AppendLine($"• Current BMI: {bmi:F1} ({bmiCategory})");
                sb.AppendLine($"• Healthy Reference Weight for {heightCm:F0} cm: {minWeight:F1} kg – {maxWeight:F1} kg");

                switch (bmiCategory)
                {
                    case "Underweight":
                        decimal toGain = Math.Max(0, minWeight - weightKg);
                        sb.AppendLine($"• Classification: Underweight. You are approximately {toGain:F1} kg below the normal threshold.");
                        sb.AppendLine("• Recommendation: Increase caloric density with wholesome foods (nuts, avocados, whole grains)");
                        sb.AppendLine("  and incorporate progressive resistance training to build lean muscle mass.");
                        break;
                    case "Normal weight":
                        sb.AppendLine("• Classification: Normal weight. Excellent! You are within the recommended healthy range.");
                        sb.AppendLine("• Recommendation: Maintain your current balanced lifestyle, active habits, and consistent meals.");
                        break;
                    case "Overweight":
                        decimal toLose = Math.Max(0, weightKg - maxWeight);
                        sb.AppendLine($"• Classification: Overweight. You are approximately {toLose:F1} kg above the upper normal limit.");
                        sb.AppendLine("• Recommendation: A moderate caloric deficit (300-500 kcal/day) paired with 150+ minutes");
                        sb.AppendLine("  of moderate cardiovascular and strength exercise will sustainably guide your weight.");
                        break;
                    case "Obese":
                        decimal toLoseOb = Math.Max(0, weightKg - maxWeight);
                        sb.AppendLine($"• Classification: Obese. Body mass exceeds reference range by approximately {toLoseOb:F1} kg.");
                        sb.AppendLine("• Recommendation: Prioritize gentle, sustainable milestones (0.5 kg loss per week).");
                        sb.AppendLine("  Focus on portion control, minimizing processed sugars, and consulting a physician.");
                        break;
                }
            }
            sb.AppendLine();

            // 2. Metabolic Energy & Caloric Estimation
            int bmr = CalculateBMR(weightKg, heightCm, age, sex);
            int tdee = CalculateTDEE(bmr, activityLevel);
            sb.AppendLine("2. METABOLIC RATE & ENERGY EXPENDITURE (Mifflin-St Jeor)");
            sb.AppendLine($"• Basal Metabolic Rate (BMR): ~{bmr:N0} kcal/day (energy burned at complete rest)");
            sb.AppendLine($"• Maintenance Calories (TDEE): ~{tdee:N0} kcal/day based on {activityLevel} lifestyle");
            if (bmi >= 25.0m && age >= 18)
            {
                sb.AppendLine($"• Weight Loss Target Deficit: ~{tdee - 450:N0} kcal/day (safe, sustainable fat loss rate)");
            }
            else if (bmi < 18.5m && age >= 18)
            {
                sb.AppendLine($"• Weight Gain Target Surplus: ~{tdee + 350:N0} kcal/day (gradual, lean mass accrual)");
            }
            sb.AppendLine();

            // 3. Hydration Evaluation
            decimal recWater = CalculateRecommendedWater(weightKg);
            sb.AppendLine("3. HYDRATION STATUS");
            sb.AppendLine($"• Reported Water Intake: {waterIntake:F1} L/day  |  Personalized Target: {recWater:F1} L/day");
            if (waterIntake < recWater - 0.5m)
            {
                sb.AppendLine("• Deficit Detected: You are drinking less water than your body mass requires.");
                sb.AppendLine("  Tip: Keep a water bottle near your desk and drink 1 glass upon waking and before each meal.");
            }
            else if (waterIntake <= recWater + 1.0m)
            {
                sb.AppendLine("• Optimal Hydration: Great job! Your water intake aligns with healthy hydration levels.");
            }
            else
            {
                sb.AppendLine("• High Intake: Ensure your electrolyte intake (sodium, potassium) is adequate if consuming high volumes.");
            }
            sb.AppendLine();

            // 4. Sleep & Recovery Analysis
            sb.AppendLine("4. SLEEP & CIRCADIAN RECOVERY");
            sb.AppendLine($"• Reported Sleep: {sleepHours:F1} hours/day");
            if (sleepHours < 7.0m)
            {
                sb.AppendLine("• Sleep Debt Detected: Chronic sleep under 7 hours elevates cortisol and impairs recovery.");
                sb.AppendLine("  Tip: Limit screen time 45 minutes before bedtime and maintain a dark, cool sleeping space.");
            }
            else if (sleepHours <= 9.0m)
            {
                sb.AppendLine("• Ideal Sleep Duration: 7-9 hours provides prime cellular repair, memory consolidation, and hormone balance.");
            }
            else
            {
                sb.AppendLine("• Extended Sleep: Ensure sleep quality is restorative and monitor for daytime drowsiness.");
            }
            sb.AppendLine();

            // 5. Physical Activity Plan
            sb.AppendLine("5. PHYSICAL ACTIVITY ADVICE");
            switch (activityLevel)
            {
                case "Sedentary":
                    sb.AppendLine("• Goal: Break prolonged sitting with 5-minute movement breaks every hour.");
                    sb.AppendLine("  Aim for a daily 20-30 minute brisk walk to activate cardiovascular and lymphatic health.");
                    break;
                case "Lightly Active":
                    sb.AppendLine("• Goal: Build on your daily routine. Add 2 sessions of fundamental strength exercises");
                    sb.AppendLine("  (bodyweight squats, push-ups, planks) weekly to preserve bone density.");
                    break;
                case "Moderately Active":
                    sb.AppendLine("• Goal: Excellent adherence to recommended guidelines! Incorporate interval training");
                    sb.AppendLine("  or increase resistance load progressively to stimulate adaptation.");
                    break;
                case "Very Active":
                    sb.AppendLine("• Goal: Athletic commitment. Prioritize joint mobility, adequate rest days,");
                    sb.AppendLine("  and post-workout nutrition to avoid overtraining and injury.");
                    break;
            }

            return sb.ToString();
        }

        // Backward compatibility overload
        public static string GenerateWellnessFeedback(int age, decimal bmi, string bmiCategory, string activityLevel, decimal sleepHours, decimal waterIntake)
        {
            return GenerateWellnessFeedback(age, 175m, 70m, "Male", bmi, bmiCategory, activityLevel, sleepHours, waterIntake);
        }
    }
}
