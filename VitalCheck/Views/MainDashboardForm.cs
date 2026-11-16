using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using VitalCheck.Controllers;
using VitalCheck.Models;

namespace VitalCheck.Views
{
    /// <summary>
    /// MainDashboardForm is the primary application shell.
    /// Provides simple, standard navigation between Dashboard, Assessment, Goals,
    /// Progress, and Client Management views.
    /// </summary>
    public class MainDashboardForm : Form
    {
        // Logged-in user and active client
        private readonly User _currentUser;
        private ClientProfile? _activeClient;

        // Controllers
        private readonly UserController _userController;
        private readonly ClientController _clientController;

        // Top bar controls
        private Panel pnlTopBar;
        private Label lblAppTitle;
        private Label lblAppSub;
        private Label lblUserInfo;
        private Label lblActiveClient;
        private ComboBox? cmbClientPicker;
        private Button btnLogout;

        // Sidebar navigation controls
        private Panel pnlSidebar;
        private Button btnNavDashboard;
        private Button btnNavAssessment;
        private Button btnNavGoals;
        private Button btnNavProgress;
        private Button btnNavClients;

        // Central container for views
        private Panel pnlContent;

        // Currently displayed view name
        private string _currentView = "Dashboard";

        public MainDashboardForm(User user)
        {
            _currentUser = user;
            _userController = new UserController();
            _clientController = new ClientController();

            InitializeActiveClient();
            InitializeComponent();
            NavigateTo("Dashboard");
        }

        /// <summary>
        /// Sets up the active client context.
        /// Individual users are linked to their own profile.
        /// Instructors select from their registered client list.
        /// </summary>
        private void InitializeActiveClient()
        {
            if (!_currentUser.IsInstructor)
            {
                // Individual user: get their own profile
                _activeClient = _userController.GetClientProfileByUserId(_currentUser.UserID);
            }
            else
            {
                // Instructor: get their first client if one exists
                List<ClientProfile> clients = _clientController.GetClientsByInstructor(_currentUser.UserID);
                if (clients.Count > 0)
                {
                    _activeClient = clients[0];
                }
            }
        }

        private void InitializeComponent()
        {
            // Form setup
            this.Text = "VitalCheck - Health & Fitness Diagnostic";
            this.ClientSize = new Size(1100, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(950, 600);
            this.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
            this.BackColor = Color.FromArgb(245, 247, 250);

            // 1. Top Header Bar
            pnlTopBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.FromArgb(15, 23, 42) // Slate Navy
            };

            lblAppTitle = new Label
            {
                Text = "VitalCheck",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 12),
                AutoSize = true
            };

            lblAppSub = new Label
            {
                Text = "Health & Fitness Diagnostic Suite",
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.FromArgb(148, 163, 184),
                Location = new Point(22, 42),
                AutoSize = true
            };

            pnlTopBar.Controls.Add(lblAppTitle);
            pnlTopBar.Controls.Add(lblAppSub);

            // Top Bar Right: User info, client picker, logout
            BuildTopBarRight();

            this.Controls.Add(pnlTopBar);

            // 2. Left Sidebar Navigation
            pnlSidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 210,
                BackColor = Color.FromArgb(30, 41, 59) // Dark Slate
            };

            int y = 20;
            int btnHeight = 44;
            int gap = 8;

            btnNavDashboard = CreateNavButton("Dashboard", y);
            btnNavDashboard.Click += (s, e) => NavigateTo("Dashboard");
            pnlSidebar.Controls.Add(btnNavDashboard);

            y += btnHeight + gap;
            btnNavAssessment = CreateNavButton("Health Assessment", y);
            btnNavAssessment.Click += (s, e) => NavigateTo("Assessment");
            pnlSidebar.Controls.Add(btnNavAssessment);

            y += btnHeight + gap;
            btnNavGoals = CreateNavButton("Fitness Goals", y);
            btnNavGoals.Click += (s, e) => NavigateTo("Goals");
            pnlSidebar.Controls.Add(btnNavGoals);

            y += btnHeight + gap;
            btnNavProgress = CreateNavButton("Progress History", y);
            btnNavProgress.Click += (s, e) => NavigateTo("Progress");
            pnlSidebar.Controls.Add(btnNavProgress);

            // Client Management button is only visible to Fitness Instructors
            if (_currentUser.IsInstructor)
            {
                y += btnHeight + gap;
                btnNavClients = CreateNavButton("Manage Clients", y);
                btnNavClients.Click += (s, e) => NavigateTo("Clients");
                pnlSidebar.Controls.Add(btnNavClients);
            }

            this.Controls.Add(pnlSidebar);

