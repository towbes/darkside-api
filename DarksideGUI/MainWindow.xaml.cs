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
        public static extern IntPtr CreateDarksideAPI();

        [DllImport("darkside-api.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr DisposeDarksideAPI(IntPtr pApiObject);

        [DllImport("darkside-api.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void InjectPid(IntPtr pApiObject, int pid);

        IntPtr apiObject;

        public MainWindow()
        {
            InitializeComponent();
            //DataContext = new ViewModelBase();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            apiObject = CreateDarksideAPI();
            MessageBox.Show(String.Format("Created API object {0}", apiObject));
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            InjectPid(apiObject, 25);
        }

    }
}
