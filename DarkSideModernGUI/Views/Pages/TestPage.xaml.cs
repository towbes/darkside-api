using Wpf.Ui.Common.Interfaces;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Interop;
using System.Windows.Threading;
using System.Windows.Input;
using System.Xml.Linq;
using System.Linq;
using System.Collections;
using System.Windows.Documents;
using System.Diagnostics;

//Commenting these out but leaving as a reminder for simulating keypresses if we want
//using WindowsInput.Native;
//using WindowsInput;

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

        //useSkill_t
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct Skill_t
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 72)] public String name;
            public int unknown1 { get; private set; }
        }
        //useSpell_t
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct Spell_t
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public String name;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 60)] public char[] unknown1;
        }
        //item_t
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct Item_t
        {
            public int id { get; private set; }
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 72)] public char[] unknown1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)] public String name;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 312)] public char[] unknown2;
        }
        //buff_t
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct Buff_t
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)] public String name;
            public int unknown1 { get; private set; }
            public int timeRemaining { get; private set; }
            public int slotNum { get; private set; }
            public int tooltipId { get; private set; }
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)] public char[] unknown2;
            public int buffId { get; private set; }
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)] public char[] unknown3;
        }

        //Playerinfo struct
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct PlayerInfo
        {
            public int health { get; private set; }
            public int power { get; private set; }
            public int endu { get; private set; }
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 150)] public Skill_t[] Skills;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 150)] public Spell_t[] Spells;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 75)] public Buff_t[] Buffs;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)] public Item_t[] Inventory;
        }
        //Playerinfo struct
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct TargetInfo
        {
            public int entOffset { get; private set; }
            public int health { get; private set; }
            public int color { get; private set; }
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 48)] public String name;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)] public String hasTarget;
        }

        //Chatbuffer
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct Chatbuffer
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)] public String chatLine;
        }


        [DllImport("darkside-api.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr CreateDarksideAPI();

        [DllImport("darkside-api.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr DisposeDarksideAPI(IntPtr pApiObject);

        [DllImport("darkside-api.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool InjectPid(IntPtr pApiObject, int pid);
        [DllImport("darkside-api.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetPid(IntPtr pApiObject);

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
        [DllImport("darkside-api.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GetPlayerInfo(IntPtr pApiObject, IntPtr lpBuffer);
        [DllImport("darkside-api.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GetTargetInfo(IntPtr pApiObject, IntPtr lpBuffer);
        [DllImport("darkside-api.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool SetTarget(IntPtr pApiObject, int entOffset);
        [DllImport("darkside-api.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool UseSkill(IntPtr pApiObject, int skillOffset);
        [DllImport("darkside-api.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool UseSpell(IntPtr pApiObject, int spellOffset);
        [DllImport("darkside-api.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GetChatline(IntPtr pApiObject, IntPtr lpBuffer);


        public static IntPtr apiObject;
        bool autorun = false;
        bool changeHeading = false;

        List<EntityInfo> EntityList = new List<EntityInfo>();
        List<String> strEntityList = new List<String>();
        //PartyList
        List<PartyMemberInfo> partyMemberList = new List<PartyMemberInfo>();
        List<String> strPartyList = new List<String>();
        List<String> strPlayerInfo = new List<String>();
        List<String> strPlayerPos = new List<String>();
        List<String> chatLog = new List<String>();

        DispatcherTimer dispatcherTimer;
        DispatcherTimer navTargetTimer;
        bool navRunning = false;

        bool loopRunning = false;

        PlayerPosition playerPos;
        TargetInfo targetInfo;


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
            if (!loopRunning)
            {
                dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
                dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
                //update ever 100ms
                dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
                dispatcherTimer.Start();
                loopRunning = true;
            } else
            {
                dispatcherTimer.Stop();
                loopRunning = false;
            }

        }


        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            //Update entity table
            EntityList.Clear();
            strEntityList.Clear();
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
            
                        entmsg = String.Format("{0}: Name is {1} - HP: {2}% - Type: {3} - Level: {4}",
                            j, cname, entity.health, entity.type, entity.level);
            
                        strEntityList.Add(entmsg);
                    }
                }
            }
            //EntityInfoTextBlock.Text = String.Join(Environment.NewLine, strEntityList);

            IntPtr chatbuf = Marshal.AllocHGlobal(Marshal.SizeOf<Chatbuffer>());
            Chatbuffer tmpChat;
            GetChatline(DashboardPage.apiObject, chatbuf);
            tmpChat = (Chatbuffer)Marshal.PtrToStructure(chatbuf, typeof(Chatbuffer));
            if (!String.IsNullOrEmpty(tmpChat.chatLine))
            {
                chatLog.Add(tmpChat.chatLine);
            }


            EntityInfoTextBlock.Text = String.Join(Environment.NewLine, chatLog);
            Marshal.FreeHGlobal(chatbuf);



            strPlayerPos.Clear();
            IntPtr buf = Marshal.AllocHGlobal(Marshal.SizeOf<PlayerPosition>());
            GetPlayerPosition(DashboardPage.apiObject, buf);
            playerPos = (PlayerPosition)Marshal.PtrToStructure(buf, typeof(PlayerPosition));
            string msg = String.Format("PlayerPosition:" + Environment.NewLine +
                "{0:0}, {1:0}, {2:0} {3:0}",
                playerPos.pos_x,
                playerPos.pos_y,
                playerPos.pos_z,
                playerPos.heading);
            strPlayerPos.Add(msg);
            // Updating the Label which displays the current second
            Marshal.FreeHGlobal(buf);
            strPlayerPos.Add("Target");
            //Target info
            //Size should be 0xdcd4
            IntPtr tInfobuf = Marshal.AllocHGlobal(Marshal.SizeOf<TargetInfo>());
            GetTargetInfo(DashboardPage.apiObject, tInfobuf);
            string tInfoMsg = "";
            targetInfo = (TargetInfo)Marshal.PtrToStructure(tInfobuf, typeof(TargetInfo));
            if (!String.IsNullOrEmpty(targetInfo.hasTarget))
            {
                tInfoMsg = String.Format("Ent: {0} - HP:{1} - Col:{2} - {3} - Lvl:{4} DistToTarget:{5:0.0} CalcHead:{6:0}",
                    targetInfo.entOffset,
                    targetInfo.health,
                    targetInfo.color,
                    targetInfo.name,
                    EntityList[targetInfo.entOffset].level,
                    DistanceToPoint(playerPos, EntityList[targetInfo.entOffset].pos_x, EntityList[targetInfo.entOffset].pos_y, EntityList[targetInfo.entOffset].pos_z),
                    GetDegreesHeading(playerPos, EntityList[targetInfo.entOffset].pos_x, EntityList[targetInfo.entOffset].pos_y));
            }
            else
            {
                tInfoMsg = "No target";
            }
            strPlayerPos.Add(tInfoMsg);
            //
            // Updating the Label which displays the current second
            Marshal.FreeHGlobal(tInfobuf);

            PlayerPositionInfo.Text = String.Join(Environment.NewLine, strPlayerPos);


            //PlayerInfo
            strPlayerInfo.Clear();
            //Size should be 0xdcd4
            IntPtr pInfobuf = Marshal.AllocHGlobal(Marshal.SizeOf<PlayerInfo>());
            GetPlayerInfo(DashboardPage.apiObject, pInfobuf);
            PlayerInfo playerInfo = (PlayerInfo)Marshal.PtrToStructure(pInfobuf, typeof(PlayerInfo));
            string pInfoMsg = String.Format("PlayerInfo:" + Environment.NewLine +
                "HP:{0:0} - Pow:{1:0} - Endu:{2:0}",
                playerInfo.health,
                playerInfo.power,
                playerInfo.endu);

            strPlayerInfo.Add(pInfoMsg);
            strPlayerInfo.Add("---Buffs---");
            //items
            for (int i = 0; i < 75; i++)
            {
                string pBuff = String.Format("{0}: {1} ", i, playerInfo.Buffs[i].name);
                if (playerInfo.Buffs[i].buffId > 0 )
                {
                    strPlayerInfo.Add(pBuff);
                }
            }
            strPlayerInfo.Add("---Items---");
            //items
            for (int i = 0; i < 40; i++)
            {
                string pItem = String.Format("{0}: {1} {2}", i, playerInfo.Inventory[i].id, playerInfo.Inventory[i].name);
                if (!String.IsNullOrEmpty(playerInfo.Inventory[i].name)) {
                    strPlayerInfo.Add(pItem);
                }
                
            }
            //
            Marshal.FreeHGlobal(pInfobuf);
            PlayerInfoText.Text = String.Join(Environment.NewLine, strPlayerInfo);

            partyMemberList.Clear();
            strPartyList.Clear();
            //party list
            for (int i = 0; i < 8; i++)
            {
                IntPtr pbuf = Marshal.AllocHGlobal(Marshal.SizeOf<PartyMemberInfo>());
                GetPartyMember(DashboardPage.apiObject, i, pbuf);
                PartyMemberInfo partyMember = (PartyMemberInfo)Marshal.PtrToStructure(pbuf, typeof(PartyMemberInfo));
                if (partyMember.hp_pct > 0)
                {
                    partyMemberList.Add(partyMember);
                }
                Marshal.FreeHGlobal(pbuf);

            }
            for (int j = 0; j < partyMemberList.Count; j++)
            {
                String cname = new string(partyMemberList[j].name);

                String pmsg = String.Format("{0} - HP: {1}% - Endu: {2}% - Pow: {3}%",
                    cname, partyMemberList[j].hp_pct, partyMemberList[j].endu_pct, partyMemberList[j].pow_pct);

                strPartyList.Add(pmsg);

                //Check if someone needs heal
                if (partyMemberList[j].hp_pct < 100)
                {
                    int targ = findEntityByName("Suzyqueue");
                    SetTarget(DashboardPage.apiObject, targ);
                    UseSkill(DashboardPage.apiObject, 17);
                }

            }

            MemberInfo.Text = String.Join(Environment.NewLine, strPartyList);




            // Forcing the CommandManager to raise the RequerySuggested event
            CommandManager.InvalidateRequerySuggested();
        }

        const UInt32 WM_KEYDOWN = 0x0100;
        const UInt32 WM_KEYUP = 0x0101;
        const int VK_NUMPAD5 = 0x31;

        [DllImport("user32.dll")]
        static extern bool PostMessage(IntPtr hWnd, UInt32 Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);


        private void Button_Click_Numpad5(object sender, RoutedEventArgs e)
        {
            //https://github.com/michaelnoonan/inputsimulator
            //Some test code with input simulator, but requires that the window come to foreground and is somewhat unreliable
            //InputSimulator sim = new InputSimulator();
            Process gameproc = Process.GetProcessById(GetPid(DashboardPage.apiObject));
            SetForegroundWindow(gameproc.MainWindowHandle);
            //InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_KEY_1)
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            partyMemberList.Clear();
            strPartyList.Clear();
            for (int i = 0; i < 8; i++)
            {
                IntPtr buf = Marshal.AllocHGlobal(Marshal.SizeOf<PartyMemberInfo>());
                GetPartyMember(DashboardPage.apiObject, i, buf);
                PartyMemberInfo partyMember = (PartyMemberInfo)Marshal.PtrToStructure(buf, typeof(PartyMemberInfo));
                if (partyMember.hp_pct > 0)
                {
                    partyMemberList.Add(partyMember);
                }
                Marshal.FreeHGlobal(buf);

            }
            for (int j = 0; j < partyMemberList.Count; j++)
            {
                String cname = new string(partyMemberList[j].name);

                String msg = String.Format("{0} - HP: {1}% - Endu: {2}% - Pow: {3}%",
                    cname, partyMemberList[j].hp_pct, partyMemberList[j].endu_pct, partyMemberList[j].pow_pct);

                strPartyList.Add(msg);
            }

            MemberInfo.Text = String.Join(Environment.NewLine, strPartyList);
        }
        int findEntityByName(String entName)
        {
            for (int i = 0; i < 2000; i++)
            {
                if (!String.IsNullOrEmpty(EntityList[i].name))
                {
                    if (String.Equals(entName, EntityList[i].name))
                    {
                        return i;
                    }
                }

            }
            return -1;
        }

        private void Button_Click_SetTarget(object sender, RoutedEventArgs e)
        {
            String tmp = SetTargetOffset.Text;
            int offset = -1;
            //If there are letters, get the offset by name
            if (!tmp.All(char.IsDigit)) {
                offset = findEntityByName(tmp);
            } else
            {
                offset = Int32.Parse(SetTargetOffset.Text);
            }
            SetTarget(DashboardPage.apiObject, offset);
        }

        private void Button_Click_UseSkill(object sender, RoutedEventArgs e)
        {
            UseSkill(DashboardPage.apiObject, Int32.Parse(UseSkillOffset.Text));
        }

        private void Button_Click_UseSpell(object sender, RoutedEventArgs e)
        {
            UseSpell(DashboardPage.apiObject, Int32.Parse(UseSpellOffset.Text));
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
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

            strEntityList.Clear();
                
            for (int j = 0; j < EntityList.Count; j++)
            {
                String msg = "";
                if (EntityList[j].objectId > 0)
                {
                    EntityInfo entity = EntityList[j];

                    if (entity.objectId > 0)
                    {
                        String cname = new string(entity.name);

                        msg = String.Format("{0}: Name is {1} - HP: {2}% - Type: {3} - Level: {4}",
                            j, cname, entity.health, entity.type, entity.level);
                        strEntityList.Add(msg);
                    }

                }

            }
            EntityInfoTextBlock.Text = String.Join(Environment.NewLine, strEntityList);

        }

        //private void Button_Click_PlayerInfo(object sender, RoutedEventArgs e)
        //{
        //    //Size should be 0xdcd4
        //    IntPtr pInfobuf = Marshal.AllocHGlobal(Marshal.SizeOf<PlayerInfo>());
        //    GetPlayerInfo(DashboardPage.apiObject, pInfobuf);
        //    PlayerInfo playerInfo = (PlayerInfo)Marshal.PtrToStructure(pInfobuf, typeof(PlayerInfo));
        //    string pInfoMsg = String.Format("PlayerInfo:" + Environment.NewLine +
        //        "HP:{0:0} - Pow:{1:0} - Endu:{2:0}",
        //        playerInfo.health,
        //        playerInfo.power,
        //        playerInfo.endu);
        //    //
        //    // Updating the Label which displays the current second
        //    Marshal.FreeHGlobal(pInfobuf);
        //    PlayerInfoText.Text = pInfoMsg;
        //}

        private void Button_Click_RunTarget(object sender, RoutedEventArgs e)
        {
            if (!navRunning)
            {
                navTargetTimer = new System.Windows.Threading.DispatcherTimer();
                navTargetTimer.Tick += new EventHandler(navTimer_Tick);
                //update ever 100ms
                navTargetTimer.Interval = new TimeSpan(0, 0, 0, 0, 50);
                navTargetTimer.Start();
                navRunning = true;
            }
            else
            {
                navTargetTimer.Stop();
                navRunning = false;
                SetAutorun(DashboardPage.apiObject, false);
                SetPlayerHeading(DashboardPage.apiObject, false, 0);
            }

        }

        int currentTarget = 0;

        private void navTimer_Tick(object sender, EventArgs e)
        {
            if (currentTarget == 0) {
                float stoppingDist = 40.0f;
                int trackerTarget = findEntityByName("Tracker");
                SetTarget(DashboardPage.apiObject, trackerTarget);

                float dist = DistanceToPoint(playerPos, EntityList[trackerTarget].pos_x, EntityList[trackerTarget].pos_y, EntityList[trackerTarget].pos_z);
                short newheading = GetGameHeading(playerPos, EntityList[trackerTarget].pos_x, EntityList[trackerTarget].pos_y);
                if (dist > stoppingDist)
                {
                    SetAutorun(DashboardPage.apiObject, true);
                    SetPlayerHeading(DashboardPage.apiObject, true, newheading);
                    dist = DistanceToPoint(playerPos, EntityList[trackerTarget].pos_x, EntityList[trackerTarget].pos_y, EntityList[trackerTarget].pos_z);
                    newheading = GetGameHeading(playerPos, EntityList[trackerTarget].pos_x, EntityList[trackerTarget].pos_y);
                } else
                {
                    SetAutorun(DashboardPage.apiObject, false);
                    SetPlayerHeading(DashboardPage.apiObject, false, 0);
                    currentTarget++;
                }

            } else if (currentTarget == 1)
            {
                float stoppingDist = 40.0f;
                int trackerTarget = findEntityByName("Nera");
                SetTarget(DashboardPage.apiObject, trackerTarget);

                float dist = DistanceToPoint(playerPos, EntityList[trackerTarget].pos_x, EntityList[trackerTarget].pos_y, EntityList[trackerTarget].pos_z);
                short newheading = GetGameHeading(playerPos, EntityList[trackerTarget].pos_x, EntityList[trackerTarget].pos_y);
                if (dist > stoppingDist)
                {
                    SetAutorun(DashboardPage.apiObject, true);
                    SetPlayerHeading(DashboardPage.apiObject, true, newheading);
                    dist = DistanceToPoint(playerPos, EntityList[trackerTarget].pos_x, EntityList[trackerTarget].pos_y, EntityList[trackerTarget].pos_z);
                    newheading = GetGameHeading(playerPos, EntityList[trackerTarget].pos_x, EntityList[trackerTarget].pos_y);
                } else
                {
                    SetAutorun(DashboardPage.apiObject, false);
                    SetPlayerHeading(DashboardPage.apiObject, false, 0);
                    currentTarget++;
                }

            } else
            {
                SetAutorun(DashboardPage.apiObject, false);
                SetPlayerHeading(DashboardPage.apiObject, false, 0);
            }

        }

        private float DistanceToPoint(PlayerPosition playerPos, float targX, float targY, float targZ)
        {
            float dist = (float)(Math.Sqrt((playerPos.pos_x - targX) * (playerPos.pos_x - targX) + (playerPos.pos_y - targY) * (playerPos.pos_y - targY)));
            return (float)dist;
        }
    
        private short GetDegreesHeading(PlayerPosition playerPos, float targX, float targY)
        {
            float xDiff = playerPos.pos_x - targX;
            float yDiff = playerPos.pos_y - targY;
            //https://stackoverflow.com/questions/70511665/how-to-calculate-rotation-needed-to-face-an-object
            return (short)((Math.Atan2(yDiff, xDiff) * 180.0 / Math.PI + 630.0) % 360.0);
        }

        //DOL heading func
        //https://github.com/Dawn-of-Light/DOLSharp/blob/9af87af011497c3fda852559b01a269c889b162e/GameServer/world/Point2D.cs
        public short GetGameHeading(PlayerPosition playerPos, float posx, float posy)
        {
            float dx = posx - playerPos.pos_x;
            float dy = posy - playerPos.pos_y;

            double heading = Math.Atan2(-dx, dy) * (180.0 / Math.PI) * (4096.0 / 360.0);

            if (heading < 0)
                heading += 4096;

            return (short)heading;
        }

    }
}