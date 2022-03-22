using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Deployment.Application;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FiremonSystray.Utils
{
    public static class HelperFunctions
    {
        private static NotificationWindow notificationWindow;

        public static Process ChromeProcess { get; set; }
        public static string FiremonMonitorUrl { get; set; }
        #region Autostart
        public static void AddAutoStartRegistry(string appName, string appPath)
        {
            string run = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";

            if (Path.HasExtension(appPath))
            {
                RegistryKey startKey = Registry.CurrentUser.OpenSubKey(run, true);
                startKey.SetValue(appName, appPath);
            }
        }

        public static void RemoveAutoStartRegistry(string appName)
        {
            string run = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
            RegistryKey startKey = Registry.CurrentUser.OpenSubKey(run, true);
            startKey.DeleteValue(appName, false);
        }

        public static bool IsAutoStartEnabled(string appName)
        {
            string run = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
            RegistryKey startKey = Registry.CurrentUser.OpenSubKey(run, true);
            var names = startKey.GetValueNames();
            if (names.Contains(appName))
                return true;

            return false;
        }

        public static bool OpenFiremonMonitor(bool showReopenDialog = true, bool useInternalBrowser = true)
        {
            if (useInternalBrowser)
            {
                Webbrowser webbrowser = new Webbrowser();
                webbrowser.webView.Source = new Uri(FiremonMonitorUrl);
                webbrowser.Show();
                webbrowser.Focus();

                return true;
            }


            string chrome_exe_path = "";

            string chrome_x64 = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths";
            RegistryKey c64_key = Registry.LocalMachine.OpenSubKey(chrome_x64);
            if (c64_key.GetSubKeyNames().Contains("chrome.exe"))
            {
                if (c64_key.OpenSubKey("chrome.exe").GetValueNames().Contains("Path"))
                {
                    chrome_exe_path = Path.Combine(c64_key.OpenSubKey("chrome.exe").GetValue("Path").ToString(), "chrome.exe");
                }
            }

            if (string.IsNullOrWhiteSpace(chrome_exe_path))
            {
                string chrome_x86 = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths";
                RegistryKey c86_key = Registry.LocalMachine.OpenSubKey(chrome_x86);
                if (c86_key.GetSubKeyNames().Contains("chrome.exe"))
                {
                    if (c86_key.OpenSubKey("chrome.exe").GetValueNames().Contains("Path"))
                    {
                        chrome_exe_path = Path.Combine(c86_key.OpenSubKey("chrome.exe").GetValue("Path").ToString());
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(chrome_exe_path))
            {
                return false;
            }

            try
            {

                HelperFunctions.ChromeProcess = new Process
                {
                    StartInfo =
              {
                  FileName = chrome_exe_path,
                  Arguments = "--user-data-dir=" + Path.GetTempPath() + "/firemon --kiosk --kiosk-printing " + HelperFunctions.FiremonMonitorUrl
              }
                };
                ChromeProcess.Start();

                if (showReopenDialog)
                {
                    Task t = Task.Run(async () =>
                    {
                        HelperFunctions.ChromeProcess.WaitForExit();
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            ShowReopenWindow();
                        });
                    });
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }


        }

        public static void ShowReopenWindow()
        {
            if (notificationWindow != null)
                notificationWindow.Close();

            notificationWindow = new NotificationWindow();
            notificationWindow.Topmost = true;

            notificationWindow.Show();
            notificationWindow.Focus();
        }

        #endregion

        #region VGA / Monitor / Windows API
        [DllImport("user32.dll")]
        private static extern int SendMessage(int hWnd, int hMsg, int wParam, int lParam);

        public static void SetMonitorInState(MonitorState state)
        {
            SendMessage(0xFFFF, 0x112, 0xF170, (int)state);
        }

        public enum MonitorState
        {
            MonitorStateOn = -1,
            MonitorStateOff = 2,
            MonitorStateStandBy = 1
        }
        #endregion

        #region Updates
        public static void InstallUpdate()
        {
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                ApplicationDeployment ad = ApplicationDeployment.CurrentDeployment;
                ad.CheckForUpdateCompleted += new CheckForUpdateCompletedEventHandler(CheckForUpdateCopmleted);
                //ad.CheckForUpdateProgressChanged += new DeploymentProgressChangedEventHandler(ad_CheckForUpdateProgressChanged);

                ad.CheckForUpdateAsync();
            }
        }

        private static void CheckForUpdateCopmleted(object sender, CheckForUpdateCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                return;
            }
            else if (e.Cancelled == true)
            {
                return;
            }

            if (e.UpdateAvailable)
            {
                BeginUpdate();
            }
        }

        private static void BeginUpdate()
        {
            ApplicationDeployment ad = ApplicationDeployment.CurrentDeployment;
            ad.UpdateCompleted += new AsyncCompletedEventHandler(UpdateComplete);
            ad.UpdateAsync();
        }

        private static void UpdateComplete(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                return;
            }
            else if (e.Error != null)
            {
                return;
            }

        }
        #endregion

        public static int GetHDMIMonitorCount()
        {
            int HDMI_Monitors = 0;
            ManagementClass mClass = new ManagementClass(@"\\localhost\ROOT\WMI:WmiMonitorConnectionParams");
            foreach (ManagementObject mObject in mClass.GetInstances())
            {
                if (mObject["VideoOutputTechnology"].Equals(5)) //Because D3DKMDT_VOT_HDMI = 5
                {
                    HDMI_Monitors += 1;
                }
            }

            return HDMI_Monitors;
        }



        #region Shutdown / Reboot
        public static void RebootPC()
        {
            System.Diagnostics.Process.Start("shutdown.exe", "-r -t 0");
        }

        public static void ShutdownPC()
        {
            System.Diagnostics.Process.Start("shutdown.exe", "-s -t 0");
        }

        #endregion
    }
}
