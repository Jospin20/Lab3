using System;
using System.Windows.Forms;
using VitalCheck.Data;
using VitalCheck.Views;

namespace VitalCheck
{
    /// <summary>
    /// Program is the main entry point for the VitalCheck desktop application.
    /// It ensures the database is initialized and opens the initial LoginForm.
    /// </summary>
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            // 1. Initialize SQL Server LocalDB and required tables
            try
            {
                DatabaseHelper.InitializeDatabase();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to initialize VitalCheck database:\n\n{ex.Message}\n\nPlease ensure Microsoft SQL Server LocalDB or SQL Server Express is installed and running.",
                    "Database Connection Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }

            // 2. Launch the application with the LoginForm
            Application.Run(new LoginForm());
        }
    }
}
