using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Timers;
using System.Windows;
using DarkSideModernGUI.Models;
using DarkSideModernGUI.ViewModels;
using Newtonsoft.Json;
using Wpf.Ui.Common.Interfaces;

namespace DarkSideModernGUI.Views.Pages;

/// <summary>
///     Interaction logic for DashboardPage.xaml
/// </summary>
public partial class DashboardPage : INavigableView<DashboardViewModel>
{
    //Timer to be used for reading the Existing Processes  every 5 seconds
    public static Timer tReadGameDll = new(1000); // 1 sec = 1000, 60 sec = 60000

    public static IntPtr apiObject;

    //Load or Save variables
    private readonly string currentDirectory;
    private readonly string strExeFilePath = Assembly.GetExecutingAssembly().Location;

    public DashboardPage(DashboardViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;

        tReadGameDll.AutoReset = true;
        tReadGameDll.Elapsed += t_ElpasedReadGameDll;
        tReadGameDll.Start();

        currentDirectory = Path.GetDirectoryName(strExeFilePath);

        //gameproccess list
        gameproccess = new ObservableCollection<GameDLL>();
        cbxgameproccess.ItemsSource = gameproccess;
    }

    public ObservableCollection<GameDLL> gameproccess { get; set; }

    public DashboardViewModel ViewModel { get; }

    [DllImport("darkside-api.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void GetPlayerPosition(IntPtr pApiObject, IntPtr lpBuffer);

    private void t_ElpasedReadGameDll(object sender, ElapsedEventArgs e)

    {
        //dummyproof ==> If injected == 1 then.. otherwise do nothing.

        //get all GameDLL processes
        var localByName = Process.GetProcessesByName("game.dll");

        Dispatcher.Invoke(
            () =>
            {
                //gameproccess.Clear();
            });

        foreach (var localGameProcess in localByName)
        {
            Dispatcher.Invoke(
                () =>
                {
                    //Check if the ID already exists in the colleciton
                    if (!gameproccess.Any(u => u.GameDLLID == localGameProcess.Id))
                    {
                        gameproccess.Add(new GameDLL { GameDLLID = localGameProcess.Id, Name = localGameProcess.MainWindowTitle });
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

    [DllImport("darkside-api.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr CreateDarksideAPI();

    [DllImport("darkside-api.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr DisposeDarksideAPI(IntPtr pApiObject);

    [DllImport("darkside-api.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void InjectPid(IntPtr pApiObject, int pid);

    private void btnInjectGameDLL_Click(object sender, RoutedEventArgs e)
    {
        apiObject = CreateDarksideAPI();

        if (cbxgameproccess.SelectedIndex != -1)
        {
            InjectPid(apiObject, int.Parse(cbxgameproccess.SelectedValue.ToString()));
        }
    }

    private void btnLaunchBot_Click(object sender, RoutedEventArgs e)
    {
        //Check if a route has been selected.

        if (cbxRouteLoad.SelectedValue.ToString() != "")
        {
            //if route selected then load route

            var selectedFilename = (string)cbxRouteLoad.SelectedItem;

            if (selectedFilename.Length <= 0)
            {
                return;
            }

            var routepath = Path.Combine(currentDirectory, "Routes");
            var fullPath = Path.Combine(routepath, selectedFilename);
            fullPath = Path.ChangeExtension(fullPath, "json");
            var importedWaypoints = JsonConvert.DeserializeObject<ObservableCollection<Waypoint>>(File.ReadAllText(fullPath));

            tbMultiLine.Text += "Dude, Route: " + selectedFilename + " has been selected, let's move baby..." + Environment.NewLine;

            if (importedWaypoints == null)
            {
                return;
            }

            var WaypointsAndDistance = new List<distancetowaypoints>();
            WaypointsAndDistance.Clear();

            foreach (var importedWaypoint in importedWaypoints)
            {
                var size = Marshal.SizeOf<PlayerPosition>();
                var buf = Marshal.AllocHGlobal(Marshal.SizeOf<PlayerPosition>());

                GetPlayerPosition(apiObject, buf);

                var playerPos = (PlayerPosition)Marshal.PtrToStructure(buf, typeof(PlayerPosition));

                var DistanceBetweenPlayerAndWaypoint = Math.Sqrt((playerPos.pos_x - float.Parse(importedWaypoint.playerPosX)) * (playerPos.pos_x - float.Parse(importedWaypoint.playerPosX)) + (playerPos.pos_y - float.Parse(importedWaypoint.playerPosY)) * (playerPos.pos_y - float.Parse(importedWaypoint.playerPosY)));

                tbMultiLine.Text += "Distance to Waypoint " + importedWaypoint.waypointID + " is:" + DistanceBetweenPlayerAndWaypoint + Environment.NewLine;

                WaypointsAndDistance.Add(new distancetowaypoints { WaypointID = importedWaypoint.waypointID, DistanceBetweenPlayerAndWaypoint = DistanceBetweenPlayerAndWaypoint });
            }

            // Ordering the list from small to big distance to waypoint
            var OrderedWaypointsAndDistance = WaypointsAndDistance.OrderBy(x => x.DistanceBetweenPlayerAndWaypoint).ToList();

            // First will be the closest Waypoint
            var closestWaypoint = OrderedWaypointsAndDistance.First();

            tbMultiLine.Text += "--------------------------------------------" + Environment.NewLine;

            tbMultiLine.Text += "The closest waypoint is Waypoint: " + closestWaypoint.WaypointID + Environment.NewLine;

            //do some fucking movement with the waypoints
        }
        else
        {
            //if no route selected, tell the user that no route has been selected and bot will be static
            tbMultiLine.Text += "Dude, no Route selected, bot will be static ..." + Environment.NewLine;
        }
    }

    private void scanDirectoryForWaypointRoute()
    {
        var routepath = Path.Combine(currentDirectory, "Routes");

        if (!Directory.Exists(routepath))
        {
            return;
        }

        var wprs = Directory.GetFiles(routepath, "*.json");

        if (wprs.Length > 0)
        {
            var fileName = new string[wprs.Length + 1];

            for (var i = 0; i < wprs.Length; ++i)
            {
                fileName[i] = Path.GetFileNameWithoutExtension(wprs[i]);
            }

            Dispatcher.Invoke(() => { cbxRouteLoad.ItemsSource = fileName; });
        }
    }

    //Player position struct
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct PlayerPosition
    {
        public float pos_x { get; private set; }
        public short heading { get; private set; }
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 68)]
        public char[] unknown1;
        public float pos_y { get; private set; }
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public char[] unknown2;
        public float pos_z { get; private set; }
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
        public char[] unknown3;
    }

    public struct GameDLL
    {
        public int GameDLLID { get; set; }
        public string Name { get; set; }
        public bool isInjected { get; set; }
    }

    public class distancetowaypoints
    {
        public string WaypointID { get; set; }
        public double DistanceBetweenPlayerAndWaypoint { get; set; }
    }
}