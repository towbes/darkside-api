﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Formats.Asn1;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static DarkSideModernGUI.Helpers.CharacterLoops;
using static DarkSideModernGUI.Helpers.DarksideGameAPI;
using static DarkSideModernGUI.Helpers.Movement;
using static DarkSideModernGUI.Helpers.DragonSettings;
using static DarkSideModernGUI.Views.Pages.ClassSettingsPage;
using Newtonsoft.Json;
using System.IO;
using DarkSideModernGUI.Views.Pages;
using System.Windows.Shapes;

namespace DarkSideModernGUI.Helpers
{
    internal class CharacterLoops
    {
        public static string leaderName;
        public static string readyName;
        public static string tankName;

        //Global bool to trigger dragon script on/off
        public static bool dragonRunning = false;
        public static bool stickRunning = false;
        public static bool healRunning = false;

        public enum DragonState
        {
            idle,
            reset,
            moveTo,
            fighting
        }

        public static DragonState currentState = DragonState.idle;
        public static bool dragonLooping = false;
        public static bool charLooping = false;

        //Run speed globals
        public static float fwdSpeed = 300;
        public static float strafeSpeed = 187;

        //Dicts to maintain currently injected state
        static public Dictionary<string, int> charNames = new Dictionary<string, int>();
        static public Dictionary<int, CharGlobals> CharGlobalDict = new Dictionary<int, CharGlobals>();



        public static DragonSettings drgSettings;

        public struct CharGlobals
        {
            //Api Object
            public IntPtr apiObject;
            //alloc buffers
            public IntPtr entbuf { get; set; }//= Marshal.AllocHGlobal(Marshal.SizeOf<EntityList>());
            public IntPtr chatbuf { get; set; }//= Marshal.AllocHGlobal(Marshal.SizeOf<Chatbuffer>());
            public IntPtr playerPosbuf { get; set; }//= Marshal.AllocHGlobal(Marshal.SizeOf<PlayerPosition>());
            public IntPtr tInfobuf { get; set; }//= Marshal.AllocHGlobal(Marshal.SizeOf<TargetInfo>());
            public IntPtr pInfobuf { get; set; }//= Marshal.AllocHGlobal(Marshal.SizeOf<PlayerInfo>());
            public IntPtr pbuf { get; set; }//= Marshal.AllocHGlobal(Marshal.SizeOf<PartyMemberInfo>());
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
            public DragonState currentState;

        }

        public static void LoadSettings()
        {
            string json = "";
            using (StreamReader r = new StreamReader("DragonSettings.json"))
            {
                json = r.ReadToEnd();
            }
            
            if (string.IsNullOrEmpty(json))
                return;
                
            drgSettings = JsonConvert.DeserializeObject<DragonSettings>(json);


            leaderName = drgSettings.roleNames.leaderName;
            tankName = drgSettings.roleNames.tankName;
            readyName = drgSettings.roleNames.readyName;

            //drgSettings = new DragonSettings();
            //
            //drgSettings.roleNames = new Names();
            //drgSettings.resetLocs = new ResetLocs();
            //drgSettings.battleLocs = new BattleLocs();
            //drgSettings.dragonFight = new DragonFight();
            //
            ////Names
            //drgSettings.roleNames.leaderName = "Asmoe";
            //drgSettings.roleNames.readyName = "Suzuha";
            //drgSettings.roleNames.tankName = "Okarin";
            //
            //drgSettings.resetLocs.tankLocs = new SettingsLoc();
            //drgSettings.resetLocs.otherLocs = new SettingsLoc();
            //
            //drgSettings.resetLocs.tankLocs.xloc = 12222;
            //
            //string json = JsonConvert.SerializeObject(drgSettings, Formatting.Indented);
            //
            //
            //File.WriteAllText("DragonSettings.json", json);
        }
        public static void UpdateGlobals(int procId)
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
                Debug.WriteLine(modGlobals.chatLine.chatLine);
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

        static public void ReleasecharGlobals(CharGlobals charBuffer)
        {
            Marshal.FreeHGlobal(charBuffer.entbuf);
            Marshal.FreeHGlobal(charBuffer.chatbuf);
            Marshal.FreeHGlobal(charBuffer.playerPosbuf);
            Marshal.FreeHGlobal(charBuffer.tInfobuf);
            Marshal.FreeHGlobal(charBuffer.pInfobuf);
            Marshal.FreeHGlobal(charBuffer.pbuf);
        }

        public static void DragonLoop(int procId)
        {
            //Maybe each char should have it's own state?
            while (dragonLooping)
            {
                UpdateGlobals(procId);
                CharGlobals charGlobals = CharGlobalDict[procId];
                switch (charGlobals.currentState)
                {
                    case DragonState.idle:
                        //do nothing
                        break;
                    case DragonState.reset:
                        charGlobals.currentState = DragonState.idle;
                        resetLocFunc(procId);
                        goto case DragonState.idle;
                    case DragonState.moveTo:
                        charGlobals.currentState = DragonState.idle;
                        battleLocFunc(procId);
                        goto case DragonState.idle;
                    case DragonState.fighting:
                        dragonRunning = true;
                        charGlobals.currentState = DragonState.idle;
                        battleFunc(procId);
                        goto case DragonState.idle;
                }
                Thread.Sleep(500);
            }
        }

        public static void MainLoop(int procId)
        {
            //Maybe each char should have it's own state?
            while (charLooping)
            {
                UpdateGlobals(procId);
                CharGlobals charGlobals = CharGlobalDict[procId];
                string playerClass = charGlobals.playerInfo.className;
                //Check if we're dead, if so return
                if (charGlobals.EntityList[charGlobals.playerInfo.entListIndex].isDead == 1)
                {
                    return;
                }

                if (healRunning && (playerClass.Contains("Bard") || playerClass.Contains("Druid")))
                    HealFunc(procId);
                if (stickRunning)
                    stickFunc(procId);

                Thread.Sleep(50);
            }
        }

