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
        public static extern void SetAutorun(IntPtr pApiObject, bool autorun);
        [DllImport("darkside-api.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetPartyMember(IntPtr pApiObject, int memberIndex, IntPtr lpBuffer);

        IntPtr apiObject;
        bool autorun = false;

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
            InjectPid(apiObject, Int32.Parse(NumberTextBox.Text));
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            int size = Marshal.SizeOf<PlayerPosition>();
            IntPtr buf = Marshal.AllocHGlobal(Marshal.SizeOf<PlayerPosition>());



            GetPlayerPosition(apiObject, buf);


            PlayerPosition playerPos = (PlayerPosition)Marshal.PtrToStructure(buf, typeof(PlayerPosition));

            MessageBox.Show(String.Format("Created PlayerPosition object at {0}", buf));
            MessageBox.Show((playerPos.pos_x).ToString());
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            autorun = !autorun;
            SetAutorun(apiObject, autorun);
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            int size = Marshal.SizeOf<PartyMemberInfo>();
            IntPtr buf = Marshal.AllocHGlobal(Marshal.SizeOf<PartyMemberInfo>());

            GetPartyMember(apiObject, Int32.Parse(MemberIndexBox.Text), buf);

            PartyMemberInfo partyMember = (PartyMemberInfo)Marshal.PtrToStructure(buf, typeof(PartyMemberInfo));

            String cname = new string(partyMember.name);

            String msg = String.Format("PartyMember Name is {0}" + Environment.NewLine + "HP: {1}%"
                + Environment.NewLine + "Endu: {2}%" + Environment.NewLine + "Pow: {3}%",
                cname, partyMember.hp_pct, partyMember.endu_pct, partyMember.pow_pct);

            MemberInfo.Text = msg;
        }
    }
}
