using Wpf.Ui.Common.Interfaces;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Interop;
using System.Windows.Threading;
using System.Windows.Input;

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

        //EntityInfo
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct EntityInfo
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 48)] public String name;
            public byte type { get; private set; }
            public short objectId { get; private set; }
            public int level { get; private set; }
            public int health { get; private set; }
            public float pos_x { get; private set; }
            public float pos_y { get; private set; }
            public float pos_z { get; private set; }
            public short heading { get; private set; }
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
        public static extern bool GetPartyMember(IntPtr pApiObject, int memberIndex, IntPtr lpBuffer);
        [DllImport("darkside-api.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GetEntityInfo(IntPtr pApiObject, int entityIndex, IntPtr lpBuffer);

        public static IntPtr apiObject;
        bool autorun = false;
        bool changeHeading = false;

        //EntityInfo[] EntityList = new EntityInfo[2000];
        List<EntityInfo> EntityList = new List<EntityInfo>();
        DispatcherTimer dispatcherTimer;
        //Flag to prevent race condition updating EntityList from other threads
        bool ListUpdating = false;

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
            //int size = Marshal.SizeOf<PlayerPosition>();
            //IntPtr buf = Marshal.AllocHGlobal(Marshal.SizeOf<PlayerPosition>());
            //GetPlayerPosition(DashboardPage.apiObject, buf);
            //PlayerPosition playerPos = (PlayerPosition)Marshal.PtrToStructure(buf, typeof(PlayerPosition));
            //
            ////MessageBox.Show(String.Format("Created PlayerPosition object at {0}", buf));
            //MessageBox.Show((playerPos.pos_x).ToString());
            //Start a timer to update the player position textbox
            dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            //update ever 100ms
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0,0, 100);
            dispatcherTimer.Start();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            int size = Marshal.SizeOf<PlayerPosition>();
            IntPtr buf = Marshal.AllocHGlobal(Marshal.SizeOf<PlayerPosition>());
            GetPlayerPosition(DashboardPage.apiObject, buf);
            PlayerPosition playerPos = (PlayerPosition)Marshal.PtrToStructure(buf, typeof(PlayerPosition));
            string msg = String.Format("PlayerPosition:" + Environment.NewLine +
                "{0:0}, {1:0}, {2:0} {3:0}",
                playerPos.pos_x,
                playerPos.pos_y,
                playerPos.pos_z,
                playerPos.heading);
            //
            // Updating the Label which displays the current second
            Marshal.FreeHGlobal(buf);
            PlayerPositionInfo.Text = msg;

            if (!ListUpdating)
            {
                ListUpdating = true;
            
            
                //Update entity table
                EntityList.Clear();
                for (int i = 0; i < 2000; i++)
                {
                    IntPtr entbuf = Marshal.AllocHGlobal(Marshal.SizeOf<EntityInfo>());
                    EntityInfo tmpentity;
                    GetEntityInfo(DashboardPage.apiObject, i, entbuf);
                    tmpentity = (EntityInfo)Marshal.PtrToStructure(entbuf, typeof(EntityInfo));
                    if (tmpentity.objectId > 0)
                    {
                        EntityList.Add(tmpentity);
                    }
                    else
                    {
                        EntityList.Add(new EntityInfo());
                    }
            
                    Marshal.FreeHGlobal(entbuf);
                }
            
            
                String entmsg = "";
                for (int j = 0; j < EntityList.Count; j++)
                {
                    if (EntityList[j].objectId > 0)
                    {
                        EntityInfo entity = EntityList[j];
            
                        if (entity.objectId > 0)
                        {
                            String cname = new string(entity.name);
            
                            entmsg += String.Format("{0}: Name is {1} - HP: {2}% - Type: {3} - Level: {4}"
                                + Environment.NewLine,
                                j, cname, entity.health, entity.type, entity.level);
                        }
            
                    }
            
                }
                EntityInfoTextBlock.Text = entmsg;
                ListUpdating = false;
            }

            // Forcing the CommandManager to raise the RequerySuggested event
            CommandManager.InvalidateRequerySuggested();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            autorun = !autorun;
            SetAutorun(DashboardPage.apiObject, autorun);
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {

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

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            if (!ListUpdating)
            {
                ListUpdating = true;
                EntityList.Clear();
                for (int i = 0; i < 2000; i++)
                {
                    IntPtr buf = Marshal.AllocHGlobal(Marshal.SizeOf<EntityInfo>());
                    EntityInfo tmpentity;
                    GetEntityInfo(DashboardPage.apiObject, i, buf);
                    tmpentity = (EntityInfo)Marshal.PtrToStructure(buf, typeof(EntityInfo));
                    if (tmpentity.objectId > 0)
                    {
                        EntityList.Add(tmpentity);
                    }
                    else
                    {
                        EntityList.Add(new EntityInfo());
                    }

                    Marshal.FreeHGlobal(buf);
                }


                String msg = "";
                for (int j = 0; j < EntityList.Count; j++)
                {
                    if (EntityList[j].objectId > 0)
                    {
                        EntityInfo entity = EntityList[j];

                        if (entity.objectId > 0)
                        {
                            String cname = new string(entity.name);

                            msg += String.Format("{0}: Name is {1} - HP: {2}% - Type: {3} - Level: {4}"
                                + Environment.NewLine,
                                j, cname, entity.health, entity.type, entity.level);
                        }

                    }

                }
                EntityInfoTextBlock.Text = msg;
                ListUpdating = false;
            }

        }

        private void HeadingSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //Change heading when the slider moves if changeHeading is toggled on, otherwise it will do nothing
            SetPlayerHeading(DashboardPage.apiObject, changeHeading, (short)HeadingSlider.Value);
        }

    
    }
}