        public static void battleLocFunc(int procId)
        {
            int threadsleep = 50;
            bool isMoving = true;
            bool isLooting = false;
            bool isResetting = false;

            float xloc;
            float yloc;
            short finalheading;

            UpdateGlobals(procId);

            CharGlobals charGlobals = CharGlobalDict[procId];

            int currentLoc = 0; //used to swap from prep loc -> actual battle loc

            //Check if ready char needs to run to reset
            if (drgSettings.resetLocs.readyCharReset)
            {
                if (charGlobals.playerInfo.name.Equals(drgSettings.roleNames.readyName))
                {
                    isResetting = true;
                }
            }

            while (isMoving)
            {
                UpdateGlobals(procId);

                charGlobals = CharGlobalDict[procId];
                string plyrName = new string(charGlobals.playerInfo.name);

                string className = new string(charGlobals.playerInfo.className);


                //Check if we're dead, if so return
                if (charGlobals.EntityList[charGlobals.playerInfo.entListIndex].isDead == 1)
                {
                    return;
                }

                //don't move if we're readyName
                if (!string.IsNullOrEmpty(readyName) && !drgSettings.resetLocs.readyCharReset)
                {
                    if (plyrName.Contains(readyName))
                    {
                        isMoving = false;

                        int maxEnt = findEntityByName(charGlobals.EntityList, "Maximilian");
                        while (maxEnt == 0 && dragonLooping)
                        {
                            UpdateGlobals(procId);
                            charGlobals = CharGlobalDict[procId];
                            maxEnt = findEntityByName(charGlobals.EntityList, "Maximilian");
                            Thread.Sleep(100);
                        }
                        if (maxEnt > 0)
                        {
                            float maxdist = DistanceToPoint(charGlobals.playerPos, charGlobals.EntityList[maxEnt].pos_x, charGlobals.EntityList[maxEnt].pos_y);
                            if (maxdist < 100)
                            {
                                IntPtr cmdbuf = Marshal.AllocHGlobal(Marshal.SizeOf<CmdBuffer>());
                                cmdbuf = Marshal.StringToHGlobalAnsi("/say ready");
                                SendCommand(charGlobals.apiObject, 0, 0, cmdbuf);
                                Marshal.FreeHGlobal(cmdbuf);
                            }
                        }
                        break;
                    }
                }




                //Set up locs
                if (className.Contains(drgSettings.dragonFight.tank.className))
                {
                    xloc = drgSettings.battleLocs.tankLocs.xloc;
                    yloc = drgSettings.battleLocs.tankLocs.yloc;
                    finalheading = drgSettings.battleLocs.tankLocs.heading;

                } else if (currentLoc == 0){
                    xloc = drgSettings.battleLocs.prepBattleLoc.xloc;
                    yloc = drgSettings.battleLocs.prepBattleLoc.yloc;
                    finalheading = drgSettings.battleLocs.prepBattleLoc.heading;
                }
                else if (drgSettings.battleLocs.loc1Chars.Contains(plyrName))
                {
                    xloc = drgSettings.battleLocs.otherLocs1.xloc;
                    yloc = drgSettings.battleLocs.otherLocs1.yloc;
                    finalheading = drgSettings.battleLocs.otherLocs1.heading;
                }
                else if (drgSettings.battleLocs.loc2Chars.Contains(plyrName))
                {
                    xloc = drgSettings.battleLocs.otherLocs2.xloc;
                    yloc = drgSettings.battleLocs.otherLocs2.yloc;
                    finalheading = drgSettings.battleLocs.otherLocs2.heading;

                }
                //remaining go to loc 3
                //else if (drgSettings.battleLocs.loc3Chars.Contains(plyrName))
                else
                {
                    xloc = drgSettings.battleLocs.otherLocs3.xloc;
                    yloc = drgSettings.battleLocs.otherLocs3.yloc;
                    finalheading = drgSettings.battleLocs.otherLocs3.heading;

                }

                float stoppingDist = 25.0f;

                //Initial battle location
                float dist = DistanceToPoint(charGlobals.playerPos, xloc, yloc);
                short newheading = GetGameHeading(charGlobals.playerPos, xloc, yloc);
                if (!isLooting && !isResetting)
                {
                    if (dist > stoppingDist)
                    {
                        SetPlayerFwdSpeed(charGlobals.apiObject, true, fwdSpeed);
                        SetPlayerHeading(charGlobals.apiObject, true, newheading);
                    }
                    else
                    {
                        SetPlayerHeading(charGlobals.apiObject, true, ConvertDirHeading(finalheading));
                        SetPlayerFwdSpeed(charGlobals.apiObject, true, 0);
                        //Stop if we made it to second location
                        if (currentLoc > 0)
                        {
                            isMoving = false;
                            isLooting = true;
                        }

                        currentLoc++;
                    }
                    
                }

                //If ready char needs to run to reset
                if (isResetting)
                {
                    int currentDest = 0; //0 = battle location, 1 = ramp, 2 = max, 3 = back to ramp, 4 = battle location
                    while (isResetting && dragonLooping)
                    {
                        dist = DistanceToPoint(charGlobals.playerPos, xloc, yloc);
                        newheading = GetGameHeading(charGlobals.playerPos, xloc, yloc);
                        UpdateGlobals(procId);
                        charGlobals = CharGlobalDict[procId];
                        //Check if we're dead, if so return
                        if (charGlobals.EntityList[charGlobals.playerInfo.entListIndex].isDead == 1)
                        {
                            return;
                        }
                        if (dist > stoppingDist)
                        {
                            SetPlayerFwdSpeed(charGlobals.apiObject, true, fwdSpeed);
                            SetPlayerHeading(charGlobals.apiObject, true, newheading);
                        }
                        else
                        {
                            SetPlayerFwdSpeed(charGlobals.apiObject, true, 0);
                            //Arrived at battle loc, move to ramp
                            if (currentDest == 0)
                            {
                                currentDest = 1;
                                xloc = drgSettings.resetLocs.rampLoc.xloc;
                                yloc = drgSettings.resetLocs.rampLoc.yloc;
                            //Arrived at ramp, move to max
                            } else if (currentDest == 1)
                            {
                                currentDest = 2;
                                xloc = drgSettings.resetLocs.maxLoc.xloc;
                                yloc = drgSettings.resetLocs.maxLoc.yloc;
                            //Arrived at max, say ready and move back to ramp
                            } else if (currentDest == 2)
                            {
                                IntPtr cmdbuf = Marshal.AllocHGlobal(Marshal.SizeOf<CmdBuffer>());
                                cmdbuf = Marshal.StringToHGlobalAnsi("/say ready");
                                SendCommand(charGlobals.apiObject, 0, 0, cmdbuf);
                                Marshal.FreeHGlobal(cmdbuf);
                                currentDest = 3;
                                xloc = drgSettings.resetLocs.rampLoc.xloc;
                                yloc = drgSettings.resetLocs.rampLoc.yloc;
                            //Arrived back at ramp, end this loop so that char goes back to initial battle location
                            } else if (currentDest == 3)
                            {
                                isResetting = false;
                            }
                        }
                    }
                }

                //tank should get loot first
                if (isLooting && className.Contains(drgSettings.dragonFight.tank.className))
                {
                    while (isLooting && dragonLooping)
                    {
                        UpdateGlobals(procId);
                        charGlobals = CharGlobalDict[procId];
                        int loot = findEntityByName(charGlobals.EntityList, "Sanguine");
                        if (loot > 0)
                        {

                            SetTarget(charGlobals.apiObject, loot);
                            float lootdist = DistanceToPoint(charGlobals.playerPos, charGlobals.EntityList[loot].pos_x, charGlobals.EntityList[loot].pos_y);
                            short loothead = GetGameHeading(charGlobals.playerPos, charGlobals.EntityList[loot].pos_x, charGlobals.EntityList[loot].pos_y);
                            if (lootdist > stoppingDist)
                            {
                                SetPlayerFwdSpeed(charGlobals.apiObject, true, fwdSpeed);
                                SetPlayerHeading(charGlobals.apiObject, true, loothead);
                            }
                            else
                            {
                                SetPlayerHeading(charGlobals.apiObject, false, 0);
                                //SetPlayerHeading(charGlobals.apiObject, false, 0);
                                SetPlayerFwdSpeed(charGlobals.apiObject, true, 0);
                                //Looting packets:
                                //74 00 01 12 05 68
                                //B0 00 00 00 00 68 
                                //B0 2F C1 60 00 E0
                                //B5 00 26
                                string pickupPkt = "B5 00 26";
                                IntPtr pktbuf = Marshal.AllocHGlobal(Marshal.SizeOf<PktBuffer>());
                                pktbuf = Marshal.StringToHGlobalAnsi(pickupPkt);
                                while (loot > 0)
                                {
                                    SetTarget(charGlobals.apiObject, loot);
                                    Thread.Sleep(300);
                                    SendPacket(charGlobals.apiObject, pktbuf);
                                    Thread.Sleep(300);
                                    loot = findEntityByName(charGlobals.EntityList, "Sanguine");
                                    UpdateGlobals(procId);
                                }
                                Marshal.FreeHGlobal(pktbuf);
                                isLooting = false;
                                isMoving = true;
                            }
                        }
                        else
                        {
                            isLooting = false;
                            isMoving = false;
                        }
                    }
                }

                Thread.Sleep(threadsleep);
            }
            CharGlobals finalGlobals = CharGlobalDict[procId];
            SetPlayerHeading(finalGlobals.apiObject, false, 0);
            SetPlayerFwdSpeed(finalGlobals.apiObject, false, 0);
            finalGlobals.currentState = DragonState.fighting;
            CharGlobalDict[procId] = finalGlobals;
        }


