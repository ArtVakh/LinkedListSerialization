using System;
using System.Collections.Generic;

using SaberLibrary;

namespace Tests.Helpers;

public class ListNodeComparer : IEqualityComparer<ListNode>
{
    public bool Equals(ListNode x, ListNode y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }

        if (ReferenceEquals(x, null))
        {
            return false;
        }

        if (ReferenceEquals(y, null))
        {
            return false;
        }

        if (x.GetType() != y.GetType())
        {
            return false;
        }

        return x.Data == y.Data &&
               x.Next?.Data == y.Next?.Data &&
               x.Previous?.Data == y.Previous?.Data &&
               x.Random?.Data == y.Random?.Data;
    }

    public int GetHashCode(ListNode obj)
    {
        return HashCode.Combine(obj.Previous, obj.Next, obj.Random, obj.Data);
    }
}