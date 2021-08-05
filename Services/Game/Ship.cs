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

        public IDictionary<string, object> P {get;} = new Dictionary<string, object>();

        public double Wo {get; init;}

        public Pos2d Pos => this.As<Pos2d>();
        public Rot2d Rot => this.As<Rot2d>();
        public double Speed {get;set;} = 50;
        public double Turn {get;set;} = 0.5/M.tpi;

        public void Left() => Rot.W-=Turn;
        public void Right() => Rot.W+=Turn;
        public void Forward()
        {
            var dx = (double)Math.Cos(Rot.W);
            var dy = (double)Math.Sin(Rot.W);
            if(this is IHas<Velocity2d> v2)
            {
                v2.Value.DDX += dx*Speed;
                v2.Value.DDY += dy*Speed;
            }
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