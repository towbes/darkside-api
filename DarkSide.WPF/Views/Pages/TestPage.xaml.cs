using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using DarkSide.WPF.ViewModels;
using Wpf.Ui.Common.Interfaces;

namespace DarkSide.WPF.Views.Pages;

/// <summary>
///     Interaction logic for TestPage.xaml
/// </summary>
public partial class TestPage : INavigableView<TestViewModel>
{
    public static IntPtr apiObject;

    private readonly List<EntityInfo> EntityList = new();
    //PartyList
    private readonly List<PartyMemberInfo> partyMemberList = new();
    private readonly List<string> strEntityList = new();
    private readonly List<string> strPartyList = new();
    private readonly List<string> strPlayerInfo = new();
    private readonly List<string> strPlayerPos = new();
    private bool autorun;
    private bool changeHeading = false;

    private DispatcherTimer dispatcherTimer;

    private bool loopRunning;

    public TestPage(TestViewModel viewModel)
    {
        ViewModel = viewModel;

        InitializeComponent();
    }

    public TestViewModel ViewModel { get; }

    [DllImport("darkside-api.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr CreateDarksideAPI();

    [DllImport("darkside-api.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr DisposeDarksideAPI(IntPtr pApiObject);

    [DllImport("darkside-api.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void InjectPid(IntPtr pApiObject, int pid);

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
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            //update ever 100ms
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            dispatcherTimer.Start();
            loopRunning = true;
        }
        else
        {
            dispatcherTimer.Stop();
            loopRunning = false;
        }
    }

    private void dispatcherTimer_Tick(object sender, EventArgs e)
    {
        //Update entity table
        EntityList.Clear();
        strEntityList.Clear();

        for (var i = 0; i < 2000; i++)
        {
            var entbuf = Marshal.AllocHGlobal(Marshal.SizeOf<EntityInfo>());
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

        var entmsg = "";

        for (var j = 0; j < EntityList.Count; j++)
        {
            if (EntityList[j].objectId > 0)
            {
                var entity = EntityList[j];

                if (entity.objectId > 0)
                {
                    var cname = new string(entity.name);

                    entmsg = string.Format(
                        "{0}: Name is {1} - HP: {2}% - Type: {3} - Level: {4}",
                        j, cname, entity.health, entity.type, entity.level);

                    strEntityList.Add(entmsg);
                }
            }
        }

        EntityInfoTextBlock.Text = string.Join(Environment.NewLine, strEntityList);

        strPlayerPos.Clear();
        var buf = Marshal.AllocHGlobal(Marshal.SizeOf<PlayerPosition>());
        GetPlayerPosition(DashboardPage.apiObject, buf);
        var playerPos = (PlayerPosition)Marshal.PtrToStructure(buf, typeof(PlayerPosition));

        var msg = string.Format(
            "PlayerPosition:" + Environment.NewLine +
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
        var tInfobuf = Marshal.AllocHGlobal(Marshal.SizeOf<TargetInfo>());
        GetTargetInfo(DashboardPage.apiObject, tInfobuf);
        var tInfoMsg = "";
        var targetInfo = (TargetInfo)Marshal.PtrToStructure(tInfobuf, typeof(TargetInfo));

        if (!string.IsNullOrEmpty(targetInfo.hasTarget))
        {
            tInfoMsg = string.Format(
                "Ent: {0} - HP:{1} - Col:{2} - {3} - Lvl:{4}",
                targetInfo.entOffset,
                targetInfo.health,
                targetInfo.color,
                targetInfo.name,
                EntityList[targetInfo.entOffset].level);
        }
        else
        {
            tInfoMsg = "No target";
        }

        strPlayerPos.Add(tInfoMsg);
        //
        // Updating the Label which displays the current second
        Marshal.FreeHGlobal(tInfobuf);

        PlayerPositionInfo.Text = string.Join(Environment.NewLine, strPlayerPos);

        //PlayerInfo
        strPlayerInfo.Clear();
        //Size should be 0xdcd4
        var pInfobuf = Marshal.AllocHGlobal(Marshal.SizeOf<PlayerInfo>());
        GetPlayerInfo(DashboardPage.apiObject, pInfobuf);
        var playerInfo = (PlayerInfo)Marshal.PtrToStructure(pInfobuf, typeof(PlayerInfo));

        var pInfoMsg = string.Format(
            "PlayerInfo:" + Environment.NewLine +
            "HP:{0:0} - Pow:{1:0} - Endu:{2:0}",
            playerInfo.health,
            playerInfo.power,
            playerInfo.endu);

        strPlayerInfo.Add(pInfoMsg);
        strPlayerInfo.Add("---Buffs---");

        //items
        for (var i = 0; i < 75; i++)
        {
            var pBuff = string.Format("{0}: {1} ", i, playerInfo.Buffs[i].name);

            if (playerInfo.Buffs[i].buffId > 0)
            {
                strPlayerInfo.Add(pBuff);
            }
        }

        strPlayerInfo.Add("---Items---");

        //items
        for (var i = 0; i < 40; i++)
        {
            var pItem = string.Format("{0}: {1} {2}", i, playerInfo.Inventory[i].id, playerInfo.Inventory[i].name);

            if (!string.IsNullOrEmpty(playerInfo.Inventory[i].name))
            {
                strPlayerInfo.Add(pItem);
            }
        }

        //
        Marshal.FreeHGlobal(pInfobuf);
        PlayerInfoText.Text = string.Join(Environment.NewLine, strPlayerInfo);

        partyMemberList.Clear();
        strPartyList.Clear();

        //party list
        for (var i = 0; i < 8; i++)
        {
            var pbuf = Marshal.AllocHGlobal(Marshal.SizeOf<PartyMemberInfo>());
            GetPartyMember(DashboardPage.apiObject, i, pbuf);
            var partyMember = (PartyMemberInfo)Marshal.PtrToStructure(pbuf, typeof(PartyMemberInfo));

            if (partyMember.hp_pct > 0)
            {
                partyMemberList.Add(partyMember);
            }

            Marshal.FreeHGlobal(pbuf);
        }

        for (var j = 0; j < partyMemberList.Count; j++)
        {
            var cname = new string(partyMemberList[j].name);

            var pmsg = string.Format(
                "{0} - HP: {1}% - Endu: {2}% - Pow: {3}%",
                cname, partyMemberList[j].hp_pct, partyMemberList[j].endu_pct, partyMemberList[j].pow_pct);

            strPartyList.Add(pmsg);

            //Check if someone needs heal
            if (partyMemberList[j].hp_pct < 100)
            {
                var targ = findEntityByName("Suzyqueue");
                SetTarget(DashboardPage.apiObject, targ);
                UseSkill(DashboardPage.apiObject, 17);
            }
        }

        MemberInfo.Text = string.Join(Environment.NewLine, strPartyList);

        // Forcing the CommandManager to raise the RequerySuggested event
        CommandManager.InvalidateRequerySuggested();
    }

    private void Button_Click_3(object sender, RoutedEventArgs e)
    {
        autorun = !autorun;
        SetAutorun(DashboardPage.apiObject, autorun);
    }

    private void Button_Click_4(object sender, RoutedEventArgs e)
    {
        partyMemberList.Clear();
        strPartyList.Clear();

        for (var i = 0; i < 8; i++)
        {
            var buf = Marshal.AllocHGlobal(Marshal.SizeOf<PartyMemberInfo>());
            GetPartyMember(DashboardPage.apiObject, i, buf);
            var partyMember = (PartyMemberInfo)Marshal.PtrToStructure(buf, typeof(PartyMemberInfo));

            if (partyMember.hp_pct > 0)
            {
                partyMemberList.Add(partyMember);
            }

            Marshal.FreeHGlobal(buf);
        }

        for (var j = 0; j < partyMemberList.Count; j++)
        {
            var cname = new string(partyMemberList[j].name);

            var msg = string.Format(
                "{0} - HP: {1}% - Endu: {2}% - Pow: {3}%",
                cname, partyMemberList[j].hp_pct, partyMemberList[j].endu_pct, partyMemberList[j].pow_pct);

            strPartyList.Add(msg);
        }

        MemberInfo.Text = string.Join(Environment.NewLine, strPartyList);
    }

    private int findEntityByName(string entName)
    {
        for (var i = 0; i < 2000; i++)
        {
            if (!string.IsNullOrEmpty(EntityList[i].name))
            {
                if (string.Equals(entName, EntityList[i].name))
                {
                    return i;
                }
            }
        }

        return -1;
    }

    private void Button_Click_SetTarget(object sender, RoutedEventArgs e)
    {
        var tmp = SetTargetOffset.Text;
        var offset = -1;

        //If there are letters, get the offset by name
        if (!tmp.All(char.IsDigit))
        {
            offset = findEntityByName(tmp);
        }
        else
        {
            offset = int.Parse(SetTargetOffset.Text);
        }

        SetTarget(DashboardPage.apiObject, offset);
    }

    private void Button_Click_UseSkill(object sender, RoutedEventArgs e)
    {
        UseSkill(DashboardPage.apiObject, int.Parse(UseSkillOffset.Text));
    }

    private void Button_Click_UseSpell(object sender, RoutedEventArgs e)
    {
        UseSpell(DashboardPage.apiObject, int.Parse(UseSpellOffset.Text));
    }

    private void Button_Click_6(object sender, RoutedEventArgs e)
    {
        EntityList.Clear();

        for (var i = 0; i < 2000; i++)
        {
            var buf = Marshal.AllocHGlobal(Marshal.SizeOf<EntityInfo>());
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

        for (var j = 0; j < EntityList.Count; j++)
        {
            var msg = "";

            if (EntityList[j].objectId > 0)
            {
                var entity = EntityList[j];

                if (entity.objectId > 0)
                {
                    var cname = new string(entity.name);

                    msg = string.Format(
                        "{0}: Name is {1} - HP: {2}% - Type: {3} - Level: {4}",
                        j, cname, entity.health, entity.type, entity.level);

                    strEntityList.Add(msg);
                }
            }
        }

        EntityInfoTextBlock.Text = string.Join(Environment.NewLine, strEntityList);
    }

    private void Button_Click_PlayerInfo(object sender, RoutedEventArgs e)
    {
        //Size should be 0xdcd4
        var pInfobuf = Marshal.AllocHGlobal(Marshal.SizeOf<PlayerInfo>());
        GetPlayerInfo(DashboardPage.apiObject, pInfobuf);
        var playerInfo = (PlayerInfo)Marshal.PtrToStructure(pInfobuf, typeof(PlayerInfo));

        var pInfoMsg = string.Format(
            "PlayerInfo:" + Environment.NewLine +
            "HP:{0:0} - Pow:{1:0} - Endu:{2:0}",
            playerInfo.health,
            playerInfo.power,
            playerInfo.endu);

        //
        // Updating the Label which displays the current second
        Marshal.FreeHGlobal(pInfobuf);
        PlayerInfoText.Text = pInfoMsg;
    }

    //Player position struct
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct PlayerPosition
    {
        public float pos_x { get; private set; }
        public short heading { get; private set; }
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 68)]
        public char[] unknown1;
        public float pos_y { get; private set; }
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public char[] unknown2;
        public float pos_z { get; private set; }
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
        public char[] unknown3;
    }

    //PartyMemberInfo
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct PartyMemberInfo
    {
        public int hp_pct { get; private set; }
        public int endu_pct { get; private set; }
        public int unknown;
        public int pow_pct { get; private set; }
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public char[] name;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
        public char[] unknown1;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
        public char[] class_name;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4616)]
        public char[] unknown2;
    }

    //EntityInfo
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct EntityInfo
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 48)]
        public string name;
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
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 72)]
        public string name;
        public int unknown1 { get; private set; }
    }

    //useSpell_t
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct Spell_t
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string name;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 60)]
        public char[] unknown1;
    }

    //item_t
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct Item_t
    {
        public int id { get; private set; }
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 72)]
        public char[] unknown1;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string name;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 312)]
        public char[] unknown2;
    }

    //buff_t
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct Buff_t
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string name;
        public int unknown1 { get; private set; }
        public int timeRemaining { get; private set; }
        public int slotNum { get; private set; }
        public int tooltipId { get; private set; }
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public char[] unknown2;
        public int buffId { get; private set; }
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public char[] unknown3;
    }

    //Playerinfo struct
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct PlayerInfo
    {
        public int health { get; private set; }
        public int power { get; private set; }
        public int endu { get; private set; }
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 150)]
        public Skill_t[] Skills;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 150)]
        public Spell_t[] Spells;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 75)]
        public Buff_t[] Buffs;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
        public Item_t[] Inventory;
    }

    //Playerinfo struct
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct TargetInfo
    {
        public int entOffset { get; private set; }
        public int health { get; private set; }
        public int color { get; private set; }
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 48)]
        public string name;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
        public string hasTarget;
    }
}