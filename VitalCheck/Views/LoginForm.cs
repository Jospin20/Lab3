using System;
using System.Drawing;
using System.Windows.Forms;
using VitalCheck.Controllers;
using VitalCheck.Models;

namespace VitalCheck.Views
{
    /// <summary>
    /// LoginForm provides straightforward user authentication and registration.
    /// Uses standard Windows Forms controls (TabControl, GroupBoxes, Labels, TextBoxes, Buttons)
    /// for clear, beginner-level desktop development.
    /// </summary>
    public class LoginForm : Form
    {
        // Controller for user operations
        private readonly UserController _userController;

        // Header controls
        private Label lblAppTitle;
        private Label lblAppSubtitle;

        // Tab control to switch between Sign In and Register
        private TabControl tabAuth;
        private TabPage tabSignIn;
        private TabPage tabRegister;

        // Sign In controls
        private GroupBox grpSignIn;
        private Label lblLoginEmail;
        private TextBox txtLoginEmail;
        private Label lblLoginPassword;
        private TextBox txtLoginPassword;
        private Button btnLoginSubmit;
        private Label lblLoginStatus;

        // Register controls
        private GroupBox grpRegister;
        private Label lblRegFullName;
        private TextBox txtRegFullName;
        private Label lblRegEmail;
        private TextBox txtRegEmail;
        private Label lblRegPassword;
        private TextBox txtRegPassword;
        private Label lblRegRole;
        private ComboBox cmbRegRole;
        private Button btnRegisterSubmit;
        private Label lblRegStatus;

        public LoginForm()
        {
            _userController = new UserController();
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // === FORM SETUP ===
            // Configure the login form: fixed dialog, center screen, light background
            this.Text = "VitalCheck - Login";
            this.ClientSize = new Size(480, 580); // Generous height to fit tabs without crowding
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
            this.BackColor = Color.FromArgb(245, 247, 250);

            // === HEADER: APP TITLE AND SUBTITLE ===
            // Display the application branding at the top with teal color scheme
            // Clean vertical separation ensures no overlap regardless of display scaling

            // Main title - large, bold, teal
            lblAppTitle = new Label
            {
                Text = "VitalCheck",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.FromArgb(13, 148, 136), // Teal accent
                Location = new Point(25, 16),
                AutoSize = true
            };

            // Subtitle - descriptive text placed with comfortable clearance below title
            lblAppSubtitle = new Label
            {
                Text = "Health & Fitness Diagnostic System",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                ForeColor = Color.FromArgb(100, 116, 139), // Slate gray
                Location = new Point(27, 56), // Clear separation from title bottom
                AutoSize = true
            };

            this.Controls.Add(lblAppTitle);
            this.Controls.Add(lblAppSubtitle);

            // === AUTHENTICATION TABS ===
            // Tab control with ample vertical clearance below subtitle
            tabAuth = new TabControl
            {
                Location = new Point(25, 96),
                Size = new Size(430, 460)
            };

            // "Sign In" tab - for returning users
            tabSignIn = new TabPage("Sign In");
            tabSignIn.BackColor = Color.White;
            tabSignIn.Padding = new Padding(15);

            // "Register Account" tab - for new users
            tabRegister = new TabPage("Register Account");
            tabRegister.BackColor = Color.White;
            tabRegister.Padding = new Padding(15);

            BuildSignInTab();
            BuildRegisterTab();

            tabAuth.TabPages.Add(tabSignIn);
            tabAuth.TabPages.Add(tabRegister);
            this.Controls.Add(tabAuth);
        }

        /// <summary>
        /// Builds the Sign In tab with a clean GroupBox, TextBoxes, and Login Button.
        /// </summary>
        private void BuildSignInTab()
        {
            // === SIGN IN FORM CONTAINER ===
            // GroupBox to organize login fields and button

            grpSignIn = new GroupBox
            {
                Text = " User Login ",
                Location = new Point(12, 12),
                Size = new Size(398, 395),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59)
            };

            // === EMAIL FIELD ===
            lblLoginEmail = new Label
            {
                Text = "Email Address:",
                Location = new Point(20, 35),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.FromArgb(51, 65, 85)
            };

            txtLoginEmail = new TextBox
            {
                Location = new Point(20, 58),
                Width = 350,
                Font = new Font("Segoe UI", 10F),
                PlaceholderText = "name@example.com"
            };

            // === PASSWORD FIELD ===
            lblLoginPassword = new Label
            {
                Text = "Password:",
                Location = new Point(20, 108),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.FromArgb(51, 65, 85)
            };

