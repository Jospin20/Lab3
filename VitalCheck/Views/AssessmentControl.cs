using System;
using System.Drawing;
using System.Windows.Forms;
using VitalCheck.Controllers;
using VitalCheck.Models;

namespace VitalCheck.Views
{
    /// <summary>
    /// AssessmentControl provides the interface for entering health and lifestyle metrics,
    /// calculating BMI with age-appropriate rules, and displaying rule-based wellness recommendations.
    /// </summary>
    public class AssessmentControl : UserControl
    {
        private readonly User _currentUser;
        private readonly ClientProfile? _activeClient;
        private readonly AssessmentController _assessmentController;

        // Container Panels
        private Panel pnlHeader;
        private TableLayoutPanel pnlGrid;
        private Panel pnlFormCard;
        private Panel pnlReportCard;

        // Form Inputs
        private NumericUpDown numAge;
        private ComboBox cmbSex;
        private NumericUpDown numHeight;
        private Label lblHeightMeters;
        private NumericUpDown numWeight;
        private ComboBox cmbActivity;
        private NumericUpDown numSleep;
        private NumericUpDown numWater;
        private Button btnSaveAssessment;
        private Button btnReset;
        private Label lblStatusMessage;

        // Live Diagnostic Badges
        private Label lblLiveBmi;
        private Label lblLiveCategory;
        private Label lblLiveIdealWeight;
        private Label lblLiveCalories;
        private Label lblAdolescentAlert;
        private TextBox txtWellnessFeedback;

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public Action? OnAssessmentSaved { get; set; }

        public AssessmentControl(User user, ClientProfile? activeClient)
        {
            _currentUser = user;
            _activeClient = activeClient;
            _assessmentController = new AssessmentController();

            InitializeComponent();
            PopulateDefaults();
            UpdateLiveDiagnostic();
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(248, 250, 252);
            this.Font = new Font("Segoe UI", 10F);
            this.AutoScroll = true;
            this.Padding = new Padding(25, 20, 25, 25);

            // 1. Top Header Banner
            pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 75,
                BackColor = Color.White,
                Padding = new Padding(24, 14, 24, 12),
                Margin = new Padding(0, 0, 0, 16)
            };

            Label lblTitle = new Label
            {
                Text = "Health & Fitness Diagnostic Assessment",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(20, 12),
                AutoSize = true
            };

            string clientText = _activeClient != null
                ? $"Evaluating Subject: {_activeClient.FullName}  •  Baseline Age: {_activeClient.Age}, Biological Sex: {_activeClient.Sex}"
                : "No active client profile selected. Please select a client in the Client Directory.";

            Label lblClientInfo = new Label
            {
                Text = clientText,
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.FromArgb(13, 148, 136),
                Location = new Point(22, 42),
                AutoSize = true
            };

            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblClientInfo);
            this.Controls.Add(pnlHeader);

