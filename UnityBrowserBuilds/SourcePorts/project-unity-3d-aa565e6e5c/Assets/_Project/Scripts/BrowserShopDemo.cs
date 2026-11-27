using System.Collections.Generic;
public static class BrowserShopDemo {
 public static readonly bool Enabled=true;
 public static readonly HashSet<string> Purchased=new HashSet<string>();
 public static ItemData[] Items={new ItemData{objectId="demo-sword",name="Astral Sword",description="Practice blade. Local demo item; no NFT or payment.",imageUrl=""},new ItemData{objectId="demo-shield",name="Field Shield",description="Local demo equipment. No wallet required.",imageUrl=""},new ItemData{objectId="demo-boots",name="Traveler Boots",description="Preview item stored for this visit only.",imageUrl=""}};
}