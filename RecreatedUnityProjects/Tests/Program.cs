using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using PortfolioStudies;

static class Program
{
    static int checks;
    static void Check(bool value,string message){checks++;if(!value)throw new Exception(message);}
    static void Main()
    {
        var a=new Vitals();var b=new Vitals();
        for(int i=0;i<50;i++)a.Tick(1,false);
        for(int i=0;i<500;i++)b.Tick(.1f,false);
        Check(Math.Abs(a.Food-b.Food)<.01f&&Math.Abs(a.Water-b.Water)<.01f,"Vitals changed with frame frequency.");
        a.Tick(1000,true);Check(a.Health==0&&a.Food==0&&a.Water==0&&a.Stamina==0,"Vitals underflow or survival death missing.");
        bool invalid=false;try{a.Tick(float.NaN,false);}catch(ArgumentOutOfRangeException){invalid=true;}Check(invalid,"Nonfinite time accepted.");
        var gun=new Magazine(18,7);for(int i=0;i<6;i++)Check(gun.Fire(),"Premature empty magazine");
        Check(gun.Reload()==6&&gun.Loaded==18&&gun.Reserve==1,"Reload conservation failed.");
        for(int i=0;i<18;i++)Check(gun.Fire(),"Magazine size mismatch.");
        Check(!gun.Fire()&&gun.Reload()==1&&gun.Loaded==1&&gun.Reserve==0,"Partial reserve reload failed.");
        Check(gun.Fire()&&!gun.Fire()&&gun.Reload()==0,"Empty gun generated ammunition.");
        var flags=new Objectives();Check(flags.Has("")&&!flags.Has("key"),"Prerequisite behavior invalid.");Check(flags.Set("key")&&!flags.Set("key")&&flags.Has("key"),"Duplicate objective completion.");
        using(var host=new LanRoom())using(var one=new LanRoom())using(var two=new LanRoom())
        {
            host.Host(0);one.Join("127.0.0.1",host.Port);two.Join("127.0.0.1",host.Port);one.SetInput(1,1);two.SetInput(-1,0);
            for(int i=0;i<300;i++){host.Update(.02f);one.Update(.02f);two.Update(.02f);Thread.Sleep(1);}
            Check(one.Connected&&two.Connected&&one.LocalId!=two.LocalId,"UDP join/identity failed.");
            Check(host.Players.Count==3&&one.Players.Count==3&&two.Players.Count==3,"Room snapshots missing players.");
            var p=host.Players[one.LocalId];Check(p.X>1&&p.Z>1&&p.X<=19&&p.Z<=19,"Host movement or world limits failed.");
            Check(Math.Abs(p.X-one.Players[one.LocalId].X)<1,"Authoritative snapshot diverged.");
            one.SetInput(float.NaN,float.PositiveInfinity);float previous=host.Players[one.LocalId].X;
            for(int i=0;i<20;i++){one.Update(.02f);host.Update(.02f);two.Update(.02f);Thread.Sleep(1);}
            Check(Math.Abs(host.Players[one.LocalId].X-previous)<.2f,"Nonfinite input propagated.");
            using(var bad=new UdpClient()){byte[] packet={1,2,3};bad.Send(packet,packet.Length,new IPEndPoint(IPAddress.Loopback,host.Port));host.Update(.02f);Check(host.Players.Count==3,"Malformed packet changed membership.");}
            one.Dispose();two.Dispose();host.Update(11);Check(host.Players.Count==1,"Disconnected peers not expired.");
        }
        Console.WriteLine("PASS: "+checks+" assertions; real loopback host + two clients, snapshots, invalid input, disconnects, ammo, survival and objectives.");
    }
}
