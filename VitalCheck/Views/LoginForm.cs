using System;
using System.Drawing;
using System.Windows.Forms;
using VitalCheck.Controllers;
using VitalCheck.Models;

namespace VitalCheck.Views
{
    /// <summary>
    /// LoginForm provides authentication and user registration.
    /// Features a clean, modern medical/fitness card design with responsive centering,
    /// high-DPI scaling immunity, and clear validation feedback.
    /// </summary>
    public class LoginForm : Form
    {
        private readonly UserController _userController;

        // Container Panels
        private Panel pnlHeader;
        private Label lblBrandIcon;
        private Label lblAppTitle;
        private Label lblAppSubtitle;

        private Panel pnlCenterCard;
        private Panel pnlSegmentedNav;
        private Button btnToggleSignIn;
        private Button btnToggleRegister;

        private Panel pnlLoginView;
        private Panel pnlRegisterView;

        // Login Controls
        private TextBox txtLoginEmail;
        private TextBox txtLoginPassword;
        private Button btnLoginSubmit;
        private Label lblLoginError;

        // Register Controls
        private TextBox txtRegFullName;
        private TextBox txtRegEmail;
        private TextBox txtRegPassword;
        private ComboBox cmbRegRole;
        private Button btnRegisterSubmit;
        private Label lblRegError;

        public LoginForm()
        {
            _userController = new UserController();
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "VitalCheck — Health & Fitness Diagnostic";
            this.ClientSize = new Size(540, 710);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(241, 245, 249); // Clean slate background
            this.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);

            // 1. Top Header Banner (Fully docked and centered for zero text clipping)
            pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 135,
                BackColor = Color.FromArgb(15, 23, 42), // Deep Navy Slate
                Padding = new Padding(20, 20, 20, 10)
            };

            lblBrandIcon = new Label
            {
                Text = "⚡",
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = Color.FromArgb(13, 148, 136), // Vibrant Teal
                Dock = DockStyle.Top,
                Height = 35,
                TextAlign = ContentAlignment.MiddleCenter
            };

            lblAppTitle = new Label
            {
                Text = "VitalCheck",
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = ContentAlignment.MiddleCenter
            };

            lblAppSubtitle = new Label
            {
                Text = "Personal Health & Fitness Diagnostic Platform",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                ForeColor = Color.FromArgb(148, 163, 184),
                Dock = DockStyle.Top,
                Height = 25,
                TextAlign = ContentAlignment.MiddleCenter
            };

            pnlHeader.Controls.Add(lblAppSubtitle);
            pnlHeader.Controls.Add(lblAppTitle);
            pnlHeader.Controls.Add(lblBrandIcon);
            this.Controls.Add(pnlHeader);

            // 2. Center Card Container
            pnlCenterCard = new Panel
            {
                Size = new Size(460, 525),
                Location = new Point(40, 155),
                BackColor = Color.White,
                Padding = new Padding(25, 20, 25, 20)
            };
            this.Controls.Add(pnlCenterCard);

            // Segmented View Switcher (Sign In vs Register)
            pnlSegmentedNav = new Panel
            {
                Dock = DockStyle.Top,
                Height = 44,
                BackColor = Color.FromArgb(241, 245, 249),
                Padding = new Padding(3)
            };

            btnToggleSignIn = new Button
            {
                Text = "Sign In",
                Dock = DockStyle.Left,
                Width = 202,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(15, 23, 42),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnToggleSignIn.FlatAppearance.BorderSize = 0;
            btnToggleSignIn.Click += (s, e) => ShowLoginPanel();

            btnToggleRegister = new Button
            {
                Text = "Create Account",
                Dock = DockStyle.Right,
                Width = 202,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(100, 116, 139),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnToggleRegister.FlatAppearance.BorderSize = 0;
            btnToggleRegister.Click += (s, e) => ShowRegisterPanel();

            pnlSegmentedNav.Controls.Add(btnToggleSignIn);
            pnlSegmentedNav.Controls.Add(btnToggleRegister);
            pnlCenterCard.Controls.Add(pnlSegmentedNav);

            // 3. Build Login View Panel
            pnlLoginView = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(10, 20, 10, 10)
            };
            BuildLoginView();

            // 4. Build Register View Panel
            pnlRegisterView = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(10, 15, 10, 10),
                Visible = false
            };
            BuildRegisterView();

