using System;
using System.Drawing;
using System.Windows.Forms;
using VitalCheck.Controllers;
using VitalCheck.Models;

namespace VitalCheck.Views
{
    /// <summary>
    /// MainDashboardForm serves as the primary application shell.
    /// Hosts the modern sidebar navigation, role-based controls, active client context,
    /// and seamlessly swaps content views (UserControls) in the main display panel.
    /// </summary>
    public class MainDashboardForm : Form
    {
        private readonly User _currentUser;
        private ClientProfile? _activeClient;
        private readonly UserController _userController;
        private readonly ClientController _clientController;

        // Shell Layout Panels
        private Panel pnlSidebar;
        private Panel pnlTopBar;
        private Panel pnlContent;

        // Sidebar Controls
        private Label lblBrand;
        private Label lblBrandSub;
        private Panel pnlUserBadge;
        private Label lblUserName;
        private Label lblUserRole;

        // Navigation Buttons
        private Button btnNavDashboard;
        private Button btnNavAssessment;
        private Button btnNavGoals;
        private Button btnNavProgress;
        private Button btnNavClients;
        private Button btnNavLogout;

        // Top Bar Controls
        private Label lblPageTitle;
        private Label lblActiveClientNotice;
        private ComboBox? cmbClientPicker; // For instructors to quickly switch clients

        private string _currentViewName = "Dashboard";

        public MainDashboardForm(User user)
        {
            _currentUser = user;
            _userController = new UserController();
            _clientController = new ClientController();

            InitializeActiveClient();
            InitializeComponent();
            NavigateTo("Dashboard");
        }

        private void InitializeActiveClient()
        {
            if (!_currentUser.IsInstructor)
            {
                // Individual User: Retrieve their own linked ClientProfile
                _activeClient = _userController.GetClientProfileByUserId(_currentUser.UserID);
            }
            else
            {
                // Fitness Instructor: Pick their first client if available
                var clients = _clientController.GetClientsByInstructor(_currentUser.UserID);
                if (clients.Count > 0)
                {
                    _activeClient = clients[0];
                }
            }
        }

        private void InitializeComponent()
        {
            this.Text = "VitalCheck: Health & Fitness Diagnostic";
            this.Size = new Size(1280, 800);
            this.MinimumSize = new Size(1100, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(248, 250, 252);
            this.Font = new Font("Segoe UI", 10F);

            // 1. Sidebar Panel (Left)
            pnlSidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 240,
                BackColor = Color.FromArgb(15, 23, 42), // Deep Navy Slate
                Padding = new Padding(0)
            };
            this.Controls.Add(pnlSidebar);

            // Brand Header
            lblBrand = new Label
            {
                Text = "VitalCheck",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 20),
                AutoSize = true
            };
            lblBrandSub = new Label
            {
                Text = "Health & Fitness Diagnostic",
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(148, 163, 184),
                Location = new Point(22, 54),
                AutoSize = true
            };
            pnlSidebar.Controls.Add(lblBrand);
            pnlSidebar.Controls.Add(lblBrandSub);

            // User Badge
            pnlUserBadge = new Panel
            {
                Location = new Point(15, 85),
                Size = new Size(210, 65),
                BackColor = Color.FromArgb(30, 41, 59),
                Padding = new Padding(10)
            };
            lblUserName = new Label
            {
                Text = _currentUser.FullName,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(10, 10),
                Width = 190,
                AutoEllipsis = true
            };
            lblUserRole = new Label
            {
                Text = _currentUser.IsInstructor ? "Fitness Instructor" : "Individual Account",
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(13, 148, 136), // Vibrant Teal
                Location = new Point(10, 34),
                AutoSize = true
            };
            pnlUserBadge.Controls.Add(lblUserName);
            pnlUserBadge.Controls.Add(lblUserRole);
            pnlSidebar.Controls.Add(pnlUserBadge);

