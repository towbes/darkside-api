using System.Runtime.InteropServices;

namespace DarkSide.Core.Wrappers;

public static class DarkSideApiWrapper
{
    [DllImport("darkside-api.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void GetPlayerPosition(IntPtr pApiObject, IntPtr lpBuffer);

    [DllImport("darkside-api.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr CreateDarksideAPI();

    [DllImport("darkside-api.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr DisposeDarksideAPI(IntPtr pApiObject);

    [DllImport("darkside-api.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void InjectPid(IntPtr pApiObject, int pid);

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
}