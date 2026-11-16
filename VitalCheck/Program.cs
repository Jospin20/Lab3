using System;
using System.Collections.Generic;
using System.Windows.Forms;
using VitalCheck.Controllers;
using VitalCheck.Data;
using VitalCheck.Models;
using VitalCheck.Views;

namespace VitalCheck
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the VitalCheck Health & Fitness Diagnostic application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            ApplicationConfiguration.Initialize();

            // 1. Ensure SQL Server LocalDB and tables exist
            try
            {
                DatabaseHelper.InitializeDatabase();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to initialize VitalCheck database:\n\n{ex.Message}\n\nPlease ensure Microsoft SQL Server LocalDB or SQL Server Express is installed and running.",
                    "Database Connection Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }

            // Automated Test Mode (Stage 14 verification)
            if (args.Length > 0 && args[0] == "--run-tests")
            {
                RunAutomatedDiagnostics();
                return;
            }

            // Headless initialization check
            if (args.Length > 0 && args[0] == "--init-only")
            {
                return;
            }

            // 2. Start application with the Login / Authentication screen
            Application.Run(new LoginForm());
        }

        /// <summary>
        /// Stage 14: Comprehensive Verification of database operations, business logic,
        /// MVC controllers, BMI rules, wellness feedback, and role access.
        /// </summary>
        private static void RunAutomatedDiagnostics()
        {
            Console.WriteLine("==================================================================");
            Console.WriteLine("  VITALCHECK: HEALTH & FITNESS DIAGNOSTIC - AUTOMATED TEST SUITE  ");
            Console.WriteLine("==================================================================");

            int passed = 0;
            int failed = 0;

            UserController userController = new UserController();
            ClientController clientController = new ClientController();
            AssessmentController assessmentController = new AssessmentController();
            GoalController goalController = new GoalController();

            string testEmailIndiv = $"test_indiv_{DateTime.Now.Ticks}@vitalcheck.test";
            string testEmailInst = $"test_coach_{DateTime.Now.Ticks}@vitalcheck.test";

            // Test 1: Register Individual User
            Console.Write("[Test 1] User Registration (Individual Role)... ");
            bool r1 = userController.Register("Alice Walker", testEmailIndiv, "password123", "Individual", out string err1);
            if (r1) { Console.WriteLine("PASSED"); passed++; }
            else { Console.WriteLine($"FAILED: {err1}"); failed++; }

            // Test 2: Auto-creation of ClientProfile for Individual
            Console.Write("[Test 2] Automatic ClientProfile creation for Individual... ");
            bool l1 = userController.Login(testEmailIndiv, "password123", out User? userAlice, out string errL1);
            ClientProfile? profileAlice = userAlice != null ? userController.GetClientProfileByUserId(userAlice.UserID) : null;
            if (l1 && profileAlice != null && profileAlice.FullName == "Alice Walker") { Console.WriteLine($"PASSED (ClientID: {profileAlice.ClientID})"); passed++; }
            else { Console.WriteLine("FAILED"); failed++; }

            // Test 3: Register Fitness Instructor
            Console.Write("[Test 3] User Registration (Instructor Role)... ");
            bool r2 = userController.Register("Coach Marcus", testEmailInst, "coachSecret!", "Instructor", out string err2);
            bool l2 = userController.Login(testEmailInst, "coachSecret!", out User? coachMarcus, out string errL2);
            if (r2 && l2 && coachMarcus != null && coachMarcus.IsInstructor) { Console.WriteLine($"PASSED (InstructorID: {coachMarcus.UserID})"); passed++; }
            else { Console.WriteLine($"FAILED: {err2}"); failed++; }

            // Test 4: Duplicate Email Prevention
            Console.Write("[Test 4] Duplicate Email Prevention... ");
            bool rDup = userController.Register("Alice Imposter", testEmailIndiv, "newPassword", "Individual", out string errDup);
            if (!rDup && errDup.Contains("already exists")) { Console.WriteLine("PASSED (Correctly rejected duplicate)"); passed++; }
            else { Console.WriteLine("FAILED (Allowed duplicate)"); failed++; }

            // Test 5: Instructor Client Management (Add Client)
            Console.Write("[Test 5] Instructor Adding Client (David Miller)... ");
            ClientProfile newClient = new ClientProfile(0, null, coachMarcus?.UserID, "David Miller", 30, "Male", 182.0m, 88.5m, DateTime.Now);
            bool cAdd = clientController.AddClient(newClient, out string errC);
            if (cAdd && newClient.ClientID > 0) { Console.WriteLine($"PASSED (ClientID: {newClient.ClientID})"); passed++; }
            else { Console.WriteLine($"FAILED: {errC}"); failed++; }

            // Test 6: Instructor Searching Client
            Console.Write("[Test 6] Instructor Searching Clients by Name... ");
            var searchResults = clientController.GetClientsByInstructor(coachMarcus!.UserID, "David");
            if (searchResults.Count > 0 && searchResults[0].FullName == "David Miller") { Console.WriteLine("PASSED"); passed++; }
            else { Console.WriteLine("FAILED"); failed++; }

            // Test 7: BMI Calculation & Classification (Adult)
            Console.Write("[Test 7] Adult BMI Calculation (182cm, 88.5kg)... ");
            decimal adultBmi = Assessment.CalculateBMI(88.5m, 182.0m); // 88.5 / (1.82^2) = 26.7
            string adultCat = Assessment.DetermineBMICategory(adultBmi, 30);
            if (adultBmi == 26.7m && adultCat == "Overweight") { Console.WriteLine($"PASSED (BMI: {adultBmi}, Category: {adultCat})"); passed++; }
            else { Console.WriteLine($"FAILED (Got {adultBmi}, {adultCat})"); failed++; }

            // Test 8: BMI Pediatric Rule (Child/Teen under 18)
            Console.Write("[Test 8] Pediatric Rule: Age 14 Child BMI (Adult rules must NOT apply)... ");
            decimal teenBmi = Assessment.CalculateBMI(55.0m, 160.0m);
            string teenCat = Assessment.DetermineBMICategory(teenBmi, 14);
            if (teenCat.StartsWith("Pediatric / Adolescent")) { Console.WriteLine($"PASSED ({teenCat})"); passed++; }
            else { Console.WriteLine($"FAILED: Standard adult category inappropriately applied ({teenCat})"); failed++; }

            // Test 9: Rule-based Wellness Feedback Generation
            Console.Write("[Test 9] Rule-Based Wellness Recommendations... ");
            string feedback = Assessment.GenerateWellnessFeedback(30, adultBmi, adultCat, "Sedentary", 6.0m, 1.5m);
            if (feedback.Contains("Sedentary") && feedback.Contains("sleep debt") && feedback.Contains("Below optimal hydration")) { Console.WriteLine("PASSED (All rules evaluated)"); passed++; }
            else { Console.WriteLine("FAILED"); failed++; }

            // Test 10: Saving Health Assessment & Syncing Profile Weight/Height
            Console.Write("[Test 10] Save Assessment and Synchronize Client Profile... ");
            Assessment assessment1 = assessmentController.CalculateDiagnostic(newClient.ClientID, 30, 182.0m, 88.5m, "Sedentary", 6.0m, 1.5m);
            bool aSaved = assessmentController.SaveAssessment(assessment1, out string errA);
            ClientProfile? refreshedClient = clientController.GetClientById(newClient.ClientID);
            if (aSaved && refreshedClient != null && refreshedClient.Weight == 88.5m) { Console.WriteLine($"PASSED (AssessmentID: {assessment1.AssessmentID})"); passed++; }
            else { Console.WriteLine($"FAILED: {errA}"); failed++; }

            // Test 11: Create Fitness Goal
            Console.Write("[Test 11] Establish Fitness Goal... ");
            FitnessGoal goal = new FitnessGoal(0, newClient.ClientID, "Reach target weight of 80 kg", 80.0m, 88.5m, "kg", DateTime.Today.AddMonths(2), "In Progress", DateTime.Now);
            bool gCreated = goalController.CreateGoal(goal, out string errG);
            if (gCreated && goal.GoalID > 0) { Console.WriteLine($"PASSED (GoalID: {goal.GoalID})"); passed++; }
            else { Console.WriteLine($"FAILED: {errG}"); failed++; }

            // Test 12: Goal Progress Update & Completion
            Console.Write("[Test 12] Update Goal Progress and Mark Completed... ");
            goalController.UpdateGoalProgress(goal.GoalID, 80.0m, "In Progress", out _);
            bool gComp = goalController.MarkGoalCompleted(goal.GoalID, out _);
            var clientGoals = goalController.GetGoalsByClient(newClient.ClientID, "Completed");
            if (gComp && clientGoals.Count > 0 && clientGoals[0].Status == "Completed") { Console.WriteLine("PASSED (Goal marked 100% completed)"); passed++; }
            else { Console.WriteLine("FAILED"); failed++; }

            // Test 13: Progress Tracking & Delta Comparison
            Console.Write("[Test 13] Progress Tracking & Historical Delta (Second Checkup)... ");
            // Log second assessment with improved metrics
            Assessment assessment2 = assessmentController.CalculateDiagnostic(newClient.ClientID, 30, 182.0m, 82.0m, "Moderately Active", 8.0m, 2.5m);
            assessmentController.SaveAssessment(assessment2, out _);
            List<Assessment> history = assessmentController.GetAssessmentsByClient(newClient.ClientID);
            decimal weightDelta = history[0].Weight - history[history.Count - 1].Weight;
            if (history.Count == 2 && weightDelta == -6.5m) { Console.WriteLine($"PASSED (History count: 2, Weight Change: {weightDelta} kg)"); passed++; }
            else { Console.WriteLine("FAILED"); failed++; }

            Console.WriteLine("==================================================================");
            Console.WriteLine($"TEST EXECUTION FINISHED: {passed} PASSED, {failed} FAILED.");
            Console.WriteLine("==================================================================");
        }
    }
}
