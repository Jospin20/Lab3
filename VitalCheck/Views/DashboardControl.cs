using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using VitalCheck.Controllers;
using VitalCheck.Models;

namespace VitalCheck.Views
{
    /// <summary>
    /// DashboardControl provides a simple, clean overview of the user's health status.
    /// Uses standard Windows Forms GroupBoxes and Labels for beginner-level readability.
    /// </summary>
    public class DashboardControl : UserControl
    {
        private readonly User _currentUser;
        private readonly ClientProfile? _activeClient;
        private readonly AssessmentController _assessmentController;
        private readonly GoalController _goalController;
        private readonly ClientController _clientController;

        // Welcome Box
        private GroupBox grpWelcome;
        private Label lblGreeting;
        private Label lblSubjectInfo;

        // Summary Boxes
        private GroupBox grpBmi;
        private Label lblBmiValue;
        private Label lblBmiCategory;
        private Label lblBmiRange;

        private GroupBox grpStats;
        private Label lblHeight;
        private Label lblWeight;
        private Label lblHabits;

        private GroupBox grpGoals;
        private Label lblGoalCount;
        private Label lblGoalTip;

        // Feedback & Actions
        private GroupBox grpFeedback;
        private TextBox txtFeedback;

        private GroupBox grpActions;
        private Button btnStartAssessment;
        private Button btnManageGoals;
        private Button btnViewProgress;
        private Button? btnClientDirectory;

        // Callback to navigate in main form
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public Action<string>? OnNavigate { get; set; }

        public DashboardControl(User user, ClientProfile? activeClient)
        {
            _currentUser = user;
            _activeClient = activeClient;
            _assessmentController = new AssessmentController();
            _goalController = new GoalController();
            _clientController = new ClientController();

            InitializeComponent();
            LoadDashboardData();
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
            this.AutoScroll = true;

            int leftMargin = 20;
            int curY = 15;

            // 1. Welcome GroupBox
            grpWelcome = new GroupBox
            {
                Text = " Welcome ",
                Location = new Point(leftMargin, curY),
                Size = new Size(840, 75),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59)
            };

            lblGreeting = new Label
            {
                Text = $"Welcome, {_currentUser.FullName}!",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(15, 22),
                AutoSize = true
            };

            string subtitle = _currentUser.IsInstructor
                ? (_activeClient != null ? $"Managing Client: {_activeClient.FullName} (Age: {_activeClient.Age}, {_activeClient.Sex})" : "Fitness Instructor Portal - Select or register a client to begin.")
                : "Personal Health & Wellness Portal - Monitor your indicators and progress.";

            lblSubjectInfo = new Label
            {
                Text = subtitle,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(16, 48),
                AutoSize = true
            };

            grpWelcome.Controls.Add(lblGreeting);
            grpWelcome.Controls.Add(lblSubjectInfo);
            this.Controls.Add(grpWelcome);

            // 2. Row of 3 Summary Boxes (BMI, Stats, Goals)
            curY += 85;
            int boxWidth = 270;
            int boxGap = 15;

            // Box 1: BMI Summary
            grpBmi = new GroupBox
            {
                Text = " Latest BMI Status ",
                Location = new Point(leftMargin, curY),
                Size = new Size(boxWidth, 120),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59)
            };

            lblBmiValue = new Label
            {
                Text = "BMI: --",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.FromArgb(13, 148, 136), // Teal
                Location = new Point(15, 25),
                AutoSize = true
            };

            lblBmiCategory = new Label
            {
                Text = "No assessment on record",
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.FromArgb(71, 85, 105),
                Location = new Point(16, 62),
                AutoSize = true
            };

            lblBmiRange = new Label
            {
                Text = "Normal adult range: 18.5 – 24.9",
                Font = new Font("Segoe UI", 8F, FontStyle.Italic),
                ForeColor = Color.FromArgb(148, 163, 184),
                Location = new Point(16, 88),
                AutoSize = true
            };

            grpBmi.Controls.Add(lblBmiValue);
            grpBmi.Controls.Add(lblBmiCategory);
            grpBmi.Controls.Add(lblBmiRange);
            this.Controls.Add(grpBmi);

