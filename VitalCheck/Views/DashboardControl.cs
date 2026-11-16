using System;
using System.Drawing;
using System.Windows.Forms;
using VitalCheck.Controllers;
using VitalCheck.Models;

namespace VitalCheck.Views
{
    /// <summary>
    /// DashboardControl provides an executive overview of the active profile's health status.
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

        // Container Panels
        private Panel pnlBanner;
        private FlowLayoutPanel pnlMetrics;
        private TableLayoutPanel pnlColumns;
        private Panel pnlLeftCard;
        private Panel pnlRightCard;

        // Latest Diagnostic Controls
        private Label lblLatestTitle;
        private Label lblLatestDate;
        private Label lblLatestBmiBadge;
        private Label lblLatestHabits;
        private TextBox txtLatestFeedback;

        // Navigation callback delegate
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
            this.BackColor = Color.FromArgb(248, 250, 252);
            this.Font = new Font("Segoe UI", 10F);
            this.AutoScroll = true;
            this.Padding = new Padding(25, 20, 25, 25);

            // 1. Welcome Banner Card
            pnlBanner = new Panel
            {
                Dock = DockStyle.Top,
                Height = 88,
                BackColor = Color.White,
                Padding = new Padding(24, 16, 24, 14),
                Margin = new Padding(0, 0, 0, 16)
            };

