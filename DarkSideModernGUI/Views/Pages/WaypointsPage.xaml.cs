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
using System.Windows.Shapes;
using System.Timers;
using System.Threading;


namespace DarkSideModernGUI.Views.Pages
{
    /// <summary>
    /// Interaction logic for WaypointsPage.xaml
    /// </summary>
    public partial class WaypointsPage : INavigableView<ViewModels.WaypointsViewModel>
    {


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
       

        //IntPtr apiObject;


        public ViewModels.WaypointsViewModel ViewModel
        {
            get;
        }

        public WaypointsPage(ViewModels.WaypointsViewModel viewModel)
        {
            ViewModel = viewModel;

            InitializeComponent();

            // CheckForIllegalCrossThreadCalls = false; //this is BAD CODING :) - Need reworks with the threads.


            //Timer to be used for reading the Position Stream every 1 ms
            System.Timers.Timer tLogStream = new System.Timers.Timer(1000); // 1 sec = 1000, 60 sec = 60000
            tLogStream.AutoReset = true;
            tLogStream.Elapsed += new System.Timers.ElapsedEventHandler(t_Elapsed);
            tLogStream.Start();

        }



        private void t_Elapsed(object sender, System.Timers.ElapsedEventArgs e)

        {

            //dummyproof ==> If injected == 1 then.. otherwise do nothing.

            int size = Marshal.SizeOf<PlayerPosition>();
            IntPtr buf = Marshal.AllocHGlobal(Marshal.SizeOf<PlayerPosition>());


            GetPlayerPosition(TestPage.apiObject, buf);


            PlayerPosition playerPos = (PlayerPosition)Marshal.PtrToStructure(buf, typeof(PlayerPosition));

            // MessageBox.Show(String.Format("Created PlayerPosition object at {0}", buf));
            // MessageBox.Show((playerPos.pos_x).ToString());

            Dispatcher.Invoke(() => {
            lblWaypointX.Content = (playerPos.pos_x).ToString();
            lblWaypointY.Content = (playerPos.pos_y).ToString();
            lblWaypointZ.Content = (playerPos.pos_z).ToString();
            lblWaypointDir.Content = (playerPos.heading).ToString();
            });
        }


    }
}