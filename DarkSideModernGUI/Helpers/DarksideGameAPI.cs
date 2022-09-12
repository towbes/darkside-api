using DarkSideModernGUI.Views.Pages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static DarkSideModernGUI.Helpers.DarksideGameAPI;


namespace DarkSideModernGUI.Helpers
{
    public class DarksideGameAPI
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct PlayerPosition
        {
            public float pos_x { get; private set; }
            public short heading { get; private set; }
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)] public char[] unknown1;
            public int plyrSpeedFwd { get; private set; }
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)] public char[] unknown2;
            public int momentumMaxFwdBack { get; private set; }
            public float momentumFwdBack { get; private set; }
            public float momentumLeftRight { get; private set; } // Left = Positive, Right=Negative | Nospeed: 137 | (204%)Bard Speed: 224
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)] public char[] unknown3;
            public float momentumFwdBackWritable { get; private set; } //Max values:  Nospeed: 238  | (204%)Bard Speed: 389
            public int unknown4 { get; private set; }
            public float pos_y { get; private set; }
            public float writeablePos_Zadd { get; private set; }
            public int unknown5 { get; private set; }            
            public float pos_z { get; private set; }
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public char[] unknown6;
            public int rotatePlayer { get; private set; }

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
            public int castCountdown { get; private set; }
            public int entityStatus { get; private set; } // 8 = idle
            public short isDead { get; private set; }
        }
        //Playerinfo struct
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct EntityList
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2000)] public EntityInfo[] EntList;

        }

        //useSkill_t
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct Skill_t
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)] public String name;
            public int unknown1 { get; private set; }
            public int unknown2 { get; private set; }
            public int tickCount { get; private set; }
        }
        //useSpell_t
        //struct useSpell_t
        //{
        //    char name[64];
        //    short spellLevel;
        //    short unknown1;
        //    int tickCount;
        //    int unknown2;
        //    int unknown3;
        //    int unknown4;
        //    int unknown5;
        //    int unknown6;
        //};
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct Spell_t
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)] public String name;
            public short spellLevel { get; private set; }
            public short unknown1 { get; private set; }
            public int tickCount { get; private set; }
            public int unknown2 { get; private set; }
            public int unknown3 { get; private set; }
            public int unknown4 { get; private set; }
            public int unknown5 { get; private set; }
            public int unknown6 { get; private set; }
        }
        //spellCategory_t
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct spellCategory_t
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)] public String name;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 75)] public Spell_t[] spells;
            public int align { get; private set; }
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
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 168)] public String name;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 168)] public String className;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 150)] public Skill_t[] Skills;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] public spellCategory_t[] SpellLines;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 75)] public Buff_t[] Buffs;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)] public Item_t[] Inventory;
            public int entListIndex { get; private set; }
            public int petEntIndex { get; private set; }
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
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 2048)] public String chatLine;
        }


        //CmdBuffer
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct CmdBuffer
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)] public String sendCmd;
        }

        //SendPktBuffer
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct PktBuffer
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 2048)] public String sendPkt;
        }


        [DllImport("darkside-api.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr CreateDarksideAPI();

        [DllImport("darkside-api.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void DisposeDarksideAPI(IntPtr pApiObject);

        [DllImport("darkside-api.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool InjectPid(IntPtr pApiObject, int pid);
        [DllImport("darkside-api.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetPid(IntPtr pApiObject);
        [DllImport("darkside-api.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Unload(IntPtr pApiObject, int pid);

        [DllImport("darkside-api.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetPlayerPosition(IntPtr pApiObject, IntPtr lpBuffer);
        [DllImport("darkside-api.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetPlayerHeading(IntPtr pApiObject, bool changeHeading, short newHeading);
        [DllImport("darkside-api.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetPlayerFwdSpeed(IntPtr pApiObject, bool changeSpeed, float newSpeed);
        [DllImport("darkside-api.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetPlayerStrafeSpeed(IntPtr pApiObject, bool changeSpeed, float newSpeed);

        [DllImport("darkside-api.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetAutorun(IntPtr pApiObject, bool autorun);
        [DllImport("darkside-api.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GetPartyMember(IntPtr pApiObject, int memberIndex, IntPtr lpBuffer);
        [DllImport("darkside-api.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GetEntityInfo(IntPtr pApiObject, int entityIndex, IntPtr lpBuffer);
        [DllImport("darkside-api.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GetEntityList(IntPtr pApiObject, IntPtr lpBuffer);
        [DllImport("darkside-api.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GetPlayerInfo(IntPtr pApiObject, IntPtr lpBuffer);
        [DllImport("darkside-api.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GetTargetInfo(IntPtr pApiObject, IntPtr lpBuffer);
        [DllImport("darkside-api.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool SetTarget(IntPtr pApiObject, int entOffset);
        [DllImport("darkside-api.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool InteractRequest(IntPtr pApiObject, short objId);
        [DllImport("darkside-api.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool UseSkill(IntPtr pApiObject, int skillOffset);
        [DllImport("darkside-api.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool UseSpell(IntPtr pApiObject, int spellCategory, int spellLevel);
        [DllImport("darkside-api.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool UsePetCmd(IntPtr pApiObject, int aggroState, int walkState, int petCmd);
        [DllImport("darkside-api.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool MoveItem(IntPtr pApiObject, int fromSlot, int toSlot, int count);
        [DllImport("darkside-api.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GetChatline(IntPtr pApiObject, IntPtr lpBuffer);
        [DllImport("darkside-api.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool SendCommand(IntPtr pApiObject, int cmdMode, int iMode, IntPtr lpBuffer);
        //SendPacket expects an ascii hex buffer with packet header as first byte ie:
        //78 00 05 55 03 00 07 78 87 00 02 00 00 01 00 00 00 00
        [DllImport("darkside-api.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool SendPacket(IntPtr pApiObject, IntPtr lpBuffer);


        public static int findEntityByName(List<EntityInfo> EntityList, String entName, bool exactMatch=false)
        {
            entName = entName.ToLower();
            //Skip index 0 for now
            for (int i = 1; i < EntityList.Count; i++)
            {
                if (!String.IsNullOrEmpty(EntityList[i].name))
                {   
                    if (exactMatch)
                    {
                        if (EntityList[i].name.ToLower().Equals(entName))
                            return i;
                    }
                    else 
                    {
                        if (EntityList[i].name.ToLower().StartsWith(entName))
                            return i;
                    }
                }

            }
            return 0;
        }

        public static int ItemSlotByName(Item_t[] inventory, String itemName)
        {
            itemName = itemName.ToLower();
            //add +40 at the end to get proper slot number
            for (int slotNum = 0; slotNum < inventory.Length; slotNum++)
            {
                if (!String.IsNullOrEmpty(inventory[slotNum].name))
                {
                    if (inventory[slotNum].name.ToLower().StartsWith(itemName))
                    {
                        return slotNum + 40;
                    }
                }
            }
            return 0;
        }


        public static bool HasBuffByName(Buff_t[] buffs, string buffName)
        {
            buffName = buffName.ToLower();
            for (int i = 0; i < buffs.Length; i++)
            {
                if (buffs[i].name.ToLower().StartsWith(buffName))
                {
                    return true;
                }
            }
            
            return false;
        }

        public static int GetSkillByName(Skill_t[] skills, string skillName)
        {
            skillName = skillName.ToLower();
            for (int i = 0; i < skills.Length; i++)
            {
                if (skills[i].name.ToLower().StartsWith(skillName))
                {
                    return i;
                }
            }
            //return 999 if failed
            return 999;
        }

        //This function auto checks recast to avoid additional lookups
        public static bool UseSkillByName(IntPtr apiObject, Skill_t[] skills, string skillName)
        {
            int skillNum = GetSkillByName(skills, skillName);
            if (skillNum == 999)
            {
                return false;
            }
            //Check recast
            if (RecastSkillByOffset(skills, skillNum) == 0)
            {
                if (UseSkill(apiObject, skillNum))
                {
                    return true;
                }
            }
            return false;

        }

        public static int RecastSkillByOffset(Skill_t[] skills, int skillNum)
        {
            int currTick = Environment.TickCount;
            int recast = skills[skillNum].tickCount - currTick;
            if (recast < 0)
                recast = 0;
            return recast;
        }

        public static int RecastSkillByName(Skill_t[] skills, string skillName)
        {
            int currTick = Environment.TickCount;
            int skillNum = GetSkillByName(skills, skillName);
            int recast = skills[skillNum].tickCount - currTick;
            if (recast < 0)
                recast = 0;
            return recast;
        }

        //Returns spell category and level
        public static (int category, int level) GetSpellByName(spellCategory_t[] spellLines, string spellName)
        {
            spellName = spellName.ToLower();
            for (int cat = 0; cat < spellLines.Length; cat++)
            {
                for (int i = 0; i < 75; i++) {
                    string curSpellName = spellLines[cat].spells[i].name.ToLower();
                    int curSpellLevel = spellLines[cat].spells[i].spellLevel;
                    if (!string.IsNullOrEmpty(curSpellName)) {
                        if (curSpellName.StartsWith(spellName))
                        {
                            return (cat, curSpellLevel);
                        }
                    }

                }

            }
            //return 999 if failed
            return (999,999);
        }

        public static bool UseSpellByName(IntPtr apiObject, spellCategory_t[] spellLines, string spellName)
        {
            (int cat, int lvl) = GetSpellByName(spellLines, spellName);
            if (cat == 999)
            {
                return false;
            }
            //Check if recast is up
            if (RecastSpellByOffset(spellLines, cat, lvl) == 0)
            {
                if (UseSpell(apiObject, cat, lvl))
                {
                    return true;
                }
            }
            return false;
        }

        public static int RecastSpellByName(spellCategory_t[] spellLines, string spellName)
        {
            int currTick = Environment.TickCount;
            (int spellCat, int spellLevel) = GetSpellByName(spellLines, spellName);
            int recast = spellLines[spellCat].spells[spellLevel].tickCount - currTick;
            if (recast < 0)
                recast = 0;
            return recast;
        }

        public static int RecastSpellByOffset(spellCategory_t[] spellLines, int spellCat, int spellLevel)
        {
            int currTick = Environment.TickCount;
            int recast = spellLines[spellCat].spells[spellLevel].tickCount - currTick;
            if (recast < 0)
                recast = 0;
            return recast;
        }

        public static bool UsePetCmdByName(IntPtr apiObject, string petCommand)
        {
            //pet window packet function
            //aggroState // 1-Aggressive, 2-Deffensive, 3-Passive
            //walkState // 1-Follow, 2-Stay, 3-GoTarg, 4-Here
            //command // 1-Attack, 2-Release
            if (petCommand.ToLower().StartsWith("passive")) {
                return UsePetCmd(apiObject, 3, 0, 0);

            } else if (petCommand.ToLower().StartsWith("attack")) {
                return UsePetCmd(apiObject, 0, 0, 1);
            }
            return false;
        }

        //Set Target Helpers
        public static bool SetTargetByName(IntPtr apiObject, List<EntityInfo> EntityList, string name)
        {
            int targ = findEntityByName(EntityList, name);

            return SetTarget(DashboardPage.apiObject, targ);
        }

        //Returns the entitylist offset for party member that needs heal
        public static int PartyMemberNeedsHeal(List<PartyMemberInfo> partyMemberList, List<EntityInfo> EntityList)
        {
            for (int j = 0; j < partyMemberList.Count; j++)
            {
                String cname = new string(partyMemberList[j].name);

                cname = cname.Trim('\0');
                //Check if someone needs heal
                if (partyMemberList[j].hp_pct < 95)
                {
                    int targ = findEntityByName(EntityList, cname);
                    return targ;
                }
            }
            //return 0 if not found
            return 0;
        }

        public static int GetPartyAverageHP(List<PartyMemberInfo> partyMemberList)
        {
            //If there are party members calculate average, otherwise just return 100
            if (partyMemberList.Count > 0)
            {
                int totalhp = 0;
                int hpavg = 0;
                for (int j = 0; j < partyMemberList.Count; j++)
                {
                    totalhp += partyMemberList[j].hp_pct;
                }

                hpavg = totalhp / partyMemberList.Count;

                return hpavg;
            }
            else
            {
                return 100;
            }
            
        }
    }
}
