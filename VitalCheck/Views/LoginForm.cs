using System;
using System.Drawing;
using System.Windows.Forms;
using VitalCheck.Controllers;
using VitalCheck.Models;

namespace VitalCheck.Views
{
    /// <summary>
    /// LoginForm provides authentication and new user registration interfaces.
    /// It communicates directly with UserController to validate credentials,
    /// hash passwords, enforce unique accounts, and route into the MainDashboardForm.
    /// </summary>
    public class LoginForm : Form
    {
        private readonly UserController _userController;

        // Container Panels
        private Panel pnlHeader;
        private Label lblAppTitle;
        private Label lblAppSubtitle;

        private Panel pnlCard;
        private TabControl tabAuth;
        private TabPage tabLogin;
        private TabPage tabRegister;

        // Login Controls
        private Label lblLoginEmail;
        private TextBox txtLoginEmail;
        private Label lblLoginPassword;
        private TextBox txtLoginPassword;
        private Button btnLogin;
        private Label lblLoginError;
        private Button btnGoToRegister;

        // Register Controls
        private Label lblRegFullName;
        private TextBox txtRegFullName;
        private Label lblRegEmail;
        private TextBox txtRegEmail;
        private Label lblRegPassword;
        private TextBox txtRegPassword;
        private Label lblRegRole;
        private ComboBox cmbRegRole;
        private Button btnRegister;
        private Label lblRegError;
        private Button btnGoToLogin;

        public LoginForm()
        {
            _userController = new UserController();
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "VitalCheck: Health & Fitness Diagnostic - Sign In";
            this.Size = new Size(520, 680);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(241, 245, 249); // Clean slate background
            this.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);

            // 1. Top Header Banner
            pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 120,
                BackColor = Color.FromArgb(15, 23, 42) // Dark Navy
            };

            lblAppTitle = new Label
            {
                Text = "VitalCheck",
                Font = new Font("Segoe UI", 22F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(0, 24),
                Width = 520,
                TextAlign = ContentAlignment.MiddleCenter
            };

            lblAppSubtitle = new Label
            {
                Text = "Health & Fitness Diagnostic Monitoring",
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                ForeColor = Color.FromArgb(148, 163, 184),
                Location = new Point(0, 68),
                Width = 520,
                TextAlign = ContentAlignment.MiddleCenter
            };

            pnlHeader.Controls.Add(lblAppTitle);
            pnlHeader.Controls.Add(lblAppSubtitle);
            this.Controls.Add(pnlHeader);

            // 2. Center Card Container
            pnlCard = new Panel
            {
                Location = new Point(40, 140),
                Size = new Size(424, 470),
                BackColor = Color.White,
                Padding = new Padding(15)
            };
            this.Controls.Add(pnlCard);

            // 3. Tab Control for Login vs Register
            tabAuth = new TabControl
            {
                Dock = DockStyle.Fill,
                SizeMode = TabSizeMode.Fixed,
                ItemSize = new Size(195, 36),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };

            tabLogin = new TabPage("Sign In");
            tabRegister = new TabPage("Create Account");

            tabLogin.BackColor = Color.White;
            tabRegister.BackColor = Color.White;

            BuildLoginTab();
            BuildRegisterTab();

            tabAuth.TabPages.Add(tabLogin);
            tabAuth.TabPages.Add(tabRegister);
            pnlCard.Controls.Add(tabAuth);
        }

        private void BuildLoginTab()
        {
            tabLogin.Controls.Clear();

            lblLoginEmail = new Label
            {
                Text = "Email Address",
                Location = new Point(20, 25),
                AutoSize = true,
                ForeColor = Color.FromArgb(51, 65, 85)
            };

            txtLoginEmail = new TextBox
            {
                Location = new Point(20, 52),
                Width = 350,
                Font = new Font("Segoe UI", 11F)
            };

            lblLoginPassword = new Label
            {
                Text = "Password",
                Location = new Point(20, 105),
                AutoSize = true,
                ForeColor = Color.FromArgb(51, 65, 85)
            };

            txtLoginPassword = new TextBox
            {
                Location = new Point(20, 132),
                Width = 350,
                UseSystemPasswordChar = true,
                Font = new Font("Segoe UI", 11F)
            };

            btnLogin = new Button
            {
                Text = "Sign In",
                Location = new Point(20, 190),
                Width = 350,
                Height = 44,
                BackColor = Color.FromArgb(13, 148, 136), // Vibrant Teal
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += BtnLogin_Click;

            lblLoginError = new Label
            {
                Location = new Point(20, 245),
                Width = 350,
                Height = 45,
                ForeColor = Color.FromArgb(220, 38, 38), // Crimson Red
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                Text = "",
                TextAlign = ContentAlignment.TopLeft
            };

            btnGoToRegister = new Button
            {
                Text = "Need an account? Register here",
                Location = new Point(20, 310),
                Width = 350,
                Height = 36,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.FromArgb(2, 132, 199),
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 9F, FontStyle.Underline),
                Cursor = Cursors.Hand
            };
            btnGoToRegister.FlatAppearance.BorderSize = 0;
            btnGoToRegister.Click += (s, e) => { tabAuth.SelectedTab = tabRegister; };

            tabLogin.Controls.Add(lblLoginEmail);
            tabLogin.Controls.Add(txtLoginEmail);
            tabLogin.Controls.Add(lblLoginPassword);
            tabLogin.Controls.Add(txtLoginPassword);
            tabLogin.Controls.Add(btnLogin);
            tabLogin.Controls.Add(lblLoginError);
            tabLogin.Controls.Add(btnGoToRegister);
        }

