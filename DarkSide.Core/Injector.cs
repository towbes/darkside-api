using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using DarkSide.Core.Wrappers;

namespace DarkSide.Core
{
    public class Injector
    {
        //demo code assuming one instance of the game wihtout injected dll is running
        public static IntPtr apiObject;
        public Injector()
        {
            var localByName = Process.GetProcessesByName("game.dll");
            apiObject = DarkSideApiWrapper.CreateDarksideAPI();
            DarkSideApiWrapper.InjectPid(apiObject, localByName[0].Id);
            var bla = new Stuff();
            IntPtr outputIntPtr=new IntPtr();
            DarkSideApiWrapper.GetPlayerPosition(apiObject,outputIntPtr);
            
        }

        //[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        //public struct PlayerPosition
        //{
        //    public float pos_x { get; private set; }
        //    public short heading { get; private set; }
        //    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 68)]
        //    public char[] unknown1;
        //    public float pos_y { get; private set; }
        //    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        //    public char[] unknown2;
        //    public float pos_z { get; private set; }
        //    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
        //    public char[] unknown3;
        //}
    }
}
