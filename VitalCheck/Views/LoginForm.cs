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
            // Form setup
            this.Text = "VitalCheck - Login";
            this.ClientSize = new Size(480, 520);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
            this.BackColor = Color.FromArgb(245, 247, 250);

            // 1. App Title and Subtitle at top
            lblAppTitle = new Label
            {
                Text = "VitalCheck",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.FromArgb(13, 148, 136), // Teal
                Location = new Point(25, 18),
                AutoSize = true
            };

            lblAppSubtitle = new Label
            {
                Text = "Health & Fitness Diagnostic System",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                ForeColor = Color.FromArgb(100, 116, 139), // Slate gray
                Location = new Point(27, 52),
                AutoSize = true
            };

            this.Controls.Add(lblAppTitle);
            this.Controls.Add(lblAppSubtitle);

            // 2. TabControl for switching between Sign In and Register
            tabAuth = new TabControl
            {
                Location = new Point(25, 85),
                Size = new Size(430, 410)
            };

            tabSignIn = new TabPage("Sign In");
            tabSignIn.BackColor = Color.White;
            tabSignIn.Padding = new Padding(15);

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
            grpSignIn = new GroupBox
            {
                Text = " User Login ",
                Location = new Point(15, 15),
                Size = new Size(390, 335),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59)
            };

            // Email Field
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

            // Password Field
            lblLoginPassword = new Label
            {
                Text = "Password:",
                Location = new Point(20, 100),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.FromArgb(51, 65, 85)
            };

            txtLoginPassword = new TextBox
            {
                Location = new Point(20, 123),
                Width = 350,
                Font = new Font("Segoe UI", 10F),
                PasswordChar = '•'
            };
            txtLoginPassword.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) BtnLoginSubmit_Click(s, e); };

            // Sign In Button
            btnLoginSubmit = new Button
            {
                Text = "Sign In",
                Location = new Point(20, 180),
                Size = new Size(350, 40),
                BackColor = Color.FromArgb(13, 148, 136),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnLoginSubmit.FlatAppearance.BorderSize = 0;
            btnLoginSubmit.Click += BtnLoginSubmit_Click;

            // Status / Error message
            lblLoginStatus = new Label
            {
                Location = new Point(20, 235),
                Size = new Size(350, 60),
                ForeColor = Color.FromArgb(220, 38, 38), // Red
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
        /// </summary>
        private void BuildRegisterTab()
        {
            grpRegister = new GroupBox
            {
                Text = " Create New Account ",
                Location = new Point(15, 15),
                Size = new Size(390, 345),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59)
            };

            int curY = 30;

            // Full Name
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

            // Email
            curY += 52;
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

            // Password
            curY += 52;
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
                PasswordChar = '•'
            };

            // Role
            curY += 52;
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
            cmbRegRole.SelectedIndex = 0;

            // Submit Button
            curY += 56;
            btnRegisterSubmit = new Button
            {
                Text = "Create Account",
                Location = new Point(20, curY),
                Size = new Size(350, 38),
                BackColor = Color.FromArgb(13, 148, 136),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnRegisterSubmit.FlatAppearance.BorderSize = 0;
            btnRegisterSubmit.Click += BtnRegisterSubmit_Click;

            // Status Label
            curY += 44;
            lblRegStatus = new Label
            {
                Location = new Point(20, curY),
                Size = new Size(350, 30),
                ForeColor = Color.FromArgb(220, 38, 38),
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
        /// </summary>
        private void BtnLoginSubmit_Click(object? sender, EventArgs e)
        {
            lblLoginStatus.Text = "";
            string email = txtLoginEmail.Text.Trim();
            string password = txtLoginPassword.Text;

            // Basic validation
            if (string.IsNullOrWhiteSpace(email))
            {
                lblLoginStatus.Text = "Please enter your email address.";
                txtLoginEmail.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                lblLoginStatus.Text = "Please enter your password.";
                txtLoginPassword.Focus();
                return;
            }

            // Attempt login via controller
            bool success = _userController.Login(email, password, out User? user, out string errorMessage);

            if (success && user != null)
            {
                // Clear fields
                txtLoginPassword.Clear();
                lblLoginStatus.Text = "";

                // Open Main Dashboard
                this.Hide();
                MainDashboardForm dashboard = new MainDashboardForm(user);
                dashboard.FormClosed += (s, args) => {
                    this.Show();
                    txtLoginPassword.Clear();
                };
                dashboard.Show();
            }
            else
            {
                lblLoginStatus.Text = errorMessage;
            }
        }

        /// <summary>
        /// Handles the Register button click event.
        /// </summary>
        private void BtnRegisterSubmit_Click(object? sender, EventArgs e)
        {
            lblRegStatus.Text = "";
            string fullName = txtRegFullName.Text.Trim();
            string email = txtRegEmail.Text.Trim();
            string password = txtRegPassword.Text;
            string role = cmbRegRole.SelectedIndex == 1 ? "Instructor" : "Individual";

            // Basic validation
            if (string.IsNullOrWhiteSpace(fullName))
            {
                lblRegStatus.ForeColor = Color.FromArgb(220, 38, 38);
                lblRegStatus.Text = "Please enter your full name.";
                txtRegFullName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@") || !email.Contains("."))
            {
                lblRegStatus.ForeColor = Color.FromArgb(220, 38, 38);
                lblRegStatus.Text = "Please enter a valid email address.";
                txtRegEmail.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            {
                lblRegStatus.ForeColor = Color.FromArgb(220, 38, 38);
                lblRegStatus.Text = "Password must be at least 6 characters long.";
                txtRegPassword.Focus();
                return;
            }

            // Register via controller
            bool success = _userController.Register(fullName, email, password, role, out string errorMessage);

            if (success)
            {
                MessageBox.Show(
                    "Account registered successfully!\r\n\r\nYou can now sign in using your email and password.",
                    "Registration Successful",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                // Switch to Sign In tab and pre-fill email
                txtLoginEmail.Text = email;
                txtLoginPassword.Clear();
                txtRegFullName.Clear();
                txtRegEmail.Clear();
                txtRegPassword.Clear();
                lblRegStatus.Text = "";

                tabAuth.SelectedTab = tabSignIn;
                txtLoginPassword.Focus();
            }
            else
            {
                lblRegStatus.ForeColor = Color.FromArgb(220, 38, 38);
                lblRegStatus.Text = errorMessage;
            }
        }
    }
}
