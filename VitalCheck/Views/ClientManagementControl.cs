using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using VitalCheck.Controllers;
using VitalCheck.Models;

namespace VitalCheck.Views
{
    /// <summary>
    /// ClientManagementControl allows Fitness Instructors to register clients,
    /// view the client directory, update profile data, and select an active client.
    /// </summary>
    public class ClientManagementControl : UserControl
    {
        private readonly User _instructorUser;
        private readonly ClientController _clientController;

        // GroupBoxes
        private GroupBox grpClientList;
        private GroupBox grpClientForm;

        // Search Controls
        private Label lblSearch;
        private TextBox txtSearch;
        private Button btnSearch;
        private Button btnShowAll;

        // DataGridView
        private DataGridView dgvClients;
        private Button btnSetActive;
        private Button btnDelete;

        // Form Controls
        private Label lblName;
        private TextBox txtFullName;
        private Label lblAge;
        private NumericUpDown numAge;
        private Label lblSex;
        private ComboBox cmbSex;
        private Label lblHeight;
        private NumericUpDown numHeight;
        private Label lblWeight;
        private NumericUpDown numWeight;
        private Button btnSave;
        private Button btnClear;
        private Label lblFormStatus;

        private int _editingClientId = 0;
        private List<ClientProfile> _clients = new List<ClientProfile>();

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public Action<ClientProfile>? OnActiveClientChanged { get; set; }

        public ClientManagementControl(User instructorUser)
        {
            _instructorUser = instructorUser;
            _clientController = new ClientController();

            InitializeComponent();
            LoadClients();
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
            this.AutoScroll = true;

            int leftMargin = 20;
            int curY = 15;

            // 1. Client List GroupBox (Left: 540px wide)
            grpClientList = new GroupBox
            {
                Text = " Client Directory ",
                Location = new Point(leftMargin, curY),
                Size = new Size(540, 580),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59)
            };