            // 2. Responsive 2-Column Split
            pnlGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(0, 12, 0, 0)
            };
            pnlGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 430F)); // Left form card fixed width
            pnlGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));  // Right report card takes remaining space

            pnlFormCard = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(22),
                Margin = new Padding(0, 0, 12, 0),
                AutoScroll = true
            };
            BuildFormCard();

            pnlReportCard = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(22),
                Margin = new Padding(12, 0, 0, 0)
            };
            BuildReportCard();

            pnlGrid.Controls.Add(pnlFormCard, 0, 0);
            pnlGrid.Controls.Add(pnlReportCard, 1, 0);

            this.Controls.Add(pnlGrid);
            pnlGrid.BringToFront();
        }

        private void BuildFormCard()
        {
            Label lblHeading = new Label
            {
                Text = "Diagnostic Measurements",
                Font = new Font("Segoe UI", 12.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(15, 12),
                AutoSize = true
            };
            pnlFormCard.Controls.Add(lblHeading);

            int curY = 48;
            int step = 62;

            // Age & Biological Sex
            Label lblAge = new Label { Text = "Age (Years):", Location = new Point(15, curY), AutoSize = true, ForeColor = Color.FromArgb(71, 85, 105), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            numAge = new NumericUpDown { Location = new Point(15, curY + 22), Width = 165, Minimum = 5, Maximum = 120, Value = 25, Font = new Font("Segoe UI", 10F) };
            numAge.ValueChanged += (s, e) => UpdateLiveDiagnostic();

            Label lblSex = new Label { Text = "Biological Sex:", Location = new Point(205, curY), AutoSize = true, ForeColor = Color.FromArgb(71, 85, 105), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            cmbSex = new ComboBox { Location = new Point(205, curY + 22), Width = 175, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10F) };
            cmbSex.Items.AddRange(new object[] { "Male", "Female", "Other" });
            cmbSex.SelectedIndex = 0;
            cmbSex.SelectedIndexChanged += (s, e) => UpdateLiveDiagnostic();

            pnlFormCard.Controls.Add(lblAge);
            pnlFormCard.Controls.Add(numAge);
            pnlFormCard.Controls.Add(lblSex);
            pnlFormCard.Controls.Add(cmbSex);

            // Height (cm)
            curY += step;
            Label lblHeight = new Label { Text = "Height (cm):", Location = new Point(15, curY), AutoSize = true, ForeColor = Color.FromArgb(71, 85, 105), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            numHeight = new NumericUpDown { Location = new Point(15, curY + 22), Width = 165, Minimum = 50, Maximum = 260, DecimalPlaces = 1, Value = 175, Font = new Font("Segoe UI", 10F) };
            lblHeightMeters = new Label { Text = "= 1.75 m", Location = new Point(188, curY + 25), AutoSize = true, ForeColor = Color.FromArgb(100, 116, 139) };
            numHeight.ValueChanged += (s, e) => {
                lblHeightMeters.Text = $"= {(numHeight.Value / 100m):F2} m";
                UpdateLiveDiagnostic();
            };
            pnlFormCard.Controls.Add(lblHeight);
            pnlFormCard.Controls.Add(numHeight);
            pnlFormCard.Controls.Add(lblHeightMeters);

            // Weight (kg)
            Label lblWeight = new Label { Text = "Body Weight (kg):", Location = new Point(205, curY), AutoSize = true, ForeColor = Color.FromArgb(71, 85, 105), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            numWeight = new NumericUpDown { Location = new Point(205, curY + 22), Width = 175, Minimum = 20, Maximum = 350, DecimalPlaces = 1, Value = 70, Font = new Font("Segoe UI", 10F) };
            numWeight.ValueChanged += (s, e) => UpdateLiveDiagnostic();
            pnlFormCard.Controls.Add(lblWeight);
            pnlFormCard.Controls.Add(numWeight);

            // Physical Activity
            curY += step;
            Label lblAct = new Label { Text = "Physical Activity Level:", Location = new Point(15, curY), AutoSize = true, ForeColor = Color.FromArgb(71, 85, 105), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            cmbActivity = new ComboBox { Location = new Point(15, curY + 22), Width = 365, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10F) };
            cmbActivity.Items.AddRange(new object[] { "Sedentary", "Lightly Active", "Moderately Active", "Very Active" });
            cmbActivity.SelectedIndex = 2; // Moderately Active
            cmbActivity.SelectedIndexChanged += (s, e) => UpdateLiveDiagnostic();
            pnlFormCard.Controls.Add(lblAct);
            pnlFormCard.Controls.Add(cmbActivity);

            // Daily Sleep Hours
            curY += step;
            Label lblSleep = new Label { Text = "Daily Sleep (Hours):", Location = new Point(15, curY), AutoSize = true, ForeColor = Color.FromArgb(71, 85, 105), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            numSleep = new NumericUpDown { Location = new Point(15, curY + 22), Width = 165, Minimum = 0, Maximum = 24, DecimalPlaces = 1, Value = 8.0m, Font = new Font("Segoe UI", 10F) };
            numSleep.ValueChanged += (s, e) => UpdateLiveDiagnostic();
            pnlFormCard.Controls.Add(lblSleep);
            pnlFormCard.Controls.Add(numSleep);

            // Daily Water Intake (Litres)
            Label lblWater = new Label { Text = "Daily Water (Litres):", Location = new Point(205, curY), AutoSize = true, ForeColor = Color.FromArgb(71, 85, 105), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            numWater = new NumericUpDown { Location = new Point(205, curY + 22), Width = 175, Minimum = 0, Maximum = 15, DecimalPlaces = 1, Value = 2.5m, Font = new Font("Segoe UI", 10F) };
            numWater.ValueChanged += (s, e) => UpdateLiveDiagnostic();
            pnlFormCard.Controls.Add(lblWater);
            pnlFormCard.Controls.Add(numWater);

            // Buttons
            curY += step + 15;
            btnSaveAssessment = new Button
            {
                Text = "✓ Save Assessment Record",
                Location = new Point(15, curY),
                Size = new Size(245, 44),
                BackColor = Color.FromArgb(13, 148, 136),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSaveAssessment.FlatAppearance.BorderSize = 0;
            btnSaveAssessment.Click += BtnSaveAssessment_Click;

            btnReset = new Button
            {
                Text = "Reset",
                Location = new Point(270, curY),
                Size = new Size(110, 44),
                BackColor = Color.FromArgb(241, 245, 249),
                ForeColor = Color.FromArgb(51, 65, 85),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnReset.FlatAppearance.BorderSize = 0;
            btnReset.Click += (s, e) => PopulateDefaults();

            curY += 50;
            lblStatusMessage = new Label
            {
                Location = new Point(15, curY),
                Size = new Size(365, 30),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Text = ""
            };

            pnlFormCard.Controls.Add(btnSaveAssessment);
            pnlFormCard.Controls.Add(btnReset);
            pnlFormCard.Controls.Add(lblStatusMessage);
        }

        private void BuildReportCard()
        {
            Label lblHeading = new Label
            {
                Text = "Live Clinical & Wellness Evaluation",
                Font = new Font("Segoe UI", 12.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Dock = DockStyle.Top,
                Height = 28
            };

            // Summary Badges Strip (4 key indicators)
            Panel pnlBadges = new Panel
            {
                Dock = DockStyle.Top,
                Height = 85,
                Padding = new Padding(0, 6, 0, 6)
            };

            // Badge 1: BMI
            Panel cardBmi = new Panel { Dock = DockStyle.Left, Width = 180, BackColor = Color.FromArgb(248, 250, 252), Padding = new Padding(10) };
            Label lblT1 = new Label { Text = "BODY MASS INDEX", Font = new Font("Segoe UI", 8F, FontStyle.Bold), ForeColor = Color.FromArgb(100, 116, 139), Dock = DockStyle.Top, Height = 18 };
            lblLiveBmi = new Label { Text = "BMI: 22.9", Font = new Font("Segoe UI", 14F, FontStyle.Bold), ForeColor = Color.FromArgb(13, 148, 136), Dock = DockStyle.Top, Height = 28 };
            lblLiveCategory = new Label { Text = "Normal weight", Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(16, 185, 129), Dock = DockStyle.Top, Height = 20 };
            cardBmi.Controls.Add(lblLiveCategory);
            cardBmi.Controls.Add(lblLiveBmi);
            cardBmi.Controls.Add(lblT1);

            // Badge 2: Ideal Weight
            Panel cardIdeal = new Panel { Dock = DockStyle.Left, Width = 200, BackColor = Color.FromArgb(248, 250, 252), Padding = new Padding(10), Margin = new Padding(10, 0, 0, 0) };
            Label lblT2 = new Label { Text = "HEALTHY WEIGHT (WHO)", Font = new Font("Segoe UI", 8F, FontStyle.Bold), ForeColor = Color.FromArgb(100, 116, 139), Dock = DockStyle.Top, Height = 18 };
            lblLiveIdealWeight = new Label { Text = "56.7 – 76.3 kg", Font = new Font("Segoe UI", 13F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Dock = DockStyle.Top, Height = 28 };
            Label lblSub2 = new Label { Text = "Based on adult height", Font = new Font("Segoe UI", 8.5F), ForeColor = Color.FromArgb(100, 116, 139), Dock = DockStyle.Top, Height = 20 };
            cardIdeal.Controls.Add(lblSub2);
            cardIdeal.Controls.Add(lblLiveIdealWeight);
            cardIdeal.Controls.Add(lblT2);

            // Badge 3: Calories
            Panel cardCal = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(248, 250, 252), Padding = new Padding(10), Margin = new Padding(10, 0, 0, 0) };
            Label lblT3 = new Label { Text = "ENERGY METABOLISM (TDEE)", Font = new Font("Segoe UI", 8F, FontStyle.Bold), ForeColor = Color.FromArgb(100, 116, 139), Dock = DockStyle.Top, Height = 18 };
            lblLiveCalories = new Label { Text = "~2,450 kcal/day", Font = new Font("Segoe UI", 13F, FontStyle.Bold), ForeColor = Color.FromArgb(2, 132, 199), Dock = DockStyle.Top, Height = 28 };
            Label lblSub3 = new Label { Text = "Maintenance intake", Font = new Font("Segoe UI", 8.5F), ForeColor = Color.FromArgb(100, 116, 139), Dock = DockStyle.Top, Height = 20 };
            cardCal.Controls.Add(lblSub3);
            cardCal.Controls.Add(lblLiveCalories);
            cardCal.Controls.Add(lblT3);

            pnlBadges.Controls.Add(cardCal);
            pnlBadges.Controls.Add(cardIdeal);
            pnlBadges.Controls.Add(cardBmi);

            lblAdolescentAlert = new Label
            {
                Text = "* Note: Standard adult BMI classifications do not apply to minors (under 18). Clinical percentiles apply.",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Italic),
                ForeColor = Color.FromArgb(2, 132, 199),
                Dock = DockStyle.Top,
                Height = 24,
                Visible = false
            };

            Label lblReportTitle = new Label
            {
                Text = "Comprehensive Diagnostic Feedback:",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(71, 85, 105),
                Dock = DockStyle.Top,
                Height = 24
            };

            txtWellnessFeedback = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.FromArgb(248, 250, 252),
                Font = new Font("Consolas", 9F),
                ForeColor = Color.FromArgb(30, 41, 59),
                BorderStyle = BorderStyle.FixedSingle
            };

            pnlReportCard.Controls.Add(txtWellnessFeedback);
            pnlReportCard.Controls.Add(lblReportTitle);
            pnlReportCard.Controls.Add(lblAdolescentAlert);
            pnlReportCard.Controls.Add(pnlBadges);
            pnlReportCard.Controls.Add(lblHeading);
            txtWellnessFeedback.BringToFront();
        }

        private void PopulateDefaults()
        {
            if (_activeClient != null)
            {
                numAge.Value = Math.Clamp(_activeClient.Age, 5, 120);
                cmbSex.SelectedItem = _activeClient.Sex;
                numHeight.Value = Math.Clamp(_activeClient.Height, 50, 260);
                numWeight.Value = Math.Clamp(_activeClient.Weight, 20, 350);
            }
            else
            {
                numAge.Value = 25;
                cmbSex.SelectedIndex = 0;
                numHeight.Value = 175;
                numWeight.Value = 70;
            }

            cmbActivity.SelectedIndex = 2; // Moderately Active
            numSleep.Value = 8.0m;
            numWater.Value = 2.5m;
            lblStatusMessage.Text = "";
            UpdateLiveDiagnostic();
        }

        private void UpdateLiveDiagnostic()
        {
            int age = (int)numAge.Value;
            decimal height = numHeight.Value;
            decimal weight = numWeight.Value;
            string sex = cmbSex.SelectedItem?.ToString() ?? "Male";
            string activity = cmbActivity.SelectedItem?.ToString() ?? "Moderately Active";
            decimal sleep = numSleep.Value;
            decimal water = numWater.Value;

            Assessment diagnostic = _assessmentController.CalculateDiagnostic(
                clientId: _activeClient?.ClientID ?? 0,
                age: age,
                heightCm: height,
                weightKg: weight,
                sex: sex,
                activityLevel: activity,
                sleepHours: sleep,
                waterIntake: water
            );

            lblLiveBmi.Text = $"BMI: {diagnostic.BMI:F1}";
            lblLiveCategory.Text = diagnostic.BMICategory;

            // Ideal weight range
            if (age >= 18)
            {
                Assessment.CalculateIdealWeightRange(height, out decimal minW, out decimal maxW);
                lblLiveIdealWeight.Text = $"{minW:F1} – {maxW:F1} kg";
                lblAdolescentAlert.Visible = false;
            }
            else
            {
                lblLiveIdealWeight.Text = "Growth Percentiles";
                lblAdolescentAlert.Visible = true;
            }

            // Caloric calculation
            int bmr = Assessment.CalculateBMR(weight, height, age, sex);
            int tdee = Assessment.CalculateTDEE(bmr, activity);
            lblLiveCalories.Text = $"~{tdee:N0} kcal/day";

            // Category color
            lblLiveCategory.ForeColor = GetBmiColor(diagnostic.BMICategory);

            txtWellnessFeedback.Text = diagnostic.Feedback;
        }

        private void BtnSaveAssessment_Click(object? sender, EventArgs e)
        {
            lblStatusMessage.Text = "";

            if (_activeClient == null || _activeClient.ClientID <= 0)
            {
                MessageBox.Show("Please select an active client before saving an assessment.", "No Client Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int age = (int)numAge.Value;
            decimal height = numHeight.Value;
            decimal weight = numWeight.Value;
            string sex = cmbSex.SelectedItem?.ToString() ?? "Male";
            string activity = cmbActivity.SelectedItem?.ToString() ?? "Moderately Active";
            decimal sleep = numSleep.Value;
            decimal water = numWater.Value;

            Assessment assessment = _assessmentController.CalculateDiagnostic(
                clientId: _activeClient.ClientID,
                age: age,
                heightCm: height,
                weightKg: weight,
                sex: sex,
                activityLevel: activity,
                sleepHours: sleep,
                waterIntake: water
            );

            bool saved = _assessmentController.SaveAssessment(assessment, out string errorMessage);

            if (saved)
            {
                _activeClient.Height = height;
                _activeClient.Weight = weight;
                _activeClient.Age = age;

                lblStatusMessage.ForeColor = Color.FromArgb(16, 185, 129);
                lblStatusMessage.Text = "✓ Assessment saved & profile synchronized!";

                MessageBox.Show($"Assessment saved successfully for {_activeClient.FullName}!\r\nBMI: {assessment.BMI:F1} ({assessment.BMICategory})\r\nWeight goals updated.",
                                "Assessment Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);

                OnAssessmentSaved?.Invoke();
            }
            else
            {
                lblStatusMessage.ForeColor = Color.FromArgb(220, 38, 38);
                lblStatusMessage.Text = errorMessage;
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