            // Box 2: Measurements & Habits
            grpStats = new GroupBox
            {
                Text = " Measurements & Lifestyle ",
                Location = new Point(leftMargin + boxWidth + boxGap, curY),
                Size = new Size(boxWidth, 120),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59)
            };

            lblHeight = new Label
            {
                Text = "Height: -- cm",
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.FromArgb(51, 65, 85),
                Location = new Point(15, 30),
                AutoSize = true
            };

            lblWeight = new Label
            {
                Text = "Weight: -- kg",
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.FromArgb(51, 65, 85),
                Location = new Point(15, 58),
                AutoSize = true
            };

            lblHabits = new Label
            {
                Text = "Sleep: -- hrs  |  Water: -- L",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(15, 88),
                AutoSize = true
            };

            grpStats.Controls.Add(lblHeight);
            grpStats.Controls.Add(lblWeight);
            grpStats.Controls.Add(lblHabits);
            this.Controls.Add(grpStats);

            // Box 3: Fitness Goals
            grpGoals = new GroupBox
            {
                Text = " Fitness Goals ",
                Location = new Point(leftMargin + (boxWidth + boxGap) * 2, curY),
                Size = new Size(boxWidth, 120),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59)
            };

            lblGoalCount = new Label
            {
                Text = "0 Active Goals",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(15, 30),
                AutoSize = true
            };

            lblGoalTip = new Label
            {
                Text = "Set measurable targets in the Goals tab.",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(16, 65),
                Size = new Size(240, 40)
            };

            grpGoals.Controls.Add(lblGoalCount);
            grpGoals.Controls.Add(lblGoalTip);
            this.Controls.Add(grpGoals);

            // 3. Lower Section: Feedback on Left, Quick Actions on Right
            curY += 135;

            // Latest Feedback Box (Left)
            grpFeedback = new GroupBox
            {
                Text = " Latest Diagnostic & Wellness Recommendations ",
                Location = new Point(leftMargin, curY),
                Size = new Size(550, 360),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59)
            };

            txtFeedback = new TextBox
            {
                Location = new Point(15, 28),
                Size = new Size(520, 315),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                ForeColor = Color.FromArgb(30, 41, 59)
            };
            grpFeedback.Controls.Add(txtFeedback);
            this.Controls.Add(grpFeedback);

            // Quick Actions Box (Right)
            grpActions = new GroupBox
            {
                Text = " Quick Actions ",
                Location = new Point(leftMargin + 565, curY),
                Size = new Size(275, 360),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59)
            };

            int actionY = 35;
            int actionH = 40;
            int actionGap = 12;

            btnStartAssessment = CreateActionButton("Conduct Assessment", actionY);
            btnStartAssessment.Click += (s, e) => OnNavigate?.Invoke("Assessment");
            grpActions.Controls.Add(btnStartAssessment);

            actionY += actionH + actionGap;
            btnManageGoals = CreateActionButton("Manage Goals", actionY);
            btnManageGoals.Click += (s, e) => OnNavigate?.Invoke("Goals");
            grpActions.Controls.Add(btnManageGoals);

            actionY += actionH + actionGap;
            btnViewProgress = CreateActionButton("View History & Charts", actionY);
            btnViewProgress.Click += (s, e) => OnNavigate?.Invoke("Progress");
            grpActions.Controls.Add(btnViewProgress);

            if (_currentUser.IsInstructor)
            {
                actionY += actionH + actionGap;
                btnClientDirectory = CreateActionButton("Manage Clients", actionY);
                btnClientDirectory.Click += (s, e) => OnNavigate?.Invoke("Clients");
                grpActions.Controls.Add(btnClientDirectory);
            }

            this.Controls.Add(grpActions);
        }

        private Button CreateActionButton(string text, int top)
        {
            Button btn = new Button
            {
                Text = text,
                Location = new Point(20, top),
                Size = new Size(235, 40),
                BackColor = Color.FromArgb(13, 148, 136),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        /// <summary>
        /// Loads the active subject's latest health assessment and goals into the controls.
        /// </summary>
        public void LoadDashboardData()
        {
            if (_activeClient == null)
            {
                lblBmiValue.Text = "BMI: --";
                lblBmiCategory.Text = "No client selected";
                txtFeedback.Text = "Please select or register a client to view health records.";
                return;
            }

            // 1. Load latest assessment
            Assessment? latest = _assessmentController.GetLatestAssessment(_activeClient.ClientID);

            if (latest != null)
            {
                lblBmiValue.Text = $"BMI: {latest.BMI:F1}";
                lblBmiCategory.Text = latest.BMICategory;

                // Color code category
                if (latest.BMICategory == "Normal weight")
                    lblBmiCategory.ForeColor = Color.FromArgb(16, 185, 129); // Green
                else if (latest.BMICategory.Contains("Overweight") || latest.BMICategory.Contains("Underweight"))
                    lblBmiCategory.ForeColor = Color.FromArgb(217, 119, 6); // Amber
                else
                    lblBmiCategory.ForeColor = Color.FromArgb(220, 38, 38); // Red

                lblHeight.Text = $"Height: {latest.Height:F0} cm ({latest.Height / 100m:F2} m)";
                lblWeight.Text = $"Weight: {latest.Weight:F1} kg";
                lblHabits.Text = $"Sleep: {latest.SleepHours:F1} hrs  |  Water: {latest.WaterIntake:F1} L";
                txtFeedback.Text = latest.Feedback;
            }
            else
            {
                lblBmiValue.Text = "BMI: --";
                lblBmiCategory.Text = "No previous assessment";
                lblHeight.Text = $"Baseline Height: {_activeClient.Height:F0} cm";
                lblWeight.Text = $"Baseline Weight: {_activeClient.Weight:F1} kg";
                lblHabits.Text = "No lifestyle habits reported yet.";
                txtFeedback.Text = "Welcome to VitalCheck!\r\n\r\nTo evaluate health indicators and receive wellness feedback, click 'Conduct Assessment' on the right.";
            }

            // 2. Load fitness goals summary
            List<FitnessGoal> goals = _goalController.GetGoalsByClient(_activeClient.ClientID);
            int activeCount = 0;
            int completedCount = 0;

            foreach (var g in goals)
            {
                if (g.Status == "Completed") completedCount++;
                else if (g.Status == "In Progress") activeCount++;
            }

            lblGoalCount.Text = $"{activeCount} Active Goal{(activeCount == 1 ? "" : "s")}";
            lblGoalTip.Text = completedCount > 0
                ? $"{completedCount} goal(s) completed so far! Great job."
                : "Establish targets to track your weight and fitness development.";
        }
    }
}
