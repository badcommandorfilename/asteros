using System.ComponentModel;
using System.Threading;
using System.Diagnostics;
using System.Runtime.CompilerServices;
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
    public class Shell : GameObject, IObj2d, IMomentum, IHas<Lifetime>
    {
        public Shell(string key) : base(key)
        {
            this.As<Mass>().Kg = 1;
            this.As<Lifetime>().Birth = DateTime.UtcNow;
            this.As<Lifetime>().Death = DateTime.UtcNow + TimeSpan.FromSeconds(10);
        }

        public IDictionary<string, object> P {get;} = new Dictionary<string, object>();

        public double Wo {get; init;}

        public Pos2d Pos => this.As<Pos2d>();
        public Rot2d Rot => this.As<Rot2d>();
        public double Speed {get;set;} = 50;

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
    }
}