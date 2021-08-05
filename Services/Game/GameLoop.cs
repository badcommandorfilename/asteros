using System.Runtime.InteropServices.ComTypes;
using System.Collections.Specialized;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Reflection.Emit;
using Microsoft.VisualBasic.CompilerServices;
using System.Reflection;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Reflection.PortableExecutable;
using System.IO.Enumeration;
using System;
using System.Security.AccessControl;
using System.Diagnostics;
using System.Net;
using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;


namespace Petscribe.Services.Game
{
    public static class Extensions
    {
        public static T As<T>(this IHas self) where T : new()
        {
            var t = self.Get<T>();
            return t;
        }
    }

    public class Pos2d
    {
        public double X { get; set; }

        public double Y { get; set; }
    }

    public class Rot2d
    {
        private double w;
        public double W 
        {
            get => w;
            set => w = value % M.tpi;
        }
    }

    public interface IObj2d : IHas<Pos2d>, IHas<Rot2d>
    {
    }

    public class Velocity2d
    {
        public double DDX { get; set; }
        public double DDY { get; set; }
    }

    public class Mass
    {
        public double Kg { get; set; }
    }

    public interface IMomentum : IHas<Mass>, IHas<Velocity2d>
    {
        public Mass Mass => Get<Mass>();
        public Velocity2d Velocity => Get<Velocity2d>();
    }

    public interface IHas
    {
        public IDictionary<string, object> P { get; }


        public void Set<T>(T value)
        {
            var name = typeof(T).FullName;
            P[name] = value;
        }

        public T Get<T>() where T : new()
        {
            var name = typeof(T).FullName;
            if (!P.ContainsKey(name))
            {
                P[name] = new T();
            }
            return (T)P[name];
        }
    }

    public interface IHas<T> : IHas where T : new()
    {
        public T Value
        {
            get => Get<T>();
            set => Set<T>(value);
        }
    }

    public abstract class GameObject
    {
        public GameObject(string key)
        {
            Key = key;
        }
        public string Key { get; init; }
    }

    public interface ISystem
    {
        void Step(WorldState w);
    }


    public class Square2d
    {
        public double W { get; set; }

        public double H { get; set; }
    }

    public class WorldState
    {
        public long TimeMs { get; set; }
        public Dictionary<string, GameObject> Objects = new();

        public GameObject Add(GameObject o)
        {
            Objects[o.Key] = o;
            return o;
        }

        public void Remove(string key)
        {
            if(Objects.ContainsKey(key))
            {
                Objects.Remove(key);
            }
        }

        public GameObject Get(string key)
        {            
            if(Objects.ContainsKey(key))
            {
                return Objects[key];
            }
            return default;
        }
    }

    public class GameLoop
    {
        public GameLoop()
        {
            World.Add(new Space("arena"));
        }

        public WorldState World { get; } = new WorldState();

        public event Action OnStep = () => {};
        public async Task Run()
        {
            int step = 10; //ms

            var sw = Stopwatch.StartNew();
            while (World.TimeMs >= 0)
            {
                var now = sw.ElapsedMilliseconds;
                await Step(World, step);
                var last = sw.ElapsedMilliseconds;
                var dstep = last - now;
                var wait = (int)(step - dstep);
                await Task.Delay(Math.Max(wait, 5));
                OnStep();
            }
        }

        public async Task Step(WorldState world, int ms)
        {
            foreach (var o in world.Objects.Values)
            {
                if (o is IMomentum m)
                {
                    var dx = (ms / 1000.0) * m.Velocity.DDX / m.Mass.Kg;
                    var dy = (ms / 1000.0) * m.Velocity.DDY / m.Mass.Kg;
                    //m.Velocity.DDX = dx;
                    //m.Velocity.DDY = dy;
                    if (m is IHas<Pos2d> p)
                    {
                        p.Value.X += dx;
                        p.Value.Y += dy;
                    }
                }
                if(o is ISystem s)
                {
                    s.Step(world);
                }
            }
        }
    }
}