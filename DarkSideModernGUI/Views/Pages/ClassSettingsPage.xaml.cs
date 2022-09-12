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
using static DarkSideModernGUI.Helpers.CharacterLoops;

namespace DarkSideModernGUI.Views.Pages
{
    /// <summary>
    /// Interaction logic for TestPage.xaml
    /// </summary>
    public partial class ClassSettingsPage : INavigableView<ViewModels.ClassSettingsViewModel>
    {
        DispatcherTimer dispatcherTimer;
        bool loopRunning = false;
        

        DispatcherTimer stickTimer;

        static public List<String> loadedList = new List<string>();


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

        private void Button_Click_HealLoop(object sender, RoutedEventArgs e)
        {
            if (!healRunning)
            {
                btnHealLoop.Content = "Heals Looping";
                healRunning = true;
                foreach (DashboardPage.GameDLL proc in DashboardPage.gameprocs)
                {
                    //https://stackoverflow.com/questions/14854878/creating-new-thread-with-method-with-parameter
                    Thread newThread = new Thread(() => HealFunc(proc.procId));
                    newThread.Start();

                }
                btnHealLoop.Background = new SolidColorBrush(Colors.Green);
            }
            else
            {
                btnHealLoop.Content = "Heals Stopped";
                healRunning = false;
                btnHealLoop.Background = new SolidColorBrush(Colors.Orange);
            }
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
        

    }
}