        public static void resetLocFunc(int procId)
        {
            bool isMoving = true;

            float xloc = 0;
            float yloc = 0;
            short finalheading = 0;

            int currentLoc = 0; //used to swap from prep loc -> actual reset loc

            while (isMoving && dragonLooping)
            {
                UpdateGlobals(procId);



                CharGlobals charGlobals = CharGlobalDict[procId];
                string plyrName = new string(charGlobals.playerInfo.name);

                string className = new string(charGlobals.playerInfo.className);

                //Check if we're dead, if so return
                if (charGlobals.EntityList[charGlobals.playerInfo.entListIndex].isDead == 1)
                {
                    return;
                }

                //don't move if we're readyName and we are using a separate readychar
                if (!string.IsNullOrEmpty(readyName) && !drgSettings.resetLocs.readyCharReset) {
                    if (plyrName.Contains(readyName))
                    {
                        break;
                    }
                }


                //Set up prep loc first
                if (currentLoc == 0)
                {
                    if (className.Contains(drgSettings.dragonFight.tank.className))
                    {
                        xloc = drgSettings.resetLocs.prepResetLoc.xloc;
                        yloc = drgSettings.resetLocs.prepResetLoc.yloc;
                        finalheading = drgSettings.resetLocs.prepResetLoc.heading;

                    }
                    else
                    {
                        xloc = drgSettings.resetLocs.prepResetLoc.xloc;
                        yloc = drgSettings.resetLocs.prepResetLoc.yloc;
                        finalheading = drgSettings.resetLocs.prepResetLoc.heading;
                    }
                }


                float stoppingDist = 10.0f;
                //currentTarget = findEntityByName(EntityList, "Asmoe");
                //SetTarget(charGlobals.apiObject, currentTarget);


                float dist = DistanceToPoint(charGlobals.playerPos, xloc, yloc);
                short newheading = GetGameHeading(charGlobals.playerPos, xloc, yloc);
                if (dist > stoppingDist)
                {
                    SetPlayerFwdSpeed(charGlobals.apiObject, true, fwdSpeed);
                    SetPlayerHeading(charGlobals.apiObject, true, newheading);
                }
                else
                {
                    SetPlayerFwdSpeed(charGlobals.apiObject, true, 0);

                    //We made it to the prep loc, so set to actual wall loc
                    if (currentLoc == 0)
                    {
                        currentLoc = 1;
                        xloc = drgSettings.resetLocs.tankLocs.xloc;
                        yloc = drgSettings.resetLocs.tankLocs.yloc;
                        SetPlayerFwdSpeed(charGlobals.apiObject, true, fwdSpeed);
                    }
                    //We made it to the wall 
                    else if (currentLoc == 1)
                    {
                        currentLoc = 2;
                        //SetPlayerHeading(charGlobals.apiObject, true, ConvertDirHeading(finalheading));
                        isMoving = false;
                    }
                }

                Thread.Sleep(100);
            }
            CharGlobals finalGlobals = CharGlobalDict[procId];
            SetPlayerHeading(finalGlobals.apiObject, false, 0);
            SetPlayerFwdSpeed(finalGlobals.apiObject, false, 0);

            int goleOffset = findEntityByName(finalGlobals.EntityList, "Golestandt", true);
            string cName = finalGlobals.playerInfo.className;
            //Set the state to battle
            //wait for gole to come back, or not be dead
            while (dragonLooping && goleOffset == 0)
            {
                UpdateGlobals(procId);
                finalGlobals = CharGlobalDict[procId];
                goleOffset = findEntityByName(finalGlobals.EntityList, "Golestandt", true);
            }
            //Progress the state
            finalGlobals.currentState = DragonState.moveTo;
            CharGlobalDict[procId] = finalGlobals;

        }

