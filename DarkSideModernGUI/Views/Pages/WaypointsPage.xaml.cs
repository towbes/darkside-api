using Wpf.Ui.Common.Interfaces;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;

using DarkSideModernGUI.Models;

using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

using static DarkSideModernGUI.Helpers.DarksideGameAPI;


namespace DarkSideModernGUI.Views.Pages
{
    /// <summary>
    /// Interaction logic for WaypointsPage.xaml
    /// </summary>
    public partial class WaypointsPage : INavigableView<ViewModels.WaypointsViewModel>
    {

        //To update the gridview on waypoints addition

        public ObservableCollection<Waypoint> waypoint { get; set; }

        //Load or Save variables
        private String currentDirectory;
        private string strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;



        //Timer to be used for reading the Position Stream every 1 ms
        //public static System.Timers.Timer tAutoWaypoints = new System.Timers.Timer(1000); // 1 sec = 1000, 60 sec = 60000
    

        //IntPtr apiObject;


        public ViewModels.WaypointsViewModel ViewModel
        {
            get;
        }

        public WaypointsPage(ViewModels.WaypointsViewModel viewModel)
        {
            ViewModel = viewModel;

           //Timer to be used for reading the Position Stream every 1 ms
            //System.Timers.Timer tPlayerPositionUpdae = new System.Timers.Timer(1000); // 1 sec = 1000, 60 sec = 60000
            //tPlayerPositionUpdae.AutoReset = true;
            //tPlayerPositionUpdae.Elapsed += new System.Timers.ElapsedEventHandler(t_Elapsed);
            //tPlayerPositionUpdae.Start();
            //
            //tAutoWaypoints.AutoReset = true;
            //tAutoWaypoints.Elapsed += new System.Timers.ElapsedEventHandler(t_ElpasedAutoWaypoint);
            //tAutoWaypoints.Start();

            //this.currentDirectory = Path.GetDirectoryName(strExeFilePath);


            InitializeComponent();

            //waypoint list
            //waypoint = new ObservableCollection<Waypoint>(){};
            //grdWaypoints.ItemsSource = waypoint;
        }



        private void t_Elapsed(object sender, System.Timers.ElapsedEventArgs e)

        {

            //dummyproof ==> If injected == 1 then.. otherwise do nothing.

            //int size = Marshal.SizeOf<PlayerPosition>();
            //IntPtr buf = Marshal.AllocHGlobal(Marshal.SizeOf<PlayerPosition>());
            //
            //GetPlayerPosition(DashboardPage.apiObject, buf);
            //
            //PlayerPosition playerPos = (PlayerPosition)Marshal.PtrToStructure(buf, typeof(PlayerPosition));
            //
            //Dispatcher.Invoke(() => {
            //lblWaypointX.Content = (playerPos.pos_x).ToString("0.0000");
            //lblWaypointY.Content = (playerPos.pos_y).ToString("0.0000");
            //lblWaypointZ.Content = (playerPos.pos_z).ToString("0.0000");
            //lblWaypointDir.Content = (playerPos.heading).ToString("0");
            //
            //});

            //scanDirectoryForWaypointRoute();
        }


        private void t_ElpasedAutoWaypoint(object sender, System.Timers.ElapsedEventArgs e)

        {

            //int size = Marshal.SizeOf<PlayerPosition>();
            //IntPtr buf = Marshal.AllocHGlobal(Marshal.SizeOf<PlayerPosition>());
            //GetPlayerPosition(DashboardPage.apiObject, buf);
            //PlayerPosition playerPos = (PlayerPosition)Marshal.PtrToStructure(buf, typeof(PlayerPosition));
            //
            //Dispatcher.Invoke(() =>
            //{
            //        waypoint.Add(new Waypoint()
            //    {
            //        waypointID = (grdWaypoints.Items.Count).ToString(),
            //        playerPosX = (playerPos.pos_x).ToString("0.0000"),
            //        playerPosY = (playerPos.pos_y).ToString("0.0000"),
            //        playerPosZ = (playerPos.pos_z).ToString("0.0000"),
            //        playerHeading = (playerPos.heading).ToString("0")
            //    });
            //});
            //
            //Marshal.FreeHGlobal(buf);
        }

 


