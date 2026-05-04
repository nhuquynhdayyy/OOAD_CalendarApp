using CalendarApp.Database;
using CalendarApp.Forms;

namespace CalendarApp;

static class Program
{
        [STAThread]
        static void Main()
        {
            DatabaseHelper db = new DatabaseHelper();
            if (db.IsConnectionSuccessful())
            {
                Console.WriteLine("SQL OK!");
            }
            else
            {
                Console.WriteLine("SQL Fail!");
            }

            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }