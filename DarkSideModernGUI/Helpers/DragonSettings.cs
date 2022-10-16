using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkSideModernGUI.Helpers
{
    public class DragonSettings
    {
        public Names roleNames;
        public BattleLocs battleLocs;
        public ResetLocs resetLocs;
        public ItemFilter itemFilter;
        public DragonFight dragonFight;
    }

    public class Names
    {
        public string leaderName { get; set; }
        public string readyName { get; set; }
        public string tankName { get; set; }

    }

    public class BattleLocs
    {
        public SettingsLoc prepBattleLoc;
        public SettingsLoc tankLocs;
        public SettingsLoc otherLocs1;
        public SettingsLoc otherLocs2;
        public SettingsLoc otherLocs3;
        public List<string> loc1Chars;
        public List<string> loc2Chars;
        public List<string> loc3Chars;

    }


    public class ResetLocs
    {
        public SettingsLoc tankLocs;
        public SettingsLoc otherLocs;
        public SettingsLoc prepResetLoc;
        public SettingsLoc rampLoc;
        public SettingsLoc maxLoc;
        public bool readyCharReset;
    }

    public class SettingsLoc
    {
        public float xloc;
        public float yloc;
        public short heading;
    }

    public class ItemFilter
    {
        public List<string> badItems;
    }

    public class DragonFight
    {
        public Tank tank;
        public DPS dps;
        public Debuff debuff;
        public Bard bard;
        public Healer healer;
    }

    public class Tank
    {
        public string className { get; set; }
        public string buff1 { get; set; }
        public string buff2 { get; set; }
        public string buff3 { get; set; }
        public string buff4 { get; set; }
        public string buff5 { get; set; }
        public string buff6 { get; set; }
        public string dps1 { get; set; }
        public string dps2 { get; set; }
        public string dps3 { get; set; }
        public string dps4 { get; set; }
        public string dps5 { get; set; }
        public string dps6 { get; set; }
        public string heal1 { get; set; }
        public int heal1pct { get; set; }
        public string heal2 { get; set; }
        public int heal2pct { get; set; }
        public string heal3 { get; set; }
        public int heal3pct { get; set; }
        public string heal4 { get; set; }
        public int heal4pct { get; set; }
        public string heal5 { get; set; }
        public int heal5pct { get; set; }
        public string heal6 { get; set; }
        public int heal6pct { get; set; }
        public string pet1 { get; set; }
        public string pet2 { get; set; }
        public string pet3 { get; set; }
        public string pet4 { get; set; }
        public string pet5 { get; set; }
        public string pet6 { get; set; }

    }

    public class DPS
    {
        public string className { get; set; }
        public string buff1 { get; set; }
        public string buff2 { get; set; }
        public string buff3 { get; set; }
        public string buff4 { get; set; }
        public string buff5 { get; set; }
        public string buff6 { get; set; }
        public string dps1 { get; set; }
        public string dps2 { get; set; }
        public string dps3 { get; set; }
        public string dps4 { get; set; }
        public string dps5 { get; set; }
        public string dps6 { get; set; }
        public string heal1 { get; set; }
        public int heal1pct { get; set; }
        public string heal2 { get; set; }
        public int heal2pct { get; set; }
        public string heal3 { get; set; }
        public int heal3pct { get; set; }
        public string heal4 { get; set; }
        public int heal4pct { get; set; }
        public string heal5 { get; set; }
        public int heal5pct { get; set; }
        public string heal6 { get; set; }
        public int heal6pct { get; set; }
        public string pet1 { get; set; }
        public string pet2 { get; set; }
        public string pet3 { get; set; }
        public string pet4 { get; set; }
        public string pet5 { get; set; }
        public string pet6 { get; set; }

    }

    public class Debuff
    {
        public string className { get; set; }
        public string buff1 { get; set; }
        public string buff2 { get; set; }
        public string buff3 { get; set; }
        public string buff4 { get; set; }
        public string buff5 { get; set; }
        public string buff6 { get; set; }
        public string dps1 { get; set; }
        public string dps2 { get; set; }
        public string dps3 { get; set; }
        public string dps4 { get; set; }
        public string dps5 { get; set; }
        public string dps6 { get; set; }
        public string heal1 { get; set; }
        public int heal1pct { get; set; }
        public string heal2 { get; set; }
        public int heal2pct { get; set; }
        public string heal3 { get; set; }
        public int heal3pct { get; set; }
        public string heal4 { get; set; }
        public int heal4pct { get; set; }
        public string heal5 { get; set; }
        public int heal5pct { get; set; }
        public string heal6 { get; set; }
        public int heal6pct { get; set; }
        public string pet1 { get; set; }
        public string pet2 { get; set; }
        public string pet3 { get; set; }
        public string pet4 { get; set; }
        public string pet5 { get; set; }
        public string pet6 { get; set; }
    }

    public class Bard
    {
        public string className { get; set; }
        public string buff1 { get; set; }
        public string buff2 { get; set; }
        public string buff3 { get; set; }
        public string buff4 { get; set; }
        public string buff5 { get; set; }
        public string buff6 { get; set; }
        public string dps1 { get; set; }
        public string dps2 { get; set; }
        public string dps3 { get; set; }
        public string dps4 { get; set; }
        public string dps5 { get; set; }
        public string dps6 { get; set; }
        public string heal1 { get; set; }
        public int heal1pct { get; set; }
        public string heal2 { get; set; }
        public int heal2pct { get; set; }
        public string heal3 { get; set; }
        public int heal3pct { get; set; }
        public string heal4 { get; set; }
        public int heal4pct { get; set; }
        public string heal5 { get; set; }
        public int heal5pct { get; set; }
        public string heal6 { get; set; }
        public int heal6pct { get; set; }
        public string pet1 { get; set; }
        public string pet2 { get; set; }
        public string pet3 { get; set; }
        public string pet4 { get; set; }
        public string pet5 { get; set; }
        public string pet6 { get; set; }
        public string grpHeal1 { get; set; }
        public int grpHeal1pct { get; set; }
    }

    public class Healer
    {
        public string className { get; set; }
        public string buff1 { get; set; }
        public string buff2 { get; set; }
        public string buff3 { get; set; }
        public string buff4 { get; set; }
        public string buff5 { get; set; }
        public string buff6 { get; set; }
        public string dps1 { get; set; }
        public string dps2 { get; set; }
        public string dps3 { get; set; }
        public string dps4 { get; set; }
        public string dps5 { get; set; }
        public string dps6 { get; set; }
        public string heal1 { get; set; }
        public int heal1pct { get; set; }
        public string heal2 { get; set; }
        public int heal2pct { get; set; }
        public string heal3 { get; set; }
        public int heal3pct { get; set; }
        public string heal4 { get; set; }
        public int heal4pct { get; set; }
        public string heal5 { get; set; }
        public int heal5pct { get; set; }
        public string heal6 { get; set; }
        public int heal6pct { get; set; }
        public string pet1 { get; set; }
        public string pet2 { get; set; }
        public string pet3 { get; set; }
        public string pet4 { get; set; }
        public string pet5 { get; set; }
        public string pet6 { get; set; }
        public string grpHeal1 { get; set; }
        public int grpHeal1pct { get; set; }
    }
    
}
