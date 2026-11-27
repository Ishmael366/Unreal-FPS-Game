// Copyright (c) 2026 Daniel Nguyen. MIT License.
using UnityEngine;

namespace PortfolioStudies
{
    public static class Layouts
    {
        static readonly Color Stone=new Color(.29f,.32f,.36f),Gold=new Color(1,.7f,.13f),Teal=new Color(.1f,.75f,.76f);
        public static Vector3 Build(GameSession s)
        {
            switch(s.Def.Mode)
            {
                case StudyMode.Survival:return Survival(s);
                case StudyMode.Meadow:World.Landscape(true);return new Vector3(0,World.Height(0,0)+.2f,0);
                case StudyMode.Multiplayer:return Multiplayer();
                case StudyMode.ClickRpg:return Rpg();
                case StudyMode.SideRunner:return SideRunner();
                case StudyMode.Castle:return Castle();
                case StudyMode.Dungeon:return Dungeon();
                case StudyMode.Infiltration:return Infiltration();
                case StudyMode.SkyIslands:return Islands();
                case StudyMode.Horror:return Horror();
                case StudyMode.Shooter:return Shooter();
                case StudyMode.Duel:return Duel();
            }
            return Vector3.zero;
        }
        static Collectible Pickup(string kind,Vector3 position,Color color,int amount=1)
        {
            var g=World.Shape(kind,PrimitiveType.Sphere,position,Vector3.one*.55f,color,false);var c=g.AddComponent<Collectible>();c.Kind=kind;c.Amount=amount;return c;
        }
        static Interactable Use(string label,Vector3 p,string kind,string flag="",string requires="",bool remove=false,Vector3? size=null)
        {
            var g=World.Box(label,p,size??new Vector3(1,1.4f,1),kind=="goal"||kind=="extract"?Teal:Gold);var c=g.AddComponent<Interactable>();c.Label=label;c.Kind=kind;c.Flag=flag;c.Requires=requires;c.Remove=remove;return c;
        }
        static ActorHealth Enemy(Vector3 p,bool ranged=false,bool boss=false,bool side=false,bool drone=false,bool dormant=false)
        {
            var g=World.Body(boss?"Warden":drone?"Patrol drone":"Guard",p,boss?new Color(.65f,.1f,.65f):new Color(.7f,.2f,.13f),boss?1.2f:1);
            var h=g.AddComponent<ActorHealth>();h.Boss=boss;h.Maximum=h.Current=boss?240:70;
            var brain=g.AddComponent<EnemyBrain>();brain.Boss=boss;brain.Ranged=ranged;brain.Side=side;brain.Drone=drone;brain.Dormant=dormant;brain.Speed=side?1.6f:2.4f;
            if(drone){foreach(var r in g.GetComponentsInChildren<Renderer>())r.sharedMaterial=World.Mat(new Color(.7f,.45f,.1f));}
            return h;
        }
        static Vector3 Survival(GameSession s)
        {
            World.Landscape(false);
            foreach(var v in new[]{new Vector2(4,4),new Vector2(-5,8),new Vector2(8,12)})Pickup("wood",new Vector3(v.x,World.Height(v.x,v.y)+.8f,v.y),new Color(.6f,.3f,.12f));
            foreach(var v in new[]{new Vector2(-3,4),new Vector2(5,-3),new Vector2(12,4)})
            {Pickup("water",new Vector3(v.x,World.Height(v.x,v.y)+.8f,v.y),Color.cyan);Pickup("food",new Vector3(v.x+1,World.Height(v.x+1,v.y)+.8f,v.y),new Color(.8f,.4f,.15f));}
            Use("Establish camp",new Vector3(0,World.Height(0,6)+.5f,6),"campfire");
            for(int i=0;i<6;i++)World.Box("Camp stones",new Vector3(Mathf.Sin(i)*1.2f,World.Height(0,6)+.2f,6+Mathf.Cos(i)*1.2f),Vector3.one*.4f,Stone,false);
            return new Vector3(0,World.Height(0,0)+.15f,0);
        }
        static Vector3 Multiplayer()
        {
            World.Floor(Vector3.zero,new Vector3(42,1,42),new Color(.13f,.18f,.25f));
            for(int i=-20;i<=20;i+=4){World.Box("Lane marking",new Vector3(i,.01f,0),new Vector3(.06f,.02f,40),Teal,false);World.Box("Lane marking",new Vector3(0,.01f,i),new Vector3(40,.02f,.06f),Teal,false);}
            foreach(int sign in new[]{-1,1}){World.Box("Arena wall",new Vector3(sign*21,1.5f,0),new Vector3(1,3,43),Stone);World.Box("Arena wall",new Vector3(0,1.5f,sign*21),new Vector3(43,3,1),Stone);}
            return Vector3.zero;
        }
        static Vector3 Rpg()
        {
            World.Room(new Vector3(0,0,10),40,45,Stone,false);
            for(int z=0;z<5;z++)for(int x=0;x<4;x++)if((x+z)%3==0)World.Pillar(new Vector3(x*8-12,0,z*7-6),3.5f,Stone*.8f);
            foreach(var p in new[]{new Vector3(-8,0,7),new Vector3(7,0,13),new Vector3(-5,0,19),new Vector3(10,0,25)})Enemy(p);
            Pickup("health",new Vector3(-12,1,14),Color.green);Pickup("gem",new Vector3(0,1,22),Teal);
            Use("Reach the shrine",new Vector3(0,1,29),"goal","","clear");return new Vector3(0,0,-8);
        }
        static Vector3 SideRunner()
        {
            Color earth=new Color(.65f,.3f,.13f);for(int i=0;i<6;i++){float x=i*10;World.Floor(new Vector3(x,0,0),new Vector3(8.5f,1,5),earth);World.Box("Terrace",new Vector3(x,-2.5f,0),new Vector3(8.5f,4,5),earth*.6f);}
            for(int i=0;i<18;i++)Pickup("coin",new Vector3(i*3+1,1.2f,0),Gold);
            for(int i=0;i<4;i++){var b=World.Box("Bonus block",new Vector3(i*11+6,2.8f,0),Vector3.one,Gold);b.AddComponent<PrizeBlock>();}
            Enemy(new Vector3(12,0,0),false,false,true);Enemy(new Vector3(32,0,0),false,false,true);
            World.Box("Finish pole",new Vector3(52,2,0),new Vector3(.15f,4,.15f),Color.white);var goal=Use("Finish — 10 tokens",new Vector3(52,1,0),"goal");goal.RequiredCoins=10;return new Vector3(-2,0,0);
        }
        static Vector3 Castle()
        {
            World.Floor(new Vector3(5,0,0),new Vector3(22,1,5),Stone);World.Floor(new Vector3(25,0,0),new Vector3(16,1,5),Stone);World.Floor(new Vector3(44,0,0),new Vector3(20,1,5),Stone);
            for(int x=-4;x<55;x+=5){World.Pillar(new Vector3(x,0,3),5,Stone*.6f);World.Box("Castle backdrop",new Vector3(x,2.5f,4),new Vector3(5,5,.5f),Stone*.35f);}
            Pickup("double-jump",new Vector3(10,1,0),Teal);Pickup("dash",new Vector3(24,1,0),new Color(.7f,.2f,1));
            World.Box("Upper ledge",new Vector3(14,2.3f,0),new Vector3(3,.4f,3),Stone);
            Use("Open the sealed gate",new Vector3(30,1.5f,0),"flag","gate","dash",true,new Vector3(.6f,3,4));
            Enemy(new Vector3(6,0,0),false,false,true);Enemy(new Vector3(42,0,0),true,true,true);
            Pickup("health",new Vector3(36,1,0),Color.green);Use("Leave the castle",new Vector3(51,1,0),"goal","","boss");return new Vector3(-2,0,0);
        }
        static Vector3 Dungeon()
        {
            World.Room(new Vector3(0,0,15),30,40,new Color(.18f,.2f,.29f),false);
            for(int z=0;z<6;z++)foreach(int sign in new[]{-1,1}){World.Pillar(new Vector3(sign*12,0,z*6),4,Stone);World.Shape("Rune lamp",PrimitiveType.Sphere,new Vector3(sign*11.9f,3,z*6),Vector3.one*.35f,Teal,false);}
            Use("Enter the dungeon",new Vector3(0,1.5f,5),"flag","entered","",true,new Vector3(6,3,.6f));
            foreach(var p in new[]{new Vector3(-6,0,12),new Vector3(6,0,12),new Vector3(-6,0,17),new Vector3(6,0,17)})Enemy(p,false,false,false,false,true);
            Enemy(new Vector3(8,0,23),true,false,false,false,true);Enemy(new Vector3(0,0,27),true,true,false,false,true);
            Pickup("ammo",new Vector3(-7,1,22),Gold,40);Pickup("health",new Vector3(-8,1,9),Color.green);Use("Leave the vault",new Vector3(0,1,32),"goal","","boss");return Vector3.zero;
        }
        static Vector3 Infiltration()
        {
            World.Floor(new Vector3(0,0,18),new Vector3(70,1,78),new Color(.24f,.29f,.24f));
            World.Box("Facility",new Vector3(0,3,25),new Vector3(25,6,1),Stone);
            foreach(int sign in new[]{-1,1})World.Box("Fence wing",new Vector3(sign*12,1.6f,5),new Vector3(20,3.2f,.5f),Stone);
            Use("Unlock the gate",new Vector3(0,1.6f,5),"flag","gate","key",true,new Vector3(4,3.2f,.5f));Pickup("key",new Vector3(-8,1,-3),Gold);
            Use("Shut down terminal",new Vector3(0,.8f,18),"flag","terminal","key");Use("Disable west generator",new Vector3(-15,1,30),"flag","gen-a","terminal");Use("Disable east generator",new Vector3(15,1,30),"flag","gen-b","terminal");
            var vehicle=Use("Extract with vehicle",new Vector3(0,1,43),"extract","","",false,new Vector3(3,2,5));
            for(int x=-15;x<=15;x+=15){World.Box("Storage cover",new Vector3(x,1,11),new Vector3(3,2,3),new Color(.4f,.35f,.21f));}
            Enemy(new Vector3(-12,0,0),true);Enemy(new Vector3(7,0,15),true);Enemy(new Vector3(-9,0,30),true);Enemy(new Vector3(10,4,3),true,false,false,true);
            Pickup("ammo",new Vector3(5,1,-4),Gold,36);Pickup("health",new Vector3(9,1,29),Color.green);return new Vector3(0,0,-10);
        }
        static Vector3 Islands()
        {
            World.Floor(Vector3.zero,new Vector3(6,1,6),Teal*.5f);
            for(int i=0;i<12;i++)
            {
                Vector3 p=new Vector3(Mathf.Sin(i*.6f)*3,i*.38f,6+i*5);var island=World.Box("Floating island",p-Vector3.up*.5f,new Vector3(4,1,4),new Color(.17f,.43f,.39f));
                World.Shape("Island crystal",PrimitiveType.Cylinder,p-Vector3.up*2,new Vector3(1.3f,1.5f,1.3f),new Color(.18f,.22f,.35f),false);
                if(i==5){var moving=island.AddComponent<MovingPlatform>();moving.Travel=Vector3.right*1.2f;}
                if(i%2==0)Pickup("gem",p+Vector3.up,Teal);
                if(i==11){var goal=Use("Activate the beacon — 6 gems",p+Vector3.up,"goal");goal.RequiredGems=6;}
            }
            return new Vector3(0,0,-1);
        }
        static Vector3 Horror()
        {
            World.Room(new Vector3(0,0,10),26,48,new Color(.22f,.17f,.19f),false);
            foreach(int sign in new[]{-1,1})World.Box("Partition",new Vector3(sign*7.5f,2.5f,8),new Vector3(11,5,.8f),Stone*.6f);
            Use("Unlock the archive",new Vector3(0,1.5f,8),"flag","archive","key",true,new Vector3(4,3,.8f));
            Pickup("key",new Vector3(-8,1,1),Gold);Use("Restore the escape mechanism",new Vector3(8,1,23),"flag","terminal","archive");Use("Escape",new Vector3(0,1,31),"goal","","terminal");
            for(int i=0;i<4;i++)World.Box("Archive shelf",new Vector3(-8+i*5,1.5f,17),new Vector3(2,3,1),new Color(.2f,.11f,.08f));
            var zombie=Enemy(new Vector3(7,0,1));zombie.GetComponent<EnemyBrain>().Speed=1.2f;
            zombie=Enemy(new Vector3(-5,0,22));zombie.GetComponent<EnemyBrain>().Speed=1.1f;
            Pickup("ammo",new Vector3(-8,1,-4),Gold,12);Pickup("health",new Vector3(8,1,28),Color.green);return new Vector3(-8,0,-8);
        }
        static Vector3 Shooter()
        {
            World.Room(new Vector3(0,0,12),40,56,Stone,false);
            for(int z=0;z<4;z++)foreach(int sign in new[]{-1,1})World.Box("Cover",new Vector3(sign*(z%2==0?7:12),1,z*9-3),new Vector3(3,2,2),new Color(.3f,.4f,.45f));
            foreach(var p in new[]{new Vector3(-8,0,0),new Vector3(8,0,7),new Vector3(-12,0,15),new Vector3(8,0,22),new Vector3(-8,0,29),new Vector3(4,0,33)})Enemy(p,true);
            Pickup("ammo",new Vector3(7,1,-7),Gold,36);Pickup("health",new Vector3(-13,1,13),Color.green);Use("Extraction — clear all six guards",new Vector3(0,1,36),"goal","","clear");return new Vector3(0,0,-10);
        }
        static Vector3 Duel()
        {
            World.Shape("Arena",PrimitiveType.Cylinder,new Vector3(0,-.5f,0),new Vector3(24,.5f,24),new Color(.47f,.35f,.2f));
            for(int i=0;i<16;i++){float a=i*Mathf.PI/8;World.Pillar(new Vector3(Mathf.Sin(a)*12,0,Mathf.Cos(a)*12),5,Stone);}
            var enemy=Enemy(new Vector3(0,0,5));enemy.Maximum=enemy.Current=120;enemy.GetComponent<EnemyBrain>().Speed=2.7f;enemy.GetComponent<EnemyBrain>().AttackDamage=12;
            World.Part(enemy.gameObject,"Sword",PrimitiveType.Cube,new Vector3(.5f,1,.5f),new Vector3(.12f,.12f,1.6f),new Color(.7f,.75f,.8f));return new Vector3(0,0,-3);
        }
    }
}
