using System;

using SaberLibrary;

namespace Tests.Helpers;

public static class TestHelper
{
    public static ListNode InitList(int length, int? seed = null)
    {
        if (length == 0)
        {
            return null;
        }

        var rand = seed.HasValue ? new Random(seed.Value) : new Random();
        var nodesMap = new ListNode[length];

        var head = new ListNode
        {
            Data = GetData(rand)
        };

        nodesMap[0] = head;

        var cur = head;
        for (var i = 1; i < length; i++)
        {
            var newNode = new ListNode()
            {
                Data = GetData(rand),
                Previous = cur
            };
            nodesMap[i] = newNode;

            cur.Next = newNode;
            cur = cur.Next;
        }

        cur = head;
        while (cur != null)
        {
            var dice = rand.Next(6);
            if (dice < 2)
            {
                var randElement = rand.Next(length);
                cur.Random = nodesMap[randElement];
            }

            cur = cur.Next;
        }

        return head;
    }

    private static string GetData(Random rand)
    {
        return rand.Next(1000).ToString();
    }
}