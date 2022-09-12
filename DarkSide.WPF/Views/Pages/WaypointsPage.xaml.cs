using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using DarkSide.WPF.Models;
using DarkSide.WPF.ViewModels;
using Newtonsoft.Json;
using Wpf.Ui.Common.Interfaces;

namespace DarkSide.WPF.Views.Pages;

/// <summary>
///     Interaction logic for WaypointsPage.xaml
/// </summary>
public partial class WaypointsPage : INavigableView<WaypointsViewModel>
{
    //Timer to be used for reading the Position Stream every 1 ms
    public static Timer tAutoWaypoints = new(1000); // 1 sec = 1000, 60 sec = 60000

    //Load or Save variables
    private readonly string currentDirectory;
    private readonly string strExeFilePath = Assembly.GetExecutingAssembly().Location;

    public WaypointsPage(WaypointsViewModel viewModel)
    {
        ViewModel = viewModel;

        //Timer to be used for reading the Position Stream every 1 ms
        var tPlayerPositionUpdae = new Timer(1000); // 1 sec = 1000, 60 sec = 60000
        tPlayerPositionUpdae.AutoReset = true;
        tPlayerPositionUpdae.Elapsed += t_Elapsed;
        tPlayerPositionUpdae.Start();

        tAutoWaypoints.AutoReset = true;
        tAutoWaypoints.Elapsed += t_ElpasedAutoWaypoint;
        //tAutoWaypoints.Start();

        currentDirectory = Path.GetDirectoryName(strExeFilePath);

        InitializeComponent();

        //waypoint list
        waypoint = new ObservableCollection<Waypoint>();
        grdWaypoints.ItemsSource = waypoint;
    }

    //To update the gridview on waypoints addition

    public ObservableCollection<Waypoint> waypoint { get; set; }

    //IntPtr apiObject;

    public WaypointsViewModel ViewModel { get; }

