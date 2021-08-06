using System.Runtime.Serialization;
using System.Reflection.Metadata;
using System.Net.Http;
using System.Reflection.PortableExecutable;
using System.IO;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.AccessControl;
using System.IO.Enumeration;
using System.Security.Cryptography.X509Certificates;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Firebase.Database;
using Firebase.Database.Query;


namespace Petscribe.Services.Game
{
    public class WXY
    {
        public string Key { get; init; }
        public double W;
        public double X;
        public double Y;
        public DateTime Birth;

        public DateTime Death;
    }

    public class GameClient
    {
        public GameClient()
        {
            var apiKey = "AIzaSyC1I0YzZOw7Rr-l20piKiPXM6ypVA4TLWU";
            var firebase = new FirebaseClient(
                "https://spacewar-271f1-default-rtdb.asia-southeast1.firebasedatabase.app/",
            new FirebaseOptions());
            Client = firebase;
        }
        public GameClient(FirebaseClient client)
        {
            Client = client;
        }
        public GameLoop Game { get; } = new GameLoop();

        public FirebaseClient Client { get; }

        public static WXY StoreAs2d(IHas<Pos2d> item)
        {
            var X = item.Value.X;
            var Y = item.Value.Y;
            var W = 0.0;
            var Birth = DateTime.MinValue;
            var Death = DateTime.MaxValue;
            if (item is IHas<Rot2d> r)
            {
                W = r.Value.W;
            }
            if (item is IHas<Lifetime> t)
            {
                Birth = t.Value.Birth;
                Death = t.Value.Death;
            }

            return new WXY { 
                Key = (item as GameObject).Key,
                X = X, Y = Y, W = W, 
                Birth = Birth, Death = Death };
        }

        public static GameObject ReadAsWXY(GameObject s, WXY x)
        {
            if(s is IHas<Pos2d> p)
            {
                p.Value.X = x.X;
                p.Value.Y = x.Y;
            }
            if(s is IHas<Rot2d> r)
            {
                r.Value.W = x.W;
            }
            if(s is IHas<Lifetime> t)
            {
                t.Value.Death = x.Death;
            }
            return s;
        }

        public async Task PushShipUpdate(Ship player)
        {
            // add new item to list of data and let the client generate new key for you (done offline)
            await Client
            .Child("ships")
            .Child($"{player.Key}")
            .PutAsync(StoreAs2d(player));

            var shots = player.Shells
            .Where(s => !s.IsDead)
            .Select(s => StoreAs2d(s))
            .ToDictionary(o => o.Key, o => o);

            await Client
            .Child("ships")
            .Child($"{player.Key}")
            .Child("shells")
            .PutAsync(shots);
        }

        public async Task GetShellsUpdate(string key)
        {
            var shells = await Client
            .Child("ships")
            .Child($"{key}")
            .Child("shells")
            .OnceAsync<WXY>();

            Console.WriteLine(shells.Count);

            if(shells != null)
            {
                foreach (var o in shells)
                {
                    WXY w = o.Object;
                    Console.WriteLine(w.Key);
                    Game.World.Objects.TryGetValue(w.Key, out GameObject s);
                    s = s ?? Game.World.Add(new Shell(w.Key));
                    ReadAsWXY(s,w);
                }
            }
        }

        public async Task GetShipUpdate(string key)
        {
            var o = await Client
            .Child("ships")
            .Child($"{key}")
            .OnceSingleAsync<WXY>();

            WXY x = o;
            if(x != null)
            {
                //Console.WriteLine(x);
                Game.World.Objects.TryGetValue(key, out GameObject s);
                s = s ?? Game.World.Add(new TriShip(key));
                ReadAsWXY(s,x);

                GetShellsUpdate(key);
            }

        }

        public HashSet<string> InKeys { get; } = new HashSet<string>();
        public HashSet<string> OutKeys { get; } = new HashSet<string>();

        public async Task Sync(int max, int t = 200)
        {
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < max; i++)
            {
                var now = sw.ElapsedMilliseconds;

                try
                {
                    await step();
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                var last = sw.ElapsedMilliseconds;
                var dstep = last - now;
                var wait = (int)(t - dstep);
                await Task.Delay(Math.Max(wait, 5));
            }
        }

        private async Task step()
        {
            foreach (var ok in OutKeys)
            {
                var o = Game.World.Objects[ok];
                //Console.WriteLine(o.Key);
                if (o is TriShip s)
                {
                    await PushShipUpdate(s);
                }
                if (o is Shell a)
                {

                }
            }

            foreach (var ik in InKeys)
            {
                await GetShipUpdate(ik);
            }
        }
    }
}