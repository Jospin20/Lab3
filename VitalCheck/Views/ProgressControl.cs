using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using VitalCheck.Controllers;
using VitalCheck.Models;

namespace VitalCheck.Views
{
    /// <summary>
    /// ProgressControl provides historical tracking, measurement comparison (initial vs latest),
    /// weight/BMI delta tracking, and past assessment log inspection.
    /// </summary>
    public class ProgressControl : UserControl
    {
        private readonly User _currentUser;
        private readonly ClientProfile? _activeClient;
        private readonly AssessmentController _assessmentController;

        // UI Controls
        private Panel pnlHeader;
        private Label lblTitle;
        private Label lblSubtitle;

        // Metric Comparison Cards
        private FlowLayoutPanel pnlComparison;
        private Panel cardWeightChange;
        private Panel cardBmiChange;
        private Panel cardTotalChecks;
        private Panel cardAverages;

        // History Grid & Details
        private Panel pnlHistoryContainer;
        private DataGridView dgvAssessments;
        private TextBox txtSelectedFeedback;
        private Label lblFeedbackHeader;

        private List<Assessment> _assessments = new List<Assessment>();

        public ProgressControl(User user, ClientProfile? activeClient)
        {
            _currentUser = user;
            _activeClient = activeClient;
            _assessmentController = new AssessmentController();

            InitializeComponent();
            LoadProgressData();
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(248, 250, 252);
            this.Font = new Font("Segoe UI", 10F);
            this.AutoScroll = true;
            this.Padding = new Padding(25);

            // 1. Header
            pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.White,
                Padding = new Padding(20, 12, 20, 10)
            };

            lblTitle = new Label
            {
                Text = "Progress Tracking & Assessment History",
                Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(15, 10),
                AutoSize = true
            };

            string sub = _activeClient != null
                ? $"Monitoring Progress for: {_activeClient.FullName}"
                : "No active client selected.";

            lblSubtitle = new Label
            {
                Text = sub,
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.FromArgb(13, 148, 136),
                Location = new Point(15, 40),
                AutoSize = true
            };

            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblSubtitle);
            this.Controls.Add(pnlHeader);

