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
        /// <summary>
        /// Main entry point for the VitalCheck Desktop Application.
        /// Initializes Windows Forms and ensures database is ready before launching the UI.
        /// Uses STAThread for COM interoperability with Windows Forms.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // === INITIALIZE WINDOWS FORMS ===
            // Set up Windows Forms application with default configuration
            ApplicationConfiguration.Initialize();

            // === DATABASE INITIALIZATION ===
            // Attempt to initialize SQL Server LocalDB and create required tables
            // This ensures the database schema is ready before any data operations
            try
            {
                DatabaseHelper.InitializeDatabase();
            }
            catch (Exception ex)
            {
                // === DATABASE ERROR HANDLING ===
                // Display user-friendly error message if database initialization fails
                // Common causes: SQL Server/LocalDB not installed, missing permissions, port conflicts
                MessageBox.Show(
                    $"Unable to initialize VitalCheck database:\n\n{ex.Message}\n\nPlease ensure Microsoft SQL Server LocalDB or SQL Server Express is installed and running.",
                    "Database Connection Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return; // Exit if database is unavailable
            }

            // === LAUNCH APPLICATION UI ===
            // Start the application with LoginForm as the initial window
            // All subsequent window navigation is driven by user actions
            Application.Run(new LoginForm());
        }
    }
}
