using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using VitalCheck.Controllers;
using VitalCheck.Models;

namespace VitalCheck.Views
{
    /// <summary>
    /// GoalsControl allows users and instructors to create, update,
    /// track, and complete fitness goals using standard Windows Forms controls.
    /// </summary>
    public class GoalsControl : UserControl
    {
        private readonly User _currentUser;
        private readonly ClientProfile? _activeClient;
        private readonly GoalController _goalController;

        // GroupBoxes
        private GroupBox grpAddGoal;
        private GroupBox grpGoalList;

        // Add Goal Inputs
        private Label lblDesc;
        private TextBox txtDescription;
        private Label lblCurrent;
        private NumericUpDown numCurrentVal;
        private Label lblTarget;
        private NumericUpDown numTargetVal;
        private Label lblUnit;
        private ComboBox cmbUnit;
        private Label lblTargetDate;
        private DateTimePicker dtpTargetDate;
        private Button btnAddGoal;
        private Label lblAddStatus;

        // Goal List & Management
        private Label lblFilter;
        private ComboBox cmbStatusFilter;
        private DataGridView dgvGoals;

        // Update actions
        private Label lblUpdatePrompt;
        private NumericUpDown numUpdateProgress;
        private Button btnUpdateProgress;
        private Button btnMarkCompleted;
        private Button btnDeleteGoal;

        public GoalsControl(User user, ClientProfile? activeClient)
        {
            _currentUser = user;
            _activeClient = activeClient;
            _goalController = new GoalController();

            InitializeComponent();
            LoadGoals();
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
            this.AutoScroll = true;

            int leftMargin = 20;
            int curY = 15;

            // 1. Add Goal GroupBox (Top)
            grpAddGoal = new GroupBox
            {
                Text = " Create New Fitness Goal ",
                Location = new Point(leftMargin, curY),
                Size = new Size(880, 140),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59)
            };

            // Goal Description
            lblDesc = new Label { Text = "Goal Description:", Location = new Point(15, 28), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Regular), ForeColor = Color.FromArgb(51, 65, 85) };
            txtDescription = new TextBox { Location = new Point(15, 48), Width = 310, Font = new Font("Segoe UI", 9.5F), PlaceholderText = "e.g., Target body weight of 68 kg" };