            // 2. Comparison Cards
            pnlComparison = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 125,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Margin = new Padding(0, 15, 0, 15),
                Padding = new Padding(0, 15, 0, 0)
            };
            this.Controls.Add(pnlComparison);

            // 3. History Grid and Selected Feedback container
            pnlHistoryContainer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(20),
                Margin = new Padding(0, 15, 0, 0)
            };

            Label lblGridHeading = new Label
            {
                Text = "Assessment History Log",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(15, 12),
                AutoSize = true
            };

            dgvAssessments = new DataGridView
            {
                Location = new Point(15, 45),
                Size = new Size(580, 420),
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            dgvAssessments.EnableHeadersVisualStyles = false;
            dgvAssessments.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(15, 23, 42);
            dgvAssessments.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvAssessments.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            dgvAssessments.ColumnHeadersHeight = 32;
            dgvAssessments.RowTemplate.Height = 30;
            dgvAssessments.SelectionChanged += DgvAssessments_SelectionChanged;

            lblFeedbackHeader = new Label
            {
                Text = "Diagnostic Feedback for Selected Checkup:",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(615, 15),
                AutoSize = true
            };

            txtSelectedFeedback = new TextBox
            {
                Location = new Point(615, 45),
                Size = new Size(330, 420),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.FromArgb(248, 250, 252),
                Font = new Font("Consolas", 9F),
                ForeColor = Color.FromArgb(30, 41, 59),
                BorderStyle = BorderStyle.FixedSingle
            };

            pnlHistoryContainer.Controls.Add(lblGridHeading);
            pnlHistoryContainer.Controls.Add(dgvAssessments);
            pnlHistoryContainer.Controls.Add(lblFeedbackHeader);
            pnlHistoryContainer.Controls.Add(txtSelectedFeedback);

            this.Controls.Add(pnlHistoryContainer);
            pnlHistoryContainer.BringToFront();
        }

        private Panel CreateMetricCard(string title, string value, string subtitle, Color accentColor)
        {
            Panel card = new Panel
            {
                Size = new Size(225, 100),
                BackColor = Color.White,
                Margin = new Padding(0, 0, 15, 0),
                Padding = new Padding(12)
            };

            Panel bar = new Panel { Dock = DockStyle.Left, Width = 5, BackColor = accentColor };
            Label lblT = new Label { Text = title, Font = new Font("Segoe UI", 9F), ForeColor = Color.FromArgb(100, 116, 139), Location = new Point(14, 10), AutoSize = true };
            Label lblV = new Label { Text = value, Font = new Font("Segoe UI", 15F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(14, 28), AutoSize = true };
            Label lblS = new Label { Text = subtitle, Font = new Font("Segoe UI", 8.5F), ForeColor = accentColor, Location = new Point(14, 65), AutoSize = true };

            card.Controls.Add(bar);
            card.Controls.Add(lblT);
            card.Controls.Add(lblV);
            card.Controls.Add(lblS);

            return card;
        }

        private void LoadProgressData()
        {
            if (_activeClient == null || _activeClient.ClientID <= 0)
            {
                pnlComparison.Controls.Clear();
                dgvAssessments.DataSource = null;
                txtSelectedFeedback.Text = "Please select an active client to view progress records.";
                return;
            }

            _assessments = _assessmentController.GetAssessmentsByClient(_activeClient.ClientID);

            pnlComparison.Controls.Clear();

            if (_assessments.Count == 0)
            {
                cardTotalChecks = CreateMetricCard("Assessments Logged", "0", "No checkups yet", Color.FromArgb(100, 116, 139));
                pnlComparison.Controls.Add(cardTotalChecks);
                txtSelectedFeedback.Text = "No previous assessments recorded for this client.";
                dgvAssessments.DataSource = null;
                return;
            }

            // Calculations
            Assessment newest = _assessments[0];
            Assessment oldest = _assessments[_assessments.Count - 1];

            decimal weightDiff = newest.Weight - oldest.Weight;
            decimal bmiDiff = newest.BMI - oldest.BMI;

            string weightDiffStr = weightDiff > 0 ? $"+{weightDiff:F1} kg" : $"{weightDiff:F1} kg";
            string weightSub = _assessments.Count > 1 ? $"From {oldest.Weight:F1} to {newest.Weight:F1} kg" : "Baseline recorded";
            Color weightColor = weightDiff < 0 ? Color.FromArgb(16, 185, 129) : (weightDiff > 0 ? Color.FromArgb(249, 115, 22) : Color.FromArgb(13, 148, 136));
            cardWeightChange = CreateMetricCard("Net Weight Change", weightDiffStr, weightSub, weightColor);
            pnlComparison.Controls.Add(cardWeightChange);

            string bmiDiffStr = bmiDiff > 0 ? $"+{bmiDiff:F1}" : $"{bmiDiff:F1}";
            string bmiSub = _assessments.Count > 1 ? $"Initial: {oldest.BMI:F1} -> Now: {newest.BMI:F1}" : newest.BMICategory;
            Color bmiColor = Color.FromArgb(2, 132, 199);
            cardBmiChange = CreateMetricCard("Net BMI Change", bmiDiffStr, bmiSub, bmiColor);
            pnlComparison.Controls.Add(cardBmiChange);

            cardTotalChecks = CreateMetricCard("Total Checkups", _assessments.Count.ToString(), $"First: {oldest.AssessmentDate:dd MMM yyyy}", Color.FromArgb(124, 58, 237));
            pnlComparison.Controls.Add(cardTotalChecks);

            decimal avgSleep = 0m;
            decimal avgWater = 0m;
            foreach (var a in _assessments)
            {
                avgSleep += a.SleepHours;
                avgWater += a.WaterIntake;
            }
            avgSleep /= _assessments.Count;
            avgWater /= _assessments.Count;

            cardAverages = CreateMetricCard("Lifestyle Averages", $"{avgSleep:F1}h / {avgWater:F1}L", "Avg Sleep / Water Intake", Color.FromArgb(13, 148, 136));
            pnlComparison.Controls.Add(cardAverages);

            // Populate Grid
            dgvAssessments.Columns.Clear();
            dgvAssessments.AutoGenerateColumns = false;

            dgvAssessments.Columns.Add(new DataGridViewTextBoxColumn { Name = "AssessmentDate", HeaderText = "Date", DataPropertyName = "AssessmentDate", FillWeight = 110 });
            dgvAssessments.Columns.Add(new DataGridViewTextBoxColumn { Name = "Weight", HeaderText = "Weight", DataPropertyName = "Weight", FillWeight = 70 });
            dgvAssessments.Columns.Add(new DataGridViewTextBoxColumn { Name = "Height", HeaderText = "Height", DataPropertyName = "Height", FillWeight = 70 });
            dgvAssessments.Columns.Add(new DataGridViewTextBoxColumn { Name = "BMI", HeaderText = "BMI", DataPropertyName = "BMI", FillWeight = 60 });
            dgvAssessments.Columns.Add(new DataGridViewTextBoxColumn { Name = "BMICategory", HeaderText = "Classification", DataPropertyName = "BMICategory", FillWeight = 110 });
            dgvAssessments.Columns.Add(new DataGridViewTextBoxColumn { Name = "ActivityLevel", HeaderText = "Activity", DataPropertyName = "ActivityLevel", FillWeight = 90 });

            var displayList = new List<object>();
            foreach (var a in _assessments)
            {
                displayList.Add(new
                {
                    AssessmentDate = a.AssessmentDate.ToString("dd MMM yyyy, hh:mm tt"),
                    Weight = $"{a.Weight:F1} kg",
                    Height = $"{a.Height:F1} cm",
                    BMI = $"{a.BMI:F1}",
                    BMICategory = a.BMICategory,
                    ActivityLevel = a.ActivityLevel
                });
            }

            dgvAssessments.DataSource = displayList;

            if (_assessments.Count > 0)
            {
                txtSelectedFeedback.Text = _assessments[0].Feedback;
            }
        }

        private void DgvAssessments_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvAssessments.SelectedRows.Count > 0)
            {
                int index = dgvAssessments.SelectedRows[0].Index;
                if (index >= 0 && index < _assessments.Count)
                {
                    txtSelectedFeedback.Text = _assessments[index].Feedback;
                    lblFeedbackHeader.Text = $"Diagnostic Feedback ({_assessments[index].AssessmentDate:dd MMM yyyy}):";
                }
            }
        }
    }
}
