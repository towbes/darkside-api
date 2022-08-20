using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace DarkSideModernGUI.Helpers
{
    public class Various
    {

        public Array getGameDLLProcesses() {
            //Getting all game.dll processes

            Process[] localByName = Process.GetProcessesByName("game.dll");
            //private static Process p = Process.GetProcessesByName("game.dll").FirstOrDefault(); // get  DAoCMWC
            //IntPtr procId = Process.GetProcessesByName("game.dll").FirstOrDefault().MainWindowHandle; //get Mainwindow

            return localByName;

        }




    }

    



}