            // 3. Central Content Panel
            pnlContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(245, 247, 250)
            };
            this.Controls.Add(pnlContent);
            pnlContent.BringToFront();
        }

        /// <summary>
        /// Helper to create a consistent navigation button.
        /// </summary>
        private Button CreateNavButton(string text, int top)
        {
            Button btn = new Button
            {
                Text = text,
                Location = new Point(10, top),
                Size = new Size(190, 44),
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(203, 213, 225),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 0, 0),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        /// <summary>
        /// Populates the right side of the top header bar with user identity and client switcher.
        /// </summary>
        private void BuildTopBarRight()
        {
            // Logout button on far right
            btnLogout = new Button
            {
                Text = "Sign Out",
                Size = new Size(90, 32),
                BackColor = Color.FromArgb(51, 65, 85),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnLogout.Location = new Point(this.ClientSize.Width - 110, 19);
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.Click += BtnLogout_Click;
            pnlTopBar.Controls.Add(btnLogout);

            // User identity label
            lblUserInfo = new Label
            {
                Text = $"User: {_currentUser.FullName} ({_currentUser.UserRole})",
                ForeColor = Color.FromArgb(226, 232, 240),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            lblUserInfo.Location = new Point(this.ClientSize.Width - 400, 15);
            pnlTopBar.Controls.Add(lblUserInfo);

            // Active client notice or dropdown
            if (_currentUser.IsInstructor)
            {
                lblActiveClient = new Label
                {
                    Text = "Active Client:",
                    ForeColor = Color.FromArgb(148, 163, 184),
                    Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                    AutoSize = true,
                    Anchor = AnchorStyles.Top | AnchorStyles.Right,
                    Location = new Point(this.ClientSize.Width - 400, 38)
                };
                pnlTopBar.Controls.Add(lblActiveClient);

                cmbClientPicker = new ComboBox
                {
                    Width = 190,
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Font = new Font("Segoe UI", 9F),
                    Anchor = AnchorStyles.Top | AnchorStyles.Right,
                    Location = new Point(this.ClientSize.Width - 310, 34)
                };
                PopulateClientPicker();
                cmbClientPicker.SelectedIndexChanged += CmbClientPicker_SelectedIndexChanged;
                pnlTopBar.Controls.Add(cmbClientPicker);
            }
            else
            {
                lblActiveClient = new Label
                {
                    Text = _activeClient != null ? $"Profile: {_activeClient.FullName} (Age: {_activeClient.Age}, {_activeClient.Sex})" : "Profile: Ready",
                    ForeColor = Color.FromArgb(13, 148, 136), // Teal
                    Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                    AutoSize = true,
                    Anchor = AnchorStyles.Top | AnchorStyles.Right,
                    Location = new Point(this.ClientSize.Width - 400, 38)
                };
                pnlTopBar.Controls.Add(lblActiveClient);
            }

            // Adjust positions when form is resized
            this.Resize += (s, e) => {
                btnLogout.Location = new Point(pnlTopBar.Width - 110, 19);
                lblUserInfo.Location = new Point(pnlTopBar.Width - 400, 15);
                if (lblActiveClient != null) lblActiveClient.Location = new Point(pnlTopBar.Width - 400, 38);
                if (cmbClientPicker != null) cmbClientPicker.Location = new Point(pnlTopBar.Width - 310, 34);
            };
        }

        private void PopulateClientPicker()
        {
            if (cmbClientPicker == null || !_currentUser.IsInstructor) return;

            cmbClientPicker.Items.Clear();
            List<ClientProfile> clients = _clientController.GetClientsByInstructor(_currentUser.UserID);

            int selectedIndex = 0;
            for (int i = 0; i < clients.Count; i++)
            {
                cmbClientPicker.Items.Add(clients[i]);
                if (_activeClient != null && clients[i].ClientID == _activeClient.ClientID)
                {
                    selectedIndex = i;
                }
            }

            if (cmbClientPicker.Items.Count > 0)
            {
                cmbClientPicker.SelectedIndex = selectedIndex;
            }
        }

        private void CmbClientPicker_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cmbClientPicker != null && cmbClientPicker.SelectedItem is ClientProfile selected)
            {
                _activeClient = selected;
                // Refresh current view with newly selected client
                NavigateTo(_currentView);
            }
        }

        /// <summary>
        /// Navigates between views by loading the respective user control into the content area.
        /// </summary>
        public void NavigateTo(string viewName)
        {
            _currentView = viewName;
            HighlightNavButton(viewName);

            // Re-fetch active client if needed
            if (!_currentUser.IsInstructor)
            {
                _activeClient = _userController.GetClientProfileByUserId(_currentUser.UserID);
            }

            pnlContent.Controls.Clear();

            UserControl? nextControl = null;

            switch (viewName)
            {
                case "Dashboard":
                    var dash = new DashboardControl(_currentUser, _activeClient);
                    dash.OnNavigate = (target) => NavigateTo(target);
                    nextControl = dash;
                    break;

                case "Assessment":
                    var assess = new AssessmentControl(_currentUser, _activeClient);
                    assess.OnAssessmentSaved = () => {
                        // Refresh client profile
                        if (!_currentUser.IsInstructor)
                            _activeClient = _userController.GetClientProfileByUserId(_currentUser.UserID);
                    };
                    nextControl = assess;
                    break;

                case "Goals":
                    nextControl = new GoalsControl(_currentUser, _activeClient);
                    break;

                case "Progress":
                    nextControl = new ProgressControl(_currentUser, _activeClient);
                    break;

                case "Clients":
                    if (_currentUser.IsInstructor)
                    {
                        var clientMgmt = new ClientManagementControl(_currentUser);
                        clientMgmt.OnActiveClientChanged = (newClient) => {
                            _activeClient = newClient;
                            PopulateClientPicker();
                        };
                        nextControl = clientMgmt;
                    }
                    break;
            }

            if (nextControl != null)
            {
                nextControl.Dock = DockStyle.Fill;
                pnlContent.Controls.Add(nextControl);
            }
        }

        private void HighlightNavButton(string activeView)
        {
            Button[] buttons = { btnNavDashboard, btnNavAssessment, btnNavGoals, btnNavProgress, btnNavClients };
            string[] names = { "Dashboard", "Assessment", "Goals", "Progress", "Clients" };

            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] == null) continue;

                if (names[i] == activeView)
                {
                    buttons[i].BackColor = Color.FromArgb(13, 148, 136); // Teal highlight
                    buttons[i].ForeColor = Color.White;
                    buttons[i].Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
                }
                else
                {
                    buttons[i].BackColor = Color.Transparent;
                    buttons[i].ForeColor = Color.FromArgb(203, 213, 225);
                    buttons[i].Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
                }
            }
        }

        private void BtnLogout_Click(object? sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Are you sure you want to sign out?",
                "Confirm Sign Out",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                this.Close();
            }
        }
    }
}
