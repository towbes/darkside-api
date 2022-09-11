using DarkSideModernGUI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Converters;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using Wpf.Ui.Common.Interfaces;
using static DarkSideModernGUI.Helpers.DarksideGameAPI;
using static DarkSideModernGUI.Helpers.Movement;
using static DarkSideModernGUI.Views.Pages.ClassSettingsPage;

namespace DarkSideModernGUI.Views.Pages
{
    /// <summary>
    /// Interaction logic for TestPage.xaml
    /// </summary>
    public partial class ClassSettingsPage : INavigableView<ViewModels.ClassSettingsViewModel>
    {
        DispatcherTimer dispatcherTimer;
        bool loopRunning = false;
        List<String> loadedList = new List<string>();

        DispatcherTimer stickTimer;
        bool stickRunning = false;

        bool globalsRunning = false;

        string leaderName = "Asmoe";
        string readyName = "Suzuha";

        static public Dictionary<string, int> charNames = new Dictionary<string, int>();
        static public Dictionary<int, CharGlobals> CharGlobalDict = new Dictionary<int, CharGlobals>();

        //Global bool to trigger dragon script on/off
        bool dragonRunning = false;

        public ViewModels.ClassSettingsViewModel ViewModel
        {
            get;
        }

        public struct CharGlobals
        {
            //Api Object
            public IntPtr apiObject;
            //alloc buffers
            public IntPtr entbuf {get;set;}//= Marshal.AllocHGlobal(Marshal.SizeOf<EntityList>());
            public IntPtr chatbuf {get;set;}//= Marshal.AllocHGlobal(Marshal.SizeOf<Chatbuffer>());
            public IntPtr playerPosbuf {get;set;}//= Marshal.AllocHGlobal(Marshal.SizeOf<PlayerPosition>());
            public IntPtr tInfobuf {get;set;}//= Marshal.AllocHGlobal(Marshal.SizeOf<TargetInfo>());
            public IntPtr pInfobuf {get;set;}//= Marshal.AllocHGlobal(Marshal.SizeOf<PlayerInfo>());
            public IntPtr pbuf{get;set;}//= Marshal.AllocHGlobal(Marshal.SizeOf<PartyMemberInfo>());
            public IntPtr stickTargplayerPosbuf { get; set; }// = Marshal.AllocHGlobal(Marshal.SizeOf<PlayerPosition>());
            //Global lists
            public List<EntityInfo> EntityList; //= new List<EntityInfo>();
            public EntityList entList; // = new EntityList();
            public List<String> chatLog; //= new List<String>();
            public List<PartyMemberInfo> partyMemberList; //= new List<PartyMemberInfo>();
            //Global info
            public PlayerPosition playerPos;
            public TargetInfo targetInfo;
            public PlayerInfo playerInfo;
            public Chatbuffer chatLine;
            
        }



        public ClassSettingsPage(ViewModels.ClassSettingsViewModel viewModel)
        {
            ViewModel = viewModel;

            InitializeComponent();
            dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            //update ever 100ms
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            dispatcherTimer.Start();
            loopRunning = true;

            //PlayerInfo
            loadedList.Clear();
            charNames.Clear();
            loadedList.Add("Injected Chars:");
            int i = 0;
            IntPtr pInfobuf = Marshal.AllocHGlobal(Marshal.SizeOf<PlayerInfo>());
            foreach (DashboardPage.GameDLL proc in DashboardPage.gameprocs)
            {
                
                GetPlayerInfo(proc.apiObject, pInfobuf);
                PlayerInfo playerInfo = (PlayerInfo)Marshal.PtrToStructure(pInfobuf, typeof(PlayerInfo));
                string plyrName = new string(playerInfo.name);
                string className = new string(playerInfo.className);
                string pInfoMsg = String.Format("{5}: Name:{3} - Class:{4} - HP:{0:0} - Pow:{1:0} - Endu:{2:0}",
                    playerInfo.health,
                    playerInfo.power,
                    playerInfo.endu,
                    plyrName,
                    className,
                    i);
                if (!charNames.ContainsKey(plyrName))
                {
                    charNames.Add(plyrName, proc.procId);
                }
                
                if (!CharGlobalDict.ContainsKey(proc.procId))
                {
                    CharGlobalDict.Add(proc.procId, new CharGlobals()
                    {
                        //alloc buffers
                        entbuf = Marshal.AllocHGlobal(Marshal.SizeOf<EntityList>()),
                        chatbuf = Marshal.AllocHGlobal(Marshal.SizeOf<Chatbuffer>()),
                        playerPosbuf = Marshal.AllocHGlobal(Marshal.SizeOf<PlayerPosition>()),
                        tInfobuf = Marshal.AllocHGlobal(Marshal.SizeOf<TargetInfo>()),
                        pInfobuf = Marshal.AllocHGlobal(Marshal.SizeOf<PlayerInfo>()),
                        pbuf = Marshal.AllocHGlobal(Marshal.SizeOf<PartyMemberInfo>()),
                        //global lists
                        EntityList = new List<EntityInfo>(),
                        entList = new EntityList(),
                        chatLog = new List<String>(),
                        partyMemberList = new List<PartyMemberInfo>(),
                        playerPos = new PlayerPosition(),
                        targetInfo = new TargetInfo(),
                        playerInfo = new PlayerInfo(),
                        apiObject = proc.apiObject,
                        chatLine = new Chatbuffer(),
                    });
                }

                loadedList.Add(pInfoMsg);
                
                
            }
            Marshal.FreeHGlobal(pInfobuf);


            InjectedInfo.Text = String.Join(Environment.NewLine, loadedList);

        }

