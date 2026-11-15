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

        // Header
        private Panel pnlHeader;
        private Label lblTitle;
        private Label lblClientNotice;

        // Input Form Controls
        private Panel pnlFormContainer;
        private NumericUpDown numAge;
        private ComboBox cmbSex;
        private NumericUpDown numHeight;
        private Label lblHeightMeters;
        private NumericUpDown numWeight;
        private ComboBox cmbActivity;
        private NumericUpDown numSleep;
        private NumericUpDown numWater;

        // Preview & Diagnostic Controls
        private Panel pnlPreviewContainer;
        private Label lblBmiValue;
        private Label lblBmiCategory;
        private Label lblAdolescentWarning;
        private TextBox txtWellnessFeedback;
        private Button btnSaveAssessment;
        private Button btnReset;
        private Label lblStatusMessage;

        // Callback after assessment saved (to refresh dashboard or progress)
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
                Text = "Health & Fitness Diagnostic Assessment",
                Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(15, 10),
                AutoSize = true
            };

            string clientText = _activeClient != null
                ? $"Evaluating: {_activeClient.FullName} (Age: {_activeClient.Age}, Sex: {_activeClient.Sex})"
                : "No active client profile selected. Please select a client in Client Management.";

            lblClientNotice = new Label
            {
                Text = clientText,
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.FromArgb(13, 148, 136),
                Location = new Point(15, 40),
                AutoSize = true
            };

            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblClientNotice);
            this.Controls.Add(pnlHeader);

            // 2. Left Column: Input Form
            pnlFormContainer = new Panel
            {
                Location = new Point(25, 110),
                Size = new Size(380, 520),
                BackColor = Color.White,
                Padding = new Padding(20)
            };

            Label lblFormHeading = new Label
            {
                Text = "Assessment Measurements",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(15, 15),
                AutoSize = true
            };
            pnlFormContainer.Controls.Add(lblFormHeading);

            int startY = 55;
            int stepY = 62;

            // Age
            Label lblAge = new Label { Text = "Age (Years):", Location = new Point(15, startY), AutoSize = true, ForeColor = Color.FromArgb(51, 65, 85) };
            numAge = new NumericUpDown { Location = new Point(15, startY + 22), Width = 150, Minimum = 5, Maximum = 120, Value = 25 };
            numAge.ValueChanged += (s, e) => UpdateLiveDiagnostic();
            pnlFormContainer.Controls.Add(lblAge);
            pnlFormContainer.Controls.Add(numAge);

            // Sex
            Label lblSex = new Label { Text = "Biological Sex:", Location = new Point(190, startY), AutoSize = true, ForeColor = Color.FromArgb(51, 65, 85) };
            cmbSex = new ComboBox { Location = new Point(190, startY + 22), Width = 160, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbSex.Items.AddRange(new object[] { "Male", "Female", "Other" });
            cmbSex.SelectedIndex = 0;
            pnlFormContainer.Controls.Add(lblSex);
            pnlFormContainer.Controls.Add(cmbSex);

            // Height (cm)
            startY += stepY;
            Label lblHeight = new Label { Text = "Height (Centimetres):", Location = new Point(15, startY), AutoSize = true, ForeColor = Color.FromArgb(51, 65, 85) };
            numHeight = new NumericUpDown { Location = new Point(15, startY + 22), Width = 150, Minimum = 50, Maximum = 260, DecimalPlaces = 1, Value = 175 };
            lblHeightMeters = new Label { Text = "= 1.75 m", Location = new Point(175, startY + 24), AutoSize = true, ForeColor = Color.FromArgb(100, 116, 139) };
            numHeight.ValueChanged += (s, e) => {
                lblHeightMeters.Text = $"= {(numHeight.Value / 100m):F2} m";
                UpdateLiveDiagnostic();
            };
            pnlFormContainer.Controls.Add(lblHeight);
            pnlFormContainer.Controls.Add(numHeight);
            pnlFormContainer.Controls.Add(lblHeightMeters);

            // Weight (kg)
            startY += stepY;
            Label lblWeight = new Label { Text = "Weight (Kilograms):", Location = new Point(15, startY), AutoSize = true, ForeColor = Color.FromArgb(51, 65, 85) };
            numWeight = new NumericUpDown { Location = new Point(15, startY + 22), Width = 150, Minimum = 20, Maximum = 350, DecimalPlaces = 1, Value = 70 };
            numWeight.ValueChanged += (s, e) => UpdateLiveDiagnostic();
            pnlFormContainer.Controls.Add(lblWeight);
            pnlFormContainer.Controls.Add(numWeight);

            // Physical Activity Level
            startY += stepY;
            Label lblAct = new Label { Text = "Physical Activity Level:", Location = new Point(15, startY), AutoSize = true, ForeColor = Color.FromArgb(51, 65, 85) };
            cmbActivity = new ComboBox { Location = new Point(15, startY + 22), Width = 335, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbActivity.Items.AddRange(new object[] { "Sedentary", "Lightly Active", "Moderately Active", "Very Active" });
            cmbActivity.SelectedIndex = 2; // Moderately Active
            cmbActivity.SelectedIndexChanged += (s, e) => UpdateLiveDiagnostic();
            pnlFormContainer.Controls.Add(lblAct);
            pnlFormContainer.Controls.Add(cmbActivity);

            // Sleep Hours
            startY += stepY;
            Label lblSleep = new Label { Text = "Daily Sleep (Hours):", Location = new Point(15, startY), AutoSize = true, ForeColor = Color.FromArgb(51, 65, 85) };
            numSleep = new NumericUpDown { Location = new Point(15, startY + 22), Width = 150, Minimum = 0, Maximum = 24, DecimalPlaces = 1, Value = 8.0m };
            numSleep.ValueChanged += (s, e) => UpdateLiveDiagnostic();
            pnlFormContainer.Controls.Add(lblSleep);
            pnlFormContainer.Controls.Add(numSleep);

            // Water Intake
            Label lblWater = new Label { Text = "Daily Water (Litres):", Location = new Point(190, startY), AutoSize = true, ForeColor = Color.FromArgb(51, 65, 85) };
            numWater = new NumericUpDown { Location = new Point(190, startY + 22), Width = 160, Minimum = 0, Maximum = 15, DecimalPlaces = 1, Value = 2.5m };
            numWater.ValueChanged += (s, e) => UpdateLiveDiagnostic();
            pnlFormContainer.Controls.Add(lblWater);
            pnlFormContainer.Controls.Add(numWater);

            // Save and Reset buttons
            startY += stepY + 10;
            btnSaveAssessment = new Button
            {
                Text = "Save Assessment",
                Location = new Point(15, startY),
                Size = new Size(190, 42),
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
                Location = new Point(220, startY),
                Size = new Size(130, 42),
                BackColor = Color.FromArgb(226, 232, 240),
                ForeColor = Color.FromArgb(51, 65, 85),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnReset.FlatAppearance.BorderSize = 0;
            btnReset.Click += (s, e) => PopulateDefaults();

            lblStatusMessage = new Label
            {
                Location = new Point(15, startY + 48),
                Size = new Size(335, 30),
                ForeColor = Color.FromArgb(16, 185, 129),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Text = ""
            };

            pnlFormContainer.Controls.Add(btnSaveAssessment);
            pnlFormContainer.Controls.Add(btnReset);
            pnlFormContainer.Controls.Add(lblStatusMessage);
            this.Controls.Add(pnlFormContainer);

            // 3. Right Column: Live Diagnostic Preview & Wellness Feedback
            pnlPreviewContainer = new Panel
            {
                Location = new Point(425, 110),
                Size = new Size(540, 520),
                BackColor = Color.White,
                Padding = new Padding(20)
            };

            Label lblPrevHeading = new Label
            {
                Text = "Diagnostic & Wellness Feedback (Live)",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(15, 15),
                AutoSize = true
            };

            Panel pnlBmiCard = new Panel
            {
                Location = new Point(15, 50),
                Size = new Size(505, 75),
                BackColor = Color.FromArgb(241, 245, 249),
                Padding = new Padding(12)
            };

            lblBmiValue = new Label
            {
                Text = "BMI: 22.9",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.FromArgb(13, 148, 136),
                Location = new Point(10, 8),
                AutoSize = true
            };

            lblBmiCategory = new Label
            {
                Text = "Classification: Normal weight",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(16, 185, 129),
                Location = new Point(10, 42),
                AutoSize = true
            };

            pnlBmiCard.Controls.Add(lblBmiValue);
            pnlBmiCard.Controls.Add(lblBmiCategory);

            lblAdolescentWarning = new Label
            {
                Location = new Point(15, 130),
                Size = new Size(505, 30),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Italic),
                ForeColor = Color.FromArgb(2, 132, 199),
                Text = "* Adult BMI classifications are not applied to children and teenagers (under 18).",
                Visible = false
            };

            Label lblFeedbackTitle = new Label
            {
                Text = "Personalized Wellness Recommendations:",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 65, 85),
                Location = new Point(15, 160),
                AutoSize = true
            };

            txtWellnessFeedback = new TextBox
            {
                Location = new Point(15, 185),
                Size = new Size(505, 310),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.FromArgb(248, 250, 252),
                Font = new Font("Consolas", 9.5F),
                ForeColor = Color.FromArgb(30, 41, 59),
                BorderStyle = BorderStyle.FixedSingle
            };

            pnlPreviewContainer.Controls.Add(lblPrevHeading);
            pnlPreviewContainer.Controls.Add(pnlBmiCard);
            pnlPreviewContainer.Controls.Add(lblAdolescentWarning);
            pnlPreviewContainer.Controls.Add(lblFeedbackTitle);
            pnlPreviewContainer.Controls.Add(txtWellnessFeedback);
            this.Controls.Add(pnlPreviewContainer);
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
            string activity = cmbActivity.SelectedItem?.ToString() ?? "Moderately Active";
            decimal sleep = numSleep.Value;
            decimal water = numWater.Value;

            Assessment diagnostic = _assessmentController.CalculateDiagnostic(
                clientId: _activeClient?.ClientID ?? 0,
                age: age,
                heightCm: height,
                weightKg: weight,
                activityLevel: activity,
                sleepHours: sleep,
                waterIntake: water
            );

            lblBmiValue.Text = $"BMI: {diagnostic.BMI:F1}";
            lblBmiCategory.Text = $"Classification: {diagnostic.BMICategory}";

            if (age < 18)
            {
                lblAdolescentWarning.Visible = true;
                lblBmiCategory.ForeColor = Color.FromArgb(2, 132, 199);
            }
            else
            {
                lblAdolescentWarning.Visible = false;
                switch (diagnostic.BMICategory)
                {
                    case "Normal weight":
                        lblBmiCategory.ForeColor = Color.FromArgb(16, 185, 129); // Green
                        break;
                    case "Underweight":
                        lblBmiCategory.ForeColor = Color.FromArgb(217, 119, 6); // Amber
                        break;
                    case "Overweight":
                        lblBmiCategory.ForeColor = Color.FromArgb(249, 115, 22); // Orange
                        break;
                    case "Obese":
                        lblBmiCategory.ForeColor = Color.FromArgb(239, 68, 68); // Red
                        break;
                }
            }

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
            string activity = cmbActivity.SelectedItem?.ToString() ?? "Moderately Active";
            decimal sleep = numSleep.Value;
            decimal water = numWater.Value;

            Assessment assessment = _assessmentController.CalculateDiagnostic(
                clientId: _activeClient.ClientID,
                age: age,
                heightCm: height,
                weightKg: weight,
                activityLevel: activity,
                sleepHours: sleep,
                waterIntake: water
            );

            bool saved = _assessmentController.SaveAssessment(assessment, out string errorMessage);

            if (saved)
            {
                // Synchronize in-memory active client object
                _activeClient.Height = height;
                _activeClient.Weight = weight;
                _activeClient.Age = age;

                lblStatusMessage.ForeColor = Color.FromArgb(16, 185, 129);
                lblStatusMessage.Text = "✓ Assessment saved successfully to database!";

                MessageBox.Show($"Assessment saved successfully for {_activeClient.FullName}!\r\nBMI: {assessment.BMI:F1} ({assessment.BMICategory})",
                                "Assessment Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);

                OnAssessmentSaved?.Invoke();
            }
            else
            {
                lblStatusMessage.ForeColor = Color.FromArgb(220, 38, 38);
                lblStatusMessage.Text = errorMessage;
            }
        }
    }
}
