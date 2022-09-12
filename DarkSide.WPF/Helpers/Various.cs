using System;
using System.Diagnostics;

namespace DarkSide.WPF.Helpers;

public class Various
{
    public Array getGameDLLProcesses()
    {
        //Getting all game.dll processes

        var localByName = Process.GetProcessesByName("game.dll");
        //private static Process p = Process.GetProcessesByName("game.dll").FirstOrDefault(); // get  DAoCMWC
        //IntPtr procId = Process.GetProcessesByName("game.dll").FirstOrDefault().MainWindowHandle; //get Mainwindow

        return localByName;
    }
}