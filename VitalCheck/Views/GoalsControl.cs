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

        // Container Panels
        private Panel pnlHeader;
        private Panel pnlNewGoalCard;
        private Panel pnlGridCard;

        // Create Goal Form Inputs
        private TextBox txtDescription;
        private NumericUpDown numCurrentVal;
        private NumericUpDown numTargetVal;
        private ComboBox cmbUnit;
        private DateTimePicker dtpTargetDate;
        private Button btnAddGoal;
        private Label lblGoalMsg;

        // Grid & Filter Controls
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
                Text = "Fitness Goals & Milestones",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(20, 12),
                AutoSize = true
            };

            string sub = _activeClient != null
                ? $"Managing Goals for: {_activeClient.FullName}"
                : "No active client selected.";

            Label lblSubtitle = new Label
            {
                Text = sub,
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.FromArgb(13, 148, 136),
                Location = new Point(22, 42),
                AutoSize = true
            };

            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblSubtitle);
            this.Controls.Add(pnlHeader);

            // 2. Add New Goal Card
            pnlNewGoalCard = new Panel
            {
                Dock = DockStyle.Top,
                Height = 145,
                BackColor = Color.White,
                Padding = new Padding(22, 16, 22, 16),
                Margin = new Padding(0, 14, 0, 14)
            };
            BuildNewGoalCard();
            this.Controls.Add(pnlNewGoalCard);

            // 3. Grid Container Card
            pnlGridCard = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(22),
                Margin = new Padding(0, 14, 0, 0)
            };
            BuildGridCard();
            this.Controls.Add(pnlGridCard);
            pnlGridCard.BringToFront();
        }

        private void BuildNewGoalCard()
        {
            Label lblHeading = new Label
            {
                Text = "Establish New Fitness Goal",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(15, 10),
                AutoSize = true
            };
            pnlNewGoalCard.Controls.Add(lblHeading);

            int startY = 40;

            // Description
            Label lblDesc = new Label { Text = "Goal Description:", Location = new Point(15, startY), AutoSize = true, ForeColor = Color.FromArgb(71, 85, 105), Font = new Font("Segoe UI", 8.5F, FontStyle.Bold) };
            txtDescription = new TextBox { Location = new Point(15, startY + 20), Width = 310, Font = new Font("Segoe UI", 9.5F), PlaceholderText = "e.g., Reach target weight of 70 kg" };

            // Current Value
            Label lblCurrent = new Label { Text = "Current:", Location = new Point(340, startY), AutoSize = true, ForeColor = Color.FromArgb(71, 85, 105), Font = new Font("Segoe UI", 8.5F, FontStyle.Bold) };
            numCurrentVal = new NumericUpDown { Location = new Point(340, startY + 20), Width = 95, Minimum = 0, Maximum = 100000, DecimalPlaces = 1, Value = 75, Font = new Font("Segoe UI", 9.5F) };

            // Target Value
            Label lblTarget = new Label { Text = "Target:", Location = new Point(450, startY), AutoSize = true, ForeColor = Color.FromArgb(71, 85, 105), Font = new Font("Segoe UI", 8.5F, FontStyle.Bold) };
            numTargetVal = new NumericUpDown { Location = new Point(450, startY + 20), Width = 95, Minimum = 0, Maximum = 100000, DecimalPlaces = 1, Value = 70, Font = new Font("Segoe UI", 9.5F) };

            // Unit
            Label lblUnit = new Label { Text = "Unit:", Location = new Point(560, startY), AutoSize = true, ForeColor = Color.FromArgb(71, 85, 105), Font = new Font("Segoe UI", 8.5F, FontStyle.Bold) };
            cmbUnit = new ComboBox { Location = new Point(560, startY + 20), Width = 110, DropDownStyle = ComboBoxStyle.DropDown, Font = new Font("Segoe UI", 9.5F) };
            cmbUnit.Items.AddRange(new object[] { "kg", "steps", "hours", "km", "workouts/wk" });
            cmbUnit.SelectedIndex = 0;

            // Target Date
            Label lblDate = new Label { Text = "Target Deadline:", Location = new Point(685, startY), AutoSize = true, ForeColor = Color.FromArgb(71, 85, 105), Font = new Font("Segoe UI", 8.5F, FontStyle.Bold) };
            dtpTargetDate = new DateTimePicker { Location = new Point(685, startY + 20), Width = 135, Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddMonths(1), Font = new Font("Segoe UI", 9.5F) };

            // Add Button
            btnAddGoal = new Button
            {
                Text = "+ Add Goal",
                Location = new Point(835, startY + 18),
                Size = new Size(115, 32),
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
                Location = new Point(15, startY + 58),
                Size = new Size(800, 25),
                ForeColor = Color.FromArgb(16, 185, 129),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Text = ""
            };

            pnlNewGoalCard.Controls.Add(lblDesc);
            pnlNewGoalCard.Controls.Add(txtDescription);
            pnlNewGoalCard.Controls.Add(lblCurrent);
            pnlNewGoalCard.Controls.Add(numCurrentVal);
            pnlNewGoalCard.Controls.Add(lblTarget);
            pnlNewGoalCard.Controls.Add(numTargetVal);
            pnlNewGoalCard.Controls.Add(lblUnit);
            pnlNewGoalCard.Controls.Add(cmbUnit);
            pnlNewGoalCard.Controls.Add(lblDate);
            pnlNewGoalCard.Controls.Add(dtpTargetDate);
            pnlNewGoalCard.Controls.Add(btnAddGoal);
            pnlNewGoalCard.Controls.Add(lblGoalMsg);
        }

        private void BuildGridCard()
        {
            Panel pnlTopFilter = new Panel
            {
                Dock = DockStyle.Top,
                Height = 42,
                Padding = new Padding(0)
            };

            Label lblGridHeading = new Label
            {
                Text = "Current & Past Fitness Goals",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Dock = DockStyle.Left,
                AutoSize = true
            };

            Panel pnlRightFilter = new Panel { Dock = DockStyle.Right, Width = 260 };
            Label lblFilter = new Label { Text = "Status Filter:", Location = new Point(10, 8), AutoSize = true, ForeColor = Color.FromArgb(71, 85, 105), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            cmbStatusFilter = new ComboBox { Location = new Point(105, 5), Width = 145, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9.5F) };
            cmbStatusFilter.Items.AddRange(new object[] { "All", "In Progress", "Completed", "Cancelled" });
            cmbStatusFilter.SelectedIndex = 0;
            cmbStatusFilter.SelectedIndexChanged += (s, e) => LoadGoals();
            pnlRightFilter.Controls.Add(lblFilter);
            pnlRightFilter.Controls.Add(cmbStatusFilter);

            pnlTopFilter.Controls.Add(lblGridHeading);
            pnlTopFilter.Controls.Add(pnlRightFilter);
            pnlGridCard.Controls.Add(pnlTopFilter);

            // Action Buttons Bar below grid
            Panel pnlActions = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 55,
                Padding = new Padding(0, 10, 0, 0)
            };

            btnUpdateProgress = new Button
            {
                Text = "✎ Update Current Value",
                Dock = DockStyle.Left,
                Width = 200,
                BackColor = Color.FromArgb(2, 132, 199),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnUpdateProgress.FlatAppearance.BorderSize = 0;
            btnUpdateProgress.Click += BtnUpdateProgress_Click;

            Panel sep1 = new Panel { Dock = DockStyle.Left, Width = 15 };

            btnMarkCompleted = new Button
            {
                Text = "★ Mark as Completed",
                Dock = DockStyle.Left,
                Width = 190,
                BackColor = Color.FromArgb(16, 185, 129),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnMarkCompleted.FlatAppearance.BorderSize = 0;
            btnMarkCompleted.Click += BtnMarkCompleted_Click;

            Panel sep2 = new Panel { Dock = DockStyle.Left, Width = 15 };

            btnDeleteGoal = new Button
            {
                Text = "🗑 Delete Goal",
                Dock = DockStyle.Left,
                Width = 140,
                BackColor = Color.FromArgb(239, 68, 68),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnDeleteGoal.FlatAppearance.BorderSize = 0;
            btnDeleteGoal.Click += BtnDeleteGoal_Click;

            pnlActions.Controls.Add(btnDeleteGoal);
            pnlActions.Controls.Add(sep2);
            pnlActions.Controls.Add(btnMarkCompleted);
            pnlActions.Controls.Add(sep1);
            pnlActions.Controls.Add(btnUpdateProgress);
            pnlGridCard.Controls.Add(pnlActions);

            // DataGridView
            dgvGoals = new DataGridView
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
            dgvGoals.EnableHeadersVisualStyles = false;
            dgvGoals.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(15, 23, 42);
            dgvGoals.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvGoals.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            dgvGoals.ColumnHeadersHeight = 36;
            dgvGoals.RowTemplate.Height = 34;

            pnlGridCard.Controls.Add(dgvGoals);
            dgvGoals.BringToFront();
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
            dgvGoals.Columns.Add(new DataGridViewTextBoxColumn { Name = "GoalDescription", HeaderText = "Goal Description", DataPropertyName = "GoalDescription", FillWeight = 160 });
            dgvGoals.Columns.Add(new DataGridViewTextBoxColumn { Name = "CurrentValue", HeaderText = "Current", DataPropertyName = "CurrentValue", FillWeight = 60 });
            dgvGoals.Columns.Add(new DataGridViewTextBoxColumn { Name = "TargetValue", HeaderText = "Target", DataPropertyName = "TargetValue", FillWeight = 60 });
            dgvGoals.Columns.Add(new DataGridViewTextBoxColumn { Name = "Unit", HeaderText = "Unit", DataPropertyName = "Unit", FillWeight = 50 });
            dgvGoals.Columns.Add(new DataGridViewTextBoxColumn { Name = "Progress", HeaderText = "Progress", FillWeight = 70 });
            dgvGoals.Columns.Add(new DataGridViewTextBoxColumn { Name = "TargetDate", HeaderText = "Target Deadline", DataPropertyName = "TargetDate", FillWeight = 90 });
            dgvGoals.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", DataPropertyName = "Status", FillWeight = 80 });

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
                lblGoalMsg.Text = "✓ Fitness goal established successfully!";
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

            Form prompt = new Form
            {
                Width = 380,
                Height = 220,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = "Update Goal Progress",
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.White
            };

            Label textLabel = new Label { Left = 20, Top = 18, Width = 320, Text = $"Goal: {desc}", Font = new Font("Segoe UI", 9.5F, FontStyle.Bold) };
            Label valLabel = new Label { Left = 20, Top = 58, Text = "New Current Value:", AutoSize = true, Font = new Font("Segoe UI", 9F) };
            NumericUpDown numNewVal = new NumericUpDown { Left = 160, Top = 55, Width = 160, Minimum = 0, Maximum = 100000, DecimalPlaces = 1, Value = currentVal, Font = new Font("Segoe UI", 10F) };

            Button confirmation = new Button { Text = "Save Progress", Left = 160, Width = 110, Top = 110, DialogResult = DialogResult.OK, BackColor = Color.FromArgb(2, 132, 199), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            confirmation.FlatAppearance.BorderSize = 0;
            Button cancel = new Button { Text = "Cancel", Left = 280, Width = 70, Top = 110, DialogResult = DialogResult.Cancel, BackColor = Color.FromArgb(241, 245, 249), FlatStyle = FlatStyle.Flat };
            cancel.FlatAppearance.BorderSize = 0;

            prompt.Controls.Add(textLabel);
            prompt.Controls.Add(valLabel);
            prompt.Controls.Add(numNewVal);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(cancel);
            prompt.AcceptButton = confirmation;

            if (prompt.ShowDialog() == DialogResult.OK)
            {
                _goalController.UpdateGoalProgress(goalId, numNewVal.Value, "In Progress", out string _);
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
                MessageBox.Show("Goal marked as completed! Congratulations on achieving this milestone!", "Milestone Achieved", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

            DialogResult dr = MessageBox.Show("Are you sure you want to permanently delete this fitness goal?", "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr == DialogResult.Yes)
            {
                _goalController.DeleteGoal(goalId, out string _);
                LoadGoals();
            }
        }
    }
}
