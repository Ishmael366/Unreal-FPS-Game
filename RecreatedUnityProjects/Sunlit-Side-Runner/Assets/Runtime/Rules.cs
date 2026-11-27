// Copyright (c) 2026 Daniel Nguyen. MIT License.
using System;
using System.Collections.Generic;

namespace PortfolioStudies
{
    public enum StudyMode { Survival, Meadow, Multiplayer, ClickRpg, SideRunner, Castle, Dungeon, Infiltration, SkyIslands, Horror, Shooter, Duel }
    public enum RunState { Ready, Playing, Paused, Won, Lost }
    public sealed class Definition
    {
        public StudyMode Mode;
        public string Title, Instructions, Objective;
        public int Seed = 1337;
    }
    // Pure C# game rules are deliberately independent of rendering and frame rate.
    public sealed class Vitals
    {
        public float Health = 100, Food = 100, Water = 100, Stamina = 100;
        public void Tick(float seconds, bool sprinting)
        {
            if (seconds < 0 || float.IsNaN(seconds) || float.IsInfinity(seconds)) throw new ArgumentOutOfRangeException(nameof(seconds));
            Food = Clamp(Food - seconds * .32f, 0, 100);
            Water = Clamp(Water - seconds * .55f, 0, 100);
            Stamina = Clamp(Stamina + seconds * (sprinting ? -16 : 12), 0, 100);
            if (Food <= 0 || Water <= 0) Health = Clamp(Health - seconds * 7, 0, 100);
        }
        public static float Clamp(float x, float min, float max) { return Math.Max(min, Math.Min(max, x)); }
    }
    public sealed class Magazine
    {
        public int Capacity { get; private set; }
        public int Loaded { get; private set; }
        public int Reserve { get; private set; }
        public Magazine(int capacity, int reserve) { if (capacity < 1 || reserve < 0) throw new ArgumentOutOfRangeException(); Capacity = Loaded = capacity; Reserve = reserve; }
        public bool Fire() { if (Loaded == 0) return false; Loaded--; return true; }
        public int Reload() { int n = Math.Min(Capacity - Loaded, Reserve); Loaded += n; Reserve -= n; return n; }
        public void Supply(int count) { if (count > 0) Reserve += count; }
    }
    public sealed class Objectives
    {
        readonly HashSet<string> flags = new HashSet<string>();
        public bool Has(string flag) { return string.IsNullOrEmpty(flag) || flags.Contains(flag); }
        public bool Set(string flag) { return !string.IsNullOrEmpty(flag) && flags.Add(flag); }
        public string[] Completed { get { var a = new string[flags.Count]; flags.CopyTo(a); return a; } }
    }
}
