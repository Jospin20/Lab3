using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using VitalCheck.Controllers;
using VitalCheck.Models;

namespace VitalCheck.Views
{
    /// <summary>
    /// GoalsControl provides the user interface for establishing personal or client fitness goals,
    /// tracking current progress, managing deadlines, and marking goals completed.
    /// </summary>
    public class GoalsControl : UserControl
    {
        private readonly User _currentUser;
        private readonly ClientProfile? _activeClient;
        private readonly GoalController _goalController;

        // Header
        private Panel pnlHeader;
        private Label lblTitle;
        private Label lblSubtitle;

        // Create Goal Form
        private Panel pnlNewGoal;
        private TextBox txtDescription;
        private NumericUpDown numCurrentVal;
        private NumericUpDown numTargetVal;
        private ComboBox cmbUnit;
        private DateTimePicker dtpTargetDate;
        private Button btnAddGoal;
        private Label lblGoalMsg;

        // Grid & Filter Panel
        private Panel pnlGridContainer;
        private ComboBox cmbStatusFilter;
        private DataGridView dgvGoals;
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
                Text = "Fitness Goals & Milestones",
                Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(15, 10),
                AutoSize = true
            };

            string sub = _activeClient != null
                ? $"Managing Goals for: {_activeClient.FullName}"
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

            // 2. Add New Goal Card
            pnlNewGoal = new Panel
            {
                Location = new Point(25, 110),
                Size = new Size(940, 150),
                BackColor = Color.White,
                Padding = new Padding(20)
            };

            Label lblNewGoalHeading = new Label
            {
                Text = "Establish New Fitness Goal",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(15, 12),
                AutoSize = true
            };
            pnlNewGoal.Controls.Add(lblNewGoalHeading);

            // Row 1: Description
            Label lblDesc = new Label { Text = "Goal Description:", Location = new Point(15, 45), AutoSize = true, ForeColor = Color.FromArgb(51, 65, 85) };
            txtDescription = new TextBox { Location = new Point(15, 68), Width = 320, PlaceholderText = "e.g., Target Weight 70 kg, 10k Daily Steps" };

            // Current Value
            Label lblCurrent = new Label { Text = "Current:", Location = new Point(350, 45), AutoSize = true, ForeColor = Color.FromArgb(51, 65, 85) };
            numCurrentVal = new NumericUpDown { Location = new Point(350, 68), Width = 95, Minimum = 0, Maximum = 100000, DecimalPlaces = 1, Value = 75 };

            // Target Value
            Label lblTarget = new Label { Text = "Target:", Location = new Point(460, 45), AutoSize = true, ForeColor = Color.FromArgb(51, 65, 85) };
            numTargetVal = new NumericUpDown { Location = new Point(460, 68), Width = 95, Minimum = 0, Maximum = 100000, DecimalPlaces = 1, Value = 70 };

            // Unit
            Label lblUnit = new Label { Text = "Unit:", Location = new Point(570, 45), AutoSize = true, ForeColor = Color.FromArgb(51, 65, 85) };
            cmbUnit = new ComboBox { Location = new Point(570, 68), Width = 100, DropDownStyle = ComboBoxStyle.DropDown };
            cmbUnit.Items.AddRange(new object[] { "kg", "steps", "hours", "km", "workouts/wk" });
            cmbUnit.SelectedIndex = 0;

            // Target Date
            Label lblDate = new Label { Text = "Target Deadline:", Location = new Point(685, 45), AutoSize = true, ForeColor = Color.FromArgb(51, 65, 85) };
            dtpTargetDate = new DateTimePicker { Location = new Point(685, 68), Width = 125, Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddMonths(1) };

