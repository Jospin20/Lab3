using System;
using System.Drawing;
using System.Windows.Forms;
using VitalCheck.Controllers;
using VitalCheck.Models;

namespace VitalCheck.Views
{
    /// <summary>
    /// DashboardControl provides the primary overview screen.
    /// Displays key statistics, active fitness goals, latest BMI & health indicators,
    /// and quick action buttons.
    /// </summary>
    public class DashboardControl : UserControl
    {
        private readonly User _currentUser;
        private readonly ClientProfile? _activeClient;
        private readonly AssessmentController _assessmentController;
        private readonly GoalController _goalController;
        private readonly ClientController _clientController;

        // UI Controls
        private Panel pnlWelcome;
        private Label lblGreeting;
        private Label lblSubtitle;

        private FlowLayoutPanel pnlMetrics;
        private Panel cardAssessments;
        private Panel cardGoals;
        private Panel cardBmi;
        private Panel cardWeightOrClients;

        private Panel pnlRecentDiagnostic;
        private Label lblRecentTitle;
        private Label lblRecentDate;
        private Label lblRecentBmi;
        private Label lblRecentHabits;
        private TextBox txtRecentFeedback;

        // Navigation callback delegate to switch views from dashboard buttons
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public Action<string>? OnNavigateRequested { get; set; }

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
            this.BackColor = Color.FromArgb(248, 250, 252); // Light background
            this.Font = new Font("Segoe UI", 10F);
            this.AutoScroll = true;
            this.Padding = new Padding(25);

            // 1. Welcome Header Banner
            pnlWelcome = new Panel
            {
                Dock = DockStyle.Top,
                Height = 85,
                BackColor = Color.White,
                Padding = new Padding(20, 15, 20, 15)
            };

            lblGreeting = new Label
            {
                Text = $"Welcome back, {_currentUser.FullName}!",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(15, 12),
                AutoSize = true
            };

            string sub = _currentUser.IsInstructor
                ? "Fitness Instructor Portal • Monitor client wellness, assessments, and fitness targets."
                : "Personal Health Dashboard • Track your body metrics, wellness habits, and fitness milestones.";

            if (_currentUser.IsInstructor && _activeClient != null)
            {
                sub += $" | Currently Viewing Client: {_activeClient.FullName}";
            }

            lblSubtitle = new Label
            {
                Text = sub,
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(15, 45),
                AutoSize = true
            };

            pnlWelcome.Controls.Add(lblGreeting);
            pnlWelcome.Controls.Add(lblSubtitle);
            this.Controls.Add(pnlWelcome);

            // 2. Metrics Cards Panel
            pnlMetrics = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 135,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Margin = new Padding(0, 15, 0, 15),
                Padding = new Padding(0, 15, 0, 0)
            };
            this.Controls.Add(pnlMetrics);