            txtLoginPassword = new TextBox
            {
                Location = new Point(20, 132),
                Width = 350,
                Font = new Font("Segoe UI", 10F),
                PasswordChar = '•' // Masked for security
            };
            txtLoginPassword.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) BtnLoginSubmit_Click(s, e); };

            // === SIGN IN BUTTON ===
            btnLoginSubmit = new Button
            {
                Text = "Sign In",
                Location = new Point(20, 192),
                Size = new Size(350, 42),
                BackColor = Color.FromArgb(13, 148, 136), // Teal accent
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnLoginSubmit.FlatAppearance.BorderSize = 0;
            btnLoginSubmit.Click += BtnLoginSubmit_Click;

            // === STATUS MESSAGE AREA ===
            lblLoginStatus = new Label
            {
                Location = new Point(20, 252),
                Size = new Size(350, 50),
                ForeColor = Color.FromArgb(220, 38, 38), // Red for errors
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                TextAlign = ContentAlignment.TopCenter,
                Text = ""
            };

            grpSignIn.Controls.Add(lblLoginEmail);
            grpSignIn.Controls.Add(txtLoginEmail);
            grpSignIn.Controls.Add(lblLoginPassword);
            grpSignIn.Controls.Add(txtLoginPassword);
            grpSignIn.Controls.Add(btnLoginSubmit);
            grpSignIn.Controls.Add(lblLoginStatus);

            tabSignIn.Controls.Add(grpSignIn);
        }

        /// <summary>
        /// Builds the Register tab with standard form fields and Role selection.
        /// Generous vertical spacing prevents any labels from overlapping inputs.
        /// </summary>
        private void BuildRegisterTab()
        {
            // === REGISTRATION FORM CONTAINER ===
            grpRegister = new GroupBox
            {
                Text = " Create New Account ",
                Location = new Point(12, 12),
                Size = new Size(398, 400),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59)
            };

            int curY = 28;
            int rowStep = 64; // Generous step to eliminate any overlap

            // === FULL NAME FIELD ===
            lblRegFullName = new Label
            {
                Text = "Full Name:",
                Location = new Point(20, curY),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.FromArgb(51, 65, 85)
            };
            txtRegFullName = new TextBox
            {
                Location = new Point(20, curY + 20),
                Width = 350,
                Font = new Font("Segoe UI", 9.5F)
            };

            // === EMAIL FIELD ===
            curY += rowStep;
            lblRegEmail = new Label
            {
                Text = "Email Address:",
                Location = new Point(20, curY),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.FromArgb(51, 65, 85)
            };
            txtRegEmail = new TextBox
            {
                Location = new Point(20, curY + 20),
                Width = 350,
                Font = new Font("Segoe UI", 9.5F)
            };

            // === PASSWORD FIELD ===
            curY += rowStep;
            lblRegPassword = new Label
            {
                Text = "Password (minimum 6 characters):",
                Location = new Point(20, curY),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.FromArgb(51, 65, 85)
            };
            txtRegPassword = new TextBox
            {
                Location = new Point(20, curY + 20),
                Width = 350,
                Font = new Font("Segoe UI", 9.5F),
                PasswordChar = '•' // Masked for security
            };

            // === ROLE SELECTION ===
            curY += rowStep;
            lblRegRole = new Label
            {
                Text = "Account Role:",
                Location = new Point(20, curY),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.FromArgb(51, 65, 85)
            };
            cmbRegRole = new ComboBox
            {
                Location = new Point(20, curY + 20),
                Width = 350,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9.5F)
            };
            cmbRegRole.Items.AddRange(new object[] { "Individual User", "Fitness Instructor" });
            cmbRegRole.SelectedIndex = 0; // Default to Individual User

            // === CREATE ACCOUNT BUTTON ===
            curY += rowStep + 4;
            btnRegisterSubmit = new Button
            {
                Text = "Create Account",
                Location = new Point(20, curY),
                Size = new Size(350, 40),
                BackColor = Color.FromArgb(13, 148, 136), // Teal accent
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnRegisterSubmit.FlatAppearance.BorderSize = 0;
            btnRegisterSubmit.Click += BtnRegisterSubmit_Click;

            // === STATUS MESSAGE AREA ===
            curY += 46;
            lblRegStatus = new Label
            {
                Location = new Point(20, curY),
                Size = new Size(350, 35),
                ForeColor = Color.FromArgb(220, 38, 38), // Red for errors
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                TextAlign = ContentAlignment.TopCenter,
                Text = ""
            };

            grpRegister.Controls.Add(lblRegFullName);
            grpRegister.Controls.Add(txtRegFullName);
            grpRegister.Controls.Add(lblRegEmail);
            grpRegister.Controls.Add(txtRegEmail);
            grpRegister.Controls.Add(lblRegPassword);
            grpRegister.Controls.Add(txtRegPassword);
            grpRegister.Controls.Add(lblRegRole);
            grpRegister.Controls.Add(cmbRegRole);
            grpRegister.Controls.Add(btnRegisterSubmit);
            grpRegister.Controls.Add(lblRegStatus);

            tabRegister.Controls.Add(grpRegister);
        }

        /// <summary>
        /// Handles the Sign In button click event.
        /// Validates user input and attempts to authenticate the user.
        /// </summary>
        private void BtnLoginSubmit_Click(object? sender, EventArgs e)
        {
            // Clear any previous error messages
            lblLoginStatus.Text = "";

            // Trim whitespace from email input and get password as-is
            string email = txtLoginEmail.Text.Trim();
            string password = txtLoginPassword.Text;

            // === INPUT VALIDATION ===
            // Check email is provided
            if (string.IsNullOrWhiteSpace(email))
            {
                lblLoginStatus.Text = "Please enter your email address.";
                txtLoginEmail.Focus();
                return;
            }

            // Check password is provided
            if (string.IsNullOrWhiteSpace(password))
            {
                lblLoginStatus.Text = "Please enter your password.";
                txtLoginPassword.Focus();
                return;
            }

            // === AUTHENTICATION ===
            // Call controller to validate credentials against database
            bool success = _userController.Login(email, password, out User? user, out string errorMessage);

            // === LOGIN SUCCESS ===
            if (success && user != null)
            {
                // Clear sensitive password field
                txtLoginPassword.Clear();
                lblLoginStatus.Text = "";

                // === INITIALIZE MAIN DASHBOARD ===
                // Try to create and show the dashboard
                // If dashboard initialization fails, show error without crashing login form
                try
                {
                    // Hide login form and show main dashboard
                    this.Hide();
                    MainDashboardForm dashboard = new MainDashboardForm(user);

                    // When dashboard closes, return to login and clear password
                    dashboard.FormClosed += (s, args) => {
                        this.Show();
                        txtLoginPassword.Clear();
                    };
                    dashboard.Show();
                }
                catch (Exception ex)
                {
                    // Show error and return to login form
                    this.Show();
                    lblLoginStatus.Text = $"Failed to initialize dashboard: {ex.Message}";
                    MessageBox.Show(
                        $"Error initializing dashboard:\n\n{ex.Message}",
                        "Dashboard Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
            // === LOGIN FAILURE ===
            else
            {
                // Display error message from controller
                lblLoginStatus.Text = errorMessage;
            }
        }

        /// <summary>
        /// Handles the Register button click event.
        /// Validates input and creates a new user account.
        /// </summary>
        private void BtnRegisterSubmit_Click(object? sender, EventArgs e)
        {
            // Clear any previous messages
            lblRegStatus.Text = "";

            // Trim whitespace from text inputs, keep password as-is
            string fullName = txtRegFullName.Text.Trim();
            string email = txtRegEmail.Text.Trim();
            string password = txtRegPassword.Text;
            // Determine role: "Instructor" if index 1, else "Individual"
            string role = cmbRegRole.SelectedIndex == 1 ? "Instructor" : "Individual";

            // === INPUT VALIDATION ===
            // Validate full name
            if (string.IsNullOrWhiteSpace(fullName))
            {
                lblRegStatus.ForeColor = Color.FromArgb(220, 38, 38);
                lblRegStatus.Text = "Please enter your full name.";
                txtRegFullName.Focus();
                return;
            }

            // Validate email format (must contain @ and .)
            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@") || !email.Contains("."))
            {
                lblRegStatus.ForeColor = Color.FromArgb(220, 38, 38);
                lblRegStatus.Text = "Please enter a valid email address.";
                txtRegEmail.Focus();
                return;
            }

            // Validate password length (minimum 6 characters)
            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            {
                lblRegStatus.ForeColor = Color.FromArgb(220, 38, 38);
                lblRegStatus.Text = "Password must be at least 6 characters long.";
                txtRegPassword.Focus();
                return;
            }

            // === REGISTRATION ATTEMPT ===
            // Call controller to create new user account in database
            bool success = _userController.Register(fullName, email, password, role, out string errorMessage);

            // === REGISTRATION SUCCESS ===
            if (success)
            {
                // Show success message
                MessageBox.Show(
                    "Account registered successfully!\r\n\r\nYou can now sign in using your email and password.",
                    "Registration Successful",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                // === AUTO-SWITCH TO SIGN IN ===
                // Pre-fill email and clear all other fields
                txtLoginEmail.Text = email;
                txtLoginPassword.Clear();
                txtRegFullName.Clear();
                txtRegEmail.Clear();
                txtRegPassword.Clear();
                lblRegStatus.Text = "";

                // Switch to Sign In tab and focus on password field (email already filled)
                tabAuth.SelectedTab = tabSignIn;
                txtLoginPassword.Focus();
            }
            // === REGISTRATION FAILURE ===
            else
            {
                lblRegStatus.ForeColor = Color.FromArgb(220, 38, 38);
                lblRegStatus.Text = errorMessage;
            }
        }
    }
}
