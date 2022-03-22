using FiremonSystray.Utils;
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
using System.Windows.Shapes;

namespace FiremonSystray
{
    /// <summary>
    /// Interaktionslogik für NotificationWindow.xaml
    /// </summary>
    public partial class NotificationWindow : AcrylicWindow
    {
        public NotificationWindow()
        {
            InitializeComponent();
        }

        private void AcrylicWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;
            this.Left = desktopWorkingArea.Right - this.Width;
            this.Top = desktopWorkingArea.Bottom - this.Height;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Reopen_Click(object sender, RoutedEventArgs e)
        {
            HelperFunctions.OpenFiremonMonitor();
            this.Close();
        }
    }
}