        public static void battleFunc(int procId)
        {
            int threadSleep = 50; // milliseconds
            int castSleep = 0; // milliseconds

            //This is a countdown that gets reset after a cast to prevent spell spamming
            int timeoutMax = 10;
            int castTimeout = 0;

            //global flag
            bool fightStarted = false;


            //Tank abilities/variables
            string tankClass = drgSettings.dragonFight.tank.className;
            string tankMeleeTaunt = drgSettings.dragonFight.tank.dps1;
            string tankSpellTaunt = drgSettings.dragonFight.tank.dps2;
            
            //heals
            string tankHealSkill = drgSettings.dragonFight.tank.heal1;
            int tankHealpct = drgSettings.dragonFight.tank.heal1pct;
            //buffs
            string tankArmorBuff = drgSettings.dragonFight.tank.buff1;
            string tankStrChant = drgSettings.dragonFight.tank.buff2;
            bool tankStrOn = false;
            string tankEndChant = drgSettings.dragonFight.tank.buff3;
            bool tankEndOn = false;
            string tankAfChant = drgSettings.dragonFight.tank.buff4;
            bool tankAfOn = false;
            string tankAblChant = drgSettings.dragonFight.tank.buff5;
            bool tankAblOn = false;
            //Implement two tank waypoints for dodging clouds
            bool cloudNear = false;
            int currentTankPoint = -1;

            //Damage caster
            string dmgClass = drgSettings.dragonFight.dps.className;
            //pet and buffs
            string petSpell = drgSettings.dragonFight.dps.pet1;
            string mlNine = drgSettings.dragonFight.dps.pet2;

            string shieldSpell = drgSettings.dragonFight.dps.buff1;
            string absorbSpell = drgSettings.dragonFight.dps.buff2;
            string grpAbsorb = drgSettings.dragonFight.dps.buff3;
            //nuke
            string dmgNuke = drgSettings.dragonFight.dps.dps1;
            

            //Debuffer
            string dbfClass = drgSettings.dragonFight.debuff.className;
            string dbfShieldSpell = drgSettings.dragonFight.debuff.buff1;
            string dbfAbsorbSpell = drgSettings.dragonFight.debuff.buff2;
            string dbfSpell = drgSettings.dragonFight.debuff.dps1;
            string dbfStun = drgSettings.dragonFight.debuff.dps2;
            string dbfNS = drgSettings.dragonFight.debuff.dps3;

            //Bard
            string brdClass = drgSettings.dragonFight.bard.className;
            //songs
            string speedSong = drgSettings.dragonFight.bard.buff1;
            bool brdSpeedOn = false;
            string powSong = drgSettings.dragonFight.bard.buff2;
            bool brdPowOn = false;
            string endSong = drgSettings.dragonFight.bard.buff3;
            bool brdEndOn = false;
            string healSong = drgSettings.dragonFight.bard.buff4;
            bool brdHealOn = false;
            //buff
            string ablBuff = drgSettings.dragonFight.bard.buff5;
            //heals
            int smallHealPct = drgSettings.dragonFight.bard.heal1pct;
            int bigHealPct = drgSettings.dragonFight.bard.heal2pct;
            string brdSmallHeal = drgSettings.dragonFight.bard.heal1;
            string brdBigHeal = drgSettings.dragonFight.bard.heal2;
            string brdGrpHeal = drgSettings.dragonFight.bard.grpHeal1;
            int grpHealPct = drgSettings.dragonFight.bard.grpHeal1pct;

            //Healer
            string hlrClass = drgSettings.dragonFight.healer.className;
            //resist buff
            string hlrResistBuff = drgSettings.dragonFight.healer.buff1;
            bool hlrBuffOn = false;
            //heals
            string hlrSmallHeal = drgSettings.dragonFight.healer.heal1;
            string hlrBigHeal = drgSettings.dragonFight.healer.heal2;
            string hlrGrpHeal = drgSettings.dragonFight.healer.grpHeal1;

            CharGlobals charGlobals = CharGlobalDict[procId];


            while (dragonRunning)
            {
                UpdateGlobals(procId);

                charGlobals = CharGlobalDict[procId];

                //Check if we're dead, if so return
                if (charGlobals.EntityList[charGlobals.playerInfo.entListIndex].isDead == 1)
                {
                    return;
                }

                string plyrName = charGlobals.playerInfo.name;
                //don't move if we're readyName
                if (!string.IsNullOrEmpty(readyName) && !drgSettings.resetLocs.readyCharReset)
                {
                    if (plyrName.Contains(readyName))
                    {
                        break;
                    }
                }

                int decimusOffset = findEntityByName(charGlobals.EntityList, "Decimus", true);

                //Setup gole to check for health under 100
                //This should fix when the encounter bug resets before gole is dead
                int goleOffset = findEntityByName(charGlobals.EntityList, "Golestandt", true);
                EntityInfo goleEnt;
                if (goleOffset > 0)
                {
                    goleEnt = charGlobals.EntityList[goleOffset];
                    if (goleEnt.health < 100)
                    {
                        fightStarted = true;
                    }
                } else
                {
                    dragonRunning = false;
                }
                //Check chat for selan message
                //******the chat check doesn't seem to be working but the decimus offset does
                //charGlobals.chatLine.chatLine.Contains("Golestandt") || 
                if (decimusOffset == 0)
                {
                    fightStarted = true;
                }
                       
                
                //else
                //{
                //    fightStarted = false;
                //}

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

                //if we're the leader, check for seals to craft
                //craft lambent: ED 2E 3B 00 00 00
                //craft fulgent: ED 2E 3C 00 00 00
                if (charGlobals.playerInfo.name.Contains(leaderName))
                {
                    int hasSeal = ItemSlotByName(charGlobals.playerInfo.Inventory, "10 Sanguine");
                    if (hasSeal > 0)
                    {
                        string smallSeal = "ED 2E 3B 00 00 00";
                        string bigSeal = "ED 2E 3C 00 00 00";
                        IntPtr pktbuf = Marshal.AllocHGlobal(Marshal.SizeOf<PktBuffer>());
                        pktbuf = Marshal.StringToHGlobalAnsi(smallSeal);
                        //7 sanguine
                        for (int i = 0; i < 7; i++)
                        {
                            SendPacket(charGlobals.apiObject, pktbuf);
                            Thread.Sleep(3500);
                        }
                        //2 fulgent
                        pktbuf = Marshal.StringToHGlobalAnsi(bigSeal);
                        for (int i = 0; i < 2; i++)
                        {
                            SendPacket(charGlobals.apiObject, pktbuf);
                            Thread.Sleep(3500);
                        }
                        Marshal.FreeHGlobal(pktbuf);
                    }

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
                        if (charGlobals.playerInfo.petEntIndex == 0)
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
                    goleOffset = findEntityByName(charGlobals.EntityList, "Golestandt", true);
                    int graniteOffset = findEntityInRadius(charGlobals.EntityList, charGlobals.playerPos, "granite giant", 1500);
                    int cloudOffset = findEntityInRadius(charGlobals.EntityList, charGlobals.playerPos, "smoke cloud", 30);
                    int fireOffset = findEntityInRadius(charGlobals.EntityList, charGlobals.playerPos, "Fire", 30);

                    //assign gole entity to check for hp under 100 in case encounter bugs
                    goleEnt = charGlobals.EntityList[goleOffset];

                    if (goleOffset == 0)
                    {
                        fightStarted = false;
                        dragonRunning = false;
                    }

                    //Paladin check for clouds
                    //******this type of movement doesn't work for this.  Maybe just a timed movement instead?
                    //if (charGlobals.playerInfo.className.Contains(tankClass))
                    //{
                        if (cloudOffset > 0)
                        {
                            float cloudPosX = charGlobals.EntityList[cloudOffset].pos_x;
                            float cloudPosY = charGlobals.EntityList[cloudOffset].pos_y;
                            //If distance to cloud is less than 100
                            if (DistanceToPoint(charGlobals.playerPos, cloudPosX, cloudPosY) <= 30)
                            {

                                //Move for 9 seconds while the cloud spawns
                                for (int i = 0; i < 9; i ++)
                                {
                                    UpdateGlobals(procId);
                                    CharGlobals cloudGlobals = CharGlobalDict[procId];
                                    SetPlayerStrafeSpeed(cloudGlobals.apiObject, true, strafeSpeed * currentTankPoint);

                                    //Melee Taunt
                                    UseSkillByName(charGlobals.apiObject, charGlobals.playerInfo.Skills, tankMeleeTaunt);
                                    Thread.Sleep(threadSleep);
                                    //Spell taunt
                                    UseSkillByName(charGlobals.apiObject, charGlobals.playerInfo.Skills, tankSpellTaunt);
                                    Thread.Sleep(100);

                                }
                                SetPlayerStrafeSpeed(charGlobals.apiObject, true, 0);
                                //SetPlayerStrafeSpeed(charGlobals.apiObject, false, 0);
                                currentTankPoint = currentTankPoint * -1;
                            }
                        }
                        if (fireOffset > 0)
                        {
                            float firePosX = charGlobals.EntityList[fireOffset].pos_x;
                            float firePosY = charGlobals.EntityList[fireOffset].pos_y;
                            //If distance to cloud is less than 100
                            if (DistanceToPoint(charGlobals.playerPos, firePosX, firePosY) <= 30)
                            {

                                //Sleep for 6 seconds while the cloud spawns
                                for (int i = 0; i < 5; i++)
                                {
                                    UpdateGlobals(procId);
                                    CharGlobals fireGlobals = CharGlobalDict[procId];
                                    SetPlayerStrafeSpeed(fireGlobals.apiObject, true, strafeSpeed * currentTankPoint);

                                    //Melee Taunt
                                    UseSkillByName(charGlobals.apiObject, charGlobals.playerInfo.Skills, tankMeleeTaunt);
                                    Thread.Sleep(threadSleep);
                                    //Spell taunt
                                    UseSkillByName(charGlobals.apiObject, charGlobals.playerInfo.Skills, tankSpellTaunt);
                                    Thread.Sleep(100);

                                }
                                SetPlayerStrafeSpeed(charGlobals.apiObject, true, 0);
                                //SetPlayerStrafeSpeed(charGlobals.apiObject, false, 0);
                                currentTankPoint = currentTankPoint * -1;
                            }
                        }

                    //}

                    //target checks for DPS / Debuff only
                    //always check morella offset first, then check for a granite giant spawn, then gole
                    if (charGlobals.playerInfo.className.Contains(dmgClass) || charGlobals.playerInfo.className.Contains(dbfClass))
                    {
                        //only attack morella if goles health is over 95, this should kill morella initially, but then not try to retarget her if encounter was started
                        if (morellaOffset > 0 && charGlobals.targetInfo.entOffset != morellaOffset && goleEnt.health > 95)
                        {
                            EntityInfo morellaEnt = charGlobals.EntityList[morellaOffset];
                            if (morellaEnt.isDead == 0)
                            {
                                if (charGlobals.playerInfo.className.Contains(dmgClass) || charGlobals.playerInfo.className.Contains(dbfClass))
                                {
                                    SetTarget(charGlobals.apiObject, morellaOffset);
                                    Thread.Sleep(50);
                                    UsePetCmdByName(charGlobals.apiObject, "attack");
                                    UseSpellByName(charGlobals.apiObject, charGlobals.playerInfo.SpellLines, mlNine);
                                    Thread.Sleep(50);
                                    UseSpellByName(charGlobals.apiObject, charGlobals.playerInfo.SpellLines, "Emasculate Strength");
                                    //Make sure these are staying true while we're fighting
                                    fightStarted = true;
                                    dragonRunning = true;
                                }

                            }
                        }
                        if ((morellaOffset == 0 || charGlobals.EntityList[morellaOffset].isDead == 1) && graniteOffset > 0 && charGlobals.targetInfo.entOffset != graniteOffset)
                        {
                            EntityInfo giant = charGlobals.EntityList[graniteOffset];
                            float giantdist = DistanceToPoint(charGlobals.playerPos, giant.pos_x, giant.pos_y);
                            if (giantdist < 1500 && giant.health > 0)
                            {
                                if (charGlobals.playerInfo.className.Contains(dmgClass))
                                {
                                    SetTarget(charGlobals.apiObject, graniteOffset);
                                    //Make sure these are staying true while we're fighting
                                    fightStarted = true;
                                    dragonRunning = true;

                                }
                            } else
                            {
                                //set the granite offset to 0 if it's too far away so that we target gole instead
                                graniteOffset = 0;
                            }
                        }
                        //attack if morella and giant are dead, or if gole's health is under 100
                        if ((morellaOffset == 0) && (graniteOffset == 0) && goleOffset > 0 && charGlobals.targetInfo.entOffset != goleOffset)
                        {
                            goleEnt = charGlobals.EntityList[goleOffset];
                            if (goleEnt.health > 0)
                            {
                                if (charGlobals.playerInfo.className.Contains(dmgClass) || charGlobals.playerInfo.className.Contains(dbfClass))
                                {
                                    SetTarget(charGlobals.apiObject, goleOffset);
                                    Thread.Sleep(50);
                                    UsePetCmdByName(charGlobals.apiObject, "attack");
                                    Thread.Sleep(50);
                                    UseSpellByName(charGlobals.apiObject, charGlobals.playerInfo.SpellLines, "Emasculate Strength");
                                }
                                //Make sure these are staying true while we're fighting
                                fightStarted = true;
                                dragonRunning = true;
                            }
                            else
                            {
                                //Gole's dead, set flags to false to stop loops
                                fightStarted = false;
                                dragonRunning = false;
                                //set the pet to idle=
                                UsePetCmdByName(charGlobals.apiObject, "passive");
                            }
                        } else if (goleOffset == 0)
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
                            //Tank always targets gole
                            SetTarget(charGlobals.apiObject, goleOffset);
                            //face gole
                            short facegoleheading = GetGameHeading(charGlobals.playerPos, charGlobals.EntityList[goleOffset].pos_x, charGlobals.EntityList[goleOffset].pos_y);
                            SetPlayerHeading(charGlobals.apiObject, true, facegoleheading);
                            //Melee Taunt
                            UseSkillByName(charGlobals.apiObject, charGlobals.playerInfo.Skills, tankMeleeTaunt);
                            //Spell taunt
                            UseSkillByName(charGlobals.apiObject, charGlobals.playerInfo.Skills, tankSpellTaunt);
                            if (charGlobals.playerInfo.health < tankHealpct)
                            {
                                UseSkillByName(charGlobals.apiObject, charGlobals.playerInfo.Skills, tankHealSkill);
                            }
                        }
                        else if (charGlobals.playerInfo.className.Contains(dmgClass))
                        {
                            //Check if pet is idle
                            if (charGlobals.EntityList[charGlobals.playerInfo.petEntIndex].entityStatus == 8)
                            {
                                UsePetCmdByName(charGlobals.apiObject, "attack");
                            }
                            UseSpellByName(charGlobals.apiObject, charGlobals.playerInfo.SpellLines, dmgNuke);
                            UsePetCmdByName(charGlobals.apiObject, "attack");
                        }
                        else if (charGlobals.playerInfo.className.Contains(dbfClass))
                        {
                            //spam stun on morella
                            if (charGlobals.targetInfo.entOffset == morellaOffset)
                            {
                                UseSpellByName(charGlobals.apiObject, charGlobals.playerInfo.SpellLines, dbfStun);
                            }
                            UseSpellByName(charGlobals.apiObject, charGlobals.playerInfo.SpellLines, dbfSpell);
                            UseSpellByName(charGlobals.apiObject, charGlobals.playerInfo.SpellLines, dbfNS);
                        }

                        if (charGlobals.playerInfo.className.Contains(brdClass))
                        {
                            //check if need to group heal
                            if (GetPartyAverageHP(charGlobals.partyMemberList) < 75)
                            {
                                UseSkillByName(charGlobals.apiObject, charGlobals.playerInfo.Skills, brdGrpHeal);
                            }
                            else
                            {
                                //check if a party member needs heal
                                int ptEntOffset = PartyMemberNeedsHeal(charGlobals.partyMemberList, charGlobals.EntityList);
                                if (ptEntOffset > 0)
                                {
                                    SetTarget(charGlobals.apiObject, ptEntOffset);
                                    if (charGlobals.EntityList[ptEntOffset].health < bigHealPct)
                                        UseSkillByName(charGlobals.apiObject, charGlobals.playerInfo.Skills, brdBigHeal);
                                    else if (charGlobals.EntityList[ptEntOffset].health < smallHealPct)
                                        UseSkillByName(charGlobals.apiObject, charGlobals.playerInfo.Skills, brdSmallHeal);

                                }
                            }


                        }
                        else if (charGlobals.playerInfo.className.Contains(hlrClass))
                        {
                            //first check for group heal
                            if (GetPartyAverageHP(charGlobals.partyMemberList) < 85)
                            {
                                UseSkillByName(charGlobals.apiObject, charGlobals.playerInfo.Skills, hlrGrpHeal);
                            }
                            else
                            {
                                //check if a party member needs heal
                                int ptEntOffset = PartyMemberNeedsHeal(charGlobals.partyMemberList, charGlobals.EntityList);
                                if (ptEntOffset > 0)
                                {
                                    SetTarget(charGlobals.apiObject, ptEntOffset);
                                    if (charGlobals.EntityList[ptEntOffset].health < bigHealPct)
                                        UseSkillByName(charGlobals.apiObject, charGlobals.playerInfo.Skills, hlrBigHeal);
                                    else if (charGlobals.EntityList[ptEntOffset].health < smallHealPct)
                                        UseSkillByName(charGlobals.apiObject, charGlobals.playerInfo.Skills, hlrSmallHeal);

                                }
                            }
                        }
                        //We made it into the cast loop so reset the cast timeout
                        castTimeout = timeoutMax;
                    }

                }

                Thread.Sleep(threadSleep);
            }
            CharGlobals finalGlobals = CharGlobalDict[procId];
            //Make sure we aren't moving and clean our inventory
            SetPlayerFwdSpeed(finalGlobals.apiObject, true, 0);
            SetPlayerStrafeSpeed(finalGlobals.apiObject, true, 0);

