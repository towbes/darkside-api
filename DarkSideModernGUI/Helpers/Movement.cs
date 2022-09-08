using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DarkSideModernGUI.Helpers.DarksideGameAPI;
using DarkSideModernGUI.Models;


namespace DarkSideModernGUI.Helpers
{
    public class Movement
    {

        static public float DistanceToPoint(PlayerPosition playerPos, float targX, float targY, float targZ)
        {
            float dist = (float)(Math.Sqrt((playerPos.pos_x - targX) * (playerPos.pos_x - targX) + (playerPos.pos_y - targY) * (playerPos.pos_y - targY)));
            return (float)dist;
        }

        static public short GetDegreesHeading(PlayerPosition playerPos, float targX, float targY)
        {
            float xDiff = playerPos.pos_x - targX;
            float yDiff = playerPos.pos_y - targY;
            //https://stackoverflow.com/questions/70511665/how-to-calculate-rotation-needed-to-face-an-object
            return (short)((Math.Atan2(yDiff, xDiff) * 180.0 / Math.PI + 630.0) % 360.0);
        }

        //DOL heading func
        //https://github.com/Dawn-of-Light/DOLSharp/blob/9af87af011497c3fda852559b01a269c889b162e/GameServer/world/Point2D.cs
        static public short GetGameHeading(PlayerPosition playerPos, float posx, float posy)
        {
            float dx = posx - playerPos.pos_x;
            float dy = posy - playerPos.pos_y;

            double heading = Math.Atan2(-dx, dy) * (180.0 / Math.PI) * (4096.0 / 360.0);

            if (heading < 0)
                heading += 4096;

            return (short)heading;
        }

        static public void checkAndMoveToNearestWaypoint (Waypoint waypoint){




        }


    }
}
