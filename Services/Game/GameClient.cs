
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml.XPath;
using System.Reflection.Emit;
using System.Reflection;
using System.Xml.Linq;
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
        public double W;
        public double X;
        public double Y;
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
            if (item is IHas<Rot2d> r)
            {
                W = r.Value.W;
            }
            return new WXY { X = X, Y = Y, W = W };
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
            return s;
        }

        public async Task PushShipUpdate(Ship player)
        {
            // add new item to list of data and let the client generate new key for you (done offline)
            await Client
            .Child("ships")
            .Child($"{player.Key}")
            .PutAsync(StoreAs2d(player));
        }

        public async Task GetShipUpdate(string key)
        {
            //Console.WriteLine(key);
            var o = await Client
            .Child("ships")
            .Child($"{key}")
            .OnceSingleAsync<WXY>();

            WXY x = o;
            if(x != null)
            {
                Console.WriteLine(x);
                GameObject s;
                if(Game.World.Objects.TryGetValue(key, out s))
                {
                    
                }
                else
                {
                   s = Game.World.Add(new TriShip(key));
                }
                ReadAsWXY(s,x);
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
            }

            foreach (var ik in InKeys)
            {
                await GetShipUpdate(ik);
            }
        }
    }
}