            pnlCenterCard.Controls.Add(pnlRegisterView);
            pnlCenterCard.Controls.Add(pnlLoginView);
            pnlLoginView.BringToFront();
        }

        private void BuildLoginView()
        {
            int curY = 25;

            Label lblEmail = new Label
            {
                Text = "Email Address",
                Location = new Point(15, curY),
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 65, 85)
            };

            curY += 25;
            txtLoginEmail = new TextBox
            {
                Location = new Point(15, curY),
                Width = 380,
                Font = new Font("Segoe UI", 11F),
                PlaceholderText = "name@example.com"
            };

            curY += 45;
            Label lblPassword = new Label
            {
                Text = "Password",
                Location = new Point(15, curY),
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 65, 85)
            };

            curY += 25;
            txtLoginPassword = new TextBox
            {
                Location = new Point(15, curY),
                Width = 380,
                Font = new Font("Segoe UI", 11F),
                UseSystemPasswordChar = true,
                PlaceholderText = "Enter your account password"
            };
            txtLoginPassword.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) BtnLogin_Click(s, e); };

            curY += 50;
            btnLoginSubmit = new Button
            {
                Text = "Sign In to VitalCheck",
                Location = new Point(15, curY),
                Size = new Size(380, 44),
                BackColor = Color.FromArgb(13, 148, 136), // Vibrant Teal
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnLoginSubmit.FlatAppearance.BorderSize = 0;
            btnLoginSubmit.Click += BtnLogin_Click;

            curY += 55;
            lblLoginError = new Label
            {
                Location = new Point(15, curY),
                Size = new Size(380, 45),
                ForeColor = Color.FromArgb(220, 38, 38), // Crimson Red
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                Text = "",
                TextAlign = ContentAlignment.TopCenter
            };

            curY += 50;
            Button btnGoReg = new Button
            {
                Text = "Don't have an account? Register as Individual or Instructor →",
                Location = new Point(15, curY),
                Size = new Size(380, 32),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.FromArgb(2, 132, 199),
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                Cursor = Cursors.Hand
            };
            btnGoReg.FlatAppearance.BorderSize = 0;
            btnGoReg.Click += (s, e) => ShowRegisterPanel();

            pnlLoginView.Controls.Add(lblEmail);
            pnlLoginView.Controls.Add(txtLoginEmail);
            pnlLoginView.Controls.Add(lblPassword);
            pnlLoginView.Controls.Add(txtLoginPassword);
            pnlLoginView.Controls.Add(btnLoginSubmit);
            pnlLoginView.Controls.Add(lblLoginError);
            pnlLoginView.Controls.Add(btnGoReg);
        }

        private void BuildRegisterView()
        {
            int curY = 10;

            Label lblFullName = new Label
            {
                Text = "Full Name",
                Location = new Point(15, curY),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 65, 85)
            };

            curY += 22;
            txtRegFullName = new TextBox
            {
                Location = new Point(15, curY),
                Width = 380,
                Font = new Font("Segoe UI", 10F),
                PlaceholderText = "e.g., Alex Johnson"
            };

            curY += 38;
            Label lblEmail = new Label
            {
                Text = "Email Address",
                Location = new Point(15, curY),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 65, 85)
            };

            curY += 22;
            txtRegEmail = new TextBox
            {
                Location = new Point(15, curY),
                Width = 380,
                Font = new Font("Segoe UI", 10F),
                PlaceholderText = "name@example.com"
            };

            curY += 38;
            Label lblPassword = new Label
            {
                Text = "Password (min 6 characters)",
                Location = new Point(15, curY),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 65, 85)
            };

            curY += 22;
            txtRegPassword = new TextBox
            {
                Location = new Point(15, curY),
                Width = 380,
                Font = new Font("Segoe UI", 10F),
                UseSystemPasswordChar = true
            };

            curY += 38;
            Label lblRole = new Label
            {
                Text = "Account Role",
                Location = new Point(15, curY),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 65, 85)
            };

            curY += 22;
            cmbRegRole = new ComboBox
            {
                Location = new Point(15, curY),
                Width = 380,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F)
            };
            cmbRegRole.Items.Add("Individual — Track personal wellness & goals");
            cmbRegRole.Items.Add("Instructor — Manage clients & conduct diagnostics");
            cmbRegRole.SelectedIndex = 0;

            curY += 45;
            btnRegisterSubmit = new Button
            {
                Text = "Create VitalCheck Account",
                Location = new Point(15, curY),
                Size = new Size(380, 42),
                BackColor = Color.FromArgb(13, 148, 136),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnRegisterSubmit.FlatAppearance.BorderSize = 0;
            btnRegisterSubmit.Click += BtnRegister_Click;

            curY += 48;
            lblRegError = new Label
            {
                Location = new Point(15, curY),
                Size = new Size(380, 35),
                ForeColor = Color.FromArgb(220, 38, 38),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                Text = "",
                TextAlign = ContentAlignment.TopCenter
            };

            curY += 38;
            Button btnGoLogin = new Button
            {
                Text = "Already registered? Switch to Sign In →",
                Location = new Point(15, curY),
                Size = new Size(380, 28),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.FromArgb(2, 132, 199),
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                Cursor = Cursors.Hand
            };
            btnGoLogin.FlatAppearance.BorderSize = 0;
            btnGoLogin.Click += (s, e) => ShowLoginPanel();

            pnlRegisterView.Controls.Add(lblFullName);
            pnlRegisterView.Controls.Add(txtRegFullName);
            pnlRegisterView.Controls.Add(lblEmail);
            pnlRegisterView.Controls.Add(txtRegEmail);
            pnlRegisterView.Controls.Add(lblPassword);
            pnlRegisterView.Controls.Add(txtRegPassword);
            pnlRegisterView.Controls.Add(lblRole);
            pnlRegisterView.Controls.Add(cmbRegRole);
            pnlRegisterView.Controls.Add(btnRegisterSubmit);
            pnlRegisterView.Controls.Add(lblRegError);
            pnlRegisterView.Controls.Add(btnGoLogin);
        }

        private void ShowLoginPanel()
        {
            btnToggleSignIn.BackColor = Color.White;
            btnToggleSignIn.ForeColor = Color.FromArgb(15, 23, 42);
            btnToggleRegister.BackColor = Color.Transparent;
            btnToggleRegister.ForeColor = Color.FromArgb(100, 116, 139);

            pnlRegisterView.Visible = false;
            pnlLoginView.Visible = true;
            pnlLoginView.BringToFront();
            lblLoginError.Text = "";
            txtLoginEmail.Focus();
        }

        private void ShowRegisterPanel()
        {
            btnToggleRegister.BackColor = Color.White;
            btnToggleRegister.ForeColor = Color.FromArgb(15, 23, 42);
            btnToggleSignIn.BackColor = Color.Transparent;
            btnToggleSignIn.ForeColor = Color.FromArgb(100, 116, 139);

            pnlLoginView.Visible = false;
            pnlRegisterView.Visible = true;
            pnlRegisterView.BringToFront();
            lblRegError.Text = "";
            txtRegFullName.Focus();
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
            string roleSelection = cmbRegRole.SelectedItem?.ToString() ?? "";
            string role = roleSelection.StartsWith("Instructor") ? "Instructor" : "Individual";

            bool success = _userController.Register(fullName, email, password, role, out string errorMessage);

            if (success)
            {
                MessageBox.Show("Registration successful! You may now sign in with your email and password.",
                                "VitalCheck Registration", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ShowLoginPanel();
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
