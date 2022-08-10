using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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

namespace DarksideGUI
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("darkside-api.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void HelloWorld();
        public MainWindow()
        {
            InitializeComponent();
            //DataContext = new ViewModelBase();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            HelloWorld();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            InjectPid(25);
        }

        private void InjectPid(int pid)
        {
            MessageBox.Show(String.Format("Injecting pid {0}", pid));
        }
    }
}
