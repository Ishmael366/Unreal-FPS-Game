// Copyright (c) 2026 Daniel Nguyen. MIT License.
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace PortfolioStudies
{
    [Serializable] public struct PeerState { public int Id; public float X, Z, Yaw; }
    // A small host-authoritative LAN room. Only input crosses from client to host;
    // clients cannot submit health, transforms, or other players' identities.
    public sealed class LanRoom : IStudyRoom
    {
        const int Magic = 0x44555331, MaximumPlayers = 8;
        const byte Version = 1;
        sealed class Peer { public IPEndPoint Address; public PeerState State; public float X, Z; public int Sequence = -1; public double LastSeen; }
        readonly Dictionary<string, Peer> peers = new Dictionary<string, Peer>();
        readonly Dictionary<int, PeerState> visible = new Dictionary<int, PeerState>();
        UdpClient socket;
        IPEndPoint server;
        int nextId = 1, sequence;
        float localX, localZ, sendClock, broadcastClock, helloClock;
        double now;
        bool host;
        public int LocalId { get; private set; } = -1;
        public bool Connected { get { return LocalId >= 0 && socket != null; } }
        public string Status { get; private set; } = "Not connected";
        public IReadOnlyDictionary<int, PeerState> Players { get { return visible; } }
        public int Port { get { return socket == null ? 0 : ((IPEndPoint)socket.Client.LocalEndPoint).Port; } }
        public void Host(int port)
        {
            Dispose(); host = true; LocalId = 0; socket = new UdpClient(new IPEndPoint(IPAddress.Any, port));
            socket.Client.Blocking = false;
            visible[0] = new PeerState { Id = 0 }; Status = "Hosting";
        }
        public void Join(string address, int port)
        {
            IPAddress parsed;
            if (!IPAddress.TryParse(address, out parsed) || parsed.AddressFamily != AddressFamily.InterNetwork) throw new ArgumentException("Enter an IPv4 address.");
            Dispose(); host = false; server = new IPEndPoint(parsed, port);
            socket = new UdpClient(new IPEndPoint(IPAddress.Any, 0)); socket.Client.Blocking = false; Status = "Connecting";
        }
        public void SetInput(float x, float z)
        {
            if (!Finite(x) || !Finite(z)) { localX = localZ = 0; return; }
            float n = (float)Math.Sqrt(x * x + z * z); if (n > 1) { x /= n; z /= n; }
            localX = x; localZ = z;
        }
        public void Update(float seconds)
        {
            if (socket == null) return;
            if (!Finite(seconds) || seconds < 0) throw new ArgumentOutOfRangeException(nameof(seconds));
            float dt = Math.Min(seconds, .1f); now += seconds;
            // Bound work per frame even if the socket receives a flood of datagrams.
            for (int i = 0; i < 64 && socket.Available > 0; ++i)
            {
                try { var endpoint = new IPEndPoint(IPAddress.Any, 0); byte[] data = socket.Receive(ref endpoint); Receive(data, endpoint); }
                catch (SocketException) { break; }
            }
            if (host)
            {
                var own = visible[0]; Advance(ref own, localX, localZ, dt); visible[0] = own;
                var expired = new List<string>();
                foreach (var pair in peers)
                {
                    var peer = pair.Value;
                    if (now - peer.LastSeen > 10) { expired.Add(pair.Key); continue; }
                    Advance(ref peer.State, peer.X, peer.Z, dt); visible[peer.State.Id] = peer.State;
                }
                foreach (string key in expired) { visible.Remove(peers[key].State.Id); peers.Remove(key); }
                broadcastClock += dt;
                if (broadcastClock >= .05f) { broadcastClock = 0; byte[] packet = Snapshot(); foreach (var peer in peers.Values) Send(packet, peer.Address); }
            }
            else
            {
                helloClock += dt; sendClock += dt;
                if (LocalId < 0 && helloClock >= .5f) { helloClock = 0; Send(Packet(0, w => { }), server); }
                if (LocalId >= 0 && sendClock >= .04f)
                {
                    sendClock = 0; Send(Packet(2, w => { w.Write(++sequence); w.Write(localX); w.Write(localZ); }), server);
                }
            }
        }
        void Receive(byte[] bytes, IPEndPoint endpoint)
        {
            if (bytes.Length < 6 || bytes.Length > 512) return;
            try
            {
                using (var stream = new MemoryStream(bytes)) using (var reader = new BinaryReader(stream))
                {
                    if (reader.ReadInt32() != Magic || reader.ReadByte() != Version) return;
                    byte type = reader.ReadByte(); string key = endpoint.ToString();
                    if (host && type == 0 && bytes.Length == 6)
                    {
                        Peer peer;
                        if (!peers.TryGetValue(key, out peer))
                        {
                            if (peers.Count >= MaximumPlayers - 1) return;
                            peer = new Peer { Address = endpoint, State = new PeerState { Id = nextId++, X = peers.Count * 2 + 2 } }; peers[key] = peer;
                        }
                        peer.LastSeen = now; visible[peer.State.Id] = peer.State;
                        Send(Packet(1, w => w.Write(peer.State.Id)), endpoint);
                    }
                    else if (host && type == 2 && bytes.Length == 18)
                    {
                        Peer peer; if (!peers.TryGetValue(key, out peer)) return;
                        int seq = reader.ReadInt32(); float x = reader.ReadSingle(), z = reader.ReadSingle();
                        if (seq <= peer.Sequence || !Finite(x) || !Finite(z)) return;
                        float n = (float)Math.Sqrt(x * x + z * z); if (n > 1) { x /= n; z /= n; }
                        peer.Sequence = seq; peer.X = x; peer.Z = z; peer.LastSeen = now;
                    }
                    else if (!host && endpoint.Equals(server) && type == 1 && bytes.Length == 10)
                    {
                        int id = reader.ReadInt32(); if (id < 1) return; LocalId = id; Status = "Connected";
                    }
                    else if (!host && endpoint.Equals(server) && type == 3)
                    {
                        int count = reader.ReadByte(); if (count > MaximumPlayers || bytes.Length != 7 + count * 16) return;
                        var next = new Dictionary<int, PeerState>();
                        for (int i = 0; i < count; i++)
                        {
                            var p = new PeerState { Id = reader.ReadInt32(), X = reader.ReadSingle(), Z = reader.ReadSingle(), Yaw = reader.ReadSingle() };
                            if (!Finite(p.X) || !Finite(p.Z) || !Finite(p.Yaw) || p.Id < 0 || next.ContainsKey(p.Id)) return;
                            next[p.Id] = p;
                        }
                        visible.Clear(); foreach (var pair in next) visible[pair.Key] = pair.Value;
                    }
                }
            }
            catch (EndOfStreamException) { }
            catch (IOException) { }
        }
        static void Advance(ref PeerState p, float x, float z, float dt)
        {
            p.X = Vitals.Clamp(p.X + x * dt * 5, -19, 19); p.Z = Vitals.Clamp(p.Z + z * dt * 5, -19, 19);
            if (x * x + z * z > .01f) p.Yaw = (float)(Math.Atan2(x, z) * 180 / Math.PI);
        }
        byte[] Snapshot() { return Packet(3, w => { w.Write((byte)visible.Count); foreach (PeerState p in visible.Values) { w.Write(p.Id); w.Write(p.X); w.Write(p.Z); w.Write(p.Yaw); } }); }
        static byte[] Packet(byte type, Action<BinaryWriter> payload)
        { using (var s = new MemoryStream()) { using (var w = new BinaryWriter(s)) { w.Write(Magic); w.Write(Version); w.Write(type); payload(w); w.Flush(); return s.ToArray(); } } }
        void Send(byte[] data, IPEndPoint destination) { try { socket.Send(data, data.Length, destination); } catch (SocketException) { Status = "Network unavailable"; } }
        static bool Finite(float x) { return !float.IsNaN(x) && !float.IsInfinity(x); }
        public void Dispose() { if (socket != null) socket.Close(); socket = null; peers.Clear(); visible.Clear(); LocalId = -1; sequence = 0; nextId = 1; localX = localZ = 0; now = 0; sendClock = broadcastClock = helloClock = 0; Status = "Not connected"; }
    }
}
