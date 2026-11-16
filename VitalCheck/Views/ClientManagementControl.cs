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

        // Container Panels
        private Panel pnlHeader;
        private TableLayoutPanel pnlGrid;
        private Panel pnlTableCard;
        private Panel pnlFormCard;

        // Search Bar Controls
        private TextBox txtSearch;
        private Button btnSearch;
        private Button btnClearSearch;

        // Table & Action Controls
        private DataGridView dgvClients;
        private Button btnSetActiveClient;
        private Button btnEditClient;
        private Button btnDeleteClient;

        // Form Controls
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
                Text = "Client Directory & Management",
                Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 4)
            };

            Label lblSubtitle = new Label
            {
                Text = "Register clients, update demographic information, and switch active subjects.",
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.FromArgb(13, 148, 136),
                AutoSize = true,
                Margin = new Padding(0)
            };

            pnlHeaderFlow.Controls.Add(lblTitle);
            pnlHeaderFlow.Controls.Add(lblSubtitle);
            pnlHeader.Controls.Add(pnlHeaderFlow);
            this.Controls.Add(pnlHeader);

            // 2. Responsive 2-Column Split: Table (60%) on Left, Add/Edit Form (40%) on Right
            pnlGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(0, 12, 0, 0)
            };
            pnlGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62F));
            pnlGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38F));

            // Left Table Card
            pnlTableCard = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(22),
                Margin = new Padding(0, 0, 12, 0)
            };
            BuildTableCard();

            // Right Form Card
            pnlFormCard = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(22),
                Margin = new Padding(12, 0, 0, 0),
                AutoScroll = true
            };
            BuildFormCard();

            pnlGrid.Controls.Add(pnlTableCard, 0, 0);
            pnlGrid.Controls.Add(pnlFormCard, 1, 0);

            this.Controls.Add(pnlGrid);
            pnlGrid.BringToFront();
        }

        private void BuildTableCard()
        {
            // Search strip
            Panel pnlSearch = new Panel
            {
                Dock = DockStyle.Top,
                Height = 44,
                Padding = new Padding(0)
            };

            Label lblSearch = new Label { Text = "Search Clients:", Location = new Point(0, 10), AutoSize = true, ForeColor = Color.FromArgb(71, 85, 105), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            txtSearch = new TextBox { Location = new Point(100, 7), Width = 230, Font = new Font("Segoe UI", 9.5F), PlaceholderText = "Search by client name..." };
            txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) LoadClients(); };

            btnSearch = new Button
            {
                Text = "Search",
                Location = new Point(340, 6),
                Size = new Size(80, 28),
                BackColor = Color.FromArgb(2, 132, 199),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSearch.FlatAppearance.BorderSize = 0;
            btnSearch.Click += (s, e) => LoadClients();

            btnClearSearch = new Button
            {
                Text = "Reset",
                Location = new Point(428, 6),
                Size = new Size(70, 28),
                BackColor = Color.FromArgb(241, 245, 249),
                ForeColor = Color.FromArgb(51, 65, 85),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F),
                Cursor = Cursors.Hand
            };
            btnClearSearch.FlatAppearance.BorderSize = 0;
            btnClearSearch.Click += (s, e) => { txtSearch.Clear(); LoadClients(); };

            pnlSearch.Controls.Add(lblSearch);
            pnlSearch.Controls.Add(txtSearch);
            pnlSearch.Controls.Add(btnSearch);
            pnlSearch.Controls.Add(btnClearSearch);
            pnlTableCard.Controls.Add(pnlSearch);

            // Action Buttons Bar below table
            Panel pnlActions = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 55,
                Padding = new Padding(0, 10, 0, 0)
            };

            btnSetActiveClient = new Button
            {
                Text = "★ Set as Active Client",
                Dock = DockStyle.Left,
                Width = 200,
                BackColor = Color.FromArgb(13, 148, 136),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSetActiveClient.FlatAppearance.BorderSize = 0;
            btnSetActiveClient.Click += BtnSetActiveClient_Click;

            Panel sep1 = new Panel { Dock = DockStyle.Left, Width = 14 };

            btnEditClient = new Button
            {
                Text = "✎ Edit Profile",
                Dock = DockStyle.Left,
                Width = 140,
                BackColor = Color.FromArgb(2, 132, 199),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnEditClient.FlatAppearance.BorderSize = 0;
            btnEditClient.Click += BtnEditClient_Click;

            Panel sep2 = new Panel { Dock = DockStyle.Left, Width = 14 };

            btnDeleteClient = new Button
            {
                Text = "🗑 Delete",
                Dock = DockStyle.Left,
                Width = 110,
                BackColor = Color.FromArgb(239, 68, 68),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnDeleteClient.FlatAppearance.BorderSize = 0;
            btnDeleteClient.Click += BtnDeleteClient_Click;

            pnlActions.Controls.Add(btnDeleteClient);
            pnlActions.Controls.Add(sep2);
            pnlActions.Controls.Add(btnEditClient);
            pnlActions.Controls.Add(sep1);
            pnlActions.Controls.Add(btnSetActiveClient);
            pnlTableCard.Controls.Add(pnlActions);

            // DataGridView
            dgvClients = new DataGridView
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
            dgvClients.EnableHeadersVisualStyles = false;
            dgvClients.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(15, 23, 42);
            dgvClients.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvClients.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            dgvClients.ColumnHeadersHeight = 36;
            dgvClients.RowTemplate.Height = 32;

            pnlTableCard.Controls.Add(dgvClients);
            dgvClients.BringToFront();
        }

        private void BuildFormCard()
        {
            lblFormTitle = new Label
            {
                Text = "Register New Client",
                Font = new Font("Segoe UI", 12.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(15, 12),
                AutoSize = true
            };
            pnlFormCard.Controls.Add(lblFormTitle);

            int formY = 48;
            int step = 60;

            // Full Name
            Label lblName = new Label { Text = "Client Full Name:", Location = new Point(15, formY), AutoSize = true, ForeColor = Color.FromArgb(71, 85, 105), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            txtFullName = new TextBox { Location = new Point(15, formY + 22), Width = 310, Font = new Font("Segoe UI", 10F), PlaceholderText = "e.g., Emily Davies" };

            // Age & Sex
            formY += step;
            Label lblAge = new Label { Text = "Age (Years):", Location = new Point(15, formY), AutoSize = true, ForeColor = Color.FromArgb(71, 85, 105), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            numAge = new NumericUpDown { Location = new Point(15, formY + 22), Width = 145, Minimum = 5, Maximum = 120, Value = 28, Font = new Font("Segoe UI", 10F) };

            Label lblSex = new Label { Text = "Biological Sex:", Location = new Point(175, formY), AutoSize = true, ForeColor = Color.FromArgb(71, 85, 105), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            cmbSex = new ComboBox { Location = new Point(175, formY + 22), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10F) };
            cmbSex.Items.AddRange(new object[] { "Male", "Female", "Other" });
            cmbSex.SelectedIndex = 0;

            // Height & Weight
            formY += step;
            Label lblHeight = new Label { Text = "Height (cm):", Location = new Point(15, formY), AutoSize = true, ForeColor = Color.FromArgb(71, 85, 105), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            numHeight = new NumericUpDown { Location = new Point(15, formY + 22), Width = 145, Minimum = 50, Maximum = 260, DecimalPlaces = 1, Value = 175.0m, Font = new Font("Segoe UI", 10F) };

            Label lblWeight = new Label { Text = "Weight (kg):", Location = new Point(175, formY), AutoSize = true, ForeColor = Color.FromArgb(71, 85, 105), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            numWeight = new NumericUpDown { Location = new Point(175, formY + 22), Width = 150, Minimum = 20, Maximum = 350, DecimalPlaces = 1, Value = 72.0m, Font = new Font("Segoe UI", 10F) };

            // Buttons
            formY += step + 20;
            btnSaveClient = new Button
            {
                Text = "Save Client Record",
                Location = new Point(15, formY),
                Size = new Size(190, 42),
                BackColor = Color.FromArgb(13, 148, 136),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSaveClient.FlatAppearance.BorderSize = 0;
            btnSaveClient.Click += BtnSaveClient_Click;

            btnCancelEdit = new Button
            {
                Text = "Clear",
                Location = new Point(215, formY),
                Size = new Size(110, 42),
                BackColor = Color.FromArgb(241, 245, 249),
                ForeColor = Color.FromArgb(51, 65, 85),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCancelEdit.FlatAppearance.BorderSize = 0;
            btnCancelEdit.Click += (s, e) => ResetForm();

            lblFormMessage = new Label
            {
                Location = new Point(15, formY + 50),
                Size = new Size(310, 40),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Text = ""
            };

            pnlFormCard.Controls.Add(lblName);
            pnlFormCard.Controls.Add(txtFullName);
            pnlFormCard.Controls.Add(lblAge);
            pnlFormCard.Controls.Add(numAge);
            pnlFormCard.Controls.Add(lblSex);
            pnlFormCard.Controls.Add(cmbSex);
            pnlFormCard.Controls.Add(lblHeight);
            pnlFormCard.Controls.Add(numHeight);
            pnlFormCard.Controls.Add(lblWeight);
            pnlFormCard.Controls.Add(numWeight);
            pnlFormCard.Controls.Add(btnSaveClient);
            pnlFormCard.Controls.Add(btnCancelEdit);
            pnlFormCard.Controls.Add(lblFormMessage);
        }

        private void LoadClients()
        {
            string query = txtSearch.Text.Trim();
            _clients = _clientController.GetClientsByInstructor(_instructorUser.UserID, query);

            dgvClients.Columns.Clear();
            dgvClients.AutoGenerateColumns = false;

            dgvClients.Columns.Add(new DataGridViewTextBoxColumn { Name = "ClientID", HeaderText = "ID", DataPropertyName = "ClientID", Visible = false });
            dgvClients.Columns.Add(new DataGridViewTextBoxColumn { Name = "FullName", HeaderText = "Client Full Name", DataPropertyName = "FullName", FillWeight = 150 });
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
                MessageBox.Show($"Active subject switched to: {client.FullName}\r\nHealth assessments and fitness goals are now targeted to this client.",
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
            btnSaveClient.Text = "Save Client Record";
            txtFullName.Clear();
            numAge.Value = 28;
            cmbSex.SelectedIndex = 0;
            numHeight.Value = 175.0m;
            numWeight.Value = 72.0m;
            lblFormMessage.Text = "";
        }
    }
}
