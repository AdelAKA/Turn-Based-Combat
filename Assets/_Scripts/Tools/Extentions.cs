using System.Collections.Generic;
using UnityEngine;

namespace GP7.Prodigy.Combat
{
    public static class Extentions
    {
        public static void DestroyAllChildren(this Transform transform)
        {
            foreach (Transform item in transform)
            {

                Object.Destroy(item.gameObject);
            }
        }

        public static T Random<T>(this List<T> list)
        {
            return list[UnityEngine.Random.Range(0, list.Count)];
        }
    }
}
