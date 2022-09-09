using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


namespace DarkSideModernGUI.Helpers
{
    public class DarksideGameAPI
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
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 168)] public String name;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 168)] public String className;
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


        //CmdBuffer
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct CmdBuffer
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)] public String sendCmd;
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
        public static extern bool UsePetCmd(IntPtr pApiObject, int aggroState, int walkState, int petCmd);
        [DllImport("darkside-api.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GetChatline(IntPtr pApiObject, IntPtr lpBuffer);
        [DllImport("darkside-api.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool SendCommand(IntPtr pApiObject, int cmdMode, int iMode, IntPtr lpBuffer);

        public static int findEntityByName(List<EntityInfo> EntityList, String entName)
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
    }
}