            clean_inventory(procId);

            int goleCheck = findEntityByName(finalGlobals.EntityList, "Golestandt", true);
            if (goleCheck == 0)
            {
                //Make sure group is healed up before resetting
                if (charGlobals.playerInfo.className.Contains(brdClass) || charGlobals.playerInfo.className.Contains(hlrClass))
                {
                    while (dragonLooping && GetPartyAverageHP(charGlobals.partyMemberList) < 95)
                    {
                        UpdateGlobals(procId);
                        charGlobals = CharGlobalDict[procId];
                        //Check if we're dead, if so return
                        if (charGlobals.EntityList[charGlobals.playerInfo.entListIndex].isDead == 1)
                        {
                            return;
                        }

                        if (charGlobals.playerInfo.className.Contains(brdClass))
                        {
                            //check if need to group heal
                            if (GetPartyAverageHP(charGlobals.partyMemberList) < 75)
                            {
                                UseSkillByName(charGlobals.apiObject, charGlobals.playerInfo.Skills, brdGrpHeal);
                            }
                            else
                            {
                                //check if a party member needs heal
                                int ptEntOffset = PartyMemberNeedsHeal(charGlobals.partyMemberList, charGlobals.EntityList);
                                if (ptEntOffset > 0)
                                {
                                    SetTarget(charGlobals.apiObject, ptEntOffset);
                                    if (charGlobals.EntityList[ptEntOffset].health < bigHealPct)
                                        UseSkillByName(charGlobals.apiObject, charGlobals.playerInfo.Skills, brdBigHeal);
                                    else if (charGlobals.EntityList[ptEntOffset].health < smallHealPct)
                                        UseSkillByName(charGlobals.apiObject, charGlobals.playerInfo.Skills, brdSmallHeal);

                                }
                            }


                        }
                        else if (charGlobals.playerInfo.className.Contains(hlrClass))
                        {
                            //first check for group heal
                            if (GetPartyAverageHP(charGlobals.partyMemberList) < 85)
                            {
                                UseSkillByName(charGlobals.apiObject, charGlobals.playerInfo.Skills, hlrGrpHeal);
                            }
                            else
                            {
                                //check if a party member needs heal
                                int ptEntOffset = PartyMemberNeedsHeal(charGlobals.partyMemberList, charGlobals.EntityList);
                                if (ptEntOffset > 0)
                                {
                                    SetTarget(charGlobals.apiObject, ptEntOffset);
                                    if (charGlobals.EntityList[ptEntOffset].health < bigHealPct)
                                        UseSkillByName(charGlobals.apiObject, charGlobals.playerInfo.Skills, hlrBigHeal);
                                    else if (charGlobals.EntityList[ptEntOffset].health < smallHealPct)
                                        UseSkillByName(charGlobals.apiObject, charGlobals.playerInfo.Skills, hlrSmallHeal);

                                }
                            }
                        }
                    }
                }

                dragonRunning = false;
                finalGlobals.currentState = DragonState.reset;
                CharGlobalDict[procId] = finalGlobals;
            } else
            {
                finalGlobals.currentState = DragonState.fighting;
                dragonRunning = true;
                CharGlobalDict[procId] = finalGlobals;
            }


        }

