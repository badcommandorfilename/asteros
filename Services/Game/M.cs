using System.Reflection;
using System;


namespace Petscribe.Services.Game
{
    public static class M
    {
        public static double tpi = (double)(2.0*Math.PI);
        public static double hpi = (double)(Math.PI/2.0);

        public static bool IsDerivedFrom<T>(object o)
        {
            return o.GetType().IsSubclassOf(typeof(T));
        }
    }
}