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
            // === INITIALIZE DASHBOARD ===
            // Try to initialize controllers and set up UI
            // If initialization fails, user sees error message and form doesn't crash
            try
            {
                _currentUser = user;

                // === INITIALIZE CONTROLLERS ===
                // Create instances for managing users and clients
                _userController = new UserController();
                _clientController = new ClientController();

                // === LOAD ACTIVE CLIENT CONTEXT ===
                // Set up the active client/profile for this session
                InitializeActiveClient();

                // === BUILD UI COMPONENTS ===
                // Construct all form visual elements
                InitializeComponent();

                // === NAVIGATE TO INITIAL VIEW ===
                // Show Dashboard as the starting page
                NavigateTo("Dashboard");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error initializing dashboard: {ex.Message}\n\nPlease try logging in again.",
                    "Initialization Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                throw; // Re-throw to allow form to fail gracefully
            }
        }

        /// <summary>
        /// Sets up the active client context.
        /// Individual users are linked to their own profile.
        /// Instructors select from their registered client list.
        /// </summary>
        private void InitializeActiveClient()
        {
            try
            {
                if (!_currentUser.IsInstructor)
                {
                    // === INDIVIDUAL USER CONTEXT ===
                    // Get the user's own profile - may be null if not yet created
                    _activeClient = _userController.GetClientProfileByUserId(_currentUser.UserID);
                }
                else
                {
                    // === INSTRUCTOR USER CONTEXT ===
                    // Get list of clients managed by this instructor
                    List<ClientProfile> clients = _clientController.GetClientsByInstructor(_currentUser.UserID);
                    if (clients != null && clients.Count > 0)
                    {
                        // Set first client as active, may be null if instructor has no clients
                        _activeClient = clients[0];
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but don't crash - form can still work without active client
                System.Diagnostics.Debug.WriteLine($"Error initializing active client: {ex.Message}");
                _activeClient = null; // Ensure null state on error
            }
        }

        private void InitializeComponent()
        {
            // === FORM SETUP ===
            // Configure the main dashboard window: resizable, center screen, light background
            this.Text = "VitalCheck - Health & Fitness Diagnostic";
            this.ClientSize = new Size(1100, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(950, 600);
            this.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
            this.BackColor = Color.FromArgb(245, 247, 250);

            // === TOP HEADER BAR ===
            // Navy-colored bar with application branding and user controls
            // Contains: App title/subtitle, user info, client selector (for instructors), logout button

            pnlTopBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80, // Increased from 70 to 80 for comfortable vertical breathing room
                BackColor = Color.FromArgb(15, 23, 42) // Slate Navy
            };

            // App title - large, bold, white with clear separation from subtitle
            lblAppTitle = new Label
            {
                Text = "VitalCheck",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 14),
                AutoSize = true
            };

            // App subtitle - placed comfortably below title so text never collides
            lblAppSub = new Label
            {
                Text = "Health & Fitness Diagnostic Suite",
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.FromArgb(148, 163, 184),
                Location = new Point(22, 48), // 34px below title top -> no overlap
                AutoSize = true
            };

            pnlTopBar.Controls.Add(lblAppTitle);
            pnlTopBar.Controls.Add(lblAppSub);

            // === TOP BAR RIGHT SECTION ===
            // User info, client picker (instructors only), and logout button
            BuildTopBarRight();

            this.Controls.Add(pnlTopBar);

            // === LEFT SIDEBAR NAVIGATION PANEL ===
            // Dark slate panel with vertical navigation buttons
            // Contains: Dashboard, Health Assessment, Fitness Goals, Progress History, Manage Clients (instructors only)

            pnlSidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 210,
                BackColor = Color.FromArgb(30, 41, 59) // Dark Slate
            };

            // Layout navigation buttons vertically with consistent spacing
            int y = 20;
            int btnHeight = 44;
            int gap = 8;

            // === DASHBOARD NAVIGATION ===
            btnNavDashboard = CreateNavButton("Dashboard", y);
            btnNavDashboard.Click += (s, e) => NavigateTo("Dashboard");
            pnlSidebar.Controls.Add(btnNavDashboard);

            // === ASSESSMENT NAVIGATION ===
            y += btnHeight + gap;
            btnNavAssessment = CreateNavButton("Health Assessment", y);
            btnNavAssessment.Click += (s, e) => NavigateTo("Assessment");
            pnlSidebar.Controls.Add(btnNavAssessment);

            // === GOALS NAVIGATION ===
            y += btnHeight + gap;
            btnNavGoals = CreateNavButton("Fitness Goals", y);
            btnNavGoals.Click += (s, e) => NavigateTo("Goals");
            pnlSidebar.Controls.Add(btnNavGoals);

            // === PROGRESS NAVIGATION ===
            y += btnHeight + gap;
            btnNavProgress = CreateNavButton("Progress History", y);
            btnNavProgress.Click += (s, e) => NavigateTo("Progress");
            pnlSidebar.Controls.Add(btnNavProgress);

            // === CLIENT MANAGEMENT (Instructors Only) ===
            // Only visible to fitness instructors managing multiple clients
            if (_currentUser.IsInstructor)
            {
                y += btnHeight + gap;
                btnNavClients = CreateNavButton("Manage Clients", y);
                btnNavClients.Click += (s, e) => NavigateTo("Clients");
                pnlSidebar.Controls.Add(btnNavClients);
            }

            this.Controls.Add(pnlSidebar);

            // === CENTRAL CONTENT PANEL ===
            // Main viewing area where different views are loaded (Dashboard, Assessment, Goals, etc.)
            // This panel will be filled dynamically based on navigation selection
            // Added padding to prevent content from touching the top bar

            pnlContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(245, 247, 250),
                Padding = new Padding(0, 15, 0, 0) // Top padding to separate from top bar
            };
            this.Controls.Add(pnlContent);
            pnlContent.BringToFront();
        }

        /// <summary>
        /// CreateNavButton - Helper method to create consistent navigation sidebar buttons.
        /// Returns a styled button with left-aligned text, transparent background, and hover effects.
        /// </summary>
        private Button CreateNavButton(string text, int top)
        {
            // Create a flat, transparent button with light gray text
            Button btn = new Button
            {
                Text = text,
                Location = new Point(10, top),
                Size = new Size(190, 44),
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(203, 213, 225), // Light gray text
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleLeft, // Left-align text
                Padding = new Padding(12, 0, 0, 0), // Indent text slightly
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0; // No border for flat appearance
            return btn;
        }

        /// <summary>
        /// BuildTopBarRight - Populates the right side of the top header bar.
        /// Displays: User identity, active client info (and selector for instructors), and logout button.
        /// </summary>
        private void BuildTopBarRight()
        {
            // === LOGOUT BUTTON ===
            // Positioned at far right, anchored so it stays right-aligned when form resizes

            btnLogout = new Button
            {
                Text = "Sign Out",
                Size = new Size(90, 34),
                BackColor = Color.FromArgb(51, 65, 85), // Dark gray
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnLogout.Location = new Point(this.ClientSize.Width - 110, 23);
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.Click += BtnLogout_Click;
            pnlTopBar.Controls.Add(btnLogout);

            // === USER INFORMATION LABEL ===
            // Displays: "User: [Full Name] ([Role])"
            // Example: "User: John Doe (Individual)" or "User: Jane Smith (Instructor)"
            // Positioned right of center, anchored to right side

            lblUserInfo = new Label
            {
                Text = $"User: {_currentUser.FullName} ({_currentUser.UserRole})",
                ForeColor = Color.FromArgb(226, 232, 240), // Light silver
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                AutoSize = true,
                MaximumSize = new Size(280, 0), // Constrain width to prevent overflow
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            lblUserInfo.Location = new Point(this.ClientSize.Width - 410, 18);
            pnlTopBar.Controls.Add(lblUserInfo);

            // === ACTIVE CLIENT INFO (varies by user type) ===
            // For Instructors: Shows dropdown to select active client
            // For Individual Users: Shows their profile info (name, age, sex)

            if (_currentUser.IsInstructor)
            {
                // === INSTRUCTOR: CLIENT SELECTOR ===
                // Label: "Active Client:"
                lblActiveClient = new Label
                {
                    Text = "Active Client:",
                    ForeColor = Color.FromArgb(148, 163, 184),
                    Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                    AutoSize = true,
                    MaximumSize = new Size(150, 0), // Constrain width
                    Anchor = AnchorStyles.Top | AnchorStyles.Right,
                    Location = new Point(this.ClientSize.Width - 410, 46)
                };
                pnlTopBar.Controls.Add(lblActiveClient);

                // Dropdown to select which client to manage
                cmbClientPicker = new ComboBox
                {
                    Width = 190,
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Font = new Font("Segoe UI", 9F),
                    Anchor = AnchorStyles.Top | AnchorStyles.Right,
                    Location = new Point(this.ClientSize.Width - 320, 42)
                };
                PopulateClientPicker();
                cmbClientPicker.SelectedIndexChanged += CmbClientPicker_SelectedIndexChanged;
                pnlTopBar.Controls.Add(cmbClientPicker);
            }
            else
            {
                // === INDIVIDUAL USER: PROFILE INFO ===
                // Displays user's profile info or "Profile: Ready" if no profile yet
                // Format: "Profile: [Full Name] (Age: [Age], [Sex])"

                lblActiveClient = new Label
                {
                    Text = _activeClient != null ? $"Profile: {_activeClient.FullName} (Age: {_activeClient.Age}, {_activeClient.Sex})" : "Profile: Ready",
                    ForeColor = Color.FromArgb(13, 148, 136), // Teal accent
                    Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                    AutoSize = true,
                    MaximumSize = new Size(280, 0), // Constrain width to prevent text clipping
                    Anchor = AnchorStyles.Top | AnchorStyles.Right,
                    Location = new Point(this.ClientSize.Width - 410, 46)
                };
                pnlTopBar.Controls.Add(lblActiveClient);
            }

            // === RESPONSIVE LAYOUT ===
            // Adjust position of right-side controls when form is resized
            // Ensures controls stay right-aligned and don't overlap as window shrinks

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
            // === BUTTON HIGHLIGHTING LOGIC ===
            // Highlight the active navigation button with teal background
            // Skip null buttons (btnNavClients is null for non-instructor users)

            Button[] buttons = { btnNavDashboard, btnNavAssessment, btnNavGoals, btnNavProgress, btnNavClients };
            string[] names = { "Dashboard", "Assessment", "Goals", "Progress", "Clients" };

            for (int i = 0; i < buttons.Length; i++)
            {
                // Skip null buttons (important: btnNavClients is only created for instructors)
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
