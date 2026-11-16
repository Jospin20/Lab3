using System;
using System.Drawing;
using System.Windows.Forms;
using VitalCheck.Controllers;
using VitalCheck.Models;

namespace VitalCheck.Views
{
    /// <summary>
    /// AssessmentControl provides the interface for entering health measurements,
    /// calculating BMI automatically, evaluating lifestyle habits, and generating
    /// rule-based wellness recommendations.
    /// </summary>
    public class AssessmentControl : UserControl
    {
        private readonly User _currentUser;
        private readonly ClientProfile? _activeClient;
        private readonly AssessmentController _assessmentController;

        // GroupBoxes
        private GroupBox grpInputs;
        private GroupBox grpResults;

        // Input controls
        private Label lblAge;
        private NumericUpDown numAge;
        private Label lblSex;
        private ComboBox cmbSex;

        private Label lblHeight;
        private NumericUpDown numHeight;
        private Label lblHeightMeters;

        private Label lblWeight;
        private NumericUpDown numWeight;

        private Label lblActivity;
        private ComboBox cmbActivity;

        private Label lblSleep;
        private NumericUpDown numSleep;

        private Label lblWater;
        private NumericUpDown numWater;

        private Button btnSaveAssessment;
        private Button btnReset;
        private Label lblStatusMessage;

        // Result controls
        private Label lblBmiResult;
        private Label lblIdealWeight;
        private Label lblMinorNotice;
        private Label lblFeedbackHeading;
        private TextBox txtFeedback;

        // Callback event when assessment is saved
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public Action? OnAssessmentSaved { get; set; }

        public AssessmentControl(User user, ClientProfile? activeClient)
        {
            _currentUser = user;
            _activeClient = activeClient;
            _assessmentController = new AssessmentController();

            InitializeComponent();
            PopulateDefaults();
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
            this.AutoScroll = true;

            int leftMargin = 20;
            int curY = 15;

            // 1. Inputs GroupBox (Left Side: 380px wide)
            grpInputs = new GroupBox
            {
                Text = " Enter Health & Lifestyle Measurements ",
                Location = new Point(leftMargin, curY),
                Size = new Size(390, 580),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59)
            };

            int inputY = 28;
            int step = 62;

            // Age & Biological Sex
            lblAge = new Label { Text = "Age (Years):", Location = new Point(15, inputY), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Regular), ForeColor = Color.FromArgb(51, 65, 85) };
            numAge = new NumericUpDown { Location = new Point(15, inputY + 20), Width = 165, Minimum = 5, Maximum = 120, Value = 25, Font = new Font("Segoe UI", 9.5F) };
            numAge.ValueChanged += (s, e) => UpdateLiveCalculations();

