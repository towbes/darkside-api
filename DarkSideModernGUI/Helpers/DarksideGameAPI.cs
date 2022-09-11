using System;
using System.Collections.Generic;
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
            public float momentumLeftRight { get; private set; }
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)] public char[] unknown3;
            public float momentumFwdBackWritable { get; private set; }
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
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 72)] public String name;
            public int unknown1 { get; private set; }
        }
        //useSpell_t
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct Spell_t
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)] public String name;
            public short spellLevel { get; private set; }
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 26)] public char[] unknown1;
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

        public static int findEntityByName(List<EntityInfo> EntityList, String entName)
        {
            entName = entName.ToLower();
            //Skip index 0 for now
            for (int i = 1; i < 2000; i++)
            {
                if (!String.IsNullOrEmpty(EntityList[i].name))
                {
                    if (EntityList[i].name.ToLower().StartsWith(entName))
                    {
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
            for (int slotNum = 0; slotNum < 41; slotNum++)
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

        public static int UseSkillByName(Skill_t[] skills, string skillName)
        {
            skillName = skillName.ToLower();
            for (int i = 0; i < 150; i++)
            {
                if (skills[i].name.ToLower().StartsWith(skillName))
                {
                    return i;
                }
            }
            //return 999 if failed
            return 999;
        }

        //Returns spell category and level
        public static (int category, int level) UseSpellByName(spellCategory_t[] spellLines, string spellName)
        {
            spellName = spellName.ToLower();
            for (int cat = 0; cat < 8; cat++)
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
        public static bool UsePetCmdByName(IntPtr apiObject, string petCommand)
        {
            //pet window packet function
            //aggroState // 1-Aggressive, 2-Deffensive, 3-Passive
            //walkState // 1-Follow, 2-Stay, 3-GoTarg, 4-Here
            //command // 1-Attack, 2-Release
            if (petCommand.ToLower().StartsWith("passive")) {
                return UsePetCmd(apiObject, 3, 1, 0);

            } else if (petCommand.ToLower().StartsWith("attack")) {
                return UsePetCmd(apiObject, 2, 1, 1);
            }
            return false;
        }


    }
}
