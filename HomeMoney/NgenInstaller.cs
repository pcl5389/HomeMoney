using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;

namespace HomeMoney
{
    class NgenInstaller
    {
        public enum InstallTypes
        {
            Install,
            Uninstall
        }

        public static void Cmd(string path)
        {
            Process ngenProcess = new Process();

            // 关闭Shell的使用
            ngenProcess.StartInfo.UseShellExecute = true;
            //ngenProcess.StartInfo.Verb = "RunAs";
            /*
            
            ngenProcess.StartInfo.UseShellExecute = false;
            // 重定向标准输入
            ngenProcess.StartInfo.RedirectStandardInput = true;
            // 重定向标准输出
            ngenProcess.StartInfo.RedirectStandardOutput = true;
            //重定向错误输出
            ngenProcess.StartInfo.RedirectStandardError = true;
            */
            // 设置不显示窗口
            ngenProcess.StartInfo.FileName = "cmd.exe";
            ngenProcess.StartInfo.Arguments = "cmd.exe /k cd \"" + path + "\"";
            ngenProcess.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            ngenProcess.Start();
        }

        public void NgenFile(InstallTypes options, string exePath)
        {
            Process ngenProcess = new Process();

            // 关闭Shell的使用
            ngenProcess.StartInfo.UseShellExecute = true;
            ngenProcess.StartInfo.Verb = "RunAs";
            /*
            
            ngenProcess.StartInfo.UseShellExecute = false;
            // 重定向标准输入
            ngenProcess.StartInfo.RedirectStandardInput = true;
            // 重定向标准输出
            ngenProcess.StartInfo.RedirectStandardOutput = true;
            //重定向错误输出
            ngenProcess.StartInfo.RedirectStandardError = true;
            */
            // 设置不显示窗口
            ngenProcess.StartInfo.CreateNoWindow = true;

            ngenProcess.StartInfo.FileName = Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "ngen.exe");
            ngenProcess.StartInfo.Arguments = (options == InstallTypes.Install ? "install" : "uninstall") + " \"" + exePath + "\"";
            ngenProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            ngenProcess.Start();

            // 从输出流获取命令执行结果
            //string strRst = ngenProcess.StandardOutput.ReadToEnd();
            //File.WriteAllText(Application.StartupPath+"/log.txt", strRst);
            ngenProcess.WaitForExit();
        }
    }
}