            // 3. Recent Diagnostic Details Panel
            pnlRecentDiagnostic = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(20),
                Margin = new Padding(0, 15, 0, 0)
            };

            lblRecentTitle = new Label
            {
                Text = "Latest Diagnostic & Wellness Evaluation",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(20, 15),
                AutoSize = true
            };

            lblRecentDate = new Label
            {
                Text = "No previous assessment recorded.",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Italic),
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(20, 42),
                AutoSize = true
            };

            lblRecentBmi = new Label
            {
                Text = "BMI: --",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(13, 148, 136),
                Location = new Point(20, 75),
                AutoSize = true
            };

            lblRecentHabits = new Label
            {
                Text = "Sleep: -- hrs | Water: -- L | Activity: --",
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(51, 65, 85),
                Location = new Point(20, 102),
                AutoSize = true
            };

            txtRecentFeedback = new TextBox
            {
                Location = new Point(20, 135),
                Size = new Size(720, 180),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.FromArgb(248, 250, 252),
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.FromArgb(30, 41, 59),
                BorderStyle = BorderStyle.FixedSingle
            };

            Button btnNewAssessment = new Button
            {
                Text = "Conduct Health Assessment",
                Location = new Point(20, 330),
                Size = new Size(220, 38),
                BackColor = Color.FromArgb(13, 148, 136),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnNewAssessment.FlatAppearance.BorderSize = 0;
            btnNewAssessment.Click += (s, e) => OnNavigateRequested?.Invoke("Assessment");

            Button btnViewGoals = new Button
            {
                Text = "Manage Fitness Goals",
                Location = new Point(255, 330),
                Size = new Size(200, 38),
                BackColor = Color.FromArgb(2, 132, 199),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnViewGoals.FlatAppearance.BorderSize = 0;
            btnViewGoals.Click += (s, e) => OnNavigateRequested?.Invoke("Goals");

            pnlRecentDiagnostic.Controls.Add(lblRecentTitle);
            pnlRecentDiagnostic.Controls.Add(lblRecentDate);
            pnlRecentDiagnostic.Controls.Add(lblRecentBmi);
            pnlRecentDiagnostic.Controls.Add(lblRecentHabits);
            pnlRecentDiagnostic.Controls.Add(txtRecentFeedback);
            pnlRecentDiagnostic.Controls.Add(btnNewAssessment);
            pnlRecentDiagnostic.Controls.Add(btnViewGoals);

            this.Controls.Add(pnlRecentDiagnostic);
            pnlRecentDiagnostic.BringToFront();
        }

        private Panel CreateMetricCard(string title, string value, string subtitle, Color accentColor)
        {
            Panel card = new Panel
            {
                Size = new Size(185, 110),
                BackColor = Color.White,
                Margin = new Padding(0, 0, 15, 0),
                Padding = new Padding(12)
            };

            Panel bar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 5,
                BackColor = accentColor
            };

            Label lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(14, 12),
                AutoSize = true
            };

            Label lblVal = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(14, 32),
                AutoSize = true
            };

            Label lblSub = new Label
            {
                Text = subtitle,
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = accentColor,
                Location = new Point(14, 72),
                AutoSize = true
            };

            card.Controls.Add(bar);
            card.Controls.Add(lblTitle);
            card.Controls.Add(lblVal);
            card.Controls.Add(lblSub);

            return card;
        }

        private void LoadDashboardData()
        {
            int clientId = _activeClient?.ClientID ?? 0;
            int? instructorId = _currentUser.IsInstructor ? _currentUser.UserID : null;

            int totalAssessments = _assessmentController.GetTotalAssessmentsCount(
                clientId > 0 ? clientId : null,
                clientId == 0 ? instructorId : null
            );

            int activeGoals = _goalController.GetActiveGoalsCount(
                clientId > 0 ? clientId : null,
                clientId == 0 ? instructorId : null
            );

            Assessment? latest = clientId > 0 ? _assessmentController.GetLatestAssessment(clientId) : null;

            pnlMetrics.Controls.Clear();

            // Card 1: Total Assessments
            cardAssessments = CreateMetricCard("Total Assessments", totalAssessments.ToString(), "Checkups logged", Color.FromArgb(13, 148, 136));
            pnlMetrics.Controls.Add(cardAssessments);

            // Card 2: Active Goals
            cardGoals = CreateMetricCard("Active Goals", activeGoals.ToString(), "In Progress", Color.FromArgb(2, 132, 199));
            pnlMetrics.Controls.Add(cardGoals);

            // Card 3: BMI
            string bmiText = latest != null ? $"{latest.BMI:F1}" : "--";
            string bmiSub = latest != null ? latest.BMICategory : "No records";
            Color bmiColor = GetBmiColor(latest?.BMICategory);
            cardBmi = CreateMetricCard("Current BMI", bmiText, bmiSub, bmiColor);
            pnlMetrics.Controls.Add(cardBmi);

            // Card 4: Weight or Instructor Clients Count
            if (_currentUser.IsInstructor)
            {
                int clientCount = _clientController.GetClientsByInstructor(_currentUser.UserID).Count;
                cardWeightOrClients = CreateMetricCard("My Clients", clientCount.ToString(), "Registered", Color.FromArgb(124, 58, 237));
            }
            else
            {
                string weightText = latest != null ? $"{latest.Weight:F1} kg" : (_activeClient != null ? $"{_activeClient.Weight:F1} kg" : "--");
                cardWeightOrClients = CreateMetricCard("Current Weight", weightText, "Body mass", Color.FromArgb(217, 119, 6));
            }
            pnlMetrics.Controls.Add(cardWeightOrClients);

            // Populate Latest Assessment Card
            if (latest != null)
            {
                lblRecentDate.Text = $"Recorded on {latest.AssessmentDate:dd MMM yyyy, hh:mm tt}";
                lblRecentBmi.Text = $"BMI: {latest.BMI:F1}  •  Classification: {latest.BMICategory}";
                lblRecentBmi.ForeColor = bmiColor;
                lblRecentHabits.Text = $"Sleep: {latest.SleepHours:F1} hrs/day  •  Water Intake: {latest.WaterIntake:F1} L/day  •  Activity: {latest.ActivityLevel}";
                txtRecentFeedback.Text = latest.Feedback;
            }
            else
            {
                lblRecentDate.Text = "No previous assessment recorded for this profile.";
                lblRecentBmi.Text = "BMI: Not yet calculated";
                lblRecentBmi.ForeColor = Color.FromArgb(100, 116, 139);
                lblRecentHabits.Text = "Complete your first health assessment to unlock diagnostic feedback.";
                txtRecentFeedback.Text = "Welcome to VitalCheck!\r\n\r\nTo generate an initial health and wellness evaluation, click 'Conduct Health Assessment' below or select 'Health Assessment' from the sidebar menu.";
            }
        }

        private static Color GetBmiColor(string? category)
        {
            if (category == null) return Color.FromArgb(100, 116, 139);
            switch (category)
            {
                case "Normal weight":
                    return Color.FromArgb(16, 185, 129); // Green
                case "Underweight":
                    return Color.FromArgb(234, 179, 8); // Amber
                case "Overweight":
                    return Color.FromArgb(249, 115, 22); // Orange
                case "Obese":
                    return Color.FromArgb(239, 68, 68); // Red
                default:
                    return Color.FromArgb(59, 130, 246); // Blue
            }
        }
    }
}
