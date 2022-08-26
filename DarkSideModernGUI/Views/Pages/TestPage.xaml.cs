using Wpf.Ui.Common.Interfaces;
using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace DarkSideModernGUI.Views.Pages
{
    /// <summary>
    /// Interaction logic for TestPage.xaml
    /// </summary>
    public partial class TestPage : INavigableView<ViewModels.TestViewModel>
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

        //PartyMemberInfo
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct PartyMemberInfo
        {
            public int hp_pct { get; private set; }
            public int endu_pct { get; private set; }
            public int unknown;
            public int pow_pct { get; private set; }
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)] public char[] name;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)] public char[] unknown1;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)] public char[] class_name;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4616)] public char[] unknown2;
        }


        [DllImport("darkside-api.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr CreateDarksideAPI();

        [DllImport("darkside-api.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr DisposeDarksideAPI(IntPtr pApiObject);

        [DllImport("darkside-api.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void InjectPid(IntPtr pApiObject, int pid);

        [DllImport("darkside-api.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetPlayerPosition(IntPtr pApiObject, IntPtr lpBuffer);
        [DllImport("darkside-api.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetPlayerHeading(IntPtr pApiObject, bool changeHeading, short newHeading);
        [DllImport("darkside-api.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetAutorun(IntPtr pApiObject, bool autorun);
        [DllImport("darkside-api.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetPartyMember(IntPtr pApiObject, int memberIndex, IntPtr lpBuffer);

        public static IntPtr apiObject;
        bool autorun = false;
        bool changeHeading = false;




        public ViewModels.TestViewModel ViewModel
        {
            get;
        }

        public TestPage(ViewModels.TestViewModel viewModel)
        {
            ViewModel = viewModel;

            InitializeComponent();
        }


        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            int size = Marshal.SizeOf<PlayerPosition>();
            IntPtr buf = Marshal.AllocHGlobal(Marshal.SizeOf<PlayerPosition>());



            GetPlayerPosition(DashboardPage.apiObject, buf);


            PlayerPosition playerPos = (PlayerPosition)Marshal.PtrToStructure(buf, typeof(PlayerPosition));

            //MessageBox.Show(String.Format("Created PlayerPosition object at {0}", buf));
            MessageBox.Show((playerPos.pos_x).ToString());
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            autorun = !autorun;
            SetAutorun(DashboardPage.apiObject, autorun);
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            int size = Marshal.SizeOf<PartyMemberInfo>();
            IntPtr buf = Marshal.AllocHGlobal(Marshal.SizeOf<PartyMemberInfo>());

            GetPartyMember(DashboardPage.apiObject, Int32.Parse(MemberIndexBox.Text), buf);

            PartyMemberInfo partyMember = (PartyMemberInfo)Marshal.PtrToStructure(buf, typeof(PartyMemberInfo));

            String cname = new string(partyMember.name);

            String msg = String.Format("PartyMember Name is {0}" + Environment.NewLine + "HP: {1}%"
                + Environment.NewLine + "Endu: {2}%" + Environment.NewLine + "Pow: {3}%",
                cname, partyMember.hp_pct, partyMember.endu_pct, partyMember.pow_pct);

            MemberInfo.Text = msg;
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            changeHeading = !changeHeading;
            //If changeheading toggled off, set it to false
            if (!changeHeading)
            {
                SetPlayerHeading(DashboardPage.apiObject, false, 0);
            }
        }

        private void HeadingSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //Change heading when the slider moves if changeHeading is toggled on, otherwise it will do nothing
            SetPlayerHeading(DashboardPage.apiObject, changeHeading, (short)HeadingSlider.Value);
        }

    
    }
}