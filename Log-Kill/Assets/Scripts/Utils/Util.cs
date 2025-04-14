using System.Collections.Generic;
using UnityEngine;

namespace LogKill.Utils
{
    public class Util
    {
        public static void Suffle<T>(IList<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}