        private void btnAddWaypoint_Click(object sender, RoutedEventArgs e)
        {

           // int size = Marshal.SizeOf<PlayerPosition>();
           // IntPtr buf = Marshal.AllocHGlobal(Marshal.SizeOf<PlayerPosition>());
           // GetPlayerPosition(DashboardPage.apiObject, buf);
           // PlayerPosition playerPos = (PlayerPosition)Marshal.PtrToStructure(buf, typeof(PlayerPosition));
           //
           //
           //waypoint.Add(new Waypoint()
           // {
           //    waypointID = (grdWaypoints.Items.Count).ToString(),
           //    playerPosX = (playerPos.pos_x).ToString("0.0000"),
           //    playerPosY = (playerPos.pos_y).ToString("0.0000"),
           //    playerPosZ = (playerPos.pos_z).ToString("0.0000"),
           //    playerHeading = (playerPos.heading).ToString("0")
           //});
           //
           // Marshal.FreeHGlobal(buf);
        }

        private void btnAddAutoWaypoint_Click(object sender, RoutedEventArgs e)
        {

            //if (tAutoWaypoints.Enabled)
            //{
            //    tAutoWaypoints.Stop();
            //    Dispatcher.Invoke(() =>
            //    {
            //        btnAddAutoWaypoint.Content = "Start Auto Waypoints";
            //
            //    });
            //}
            //else {
            //    tAutoWaypoints.Start();
            //    Dispatcher.Invoke(() =>
            //    {
            //        btnAddAutoWaypoint.Content = "Stop Auto Waypoints";
            //
            //    });
            //}    

          
        }

        private void btnNewRouteWaypoint_Click(object sender, RoutedEventArgs e)
        {
           
                //waypoint.Clear();
            
        }

        private void btnSaveRoute_Click(object sender, RoutedEventArgs e)
        {
            //bool successSave = false;
            //String txtSaveNewRouteFileName = this.txtSaveNewRouteFileName.Text;
            //if (txtSaveNewRouteFileName.Length == 0)
            //{
            //    txtSaveNewRouteFileName = (string)this.cmbWaypointOverwriteRoute.SelectedItem;
            //}
            //
            //if (txtSaveNewRouteFileName != null && txtSaveNewRouteFileName.Length > 0)
            //{
            //    try
            //    {
            //        string regexSearch = new string(Path.GetInvalidPathChars());
            //        Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            //        txtSaveNewRouteFileName = r.Replace(txtSaveNewRouteFileName, "");
            //
            //        string routepath = Path.Combine(this.currentDirectory, "Routes");
            //        if (!Directory.Exists(routepath))
            //        {
            //            Directory.CreateDirectory("Routes");
            //        }
            //
            //        String fullPath = Path.Combine(routepath, txtSaveNewRouteFileName);
            //
            //        fullPath = Path.ChangeExtension(fullPath, "json");
            //
            //        string output = JsonConvert.SerializeObject(waypoint);
            //        System.IO.File.WriteAllText(fullPath, output);
            //
            //    }
            //    catch
            //    {
            //        successSave = false;
            //        MessageBox.Show("Unknown Error\n\nabort loading", "Loading Error");
            //    }
            //}

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
                    this.cmbWaypointRouteLoad.ItemsSource = fileName;
                    this.cmbWaypointOverwriteRoute.ItemsSource = fileName;

                });
            }
            else
            {
                
            }

        }

        private void cmbWaypointRouteLoad_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

  

        private void cmbWaypointOverwriteRoute_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //String selectedFilename = (string)this.cmbWaypointOverwriteRoute.SelectedItem;
            //if (selectedFilename.Length > 0)
            //{
            //    this.txtSaveNewRouteFileName.Text = selectedFilename;
            //}
        }


        private void btnLoadRoute_Click(object sender, RoutedEventArgs e)
        {
            //var selectedFilename = (string)this.cmbWaypointRouteLoad.SelectedItem;
            //
            //if (selectedFilename.Length <= 0)
            //{
            //    return;
            //}
            //
            //var routepath = Path.Combine(this.currentDirectory, "Routes");
            //var fullPath = Path.Combine(routepath, selectedFilename);
            //fullPath = Path.ChangeExtension(fullPath, "json");
            //var importedWaypoints = JsonConvert.DeserializeObject<ObservableCollection<Waypoint>>(File.ReadAllText(fullPath));
            //waypoint.Clear();
            //
            //if (importedWaypoints == null)
            //{
            //    return;
            //}
            //
            //foreach (var importedWaypoint in importedWaypoints)
            //{
            //    waypoint.Add(importedWaypoint);
            //}

           
        }
    }
}