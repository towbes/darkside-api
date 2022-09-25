using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DarkSide.Core.Wrappers
{
    [StructLayout(LayoutKind.Sequential,CharSet = CharSet.Ansi)]
    public class Stuff
    {
        private float _X;

        public float X
        {
            get { return _X; }
            set { _X = value; }
        }
        private float _Y;

        public float Y
        {
            get { return _Y; }
            set { _Y = value; }
        }
        private int _Z;

        public int Z
        {
            get { return _Z; }
            set { _Z = value; }
        }
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 68)]
        public char[] unknown1;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public char[] unknown2;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
        public char[] unknown3;
        
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