        public static void stickFunc(int procId)
        {
            //UpdateGlobals(procId);

            CharGlobals charGlobals = CharGlobalDict[procId];
            //while (stickRunning)
            //{
                //UpdateGlobals(procId);
                //if (charNames.ContainsKey(leaderName))
                //    UpdateGlobals(charNames[leaderName]);
                

                

            string plyrName = new string(charGlobals.playerInfo.name);

            CharGlobals stickGlobals;
            if (charNames.ContainsKey(leaderName))
                stickGlobals = CharGlobalDict[charNames[leaderName]];
            else
                return;


            if (!plyrName.Equals(leaderName))
            {
                float stoppingDist = 25.0f;


                float dist = DistanceToPoint(charGlobals.playerPos, stickGlobals.playerPos.pos_x, stickGlobals.playerPos.pos_y);
                short newheading = GetGameHeading(charGlobals.playerPos, stickGlobals.playerPos.pos_x, stickGlobals.playerPos.pos_y);
                if (dist > stoppingDist)
                {
                    SetAutorun(charGlobals.apiObject, true);
                    SetPlayerHeading(charGlobals.apiObject, true, newheading);

                }
                else
                {
                    //have to convert the heading
                    SetPlayerHeading(charGlobals.apiObject, true, ConvertCharHeading(stickGlobals.playerPos.heading));
                    //SetPlayerHeading(apiObject, false, 0);
                    SetAutorun(charGlobals.apiObject, false);
                    SetPlayerFwdSpeed(charGlobals.apiObject, true, 0);
                    SetPlayerFwdSpeed(charGlobals.apiObject, false, 0);
                }
            }
                //Thread.Sleep(100);
            //}
            //SetPlayerHeading(charGlobals.apiObject, false, 0);
            //SetAutorun(charGlobals.apiObject, false);
            //SetPlayerFwdSpeed(charGlobals.apiObject, false, 0);
        }

