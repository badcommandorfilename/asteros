using System.Reflection.Metadata;
using System.Reflection;
using System.Net.Mail;
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
    public class Space : GameObject, ISystem, IHas<Square2d>
    {
        public Space(string key) : base(key)
        {
            this.As<Square2d>().W = 300;
            this.As<Square2d>().H = 300;
        }

        public IDictionary<string, object> P {get;} = new Dictionary<string, object>();

        public void Step(WorldState world)
        {
            var arena = this.As<Square2d>();
            foreach(var o in world.Objects.Values)
            {
                if(o == this) { continue; }
                if(o is IHas<Pos2d> p)
                {
                    if(p.Value.X < 0)
                    {
                        p.Value.X = arena.W;
                    }
                    else if(p.Value.X > arena.W)
                    {
                        p.Value.X = 0;
                    }

                    if(p.Value.Y < 0)
                    {
                        p.Value.Y = arena.H;
                    }
                    else if(p.Value.Y > arena.H)
                    {
                        p.Value.Y = 0;
                    }
                }
            }
        }
    }
}