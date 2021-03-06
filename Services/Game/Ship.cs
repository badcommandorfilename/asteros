using System.Xml.Schema;
using System.Runtime.Intrinsics;
using System.ComponentModel;
using System.Reflection.Emit;
using System.Dynamic;
using System.Security.Cryptography.X509Certificates;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.IO;

namespace Petscribe.Services.Game
{
    public class Ship : GameObject, IObj2d, IMomentum
    {
        public Ship(string key) : base(key)
        {
            this.As<Mass>().Kg = 10;
        }

        public IDictionary<string, object> P { get; } = new Dictionary<string, object>();

        public double Wo { get; init; }

        public Pos2d Pos => this.As<Pos2d>();
        public Rot2d Rot => this.As<Rot2d>();
        public double Speed { get; set; } = 50;
        public double Turn { get; set; } = 0.5 / M.tpi;

        public void Left() => Rot.W -= Turn;
        public void Right() => Rot.W += Turn;
        public void Forward()
        {
            var dx = (double)Math.Cos(Rot.W);
            var dy = (double)Math.Sin(Rot.W);
            if (this is IHas<Velocity2d> v2)
            {
                v2.Value.DDX += dx * Speed;
                v2.Value.DDY += dy * Speed;
            }
        }

        public List<Shell> Shells = new List<Shell>();
        public void Shoot(WorldState world)
        {
            var shotid = Shells.Count;
            var s = new Shell($"{this.Key}_shell-{shotid++}");
            s.Pos.X = this.Pos.X;
            s.Pos.Y = this.Pos.Y;
            if (this is IHas<Velocity2d> v2)
            {
                if (s is IHas<Velocity2d> v1)
                {
                    var dx = (double)Math.Cos(Rot.W);
                    var dy = (double)Math.Sin(Rot.W);
                    v1.Value.DDX += dx * 30;
                    v1.Value.DDY += dy * 30;
                    v1.Value.DDX += v2.Value.DDX/10; //Momentum formula is wrong
                    v1.Value.DDY += v2.Value.DDY/10;
                }
            }
            Shells.Add(s);
            world.Add(s);
        }

        public string ShipStyle =>
        $"left: {Pos.X}px; top: {Pos.Y}px; transform: rotate({Rot.W + Wo}rad);";

    }

    public class TriShip : Ship
    {

        public TriShip(string key) : base(key)
        {
            Wo = M.hpi - 0.72;
        }

    }
}