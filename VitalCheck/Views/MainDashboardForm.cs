using System;
using System.Drawing;
using System.Windows.Forms;
using VitalCheck.Controllers;
using VitalCheck.Models;

namespace VitalCheck.Views
{
    /// <summary>
    /// MainDashboardForm serves as the primary application shell.
    /// Hosts the modern responsive sidebar navigation, role-based controls,
    /// active client context switcher, and dynamically docks content views.
    /// Engineered with zero-overlap and DPI-safe layout containers.
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

        // Sidebar Branding & User Profile
        private Panel pnlBrand;
        private TableLayoutPanel pnlBrandTable;
        private Label lblLogoBadge;
        private Label lblBrand;
        private Label lblBrandSub;

        private Panel pnlUserCardWrapper;
        private Panel pnlUserCard;
        private Label lblUserName;
        private Label lblUserRole;

        // Navigation Buttons
        private Panel pnlNavContainer;
        private Button btnNavDashboard;
        private Button btnNavAssessment;
        private Button btnNavGoals;
        private Button btnNavProgress;
        private Button btnNavClients;
        private Button btnNavLogout;

        // Top Bar Controls
        private Label lblPageTitle;
        private FlowLayoutPanel pnlTopRight;
        private Label lblActiveClientNotice;
        private ComboBox? cmbClientPicker;

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
                // Fitness Instructor: Select their first client if available
                var clients = _clientController.GetClientsByInstructor(_currentUser.UserID);
                if (clients.Count > 0)
                {
                    _activeClient = clients[0];
                }
            }
        }

        private void InitializeComponent()
        {
            this.Text = "VitalCheck — Health & Fitness Diagnostic Suite";
            this.ClientSize = new Size(1320, 840);
            this.MinimumSize = new Size(1100, 720);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(248, 250, 252);
            this.Font = new Font("Segoe UI", 10F);

            // 1. Sidebar Panel (Left)
            pnlSidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 270,
                BackColor = Color.FromArgb(15, 23, 42), // Deep Slate Navy
                Padding = new Padding(0)
            };
            this.Controls.Add(pnlSidebar);

            // Brand Header in Sidebar (Separated Badge + Title for Absolute Rendering Stability)
            pnlBrand = new Panel
            {
                Dock = DockStyle.Top,
                Height = 105,
                BackColor = Color.FromArgb(15, 23, 42),
                Padding = new Padding(20, 20, 16, 10)
            };

            pnlBrandTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                BackColor = Color.Transparent
            };
            pnlBrandTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 44F)); // Logo badge column
            pnlBrandTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));  // Brand text column
            pnlBrandTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));            // VitalCheck Title
            pnlBrandTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));            // Subtitle

            lblLogoBadge = new Label
            {
                Text = "VC",
                Size = new Size(36, 36),
                BackColor = Color.FromArgb(13, 148, 136), // Vibrant Teal
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11.5F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(0, 3, 8, 0)
            };
            pnlBrandTable.SetRowSpan(lblLogoBadge, 2);

            lblBrand = new Label
            {
                Text = "VitalCheck",
                Font = new Font("Segoe UI", 17F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 2)
            };

            lblBrandSub = new Label
            {
                Text = "Health & Fitness Diagnostic",
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(148, 163, 184),
                AutoSize = true,
                Margin = new Padding(0)
            };

            pnlBrandTable.Controls.Add(lblLogoBadge, 0, 0);
            pnlBrandTable.Controls.Add(lblBrand, 1, 0);
            pnlBrandTable.Controls.Add(lblBrandSub, 1, 1);
            pnlBrand.Controls.Add(pnlBrandTable);
            pnlSidebar.Controls.Add(pnlBrand);

            // User Profile Card Wrapper
            pnlUserCardWrapper = new Panel
            {
                Dock = DockStyle.Top,
                Height = 85,
                BackColor = Color.FromArgb(15, 23, 42),
                Padding = new Padding(16, 5, 16, 12)
            };

            pnlUserCard = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(30, 41, 59),
                Padding = new Padding(14, 10, 14, 10)
            };

            lblUserName = new Label
            {
                Text = _currentUser.FullName,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Top,
                Height = 22,
                AutoEllipsis = true
            };

            lblUserRole = new Label
            {
                Text = _currentUser.IsInstructor ? "● Fitness Instructor" : "● Personal Account",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = _currentUser.IsInstructor ? Color.FromArgb(45, 212, 191) : Color.FromArgb(52, 211, 153),
                Dock = DockStyle.Top,
                Height = 20
            };

            pnlUserCard.Controls.Add(lblUserRole);
            pnlUserCard.Controls.Add(lblUserName);
            pnlUserCardWrapper.Controls.Add(pnlUserCard);
            pnlSidebar.Controls.Add(pnlUserCardWrapper);

            // Navigation Container (Flow layout for perfect button spacing)
            pnlNavContainer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(15, 23, 42),
                Padding = new Padding(14, 10, 14, 10),
                AutoScroll = true
            };

            int navY = 10;
            int btnH = 46;
            int btnGap = 8;

            btnNavDashboard = CreateNavButton("📊  Dashboard Overview", navY);
            btnNavDashboard.Click += (s, e) => NavigateTo("Dashboard");
            pnlNavContainer.Controls.Add(btnNavDashboard);

            navY += btnH + btnGap;
            btnNavAssessment = CreateNavButton("📋  Health Assessment", navY);
            btnNavAssessment.Click += (s, e) => NavigateTo("Assessment");
            pnlNavContainer.Controls.Add(btnNavAssessment);

            navY += btnH + btnGap;
            btnNavGoals = CreateNavButton("🎯  Fitness Goals", navY);
            btnNavGoals.Click += (s, e) => NavigateTo("Goals");
            pnlNavContainer.Controls.Add(btnNavGoals);

            navY += btnH + btnGap;
            btnNavProgress = CreateNavButton("📈  Progress Tracking", navY);
            btnNavProgress.Click += (s, e) => NavigateTo("Progress");
            pnlNavContainer.Controls.Add(btnNavProgress);

            if (_currentUser.IsInstructor)
            {
                navY += btnH + btnGap;
                btnNavClients = CreateNavButton("👥  Client Directory", navY);
                btnNavClients.Click += (s, e) => NavigateTo("Clients");
                pnlNavContainer.Controls.Add(btnNavClients);
            }

            pnlSidebar.Controls.Add(pnlNavContainer);

            // Logout Button at bottom of sidebar
            btnNavLogout = new Button
            {
                Text = "🚪  Sign Out",
                Dock = DockStyle.Bottom,
                Height = 54,
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
                Padding = new Padding(24, 12, 24, 12)
            };
            this.Controls.Add(pnlTopBar);

            lblPageTitle = new Label
            {
                Text = "Dashboard Overview",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Dock = DockStyle.Left,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize = true
            };
            pnlTopBar.Controls.Add(lblPageTitle);

            // Right side of top bar: Active Subject Status & Dropdown
            pnlTopRight = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                Padding = new Padding(0, 6, 0, 0)
            };

            if (_currentUser.IsInstructor)
            {
                Label lblActivePrefix = new Label
                {
                    Text = "Active Client:",
                    Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(71, 85, 105),
                    AutoSize = true,
                    Margin = new Padding(0, 6, 8, 0)
                };

                cmbClientPicker = new ComboBox
                {
                    Width = 280,
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Font = new Font("Segoe UI", 9.5F)
                };
                PopulateInstructorClientPicker();
                cmbClientPicker.SelectedIndexChanged += CmbClientPicker_SelectedIndexChanged;

                pnlTopRight.Controls.Add(lblActivePrefix);
                pnlTopRight.Controls.Add(cmbClientPicker);
            }
            else
            {
                lblActiveClientNotice = new Label
                {
                    Text = _activeClient != null ? $"Active Subject: {_activeClient.FullName} (Age: {_activeClient.Age}, {_activeClient.Sex})" : "Active Subject: Loading...",
                    Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(13, 148, 136),
                    AutoSize = true,
                    Margin = new Padding(0, 6, 0, 0)
                };
                pnlTopRight.Controls.Add(lblActiveClientNotice);
            }
            pnlTopBar.Controls.Add(pnlTopRight);

            // 3. Central Content Panel
            pnlContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(248, 250, 252),
                AutoScroll = true
            };
            this.Controls.Add(pnlContent);
            pnlContent.BringToFront();
        }

        private Button CreateNavButton(string title, int yPos)
        {
            Button btn = new Button
            {
                Text = title,
                Location = new Point(0, yPos),
                Size = new Size(242, 46),
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(203, 213, 225),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(14, 0, 0, 0),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private void HighlightNavButton(Button? activeButton)
        {
            Button[] buttons = { btnNavDashboard, btnNavAssessment, btnNavGoals, btnNavProgress, btnNavClients };
            foreach (var b in buttons)
            {
                if (b == null) continue;
                if (b == activeButton)
                {
                    b.BackColor = Color.FromArgb(13, 148, 136); // Teal highlight
                    b.ForeColor = Color.White;
                    b.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
                }
                else
                {
                    b.BackColor = Color.Transparent;
                    b.ForeColor = Color.FromArgb(203, 213, 225);
                    b.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
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
                    dash.Dock = DockStyle.Fill;
                    pnlContent.Controls.Add(dash);
                    break;

                case "Assessment":
                    lblPageTitle.Text = "Health & Fitness Diagnostic Assessment";
                    HighlightNavButton(btnNavAssessment);
                    AssessmentControl assess = new AssessmentControl(_currentUser, _activeClient);
                    assess.OnAssessmentSaved = () => {
                        if (_currentUser.IsInstructor) PopulateInstructorClientPicker();
                    };
                    assess.Dock = DockStyle.Fill;
                    pnlContent.Controls.Add(assess);
                    break;

                case "Goals":
                    lblPageTitle.Text = "Fitness Goals & Milestones";
                    HighlightNavButton(btnNavGoals);
                    GoalsControl goals = new GoalsControl(_currentUser, _activeClient);
                    goals.Dock = DockStyle.Fill;
                    pnlContent.Controls.Add(goals);
                    break;

                case "Progress":
                    lblPageTitle.Text = "Progress Analytics & Historical Records";
                    HighlightNavButton(btnNavProgress);
                    ProgressControl prog = new ProgressControl(_currentUser, _activeClient);
                    prog.Dock = DockStyle.Fill;
                    pnlContent.Controls.Add(prog);
                    break;

                case "Clients":
                    lblPageTitle.Text = "Client Directory & Management";
                    HighlightNavButton(btnNavClients);
                    ClientManagementControl clientView = new ClientManagementControl(_currentUser);
                    clientView.OnActiveClientChanged = (newClient) => {
                        _activeClient = newClient;
                        PopulateInstructorClientPicker();
                    };
                    clientView.Dock = DockStyle.Fill;
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
                    NavigateTo(_currentViewName);
                }
            }
        }

        private void BtnNavLogout_Click(object? sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("Are you sure you want to sign out of VitalCheck?", "Confirm Sign Out", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr == DialogResult.Yes)
            {
                this.Hide();
                LoginForm login = new LoginForm();
                login.Show();
            }
        }
    }
}
