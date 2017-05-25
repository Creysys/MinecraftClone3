using System;
using System.Collections.Generic;

namespace MinecraftClone3API.Util
{
    public static class Extensions
    {
        public static void ReverseForEach<T>(this IList<T> list, Action<T> action)
        {
            for (var i = list.Count - 1; i >= 0; i--)
                action(list[i]);
        }
    }
}