            // Navigation Buttons
            int btnY = 170;
            int btnHeight = 44;
            int btnSpacing = 8;

            btnNavDashboard = CreateNavButton("📊  Dashboard", btnY);
            btnNavDashboard.Click += (s, e) => NavigateTo("Dashboard");
            pnlSidebar.Controls.Add(btnNavDashboard);

            btnY += btnHeight + btnSpacing;
            btnNavAssessment = CreateNavButton("📋  Health Assessment", btnY);
            btnNavAssessment.Click += (s, e) => NavigateTo("Assessment");
            pnlSidebar.Controls.Add(btnNavAssessment);

            btnY += btnHeight + btnSpacing;
            btnNavGoals = CreateNavButton("🎯  Fitness Goals", btnY);
            btnNavGoals.Click += (s, e) => NavigateTo("Goals");
            pnlSidebar.Controls.Add(btnNavGoals);

            btnY += btnHeight + btnSpacing;
            btnNavProgress = CreateNavButton("📈  Progress History", btnY);
            btnNavProgress.Click += (s, e) => NavigateTo("Progress");
            pnlSidebar.Controls.Add(btnNavProgress);

            // Client Management - visible only for Instructors
            btnY += btnHeight + btnSpacing;
            btnNavClients = CreateNavButton("👥  Client Directory", btnY);
            btnNavClients.Visible = _currentUser.IsInstructor;
            btnNavClients.Click += (s, e) => NavigateTo("Clients");
            pnlSidebar.Controls.Add(btnNavClients);

            // Logout Button at bottom of sidebar
            btnNavLogout = new Button
            {
                Text = "🚪  Sign Out",
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = Color.FromArgb(30, 41, 59),
                ForeColor = Color.FromArgb(248, 113, 113), // Soft Red
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnNavLogout.FlatAppearance.BorderSize = 0;
            btnNavLogout.Click += BtnNavLogout_Click;
            pnlSidebar.Controls.Add(btnNavLogout);

            // 2. Top Header Bar
            pnlTopBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 65,
                BackColor = Color.White,
                Padding = new Padding(25, 12, 25, 12)
            };
            this.Controls.Add(pnlTopBar);

            lblPageTitle = new Label
            {
                Text = "Dashboard",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(20, 18),
                AutoSize = true
            };
            pnlTopBar.Controls.Add(lblPageTitle);

            // Active Subject Status / Quick Dropdown on Top Bar
            if (_currentUser.IsInstructor)
            {
                Label lblActivePrefix = new Label
                {
                    Text = "Active Client:",
                    Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(71, 85, 105),
                    Location = new Point(580, 22),
                    AutoSize = true
                };

                cmbClientPicker = new ComboBox
                {
                    Location = new Point(670, 18),
                    Width = 260,
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Font = new Font("Segoe UI", 9.5F)
                };
                PopulateInstructorClientPicker();
                cmbClientPicker.SelectedIndexChanged += CmbClientPicker_SelectedIndexChanged;

                pnlTopBar.Controls.Add(lblActivePrefix);
                pnlTopBar.Controls.Add(cmbClientPicker);
            }
            else
            {
                lblActiveClientNotice = new Label
                {
                    Text = _activeClient != null ? $"Profile: {_activeClient.FullName} (Age: {_activeClient.Age}, {_activeClient.Sex})" : "Profile: Initializing...",
                    Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(13, 148, 136),
                    Location = new Point(620, 22),
                    AutoSize = true
                };
                pnlTopBar.Controls.Add(lblActiveClientNotice);
            }

            // 3. Central Content Panel
            pnlContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(248, 250, 252)
            };
            this.Controls.Add(pnlContent);
            pnlContent.BringToFront();
        }

        private Button CreateNavButton(string title, int yPos)
        {
            Button btn = new Button
            {
                Text = title,
                Location = new Point(12, yPos),
                Size = new Size(216, 44),
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(203, 213, 225),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 0, 0),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private void HighlightNavButton(Button activeButton)
        {
            Button[] buttons = { btnNavDashboard, btnNavAssessment, btnNavGoals, btnNavProgress, btnNavClients };
            foreach (var b in buttons)
            {
                if (b == activeButton)
                {
                    b.BackColor = Color.FromArgb(13, 148, 136); // Teal highlight
                    b.ForeColor = Color.White;
                    b.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
                }
                else
                {
                    b.BackColor = Color.Transparent;
                    b.ForeColor = Color.FromArgb(203, 213, 225);
                    b.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
                }
            }
        }

        public void NavigateTo(string viewName)
        {
            _currentViewName = viewName;
            pnlContent.Controls.Clear();

            switch (viewName)
            {
                case "Dashboard":
                    lblPageTitle.Text = "System Dashboard & Overview";
                    HighlightNavButton(btnNavDashboard);
                    DashboardControl dash = new DashboardControl(_currentUser, _activeClient);
                    dash.OnNavigateRequested = (target) => NavigateTo(target);
                    pnlContent.Controls.Add(dash);
                    break;

                case "Assessment":
                    lblPageTitle.Text = "Health & Fitness Assessment";
                    HighlightNavButton(btnNavAssessment);
                    AssessmentControl assess = new AssessmentControl(_currentUser, _activeClient);
                    assess.OnAssessmentSaved = () => {
                        // Refresh client picker display if weight/height updated
                        if (_currentUser.IsInstructor) PopulateInstructorClientPicker();
                    };
                    pnlContent.Controls.Add(assess);
                    break;

                case "Goals":
                    lblPageTitle.Text = "Fitness Goals & Milestones";
                    HighlightNavButton(btnNavGoals);
                    GoalsControl goals = new GoalsControl(_currentUser, _activeClient);
                    pnlContent.Controls.Add(goals);
                    break;

                case "Progress":
                    lblPageTitle.Text = "Progress History & Diagnostics";
                    HighlightNavButton(btnNavProgress);
                    ProgressControl prog = new ProgressControl(_currentUser, _activeClient);
                    pnlContent.Controls.Add(prog);
                    break;

                case "Clients":
                    lblPageTitle.Text = "Client Management Directory";
                    HighlightNavButton(btnNavClients);
                    ClientManagementControl clientView = new ClientManagementControl(_currentUser);
                    clientView.OnActiveClientChanged = (newClient) => {
                        _activeClient = newClient;
                        PopulateInstructorClientPicker();
                    };
                    pnlContent.Controls.Add(clientView);
                    break;
            }
        }

        private void PopulateInstructorClientPicker()
        {
            if (cmbClientPicker == null) return;

            cmbClientPicker.Items.Clear();
            var clients = _clientController.GetClientsByInstructor(_currentUser.UserID);

            int selectedIdx = -1;
            for (int i = 0; i < clients.Count; i++)
            {
                cmbClientPicker.Items.Add(clients[i]);
                if (_activeClient != null && clients[i].ClientID == _activeClient.ClientID)
                {
                    selectedIdx = i;
                }
            }

            if (selectedIdx >= 0)
            {
                cmbClientPicker.SelectedIndex = selectedIdx;
            }
            else if (clients.Count > 0)
            {
                cmbClientPicker.SelectedIndex = 0;
                _activeClient = clients[0];
            }
        }

        private void CmbClientPicker_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cmbClientPicker?.SelectedItem is ClientProfile selected)
            {
                if (_activeClient?.ClientID != selected.ClientID)
                {
                    _activeClient = selected;
                    // Refresh current view to display newly selected client's data
                    NavigateTo(_currentViewName);
                }
            }
        }

        private void BtnNavLogout_Click(object? sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("Are you sure you want to sign out?", "Confirm Sign Out", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr == DialogResult.Yes)
            {
                this.Hide();
                LoginForm login = new LoginForm();
                login.Show();
            }
        }
    }
}
