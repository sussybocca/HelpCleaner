using System;
using System.Windows.Forms;
using HelpCleaner.Dashboard;

namespace HelpCleaner
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new DashboardForm());
        }
    }
}
