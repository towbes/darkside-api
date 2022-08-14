using Wpf.Ui.Common.Interfaces;
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
//using System.Windows.Shapes;
using System.Timers;
using System.Threading;
using System.Collections.ObjectModel;

using System.IO;
using System.Text.RegularExpressions;
using System.Security.Permissions;

using System.ComponentModel;
using System.Data;
using System.Drawing;




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


        //Player position struct
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct PlayerPosition
        {
            public float pos_x { get; private set; }
            public short heading { get; private set; }
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 68)] public char[] unknown1;
            public float pos_y { get; private set; }
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] public char[] unknown2;
            public float pos_z { get; private set; }
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)] public char[] unknown3;
        }

        [DllImport("darkside-api.dll", CallingConvention = CallingConvention.Cdecl)]

        public static extern void GetPlayerPosition(IntPtr pApiObject, IntPtr lpBuffer);

        //Timer to be used for reading the Position Stream every 1 ms
        public static System.Timers.Timer tAutoWaypoints = new System.Timers.Timer(1000); // 1 sec = 1000, 60 sec = 60000
    

        //IntPtr apiObject;


        public ViewModels.WaypointsViewModel ViewModel
        {
            get;
        }

        public WaypointsPage(ViewModels.WaypointsViewModel viewModel)
        {
            ViewModel = viewModel;

           //Timer to be used for reading the Position Stream every 1 ms
            System.Timers.Timer tPlayerPositionUpdae = new System.Timers.Timer(1000); // 1 sec = 1000, 60 sec = 60000
            tPlayerPositionUpdae.AutoReset = true;
            tPlayerPositionUpdae.Elapsed += new System.Timers.ElapsedEventHandler(t_Elapsed);
            tPlayerPositionUpdae.Start();

            tAutoWaypoints.AutoReset = true;
            tAutoWaypoints.Elapsed += new System.Timers.ElapsedEventHandler(t_ElpasedAutoWaypoint);
            //tAutoWaypoints.Start();

            this.currentDirectory = Path.GetDirectoryName(strExeFilePath);


            InitializeComponent();

            //waypoint list
            waypoint = new ObservableCollection<Waypoint>(){};
            grdWaypoints.ItemsSource = waypoint;
        }



        private void t_Elapsed(object sender, System.Timers.ElapsedEventArgs e)

        {

            //dummyproof ==> If injected == 1 then.. otherwise do nothing.

            int size = Marshal.SizeOf<PlayerPosition>();
            IntPtr buf = Marshal.AllocHGlobal(Marshal.SizeOf<PlayerPosition>());

            GetPlayerPosition(TestPage.apiObject, buf);

            PlayerPosition playerPos = (PlayerPosition)Marshal.PtrToStructure(buf, typeof(PlayerPosition));

            Dispatcher.Invoke(() => {
            lblWaypointX.Content = (playerPos.pos_x).ToString();
            lblWaypointY.Content = (playerPos.pos_y).ToString();
            lblWaypointZ.Content = (playerPos.pos_z).ToString();
            lblWaypointDir.Content = (playerPos.heading).ToString();
            });
        }


        private void t_ElpasedAutoWaypoint(object sender, System.Timers.ElapsedEventArgs e)

        {

            int size = Marshal.SizeOf<PlayerPosition>();
            IntPtr buf = Marshal.AllocHGlobal(Marshal.SizeOf<PlayerPosition>());
            GetPlayerPosition(TestPage.apiObject, buf);
            PlayerPosition playerPos = (PlayerPosition)Marshal.PtrToStructure(buf, typeof(PlayerPosition));

            Dispatcher.Invoke(() =>
            {
                    waypoint.Add(new Waypoint()
                {
                    waypointID = (grdWaypoints.Items.Count - 1).ToString(),
                    playerPosX = (playerPos.pos_x).ToString(),
                    playerPosY = (playerPos.pos_y).ToString(),
                    playerPosZ = (playerPos.pos_z).ToString(),
                    playerHeading = (playerPos.heading).ToString()
                });
            });
        }

        public struct Waypoint
        {
            public string waypointID { get; set; }
            public string playerPosX { get; set; }
            public string playerPosY { get; set; }
            public string playerPosZ { get; set; }
            public string playerHeading { get; set; }
        }


        private void btnAddWaypoint_Click(object sender, RoutedEventArgs e)
        {

            int size = Marshal.SizeOf<PlayerPosition>();
            IntPtr buf = Marshal.AllocHGlobal(Marshal.SizeOf<PlayerPosition>());
            GetPlayerPosition(TestPage.apiObject, buf);
            PlayerPosition playerPos = (PlayerPosition)Marshal.PtrToStructure(buf, typeof(PlayerPosition));


           waypoint.Add(new Waypoint()
            {
                waypointID = (grdWaypoints.Items.Count - 1).ToString(),
                playerPosX = (playerPos.pos_x).ToString(),
                playerPosY = (playerPos.pos_y).ToString(),
                playerPosZ = (playerPos.pos_z).ToString(),
                playerHeading = (playerPos.heading).ToString()
           });

        
        }

        private void btnAddAutoWaypoint_Click(object sender, RoutedEventArgs e)
        {

            if (tAutoWaypoints.Enabled)
            {
                tAutoWaypoints.Stop();
                Dispatcher.Invoke(() =>
                {
                    btnAddAutoWaypoint.Content = "Start Auto Waypoints";

                });
            }
            else {
                tAutoWaypoints.Start();
                Dispatcher.Invoke(() =>
                {
                    btnAddAutoWaypoint.Content = "Stop Auto Waypoints";

                });
            }    

          
        }

        private void btnNewRouteWaypoint_Click(object sender, RoutedEventArgs e)
        {
           
                waypoint.Clear();
            
        }

        private void btnSaveRoute_Click(object sender, RoutedEventArgs e)
        {
            bool successSave = false;
            String txtSaveNewRouteFileName = this.txtSaveNewRouteFileName.Text;
            if (txtSaveNewRouteFileName.Length == 0)
            {
                txtSaveNewRouteFileName = (string)this.cmbWaypointOverwriteRoute.SelectedItem;
            }

            if (txtSaveNewRouteFileName != null && txtSaveNewRouteFileName.Length > 0)
            {
                try
                {
                    string regexSearch = new string(Path.GetInvalidPathChars());
                    Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
                    txtSaveNewRouteFileName = r.Replace(txtSaveNewRouteFileName, "");

                    String fullPath = Path.Combine(this.currentDirectory, txtSaveNewRouteFileName);
                    fullPath = Path.ChangeExtension(fullPath, "wpr");

                    using (FileStream fs = new FileStream(fullPath, FileMode.Create))
                    {
                        using (BinaryWriter binWriter = new BinaryWriter(fs))
                        {
                            Waypoint[] waypointpArray = waypoint.ToArray();
                            for (int i = 0; i < waypointpArray.Length; ++i)
                            {
                                Waypoint item = waypointpArray[i];
                                byte[] itemByte = this.getBytes(item);
                                binWriter.Write(itemByte);
                            }
                            successSave = true;
                            binWriter.Close();
                        }
                        fs.Close();
                    }
                }
                catch
                {
                    successSave = false;
                    MessageBox.Show("Unknown Error\n\nabort loading", "Loading Error");
                }
            }

            if (successSave)
            {
                //this.saveFinishDelegate(txtSaveNewRouteFileName);
               // this.Close();
            }
            else
            {
                //this.scanDirectoryForWaypointRoute();
            }
        }

        byte[] getBytes(Waypoint str)
        {
            int size = Marshal.SizeOf(str);
            byte[] arr = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.StructureToPtr(str, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);

            return arr;
        }

    }
}