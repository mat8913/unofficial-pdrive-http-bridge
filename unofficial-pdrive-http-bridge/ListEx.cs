using System;
using System.Collections.Generic;
using System.Linq;

namespace unofficial_pdrive_http_bridge;

public static class ListEx
{
    public static int RemoveAll<T>(this IList<T> list, Func<T, bool> predicate)
    {
        var toRemove = list.Where(predicate).ToArray();
        foreach (var x in toRemove)
        {
            list.Remove(x);
        }
        return toRemove.Length;
    }
}