    [DllImport("darkside-api.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void GetPlayerPosition(IntPtr pApiObject, IntPtr lpBuffer);

    private void t_Elapsed(object sender, ElapsedEventArgs e)

    {
        //dummyproof ==> If injected == 1 then.. otherwise do nothing.

        var size = Marshal.SizeOf<PlayerPosition>();
        var buf = Marshal.AllocHGlobal(Marshal.SizeOf<PlayerPosition>());

        GetPlayerPosition(DashboardPage.apiObject, buf);

        var playerPos = (PlayerPosition)Marshal.PtrToStructure(buf, typeof(PlayerPosition));

        Dispatcher.Invoke(
            () =>
            {
                lblWaypointX.Content = playerPos.pos_x.ToString();
                lblWaypointY.Content = playerPos.pos_y.ToString();
                lblWaypointZ.Content = playerPos.pos_z.ToString();
                lblWaypointDir.Content = playerPos.heading.ToString();
            });

        scanDirectoryForWaypointRoute();
    }

    private void t_ElpasedAutoWaypoint(object sender, ElapsedEventArgs e)

    {
        var size = Marshal.SizeOf<PlayerPosition>();
        var buf = Marshal.AllocHGlobal(Marshal.SizeOf<PlayerPosition>());
        GetPlayerPosition(TestPage.apiObject, buf);
        var playerPos = (PlayerPosition)Marshal.PtrToStructure(buf, typeof(PlayerPosition));

        Dispatcher.Invoke(
            () =>
            {
                waypoint.Add(
                    new Waypoint
                    {
                        waypointID = grdWaypoints.Items.Count.ToString(),
                        playerPosX = playerPos.pos_x.ToString(),
                        playerPosY = playerPos.pos_y.ToString(),
                        playerPosZ = playerPos.pos_z.ToString(),
                        playerHeading = playerPos.heading.ToString()
                    });
            });
    }

    private void btnAddWaypoint_Click(object sender, RoutedEventArgs e)
    {
        var size = Marshal.SizeOf<PlayerPosition>();
        var buf = Marshal.AllocHGlobal(Marshal.SizeOf<PlayerPosition>());
        GetPlayerPosition(TestPage.apiObject, buf);
        var playerPos = (PlayerPosition)Marshal.PtrToStructure(buf, typeof(PlayerPosition));

        waypoint.Add(
            new Waypoint
            {
                waypointID = grdWaypoints.Items.Count.ToString(),
                playerPosX = playerPos.pos_x.ToString(),
                playerPosY = playerPos.pos_y.ToString(),
                playerPosZ = playerPos.pos_z.ToString(),
                playerHeading = playerPos.heading.ToString()
            });
    }

    private void btnAddAutoWaypoint_Click(object sender, RoutedEventArgs e)
    {
        if (tAutoWaypoints.Enabled)
        {
            tAutoWaypoints.Stop();
            Dispatcher.Invoke(() => { btnAddAutoWaypoint.Content = "Start Auto Waypoints"; });
        }
        else
        {
            tAutoWaypoints.Start();
            Dispatcher.Invoke(() => { btnAddAutoWaypoint.Content = "Stop Auto Waypoints"; });
        }
    }

    private void btnNewRouteWaypoint_Click(object sender, RoutedEventArgs e)
    {
        waypoint.Clear();
    }

    private void btnSaveRoute_Click(object sender, RoutedEventArgs e)
    {
        var successSave = false;
        var txtSaveNewRouteFileName = this.txtSaveNewRouteFileName.Text;

        if (txtSaveNewRouteFileName.Length == 0)
        {
            txtSaveNewRouteFileName = (string)cmbWaypointOverwriteRoute.SelectedItem;
        }

        if (txtSaveNewRouteFileName != null
            && txtSaveNewRouteFileName.Length > 0)
        {
            try
            {
                var regexSearch = new string(Path.GetInvalidPathChars());
                var r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
                txtSaveNewRouteFileName = r.Replace(txtSaveNewRouteFileName, "");

                var routepath = Path.Combine(currentDirectory, "Routes");

                if (!Directory.Exists(routepath))
                {
                    Directory.CreateDirectory("Routes");
                }

                var fullPath = Path.Combine(routepath, txtSaveNewRouteFileName);

                fullPath = Path.ChangeExtension(fullPath, "json");

                var output = JsonConvert.SerializeObject(waypoint);
                File.WriteAllText(fullPath, output);
            }
            catch
            {
                successSave = false;
                MessageBox.Show("Unknown Error\n\nabort loading", "Loading Error");
            }
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

            Dispatcher.Invoke(
                () =>
                {
                    cmbWaypointRouteLoad.ItemsSource = fileName;
                    cmbWaypointOverwriteRoute.ItemsSource = fileName;
                });
        }
    }

    private void cmbWaypointRouteLoad_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
    }

    private void cmbWaypointOverwriteRoute_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selectedFilename = (string)cmbWaypointOverwriteRoute.SelectedItem;

        if (selectedFilename.Length > 0)
        {
            txtSaveNewRouteFileName.Text = selectedFilename;
        }
    }

    private void btnLoadRoute_Click(object sender, RoutedEventArgs e)
    {
        var selectedFilename = (string)cmbWaypointRouteLoad.SelectedItem;

        if (selectedFilename.Length <= 0)
        {
            return;
        }

        var routepath = Path.Combine(currentDirectory, "Routes");
        var fullPath = Path.Combine(routepath, selectedFilename);
        fullPath = Path.ChangeExtension(fullPath, "json");
        var importedWaypoints = JsonConvert.DeserializeObject<ObservableCollection<Waypoint>>(File.ReadAllText(fullPath));
        waypoint.Clear();

        if (importedWaypoints == null)
        {
            return;
        }

        foreach (var importedWaypoint in importedWaypoints)
        {
            waypoint.Add(importedWaypoint);
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
}