        private void BuildRegisterTab()
        {
            tabRegister.Controls.Clear();

            lblRegFullName = new Label
            {
                Text = "Full Name",
                Location = new Point(20, 15),
                AutoSize = true,
                ForeColor = Color.FromArgb(51, 65, 85)
            };

            txtRegFullName = new TextBox
            {
                Location = new Point(20, 38),
                Width = 350,
                Font = new Font("Segoe UI", 10F)
            };

            lblRegEmail = new Label
            {
                Text = "Email Address",
                Location = new Point(20, 78),
                AutoSize = true,
                ForeColor = Color.FromArgb(51, 65, 85)
            };

            txtRegEmail = new TextBox
            {
                Location = new Point(20, 101),
                Width = 350,
                Font = new Font("Segoe UI", 10F)
            };

            lblRegPassword = new Label
            {
                Text = "Password (min 6 characters)",
                Location = new Point(20, 141),
                AutoSize = true,
                ForeColor = Color.FromArgb(51, 65, 85)
            };

            txtRegPassword = new TextBox
            {
                Location = new Point(20, 164),
                Width = 350,
                UseSystemPasswordChar = true,
                Font = new Font("Segoe UI", 10F)
            };

            lblRegRole = new Label
            {
                Text = "Account Role",
                Location = new Point(20, 204),
                AutoSize = true,
                ForeColor = Color.FromArgb(51, 65, 85)
            };

            cmbRegRole = new ComboBox
            {
                Location = new Point(20, 227),
                Width = 350,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F)
            };
            cmbRegRole.Items.Add("Individual");
            cmbRegRole.Items.Add("Instructor");
            cmbRegRole.SelectedIndex = 0;

            btnRegister = new Button
            {
                Text = "Create Account",
                Location = new Point(20, 275),
                Width = 350,
                Height = 40,
                BackColor = Color.FromArgb(13, 148, 136),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnRegister.FlatAppearance.BorderSize = 0;
            btnRegister.Click += BtnRegister_Click;

            lblRegError = new Label
            {
                Location = new Point(20, 320),
                Width = 350,
                Height = 40,
                ForeColor = Color.FromArgb(220, 38, 38),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                Text = "",
                TextAlign = ContentAlignment.TopLeft
            };

            btnGoToLogin = new Button
            {
                Text = "Already registered? Sign in here",
                Location = new Point(20, 365),
                Width = 350,
                Height = 32,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.FromArgb(2, 132, 199),
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 9F, FontStyle.Underline),
                Cursor = Cursors.Hand
            };
            btnGoToLogin.FlatAppearance.BorderSize = 0;
            btnGoToLogin.Click += (s, e) => { tabAuth.SelectedTab = tabLogin; };

            tabRegister.Controls.Add(lblRegFullName);
            tabRegister.Controls.Add(txtRegFullName);
            tabRegister.Controls.Add(lblRegEmail);
            tabRegister.Controls.Add(txtRegEmail);
            tabRegister.Controls.Add(lblRegPassword);
            tabRegister.Controls.Add(txtRegPassword);
            tabRegister.Controls.Add(lblRegRole);
            tabRegister.Controls.Add(cmbRegRole);
            tabRegister.Controls.Add(btnRegister);
            tabRegister.Controls.Add(lblRegError);
            tabRegister.Controls.Add(btnGoToLogin);
        }

        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            lblLoginError.Text = "";
            string email = txtLoginEmail.Text.Trim();
            string password = txtLoginPassword.Text;

            bool success = _userController.Login(email, password, out User? user, out string errorMessage);

            if (success && user != null)
            {
                this.Hide();
                MainDashboardForm dashboard = new MainDashboardForm(user);
                dashboard.FormClosed += (s, args) => this.Close();
                dashboard.Show();
            }
            else
            {
                lblLoginError.Text = errorMessage;
            }
        }

        private void BtnRegister_Click(object? sender, EventArgs e)
        {
            lblRegError.Text = "";
            string fullName = txtRegFullName.Text.Trim();
            string email = txtRegEmail.Text.Trim();
            string password = txtRegPassword.Text;
            string role = cmbRegRole.SelectedItem?.ToString() ?? "Individual";

            bool success = _userController.Register(fullName, email, password, role, out string errorMessage);

            if (success)
            {
                MessageBox.Show("Registration successful! You may now sign in with your email and password.",
                                "VitalCheck Registration", MessageBoxButtons.OK, MessageBoxIcon.Information);
                // Switch back to login tab and prefill email
                tabAuth.SelectedTab = tabLogin;
                txtLoginEmail.Text = email;
                txtLoginPassword.Clear();
                txtLoginPassword.Focus();
            }
            else
            {
                lblRegError.Text = errorMessage;
            }
        }
    }
}
