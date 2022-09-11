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

        string leaderName = "Asmoe";
        string readyName = "Suzuha";

        Dictionary<string, int> charNames = new Dictionary<string, int>();

        //Global bool to trigger dragon script on/off
        bool dragonRunning = false;

        public ViewModels.ClassSettingsViewModel ViewModel
        {
            get;
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
                
                loadedList.Add(pInfoMsg);
                //
                
            }
            Marshal.FreeHGlobal(pInfobuf);


            InjectedInfo.Text = String.Join(Environment.NewLine, loadedList);

        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {

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
                loadedList.Add(pInfoMsg);
                //

            }
            Marshal.FreeHGlobal(pInfobuf);


            InjectedInfo.Text = String.Join(Environment.NewLine, loadedList);

            // Forcing the CommandManager to raise the RequerySuggested event
            CommandManager.InvalidateRequerySuggested();
        }

        private void Button_Click_RunTarget(object sender, RoutedEventArgs e)
        {
            int leaderPid = charNames[leaderName];

            DashboardPage.GameDLL leaderProc = DashboardPage.gameprocs.FirstOrDefault(x => x.procId == leaderPid);


            foreach (DashboardPage.GameDLL proc in DashboardPage.gameprocs)
            {
                //https://stackoverflow.com/questions/14854878/creating-new-thread-with-method-with-parameter
                //Thread newThread = new Thread(()=>runDemo(proc.apiObject));
                //newThread.Start();
                getBuffs(proc.apiObject, leaderProc.apiObject);
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

            DashboardPage.GameDLL stickProc = DashboardPage.gameprocs.FirstOrDefault(x => x.procId == stickPid);

            if (!stickRunning)
            {
                StickButton.Content = "Stick is On";
                stickRunning = true;
                foreach (DashboardPage.GameDLL proc in DashboardPage.gameprocs)
                {
                    if (proc.procId != readyPid)
                    {
                        //https://stackoverflow.com/questions/14854878/creating-new-thread-with-method-with-parameter
                        Thread newThread = new Thread(() => stickFunc(proc.apiObject, stickProc.apiObject));
                        newThread.Start();
                    }

                }
            }
            else
            {
                StickButton.Content = "Stick is Off";
                stickRunning = false;
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
                    Thread newThread = new Thread(() => battleLocFunc(proc.apiObject));
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
                    Thread newThread = new Thread(() => resetLocFunc(proc.apiObject));
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
                        Thread newThread = new Thread(() => battleFunc(proc.apiObject));
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
            }

        }

        private void battleLocFunc(IntPtr apiObject)
        {
            bool isMoving = true;

            bool destinationReached = false;
            List<EntityInfo> EntityList = new List<EntityInfo>();
            EntityList entList = new EntityList();
            List<String> chatLog = new List<String>();
            List<PartyMemberInfo> partyMemberList = new List<PartyMemberInfo>();
            PlayerPosition playerPos;
            PlayerPosition stickTargPos;
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
            IntPtr stickTargplayerPosbuf = Marshal.AllocHGlobal(Marshal.SizeOf<PlayerPosition>());

            while (isMoving)
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

                //Current player position
                GetPlayerPosition(apiObject, playerPosbuf);
                playerPos = (PlayerPosition)Marshal.PtrToStructure(playerPosbuf, typeof(PlayerPosition));

                //Target info

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

                string plyrName = new string(playerInfo.name);
                float xloc = 0;
                float yloc = 0;
                float zloc = 0;
                short finalheading = 0;
                //Set up locs
                if (playerInfo.className.Contains("Paladin"))
                {
                    xloc = 39094f;
                    yloc = 58789f;
                    zloc = 226f;
                    finalheading = 183;
                } else
                {
                    xloc = 38678f;
                    yloc = 59418f;
                    zloc = 227f;
                    finalheading = 73;
                }

                float stoppingDist = 20.0f;
                //currentTarget = findEntityByName(EntityList, "Asmoe");
                //SetTarget(apiObject, currentTarget);


                float dist = DistanceToPoint(playerPos, xloc, yloc);
                short newheading = GetGameHeading(playerPos, xloc, yloc);
                if (dist > stoppingDist)
                {
                    SetAutorun(apiObject, true);
                    SetPlayerHeading(apiObject, true, newheading);
                    //dist = DistanceToPoint(playerPos, stickTargPos.pos_x, stickTargPos.pos_y, stickTargPos.pos_z);
                    //newheading = GetGameHeading(playerPos, stickTargPos.pos_x, stickTargPos.pos_y);
                }
                else
                {
                    SetPlayerHeading(apiObject, true, ConvertDirHeading(finalheading));
                    //SetPlayerHeading(apiObject, false, 0);
                    SetAutorun(apiObject, false);
                    //This isn't turning them the direction of the leader for some reason
                    //
                    isMoving = false;
                }

                Thread.Sleep(100);
            }
            SetPlayerHeading(apiObject, false, 0);
            SetAutorun(apiObject, false);

            Marshal.FreeHGlobal(entbuf);
            Marshal.FreeHGlobal(chatbuf);
            Marshal.FreeHGlobal(playerPosbuf);
            Marshal.FreeHGlobal(stickTargplayerPosbuf);
            Marshal.FreeHGlobal(tInfobuf);
            Marshal.FreeHGlobal(pInfobuf);
            Marshal.FreeHGlobal(pbuf);
        }


        private void resetLocFunc(IntPtr apiObject)
        {
            bool isMoving = true;

            bool destinationReached = false;
            List<EntityInfo> EntityList = new List<EntityInfo>();
            EntityList entList = new EntityList();
            List<String> chatLog = new List<String>();
            List<PartyMemberInfo> partyMemberList = new List<PartyMemberInfo>();
            PlayerPosition playerPos;
            PlayerPosition stickTargPos;
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
            IntPtr stickTargplayerPosbuf = Marshal.AllocHGlobal(Marshal.SizeOf<PlayerPosition>());

            while (isMoving)
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

                //Current player position
                GetPlayerPosition(apiObject, playerPosbuf);
                playerPos = (PlayerPosition)Marshal.PtrToStructure(playerPosbuf, typeof(PlayerPosition));

                //Target info

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

                string plyrName = new string(playerInfo.name);
                float xloc = 0;
                float yloc = 0;
                float zloc = 0;
                short finalheading = 0;
                //Set up locs
                if (playerInfo.className.Contains("Paladin"))
                {
                    xloc = 37134f;
                    yloc = 59840f;
                    zloc = 200f;
                    finalheading = 71;
                }
                else
                {
                    xloc = 37134f;
                    yloc = 59840f;
                    zloc = 200f;
                    finalheading = 71;
                }

                float stoppingDist = 20.0f;
                //currentTarget = findEntityByName(EntityList, "Asmoe");
                //SetTarget(apiObject, currentTarget);


                float dist = DistanceToPoint(playerPos, xloc, yloc);
                short newheading = GetGameHeading(playerPos, xloc, yloc);
                if (dist > stoppingDist)
                {
                    SetAutorun(apiObject, true);
                    SetPlayerHeading(apiObject, true, newheading);
                    //dist = DistanceToPoint(playerPos, stickTargPos.pos_x, stickTargPos.pos_y, stickTargPos.pos_z);
                    //newheading = GetGameHeading(playerPos, stickTargPos.pos_x, stickTargPos.pos_y);
                }
                else
                {
                    SetPlayerHeading(apiObject, true, ConvertDirHeading(finalheading));
                    //SetPlayerHeading(apiObject, false, 0);
                    SetAutorun(apiObject, false);
                    //This isn't turning them the direction of the leader for some reason
                    //
                    isMoving = false;
                }

                Thread.Sleep(100);
            }
            SetPlayerHeading(apiObject, false, 0);
            SetAutorun(apiObject, false);

            Marshal.FreeHGlobal(entbuf);
            Marshal.FreeHGlobal(chatbuf);
            Marshal.FreeHGlobal(playerPosbuf);
            Marshal.FreeHGlobal(stickTargplayerPosbuf);
            Marshal.FreeHGlobal(tInfobuf);
            Marshal.FreeHGlobal(pInfobuf);
            Marshal.FreeHGlobal(pbuf);
        }

        private void battleFunc(IntPtr apiObject)
        {
            int threadSleep = 100; // milliseconds
            int castSleep = 0; // milliseconds

            //This is a countdown that gets reset after a cast to prevent spell spamming
            int timeoutMax = 12;
            int castTimeout = 0;

            bool fightStarted = false;

            List<EntityInfo> EntityList = new List<EntityInfo>();
            EntityList entList = new EntityList();
            List<String> chatLog = new List<String>();
            List<PartyMemberInfo> partyMemberList = new List<PartyMemberInfo>();
            PlayerPosition playerPos;
            TargetInfo targetInfo;
            PlayerInfo playerInfo;


            //alloc buffers
            IntPtr entbuf = Marshal.AllocHGlobal(Marshal.SizeOf<EntityList>());
            IntPtr chatbuf = Marshal.AllocHGlobal(Marshal.SizeOf<Chatbuffer>());
            IntPtr playerPosbuf = Marshal.AllocHGlobal(Marshal.SizeOf<PlayerPosition>());
            IntPtr tInfobuf = Marshal.AllocHGlobal(Marshal.SizeOf<TargetInfo>());
            IntPtr pInfobuf = Marshal.AllocHGlobal(Marshal.SizeOf<PlayerInfo>());
            IntPtr pbuf = Marshal.AllocHGlobal(Marshal.SizeOf<PartyMemberInfo>());
            IntPtr stickTargplayerPosbuf = Marshal.AllocHGlobal(Marshal.SizeOf<PlayerPosition>());

            //Globals
            bool buffTime = true;

            //Tank abilities/variables
            string tankClass = "Paladin";
            string tankMeleeTaunt = "Rile";
            string tankSpellTaunt = "Infuriate";
            //Implement two tank waypoints for dodging clouds
            bool cloudNear = false;
            Pos_2d[] tankPoints = new Pos_2d[2];
            int currentTankPoint = 0;
            tankPoints[0].x = 37134f;
            tankPoints[0].y = 59840f;
            tankPoints[1].x = 37134f;
            tankPoints[1].y = 59840f;

            //Damage caster
            string dmgClass = "Spiritmaster";
            string petSpell = "Spirit Warrior";
            string shieldSpell = "Superior Suppressive Barrier";
            string absorbSpell = "Suppressive Buffer";
            string grpAbsorb = "Shield of the Einherjar";

            //Bard
            string brdClass = "Bard";
            //songs
            string speedSong = "Clear Horizon";
            string powSong = "Rhyme of Creation";
            string endSong = "Rhythm of the Cosmos";
            string healSong = "Euphony of Healing";
            //buff
            string ablBuff = "Battlesong of Apotheosis";
            //heals
            string smallHeal = "Apotheosis";
            string bigHeal = "Major Apotheosis";
            string grpHeal = "Group Apotheosis";
            

            //Healer
            string healClass = "Druid";

            while (dragonRunning)
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
                    //Check chat line for the word selan
                    string chatLine = new string(tmpChat.chatLine);
                    if (chatLine.Contains("Selan"))
                    {
                        Thread.Sleep(1000);
                        fightStarted = true;
                    }
                    //chatLog.Add(tmpChat.chatLine);
                }

                //Current player position
                GetPlayerPosition(apiObject, playerPosbuf);
                playerPos = (PlayerPosition)Marshal.PtrToStructure(playerPosbuf, typeof(PlayerPosition));

                //Target info

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

                //Check if we're casting and have more than castSleep left
                if (EntityList[playerInfo.entListIndex].castCountdown > castSleep)
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


                if (buffTime && !fightStarted && castTimeout == 0)
                {
                    if (playerInfo.className.Contains(tankClass))
                    {

                    }else if (playerInfo.className.Contains(dmgClass))
                    {
                        //Pet spell
                        if(playerInfo.petEntIndex == 0)
                            UseSpellByName(apiObject, playerInfo.SpellLines, petSpell);
                        //Shield
                        else if (!HasBuffByName(playerInfo.Buffs, shieldSpell))
                            UseSpellByName(apiObject, playerInfo.SpellLines, shieldSpell);
                        //Absorb
                        else if (!HasBuffByName(playerInfo.Buffs, absorbSpell))
                            UseSpellByName(apiObject, playerInfo.SpellLines, absorbSpell);
                        //Group Absorb
                        else if (!HasBuffByName(playerInfo.Buffs, grpAbsorb))
                            UseSpellByName(apiObject, playerInfo.SpellLines, grpAbsorb);
                    }
                    else if (playerInfo.className.Contains(brdClass))
                    {
                        //Bard Songs/spells are all skills
                        if (!HasBuffByName(playerInfo.Buffs, speedSong))
                            UseSkillByName(apiObject, playerInfo.Skills, speedSong);
                        else if (!HasBuffByName(playerInfo.Buffs, powSong))
                            UseSkillByName(apiObject, playerInfo.Skills, powSong);
                        else if (!HasBuffByName(playerInfo.Buffs, healSong))
                            UseSkillByName(apiObject, playerInfo.Skills, healSong);
                        else if (!HasBuffByName(playerInfo.Buffs, endSong))
                            UseSkillByName(apiObject, playerInfo.Skills, endSong);
                        else if (!HasBuffByName(playerInfo.Buffs, ablBuff))
                            UseSkillByName(apiObject, playerInfo.Skills, ablBuff);
                    }
                    castTimeout = timeoutMax;
                }


                if (fightStarted)
                {
                    int morellaOffset = findEntityByName(EntityList, "Morella");
                    int goleOffset = findEntityByName(EntityList, "Golestandt");
                    int graniteOffset = findEntityByName(EntityList, "granite giant");
                    int cloudOffset = findEntityByName(EntityList, "smoke cloud");

                    //Paladin check for clouds
                    if (playerInfo.className.Contains(tankClass))
                    {
                        if (cloudNear)
                        {
                            float tankstoppingDist = 10f;
                            float tankdist = DistanceToPoint(playerPos, tankPoints[currentTankPoint].x, tankPoints[currentTankPoint].y);
                            short tanknewheading = GetGameHeading(playerPos, tankPoints[currentTankPoint].x, tankPoints[currentTankPoint].y);
                            while (tankdist > tankstoppingDist)
                            {
                                tankdist = DistanceToPoint(playerPos, tankPoints[currentTankPoint].x, tankPoints[currentTankPoint].y);
                                tanknewheading = GetGameHeading(playerPos, tankPoints[currentTankPoint].x, tankPoints[currentTankPoint].y);
                                SetAutorun(apiObject, true);
                                SetPlayerHeading(apiObject, true, tanknewheading);
                                //dist = DistanceToPoint(playerPos, stickTargPos.pos_x, stickTargPos.pos_y, stickTargPos.pos_z);
                                //newheading = GetGameHeading(playerPos, stickTargPos.pos_x, stickTargPos.pos_y);
                            }
                            short tankfinalheading = GetGameHeading(playerPos, EntityList[goleOffset].pos_x, EntityList[goleOffset].pos_y);
                            SetPlayerHeading(apiObject, true, ConvertDirHeading(tankfinalheading));
                            //SetPlayerHeading(apiObject, false, 0);
                            SetAutorun(apiObject, false);
                            //This isn't turning them the direction of the leader for some reason
                            //
                            cloudNear = false;
                        }
                        if (cloudOffset > 0)
                        {
                            float cloudPosX = EntityList[cloudOffset].pos_x;
                            float cloudPosY = EntityList[cloudOffset].pos_y;
                            //If distance to cloud is less than 100
                            if (DistanceToPoint(playerPos, cloudPosX, cloudPosY) <= 100) {
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
                        EntityInfo morellaEnt = EntityList[morellaOffset];
                        if (morellaEnt.health > 0)
                        {
                            if (playerInfo.className.Contains(dmgClass))
                            {
                                SetTarget(apiObject, morellaOffset);
                            }

                        }
                    }
                    else if (graniteOffset > 0)
                    {
                        EntityInfo giant = EntityList[graniteOffset];
                        float giantdist = DistanceToPoint(playerPos, giant.pos_x, giant.pos_y);
                        if (giantdist < 500 && giant.health > 0)
                        {
                            if (playerInfo.className.Contains(dmgClass))
                            {
                                SetTarget(apiObject, graniteOffset);

                            }
                        }
                    }
                    else if (goleOffset > 0)
                    {
                        EntityInfo goleEnt = EntityList[goleOffset];
                        if (goleEnt.health > 0)
                        {
                            if (playerInfo.className.Contains(dmgClass))
                            {
                                SetTarget(apiObject, goleOffset);

                            }
                        } else
                        {
                            //Gole's dead, set flags to false to stop loops
                            fightStarted = false;
                            dragonRunning = false;
                        }
                    }

                    //combat
                    //Paladin will target gole, everyone else morella
                    if (playerInfo.className.Contains(tankClass))
                    {
                        SetTarget(apiObject, goleOffset);
                        //Melee Taunt
                        UseSkillByName(apiObject, playerInfo.Skills, tankMeleeTaunt);
                        //Spell taunt
                        UseSkillByName(apiObject, playerInfo.Skills, tankSpellTaunt);
                    } else if (playerInfo.className.Contains(dmgClass))
                    {

                    }


                    castTimeout = timeoutMax;
                }
                
                Thread.Sleep(threadSleep);
            }
            //Make sure we aren't moving
            SetPlayerHeading(apiObject, false, 0);
            SetAutorun(apiObject, false);

            //Free all the buffers
            Marshal.FreeHGlobal(entbuf);
            Marshal.FreeHGlobal(chatbuf);
            Marshal.FreeHGlobal(playerPosbuf);
            Marshal.FreeHGlobal(stickTargplayerPosbuf);
            Marshal.FreeHGlobal(tInfobuf);
            Marshal.FreeHGlobal(pInfobuf);
            Marshal.FreeHGlobal(pbuf);
        }

        private void stickFunc(IntPtr apiObject, IntPtr stickTargApiObject)
        {
            bool destinationReached = false;
            List<EntityInfo> EntityList = new List<EntityInfo>();
            EntityList entList = new EntityList();
            List<String> chatLog = new List<String>();
            List<PartyMemberInfo> partyMemberList = new List<PartyMemberInfo>();
            PlayerPosition playerPos;
            PlayerPosition stickTargPos;
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
            IntPtr stickTargplayerPosbuf = Marshal.AllocHGlobal(Marshal.SizeOf<PlayerPosition>());

            while (stickRunning)
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

                //Current player position
                GetPlayerPosition(apiObject, playerPosbuf);
                playerPos = (PlayerPosition)Marshal.PtrToStructure(playerPosbuf, typeof(PlayerPosition));

                // Stick target player position
                GetPlayerPosition(stickTargApiObject, stickTargplayerPosbuf);
                stickTargPos = (PlayerPosition)Marshal.PtrToStructure(stickTargplayerPosbuf, typeof(PlayerPosition));



                //Target info

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

                string plyrName = new string(playerInfo.name);
                if(plyrName.Equals(leaderName)) {
                    break;
                }

                if (!plyrName.Equals(leaderName)) {
                    float stoppingDist = 20.0f;
                    //currentTarget = findEntityByName(EntityList, "Asmoe");
                    //SetTarget(apiObject, currentTarget);

                    float dist = DistanceToPoint(playerPos, stickTargPos.pos_x, stickTargPos.pos_y);
                    short newheading = GetGameHeading(playerPos, stickTargPos.pos_x, stickTargPos.pos_y);
                    if (dist > stoppingDist)
                    {
                        SetAutorun(apiObject, true);
                        SetPlayerHeading(apiObject, true, newheading);
                        //dist = DistanceToPoint(playerPos, stickTargPos.pos_x, stickTargPos.pos_y, stickTargPos.pos_z);
                        //newheading = GetGameHeading(playerPos, stickTargPos.pos_x, stickTargPos.pos_y);
                    }
                    else
                    {
                        //have to convert the heading
                        SetPlayerHeading(apiObject, true, ConvertCharHeading(stickTargPos.heading));
                        //SetPlayerHeading(apiObject, false, 0);
                        SetAutorun(apiObject, false);
                    }
                }


                Thread.Sleep(100);
            }
            SetPlayerHeading(apiObject, false, 0);
            SetAutorun(apiObject, false);

            Marshal.FreeHGlobal(entbuf);
            Marshal.FreeHGlobal(chatbuf);
            Marshal.FreeHGlobal(playerPosbuf);
            Marshal.FreeHGlobal(stickTargplayerPosbuf);
            Marshal.FreeHGlobal(tInfobuf);
            Marshal.FreeHGlobal(pInfobuf);
            Marshal.FreeHGlobal(pbuf);
        }

        private void getBuffs(IntPtr apiObject, IntPtr leaderApiObject)
        {
            List<EntityInfo> EntityList = new List<EntityInfo>();

            for (int i = 0; i < 2000; i++)
            {
                IntPtr entbuf = Marshal.AllocHGlobal(Marshal.SizeOf<DarksideGameAPI.EntityInfo>());
                EntityInfo tmpentity;
                GetEntityInfo(apiObject, i, entbuf);
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
            //PlayerInfo
            IntPtr pInfobuf = Marshal.AllocHGlobal(Marshal.SizeOf<PlayerInfo>());
            GetPlayerInfo(apiObject, pInfobuf);
            PlayerInfo playerInfo = (PlayerInfo)Marshal.PtrToStructure(pInfobuf, typeof(PlayerInfo));
            Marshal.FreeHGlobal(pInfobuf);

            //Leader target information
            IntPtr tInfobuf = Marshal.AllocHGlobal(Marshal.SizeOf<TargetInfo>());
            GetTargetInfo(leaderApiObject, tInfobuf);
            TargetInfo leaderTargetInfo = (TargetInfo)Marshal.PtrToStructure(tInfobuf, typeof(TargetInfo));
            Marshal.FreeHGlobal(tInfobuf);

            //MoveItem Example
            //Target name,item name
            string targName = leaderTargetInfo.name;
            string buffItem = "Full Buffs";


            if (!String.IsNullOrEmpty(targName))
            {
                //string[] args = tmp.Split(',');
                //int entOffset = -1;
                //entOffset = findEntityByName(EntityList, args[0]);
                if (leaderTargetInfo.entOffset >= 0)
                {
                    SetTarget(apiObject, leaderTargetInfo.entOffset);
                    InteractRequest(apiObject, EntityList[leaderTargetInfo.entOffset].objectId);
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
                    SendPacket(apiObject, pktbuf);
                    //Find the item and trae it to the npc
                    int fromSlot = 0;
                    fromSlot = ItemSlotByName(playerInfo.Inventory, buffItem);
                    if (fromSlot > 0)
                    {
                        //Add 1000 to objectId for moving item to NPCs
                        MoveItem(apiObject, fromSlot, EntityList[leaderTargetInfo.entOffset].objectId + 1000, 0);
                    }
                    Marshal.FreeHGlobal(pktbuf);
                }
            }
        }
        

    }
}