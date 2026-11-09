using System;
using System.Globalization;
using System.Windows.Forms;
using NetHubERP.Forms;

namespace NetHubERP
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.GetCultureInfo("pt-BR");
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("pt-BR");
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FrmLogin());
        }
    }
}