        public static void getBuffs(int procId)
        {

            if (charNames.ContainsKey(leaderName))
            {
                UpdateGlobals(procId);
                UpdateGlobals(charNames[leaderName]);

                CharGlobals stickGlobals;

                stickGlobals = CharGlobalDict[charNames[leaderName]];
                CharGlobals charGlobals = CharGlobalDict[procId];

                //Get the name of the target from the leaders entity list
                string targName = stickGlobals.EntityList[stickGlobals.targetInfo.entOffset].name;
                string buffItem = "Full Buffs";

                //Find that entity in the current players entity list
                int entOffset = findEntityByName(charGlobals.EntityList, targName);

                if (!String.IsNullOrEmpty(targName))
                {
                    //string[] args = tmp.Split(',');
                    //int entOffset = -1;
                    //entOffset = findEntityByName(EntityList, args[0]);
                    if (entOffset >= 0)
                    {
                        SetTarget(charGlobals.apiObject, entOffset);
                        InteractRequest(charGlobals.apiObject, charGlobals.EntityList[entOffset].objectId);
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
                        UpdateGlobals(procId);
                        charGlobals = CharGlobalDict[procId];
                        Thread.Sleep(500);
                        int fromSlot = 0;
                        fromSlot = ItemSlotByName(charGlobals.playerInfo.Inventory, buffItem);
                        if (fromSlot > 0)
                        {
                            //Add 1000 to objectId for moving item to NPCs
                            MoveItem(charGlobals.apiObject, fromSlot, charGlobals.EntityList[entOffset].objectId + 1000, 0);
                        }
                        Marshal.FreeHGlobal(pktbuf);
                    }
                }

                Thread.Sleep(100);
            }


        }

