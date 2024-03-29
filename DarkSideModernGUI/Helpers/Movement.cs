﻿using System;
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
        public struct Pos_2d
        {
            public float x;
            public float y;
        }
        public struct Pos_3d
        {
            public float x;
            public float y;
            public float z;
        }

        static public float DistanceToPoint(PlayerPosition playerPos, float targX, float targY)
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

        //convert a PlayerPosition heading to heading for turning
        static public short ConvertCharHeading(short degreesHeading)
        {
            degreesHeading -= 90;
            if (degreesHeading < 0)
            {
                degreesHeading += 360;
            }
            double newheading = degreesHeading * (4096 / 360);

            return (short)newheading;
        }
        //Convert in game direction to heading for turning
        static public short ConvertDirHeading(short dirHeading)
        {
            dirHeading += 180;
            if (dirHeading > 360)
            {
                dirHeading -= 360;
            }

            double newheading = dirHeading * (4096 / 360);

            return (short)newheading;
        }

        static public void checkAndMoveToNearestWaypoint (Waypoint waypoint){

        }


    }
}