            // Search row
            lblSearch = new Label { Text = "Search Name:", Location = new Point(15, 28), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Regular), ForeColor = Color.FromArgb(51, 65, 85) };
            txtSearch = new TextBox { Location = new Point(105, 25), Width = 230, Font = new Font("Segoe UI", 9F), PlaceholderText = "Enter client name..." };
            txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) BtnSearch_Click(s, e); };

            btnSearch = new Button
            {
                Text = "Search",
                Location = new Point(345, 24),
                Size = new Size(85, 27),
                BackColor = Color.FromArgb(13, 148, 136),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSearch.FlatAppearance.BorderSize = 0;
            btnSearch.Click += BtnSearch_Click;

            btnShowAll = new Button
            {
                Text = "Show All",
                Location = new Point(440, 24),
                Size = new Size(85, 27),
                BackColor = Color.FromArgb(226, 232, 240),
                ForeColor = Color.FromArgb(51, 65, 85),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                Cursor = Cursors.Hand
            };
            btnShowAll.FlatAppearance.BorderSize = 0;
            btnShowAll.Click += (s, e) => { txtSearch.Clear(); LoadClients(); };

            grpClientList.Controls.Add(lblSearch);
            grpClientList.Controls.Add(txtSearch);
            grpClientList.Controls.Add(btnSearch);
            grpClientList.Controls.Add(btnShowAll);

            // DataGridView for Clients
            dgvClients = new DataGridView
            {
                Location = new Point(15, 60),
                Size = new Size(510, 460),
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
            dgvClients.SelectionChanged += DgvClients_SelectionChanged;

            dgvClients.Columns.Add("ClientID", "ID");
            dgvClients.Columns["ClientID"].Width = 40;
            dgvClients.Columns.Add("FullName", "Client Name");
            dgvClients.Columns.Add("Age", "Age");
            dgvClients.Columns["Age"].Width = 50;
            dgvClients.Columns.Add("Sex", "Sex");
            dgvClients.Columns["Sex"].Width = 65;
            dgvClients.Columns.Add("Height", "Height (cm)");
            dgvClients.Columns["Height"].Width = 85;
            dgvClients.Columns.Add("Weight", "Weight (kg)");
            dgvClients.Columns["Weight"].Width = 85;

            grpClientList.Controls.Add(dgvClients);

            // Action buttons under grid
            btnSetActive = new Button
            {
                Text = "Set as Active Subject",
                Location = new Point(15, 532),
                Size = new Size(180, 32),
                BackColor = Color.FromArgb(13, 148, 136),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSetActive.FlatAppearance.BorderSize = 0;
            btnSetActive.Click += BtnSetActive_Click;

            btnDelete = new Button
            {
                Text = "Delete Client",
                Location = new Point(415, 532),
                Size = new Size(110, 32),
                BackColor = Color.FromArgb(226, 232, 240),
                ForeColor = Color.FromArgb(220, 38, 38),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnDelete.FlatAppearance.BorderSize = 0;
            btnDelete.Click += BtnDelete_Click;

            grpClientList.Controls.Add(btnSetActive);
            grpClientList.Controls.Add(btnDelete);
            this.Controls.Add(grpClientList);

            // 2. Client Form GroupBox (Right: 330px wide)
            grpClientForm = new GroupBox
            {
                Text = " Register / Update Client ",
                Location = new Point(leftMargin + 555, curY),
                Size = new Size(330, 580),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59)
            };

            int formY = 30;
            int step = 60;

            lblName = new Label { Text = "Client Full Name:", Location = new Point(15, formY), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Regular), ForeColor = Color.FromArgb(51, 65, 85) };
            txtFullName = new TextBox { Location = new Point(15, formY + 20), Width = 295, Font = new Font("Segoe UI", 9.5F) };

            formY += step;
            lblAge = new Label { Text = "Age (Years):", Location = new Point(15, formY), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Regular), ForeColor = Color.FromArgb(51, 65, 85) };
            numAge = new NumericUpDown { Location = new Point(15, formY + 20), Width = 140, Minimum = 5, Maximum = 120, Value = 30, Font = new Font("Segoe UI", 9.5F) };

            lblSex = new Label { Text = "Biological Sex:", Location = new Point(165, formY), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Regular), ForeColor = Color.FromArgb(51, 65, 85) };
            cmbSex = new ComboBox { Location = new Point(165, formY + 20), Width = 145, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9.5F) };
            cmbSex.Items.AddRange(new object[] { "Male", "Female", "Other" });
            cmbSex.SelectedIndex = 0;

            formY += step;
            lblHeight = new Label { Text = "Baseline Height (cm):", Location = new Point(15, formY), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Regular), ForeColor = Color.FromArgb(51, 65, 85) };
            numHeight = new NumericUpDown { Location = new Point(15, formY + 20), Width = 140, Minimum = 50, Maximum = 250, DecimalPlaces = 1, Value = 175, Font = new Font("Segoe UI", 9.5F) };

            lblWeight = new Label { Text = "Baseline Weight (kg):", Location = new Point(165, formY), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Regular), ForeColor = Color.FromArgb(51, 65, 85) };
            numWeight = new NumericUpDown { Location = new Point(165, formY + 20), Width = 145, Minimum = 20, Maximum = 350, DecimalPlaces = 1, Value = 75, Font = new Font("Segoe UI", 9.5F) };

            formY += step + 20;
            btnSave = new Button
            {
                Text = "Register Client",
                Location = new Point(15, formY),
                Size = new Size(185, 38),
                BackColor = Color.FromArgb(13, 148, 136),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;

            btnClear = new Button
            {
                Text = "Clear",
                Location = new Point(210, formY),
                Size = new Size(100, 38),
                BackColor = Color.FromArgb(226, 232, 240),
                ForeColor = Color.FromArgb(51, 65, 85),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                Cursor = Cursors.Hand
            };
            btnClear.FlatAppearance.BorderSize = 0;
            btnClear.Click += (s, e) => ResetForm();

            formY += 50;
            lblFormStatus = new Label
            {
                Location = new Point(15, formY),
                Size = new Size(295, 45),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.FromArgb(13, 148, 136),
                Text = ""
            };

            grpClientForm.Controls.Add(lblName);
            grpClientForm.Controls.Add(txtFullName);
            grpClientForm.Controls.Add(lblAge);
            grpClientForm.Controls.Add(numAge);
            grpClientForm.Controls.Add(lblSex);
            grpClientForm.Controls.Add(cmbSex);
            grpClientForm.Controls.Add(lblHeight);
            grpClientForm.Controls.Add(numHeight);
            grpClientForm.Controls.Add(lblWeight);
            grpClientForm.Controls.Add(numWeight);
            grpClientForm.Controls.Add(btnSave);
            grpClientForm.Controls.Add(btnClear);
            grpClientForm.Controls.Add(lblFormStatus);

            this.Controls.Add(grpClientForm);
        }

        private void LoadClients(string searchKeyword = "")
        {
            dgvClients.Rows.Clear();
            _clients = _clientController.GetClientsByInstructor(_instructorUser.UserID);

            foreach (var c in _clients)
            {
                if (!string.IsNullOrWhiteSpace(searchKeyword) &&
                    !c.FullName.Contains(searchKeyword, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                dgvClients.Rows.Add(
                    c.ClientID,
                    c.FullName,
                    c.Age,
                    c.Sex,
                    $"{c.Height:F1}",
                    $"{c.Weight:F1}"
                );
            }
        }

        private void DgvClients_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvClients.SelectedRows.Count > 0)
            {
                int clientId = Convert.ToInt32(dgvClients.SelectedRows[0].Cells["ClientID"].Value);
                ClientProfile? client = _clients.Find(c => c.ClientID == clientId);

                if (client != null)
                {
                    _editingClientId = client.ClientID;
                    txtFullName.Text = client.FullName;
                    numAge.Value = Math.Clamp(client.Age, numAge.Minimum, numAge.Maximum);
                    numHeight.Value = Math.Clamp(client.Height, numHeight.Minimum, numHeight.Maximum);
                    numWeight.Value = Math.Clamp(client.Weight, numWeight.Minimum, numWeight.Maximum);

                    if (string.Equals(client.Sex, "Female", StringComparison.OrdinalIgnoreCase))
                        cmbSex.SelectedIndex = 1;
                    else if (string.Equals(client.Sex, "Other", StringComparison.OrdinalIgnoreCase))
                        cmbSex.SelectedIndex = 2;
                    else
                        cmbSex.SelectedIndex = 0;

                    btnSave.Text = "Update Client";
                    lblFormStatus.Text = $"Editing client ID: {client.ClientID}";
                }
            }
        }

        private void ResetForm()
        {
            _editingClientId = 0;
            txtFullName.Clear();
            numAge.Value = 30;
            numHeight.Value = 175;
            numWeight.Value = 75;
            cmbSex.SelectedIndex = 0;
            btnSave.Text = "Register Client";
            lblFormStatus.Text = "";
            txtFullName.Focus();
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            string fullName = txtFullName.Text.Trim();
            if (string.IsNullOrWhiteSpace(fullName))
            {
                lblFormStatus.ForeColor = Color.FromArgb(220, 38, 38);
                lblFormStatus.Text = "Please enter the client's name.";
                txtFullName.Focus();
                return;
            }

            int age = (int)numAge.Value;
            string sex = cmbSex.SelectedItem?.ToString() ?? "Male";
            decimal height = numHeight.Value;
            decimal weight = numWeight.Value;

            if (_editingClientId == 0)
            {
                // Add new client
                ClientProfile newClient = new ClientProfile
                {
                    InstructorID = _instructorUser.UserID,
                    FullName = fullName,
                    Age = age,
                    Sex = sex,
                    Height = height,
                    Weight = weight,
                    DateCreated = DateTime.Now
                };

                bool success = _clientController.AddClient(newClient, out string err);
                if (success)
                {
                    lblFormStatus.ForeColor = Color.FromArgb(16, 185, 129);
                    lblFormStatus.Text = "✓ Client registered successfully!";
                    LoadClients();
                    ResetForm();
                }
                else
                {
                    lblFormStatus.ForeColor = Color.FromArgb(220, 38, 38);
                    lblFormStatus.Text = $"Error: {err}";
                }
            }
            else
            {
                // Update client
                ClientProfile client = new ClientProfile
                {
                    ClientID = _editingClientId,
                    InstructorID = _instructorUser.UserID,
                    FullName = fullName,
                    Age = age,
                    Sex = sex,
                    Height = height,
                    Weight = weight
                };

                bool success = _clientController.UpdateClient(client, out string err);
                if (success)
                {
                    lblFormStatus.ForeColor = Color.FromArgb(16, 185, 129);
                    lblFormStatus.Text = "✓ Client details updated!";
                    LoadClients();
                    ResetForm();
                }
                else
                {
                    lblFormStatus.ForeColor = Color.FromArgb(220, 38, 38);
                    lblFormStatus.Text = $"Error: {err}";
                }
            }
        }

        private void BtnSetActive_Click(object? sender, EventArgs e)
        {
            if (dgvClients.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a client from the table first.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int clientId = Convert.ToInt32(dgvClients.SelectedRows[0].Cells["ClientID"].Value);
            ClientProfile? client = _clients.Find(c => c.ClientID == clientId);

            if (client != null)
            {
                OnActiveClientChanged?.Invoke(client);
                MessageBox.Show($"Active subject switched to: {client.FullName}", "Active Client Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnSearch_Click(object? sender, EventArgs e)
        {
            LoadClients(txtSearch.Text.Trim());
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (dgvClients.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a client from the table first.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int clientId = Convert.ToInt32(dgvClients.SelectedRows[0].Cells["ClientID"].Value);
            DialogResult confirm = MessageBox.Show(
                "Are you sure you want to delete this client and their associated assessments?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (confirm != DialogResult.Yes) return;

            bool success = _clientController.DeleteClient(clientId, out string err);
            if (success)
            {
                LoadClients();
                ResetForm();
            }
            else
            {
                MessageBox.Show($"Failed to delete client: {err}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