        public static void HealFunc(int procId)
        {
            int threadSleep = 250; // milliseconds
            int castSleep = 0; // milliseconds

            //This is a countdown that gets reset after a cast to prevent spell spamming
            int timeoutMax = 10;
            int castTimeout = 0;

            bool isCasting = false;

            //Bard
            string brdClass = drgSettings.dragonFight.bard.className;
            //songs
            string speedSong = drgSettings.dragonFight.bard.buff1;
            bool brdSpeedOn = false;
            string powSong = drgSettings.dragonFight.bard.buff2;
            bool brdPowOn = false;
            string endSong = drgSettings.dragonFight.bard.buff3;
            bool brdEndOn = false;
            string healSong = drgSettings.dragonFight.bard.buff4;
            bool brdHealOn = false;
            //buff
            string ablBuff = drgSettings.dragonFight.bard.buff5;
            //heals
            int smallHealPct = drgSettings.dragonFight.bard.heal1pct;
            int bigHealPct = drgSettings.dragonFight.bard.heal2pct;
            string brdSmallHeal = drgSettings.dragonFight.bard.heal1;
            string brdBigHeal = drgSettings.dragonFight.bard.heal2;
            string brdGrpHeal = drgSettings.dragonFight.bard.grpHeal1;
            int grpHealPct = drgSettings.dragonFight.bard.grpHeal1pct;

            //Healer
            string hlrClass = drgSettings.dragonFight.healer.className;
            //resist buff
            string hlrResistBuff = drgSettings.dragonFight.healer.buff1;
            bool hlrBuffOn = false;
            //heals
            string hlrSmallHeal = drgSettings.dragonFight.healer.heal1;
            string hlrBigHeal = drgSettings.dragonFight.healer.heal2;
            string hlrGrpHeal = drgSettings.dragonFight.healer.grpHeal1;

            //while (healRunning)
            //{
                //UpdateGlobals(procId);

                CharGlobals charGlobals = CharGlobalDict[procId];
                string plyrName = new string(charGlobals.playerInfo.name);

                string className = new string(charGlobals.playerInfo.className);

                //Check if we're casting and have more than castSleep left
                if (charGlobals.EntityList[charGlobals.playerInfo.entListIndex].castCountdown > castSleep)
                {
                    isCasting = true;
                }
                else
                {
                    isCasting = false;
                    castTimeout -= 1;
                    if (castTimeout < 0)
                        castTimeout = 0;
                }

                //if cast timeout is done
                if (!isCasting)
                {
                    //Heal check
                    //******single target heals not working at all
                    if (charGlobals.playerInfo.className.Contains(brdClass))
                    {
                        //check if need to group heal
                        if (GetPartyAverageHP(charGlobals.partyMemberList) < grpHealPct)
                        {
                            UseSkillByName(charGlobals.apiObject, charGlobals.playerInfo.Skills, brdGrpHeal);
                        } else
                        {
                            //check if a party member needs heal
                            int ptEntOffset = PartyMemberNeedsHeal(charGlobals.partyMemberList, charGlobals.EntityList);
                            if (ptEntOffset > 0)
                            {
                                SetTarget(charGlobals.apiObject, ptEntOffset);
                                if (charGlobals.EntityList[ptEntOffset].health < bigHealPct)
                                    UseSkillByName(charGlobals.apiObject, charGlobals.playerInfo.Skills, brdBigHeal);
                                else if (charGlobals.EntityList[ptEntOffset].health < smallHealPct)
                                    UseSkillByName(charGlobals.apiObject, charGlobals.playerInfo.Skills, brdSmallHeal);

                            }
                        }


                    }
                    else if (charGlobals.playerInfo.className.Contains(hlrClass))
                    {
                        //first check for group heal
                        if (GetPartyAverageHP(charGlobals.partyMemberList) < grpHealPct)
                        {
                            UseSkillByName(charGlobals.apiObject, charGlobals.playerInfo.Skills, hlrGrpHeal);
                        } else
                        {
                            //check if a party member needs heal
                            int ptEntOffset = PartyMemberNeedsHeal(charGlobals.partyMemberList, charGlobals.EntityList);
                            if (ptEntOffset > 0)
                            {
                                SetTarget(charGlobals.apiObject, ptEntOffset);
                                if (charGlobals.EntityList[ptEntOffset].health < bigHealPct)
                                    UseSkillByName(charGlobals.apiObject, charGlobals.playerInfo.Skills, hlrBigHeal);
                                else if (charGlobals.EntityList[ptEntOffset].health < smallHealPct)
                                    UseSkillByName(charGlobals.apiObject, charGlobals.playerInfo.Skills, hlrSmallHeal);
                            }
                        }
                    }
                    //else
                    //{
                    //    //if we're not a healer break loop and end thread
                    //    break;
                    //}
                    //Buff check
                    if (charGlobals.playerInfo.className.Contains(brdClass))
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
                    //Reset cast timeout
                    castTimeout = timeoutMax;
                }




                //Thread.Sleep(threadSleep);
            //}

        }

        public static void clean_inventory(int procId)
        {
            UpdateGlobals(procId);
            CharGlobals charGlobals;
            if (CharGlobalDict.ContainsKey(procId))
                charGlobals = CharGlobalDict[procId];
            else
                return;

            IntPtr apiObject = charGlobals.apiObject;

            //List<string> badItems = new List<string>();
            //
            //badItems.Add("Life Stone");
            //badItems.Add("Blood Stone");

            //look through all slots
            for (int slotNum = 40; slotNum < 80; slotNum++)
            {
                string itemName = charGlobals.playerInfo.Inventory[slotNum - 40].name;
                if (drgSettings.itemFilter.badItems.Contains(itemName))
                {
                    //Move item to slot 0 to drop it
                    MoveItem(apiObject, slotNum, 0, 0);
                }
                Thread.Sleep(100);
            }
        }
    }
}
