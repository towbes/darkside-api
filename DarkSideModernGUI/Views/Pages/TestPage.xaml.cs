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
using DarkSideModernGUI.Helpers;
using static DarkSideModernGUI.Helpers.DarksideGameAPI;
using static DarkSideModernGUI.Helpers.Movement;


//Commenting these out but leaving as a reminder for simulating keypresses if we want
//Had to use the InputSimulatorPlus version from  nuget to work with daoc
//using WindowsInput.Native;
//using WindowsInput;

namespace DarkSideModernGUI.Views.Pages
{
    /// <summary>
    /// Interaction logic for TestPage.xaml
    /// </summary>
    public partial class TestPage : INavigableView<ViewModels.TestViewModel>
    {

        public static IntPtr apiObject;
        bool autorun = false;
        bool changeHeading = false;

        EntityList entList = new EntityList();
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

        Dictionary<string, int> charNames = new Dictionary<string, int>();

        public TestPage(ViewModels.TestViewModel viewModel)
        {
            ViewModel = viewModel;

            InitializeComponent();

            //PlayerInfo
            int i = 0;
            IntPtr pInfobuf = Marshal.AllocHGlobal(Marshal.SizeOf<PlayerInfo>());
            foreach (DashboardPage.GameDLL proc in DashboardPage.gameprocs)
            {

                GetPlayerInfo(proc.apiObject, pInfobuf);
                PlayerInfo playerInfo = (PlayerInfo)Marshal.PtrToStructure(pInfobuf, typeof(PlayerInfo));
                string plyrName = new string(playerInfo.name);
                charNames.Add(plyrName, proc.procId);

            }

            DashboardPage.GameDLL stickProc;

            if (charNames.ContainsKey("Asmoe"))
            {
                int stickPid = charNames["Asmoe"];

                stickProc = DashboardPage.gameprocs.FirstOrDefault(x => x.procId == stickPid);
            } else
            {
                stickProc = DashboardPage.gameprocs.FirstOrDefault();
            }


            DashboardPage.apiObject = stickProc.apiObject;

            Marshal.FreeHGlobal(pInfobuf);
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
            IntPtr entbuf = Marshal.AllocHGlobal(Marshal.SizeOf<EntityList>());
            if (GetEntityList(DashboardPage.apiObject, entbuf))
            {
                entList = (EntityList)Marshal.PtrToStructure(entbuf, typeof(EntityList));
            }

            //Update entity table
            EntityList.Clear();
            strEntityList.Clear();
            for (int i = 0; i < 2000; i++)
            {
                EntityInfo tmpentity;
                //tmpentity = (EntityInfo)Marshal.PtrToStructure(entbuf, typeof(EntityInfo));
                tmpentity = entList.EntList[i];
                EntityList.Add(tmpentity);

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

            Marshal.FreeHGlobal(entbuf);

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
                    DistanceToPoint(playerPos, EntityList[targetInfo.entOffset].pos_x, EntityList[targetInfo.entOffset].pos_y),
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
            IntPtr pInfobuf = Marshal.AllocHGlobal(Marshal.SizeOf<PlayerInfo>());
            GetPlayerInfo(DashboardPage.apiObject, pInfobuf);
            PlayerInfo playerInfo = (PlayerInfo)Marshal.PtrToStructure(pInfobuf, typeof(PlayerInfo));
            int playerIndex = playerInfo.entListIndex;
            int castCount = EntityList[playerIndex].castCountdown;
            string pInfoMsg = String.Format("PlayerInfo:" + Environment.NewLine +
                "HP:{0:0} - Pow:{1:0} - Endu:{2:0} - Casting: {3:0} - PetIdx: {4:0}",
                playerInfo.health,
                playerInfo.power,
                playerInfo.endu,
                castCount,
                playerInfo.petEntIndex);

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
                    int targ = findEntityByName(EntityList, cname);
                    SetTarget(DashboardPage.apiObject, targ);
                    UseSkill(DashboardPage.apiObject, 17);
                }

            }

            //MemberInfo.Text = String.Join(Environment.NewLine, strPartyList);




            // Forcing the CommandManager to raise the RequerySuggested event
            CommandManager.InvalidateRequerySuggested();
        }

        //const UInt32 WM_KEYDOWN = 0x0100;
        //const UInt32 WM_KEYUP = 0x0101;
        //const int VK_NUMPAD5 = 0x31;
        //
        //[DllImport("user32.dll")]
        //static extern bool PostMessage(IntPtr hWnd, UInt32 Msg, int wParam, int lParam);
        //[DllImport("user32.dll")]
        //static extern bool SetForegroundWindow(IntPtr hWnd);
        //
        //
        //private void Button_Click_Numpad5(object sender, RoutedEventArgs e)
        //{
        //    //https://github.com/michaelnoonan/inputsimulator
        //    //Some test code with input simulator, but requires that the window come to foreground and is somewhat unreliable
        //    //InputSimulator sim = new InputSimulator();
        //    Process gameproc = Process.GetProcessById(GetPid(DashboardPage.apiObject));
        //    SetForegroundWindow(gameproc.MainWindowHandle);
        //    //InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_KEY_1)
        //}

