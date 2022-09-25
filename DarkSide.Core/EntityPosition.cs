using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DarkSide.Core
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class EntityPosition
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
        private float _Z;

        public float Z
        {
            get { return _Z; }
            set { _Z = value; }
        }

        public EntityPosition(float xpos,float ypos,float zpos)
        {
            X = xpos;
            Y = ypos;
            Z = zpos;
        }

    }
}