        static public void ReleasecharGlobals(CharGlobals charBuffer)
        {
            Marshal.FreeHGlobal(charBuffer.entbuf);
            Marshal.FreeHGlobal(charBuffer.chatbuf);
            Marshal.FreeHGlobal(charBuffer.playerPosbuf);
            Marshal.FreeHGlobal(charBuffer.stickTargplayerPosbuf);
            Marshal.FreeHGlobal(charBuffer.tInfobuf);
            Marshal.FreeHGlobal(charBuffer.pInfobuf);
            Marshal.FreeHGlobal(charBuffer.pbuf);
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {

            //PlayerInfo
            loadedList.Clear();
            loadedList.Add("Injected Chars:");
            int i = 0;
            IntPtr pInfobuf = Marshal.AllocHGlobal(Marshal.SizeOf<PlayerInfo>());
            foreach (DashboardPage.GameDLL proc in DashboardPage.gameprocs)
            {

                GetPlayerInfo(proc.apiObject, pInfobuf);
                PlayerInfo playerInfo = (PlayerInfo)Marshal.PtrToStructure(pInfobuf, typeof(PlayerInfo));
                string plyrName = new string(playerInfo.name);
                string className = new string(playerInfo.className);
                string pInfoMsg = String.Format("{5}: Name:{3} - Class:{4} - HP:{0:0} - Pow:{1:0} - Endu:{2:0}",
                    playerInfo.health,
                    playerInfo.power,
                    playerInfo.endu,
                    plyrName,
                    className,
                    i);
                if (!charNames.ContainsKey(plyrName))
                {
                    charNames.Add(plyrName, proc.procId);
                }

                if (!CharGlobalDict.ContainsKey(proc.procId))
                {
                    CharGlobalDict.Add(proc.procId, new CharGlobals()
                    {
                        //alloc buffers
                        entbuf = Marshal.AllocHGlobal(Marshal.SizeOf<EntityList>()),
                        chatbuf = Marshal.AllocHGlobal(Marshal.SizeOf<Chatbuffer>()),
                        playerPosbuf = Marshal.AllocHGlobal(Marshal.SizeOf<PlayerPosition>()),
                        tInfobuf = Marshal.AllocHGlobal(Marshal.SizeOf<TargetInfo>()),
                        pInfobuf = Marshal.AllocHGlobal(Marshal.SizeOf<PlayerInfo>()),
                        pbuf = Marshal.AllocHGlobal(Marshal.SizeOf<PartyMemberInfo>()),
                        //global lists
                        EntityList = new List<EntityInfo>(),
                        entList = new EntityList(),
                        chatLog = new List<String>(),
                        partyMemberList = new List<PartyMemberInfo>(),
                        playerPos = new PlayerPosition(),
                        targetInfo = new TargetInfo(),
                        playerInfo = new PlayerInfo(),
                        chatLine = new Chatbuffer()
                    });
                }
                loadedList.Add(pInfoMsg);
                //

            }
            Marshal.FreeHGlobal(pInfobuf);
            InjectedInfo.Text = String.Join(Environment.NewLine, loadedList);

            // Forcing the CommandManager to raise the RequerySuggested event
            CommandManager.InvalidateRequerySuggested();
        }

        private void Button_Click_GlobalLoop(object sender, RoutedEventArgs e)
        {
            if (!globalsRunning)
            {
                btnGlobalLoop.Content = "Globals Looping";
                globalsRunning = true;
                foreach (DashboardPage.GameDLL proc in DashboardPage.gameprocs)
                {
                    //https://stackoverflow.com/questions/14854878/creating-new-thread-with-method-with-parameter
                    Thread newThread = new Thread(() => UpdateGlobals(proc.procId));
                    newThread.Start();

                }
                btnGlobalLoop.Background = new SolidColorBrush(Colors.Green);
            }
            else
            {
                btnGlobalLoop.Content = "Globals Stopped";
                globalsRunning = false;
                btnGlobalLoop.Background = new SolidColorBrush(Colors.Orange);
            }
        }

        private void UpdateGlobals(int procId)
        {
            //Copy the globals to a temp object to mod
            CharGlobals modGlobals = CharGlobalDict[procId];

            IntPtr apiObject = modGlobals.apiObject;

            IntPtr entbuf = modGlobals.entbuf;
            IntPtr chatbuf = modGlobals.chatbuf;
            IntPtr playerPosbuf = modGlobals.playerPosbuf;
            IntPtr tInfobuf = modGlobals.tInfobuf;
            IntPtr pInfobuf = modGlobals.pInfobuf;
            IntPtr pbuf = modGlobals.pbuf;

            if (GetEntityList(apiObject, entbuf))
            {
                modGlobals.entList = (EntityList)Marshal.PtrToStructure(entbuf, typeof(EntityList));
            }

            //Update entity table
            modGlobals.EntityList.Clear();
            for (int i = 0; i < 2000; i++)
            {
                EntityInfo tmpentity;
                //tmpentity = (EntityInfo)Marshal.PtrToStructure(entbuf, typeof(EntityInfo));
                tmpentity = modGlobals.entList.EntList[i];
                modGlobals.EntityList.Add(tmpentity);

            }


            GetChatline(apiObject, chatbuf);
            modGlobals.chatLine = (Chatbuffer)Marshal.PtrToStructure(chatbuf, typeof(Chatbuffer));
            if (!String.IsNullOrEmpty(modGlobals.chatLine.chatLine))
            {
                //modGlobals.chatLog.Add(modGlobals.chatLine.chatLine);
            }

            //Current player position
            GetPlayerPosition(apiObject, playerPosbuf);
            modGlobals.playerPos = (PlayerPosition)Marshal.PtrToStructure(playerPosbuf, typeof(PlayerPosition));

            //Target info

            GetTargetInfo(apiObject, tInfobuf);
            modGlobals.targetInfo = (TargetInfo)Marshal.PtrToStructure(tInfobuf, typeof(TargetInfo));
            // Updating the Label which displays the current second

            //PlayerInfo

            GetPlayerInfo(apiObject, pInfobuf);
            modGlobals.playerInfo = (PlayerInfo)Marshal.PtrToStructure(pInfobuf, typeof(PlayerInfo));


            modGlobals.partyMemberList.Clear();
            //party list
            for (int i = 0; i < 8; i++)
            {
                GetPartyMember(apiObject, i, pbuf);
                PartyMemberInfo partyMember = (PartyMemberInfo)Marshal.PtrToStructure(pbuf, typeof(PartyMemberInfo));
                if (partyMember.hp_pct > 0)
                {
                    modGlobals.partyMemberList.Add(partyMember);
                }

            }
            //Copy the modded object back to dict
            CharGlobalDict[procId] = modGlobals;

        }

        private void Button_Click_RunTarget(object sender, RoutedEventArgs e)
        {
            int leaderPid = 0;
            if (charNames.ContainsKey(leaderName))
                leaderPid = charNames[leaderName];

            DashboardPage.GameDLL leaderProc = DashboardPage.gameprocs.FirstOrDefault(x => x.procId == leaderPid);


            foreach (DashboardPage.GameDLL proc in DashboardPage.gameprocs)
            {
                //https://stackoverflow.com/questions/14854878/creating-new-thread-with-method-with-parameter
                //Thread newThread = new Thread(()=>runDemo(proc.apiObject));
                //newThread.Start();
                getBuffs(proc.procId);
            }
        }

        private void runDemo(IntPtr apiObject)
        {
            bool destinationReached = false;
            List<EntityInfo> EntityList = new List<EntityInfo>();
            EntityList entList = new EntityList();
            List<String> chatLog = new List<String>();
            List<PartyMemberInfo> partyMemberList = new List<PartyMemberInfo>();
            PlayerPosition playerPos;
            TargetInfo targetInfo;
            PlayerInfo playerInfo;

            int currentTarget = 0;

            //alloc buffers
            IntPtr entbuf = Marshal.AllocHGlobal(Marshal.SizeOf<EntityList>());
            IntPtr chatbuf = Marshal.AllocHGlobal(Marshal.SizeOf<Chatbuffer>());
            IntPtr playerPosbuf = Marshal.AllocHGlobal(Marshal.SizeOf<PlayerPosition>());
            IntPtr tInfobuf = Marshal.AllocHGlobal(Marshal.SizeOf<TargetInfo>());
            IntPtr pInfobuf = Marshal.AllocHGlobal(Marshal.SizeOf<PlayerInfo>());
            IntPtr pbuf = Marshal.AllocHGlobal(Marshal.SizeOf<PartyMemberInfo>());

            while (!destinationReached)
            {
                if (GetEntityList(apiObject, entbuf))
                {
                    entList = (EntityList)Marshal.PtrToStructure(entbuf, typeof(EntityList));
                }

                //Update entity table
                EntityList.Clear();
                for (int i = 0; i < 2000; i++)
                {
                    EntityInfo tmpentity;
                    //tmpentity = (EntityInfo)Marshal.PtrToStructure(entbuf, typeof(EntityInfo));
                    tmpentity = entList.EntList[i];
                    EntityList.Add(tmpentity);

                }

                Chatbuffer tmpChat;
                GetChatline(apiObject, chatbuf);
                tmpChat = (Chatbuffer)Marshal.PtrToStructure(chatbuf, typeof(Chatbuffer));
                if (!String.IsNullOrEmpty(tmpChat.chatLine))
                {
                    //chatLog.Add(tmpChat.chatLine);
                }
 
                GetPlayerPosition(apiObject, playerPosbuf);
                playerPos = (PlayerPosition)Marshal.PtrToStructure(playerPosbuf, typeof(PlayerPosition));
                // Updating the Label which displays the current second
                
                //Target info
                //Size should be 0xdcd4
                
                GetTargetInfo(apiObject, tInfobuf);
                targetInfo = (TargetInfo)Marshal.PtrToStructure(tInfobuf, typeof(TargetInfo));
                // Updating the Label which displays the current second

                //PlayerInfo
                
                GetPlayerInfo(apiObject, pInfobuf);
                playerInfo = (PlayerInfo)Marshal.PtrToStructure(pInfobuf, typeof(PlayerInfo));
                

                partyMemberList.Clear();
                //party list
                for (int i = 0; i < 8; i++)
                {  
                    GetPartyMember(apiObject, i, pbuf);
                    PartyMemberInfo partyMember = (PartyMemberInfo)Marshal.PtrToStructure(pbuf, typeof(PartyMemberInfo));
                    if (partyMember.hp_pct > 0)
                    {
                        partyMemberList.Add(partyMember);
                    }

                }
                //for (int j = 0; j < partyMemberList.Count; j++)
                //{
                //    String cname = new string(partyMemberList[j].name);
                //
                //    //Check if someone needs heal
                //    if (partyMemberList[j].hp_pct < 100)
                //    {
                //        int targ = findEntityByName(EntityList, cname);
                //        SetTarget(DashboardPage.apiObject, targ);
                //        UseSkill(DashboardPage.apiObject, 17);
                //    }
                //
                //}


                if (currentTarget == 0)
                {
                    float stoppingDist = 40.0f;
                    int trackerTarget = findEntityByName(EntityList, "Zander");
                    SetTarget(apiObject, trackerTarget);

                    float dist = DistanceToPoint(playerPos, EntityList[trackerTarget].pos_x, EntityList[trackerTarget].pos_y);
                    short newheading = GetGameHeading(playerPos, EntityList[trackerTarget].pos_x, EntityList[trackerTarget].pos_y);
                    if (dist > stoppingDist)
                    {
                        SetAutorun(apiObject, true);
                        SetPlayerHeading(apiObject, true, newheading);
                        dist = DistanceToPoint(playerPos, EntityList[trackerTarget].pos_x, EntityList[trackerTarget].pos_y);
                        newheading = GetGameHeading(playerPos, EntityList[trackerTarget].pos_x, EntityList[trackerTarget].pos_y);
                    }
                    else
                    {
                        SetAutorun(apiObject, false);
                        SetPlayerHeading(apiObject, false, 0);
                        currentTarget++;
                    }

                }
                else if (currentTarget == 1)
                {
                    float stoppingDist = 40.0f;
                    int trackerTarget = findEntityByName(EntityList, "Drucill");
                    SetTarget(apiObject, trackerTarget);

                    float dist = DistanceToPoint(playerPos, EntityList[trackerTarget].pos_x, EntityList[trackerTarget].pos_y);
                    short newheading = GetGameHeading(playerPos, EntityList[trackerTarget].pos_x, EntityList[trackerTarget].pos_y);
                    if (dist > stoppingDist)
                    {
                        SetAutorun(apiObject, true);
                        SetPlayerHeading(apiObject, true, newheading);
                        dist = DistanceToPoint(playerPos, EntityList[trackerTarget].pos_x, EntityList[trackerTarget].pos_y);
                        newheading = GetGameHeading(playerPos, EntityList[trackerTarget].pos_x, EntityList[trackerTarget].pos_y);
                    }
                    else
                    {
                        SetAutorun(apiObject, false);
                        SetPlayerHeading(apiObject, false, 0);
                        destinationReached = true;
                        currentTarget++;
                    }

                }
                else
                {
                    SetAutorun(apiObject, false);
                    SetPlayerHeading(apiObject, false, 0);
                }
                Thread.Sleep(100);
            }


            Marshal.FreeHGlobal(entbuf);
            Marshal.FreeHGlobal(chatbuf);
            Marshal.FreeHGlobal(playerPosbuf);
            Marshal.FreeHGlobal(tInfobuf);
            Marshal.FreeHGlobal(pInfobuf);
            Marshal.FreeHGlobal(pbuf);
        }

        private void Button_Click_Stick(object sender, RoutedEventArgs e)
        {
            int stickPid = 0;
            int readyPid = 0;
            if (charNames.ContainsKey(leaderName))
                stickPid = charNames[leaderName];
            if (charNames.ContainsKey(readyName))
                readyPid = charNames[readyName];

            if (!stickRunning)
            {
                StickButton.Content = "Stick is On";
                stickRunning = true;
                foreach (DashboardPage.GameDLL proc in DashboardPage.gameprocs)
                {
                    if (proc.procId != readyPid)
                    {
                        //https://stackoverflow.com/questions/14854878/creating-new-thread-with-method-with-parameter
                        Thread newThread = new Thread(() => stickFunc(proc.procId));
                        newThread.Start();
                    }

                }
                StickButton.Background = new SolidColorBrush(Colors.Green);
            }
            else
            {
                StickButton.Content = "Stick is Off";
                stickRunning = false;
                StickButton.Background = new SolidColorBrush(Colors.Orange);
            }

        }

        private void Button_Click_BattleLoc(object sender, RoutedEventArgs e)
        {
            int stickPid = 0;
            int readyPid = 0;
            if (charNames.ContainsKey(leaderName))
                stickPid = charNames[leaderName];
            if (charNames.ContainsKey(readyName))
                readyPid = charNames[readyName];

            foreach (DashboardPage.GameDLL proc in DashboardPage.gameprocs)
            {
                if (proc.procId != readyPid)
                {
                    //https://stackoverflow.com/questions/14854878/creating-new-thread-with-method-with-parameter
                    Thread newThread = new Thread(() => battleLocFunc(proc.procId));
                    newThread.Start();
                }

            }

        }

        private void Button_Click_ResetLoc(object sender, RoutedEventArgs e)
        {
            int stickPid = 0;
            int readyPid = 0;
            if (charNames.ContainsKey(leaderName))
                stickPid = charNames[leaderName];
            if (charNames.ContainsKey(readyName))
                readyPid = charNames[readyName];


            foreach (DashboardPage.GameDLL proc in DashboardPage.gameprocs)
            {
                if (proc.procId != readyPid)
                {
                    //https://stackoverflow.com/questions/14854878/creating-new-thread-with-method-with-parameter
                    Thread newThread = new Thread(() => resetLocFunc(proc.procId));
                    newThread.Start();
                }

            }

        }

        private void Button_Click_FightDragon(object sender, RoutedEventArgs e)
        {
            int stickPid = 0;
            int readyPid = 0;
            if (charNames.ContainsKey(leaderName))
                stickPid = charNames[leaderName];
            if (charNames.ContainsKey(readyName))
                readyPid = charNames[readyName];

            DashboardPage.GameDLL stickProc = DashboardPage.gameprocs.FirstOrDefault(x => x.procId == stickPid);

            if (!dragonRunning)
            {
                btnFightDragon.Content = "Dragon is On";
                dragonRunning = true;
                foreach (DashboardPage.GameDLL proc in DashboardPage.gameprocs)
                {
                    if (proc.procId != readyPid)
                    {
                        //https://stackoverflow.com/questions/14854878/creating-new-thread-with-method-with-parameter
                        Thread newThread = new Thread(() => battleFunc(proc.procId));
                        newThread.Start();
                    }

                }
                btnFightDragon.Background = new SolidColorBrush(Colors.Green);
            }
            else
            {
                btnFightDragon.Content = "Dragon is Off";
                dragonRunning = false;
                btnFightDragon.Background = new SolidColorBrush(Colors.Orange);
                //Reset autorun and heading so we don't get stuck
                foreach (DashboardPage.GameDLL proc in DashboardPage.gameprocs)
                {
                    if (proc.procId != readyPid)
                    {
                        CharGlobals finalGlobals = CharGlobalDict[proc.procId];
                        SetPlayerHeading(finalGlobals.apiObject, false, 0);
                        SetAutorun(finalGlobals.apiObject, false);
                    }

                }

            }

        }

        private void battleLocFunc(int procId)
        {
            bool isMoving = true;

            float xloc = 0;
            float yloc = 0;
            short finalheading = 0;

            while (isMoving)
            {
                UpdateGlobals(procId);

                CharGlobals charGlobals = CharGlobalDict[procId];
                string plyrName = new string(charGlobals.playerInfo.name);

                string className = new string(charGlobals.playerInfo.className);

                //Set up locs
                if (className.Contains("Paladin"))
                {
                    xloc = 39102f;
                    yloc = 58785f;
                    finalheading = 181;
                }
                else
                {
                    xloc = 38678f;
                    yloc = 59418f;
                    finalheading = 73;
                }

                float stoppingDist = 30.0f;
                //currentTarget = findEntityByName(EntityList, "Asmoe");
                //SetTarget(charGlobals.apiObject, currentTarget);


                float dist = DistanceToPoint(charGlobals.playerPos, xloc, yloc);
                short newheading = GetGameHeading(charGlobals.playerPos, xloc, yloc);
                if (dist > stoppingDist)
                {
                    SetAutorun(charGlobals.apiObject, true);
                    SetPlayerHeading(charGlobals.apiObject, true, newheading);
                    //dist = DistanceToPoint(charGlobals.playerPos, stickTargPos.pos_x, stickTargPos.pos_y, stickTargPos.pos_z);
                    //newheading = GetGameHeading(charGlobals.playerPos, stickTargPos.pos_x, stickTargPos.pos_y);
                }
                else
                {
                    SetPlayerHeading(charGlobals.apiObject, true, ConvertDirHeading(finalheading));
                    //SetPlayerHeading(charGlobals.apiObject, false, 0);
                    SetAutorun(charGlobals.apiObject, false);
                    //This isn't turning them the direction of the leader for some reason
                    //
                    isMoving = false;
                }

                Thread.Sleep(100);
            }
            CharGlobals finalGlobals = CharGlobalDict[procId];
            SetPlayerHeading(finalGlobals.apiObject, false, 0);
            SetAutorun(finalGlobals.apiObject, false);
            
        }

        private void resetLocFunc(int procId)
        {
            bool isMoving = true;

            float xloc = 0;
            float yloc = 0;
            short finalheading = 0;

            while (isMoving)
            {
                UpdateGlobals(procId);


                CharGlobals charGlobals = CharGlobalDict[procId];
                string plyrName = new string(charGlobals.playerInfo.name);

                string className = new string(charGlobals.playerInfo.className);

                //Set up locs
                if (className.Contains("Paladin"))
                {
                    xloc = 37134f;
                    yloc = 59840f;
                    finalheading = 71;
                }
                else
                {
                    xloc = 37134f;
                    yloc = 59840f;
                    finalheading = 71;
                }

                float stoppingDist = 30.0f;
                //currentTarget = findEntityByName(EntityList, "Asmoe");
                //SetTarget(charGlobals.apiObject, currentTarget);


                float dist = DistanceToPoint(charGlobals.playerPos, xloc, yloc);
                short newheading = GetGameHeading(charGlobals.playerPos, xloc, yloc);
                if (dist > stoppingDist)
                {
                    SetAutorun(charGlobals.apiObject, true);
                    SetPlayerHeading(charGlobals.apiObject, true, newheading);
                    //dist = DistanceToPoint(charGlobals.playerPos, stickTargPos.pos_x, stickTargPos.pos_y, stickTargPos.pos_z);
                    //newheading = GetGameHeading(charGlobals.playerPos, stickTargPos.pos_x, stickTargPos.pos_y);
                }
                else
                {
                    SetPlayerHeading(charGlobals.apiObject, true, ConvertDirHeading(finalheading));
                    //SetPlayerHeading(charGlobals.apiObject, false, 0);
                    SetAutorun(charGlobals.apiObject, false);
                    //This isn't turning them the direction of the leader for some reason
                    //
                    isMoving = false;
                }

                Thread.Sleep(100);
            }
            CharGlobals finalGlobals = CharGlobalDict[procId];
            SetPlayerHeading(finalGlobals.apiObject, false, 0);
            SetAutorun(finalGlobals.apiObject, false);

        }

        private void battleFunc(int procId)
        {
            int threadSleep = 75; // milliseconds
            int castSleep = 0; // milliseconds

            //This is a countdown that gets reset after a cast to prevent spell spamming
            int timeoutMax = 20;
            int castTimeout = 0;

            //global flag
            bool fightStarted = false;


            //Tank abilities/variables
            string tankClass = "Paladin";
            string tankMeleeTaunt = "Rile";
            string tankSpellTaunt = "Infuriate";
            string tankArmorBuff = "Aura of Salvation";
            //chants
            string tankStrChant = "Greater Battle Zeal";
            bool tankStrOn = false;
            string tankEndChant = "Chant of Perseverance";
            bool tankEndOn = false;
            string tankAfChant = "Crusader's Mantle";
            bool tankAfOn = false;
            string tankAblChant = "Barrier of Temperance";
            bool tankAblOn = false;
            //Implement two tank waypoints for dodging clouds
            bool cloudNear = false;
            Pos_2d[] tankPoints = new Pos_2d[2];
            int currentTankPoint = 0;
            tankPoints[0].x = 39102f;
            tankPoints[0].y = 58785f;
            tankPoints[1].x = 39407f;
            tankPoints[1].y = 58971f;

            //Damage caster
            string dmgClass = "Spiritmaster";
            //pet and buffs
            string petSpell = "Spirit Warrior";
            string shieldSpell = "Superior Suppressive Barrier";
            string absorbSpell = "Suppressive Buffer";
            string grpAbsorb = "Shield of the Einherjar";
            //nuke
            string dmgNuke = "Spirit Annihilation";

            //Debuffer
            string dbfClass = "Eldritch";
            string dbfShieldSpell = "Supreme Powerguard";
            string dbfAbsorbSpell = "Barrier of Power";
            string dbfSpell = "Annihilate Soul";

            //Bard
            string brdClass = "Bard";
            //songs
            string speedSong = "Clear Horizon";
            bool brdSpeedOn = false;
            string powSong = "Rhyme of Creation";
            bool brdPowOn = false;
            string endSong = "Rhythm of the Cosmos";
            bool brdEndOn = false;
            string healSong = "Euphony of Healing";
            bool brdHealOn = false;
            //buff
            string ablBuff = "Battlesong of Apotheosis";
            //heals
            int smallHealPct = 90;
            int bigHealPct = 70;
            string brdSmallHeal = "Apotheosis";
            string brdBigHeal = "Major Apotheosis";
            string brdGrpHeal = "Group Apotheosis";

            //Healer
            string hlrClass = "Druid";
            //resist buff
            string hlrResistBuff = "Warmth of the Bear";
            bool hlrBuffOn = false;
            //heals
            string hlrSmallHeal = "Apotheosis";
            string hlrBigHeal = "Major Renascence";
            string hlrGrpHeal = "Group Apotheosis";

            while (dragonRunning)
            {
                UpdateGlobals(procId);

                CharGlobals charGlobals = CharGlobalDict[procId];

                int decimusOffset = findEntityByName(charGlobals.EntityList, "Decimus");

                //Check chat for selan message
                //******the chat check doesn't seem to be working but the decimus offset does
                if (charGlobals.chatLine.chatLine.Contains("Golestandt") || decimusOffset == 0)
                {
                    fightStarted = true;
                }

                //Check if we're casting and have more than castSleep left
                if (charGlobals.EntityList[charGlobals.playerInfo.entListIndex].castCountdown > castSleep)
                {
                    //isCasting = true;
                }
                else
                {
                    //isCasting = false;
                    castTimeout -= 1;
                    if (castTimeout < 0)
                        castTimeout = 0;
                }


                if (!fightStarted && castTimeout == 0)
                {
                    if (charGlobals.playerInfo.className.Contains(tankClass))
                    {
                        if (!HasBuffByName(charGlobals.playerInfo.Buffs, tankArmorBuff))
                            UseSkillByName(charGlobals.apiObject, charGlobals.playerInfo.Skills, tankArmorBuff);
                        if (!tankStrOn && !HasBuffByName(charGlobals.playerInfo.Buffs, tankStrChant))
                        {
                            tankStrOn = true;
                            UseSkillByName(charGlobals.apiObject, charGlobals.playerInfo.Skills, tankStrChant);
                        }
                        else if (!tankEndOn && !HasBuffByName(charGlobals.playerInfo.Buffs, tankEndChant))
                        {
                            tankEndOn = true;
                            UseSkillByName(charGlobals.apiObject, charGlobals.playerInfo.Skills, tankEndChant);
                        }
                        else if (!tankAfOn && !HasBuffByName(charGlobals.playerInfo.Buffs, tankAfChant))
                        {
                            tankAfOn = true;
                            UseSkillByName(charGlobals.apiObject, charGlobals.playerInfo.Skills, tankAfChant);
                        }
                        else if (!tankAblOn && !HasBuffByName(charGlobals.playerInfo.Buffs, tankAblChant))
                        {
                            tankAblOn = true;
                            UseSkillByName(charGlobals.apiObject, charGlobals.playerInfo.Skills, tankAblChant);
                        }
                            

                    }
                    else if (charGlobals.playerInfo.className.Contains(dmgClass))
                    {
                        //Pet spell
                        if(charGlobals.playerInfo.petEntIndex == 0)
                            UseSpellByName(charGlobals.apiObject, charGlobals.playerInfo.SpellLines, petSpell);
                        //Shield
                        else if (!HasBuffByName(charGlobals.playerInfo.Buffs, shieldSpell))
                            UseSpellByName(charGlobals.apiObject, charGlobals.playerInfo.SpellLines, shieldSpell);
                        //Absorb
                        else if (!HasBuffByName(charGlobals.playerInfo.Buffs, absorbSpell))
                            UseSpellByName(charGlobals.apiObject, charGlobals.playerInfo.SpellLines, absorbSpell);
                        //Group Absorb
                        else if (!HasBuffByName(charGlobals.playerInfo.Buffs, grpAbsorb))
                            UseSpellByName(charGlobals.apiObject, charGlobals.playerInfo.SpellLines, grpAbsorb);
                    }
                    else if (charGlobals.playerInfo.className.Contains(dbfClass))
                    {
                        if (!HasBuffByName(charGlobals.playerInfo.Buffs, dbfShieldSpell))
                            UseSpellByName(charGlobals.apiObject, charGlobals.playerInfo.SpellLines, dbfShieldSpell);
                        else if (!HasBuffByName(charGlobals.playerInfo.Buffs, dbfAbsorbSpell))
                            UseSpellByName(charGlobals.apiObject, charGlobals.playerInfo.SpellLines, dbfAbsorbSpell);
                    }
                    else if (charGlobals.playerInfo.className.Contains(brdClass))
                    {
                        //Bard Songs/spells are all skills
                        if (!brdSpeedOn && !HasBuffByName(charGlobals.playerInfo.Buffs, speedSong))
                        {
                            brdSpeedOn = true;
                            UseSkillByName(charGlobals.apiObject, charGlobals.playerInfo.Skills, speedSong);
                        }
                        else if (!brdPowOn && !HasBuffByName(charGlobals.playerInfo.Buffs, powSong))
                        {
                            brdPowOn = true;
                            UseSkillByName(charGlobals.apiObject, charGlobals.playerInfo.Skills, powSong);
                        }
                            
                        else if (!brdHealOn && !HasBuffByName(charGlobals.playerInfo.Buffs, healSong))
                        {
                            brdHealOn = true;
                            UseSkillByName(charGlobals.apiObject, charGlobals.playerInfo.Skills, healSong);
                        }
                            
                        else if (!brdEndOn && !HasBuffByName(charGlobals.playerInfo.Buffs, endSong))
                        {
                            brdEndOn = true;
                            UseSkillByName(charGlobals.apiObject, charGlobals.playerInfo.Skills, endSong);
                        }
                        else if (!HasBuffByName(charGlobals.playerInfo.Buffs, ablBuff))
                            UseSkillByName(charGlobals.apiObject, charGlobals.playerInfo.Skills, ablBuff);
                    }
                    else if (charGlobals.playerInfo.className.Contains(hlrClass))
                    {
                        //resist buff
                        if (!hlrBuffOn && !HasBuffByName(charGlobals.playerInfo.Buffs, hlrResistBuff))
                        {
                            hlrBuffOn = true;
                            UseSkillByName(charGlobals.apiObject, charGlobals.playerInfo.Skills, hlrResistBuff);
                        }
                            
                    }
                    castTimeout = timeoutMax;
                }


                if (fightStarted)
                {
                    int morellaOffset = findEntityByName(charGlobals.EntityList, "Morella");
                    int goleOffset = findEntityByName(charGlobals.EntityList, "Golestandt");
                    int graniteOffset = findEntityByName(charGlobals.EntityList, "granite giant");
                    int cloudOffset = findEntityByName(charGlobals.EntityList, "smoke cloud");

                    //Paladin check for clouds
                    //******this type of movement doesn't work for this.  Maybe just a timed movement instead?
                    if (charGlobals.playerInfo.className.Contains(tankClass))
                    {
                        if (cloudNear)
                        {
                            float tankstoppingDist = 40f;
                            float tankdist = DistanceToPoint(charGlobals.playerPos, tankPoints[currentTankPoint].x, tankPoints[currentTankPoint].y);
                            short tanknewheading;
                            while (tankdist > tankstoppingDist)
                            {
                                UpdateGlobals(procId);
                                CharGlobals cloudGlobals = CharGlobalDict[procId];
                                tankdist = DistanceToPoint(cloudGlobals.playerPos, tankPoints[currentTankPoint].x, tankPoints[currentTankPoint].y);
                                tanknewheading = GetGameHeading(cloudGlobals.playerPos, tankPoints[currentTankPoint].x, tankPoints[currentTankPoint].y);
                                SetAutorun(cloudGlobals.apiObject, true);
                                SetPlayerHeading(cloudGlobals.apiObject, true, tanknewheading);
                                //dist = DistanceToPoint(charGlobals.playerPos, stickTargPos.pos_x, stickTargPos.pos_y, stickTargPos.pos_z);
                                //newheading = GetGameHeading(charGlobals.playerPos, stickTargPos.pos_x, stickTargPos.pos_y);
                            }
                            UpdateGlobals(procId);
                            CharGlobals finishCloudGlobals = CharGlobalDict[procId];
                            short tankfinalheading = GetGameHeading(finishCloudGlobals.playerPos, finishCloudGlobals.EntityList[goleOffset].pos_x, finishCloudGlobals.EntityList[goleOffset].pos_y);
                            SetPlayerHeading(finishCloudGlobals.apiObject, true, ConvertDirHeading(tankfinalheading));
                            //SetPlayerHeading(charGlobals.apiObject, false, 0);
                            SetAutorun(finishCloudGlobals.apiObject, false);
                            //This isn't turning them the direction of the leader for some reason
                            //
                            cloudNear = false;
                        }
                        if (cloudOffset > 0)
                        {
                            float cloudPosX = charGlobals.EntityList[cloudOffset].pos_x;
                            float cloudPosY = charGlobals.EntityList[cloudOffset].pos_y;
                            //If distance to cloud is less than 100
                            if (DistanceToPoint(charGlobals.playerPos, cloudPosX, cloudPosY) <= 200) {
                                if (currentTankPoint == 0)
                                {
                                    currentTankPoint = 1;
                                }
                                else
                                {
                                    currentTankPoint = 0;
                                }
                                cloudNear = true;
                            }
                        }
                    }

                    //target checks
                    //always check morella offset first, then check for a granite giant spawn, then gole
                    if (morellaOffset > 0)
                    {
                        EntityInfo morellaEnt = charGlobals.EntityList[morellaOffset];
                        if (morellaEnt.isDead == 0)
                        {
                            if (charGlobals.playerInfo.className.Contains(dmgClass) || charGlobals.playerInfo.className.Contains(dbfClass))
                            {
                                SetTarget(charGlobals.apiObject, morellaOffset);
                            }

                        }
                    }
                    //else if (graniteOffset > 0)
                    //{
                    //    EntityInfo giant = charGlobals.EntityList[graniteOffset];
                    //    float giantdist = DistanceToPoint(charGlobals.playerPos, giant.pos_x, giant.pos_y);
                    //    if (giantdist < 500 && giant.isDead == 0)
                    //    {
                    //        if (charGlobals.playerInfo.className.Contains(dmgClass))
                    //        {
                    //            SetTarget(charGlobals.apiObject, graniteOffset);
                    //
                    //        }
                    //    }
                    //}
                    else if (goleOffset > 0)
                    {
                        EntityInfo goleEnt = charGlobals.EntityList[goleOffset];
                        if (goleEnt.isDead == 0)
                        {
                            if (charGlobals.playerInfo.className.Contains(dmgClass) || charGlobals.playerInfo.className.Contains(dbfClass))
                            {
                                SetTarget(charGlobals.apiObject, goleOffset);
                            }
                        }
                        else
                        {
                            //Gole's dead, set flags to false to stop loops
                            fightStarted = false;
                            dragonRunning = false;
                            //set the pet to idle=
                            UsePetCmdByName(charGlobals.apiObject, "passive");
                        }
                    }

                    //combat, do another check that fight is started so we don't fight after gole dies
                    if (fightStarted && castTimeout == 0)
                    {
                        //Paladin will target gole, everyone else morella
                        if (charGlobals.playerInfo.className.Contains(tankClass))
                        {
                            //******Paladin isn't using skills at all need to test
                            SetTarget(charGlobals.apiObject, goleOffset);
                            //Melee Taunt
                            UseSkillByName(charGlobals.apiObject, charGlobals.playerInfo.Skills, tankMeleeTaunt);
                            //Spell taunt
                            UseSkillByName(charGlobals.apiObject, charGlobals.playerInfo.Skills, tankSpellTaunt);
                        }
                        else if (charGlobals.playerInfo.className.Contains(dmgClass))
                        {
                            //Check if pet is idle
                            if (charGlobals.EntityList[charGlobals.playerInfo.petEntIndex].entityStatus == 8)
                            {
                                UsePetCmdByName(charGlobals.apiObject, "attack");
                            }
                            UseSpellByName(charGlobals.apiObject, charGlobals.playerInfo.SpellLines, dmgNuke);
                        }
                        else if (charGlobals.playerInfo.className.Contains(dbfClass))
                        {
                            UseSpellByName(charGlobals.apiObject, charGlobals.playerInfo.SpellLines, dbfSpell);
                        }

                        //******single target heals not working at all
                        else if (charGlobals.playerInfo.className.Contains(brdClass))
                        {
                            //check if a party member needs heal
                            //int ptEntOffset = PartyMemberNeedsHeal(charGlobals.partyMemberList, charGlobals.EntityList);
                            //if (ptEntOffset > 0)
                            //{
                            //    SetTarget(charGlobals.apiObject, ptEntOffset);
                            //    if (charGlobals.EntityList[ptEntOffset].health < smallHealPct)
                            //        UseSkillByName(charGlobals.apiObject, charGlobals.playerInfo.Skills, brdSmallHeal);
                            //    else if (charGlobals.EntityList[ptEntOffset].health < bigHealPct)
                            //        UseSkillByName(charGlobals.apiObject, charGlobals.playerInfo.Skills, brdBigHeal);
                            //}
                            UseSkillByName(charGlobals.apiObject, charGlobals.playerInfo.Skills, brdGrpHeal);

                        }
                        else if (charGlobals.playerInfo.className.Contains(hlrClass))
                        {
                            //check if a party member needs heal
                            //int ptEntOffset = PartyMemberNeedsHeal(charGlobals.partyMemberList, charGlobals.EntityList);
                            //if (ptEntOffset > 0)
                            //{
                            //    SetTarget(charGlobals.apiObject, ptEntOffset);
                            //    if (charGlobals.EntityList[ptEntOffset].health < smallHealPct)
                            //        UseSkillByName(charGlobals.apiObject, charGlobals.playerInfo.Skills, hlrSmallHeal);
                            //    else if (charGlobals.EntityList[ptEntOffset].health < bigHealPct)
                            //        UseSkillByName(charGlobals.apiObject, charGlobals.playerInfo.Skills, hlrBigHeal);
                            //}
                            UseSkillByName(charGlobals.apiObject, charGlobals.playerInfo.Skills, hlrGrpHeal);
                        }
                        //We made it into the cast loop so reset the cast timeout
                        castTimeout = timeoutMax;
                    }
                    
                }
                
                Thread.Sleep(threadSleep);
            }
            CharGlobals globalFinish = CharGlobalDict[procId];
            //Make sure we aren't moving
            SetPlayerHeading(globalFinish.apiObject, false, 0);
            SetAutorun(globalFinish.apiObject, false);
        }

        private void stickFunc(int procId)
        {

            while (stickRunning)
            {
                UpdateGlobals(procId);
                UpdateGlobals(charNames[leaderName]);

                CharGlobals stickGlobals;
                if (charNames.ContainsKey(leaderName))
                    stickGlobals = CharGlobalDict[charNames[leaderName]];
                else
                    break;

                CharGlobals charGlobals = CharGlobalDict[procId];

                string plyrName = new string(charGlobals.playerInfo.name);
                if(plyrName.Equals(leaderName)) {
                    break;
                }

                if (!plyrName.Equals(leaderName)) {
                    float stoppingDist = 20.0f;
                    //currentTarget = findEntityByName(EntityList, "Asmoe");
                    //SetTarget(apiObject, currentTarget);

                    float dist = DistanceToPoint(charGlobals.playerPos, stickGlobals.playerPos.pos_x, stickGlobals.playerPos.pos_y);
                    short newheading = GetGameHeading(charGlobals.playerPos, stickGlobals.playerPos.pos_x, stickGlobals.playerPos.pos_y);
                    if (dist > stoppingDist)
                    {
                        SetAutorun(charGlobals.apiObject, true);
                        SetPlayerHeading(charGlobals.apiObject, true, newheading);
                        //dist = DistanceToPoint(playerPos, stickTargPos.pos_x, stickTargPos.pos_y, stickTargPos.pos_z);
                        //newheading = GetGameHeading(playerPos, stickTargPos.pos_x, stickTargPos.pos_y);
                    }
                    else
                    {
                        //have to convert the heading
                        SetPlayerHeading(charGlobals.apiObject, true, ConvertCharHeading(stickGlobals.playerPos.heading));
                        //SetPlayerHeading(apiObject, false, 0);
                        SetAutorun(charGlobals.apiObject, false);
                    }
                }


                Thread.Sleep(100);
            }
            CharGlobals charGlobalFinish = CharGlobalDict[procId];
            SetPlayerHeading(charGlobalFinish.apiObject, false, 0);
            SetAutorun(charGlobalFinish.apiObject, false);
        }

        private void getBuffs(int procId)
        {
            UpdateGlobals(procId);
            UpdateGlobals(charNames[leaderName]);

            CharGlobals stickGlobals;
            if (charNames.ContainsKey(leaderName))
            {
                stickGlobals = CharGlobalDict[charNames[leaderName]];
                CharGlobals charGlobals = CharGlobalDict[procId];
                //MoveItem Example
                //Target name,item name
                string targName = stickGlobals.targetInfo.name;
                string buffItem = "Full Buffs";


                if (!String.IsNullOrEmpty(targName))
                {
                    //string[] args = tmp.Split(',');
                    //int entOffset = -1;
                    //entOffset = findEntityByName(EntityList, args[0]);
                    if (stickGlobals.targetInfo.entOffset >= 0)
                    {
                        SetTarget(charGlobals.apiObject, stickGlobals.targetInfo.entOffset);
                        InteractRequest(charGlobals.apiObject, charGlobals.EntityList[stickGlobals.targetInfo.entOffset].objectId);
                        //After interacting, send the buy item packet
                        //alloc a buf and zero it out
                        IntPtr pktbuf = Marshal.AllocHGlobal(Marshal.SizeOf<PktBuffer>());
                        //zero out buffer
                        for (int i = 0; i < Marshal.SizeOf<CmdBuffer>(); i++)
                        {
                            Marshal.WriteByte(pktbuf, i, 0);
                        }
                        //This seems to work to buy buff token from any buff merchant
                        string buyItem = "78 00 08 EC 9C 00 07 6C 4F 00 01 00 00 01 01 00 00 00";
                        pktbuf = Marshal.StringToHGlobalAnsi(buyItem);
                        SendPacket(charGlobals.apiObject, pktbuf);
                        //Find the item and trae it to the npc
                        int fromSlot = 0;
                        fromSlot = ItemSlotByName(charGlobals.playerInfo.Inventory, buffItem);
                        if (fromSlot > 0)
                        {
                            //Add 1000 to objectId for moving item to NPCs
                            MoveItem(charGlobals.apiObject, fromSlot, charGlobals.EntityList[stickGlobals.targetInfo.entOffset].objectId + 1000, 0);
                        }
                        Marshal.FreeHGlobal(pktbuf);
                    }
                }
            }
             

        }
        

    }
}