using Wpf.Ui.Common.Interfaces;
using DarkSideModernGUI.Helpers;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Windows;
using System;
using System.Collections.Generic;
using System.Linq;

using DarkSideModernGUI.Models;

using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;


using static DarkSideModernGUI.Helpers.DarksideGameAPI;

namespace DarkSideModernGUI.Views.Pages
{
    /// <summary>
    /// Interaction logic for DashboardPage.xaml
    /// </summary>
    public partial class DashboardPage : INavigableView<ViewModels.DashboardViewModel>
    {


        //Timer to be used for reading the Existing Processes  every 5 seconds
        public static System.Timers.Timer tReadGameDll = new System.Timers.Timer(1000); // 1 sec = 1000, 60 sec = 60000

        //Load or Save variables
        private String currentDirectory;
        private string strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;

        static public ObservableCollection<GameDLL> gameprocs { get; set; }

        

        public ViewModels.DashboardViewModel ViewModel
        {
            get;
        }

        public DashboardPage(ViewModels.DashboardViewModel viewModel)
        {

            InitializeComponent();
            ViewModel = viewModel;

            tReadGameDll.AutoReset = true;
            tReadGameDll.Elapsed += new System.Timers.ElapsedEventHandler(t_ElpasedReadGameDll);
            tReadGameDll.Start();

            this.currentDirectory = Path.GetDirectoryName(strExeFilePath);

            

            //gameprocs list
            gameprocs = new ObservableCollection<GameDLL>() { };
            cbxgameproccess.ItemsSource = gameprocs;


        }

        private void t_ElpasedReadGameDll(object sender, System.Timers.ElapsedEventArgs e)

        {

            //dummyproof ==> If injected == 1 then.. otherwise do nothing.

            //get all GameDLL processes
            Process[] localByName = Process.GetProcessesByName("game.dll");
            Dispatcher.Invoke(() => {
                    //gameprocs.Clear();
            });

            foreach (var localGameProcess in localByName)
            {
                Dispatcher.Invoke(() => {
                    //Check if the ID already exists in the colleciton
                    if (!gameprocs.Any(u => u.procId == localGameProcess.Id))
                    {
                        gameprocs.Add(new GameDLL()
                        {
                            procId = localGameProcess.Id,
                            Name = localGameProcess.MainWindowTitle,
                            apiObject = CreateDarksideAPI()
                        });
                    }

                });
            }

            
            scanDirectoryForWaypointRoute();

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
            public int procId { get; set; }
            public string Name { get; set; }
            public IntPtr apiObject { get; set; }          
        }

        //Single game inject
        public static IntPtr apiObject;

        private void btnInjectGameDLL_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            apiObject = CreateDarksideAPI();
            if (cbxgameproccess.SelectedIndex != -1)
            {
                if (InjectPid(apiObject, Int32.Parse(cbxgameproccess.SelectedValue.ToString())))
                {

                }
            }
        }

        private void btnInjectAll_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            foreach (GameDLL proc in gameprocs)
            {
                if (InjectPid(proc.apiObject, proc.procId))
                {

                }
            }
        }

        private void btnUnloadAll_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            foreach (GameDLL proc in gameprocs)
            {
                if (Unload(proc.apiObject, proc.procId))
                {
                    //API calls dispose so this object is now deleted
                }
            }
            gameprocs.Clear();
        }


        public class distancetowaypoints {

            public string WaypointID { get; set; }
            public Double DistanceBetweenPlayerAndWaypoint { get; set; }

        }



        private void btnLaunchBot_Click(object sender, RoutedEventArgs e)
        {
            //Check if a route has been selected.

            if (cbxRouteLoad.SelectedValue.ToString() != "")
            {
                //if route selected then load route


                var selectedFilename = (string)this.cbxRouteLoad.SelectedItem;

                if (selectedFilename.Length <= 0)
                {
                    return;
                }

                var routepath = Path.Combine(this.currentDirectory, "Routes");
                var fullPath = Path.Combine(routepath, selectedFilename);
                fullPath = Path.ChangeExtension(fullPath, "json");
                var importedWaypoints = JsonConvert.DeserializeObject<ObservableCollection<Waypoint>>(File.ReadAllText(fullPath));

                tbMultiLine.Text += "Dude, Route: " + selectedFilename + " has been selected, let's move baby..." + Environment.NewLine;

                if (importedWaypoints == null)
                {
                    return;
                }

                List<distancetowaypoints> WaypointsAndDistance = new List<distancetowaypoints>();
                WaypointsAndDistance.Clear();

                foreach (var importedWaypoint in importedWaypoints)
                {
                    int size = Marshal.SizeOf<PlayerPosition>();
                    IntPtr buf = Marshal.AllocHGlobal(Marshal.SizeOf<PlayerPosition>());

                    GetPlayerPosition(DashboardPage.apiObject, buf);

                    PlayerPosition playerPos = (PlayerPosition)Marshal.PtrToStructure(buf, typeof(PlayerPosition));

                    Double DistanceBetweenPlayerAndWaypoint = Math.Sqrt((playerPos.pos_x - float.Parse(importedWaypoint.playerPosX)) * (playerPos.pos_x - float.Parse(importedWaypoint.playerPosX)) + (playerPos.pos_y - float.Parse(importedWaypoint.playerPosY)) * (playerPos.pos_y - float.Parse(importedWaypoint.playerPosY)));

                    tbMultiLine.Text += "Distance to Waypoint " + importedWaypoint.waypointID + " is:" + DistanceBetweenPlayerAndWaypoint + Environment.NewLine;

                    WaypointsAndDistance.Add(new distancetowaypoints { WaypointID = importedWaypoint.waypointID, DistanceBetweenPlayerAndWaypoint = DistanceBetweenPlayerAndWaypoint });

                }

                // Ordering the list from small to big distance to waypoint
                List <distancetowaypoints> OrderedWaypointsAndDistance = WaypointsAndDistance.OrderBy(x => x.DistanceBetweenPlayerAndWaypoint).ToList();

                // First will be the closest Waypoint
                distancetowaypoints closestWaypoint = OrderedWaypointsAndDistance.First();

                tbMultiLine.Text += "--------------------------------------------" +  Environment.NewLine;

                tbMultiLine.Text += "The closest waypoint is Waypoint: " + closestWaypoint.WaypointID + Environment.NewLine;




                //do some fucking movement with the waypoints

            }
            else {
                //if no route selected, tell the user that no route has been selected and bot will be static
                tbMultiLine.Text +="Dude, no Route selected, bot will be static ..." + Environment.NewLine;
            }
        }

        private void scanDirectoryForWaypointRoute()
        {
            string routepath = Path.Combine(this.currentDirectory, "Routes");
            string[] wprs = Directory.GetFiles(routepath, "*.json");
            if (wprs.Length > 0)
            {
                string[] fileName = new string[wprs.Length + 1];

                for (int i = 0; i < wprs.Length; ++i)
                {
                    fileName[i] = Path.GetFileNameWithoutExtension(wprs[i]);
                }

                Dispatcher.Invoke(() =>
                {
                    this.cbxRouteLoad.ItemsSource = fileName;
                });
            }
            else
            {

            }

        }


    }
}
