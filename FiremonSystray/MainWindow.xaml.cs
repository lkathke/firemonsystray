using FiremonSystray.Utils;
using FiremonSystray.Websocket;
using SourceChord.FluentWPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FiremonSystray
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : AcrylicWindow
    {
#if DEBUG
        private const string firemonMonitorUrl = "https://stage.firemon112.de/monitor";
#else
        private const string firemonMonitorUrl = "https://firemon112.de/monitor";
#endif
        private const int firemonWebsocketPort = 3000;


        public MainWindow()
        {
            InitializeComponent();
            this.Hide();

            HelperFunctions.FiremonMonitorUrl = firemonMonitorUrl;
#if DEBUG
            this.Title += " Debug-Mode (stage.firemon112.de)";
#endif

            Task.Run(() =>
            {
                SocketServer socketServer = new SocketServer(firemonWebsocketPort);
                socketServer.Start();
            });

            LoadSettings();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!HelperFunctions.OpenFiremonMonitor(true, cbxUseInternalBrowser.IsChecked.HasValue ? cbxUseInternalBrowser.IsChecked.Value : false))
            {
                if (MessageBox.Show("Google Chrome ist nicht installiert. Möchten Sie es herunterladen und installieren?", "Google Chrome installieren?", MessageBoxButton.YesNoCancel) == MessageBoxResult.Yes)
                {
                    System.Diagnostics.Process.Start(firemonMonitorUrl);
                }
            }
        }

        private void menuStartChrome_Click(object sender, RoutedEventArgs e)
        {
            HelperFunctions.OpenFiremonMonitor(true, cbxUseInternalBrowser.IsChecked.HasValue ? cbxUseInternalBrowser.IsChecked.Value : false);
        }

        private void menuShowSettings_Click(object sender, RoutedEventArgs e)
        {
            this.Show();
        }

        private void menuClose_Click(object sender, RoutedEventArgs e)
        {
            myNotifyIcon.Dispose();
            System.Windows.Application.Current.Shutdown();
        }

        private void AcrylicWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }

        private void cbxStartWithWindows_Checked(object sender, RoutedEventArgs e)
        {
            HelperFunctions.AddAutoStartRegistry("FiremonSystray", System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
        }

        private void cbxStartWithWindows_Unchecked(object sender, RoutedEventArgs e)
        {
            HelperFunctions.RemoveAutoStartRegistry("FiremonSystray");
        }

        private void cbxStartChrome_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.StartBrowserAutomatically = false;
            Properties.Settings.Default.Save();
        }

        private void cbxStartChrome_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.StartBrowserAutomatically = true;
            Properties.Settings.Default.Save();
        }

        private void LoadSettings()
        {
            Properties.Settings.Default.Reload();
            cbxStartChrome.IsChecked = Properties.Settings.Default.StartBrowserAutomatically;
            cbxUseInternalBrowser.IsChecked = Properties.Settings.Default.UseEmbeddedBrowser;

            var usesAutostart = HelperFunctions.IsAutoStartEnabled("FiremonSystray");
            cbxStartWithWindows.IsChecked = usesAutostart;

            if(Properties.Settings.Default.StartBrowserAutomatically) { 
                HelperFunctions.OpenFiremonMonitor(true, cbxUseInternalBrowser.IsChecked.HasValue ? cbxUseInternalBrowser.IsChecked.Value : false);
            }

           // this.myNotifyIcon.DoubleClickCommand = new Command(() =>
           //{
           //    HelperFunctions.OpenFiremonMonitor(true, cbxUseInternalBrowser.IsChecked.HasValue ? cbxUseInternalBrowser.IsChecked.Value : false);
           //});

        }

        private void myNotifyIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            HelperFunctions.OpenFiremonMonitor(true, cbxUseInternalBrowser.IsChecked.HasValue ? cbxUseInternalBrowser.IsChecked.Value : false);
        }

        private void cbxUseInternalBrowser_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.UseEmbeddedBrowser = true;
            Properties.Settings.Default.Save();
        }

        private void cbxUseInternalBrowser_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.UseEmbeddedBrowser = false;
            Properties.Settings.Default.Save();
        }
    }
}
