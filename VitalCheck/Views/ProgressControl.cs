using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using VitalCheck.Controllers;
using VitalCheck.Models;

namespace VitalCheck.Views
{
    /// <summary>
    /// ProgressControl displays historical health assessments,
    /// comparisons between initial and latest measurements, and stored feedback.
    /// </summary>
    public class ProgressControl : UserControl
    {
        private readonly User _currentUser;
        private readonly ClientProfile? _activeClient;
        private readonly AssessmentController _assessmentController;

        // GroupBoxes
        private GroupBox grpSummary;
        private Label lblTotalAssessments;
        private Label lblWeightTrend;
        private Label lblBmiTrend;

        private GroupBox grpHistory;
        private DataGridView dgvHistory;

        private GroupBox grpFeedback;
        private TextBox txtStoredFeedback;

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
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
            this.AutoScroll = true;

            int leftMargin = 20;
            int curY = 15;

            // 1. Progress Summary GroupBox (Top)
            grpSummary = new GroupBox
            {
                Text = " Progress Overview & Comparison ",
                Location = new Point(leftMargin, curY),
                Size = new Size(880, 85),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59)
            };

            lblTotalAssessments = new Label
            {
                Text = "Total Assessments: 0",
                Location = new Point(20, 32),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                AutoSize = true
            };

            lblWeightTrend = new Label
            {
                Text = "Weight Change: --",
                Location = new Point(280, 32),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(13, 148, 136), // Teal
                AutoSize = true
            };

            lblBmiTrend = new Label
            {
                Text = "BMI Change: --",
                Location = new Point(560, 32),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                AutoSize = true
            };

            grpSummary.Controls.Add(lblTotalAssessments);
            grpSummary.Controls.Add(lblWeightTrend);
            grpSummary.Controls.Add(lblBmiTrend);
            this.Controls.Add(grpSummary);

            // 2. Bottom Section: History Table (Left) and Feedback (Right)
            curY += 100;

            // History Table Box (Left: 490px wide)
            grpHistory = new GroupBox
            {
                Text = " Previous Assessments ",
                Location = new Point(leftMargin, curY),
                Size = new Size(500, 480),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59)
            };

            dgvHistory = new DataGridView
            {
                Location = new Point(15, 28),
                Size = new Size(470, 435),
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };
            dgvHistory.SelectionChanged += DgvHistory_SelectionChanged;

            dgvHistory.Columns.Add("Date", "Date");
            dgvHistory.Columns["Date"].Width = 85;
            dgvHistory.Columns.Add("Weight", "Weight (kg)");
            dgvHistory.Columns["Weight"].Width = 80;
            dgvHistory.Columns.Add("BMI", "BMI");
            dgvHistory.Columns["BMI"].Width = 60;
            dgvHistory.Columns.Add("Category", "Classification");
            dgvHistory.Columns["Category"].Width = 110;
            dgvHistory.Columns.Add("Activity", "Activity");

            grpHistory.Controls.Add(dgvHistory);
            this.Controls.Add(grpHistory);

            // Feedback Details Box (Right: 365px wide)
            grpFeedback = new GroupBox
            {
                Text = " Stored Wellness Feedback for Selected Checkup ",
                Location = new Point(leftMargin + 515, curY),
                Size = new Size(365, 480),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59)
            };

            txtStoredFeedback = new TextBox
            {
                Location = new Point(15, 28),
                Size = new Size(335, 435),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.FromArgb(30, 41, 59)
            };

            grpFeedback.Controls.Add(txtStoredFeedback);
            this.Controls.Add(grpFeedback);
        }

        /// <summary>
        /// Loads historical assessments and computes comparison statistics.
        /// </summary>
        public void LoadProgressData()
        {
            dgvHistory.Rows.Clear();
            txtStoredFeedback.Clear();

            if (_activeClient == null)
            {
                lblTotalAssessments.Text = "Total Assessments: 0";
                lblWeightTrend.Text = "Weight Change: --";
                lblBmiTrend.Text = "BMI Change: --";
                txtStoredFeedback.Text = "No active client selected.";
                return;
            }

            _assessments = _assessmentController.GetAssessmentsByClient(_activeClient.ClientID);
            lblTotalAssessments.Text = $"Total Assessments: {_assessments.Count}";

            if (_assessments.Count == 0)
            {
                lblWeightTrend.Text = "Weight Change: No data yet";
                lblBmiTrend.Text = "BMI Change: No data yet";
                txtStoredFeedback.Text = "No previous assessment records found.\r\n\r\nComplete a checkup in the 'Health Assessment' tab to begin tracking your progress.";
                return;
            }

            // Populate grid (sorted newest first)
            foreach (var a in _assessments)
            {
                dgvHistory.Rows.Add(
                    a.AssessmentDate.ToString("yyyy-MM-dd"),
                    $"{a.Weight:F1}",
                    $"{a.BMI:F1}",
                    a.BMICategory,
                    a.ActivityLevel
                );
            }

            // === COMPUTE PROGRESS COMPARISON ===
            // Compare first (latest) checkup with last (earliest) checkup
            // Add null/bounds checking to prevent IndexOutOfRangeException

            if (_assessments == null || _assessments.Count == 0)
            {
                lblWeightTrend.Text = "No assessment data available.";
                lblBmiTrend.Text = "Complete an assessment to see progress.";
                return;
            }

            Assessment latest = _assessments[0];
            Assessment earliest = _assessments[_assessments.Count - 1];

            if (_assessments.Count > 1)
            {
                decimal weightDiff = latest.Weight - earliest.Weight;
                decimal bmiDiff = latest.BMI - earliest.BMI;

                string wSign = weightDiff > 0 ? "+" : "";
                string bSign = bmiDiff > 0 ? "+" : "";

                lblWeightTrend.Text = $"Weight Change: {wSign}{weightDiff:F1} kg";
                lblBmiTrend.Text = $"BMI Change: {bSign}{bmiDiff:F1}";

                lblWeightTrend.ForeColor = weightDiff <= 0 ? Color.FromArgb(16, 185, 129) : Color.FromArgb(217, 119, 6);
            }
            else
            {
                lblWeightTrend.Text = $"Current: {latest.Weight:F1} kg (Baseline)";
                lblBmiTrend.Text = $"Current BMI: {latest.BMI:F1}";
            }

            // Select first row by default
            if (dgvHistory.Rows.Count > 0)
            {
                dgvHistory.Rows[0].Selected = true;
                txtStoredFeedback.Text = latest.Feedback;
            }
        }

        private void DgvHistory_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvHistory.SelectedRows.Count > 0)
            {
                // === SAFELY ACCESS ASSESSMENT DATA ===
                // Add null check and bounds validation before accessing array
                if (_assessments != null && _assessments.Count > 0)
                {
                    int index = dgvHistory.SelectedRows[0].Index;
                    if (index >= 0 && index < _assessments.Count)
                    {
                        txtStoredFeedback.Text = _assessments[index].Feedback;
                    }
                }
            }
        }
    }
}
