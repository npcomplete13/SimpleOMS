using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleOMSClient.Helpers
{
    public static class RandomHelper
    {
        private static Random rng = new Random();

        public static int GetRandomInt(int min, int max)
        {
            return rng.Next(min, max);
        }

        public static T PickRandom<T>(this IList<T> list)
        {
            return list[rng.Next(list.Count)];
        }

        public static T PickRandom<T>(this IEnumerable<T> enumerable)
        {
            return enumerable.ToList().PickRandom();
        }

        public static T PickRandom<T>(this T[] array)
        {
            return array[rng.Next(array.Length)];
        }
    }
}
