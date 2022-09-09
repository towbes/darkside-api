using DarkSideModernGUI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Converters;
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

        Dictionary<string, int> charNames = new Dictionary<string, int>();

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
                charNames.Add(plyrName, proc.procId);
                loadedList.Add(pInfoMsg);
                //
                
            }
            Marshal.FreeHGlobal(pInfobuf);


            InjectedInfo.Text = String.Join(Environment.NewLine, loadedList);

        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {

            //PlayerInfo
            //loadedList.Clear();
            //loadedList.Add("Injected Chars:");
            //int i = 0;
            //foreach (DashboardPage.GameDLL proc in DashboardPage.gameprocs)
            //{
            //    IntPtr pInfobuf = Marshal.AllocHGlobal(Marshal.SizeOf<PlayerInfo>());
            //    GetPlayerInfo(proc.apiObject, pInfobuf);
            //    PlayerInfo playerInfo = (PlayerInfo)Marshal.PtrToStructure(pInfobuf, typeof(PlayerInfo));
            //    string plyrName = new string(playerInfo.name);
            //    string className = new string(playerInfo.className);
            //    string pInfoMsg = String.Format("Name:{3} - Class:{4} - HP:{0:0} - Pow:{1:0} - Endu:{2:0}",
            //        playerInfo.health,
            //        playerInfo.power,
            //        playerInfo.endu,
            //        plyrName,
            //        className);
            //
            //    loadedList.Add(pInfoMsg);
            //    //
            //    Marshal.FreeHGlobal(pInfobuf);
            //}
            //
            //InjectedInfo.Text = String.Join(Environment.NewLine, loadedList);

            // Forcing the CommandManager to raise the RequerySuggested event
            CommandManager.InvalidateRequerySuggested();
        }

        private void Button_Click_RunTarget(object sender, RoutedEventArgs e)
        {
            foreach (DashboardPage.GameDLL proc in DashboardPage.gameprocs)
            {
                //https://stackoverflow.com/questions/14854878/creating-new-thread-with-method-with-parameter
                Thread newThread = new Thread(()=>runDemo(proc.apiObject));
                newThread.Start();
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
                    chatLog.Add(tmpChat.chatLine);
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

                    float dist = DistanceToPoint(playerPos, EntityList[trackerTarget].pos_x, EntityList[trackerTarget].pos_y, EntityList[trackerTarget].pos_z);
                    short newheading = GetGameHeading(playerPos, EntityList[trackerTarget].pos_x, EntityList[trackerTarget].pos_y);
                    if (dist > stoppingDist)
                    {
                        SetAutorun(apiObject, true);
                        SetPlayerHeading(apiObject, true, newheading);
                        dist = DistanceToPoint(playerPos, EntityList[trackerTarget].pos_x, EntityList[trackerTarget].pos_y, EntityList[trackerTarget].pos_z);
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

                    float dist = DistanceToPoint(playerPos, EntityList[trackerTarget].pos_x, EntityList[trackerTarget].pos_y, EntityList[trackerTarget].pos_z);
                    short newheading = GetGameHeading(playerPos, EntityList[trackerTarget].pos_x, EntityList[trackerTarget].pos_y);
                    if (dist > stoppingDist)
                    {
                        SetAutorun(apiObject, true);
                        SetPlayerHeading(apiObject, true, newheading);
                        dist = DistanceToPoint(playerPos, EntityList[trackerTarget].pos_x, EntityList[trackerTarget].pos_y, EntityList[trackerTarget].pos_z);
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
            int stickPid = charNames["Asmoe"];

            DashboardPage.GameDLL stickProc = DashboardPage.gameprocs.FirstOrDefault(x => x.procId == stickPid);

            if (!stickRunning)
            {
                stickRunning = true;
                foreach (DashboardPage.GameDLL proc in DashboardPage.gameprocs)
                {
                    //https://stackoverflow.com/questions/14854878/creating-new-thread-with-method-with-parameter
                    Thread newThread = new Thread(() => stickFunc(proc.apiObject, stickProc.apiObject));
                    newThread.Start();
                }
            }
            else
            {
                stickRunning = false;
            }

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
                    chatLog.Add(tmpChat.chatLine);
                }

                //Current player position
                GetPlayerPosition(apiObject, playerPosbuf);
                playerPos = (PlayerPosition)Marshal.PtrToStructure(playerPosbuf, typeof(PlayerPosition));

                // Stick target player position
                GetPlayerPosition(stickTargApiObject, stickTargplayerPosbuf);
                stickTargPos = (PlayerPosition)Marshal.PtrToStructure(stickTargplayerPosbuf, typeof(PlayerPosition));

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

                string plyrName = new string(playerInfo.name);
                
                if (!plyrName.Equals("Asmoe")) {
                    float stoppingDist = 20.0f;
                    currentTarget = findEntityByName(EntityList, "Asmoe");
                    SetTarget(apiObject, currentTarget);

                    float dist = DistanceToPoint(playerPos, stickTargPos.pos_x, stickTargPos.pos_y, stickTargPos.pos_z);
                    short newheading = GetGameHeading(playerPos, stickTargPos.pos_x, stickTargPos.pos_y);
                    if (dist > stoppingDist)
                    {
                        SetAutorun(apiObject, true);
                        SetPlayerHeading(apiObject, true, newheading);
                        dist = DistanceToPoint(playerPos, stickTargPos.pos_x, stickTargPos.pos_y, stickTargPos.pos_z);
                        newheading = GetGameHeading(playerPos, stickTargPos.pos_x, stickTargPos.pos_y);
                    }

                    //float dist = DistanceToPoint(playerPos, EntityList[currentTarget].pos_x, EntityList[currentTarget].pos_y, EntityList[currentTarget].pos_z);
                    //short newheading = GetGameHeading(playerPos, EntityList[currentTarget].pos_x, EntityList[currentTarget].pos_y);
                    //if (dist > stoppingDist)
                    //{
                    //    SetAutorun(apiObject, true);
                    //    SetPlayerHeading(apiObject, true, newheading);
                    //    dist = DistanceToPoint(playerPos, EntityList[currentTarget].pos_x, EntityList[currentTarget].pos_y, EntityList[currentTarget].pos_z);
                    //    newheading = GetGameHeading(playerPos, EntityList[currentTarget].pos_x, EntityList[currentTarget].pos_y);
                    //}
                    else
                    {
                        SetAutorun(apiObject, false);
                        SetPlayerHeading(apiObject, false, 0);
                    }
                }


                Thread.Sleep(100);
            }


            Marshal.FreeHGlobal(entbuf);
            Marshal.FreeHGlobal(chatbuf);
            Marshal.FreeHGlobal(playerPosbuf);
            Marshal.FreeHGlobal(stickTargplayerPosbuf);
            Marshal.FreeHGlobal(tInfobuf);
            Marshal.FreeHGlobal(pInfobuf);
            Marshal.FreeHGlobal(pbuf);
        }

    }
}