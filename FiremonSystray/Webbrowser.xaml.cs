using FiremonSystray.Utils;
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
    /// Interaktionslogik für Webbrowser.xaml
    /// </summary>
    public partial class Webbrowser : Window
    {
        public Webbrowser()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            HelperFunctions.ShowReopenWindow();
        }
    }
}
