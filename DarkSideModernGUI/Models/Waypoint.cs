using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkSideModernGUI.Models
{
    public struct Waypoint
    {
        public string waypointID { get; set; }
        public string playerPosX { get; set; }
        public string playerPosY { get; set; }
        public string playerPosZ { get; set; }
        public string playerHeading { get; set; }
    }
}