            // Add Button
            btnAddGoal = new Button
            {
                Text = "+ Add Goal",
                Location = new Point(825, 65),
                Size = new Size(100, 32),
                BackColor = Color.FromArgb(13, 148, 136),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnAddGoal.FlatAppearance.BorderSize = 0;
            btnAddGoal.Click += BtnAddGoal_Click;

            lblGoalMsg = new Label
            {
                Location = new Point(15, 112),
                Size = new Size(700, 25),
                ForeColor = Color.FromArgb(16, 185, 129),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Text = ""
            };

            pnlNewGoal.Controls.Add(lblDesc);
            pnlNewGoal.Controls.Add(txtDescription);
            pnlNewGoal.Controls.Add(lblCurrent);
            pnlNewGoal.Controls.Add(numCurrentVal);
            pnlNewGoal.Controls.Add(lblTarget);
            pnlNewGoal.Controls.Add(numTargetVal);
            pnlNewGoal.Controls.Add(lblUnit);
            pnlNewGoal.Controls.Add(cmbUnit);
            pnlNewGoal.Controls.Add(lblDate);
            pnlNewGoal.Controls.Add(dtpTargetDate);
            pnlNewGoal.Controls.Add(btnAddGoal);
            pnlNewGoal.Controls.Add(lblGoalMsg);
            this.Controls.Add(pnlNewGoal);

            // 3. Grid Container
            pnlGridContainer = new Panel
            {
                Location = new Point(25, 275),
                Size = new Size(940, 370),
                BackColor = Color.White,
                Padding = new Padding(20)
            };

            Label lblGridHeading = new Label
            {
                Text = "Current & Past Goals",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(15, 15),
                AutoSize = true
            };

            Label lblFilter = new Label { Text = "Status Filter:", Location = new Point(620, 18), AutoSize = true, ForeColor = Color.FromArgb(51, 65, 85) };
            cmbStatusFilter = new ComboBox { Location = new Point(715, 15), Width = 130, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStatusFilter.Items.AddRange(new object[] { "All", "In Progress", "Completed", "Cancelled" });
            cmbStatusFilter.SelectedIndex = 0;
            cmbStatusFilter.SelectedIndexChanged += (s, e) => LoadGoals();

            dgvGoals = new DataGridView
            {
                Location = new Point(15, 52),
                Size = new Size(910, 250),
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
            dgvGoals.EnableHeadersVisualStyles = false;
            dgvGoals.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(15, 23, 42);
            dgvGoals.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvGoals.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvGoals.ColumnHeadersHeight = 35;
            dgvGoals.RowTemplate.Height = 32;

            // Action Buttons below grid
            btnUpdateProgress = new Button
            {
                Text = "Update Current Value",
                Location = new Point(15, 315),
                Size = new Size(180, 36),
                BackColor = Color.FromArgb(2, 132, 199),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnUpdateProgress.FlatAppearance.BorderSize = 0;
            btnUpdateProgress.Click += BtnUpdateProgress_Click;

            btnMarkCompleted = new Button
            {
                Text = "Mark Completed",
                Location = new Point(205, 315),
                Size = new Size(160, 36),
                BackColor = Color.FromArgb(16, 185, 129),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnMarkCompleted.FlatAppearance.BorderSize = 0;
            btnMarkCompleted.Click += BtnMarkCompleted_Click;

            btnDeleteGoal = new Button
            {
                Text = "Delete Goal",
                Location = new Point(375, 315),
                Size = new Size(130, 36),
                BackColor = Color.FromArgb(239, 68, 68),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnDeleteGoal.FlatAppearance.BorderSize = 0;
            btnDeleteGoal.Click += BtnDeleteGoal_Click;

            pnlGridContainer.Controls.Add(lblGridHeading);
            pnlGridContainer.Controls.Add(lblFilter);
            pnlGridContainer.Controls.Add(cmbStatusFilter);
            pnlGridContainer.Controls.Add(dgvGoals);
            pnlGridContainer.Controls.Add(btnUpdateProgress);
            pnlGridContainer.Controls.Add(btnMarkCompleted);
            pnlGridContainer.Controls.Add(btnDeleteGoal);
            this.Controls.Add(pnlGridContainer);
        }

        private void LoadGoals()
        {
            if (_activeClient == null || _activeClient.ClientID <= 0)
            {
                dgvGoals.DataSource = null;
                return;
            }

            string filter = cmbStatusFilter.SelectedItem?.ToString() ?? "All";
            List<FitnessGoal> goals = _goalController.GetGoalsByClient(_activeClient.ClientID, filter);

            dgvGoals.Columns.Clear();
            dgvGoals.AutoGenerateColumns = false;

            dgvGoals.Columns.Add(new DataGridViewTextBoxColumn { Name = "GoalID", HeaderText = "ID", DataPropertyName = "GoalID", Visible = false });
            dgvGoals.Columns.Add(new DataGridViewTextBoxColumn { Name = "GoalDescription", HeaderText = "Description", DataPropertyName = "GoalDescription", FillWeight = 160 });
            dgvGoals.Columns.Add(new DataGridViewTextBoxColumn { Name = "CurrentValue", HeaderText = "Current", DataPropertyName = "CurrentValue", FillWeight = 60 });
            dgvGoals.Columns.Add(new DataGridViewTextBoxColumn { Name = "TargetValue", HeaderText = "Target", DataPropertyName = "TargetValue", FillWeight = 60 });
            dgvGoals.Columns.Add(new DataGridViewTextBoxColumn { Name = "Unit", HeaderText = "Unit", DataPropertyName = "Unit", FillWeight = 50 });
            dgvGoals.Columns.Add(new DataGridViewTextBoxColumn { Name = "Progress", HeaderText = "Progress", FillWeight = 70 });
            dgvGoals.Columns.Add(new DataGridViewTextBoxColumn { Name = "TargetDate", HeaderText = "Deadline", DataPropertyName = "TargetDate", FillWeight = 80 });
            dgvGoals.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", DataPropertyName = "Status", FillWeight = 80 });

            // Bind list of custom anonymous objects for clean display
            var displayList = new List<object>();
            foreach (var g in goals)
            {
                int percent = g.CalculateProgressPercentage();
                string deadlineStr = g.TargetDate.ToString("dd MMM yyyy");
                if (g.IsOverdue)
                {
                    deadlineStr += " (OVERDUE)";
                }

                displayList.Add(new
                {
                    GoalID = g.GoalID,
                    GoalDescription = g.GoalDescription,
                    CurrentValue = $"{g.CurrentValue:F1}",
                    TargetValue = $"{g.TargetValue:F1}",
                    Unit = g.Unit,
                    Progress = $"{percent}%",
                    TargetDate = deadlineStr,
                    Status = g.Status
                });
            }

            dgvGoals.DataSource = displayList;

            // Highlight completed or overdue rows
            foreach (DataGridViewRow row in dgvGoals.Rows)
            {
                string status = row.Cells["Status"].Value?.ToString() ?? "";
                string deadline = row.Cells["TargetDate"].Value?.ToString() ?? "";

                if (status == "Completed")
                {
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(16, 185, 129);
                    row.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
                }
                else if (deadline.Contains("OVERDUE"))
                {
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(239, 68, 68);
                }
            }
        }

        private void BtnAddGoal_Click(object? sender, EventArgs e)
        {
            lblGoalMsg.Text = "";

            if (_activeClient == null || _activeClient.ClientID <= 0)
            {
                MessageBox.Show("Please select an active client before adding a goal.", "No Client Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            FitnessGoal newGoal = new FitnessGoal(
                goalId: 0,
                clientId: _activeClient.ClientID,
                goalDescription: txtDescription.Text.Trim(),
                targetValue: numTargetVal.Value,
                currentValue: numCurrentVal.Value,
                unit: cmbUnit.Text.Trim(),
                targetDate: dtpTargetDate.Value.Date,
                status: "In Progress",
                dateCreated: DateTime.Now
            );

            bool success = _goalController.CreateGoal(newGoal, out string errorMessage);

            if (success)
            {
                lblGoalMsg.ForeColor = Color.FromArgb(16, 185, 129);
                lblGoalMsg.Text = "✓ Goal added successfully!";
                txtDescription.Clear();
                LoadGoals();
            }
            else
            {
                lblGoalMsg.ForeColor = Color.FromArgb(220, 38, 38);
                lblGoalMsg.Text = errorMessage;
            }
        }

        private void BtnUpdateProgress_Click(object? sender, EventArgs e)
        {
            if (dgvGoals.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a goal from the table to update.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int goalId = Convert.ToInt32(dgvGoals.SelectedRows[0].Cells["GoalID"].Value);
            string desc = dgvGoals.SelectedRows[0].Cells["GoalDescription"].Value?.ToString() ?? "";
            string currentValStr = dgvGoals.SelectedRows[0].Cells["CurrentValue"].Value?.ToString() ?? "0";
            decimal.TryParse(currentValStr, out decimal currentVal);

            // Simple input dialog for beginner Windows Forms
            Form prompt = new Form
            {
                Width = 360,
                Height = 210,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = "Update Goal Progress",
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false
            };

            Label textLabel = new Label { Left = 20, Top = 15, Width = 300, Text = $"Goal: {desc}" };
            Label valLabel = new Label { Left = 20, Top = 50, Text = "New Current Value:", AutoSize = true };
            NumericUpDown numNewVal = new NumericUpDown { Left = 160, Top = 48, Width = 150, Minimum = 0, Maximum = 100000, DecimalPlaces = 1, Value = currentVal };

            Button confirmation = new Button { Text = "Update", Left = 160, Width = 90, Top = 100, DialogResult = DialogResult.OK, BackColor = Color.FromArgb(2, 132, 199), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            Button cancel = new Button { Text = "Cancel", Left = 260, Width = 60, Top = 100, DialogResult = DialogResult.Cancel };

            prompt.Controls.Add(textLabel);
            prompt.Controls.Add(valLabel);
            prompt.Controls.Add(numNewVal);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(cancel);
            prompt.AcceptButton = confirmation;

            if (prompt.ShowDialog() == DialogResult.OK)
            {
                _goalController.UpdateGoalProgress(goalId, numNewVal.Value, "In Progress", out string err);
                LoadGoals();
            }
        }

        private void BtnMarkCompleted_Click(object? sender, EventArgs e)
        {
            if (dgvGoals.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a goal to mark as completed.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int goalId = Convert.ToInt32(dgvGoals.SelectedRows[0].Cells["GoalID"].Value);
            bool success = _goalController.MarkGoalCompleted(goalId, out string errorMessage);

            if (success)
            {
                MessageBox.Show("Goal marked as completed! Great milestone achievement!", "Goal Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadGoals();
            }
            else
            {
                MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDeleteGoal_Click(object? sender, EventArgs e)
        {
            if (dgvGoals.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a goal to delete.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int goalId = Convert.ToInt32(dgvGoals.SelectedRows[0].Cells["GoalID"].Value);

            DialogResult dr = MessageBox.Show("Are you sure you want to delete this fitness goal?", "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr == DialogResult.Yes)
            {
                _goalController.DeleteGoal(goalId, out string _);
                LoadGoals();
            }
        }
    }
}