            lblSex = new Label { Text = "Biological Sex:", Location = new Point(200, inputY), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Regular), ForeColor = Color.FromArgb(51, 65, 85) };
            cmbSex = new ComboBox { Location = new Point(200, inputY + 20), Width = 170, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9.5F) };
            cmbSex.Items.AddRange(new object[] { "Male", "Female", "Other" });
            cmbSex.SelectedIndex = 0;
            cmbSex.SelectedIndexChanged += (s, e) => UpdateLiveCalculations();

            grpInputs.Controls.Add(lblAge);
            grpInputs.Controls.Add(numAge);
            grpInputs.Controls.Add(lblSex);
            grpInputs.Controls.Add(cmbSex);

            // Height & Weight
            inputY += step;
            lblHeight = new Label { Text = "Height (cm):", Location = new Point(15, inputY), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Regular), ForeColor = Color.FromArgb(51, 65, 85) };
            numHeight = new NumericUpDown { Location = new Point(15, inputY + 20), Width = 165, Minimum = 50, Maximum = 250, DecimalPlaces = 1, Value = 175, Font = new Font("Segoe UI", 9.5F) };
            lblHeightMeters = new Label { Text = "= 1.75 m", Location = new Point(15, inputY + 44), AutoSize = true, Font = new Font("Segoe UI", 8F), ForeColor = Color.FromArgb(100, 116, 139) };
            numHeight.ValueChanged += (s, e) => {
                lblHeightMeters.Text = $"= {(numHeight.Value / 100m):F2} m";
                UpdateLiveCalculations();
            };

            lblWeight = new Label { Text = "Weight (kg):", Location = new Point(200, inputY), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Regular), ForeColor = Color.FromArgb(51, 65, 85) };
            numWeight = new NumericUpDown { Location = new Point(200, inputY + 20), Width = 170, Minimum = 20, Maximum = 350, DecimalPlaces = 1, Value = 70, Font = new Font("Segoe UI", 9.5F) };
            numWeight.ValueChanged += (s, e) => UpdateLiveCalculations();

            grpInputs.Controls.Add(lblHeight);
            grpInputs.Controls.Add(numHeight);
            grpInputs.Controls.Add(lblHeightMeters);
            grpInputs.Controls.Add(lblWeight);
            grpInputs.Controls.Add(numWeight);

            // Activity Level
            inputY += step + 8;
            lblActivity = new Label { Text = "Physical Activity Level:", Location = new Point(15, inputY), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Regular), ForeColor = Color.FromArgb(51, 65, 85) };
            cmbActivity = new ComboBox { Location = new Point(15, inputY + 20), Width = 355, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9.5F) };
            cmbActivity.Items.AddRange(new object[] { "Sedentary", "Lightly Active", "Moderately Active", "Very Active" });
            cmbActivity.SelectedIndex = 2; // Moderately Active
            cmbActivity.SelectedIndexChanged += (s, e) => UpdateLiveCalculations();

            grpInputs.Controls.Add(lblActivity);
            grpInputs.Controls.Add(cmbActivity);

            // Sleep & Water
            inputY += step;
            lblSleep = new Label { Text = "Daily Sleep (Hours):", Location = new Point(15, inputY), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Regular), ForeColor = Color.FromArgb(51, 65, 85) };
            numSleep = new NumericUpDown { Location = new Point(15, inputY + 20), Width = 165, Minimum = 0, Maximum = 24, DecimalPlaces = 1, Value = 8.0m, Font = new Font("Segoe UI", 9.5F) };
            numSleep.ValueChanged += (s, e) => UpdateLiveCalculations();

            lblWater = new Label { Text = "Daily Water (Litres):", Location = new Point(200, inputY), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Regular), ForeColor = Color.FromArgb(51, 65, 85) };
            numWater = new NumericUpDown { Location = new Point(200, inputY + 20), Width = 170, Minimum = 0, Maximum = 15, DecimalPlaces = 1, Value = 2.5m, Font = new Font("Segoe UI", 9.5F) };
            numWater.ValueChanged += (s, e) => UpdateLiveCalculations();

            grpInputs.Controls.Add(lblSleep);
            grpInputs.Controls.Add(numSleep);
            grpInputs.Controls.Add(lblWater);
            grpInputs.Controls.Add(numWater);

            // Buttons
            inputY += step + 15;
            btnSaveAssessment = new Button
            {
                Text = "Save Assessment Record",
                Location = new Point(15, inputY),
                Size = new Size(235, 42),
                BackColor = Color.FromArgb(13, 148, 136),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSaveAssessment.FlatAppearance.BorderSize = 0;
            btnSaveAssessment.Click += BtnSaveAssessment_Click;

            btnReset = new Button
            {
                Text = "Reset",
                Location = new Point(260, inputY),
                Size = new Size(110, 42),
                BackColor = Color.FromArgb(226, 232, 240),
                ForeColor = Color.FromArgb(51, 65, 85),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                Cursor = Cursors.Hand
            };
            btnReset.FlatAppearance.BorderSize = 0;
            btnReset.Click += (s, e) => PopulateDefaults();

            inputY += 50;
            lblStatusMessage = new Label
            {
                Location = new Point(15, inputY),
                Size = new Size(355, 30),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(13, 148, 136),
                TextAlign = ContentAlignment.MiddleLeft,
                Text = ""
            };

            grpInputs.Controls.Add(btnSaveAssessment);
            grpInputs.Controls.Add(btnReset);
            grpInputs.Controls.Add(lblStatusMessage);

            this.Controls.Add(grpInputs);

            // 2. Results GroupBox (Right Side: 450px wide)
            grpResults = new GroupBox
            {
                Text = " Health Diagnostic Results & Recommendations ",
                Location = new Point(leftMargin + 405, curY),
                Size = new Size(475, 580),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59)
            };

            // Calculated BMI
            lblBmiResult = new Label
            {
                Text = "Calculated BMI: 22.9 (Normal weight)",
                Location = new Point(18, 30),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(13, 148, 136), // Teal
                AutoSize = true
            };

            // Ideal Weight Range (WHO)
            lblIdealWeight = new Label
            {
                Text = "Healthy Adult Weight Range: 56.7 kg – 76.3 kg",
                Location = new Point(18, 62),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.FromArgb(51, 65, 85),
                AutoSize = true
            };

            // Minor Notice
            lblMinorNotice = new Label
            {
                Text = "* Note: Adult BMI categories do not apply to minors (under 18).",
                Location = new Point(18, 88),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Italic),
                ForeColor = Color.FromArgb(217, 119, 6), // Amber
                AutoSize = true,
                Visible = false
            };

            lblFeedbackHeading = new Label
            {
                Text = "Personalized Wellness Feedback:",
                Location = new Point(18, 115),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(71, 85, 105),
                AutoSize = true
            };

            txtFeedback = new TextBox
            {
                Location = new Point(18, 140),
                Size = new Size(438, 420),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                ForeColor = Color.FromArgb(30, 41, 59)
            };

            grpResults.Controls.Add(lblBmiResult);
            grpResults.Controls.Add(lblIdealWeight);
            grpResults.Controls.Add(lblMinorNotice);
            grpResults.Controls.Add(lblFeedbackHeading);
            grpResults.Controls.Add(txtFeedback);

            this.Controls.Add(grpResults);
        }

        /// <summary>
        /// Populates input fields with active client's baseline profile or defaults.
        /// </summary>
        private void PopulateDefaults()
        {
            if (_activeClient != null)
            {
                numAge.Value = Math.Clamp(_activeClient.Age > 0 ? _activeClient.Age : 25, numAge.Minimum, numAge.Maximum);
                numHeight.Value = Math.Clamp(_activeClient.Height > 0 ? _activeClient.Height : 175, numHeight.Minimum, numHeight.Maximum);
                numWeight.Value = Math.Clamp(_activeClient.Weight > 0 ? _activeClient.Weight : 70, numWeight.Minimum, numWeight.Maximum);

                if (string.Equals(_activeClient.Sex, "Female", StringComparison.OrdinalIgnoreCase))
                    cmbSex.SelectedIndex = 1;
                else if (string.Equals(_activeClient.Sex, "Other", StringComparison.OrdinalIgnoreCase))
                    cmbSex.SelectedIndex = 2;
                else
                    cmbSex.SelectedIndex = 0;
            }
            else
            {
                numAge.Value = 25;
                numHeight.Value = 175;
                numWeight.Value = 70;
                cmbSex.SelectedIndex = 0;
            }

            cmbActivity.SelectedIndex = 2; // Moderately active
            numSleep.Value = 8.0m;
            numWater.Value = 2.5m;
            lblStatusMessage.Text = "";

            UpdateLiveCalculations();
        }

        /// <summary>
        /// Recalculates BMI and refreshes the wellness recommendations preview.
        /// </summary>
        private void UpdateLiveCalculations()
        {
            int age = (int)numAge.Value;
            decimal height = numHeight.Value;
            decimal weight = numWeight.Value;
            string sex = cmbSex.SelectedItem?.ToString() ?? "Male";
            string activity = cmbActivity.SelectedItem?.ToString() ?? "Moderately Active";
            decimal sleep = numSleep.Value;
            decimal water = numWater.Value;

            // Calculate BMI
            decimal bmi = Assessment.CalculateBMI(weight, height);
            string category = Assessment.DetermineBMICategory(bmi, age);

            if (age < 18)
            {
                lblBmiResult.Text = $"Calculated BMI: {bmi:F1} (Minor / Adolescent)";
                lblMinorNotice.Visible = true;
            }
            else
            {
                lblBmiResult.Text = $"Calculated BMI: {bmi:F1} ({category})";
                lblMinorNotice.Visible = false;
            }

            // Ideal weight range
            Assessment.CalculateIdealWeightRange(height, out decimal minW, out decimal maxW);
            lblIdealWeight.Text = $"Healthy Adult Weight Range: {minW:F1} kg – {maxW:F1} kg";

            // Generate full recommendations
            string feedback = Assessment.GenerateWellnessFeedback(age, height, weight, sex, bmi, category, activity, sleep, water);
            txtFeedback.Text = feedback;
        }

        /// <summary>
        /// Saves the assessment record into the database.
        /// </summary>
        private void BtnSaveAssessment_Click(object? sender, EventArgs e)
        {
            if (_activeClient == null)
            {
                MessageBox.Show("Please select or register an active client before saving an assessment.", "No Active Client", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int age = (int)numAge.Value;
            decimal height = numHeight.Value;
            decimal weight = numWeight.Value;
            string sex = cmbSex.SelectedItem?.ToString() ?? "Male";
            string activity = cmbActivity.SelectedItem?.ToString() ?? "Moderately Active";
            decimal sleep = numSleep.Value;
            decimal water = numWater.Value;

            decimal bmi = Assessment.CalculateBMI(weight, height);
            string category = Assessment.DetermineBMICategory(bmi, age);
            string feedback = Assessment.GenerateWellnessFeedback(age, height, weight, sex, bmi, category, activity, sleep, water);

            Assessment assessment = new Assessment
            {
                ClientID = _activeClient.ClientID,
                AssessmentDate = DateTime.Now,
                Age = age,
                Height = height,
                Weight = weight,
                BMI = bmi,
                BMICategory = category,
                ActivityLevel = activity,
                SleepHours = sleep,
                WaterIntake = water,
                Feedback = feedback
            };

            bool success = _assessmentController.SaveAssessment(assessment, out string errorMessage);

            if (success)
            {
                lblStatusMessage.ForeColor = Color.FromArgb(16, 185, 129); // Green
                lblStatusMessage.Text = "✓ Assessment record saved successfully!";

                // Trigger callback to refresh parent
                OnAssessmentSaved?.Invoke();

                MessageBox.Show(
                    "Health assessment saved to database!\r\n\r\nYou can review changes in the 'Progress History' tab.",
                    "Assessment Saved",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            else
            {
                lblStatusMessage.ForeColor = Color.FromArgb(220, 38, 38); // Red
                lblStatusMessage.Text = $"Error: {errorMessage}";
            }
        }
    }
}
