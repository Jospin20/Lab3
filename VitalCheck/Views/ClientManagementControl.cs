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
    /// search registered clients, update information, and select an active client
    /// to conduct assessments and track goals for them.
    /// </summary>
    public class ClientManagementControl : UserControl
    {
        private readonly User _instructorUser;
        private readonly ClientController _clientController;

        // Header
        private Panel pnlHeader;
        private Label lblTitle;
        private Label lblSubtitle;

        // Search Bar
        private TextBox txtSearch;
        private Button btnSearch;
        private Button btnClearSearch;

        // Client Table
        private Panel pnlGridContainer;
        private DataGridView dgvClients;
        private Button btnSetActiveClient;
        private Button btnEditClient;
        private Button btnDeleteClient;

        // Add / Edit Form
        private Panel pnlForm;
        private Label lblFormTitle;
        private TextBox txtFullName;
        private NumericUpDown numAge;
        private ComboBox cmbSex;
        private NumericUpDown numHeight;
        private NumericUpDown numWeight;
        private Button btnSaveClient;
        private Button btnCancelEdit;
        private Label lblFormMessage;

        private int _editingClientId = 0;
        private List<ClientProfile> _clients = new List<ClientProfile>();

        // Event to notify MainDashboardForm when active client changes
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
                Text = "Client Management Directory",
                Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(15, 10),
                AutoSize = true
            };

            lblSubtitle = new Label
            {
                Text = "Register clients, manage profiles, and select an active client for diagnostics.",
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.FromArgb(13, 148, 136),
                Location = new Point(15, 40),
                AutoSize = true
            };

            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblSubtitle);
            this.Controls.Add(pnlHeader);

            // 2. Add / Edit Client Form (Right panel)
            pnlForm = new Panel
            {
                Location = new Point(625, 100),
                Size = new Size(340, 520),
                BackColor = Color.White,
                Padding = new Padding(20)
            };

            lblFormTitle = new Label
            {
                Text = "Register New Client",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(15, 15),
                AutoSize = true
            };

            int formY = 55;
            int step = 60;

            // Full Name
            Label lblName = new Label { Text = "Client Full Name:", Location = new Point(15, formY), AutoSize = true, ForeColor = Color.FromArgb(51, 65, 85) };
            txtFullName = new TextBox { Location = new Point(15, formY + 22), Width = 300, Font = new Font("Segoe UI", 10F) };

            // Age
            formY += step;
            Label lblAge = new Label { Text = "Age (Years):", Location = new Point(15, formY), AutoSize = true, ForeColor = Color.FromArgb(51, 65, 85) };
            numAge = new NumericUpDown { Location = new Point(15, formY + 22), Width = 140, Minimum = 5, Maximum = 120, Value = 28 };

            // Sex
            Label lblSex = new Label { Text = "Biological Sex:", Location = new Point(175, formY), AutoSize = true, ForeColor = Color.FromArgb(51, 65, 85) };
            cmbSex = new ComboBox { Location = new Point(175, formY + 22), Width = 140, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbSex.Items.AddRange(new object[] { "Male", "Female", "Other" });
            cmbSex.SelectedIndex = 0;

            // Height
            formY += step;
            Label lblHeight = new Label { Text = "Height (cm):", Location = new Point(15, formY), AutoSize = true, ForeColor = Color.FromArgb(51, 65, 85) };
            numHeight = new NumericUpDown { Location = new Point(15, formY + 22), Width = 140, Minimum = 50, Maximum = 260, DecimalPlaces = 1, Value = 175.0m };

            // Weight
            Label lblWeight = new Label { Text = "Weight (kg):", Location = new Point(175, formY), AutoSize = true, ForeColor = Color.FromArgb(51, 65, 85) };
            numWeight = new NumericUpDown { Location = new Point(175, formY + 22), Width = 140, Minimum = 20, Maximum = 350, DecimalPlaces = 1, Value = 72.0m };

            // Buttons
            formY += step + 20;
            btnSaveClient = new Button
            {
                Text = "Save Client",
                Location = new Point(15, formY),
                Size = new Size(180, 42),
                BackColor = Color.FromArgb(13, 148, 136),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSaveClient.FlatAppearance.BorderSize = 0;
            btnSaveClient.Click += BtnSaveClient_Click;

            btnCancelEdit = new Button
            {
                Text = "Clear",
                Location = new Point(205, formY),
                Size = new Size(110, 42),
                BackColor = Color.FromArgb(226, 232, 240),
                ForeColor = Color.FromArgb(51, 65, 85),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCancelEdit.FlatAppearance.BorderSize = 0;
            btnCancelEdit.Click += (s, e) => ResetForm();

            lblFormMessage = new Label
            {
                Location = new Point(15, formY + 50),
                Size = new Size(300, 40),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Text = ""
            };

            pnlForm.Controls.Add(lblFormTitle);
            pnlForm.Controls.Add(lblName);
            pnlForm.Controls.Add(txtFullName);
            pnlForm.Controls.Add(lblAge);
            pnlForm.Controls.Add(numAge);
            pnlForm.Controls.Add(lblSex);
            pnlForm.Controls.Add(cmbSex);
            pnlForm.Controls.Add(lblHeight);
            pnlForm.Controls.Add(numHeight);
            pnlForm.Controls.Add(lblWeight);
            pnlForm.Controls.Add(numWeight);
            pnlForm.Controls.Add(btnSaveClient);
            pnlForm.Controls.Add(btnCancelEdit);
            pnlForm.Controls.Add(lblFormMessage);
            this.Controls.Add(pnlForm);

            // 3. Client Grid Container (Left panel)
            pnlGridContainer = new Panel
            {
                Location = new Point(25, 100),
                Size = new Size(580, 520),
                BackColor = Color.White,
                Padding = new Padding(20)
            };

            Label lblSearch = new Label { Text = "Search Clients:", Location = new Point(15, 18), AutoSize = true, ForeColor = Color.FromArgb(51, 65, 85) };
            txtSearch = new TextBox { Location = new Point(115, 15), Width = 230, PlaceholderText = "Search by client name..." };
            txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) LoadClients(); };

            btnSearch = new Button
            {
                Text = "Search",
                Location = new Point(355, 14),
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(2, 132, 199),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSearch.FlatAppearance.BorderSize = 0;
            btnSearch.Click += (s, e) => LoadClients();

            btnClearSearch = new Button
            {
                Text = "Reset",
                Location = new Point(445, 14),
                Size = new Size(70, 30),
                BackColor = Color.FromArgb(226, 232, 240),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnClearSearch.FlatAppearance.BorderSize = 0;
            btnClearSearch.Click += (s, e) => { txtSearch.Clear(); LoadClients(); };

            dgvClients = new DataGridView
            {
                Location = new Point(15, 55),
                Size = new Size(550, 400),
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
            dgvClients.EnableHeadersVisualStyles = false;
            dgvClients.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(15, 23, 42);
            dgvClients.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvClients.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            dgvClients.ColumnHeadersHeight = 32;
            dgvClients.RowTemplate.Height = 30;

            // Action Buttons
            btnSetActiveClient = new Button
            {
                Text = "★ Select as Active Client",
                Location = new Point(15, 465),
                Size = new Size(210, 38),
                BackColor = Color.FromArgb(13, 148, 136),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSetActiveClient.FlatAppearance.BorderSize = 0;
            btnSetActiveClient.Click += BtnSetActiveClient_Click;

            btnEditClient = new Button
            {
                Text = "Edit Profile",
                Location = new Point(235, 465),
                Size = new Size(130, 38),
                BackColor = Color.FromArgb(2, 132, 199),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnEditClient.FlatAppearance.BorderSize = 0;
            btnEditClient.Click += BtnEditClient_Click;

            btnDeleteClient = new Button
            {
                Text = "Delete Client",
                Location = new Point(375, 465),
                Size = new Size(130, 38),
                BackColor = Color.FromArgb(239, 68, 68),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnDeleteClient.FlatAppearance.BorderSize = 0;
            btnDeleteClient.Click += BtnDeleteClient_Click;

            pnlGridContainer.Controls.Add(lblSearch);
            pnlGridContainer.Controls.Add(txtSearch);
            pnlGridContainer.Controls.Add(btnSearch);
            pnlGridContainer.Controls.Add(btnClearSearch);
            pnlGridContainer.Controls.Add(dgvClients);
            pnlGridContainer.Controls.Add(btnSetActiveClient);
            pnlGridContainer.Controls.Add(btnEditClient);
            pnlGridContainer.Controls.Add(btnDeleteClient);
            this.Controls.Add(pnlGridContainer);
        }

        private void LoadClients()
        {
            string query = txtSearch.Text.Trim();
            _clients = _clientController.GetClientsByInstructor(_instructorUser.UserID, query);

            dgvClients.Columns.Clear();
            dgvClients.AutoGenerateColumns = false;

            dgvClients.Columns.Add(new DataGridViewTextBoxColumn { Name = "ClientID", HeaderText = "ID", DataPropertyName = "ClientID", Visible = false });
            dgvClients.Columns.Add(new DataGridViewTextBoxColumn { Name = "FullName", HeaderText = "Client Name", DataPropertyName = "FullName", FillWeight = 140 });
            dgvClients.Columns.Add(new DataGridViewTextBoxColumn { Name = "Age", HeaderText = "Age", DataPropertyName = "Age", FillWeight = 50 });
            dgvClients.Columns.Add(new DataGridViewTextBoxColumn { Name = "Sex", HeaderText = "Sex", DataPropertyName = "Sex", FillWeight = 60 });
            dgvClients.Columns.Add(new DataGridViewTextBoxColumn { Name = "Height", HeaderText = "Height", DataPropertyName = "Height", FillWeight = 70 });
            dgvClients.Columns.Add(new DataGridViewTextBoxColumn { Name = "Weight", HeaderText = "Weight", DataPropertyName = "Weight", FillWeight = 70 });

            var displayList = new List<object>();
            foreach (var c in _clients)
            {
                displayList.Add(new
                {
                    ClientID = c.ClientID,
                    FullName = c.FullName,
                    Age = c.Age,
                    Sex = c.Sex,
                    Height = $"{c.Height:F1} cm",
                    Weight = $"{c.Weight:F1} kg"
                });
            }

            dgvClients.DataSource = displayList;
        }

        private void BtnSaveClient_Click(object? sender, EventArgs e)
        {
            lblFormMessage.Text = "";
            string name = txtFullName.Text.Trim();
            int age = (int)numAge.Value;
            string sex = cmbSex.SelectedItem?.ToString() ?? "Male";
            decimal height = numHeight.Value;
            decimal weight = numWeight.Value;

            if (_editingClientId == 0)
            {
                // Add New Client
                ClientProfile newClient = new ClientProfile(
                    clientId: 0,
                    userId: null,
                    instructorId: _instructorUser.UserID,
                    fullName: name,
                    age: age,
                    sex: sex,
                    height: height,
                    weight: weight,
                    dateCreated: DateTime.Now
                );

                bool success = _clientController.AddClient(newClient, out string errorMessage);
                if (success)
                {
                    lblFormMessage.ForeColor = Color.FromArgb(16, 185, 129);
                    lblFormMessage.Text = "✓ Client registered successfully!";
                    ResetForm();
                    LoadClients();
                    OnActiveClientChanged?.Invoke(newClient);
                }
                else
                {
                    lblFormMessage.ForeColor = Color.FromArgb(220, 38, 38);
                    lblFormMessage.Text = errorMessage;
                }
            }
            else
            {
                // Update Existing Client
                ClientProfile client = new ClientProfile(
                    clientId: _editingClientId,
                    userId: null,
                    instructorId: _instructorUser.UserID,
                    fullName: name,
                    age: age,
                    sex: sex,
                    height: height,
                    weight: weight,
                    dateCreated: DateTime.Now
                );

                bool success = _clientController.UpdateClient(client, out string errorMessage);
                if (success)
                {
                    lblFormMessage.ForeColor = Color.FromArgb(16, 185, 129);
                    lblFormMessage.Text = "✓ Client profile updated!";
                    ResetForm();
                    LoadClients();
                    OnActiveClientChanged?.Invoke(client);
                }
                else
                {
                    lblFormMessage.ForeColor = Color.FromArgb(220, 38, 38);
                    lblFormMessage.Text = errorMessage;
                }
            }
        }

        private void BtnSetActiveClient_Click(object? sender, EventArgs e)
        {
            if (dgvClients.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a client from the table first.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int clientId = Convert.ToInt32(dgvClients.SelectedRows[0].Cells["ClientID"].Value);
            ClientProfile? client = _clientController.GetClientById(clientId);

            if (client != null)
            {
                OnActiveClientChanged?.Invoke(client);
                MessageBox.Show($"Active client switched to: {client.FullName}\r\nHealth assessments and fitness goals are now targeted to this client.",
                                "Active Client Set", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnEditClient_Click(object? sender, EventArgs e)
        {
            if (dgvClients.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a client from the table to edit.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int clientId = Convert.ToInt32(dgvClients.SelectedRows[0].Cells["ClientID"].Value);
            ClientProfile? client = _clientController.GetClientById(clientId);

            if (client != null)
            {
                _editingClientId = client.ClientID;
                lblFormTitle.Text = $"Edit Client: {client.FullName}";
                txtFullName.Text = client.FullName;
                numAge.Value = Math.Clamp(client.Age, 5, 120);
                cmbSex.SelectedItem = client.Sex;
                numHeight.Value = Math.Clamp(client.Height, 50, 260);
                numWeight.Value = Math.Clamp(client.Weight, 20, 350);
                btnSaveClient.Text = "Update Client";
            }
        }

        private void BtnDeleteClient_Click(object? sender, EventArgs e)
        {
            if (dgvClients.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a client to delete.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int clientId = Convert.ToInt32(dgvClients.SelectedRows[0].Cells["ClientID"].Value);
            string clientName = dgvClients.SelectedRows[0].Cells["FullName"].Value?.ToString() ?? "";

            DialogResult dr = MessageBox.Show($"Are you sure you want to permanently delete client '{clientName}' and all associated assessments and fitness goals?",
                                              "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (dr == DialogResult.Yes)
            {
                _clientController.DeleteClient(clientId, out string _);
                LoadClients();
                ResetForm();
            }
        }

        private void ResetForm()
        {
            _editingClientId = 0;
            lblFormTitle.Text = "Register New Client";
            btnSaveClient.Text = "Save Client";
            txtFullName.Clear();
            numAge.Value = 28;
            cmbSex.SelectedIndex = 0;
            numHeight.Value = 175.0m;
            numWeight.Value = 72.0m;
        }
    }
}
