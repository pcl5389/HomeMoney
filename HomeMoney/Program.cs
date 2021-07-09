using System;
using System.IO;
using System.Windows.Forms;

namespace HomeMoney
{
    static class Program
    {
        public static Form2 frmMain;
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string path = AppDomain.CurrentDomain.BaseDirectory + "compiled.obj";
            if (!File.Exists(path))
            {
                File.Create(path).Close();
                NgenInstaller install = new NgenInstaller();
                install.NgenFile(NgenInstaller.InstallTypes.Install, Application.ExecutablePath);
            }
            Application.Run(frmMain = new Form2());
        }
    }
}
