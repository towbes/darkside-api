using System;
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
using static DarkSideModernGUI.Views.Pages.ClassSettingsPage;

namespace DarkSideModernGUI.Helpers
{
    internal class CharacterLoops
    {
        public static string leaderName = "Asmoe";
        public static string readyName = "Suzuha";
        public static string tankName = "Okarin";

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

        public static DragonState currentState =  DragonState.idle;
        public static bool dragonLooping = false;

        //Run speed globals
        public static float fwdSpeed = 300;
        public static float strafeSpeed = 187;

        //Dicts to maintain currently injected state
        static public Dictionary<string, int> charNames = new Dictionary<string, int>();
        static public Dictionary<int, CharGlobals> CharGlobalDict = new Dictionary<int, CharGlobals>();

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

        public static void battleLocFunc(int procId)
        {
            int threadsleep = 50;
            bool isMoving = true;
            bool isLooting = false;

            float xloc = 0;
            float yloc = 0;
            short finalheading = 0;
            float lootlocx = 0;
            float lootlocy = 0;

            while (isMoving)
            {
                UpdateGlobals(procId);

                CharGlobals charGlobals = CharGlobalDict[procId];
                string plyrName = new string(charGlobals.playerInfo.name);

                string className = new string(charGlobals.playerInfo.className);


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

                //Set up locs
                if (className.Contains("Paladin"))
                {
                    xloc = 39102f;
                    yloc = 58785f;
                    finalheading = 181;

                    lootlocx = 39102f;
                    lootlocy = 59309f;
                }
                else
                {
                    xloc = 38678f;
                    yloc = 59418f;
                    finalheading = 73;
                }

                float stoppingDist = 25.0f;
                //currentTarget = findEntityByName(EntityList, "Asmoe");
                //SetTarget(charGlobals.apiObject, currentTarget);


                float dist = DistanceToPoint(charGlobals.playerPos, xloc, yloc);
                short newheading = GetGameHeading(charGlobals.playerPos, xloc, yloc);
                if (!isLooting)
                {
                    if (dist > stoppingDist)
                    {
                        SetPlayerFwdSpeed(charGlobals.apiObject, true, fwdSpeed);
                        SetPlayerHeading(charGlobals.apiObject, true, newheading);
                        //dist = DistanceToPoint(charGlobals.playerPos, stickTargPos.pos_x, stickTargPos.pos_y, stickTargPos.pos_z);
                        //newheading = GetGameHeading(charGlobals.playerPos, stickTargPos.pos_x, stickTargPos.pos_y);
                    }
                    else
                    {
                        SetPlayerHeading(charGlobals.apiObject, true, ConvertDirHeading(finalheading));
                        //SetPlayerHeading(charGlobals.apiObject, false, 0);
                        SetPlayerFwdSpeed(charGlobals.apiObject, true, 0);
                        //This isn't turning them the direction of the leader for some reason
                        //
                        isMoving = false;
                        isLooting = true;

                    }
                    
                }

                //tank should get loot first

                if (isLooting && className.Contains("Paladin"))
                {
                    while (isLooting && dragonRunning)
                    {
                        UpdateGlobals(procId);
                        charGlobals = CharGlobalDict[procId];
                        int loot = findEntityByName(charGlobals.EntityList, "Sanguine");
                        if (loot > 0)
                        {

                            SetTarget(charGlobals.apiObject, loot);
                            //float lootdist = DistanceToPoint(charGlobals.playerPos, lootlocx, lootlocy);
                            //short loothead = GetGameHeading(charGlobals.playerPos, lootlocx, lootlocy);
                            float lootdist = DistanceToPoint(charGlobals.playerPos, charGlobals.EntityList[loot].pos_x, charGlobals.EntityList[loot].pos_y);
                            short loothead = GetGameHeading(charGlobals.playerPos, charGlobals.EntityList[loot].pos_x, charGlobals.EntityList[loot].pos_y);
                            if (lootdist > stoppingDist)
                            {
                                SetPlayerFwdSpeed(charGlobals.apiObject, true, fwdSpeed);
                                SetPlayerHeading(charGlobals.apiObject, true, loothead);
                                //dist = DistanceToPoint(charGlobals.playerPos, stickTargPos.pos_x, stickTargPos.pos_y, stickTargPos.pos_z);
                                //newheading = GetGameHeading(charGlobals.playerPos, stickTargPos.pos_x, stickTargPos.pos_y);
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

            while (isMoving)
            {
                UpdateGlobals(procId);



                CharGlobals charGlobals = CharGlobalDict[procId];
                string plyrName = new string(charGlobals.playerInfo.name);

                string className = new string(charGlobals.playerInfo.className);

                //don't move if we're readyName
                if (plyrName.Contains(readyName))
                {
                    break;
                }

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

                float stoppingDist = 10.0f;
                //currentTarget = findEntityByName(EntityList, "Asmoe");
                //SetTarget(charGlobals.apiObject, currentTarget);


                float dist = DistanceToPoint(charGlobals.playerPos, xloc, yloc);
                short newheading = GetGameHeading(charGlobals.playerPos, xloc, yloc);
                if (dist > stoppingDist)
                {
                    //SetPlayerFwdSpeed(charGlobals.apiObject, true, fwdSpeed);
                    SetPlayerFwdSpeed(charGlobals.apiObject, true, fwdSpeed);
                    SetPlayerHeading(charGlobals.apiObject, true, newheading);
                    //dist = DistanceToPoint(charGlobals.playerPos, stickTargPos.pos_x, stickTargPos.pos_y, stickTargPos.pos_z);
                    //newheading = GetGameHeading(charGlobals.playerPos, stickTargPos.pos_x, stickTargPos.pos_y);
                }
                else
                {
                    SetPlayerHeading(charGlobals.apiObject, true, ConvertDirHeading(finalheading));
                    //SetPlayerHeading(charGlobals.apiObject, false, 0);
                    //SetPlayerFwdSpeed(charGlobals.apiObject, true, 0);
                    SetPlayerFwdSpeed(charGlobals.apiObject, true, 0);
                    //This isn't turning them the direction of the leader for some reason
                    //
                    isMoving = false;
                }

                Thread.Sleep(100);
            }
            CharGlobals finalGlobals = CharGlobalDict[procId];
            SetPlayerHeading(finalGlobals.apiObject, false, 0);
            //SetPlayerFwdSpeed(finalGlobals.apiObject, false, 0);
            SetPlayerFwdSpeed(finalGlobals.apiObject, false, 0);

            int goleOffset = findEntityByName(finalGlobals.EntityList, "Golestandt", true);
            string cName = finalGlobals.playerInfo.className;
            //Set the state to battle
            //wait for gole to come back, or not be dead
            while (goleOffset == 0 || finalGlobals.EntityList[goleOffset].isDead == 1)
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
            string tankClass = "Paladin";
            string tankMeleeTaunt = "Rile";
            string tankSpellTaunt = "Infuriate";
            string tankArmorBuff = "Aura of Salvation";
            string tankHealSkill = "Holy Revitalization";
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
            int currentTankPoint = -1;

            //Damage caster
            string dmgClass = "Spiritmaster";
            //pet and buffs
            string petSpell = "Spirit Warrior";
            string shieldSpell = "Superior Suppressive Barrier";
            string absorbSpell = "Suppressive Buffer";
            string grpAbsorb = "Shield of the Einherjar";
            //nuke
            string dmgNuke = "Spirit Annihilation";
            string mlNine = "Summon Mastery";

            //Debuffer
            string dbfClass = "Eldritch";
            string dbfShieldSpell = "Supreme Powerguard";
            string dbfAbsorbSpell = "Barrier of Power";
            string dbfSpell = "Annihilate Soul";
            string dbfStun = "Prismatic Strobe";
            string dbfNS = "Abrogate Sight";

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
            int smallHealPct = 95;
            int bigHealPct = 90;
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

                string plyrName = charGlobals.playerInfo.name;
                //don't move if we're readyName
                if (plyrName.Contains(readyName))
                {
                    break;
                }

                int decimusOffset = findEntityByName(charGlobals.EntityList, "Decimus",true);

                //Check chat for selan message
                //******the chat check doesn't seem to be working but the decimus offset does
                //charGlobals.chatLine.chatLine.Contains("Golestandt") || 
                if (decimusOffset == 0)
                {
                    fightStarted = true;
                } else
                {
                    fightStarted = false;
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
                    int goleOffset = findEntityByName(charGlobals.EntityList, "Golestandt", true);
                    int graniteOffset = findEntityInRadius(charGlobals.EntityList, charGlobals.playerPos, "granite giant", 1500);
                    int cloudOffset = findEntityByName(charGlobals.EntityList, "smoke cloud");
                    int fireOffset = findEntityByName(charGlobals.EntityList, "Fire");

                    //Paladin check for clouds
                    //******this type of movement doesn't work for this.  Maybe just a timed movement instead?
                    if (charGlobals.playerInfo.className.Contains(tankClass))
                    {
                        if (cloudOffset > 0)
                        {
                            float cloudPosX = charGlobals.EntityList[cloudOffset].pos_x;
                            float cloudPosY = charGlobals.EntityList[cloudOffset].pos_y;
                            //If distance to cloud is less than 100
                            if (DistanceToPoint(charGlobals.playerPos, cloudPosX, cloudPosY) <= 30)
                            {
                                //if (currentTankPoint == -1)
                                //{
                                //    currentTankPoint = 1;
                                //}
                                //else
                                //{
                                //    currentTankPoint = -1;
                                //}
                                //Sleep for 6 seconds while the cloud spawns
                                for (int i = 0; i < 5; i ++)
                                {
                                    UpdateGlobals(procId);
                                    CharGlobals cloudGlobals = CharGlobalDict[procId];
                                    //tanknewheading = GetGameHeading(cloudGlobals.playerPos, cloudGlobals.EntityList[goleOffset].pos_x, cloudGlobals.EntityList[goleOffset].pos_y);
                                    SetPlayerStrafeSpeed(cloudGlobals.apiObject, true, strafeSpeed * currentTankPoint);
                                    //SetPlayerHeading(cloudGlobals.apiObject, true, tanknewheading);
                                    //dist = DistanceToPoint(charGlobals.playerPos, stickTargPos.pos_x, stickTargPos.pos_y, stickTargPos.pos_z);
                                    //newheading = GetGameHeading(charGlobals.playerPos, stickTargPos.pos_x, stickTargPos.pos_y);
                                    //Melee Taunt
                                    UseSkillByName(charGlobals.apiObject, charGlobals.playerInfo.Skills, tankMeleeTaunt);
                                    Thread.Sleep(threadSleep);
                                    //Spell taunt
                                    UseSkillByName(charGlobals.apiObject, charGlobals.playerInfo.Skills, tankSpellTaunt);
                                    Thread.Sleep(100);

                                }
                                SetPlayerStrafeSpeed(charGlobals.apiObject, true, 0);
                                SetPlayerStrafeSpeed(charGlobals.apiObject, false, 0);
                                currentTankPoint = currentTankPoint * -1;
                            }
                        }
                        else if (fireOffset > 0)
                        {
                            float firePosX = charGlobals.EntityList[fireOffset].pos_x;
                            float firePosY = charGlobals.EntityList[fireOffset].pos_y;
                            //If distance to cloud is less than 100
                            if (DistanceToPoint(charGlobals.playerPos, firePosX, firePosY) <= 30)
                            {
                                //if (currentTankPoint == -1)
                                //{
                                //    currentTankPoint = 1;
                                //}
                                //else
                                //{
                                //    currentTankPoint = -1;
                                //}
                                //Sleep for 6 seconds while the cloud spawns
                                for (int i = 0; i < 5; i++)
                                {
                                    UpdateGlobals(procId);
                                    CharGlobals fireGlobals = CharGlobalDict[procId];
                                    //tanknewheading = GetGameHeading(cloudGlobals.playerPos, cloudGlobals.EntityList[goleOffset].pos_x, cloudGlobals.EntityList[goleOffset].pos_y);
                                    SetPlayerStrafeSpeed(fireGlobals.apiObject, true, strafeSpeed * currentTankPoint);
                                    //SetPlayerHeading(cloudGlobals.apiObject, true, tanknewheading);
                                    //dist = DistanceToPoint(charGlobals.playerPos, stickTargPos.pos_x, stickTargPos.pos_y, stickTargPos.pos_z);
                                    //newheading = GetGameHeading(charGlobals.playerPos, stickTargPos.pos_x, stickTargPos.pos_y);
                                    //Melee Taunt
                                    UseSkillByName(charGlobals.apiObject, charGlobals.playerInfo.Skills, tankMeleeTaunt);
                                    Thread.Sleep(threadSleep);
                                    //Spell taunt
                                    UseSkillByName(charGlobals.apiObject, charGlobals.playerInfo.Skills, tankSpellTaunt);
                                    Thread.Sleep(100);

                                }
                                SetPlayerStrafeSpeed(charGlobals.apiObject, true, 0);
                                SetPlayerStrafeSpeed(charGlobals.apiObject, false, 0);
                                currentTankPoint = currentTankPoint * -1;
                            }
                        }

                    }

                    //target checks for DPS / Debuff only
                    //always check morella offset first, then check for a granite giant spawn, then gole
                    if (charGlobals.playerInfo.className.Contains(dmgClass) || charGlobals.playerInfo.className.Contains(dbfClass))
                    {
                        if (morellaOffset > 0 && charGlobals.targetInfo.entOffset != morellaOffset)
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
                        
                                }
                            } else
                            {
                                //set the granite offset to 0 if it's too far away so that we target gole instead
                                graniteOffset = 0;
                            }
                        }
                        if ((morellaOffset == 0) && (graniteOffset == 0) && goleOffset > 0 && charGlobals.targetInfo.entOffset != goleOffset)
                        {
                            EntityInfo goleEnt = charGlobals.EntityList[goleOffset];
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
                        if (goleOffset == 0)
                        {
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
                            Thread.Sleep(50);
                            //set to false to avoid locking heading update
                            SetPlayerHeading(charGlobals.apiObject, false, 0);
                            //Melee Taunt
                            UseSkillByName(charGlobals.apiObject, charGlobals.playerInfo.Skills, tankMeleeTaunt);
                            Thread.Sleep(threadSleep);
                            //Spell taunt
                            UseSkillByName(charGlobals.apiObject, charGlobals.playerInfo.Skills, tankSpellTaunt);
                            if (charGlobals.playerInfo.health < 75)
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
                            Thread.Sleep(50);
                            UseSpellByName(charGlobals.apiObject, charGlobals.playerInfo.SpellLines, dbfNS);
                        }

                        //******single target heals not working at all
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
            SetPlayerHeading(finalGlobals.apiObject, true, 0);
            SetPlayerFwdSpeed(finalGlobals.apiObject, true, 0);
            SetPlayerStrafeSpeed(finalGlobals.apiObject, true, 0);
            SetPlayerHeading(finalGlobals.apiObject, false, 0);
            SetPlayerFwdSpeed(finalGlobals.apiObject, false, 0);
            SetPlayerStrafeSpeed(finalGlobals.apiObject, false, 0);
            clean_inventory(procId);

            int goleCheck = findEntityByName(finalGlobals.EntityList, "Golestandt", true);
            if (goleCheck == 0)
            {
                finalGlobals.currentState = DragonState.reset;
                CharGlobalDict[procId] = finalGlobals;
            }


        }

        public static void stickFunc(int procId)
        {

            while (stickRunning)
            {
                UpdateGlobals(procId);
                if (charNames.ContainsKey(leaderName))
                    UpdateGlobals(charNames[leaderName]);

                CharGlobals stickGlobals;
                if (charNames.ContainsKey(leaderName))
                    stickGlobals = CharGlobalDict[charNames[leaderName]];
                else
                    break;

                CharGlobals charGlobals = CharGlobalDict[procId];

                string plyrName = new string(charGlobals.playerInfo.name);
                if (plyrName.Equals(leaderName))
                {
                    break;
                }

                if (!plyrName.Equals(leaderName))
                {
                    float stoppingDist = 25.0f;
                    //currentTarget = findEntityByName(EntityList, "Asmoe");
                    //SetTarget(apiObject, currentTarget);

                    float dist = DistanceToPoint(charGlobals.playerPos, stickGlobals.playerPos.pos_x, stickGlobals.playerPos.pos_y);
                    short newheading = GetGameHeading(charGlobals.playerPos, stickGlobals.playerPos.pos_x, stickGlobals.playerPos.pos_y);
                    if (dist > stoppingDist)
                    {
                        //SetPlayerFwdSpeed(charGlobals.apiObject, true, stickGlobals.playerPos.momentumFwdBackWritable);
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
                        SetPlayerFwdSpeed(charGlobals.apiObject, true, 0);
                        SetPlayerFwdSpeed(charGlobals.apiObject, false, 0);
                    }
                }
                Thread.Sleep(100);
            }
            CharGlobals charGlobalFinish = CharGlobalDict[procId];
            SetPlayerHeading(charGlobalFinish.apiObject, false, 0);
            SetAutorun(charGlobalFinish.apiObject, false);
            SetPlayerFwdSpeed(charGlobalFinish.apiObject, false, 0);
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
            int smallHealPct = 95;
            int bigHealPct = 70;
            string brdSmallHeal = "Minor Apotheosis";
            string brdBigHeal = "Major Apotheosis";
            string brdGrpHeal = "Group Apotheosis";

            //Healer
            string hlrClass = "Druid";
            //resist buff
            string hlrResistBuff = "Warmth of the Bear";
            bool hlrBuffOn = false;
            //heals
            string hlrSmallHeal = "Minor Apotheosis";
            string hlrBigHeal = "Major Renascence";
            string hlrGrpHeal = "Group Apotheosis";

            while (healRunning)
            {
                UpdateGlobals(procId);

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
                        if (GetPartyAverageHP(charGlobals.partyMemberList) < 75)
                        {
                            UseSkillByName(charGlobals.apiObject, charGlobals.playerInfo.Skills, brdGrpHeal);
                        } else
                        {
                            //check if a party member needs heal
                            int ptEntOffset = PartyMemberNeedsHeal(charGlobals.partyMemberList, charGlobals.EntityList);
                            if (ptEntOffset > 0)
                            {
                                SetTarget(charGlobals.apiObject, ptEntOffset);
                                if (charGlobals.EntityList[ptEntOffset].health < smallHealPct)
                                    UseSkillByName(charGlobals.apiObject, charGlobals.playerInfo.Skills, brdSmallHeal);
                                else if (charGlobals.EntityList[ptEntOffset].health < bigHealPct)
                                    UseSkillByName(charGlobals.apiObject, charGlobals.playerInfo.Skills, brdBigHeal);
                            }
                        }


                    }
                    else if (charGlobals.playerInfo.className.Contains(hlrClass))
                    {
                        //first check for group heal
                        if (GetPartyAverageHP(charGlobals.partyMemberList) < 85)
                        {
                            UseSkillByName(charGlobals.apiObject, charGlobals.playerInfo.Skills, hlrGrpHeal);
                        } else
                        {
                            //check if a party member needs heal
                            int ptEntOffset = PartyMemberNeedsHeal(charGlobals.partyMemberList, charGlobals.EntityList);
                            if (ptEntOffset > 0)
                            {
                                SetTarget(charGlobals.apiObject, ptEntOffset);
                                if (charGlobals.EntityList[ptEntOffset].health < smallHealPct)
                                    UseSkillByName(charGlobals.apiObject, charGlobals.playerInfo.Skills, hlrSmallHeal);
                                else if (charGlobals.EntityList[ptEntOffset].health < bigHealPct)
                                    UseSkillByName(charGlobals.apiObject, charGlobals.playerInfo.Skills, hlrBigHeal);
                            }
                        }
                    }
                    else
                    {
                        //if we're not a healer break loop and end thread
                        break;
                    }
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




                Thread.Sleep(threadSleep);
            }

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

            List<string> badItems = new List<string>();

            badItems.Add("Life Stone");
            badItems.Add("Blood Stone");

            //look through all slots
            for (int slotNum = 40; slotNum < 80; slotNum++)
            {
                string itemName = charGlobals.playerInfo.Inventory[slotNum - 40].name;
                if (badItems.Contains(itemName))
                {
                    //Move item to slot 0 to drop it
                    MoveItem(apiObject, slotNum, 0, 0);
                }
                Thread.Sleep(100);
            }
        }
    }
}
