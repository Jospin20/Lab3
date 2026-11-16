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

        // Container Panels
        private Panel pnlHeader;
        private FlowLayoutPanel pnlComparison;
        private TableLayoutPanel pnlBodyGrid;
        private Panel pnlHistoryCard;
        private Panel pnlFeedbackCard;

        // Grid & Feedback Controls
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
            this.Padding = new Padding(25, 20, 25, 25);

            // 1. Top Header Banner (DPI-Safe Container)
            pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 84,
                BackColor = Color.White,
                Padding = new Padding(24, 16, 24, 14),
                Margin = new Padding(0, 0, 0, 16)
            };

            FlowLayoutPanel pnlHeaderFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = Color.Transparent
            };

            Label lblTitle = new Label
            {
                Text = "Progress Analytics & Assessment History",
                Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 4)
            };

            string sub = _activeClient != null
                ? $"Monitoring Progress for: {_activeClient.FullName}"
                : "No active client selected.";

            Label lblSubtitle = new Label
            {
                Text = sub,
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.FromArgb(13, 148, 136),
                AutoSize = true,
                Margin = new Padding(0)
            };

            pnlHeaderFlow.Controls.Add(lblTitle);
            pnlHeaderFlow.Controls.Add(lblSubtitle);
            pnlHeader.Controls.Add(pnlHeaderFlow);
            this.Controls.Add(pnlHeader);

            // 2. Comparative Metric Cards Strip
            pnlComparison = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 110,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoScroll = false,
                Padding = new Padding(0, 10, 0, 10)
            };
            this.Controls.Add(pnlComparison);

            // 3. Middle 2-Column Split: Table on Left (55%), Full Report Viewer on Right (45%)
            pnlBodyGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(0, 12, 0, 0)
            };
            pnlBodyGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));
            pnlBodyGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));

            // Left History Card
            pnlHistoryCard = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(22),
                Margin = new Padding(0, 0, 12, 0)
            };
            BuildHistoryCard();

            // Right Feedback Card
            pnlFeedbackCard = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(22),
                Margin = new Padding(12, 0, 0, 0)
            };
            BuildFeedbackCard();

            pnlBodyGrid.Controls.Add(pnlHistoryCard, 0, 0);
            pnlBodyGrid.Controls.Add(pnlFeedbackCard, 1, 0);

            this.Controls.Add(pnlBodyGrid);
            pnlBodyGrid.BringToFront();
        }

        private void BuildHistoryCard()
        {
            Label lblGridHeading = new Label
            {
                Text = "Historical Checkup Logs",
                Font = new Font("Segoe UI", 12.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Dock = DockStyle.Top,
                Height = 28
            };

            Label lblSub = new Label
            {
                Text = "Select any assessment below to inspect its detailed evaluation:",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(100, 116, 139),
                Dock = DockStyle.Top,
                Height = 24
            };

            dgvAssessments = new DataGridView
            {
                Dock = DockStyle.Fill,
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
            dgvAssessments.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvAssessments.ColumnHeadersHeight = 36;
            dgvAssessments.RowTemplate.Height = 32;
            dgvAssessments.SelectionChanged += DgvAssessments_SelectionChanged;

            pnlHistoryCard.Controls.Add(dgvAssessments);
            pnlHistoryCard.Controls.Add(lblSub);
            pnlHistoryCard.Controls.Add(lblGridHeading);
            dgvAssessments.BringToFront();
        }

        private void BuildFeedbackCard()
        {
            lblFeedbackHeader = new Label
            {
                Text = "Diagnostic Report for Selected Checkup",
                Font = new Font("Segoe UI", 12.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Dock = DockStyle.Top,
                Height = 28
            };

            Label lblSub = new Label
            {
                Text = "Stored recommendations, metabolic data, and lifestyle feedback:",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(100, 116, 139),
                Dock = DockStyle.Top,
                Height = 24
            };

            txtSelectedFeedback = new TextBox
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

            pnlFeedbackCard.Controls.Add(txtSelectedFeedback);
            pnlFeedbackCard.Controls.Add(lblSub);
            pnlFeedbackCard.Controls.Add(lblFeedbackHeader);
            txtSelectedFeedback.BringToFront();
        }

        private Panel CreateMetricCard(string title, string value, string subtitle, Color accentColor)
        {
            Panel card = new Panel
            {
                Size = new Size(230, 90),
                BackColor = Color.White,
                Margin = new Padding(0, 0, 16, 0),
                Padding = new Padding(14, 10, 14, 10)
            };

            Panel bar = new Panel { Dock = DockStyle.Left, Width = 5, BackColor = accentColor };
            Label lblT = new Label { Text = title, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), ForeColor = Color.FromArgb(100, 116, 139), Location = new Point(14, 8), AutoSize = true };
            Label lblV = new Label { Text = value, Font = new Font("Segoe UI", 15F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(12, 26), AutoSize = true };
            Label lblS = new Label { Text = subtitle, Font = new Font("Segoe UI", 8F), ForeColor = accentColor, Location = new Point(14, 62), AutoSize = true };

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
                pnlComparison.Controls.Add(CreateMetricCard("CHECKUPS LOGGED", "0", "No assessments on record", Color.FromArgb(100, 116, 139)));
                txtSelectedFeedback.Text = "No previous assessments recorded for this client. Conduct an assessment to begin tracking development.";
                dgvAssessments.DataSource = null;
                return;
            }

            // Calculations
            Assessment newest = _assessments[0];
            Assessment oldest = _assessments[_assessments.Count - 1];

            decimal weightDiff = newest.Weight - oldest.Weight;
            decimal bmiDiff = newest.BMI - oldest.BMI;

            string weightDiffStr = weightDiff > 0 ? $"+{weightDiff:F1} kg" : $"{weightDiff:F1} kg";
            string weightSub = _assessments.Count > 1 ? $"From {oldest.Weight:F1} to {newest.Weight:F1} kg" : "Baseline established";
            Color weightColor = weightDiff < 0 ? Color.FromArgb(16, 185, 129) : (weightDiff > 0 ? Color.FromArgb(249, 115, 22) : Color.FromArgb(13, 148, 136));
            pnlComparison.Controls.Add(CreateMetricCard("NET WEIGHT CHANGE", weightDiffStr, weightSub, weightColor));

            string bmiDiffStr = bmiDiff > 0 ? $"+{bmiDiff:F1}" : $"{bmiDiff:F1}";
            string bmiSub = _assessments.Count > 1 ? $"Initial: {oldest.BMI:F1} → Now: {newest.BMI:F1}" : newest.BMICategory;
            pnlComparison.Controls.Add(CreateMetricCard("NET BMI CHANGE", bmiDiffStr, bmiSub, Color.FromArgb(2, 132, 199)));

            pnlComparison.Controls.Add(CreateMetricCard("TOTAL CHECKUPS", _assessments.Count.ToString(), $"Baseline: {oldest.AssessmentDate:dd MMM yyyy}", Color.FromArgb(124, 58, 237)));

            decimal avgSleep = 0m;
            decimal avgWater = 0m;
            foreach (var a in _assessments)
            {
                avgSleep += a.SleepHours;
                avgWater += a.WaterIntake;
            }
            avgSleep /= _assessments.Count;
            avgWater /= _assessments.Count;

            pnlComparison.Controls.Add(CreateMetricCard("LIFESTYLE AVERAGES", $"{avgSleep:F1}h / {avgWater:F1}L", "Avg Sleep / Water Intake", Color.FromArgb(13, 148, 136)));

            // Populate Grid
            dgvAssessments.Columns.Clear();
            dgvAssessments.AutoGenerateColumns = false;

            dgvAssessments.Columns.Add(new DataGridViewTextBoxColumn { Name = "AssessmentDate", HeaderText = "Date & Time", DataPropertyName = "AssessmentDate", FillWeight = 110 });
            dgvAssessments.Columns.Add(new DataGridViewTextBoxColumn { Name = "Weight", HeaderText = "Weight", DataPropertyName = "Weight", FillWeight = 65 });
            dgvAssessments.Columns.Add(new DataGridViewTextBoxColumn { Name = "Height", HeaderText = "Height", DataPropertyName = "Height", FillWeight = 65 });
            dgvAssessments.Columns.Add(new DataGridViewTextBoxColumn { Name = "BMI", HeaderText = "BMI", DataPropertyName = "BMI", FillWeight = 55 });
            dgvAssessments.Columns.Add(new DataGridViewTextBoxColumn { Name = "BMICategory", HeaderText = "Classification", DataPropertyName = "BMICategory", FillWeight = 110 });
            dgvAssessments.Columns.Add(new DataGridViewTextBoxColumn { Name = "ActivityLevel", HeaderText = "Activity", DataPropertyName = "ActivityLevel", FillWeight = 85 });

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
                    lblFeedbackHeader.Text = $"Diagnostic Report ({_assessments[index].AssessmentDate:dd MMM yyyy}):";
                }
            }
        }
    }
}