            FlowLayoutPanel pnlBannerFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = Color.Transparent
            };

            Label lblGreeting = new Label
            {
                Text = $"Welcome, {_currentUser.FullName}!",
                Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 4)
            };

            string subtitle = _currentUser.IsInstructor
                ? (_activeClient != null ? $"Managing Client: {_activeClient.FullName} (Age: {_activeClient.Age}, {_activeClient.Sex})" : "Fitness Instructor Portal — Select or register a client to begin.")
                : "Personal Health & Wellness Portal — Monitor indicators, set targets, and track development.";

            Label lblSub = new Label
            {
                Text = subtitle,
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.FromArgb(100, 116, 139),
                AutoSize = true,
                Margin = new Padding(0)
            };

            pnlBannerFlow.Controls.Add(lblGreeting);
            pnlBannerFlow.Controls.Add(lblSub);
            pnlBanner.Controls.Add(pnlBannerFlow);
            this.Controls.Add(pnlBanner);

            // 2. Metrics Cards Row (FlowLayoutPanel with auto-wrapping for responsiveness)
            pnlMetrics = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 120,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoScroll = false,
                Padding = new Padding(0, 10, 0, 10)
            };
            this.Controls.Add(pnlMetrics);

            // 3. Middle 2-Column Split (TableLayoutPanel for responsive 60% / 40% split)
            pnlColumns = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(0, 12, 0, 0)
            };
            pnlColumns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62F));
            pnlColumns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38F));

            // Left Column: Latest Diagnostic Report
            pnlLeftCard = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(22),
                Margin = new Padding(0, 0, 12, 0)
            };
            BuildLeftDiagnosticCard();

            // Right Column: Quick Actions & Milestones
            pnlRightCard = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(22),
                Margin = new Padding(12, 0, 0, 0)
            };
            BuildRightActionsCard();

            pnlColumns.Controls.Add(pnlLeftCard, 0, 0);
            pnlColumns.Controls.Add(pnlRightCard, 1, 0);

            this.Controls.Add(pnlColumns);
            pnlColumns.BringToFront();
        }

        private void BuildLeftDiagnosticCard()
        {
            lblLatestTitle = new Label
            {
                Text = "Latest Diagnostic & Wellness Evaluation",
                Font = new Font("Segoe UI", 12.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Dock = DockStyle.Top,
                Height = 28
            };

            lblLatestDate = new Label
            {
                Text = "No previous assessment recorded.",
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                ForeColor = Color.FromArgb(100, 116, 139),
                Dock = DockStyle.Top,
                Height = 22
            };

            Panel pnlBmiBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 42,
                Padding = new Padding(0, 6, 0, 6)
            };

            lblLatestBmiBadge = new Label
            {
                Text = "BMI: --",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(13, 148, 136),
                Dock = DockStyle.Left,
                AutoSize = true
            };
            pnlBmiBar.Controls.Add(lblLatestBmiBadge);

            lblLatestHabits = new Label
            {
                Text = "Sleep: -- hrs  •  Water: -- L  •  Activity: --",
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.FromArgb(71, 85, 105),
                Dock = DockStyle.Top,
                Height = 25
            };

            txtLatestFeedback = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.FromArgb(248, 250, 252),
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.FromArgb(30, 41, 59),
                BorderStyle = BorderStyle.FixedSingle
            };

            pnlLeftCard.Controls.Add(txtLatestFeedback);
            pnlLeftCard.Controls.Add(lblLatestHabits);
            pnlLeftCard.Controls.Add(pnlBmiBar);
            pnlLeftCard.Controls.Add(lblLatestDate);
            pnlLeftCard.Controls.Add(lblLatestTitle);
            txtLatestFeedback.BringToFront();
        }

        private void BuildRightActionsCard()
        {
            Label lblActionHeading = new Label
            {
                Text = "Quick Actions & Navigation",
                Font = new Font("Segoe UI", 12.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Dock = DockStyle.Top,
                Height = 28
            };

            Label lblActionSub = new Label
            {
                Text = "Perform regular evaluations and monitor targets:",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(100, 116, 139),
                Dock = DockStyle.Top,
                Height = 24
            };

            Button btnNewCheckup = CreateActionButton("📋  Conduct Health Assessment", Color.FromArgb(13, 148, 136));
            btnNewCheckup.Click += (s, e) => OnNavigateRequested?.Invoke("Assessment");

            Button btnGoGoals = CreateActionButton("🎯  Manage Fitness Goals", Color.FromArgb(2, 132, 199));
            btnGoGoals.Click += (s, e) => OnNavigateRequested?.Invoke("Goals");

            Button btnGoProgress = CreateActionButton("📈  View Progress Analytics", Color.FromArgb(124, 58, 237));
            btnGoProgress.Click += (s, e) => OnNavigateRequested?.Invoke("Progress");

            Panel pnlSpacer = new Panel { Dock = DockStyle.Top, Height = 10 };

            Panel pnlTipCard = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(240, 253, 250), // Soft Mint
                Padding = new Padding(14)
            };

            Label lblTipTitle = new Label
            {
                Text = "💡 Pro Tip: Clinical Assessment",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(13, 148, 136),
                Dock = DockStyle.Top,
                Height = 22
            };

            Label lblTipBody = new Label
            {
                Text = "For the most accurate body mass and diagnostic feedback, conduct your assessment in the morning before meals. Consistent weekly checkups yield the best progress comparison.",
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(51, 65, 85),
                Dock = DockStyle.Fill
            };

            pnlTipCard.Controls.Add(lblTipBody);
            pnlTipCard.Controls.Add(lblTipTitle);

            pnlRightCard.Controls.Add(pnlTipCard);
            pnlRightCard.Controls.Add(pnlSpacer);
            pnlRightCard.Controls.Add(btnGoProgress);
            pnlRightCard.Controls.Add(btnGoGoals);
            pnlRightCard.Controls.Add(btnNewCheckup);
            pnlRightCard.Controls.Add(lblActionSub);
            pnlRightCard.Controls.Add(lblActionHeading);
        }

        private Button CreateActionButton(string title, Color bg)
        {
            Button btn = new Button
            {
                Text = title,
                Dock = DockStyle.Top,
                Height = 44,
                BackColor = bg,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 0, 10)
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private Panel CreateMetricCard(string title, string value, string subtitle, Color accentColor)
        {
            Panel card = new Panel
            {
                Size = new Size(230, 95),
                BackColor = Color.White,
                Margin = new Padding(0, 0, 16, 0),
                Padding = new Padding(14, 10, 14, 10)
            };

            Panel bar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 5,
                BackColor = accentColor
            };

            Label lblT = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(14, 8),
                AutoSize = true
            };

            Label lblV = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(12, 26),
                AutoSize = true
            };

            Label lblS = new Label
            {
                Text = subtitle,
                Font = new Font("Segoe UI", 8F),
                ForeColor = accentColor,
                Location = new Point(14, 64),
                AutoSize = true
            };

            card.Controls.Add(bar);
            card.Controls.Add(lblT);
            card.Controls.Add(lblV);
            card.Controls.Add(lblS);

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
            pnlMetrics.Controls.Add(CreateMetricCard("TOTAL CHECKUPS", totalAssessments.ToString(), "Historical assessments logged", Color.FromArgb(13, 148, 136)));

            // Card 2: Active Goals
            pnlMetrics.Controls.Add(CreateMetricCard("ACTIVE TARGETS", activeGoals.ToString(), "Goals currently in progress", Color.FromArgb(2, 132, 199)));

            // Card 3: Current BMI
            string bmiText = latest != null ? $"{latest.BMI:F1}" : "--";
            string bmiSub = latest != null ? latest.BMICategory : "No records yet";
            Color bmiColor = GetBmiColor(latest?.BMICategory);
            pnlMetrics.Controls.Add(CreateMetricCard("CURRENT BMI", bmiText, bmiSub, bmiColor));

            // Card 4: Weight or Instructor Clients
            if (_currentUser.IsInstructor)
            {
                int clientCount = _clientController.GetClientsByInstructor(_currentUser.UserID).Count;
                pnlMetrics.Controls.Add(CreateMetricCard("CLIENT DIRECTORY", clientCount.ToString(), "Registered active clients", Color.FromArgb(124, 58, 237)));
            }
            else
            {
                string weightText = latest != null ? $"{latest.Weight:F1} kg" : (_activeClient != null ? $"{_activeClient.Weight:F1} kg" : "--");
                pnlMetrics.Controls.Add(CreateMetricCard("CURRENT WEIGHT", weightText, "Baseline body mass", Color.FromArgb(217, 119, 6)));
            }

            // Populate Latest Assessment Card
            if (latest != null)
            {
                lblLatestDate.Text = $"Last recorded on {latest.AssessmentDate:dd MMM yyyy, hh:mm tt}";
                lblLatestBmiBadge.Text = $"BMI: {latest.BMI:F1}  •  Classification: {latest.BMICategory}";
                lblLatestBmiBadge.ForeColor = bmiColor;
                lblLatestHabits.Text = $"Sleep: {latest.SleepHours:F1} hrs/day  •  Water: {latest.WaterIntake:F1} L/day  •  Activity: {latest.ActivityLevel}";
                txtLatestFeedback.Text = latest.Feedback;
            }
            else
            {
                lblLatestDate.Text = "No previous assessment recorded for this profile.";
                lblLatestBmiBadge.Text = "BMI: Awaiting First Assessment";
                lblLatestBmiBadge.ForeColor = Color.FromArgb(100, 116, 139);
                lblLatestHabits.Text = "Complete your initial checkup to calculate BMI, BMR, and wellness feedback.";
                txtLatestFeedback.Text = "Welcome to VitalCheck!\r\n\r\nTo generate an in-depth clinical & wellness evaluation, click 'Conduct Health Assessment' on the right or choose 'Health Assessment' from the sidebar.";
            }
        }

        private static Color GetBmiColor(string? category)
        {
            if (category == null) return Color.FromArgb(100, 116, 139);
            return category switch
            {
                "Normal weight" => Color.FromArgb(16, 185, 129), // Green
                "Underweight" => Color.FromArgb(234, 179, 8),    // Amber
                "Overweight" => Color.FromArgb(249, 115, 22),   // Orange
                "Obese" => Color.FromArgb(239, 68, 68),          // Red
                _ => Color.FromArgb(59, 130, 246)               // Blue
            };
        }
    }
}