        private void Button_Click_PetAttk(object sender, RoutedEventArgs e)
        {
            //https://github.com/michaelnoonan/inputsimulator
            //Some test code with input simulator, but requires that the window come to foreground and is somewhat unreliable
            //InputSimulator sim = new InputSimulator();
            UsePetCmd(DashboardPage.apiObject, 0, 0, 1);
            //InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_KEY_1)
        }

        //Get party member list example
        //private void Button_Click_4(object sender, RoutedEventArgs e)
        //{
        //    partyMemberList.Clear();
        //    strPartyList.Clear();
        //    for (int i = 0; i < 8; i++)
        //    {
        //        IntPtr buf = Marshal.AllocHGlobal(Marshal.SizeOf<PartyMemberInfo>());
        //        GetPartyMember(DashboardPage.apiObject, i, buf);
        //        PartyMemberInfo partyMember = (PartyMemberInfo)Marshal.PtrToStructure(buf, typeof(PartyMemberInfo));
        //        if (partyMember.hp_pct > 0)
        //        {
        //            partyMemberList.Add(partyMember);
        //        }
        //        Marshal.FreeHGlobal(buf);
        //
        //    }
        //    for (int j = 0; j < partyMemberList.Count; j++)
        //    {
        //        String cname = new string(partyMemberList[j].name);
        //
        //        String msg = String.Format("{0} - HP: {1}% - Endu: {2}% - Pow: {3}%",
        //            cname, partyMemberList[j].hp_pct, partyMemberList[j].endu_pct, partyMemberList[j].pow_pct);
        //
        //        strPartyList.Add(msg);
        //    }
        //
        //    MemberInfo.Text = String.Join(Environment.NewLine, strPartyList);
        //}

