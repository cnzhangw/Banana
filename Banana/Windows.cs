using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banana
{
    public sealed class Windows
    {

        public static void CMDProcess(Action<string> outputDataReceived, Action<string> errorDataReceived, Action<Exception> onException, params string[] commands)
        {
            if (commands.Length == 0)
            {
                return;
            }

            Process process = new Process();
            try
            {
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;

                process.OutputDataReceived += ((object sender, DataReceivedEventArgs e) =>
                {
                    string value = e.Data;
                    if (value.HasValue())
                    {
                        if (value.ToLower().Contains("microsoft"))
                        {
                            return;
                        }
                        if (value.Contains(">"))
                        {
                            value = value.Substring(value.IndexOf(">"));
                            return;
                        }

                        outputDataReceived?.Invoke(value);
                    }
                });
                process.ErrorDataReceived += ((object sender, DataReceivedEventArgs e) =>
                {
                    string value = e.Data;
                    if (value.HasValue())
                    {
                        if (value.Equals("error:", StringComparison.OrdinalIgnoreCase))
                        {
                            return;
                        }
                        if (value.ToLower().Contains("microsoft"))
                        {
                            return;
                        }

                        errorDataReceived?.Invoke(value);
                    }
                });

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                //bool hasExit = false;
                process.StandardInput.AutoFlush = true;
                foreach (string item in commands)
                {
                    //hasExit = item.ToLower().EndsWith("exit", StringComparison.OrdinalIgnoreCase);
                    process.StandardInput.WriteLine(item);
                }
                process.StandardInput.Close();
                process.StandardInput.Dispose();

                //if (!hasExit)
                //{
                //    process.CancelOutputRead();
                //    process.CancelErrorRead();
                //    process.StandardInput.WriteLine("exit");
                //}
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                if (onException == null)
                {
                    throw ex;
                }
                else
                {
                    onException.Invoke(ex);
                }
            }
            finally
            {
                process.Close();
                process.Dispose();
            }
        }

        public static void CMDProcess(Action<string> outputDataReceived, Action<string> errorDataReceived, params string[] commands)
        {
            CMDProcess(outputDataReceived, errorDataReceived, null, commands);
        }

        public static void CMDProcess(Action<string> outputDataReceived, params string[] commands)
        {
            CMDProcess(outputDataReceived, null, commands);
        }

        public static void CMDProcess(params string[] commands)
        {
            CMDProcess(null, commands);
        }


#if !NETSTANDARD2_0

        /// <summary>
        /// 根据软件名获取安装位置
        /// </summary>
        /// <param name="name">如：chrome、360safe、360zip</param>
        /// <returns>返回安装路径，为空时表示未找到</returns>
        public static string FindInstallPath(string name)
        {
            string ret = string.Empty;
            try
            {
                string key = string.Empty;
                string path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\";
                Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(path + name + ".exe", false);
                object result = regKey.GetValue(key);
                Microsoft.Win32.RegistryValueKind valueKind = regKey.GetValueKind(key);
                if (valueKind == Microsoft.Win32.RegistryValueKind.String)
                {
                    ret = result.ToString();
                }
            }
            catch { }
            return ret;
        }

#endif

    }
}
