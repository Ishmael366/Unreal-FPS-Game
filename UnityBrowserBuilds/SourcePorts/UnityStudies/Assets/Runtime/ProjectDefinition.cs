using System;
using UnityEngine;
namespace PortfolioStudies {
 public static class ProjectDefinition {
  public static Definition Create() {
   string study="Forest-Survival";
   try {Uri uri;if(Uri.TryCreate(Application.absoluteURL,UriKind.Absolute,out uri)){foreach(string part in uri.Query.TrimStart('?').Split('&'))if(part.StartsWith("study="))study=Uri.UnescapeDataString(part.Substring(6));}}catch(UriFormatException){}
   switch(study){
case "Forest-Survival": return new Definition { Mode=StudyMode.Survival, Title="Forest Survival", Instructions="WASD move; mouse look; Space jump; Shift sprint; E interact. Collect three wood and establish camp after 60 seconds.", Objective="Collect supplies, watch food and water, and establish camp.", Seed=1337 };
case "Wind-Meadow": return new Definition { Mode=StudyMode.Meadow, Title="Wind Meadow", Instructions="WASD explore; mouse look; Space jump; G toggle grass; T change sun angle; adjust wind slider.", Objective="Explore the rolling meadow and compare wind and lighting.", Seed=1438 };
case "LAN-Arena": return new Definition { Mode=StudyMode.Multiplayer, Title="LAN Arena", Instructions="Host on one instance, join its IPv4 address on another; WASD move. Port 7777. Native desktop LAN prototype.", Objective="Meet other players in the arena.", Seed=1539 };
case "Shrine-Path": return new Definition { Mode=StudyMode.ClickRpg, Title="Shrine Path", Instructions="Left-click ground to move or an enemy to approach and attack. Defeat four guards, then use E at the shrine.", Objective="Clear the courtyard and reach the shrine.", Seed=1640 };
case "Sunlit-Side-Runner": return new Definition { Mode=StudyMode.SideRunner, Title="Sunlit Side Runner", Instructions="A/D or arrows move; Space jump; land on enemies; strike bonus blocks from below; E at the finish.", Objective="Collect ten tokens and reach the finish pole.", Seed=1741 };
case "Castle-Relics": return new Definition { Mode=StudyMode.Castle, Title="Castle Relics", Instructions="A/D move; Space jump; collect double jump and dash relics; Shift dash; left click attack; E interact.", Objective="Defeat the warden and escape the castle.", Seed=1842 };
case "Neon-Vault": return new Definition { Mode=StudyMode.Dungeon, Title="Neon Vault", Instructions="WASD move; mouse aim; left click fire; 1/2/3 weapon modes; Shift dash; R reload; E gates.", Objective="Enter the vault, defeat the warden, and leave.", Seed=1943 };
case "Silent-Extraction": return new Definition { Mode=StudyMode.Infiltration, Title="Silent Extraction", Instructions="WASD move; mouse look; Space jump; Shift sprint; left click fire; R reload; E use objectives.", Objective="Find the key, shut down the terminal and both generators, then extract.", Seed=2044 };
case "Sky-Island-Trail": return new Definition { Mode=StudyMode.SkyIslands, Title="Sky Island Trail", Instructions="WASD move; mouse camera; Space jump; collect six gems; E at the last beacon.", Objective="Collect six gems and activate the final beacon.", Seed=2145 };
case "Archive-Escape": return new Definition { Mode=StudyMode.Horror, Title="Archive Escape", Instructions="WASD move; mouse turn; left click fire; R reload; F flashlight; C fixed/shoulder camera; E interact.", Objective="Find the archive key, restore the mechanism, and escape.", Seed=2246 };
case "Facility-Breach": return new Definition { Mode=StudyMode.Shooter, Title="Facility Breach", Instructions="WASD move; mouse look; Shift sprint; Space jump; left click fire; R reload; E extract.", Objective="Clear all six guards and reach extraction.", Seed=2347 };
case "Shield-Arena": return new Definition { Mode=StudyMode.Duel, Title="Shield Arena", Instructions="WASD move; mouse camera; left click strike; hold right click to block. Watch health and stamina.", Objective="Defeat the opposing fighter.", Seed=2448 };
   default:return new Definition {Mode=StudyMode.Survival,Title="Forest Survival",Instructions="WASD move; mouse look; E interact; Space jump.",Objective="Find supplies and establish camp."};
   }
  }
 }
}