            // Current Value
            lblCurrent = new Label { Text = "Current:", Location = new Point(340, 28), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Regular), ForeColor = Color.FromArgb(51, 65, 85) };
            numCurrentVal = new NumericUpDown { Location = new Point(340, 48), Width = 90, Minimum = 0, Maximum = 100000, DecimalPlaces = 1, Value = 75, Font = new Font("Segoe UI", 9.5F) };

            // Target Value
            lblTarget = new Label { Text = "Target:", Location = new Point(445, 28), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Regular), ForeColor = Color.FromArgb(51, 65, 85) };
            numTargetVal = new NumericUpDown { Location = new Point(445, 48), Width = 90, Minimum = 0, Maximum = 100000, DecimalPlaces = 1, Value = 70, Font = new Font("Segoe UI", 9.5F) };

            // Unit
            lblUnit = new Label { Text = "Unit:", Location = new Point(550, 28), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Regular), ForeColor = Color.FromArgb(51, 65, 85) };
            cmbUnit = new ComboBox { Location = new Point(550, 48), Width = 80, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9.5F) };
            cmbUnit.Items.AddRange(new object[] { "kg", "km", "steps", "hrs", "reps" });
            cmbUnit.SelectedIndex = 0;

            // Target Date
            lblTargetDate = new Label { Text = "Target Date:", Location = new Point(645, 28), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Regular), ForeColor = Color.FromArgb(51, 65, 85) };
            dtpTargetDate = new DateTimePicker { Location = new Point(645, 48), Width = 115, Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddMonths(1), Font = new Font("Segoe UI", 9.5F) };

            // Add Button
            btnAddGoal = new Button
            {
                Text = "Add Goal",
                Location = new Point(775, 45),
                Size = new Size(90, 32),
                BackColor = Color.FromArgb(13, 148, 136),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnAddGoal.FlatAppearance.BorderSize = 0;
            btnAddGoal.Click += BtnAddGoal_Click;

            // Status message
            lblAddStatus = new Label
            {
                Location = new Point(15, 88),
                Size = new Size(850, 30),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.FromArgb(13, 148, 136),
                Text = ""
            };

            grpAddGoal.Controls.Add(lblDesc);
            grpAddGoal.Controls.Add(txtDescription);
            grpAddGoal.Controls.Add(lblCurrent);
            grpAddGoal.Controls.Add(numCurrentVal);
            grpAddGoal.Controls.Add(lblTarget);
            grpAddGoal.Controls.Add(numTargetVal);
            grpAddGoal.Controls.Add(lblUnit);
            grpAddGoal.Controls.Add(cmbUnit);
            grpAddGoal.Controls.Add(lblTargetDate);
            grpAddGoal.Controls.Add(dtpTargetDate);
            grpAddGoal.Controls.Add(btnAddGoal);
            grpAddGoal.Controls.Add(lblAddStatus);

            this.Controls.Add(grpAddGoal);

            // 2. Goal List GroupBox (Bottom)
            curY += 155;
            grpGoalList = new GroupBox
            {
                Text = " Goal Progress & Management ",
                Location = new Point(leftMargin, curY),
                Size = new Size(880, 440),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59)
            };

            // Filter
            lblFilter = new Label { Text = "Filter Status:", Location = new Point(15, 26), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Regular), ForeColor = Color.FromArgb(51, 65, 85) };
            cmbStatusFilter = new ComboBox { Location = new Point(95, 23), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9F) };
            cmbStatusFilter.Items.AddRange(new object[] { "All Goals", "In Progress", "Completed" });
            cmbStatusFilter.SelectedIndex = 0;
            cmbStatusFilter.SelectedIndexChanged += (s, e) => LoadGoals();

            grpGoalList.Controls.Add(lblFilter);
            grpGoalList.Controls.Add(cmbStatusFilter);

            // DataGridView for Goals
            dgvGoals = new DataGridView
            {
                Location = new Point(15, 60),
                Size = new Size(850, 310),
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
            dgvGoals.SelectionChanged += DgvGoals_SelectionChanged;

            // Configure columns
            dgvGoals.Columns.Add("GoalID", "ID");
            dgvGoals.Columns["GoalID"].Width = 45;
            dgvGoals.Columns.Add("Description", "Goal Description");
            dgvGoals.Columns.Add("Current", "Current");
            dgvGoals.Columns["Current"].Width = 70;
            dgvGoals.Columns.Add("Target", "Target");
            dgvGoals.Columns["Target"].Width = 70;
            dgvGoals.Columns.Add("Unit", "Unit");
            dgvGoals.Columns["Unit"].Width = 55;
            dgvGoals.Columns.Add("TargetDate", "Deadline");
            dgvGoals.Columns["TargetDate"].Width = 90;
            dgvGoals.Columns.Add("Status", "Status");
            dgvGoals.Columns["Status"].Width = 95;
            dgvGoals.Columns.Add("Progress", "Progress (%)");
            dgvGoals.Columns["Progress"].Width = 95;

            grpGoalList.Controls.Add(dgvGoals);

            // Bottom action row: update progress / complete / delete
            int actY = 385;

            lblUpdatePrompt = new Label
            {
                Text = "New Value for Selected:",
                Location = new Point(15, actY + 4),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.FromArgb(51, 65, 85)
            };

            numUpdateProgress = new NumericUpDown
            {
                Location = new Point(160, actY + 1),
                Width = 90,
                Minimum = 0,
                Maximum = 100000,
                DecimalPlaces = 1,
                Font = new Font("Segoe UI", 9.5F)
            };

            btnUpdateProgress = new Button
            {
                Text = "Update Progress",
                Location = new Point(265, actY),
                Size = new Size(125, 30),
                BackColor = Color.FromArgb(13, 148, 136),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnUpdateProgress.FlatAppearance.BorderSize = 0;
            btnUpdateProgress.Click += BtnUpdateProgress_Click;

            btnMarkCompleted = new Button
            {
                Text = "Mark as Completed",
                Location = new Point(405, actY),
                Size = new Size(140, 30),
                BackColor = Color.FromArgb(16, 185, 129), // Green
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnMarkCompleted.FlatAppearance.BorderSize = 0;
            btnMarkCompleted.Click += BtnMarkCompleted_Click;

            btnDeleteGoal = new Button
            {
                Text = "Delete Goal",
                Location = new Point(560, actY),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(226, 232, 240),
                ForeColor = Color.FromArgb(220, 38, 38), // Red
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnDeleteGoal.FlatAppearance.BorderSize = 0;
            btnDeleteGoal.Click += BtnDeleteGoal_Click;

            grpGoalList.Controls.Add(lblUpdatePrompt);
            grpGoalList.Controls.Add(numUpdateProgress);
            grpGoalList.Controls.Add(btnUpdateProgress);
            grpGoalList.Controls.Add(btnMarkCompleted);
            grpGoalList.Controls.Add(btnDeleteGoal);

            this.Controls.Add(grpGoalList);
        }

        /// <summary>
        /// Loads goals for active client into DataGridView.
        /// </summary>
        private void LoadGoals()
        {
            dgvGoals.Rows.Clear();
            if (_activeClient == null) return;

            string filter = cmbStatusFilter.SelectedItem?.ToString() ?? "All Goals";
            List<FitnessGoal> goals = _goalController.GetGoalsByClient(_activeClient.ClientID);

            foreach (var g in goals)
            {
                if (filter == "In Progress" && g.Status != "In Progress") continue;
                if (filter == "Completed" && g.Status != "Completed") continue;

                int pct = g.CalculateProgressPercentage();
                dgvGoals.Rows.Add(
                    g.GoalID,
                    g.GoalDescription,
                    $"{g.CurrentValue:F1}",
                    $"{g.TargetValue:F1}",
                    g.Unit,
                    g.TargetDate.ToString("yyyy-MM-dd"),
                    g.Status,
                    $"{pct}%"
                );
            }
        }

        private void DgvGoals_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvGoals.SelectedRows.Count > 0)
            {
                var row = dgvGoals.SelectedRows[0];
                if (decimal.TryParse(row.Cells["Current"].Value?.ToString(), out decimal currentVal))
                {
                    numUpdateProgress.Value = Math.Clamp(currentVal, numUpdateProgress.Minimum, numUpdateProgress.Maximum);
                }
            }
        }

        private void BtnAddGoal_Click(object? sender, EventArgs e)
        {
            if (_activeClient == null)
            {
                MessageBox.Show("Please select or register an active client before adding a goal.", "No Active Client", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string desc = txtDescription.Text.Trim();
            if (string.IsNullOrWhiteSpace(desc))
            {
                lblAddStatus.ForeColor = Color.FromArgb(220, 38, 38);
                lblAddStatus.Text = "Please enter a description for the goal.";
                txtDescription.Focus();
                return;
            }

            decimal current = numCurrentVal.Value;
            decimal target = numTargetVal.Value;
            string unit = cmbUnit.SelectedItem?.ToString() ?? "kg";
            DateTime targetDate = dtpTargetDate.Value.Date;

            FitnessGoal newGoal = new FitnessGoal
            {
                ClientID = _activeClient.ClientID,
                GoalDescription = desc,
                CurrentValue = current,
                TargetValue = target,
                Unit = unit,
                TargetDate = targetDate,
                Status = "In Progress",
                DateCreated = DateTime.Now
            };

            bool success = _goalController.CreateGoal(newGoal, out string err);

            if (success)
            {
                lblAddStatus.ForeColor = Color.FromArgb(16, 185, 129);
                lblAddStatus.Text = "✓ Goal added successfully!";
                txtDescription.Clear();
                LoadGoals();
            }
            else
            {
                lblAddStatus.ForeColor = Color.FromArgb(220, 38, 38);
                lblAddStatus.Text = $"Error: {err}";
            }
        }

        private void BtnUpdateProgress_Click(object? sender, EventArgs e)
        {
            if (dgvGoals.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a goal from the table to update.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // === SAFELY EXTRACT GOAL ID ===
            // Wrap Convert.ToInt32 in try-catch to handle invalid data
            try
            {
                int goalId = Convert.ToInt32(dgvGoals.SelectedRows[0].Cells["GoalID"].Value);
                decimal newValue = numUpdateProgress.Value;

                bool success = _goalController.UpdateGoalProgress(goalId, newValue, "In Progress", out string err);
                if (success)
                {
                    LoadGoals();
                }
                else
                {
                    MessageBox.Show($"Failed to update progress: {err}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (FormatException)
            {
                MessageBox.Show("Invalid Goal ID format. Please select a valid goal.", "Invalid Data", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (OverflowException)
            {
                MessageBox.Show("Goal ID value is too large. Please select a valid goal.", "Invalid Data", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error extracting goal ID: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnMarkCompleted_Click(object? sender, EventArgs e)
        {
            if (dgvGoals.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a goal from the table.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // === SAFELY EXTRACT GOAL ID ===
            // Wrap Convert.ToInt32 in try-catch to handle invalid data
            try
            {
                int goalId = Convert.ToInt32(dgvGoals.SelectedRows[0].Cells["GoalID"].Value);
                bool success = _goalController.MarkGoalCompleted(goalId, out string err);
                if (success)
                {
                    LoadGoals();
                }
                else
                {
                    MessageBox.Show($"Failed to complete goal: {err}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (FormatException)
            {
                MessageBox.Show("Invalid Goal ID format. Please select a valid goal.", "Invalid Data", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (OverflowException)
            {
                MessageBox.Show("Goal ID value is too large. Please select a valid goal.", "Invalid Data", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error extracting goal ID: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDeleteGoal_Click(object? sender, EventArgs e)
        {
            if (dgvGoals.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a goal from the table to delete.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult confirm = MessageBox.Show("Are you sure you want to delete this fitness goal?", "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            // === SAFELY EXTRACT GOAL ID ===
            // Wrap Convert.ToInt32 in try-catch to handle invalid data
            try
            {
                int goalId = Convert.ToInt32(dgvGoals.SelectedRows[0].Cells["GoalID"].Value);
                bool success = _goalController.DeleteGoal(goalId, out string err);
                if (success)
                {
                    LoadGoals();
                }
                else
                {
                    MessageBox.Show($"Failed to delete goal: {err}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (FormatException)
            {
                MessageBox.Show("Invalid Goal ID format. Please select a valid goal.", "Invalid Data", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (OverflowException)
            {
                MessageBox.Show("Goal ID value is too large. Please select a valid goal.", "Invalid Data", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error extracting goal ID: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