        private void Button_Click_SendPacket(object sender, RoutedEventArgs e)
        {

            //Send packet example
            ////String tmp = SendPacketText.Text;
            //
            ////String tmp = "78 00 05 55 03 00 07 78 87 00 02 00 00 01 00 00 00 00";
            //
            //
            //if (!String.IsNullOrEmpty(tmp))
            //{
            //    //alloc a buf and zero it out
            //    IntPtr buf = Marshal.AllocHGlobal(Marshal.SizeOf<PktBuffer>());
            //    //zero out buffer
            //    for (int i = 0; i < Marshal.SizeOf<PktBuffer>(); i++)
            //    {
            //        Marshal.WriteByte(buf, i, 0);
            //    }
            //
            //    //char[] strBuf = tmp.ToCharArray();
            //    buf = Marshal.StringToHGlobalAnsi(tmp);
            //    //Marshal.Copy(strBuf, 0, buf, strBuf.Length);
            //    //Examples:  0,0,/face  or 0,0,/say hi
            //    SendPacket(DashboardPage.apiObject, buf);
            //    Marshal.FreeHGlobal(buf);
            //}

            //Get player and entitylist
            for (int i = 0; i < 2000; i++)
            {
                IntPtr entbuf = Marshal.AllocHGlobal(Marshal.SizeOf<DarksideGameAPI.EntityInfo>());
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
            //PlayerInfo
            IntPtr pInfobuf = Marshal.AllocHGlobal(Marshal.SizeOf<PlayerInfo>());
            GetPlayerInfo(DashboardPage.apiObject, pInfobuf);
            PlayerInfo playerInfo = (PlayerInfo)Marshal.PtrToStructure(pInfobuf, typeof(PlayerInfo));
            Marshal.FreeHGlobal(pInfobuf);

           

            //MoveItem Example
            //Target name,item name
            string tmp = SendPacketText.Text;


            if (!String.IsNullOrEmpty(tmp))
            {
                string[] args = tmp.Split(',');
                int entOffset = -1;
                entOffset = findEntityByName(EntityList, args[0]);
                if (entOffset >= 0)
                {
                    SetTarget(DashboardPage.apiObject, entOffset);
                    InteractRequest(DashboardPage.apiObject, EntityList[entOffset].objectId);
                    //After interacting, send the buy item packet
                    //alloc a buf and zero it out
                    IntPtr pktbuf = Marshal.AllocHGlobal(Marshal.SizeOf<PktBuffer>());
                    //zero out buffer
                    for (int i = 0; i < Marshal.SizeOf<CmdBuffer>(); i++)
                    {
                        Marshal.WriteByte(pktbuf, i, 0);
                    }
                    string buyItem = "78 00 08 EC 9C 00 07 6C 4F 00 01 00 00 01 01 00 00 00";
                    pktbuf = Marshal.StringToHGlobalAnsi(buyItem);
                    SendPacket(DashboardPage.apiObject, pktbuf);
                    //Find the item and trae it to the npc
                    int fromSlot = 0;
                    fromSlot = ItemSlotByName(playerInfo.Inventory, args[1]);
                    if (fromSlot > 0)
                    {
                        //Add 1000 to objectId for moving item to NPCs
                        MoveItem(DashboardPage.apiObject, fromSlot, EntityList[entOffset].objectId + 1000, 0);
                    }
                    Marshal.FreeHGlobal(pktbuf);
                }
            }

        }
        private void Button_Click_SetTarget(object sender, RoutedEventArgs e)
        {

            //String tmp = SetTargetOffset.Text;
            //int offset = -1;
            ////If there are letters, get the offset by name
            //if (!tmp.All(char.IsDigit)) {
            //    offset = findEntityByName(tmp);
            //} else
            //{
            //    offset = Int32.Parse(SetTargetOffset.Text);
            //}
            //SetTarget(DashboardPage.apiObject, offset);

            //Reuse to test send command
            String tmp = SetTargetOffset.Text;
            string[] args = tmp.Split(',');
            if (!String.IsNullOrEmpty(tmp))
            {
                //alloc a buf and zero it out
                IntPtr buf = Marshal.AllocHGlobal(Marshal.SizeOf<CmdBuffer>());
                //zero out buffer
                for (int i = 0; i < Marshal.SizeOf<CmdBuffer>(); i++)
                {
                    Marshal.WriteByte(buf, i, 0);
                }

                //char[] strBuf = tmp.ToCharArray();
                buf = Marshal.StringToHGlobalAnsi(args[2]);
                //Marshal.Copy(strBuf, 0, buf, strBuf.Length);
                //Examples:  0,0,/face  or 0,0,/say hi
                SendCommand(DashboardPage.apiObject, Int32.Parse(args[0]), Int32.Parse(args[1]), buf);
                Marshal.FreeHGlobal(buf);
            }
            
        }

        private void Button_Click_UseSkill(object sender, RoutedEventArgs e)
        {
            UseSkill(DashboardPage.apiObject, Int32.Parse(UseSkillOffset.Text));
        }

        private void Button_Click_UseSpell(object sender, RoutedEventArgs e)
        {
            //PlayerInfo
            strPlayerInfo.Clear();
            IntPtr pInfobuf = Marshal.AllocHGlobal(Marshal.SizeOf<PlayerInfo>());
            GetPlayerInfo(DashboardPage.apiObject, pInfobuf);
            PlayerInfo playerInfo = (PlayerInfo)Marshal.PtrToStructure(pInfobuf, typeof(PlayerInfo));
            Marshal.FreeHGlobal(pInfobuf);

            string spellName = UseSpellOffset.Text;

            (int spellCategory, int spellLevel) = GetSpellByName(playerInfo.SpellLines, spellName);

            if (spellCategory != 999)
            {
                UseSpell(DashboardPage.apiObject, spellCategory, spellLevel);
            }
            
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            EntityList.Clear();
            for (int i = 0; i < 2000; i++)
            {
                IntPtr buf = Marshal.AllocHGlobal(Marshal.SizeOf<DarksideGameAPI.EntityInfo>());
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
                int trackerTarget = findEntityByName(EntityList, "Tracker");
                SetTarget(DashboardPage.apiObject, trackerTarget);

                float dist = DistanceToPoint(playerPos, EntityList[trackerTarget].pos_x, EntityList[trackerTarget].pos_y);
                short newheading = GetGameHeading(playerPos, EntityList[trackerTarget].pos_x, EntityList[trackerTarget].pos_y);
                if (dist > stoppingDist)
                {
                    SetAutorun(DashboardPage.apiObject, true);
                    SetPlayerHeading(DashboardPage.apiObject, true, newheading);
                    dist = DistanceToPoint(playerPos, EntityList[trackerTarget].pos_x, EntityList[trackerTarget].pos_y);
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
                int trackerTarget = findEntityByName(EntityList, "Nera");
                SetTarget(DashboardPage.apiObject, trackerTarget);

                float dist = DistanceToPoint(playerPos, EntityList[trackerTarget].pos_x, EntityList[trackerTarget].pos_y);
                short newheading = GetGameHeading(playerPos, EntityList[trackerTarget].pos_x, EntityList[trackerTarget].pos_y);
                if (dist > stoppingDist)
                {
                    SetAutorun(DashboardPage.apiObject, true);
                    SetPlayerHeading(DashboardPage.apiObject, true, newheading);
                    dist = DistanceToPoint(playerPos, EntityList[trackerTarget].pos_x, EntityList[trackerTarget].pos_y);
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



    }
}