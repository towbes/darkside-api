using Wpf.Ui.Common.Interfaces;
using DarkSideModernGUI.Helpers;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Windows;
using System;

namespace DarkSideModernGUI.Views.Pages
{
    /// <summary>
    /// Interaction logic for DashboardPage.xaml
    /// </summary>
    public partial class DashboardPage : INavigableView<ViewModels.DashboardViewModel>
    {


        //Timer to be used for reading the Existing Processes  every 5 seconds
        public static System.Timers.Timer tReadGameDll = new System.Timers.Timer(5000); // 1 sec = 1000, 60 sec = 60000

        public ObservableCollection<GameDLL> gameproccess { get; set; }

        public ViewModels.DashboardViewModel ViewModel
        {
            get;
        }

        public DashboardPage(ViewModels.DashboardViewModel viewModel)
        {
            ViewModel = viewModel;

            tReadGameDll.AutoReset = true;
            tReadGameDll.Elapsed += new System.Timers.ElapsedEventHandler(t_ElpasedReadGameDll);
            tReadGameDll.Start();

            InitializeComponent();

            //gameproccess list
            gameproccess = new ObservableCollection<GameDLL>() { };
            cbxgameproccess.ItemsSource = gameproccess;


        }

        private void t_ElpasedReadGameDll(object sender, System.Timers.ElapsedEventArgs e)

        {

            //dummyproof ==> If injected == 1 then.. otherwise do nothing.


            //get all GameDLL processes
            Process[] localByName = Process.GetProcessesByName("game.dll");
            Dispatcher.Invoke(() => {
                gameproccess.Clear();
            });

            foreach (var localGameProcess in localByName)
            {
                Dispatcher.Invoke(() => {
                    gameproccess.Add(new GameDLL()
                    {
                       GameDLLID = localGameProcess.Id

                    });
                });
            }


            //private static Process p = Process.GetProcessesByName("game.dll").FirstOrDefault(); // get  DAoCMWC
            //IntPtr procId = Process.GetProcessesByName("game.dll").FirstOrDefault().MainWindowHandle; //get Mainwindow

            //Dispatcher.Invoke(() => {
            //    lblWaypointX.Content = (playerPos.pos_x).ToString();
            //    lblWaypointY.Content = (playerPos.pos_y).ToString();
            //    lblWaypointZ.Content = (playerPos.pos_z).ToString();
            //    lblWaypointDir.Content = (playerPos.heading).ToString();

            //});


        }


        public struct GameDLL
        {
            public int GameDLLID { get; set; }
          
        }


        [DllImport("darkside-api.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr CreateDarksideAPI();

        [DllImport("darkside-api.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr DisposeDarksideAPI(IntPtr pApiObject);

        [DllImport("darkside-api.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void InjectPid(IntPtr pApiObject, int pid);

      
        private void btnInjectGameDLL_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            apiObject = CreateDarksideAPI();
            InjectPid(apiObject, Int32.Parse(cbxgameproccess.SelectedValue.ToString()));
        }

        public static IntPtr apiObject;

    }
}
