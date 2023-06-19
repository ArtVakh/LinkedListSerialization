using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SaberLibrary;

public class LinkedListSerializer : IListSerializer
{
    public const string InvalidFormatExceptionMessage = "Invalid file format";
    public const string EmptyFileExceptionMessage = "Empty file";

    private static readonly Encoding Encoding = Encoding.UTF8;

    public Task<ListNode> DeepCopy(ListNode head)
    {
        if (head == null)
        {
            return null;
        }

        return Task.Run(() => DeepCopyInner(head));
    }

    public Task<ListNode> Deserialize(Stream s)
    {
        if (s.Length == 0)
        {
            throw new ArgumentException(EmptyFileExceptionMessage);
        }

        return Task.Run(() => DeserializeInner(s));
    }

    public Task Serialize(ListNode head, Stream s)
    {
        if (head == null)
        {
            return Task.CompletedTask;
        }

        return Task.Run(() => SerializeInner(head, s));
    }

    private static ListNode GetOrAddNode(IDictionary<int, ListNode> map, int nodeIndex)
    {
        if (map.TryGetValue(nodeIndex, out var node))
        {
            return node;
        }

        node = new ListNode();
        map.Add(nodeIndex, node);

        return node;
    }

    private static ListNode DeepCopyInner(ListNode head)
    {
        var dict = new Dictionary<ListNode, ListNode>();
        var newHead = new ListNode {Data = head.Data};
        dict.Add(head, newHead);

        var cur = head.Next;
        var curNew = newHead;
        while (cur != null)
        {
            var nextNewNode = new ListNode
            {
                Data = cur.Data,
                Previous = curNew
            };

            dict.Add(cur, nextNewNode);

            curNew.Next = nextNewNode;
            curNew = curNew.Next;
            cur = cur.Next;
        }

        cur = head;
        curNew = newHead;
        while (cur != null)
        {
            if (cur.Random != null)
            {
                curNew.Random = dict[cur.Random];
            }

            cur = cur.Next;
            curNew = curNew.Next;
        }

        return newHead;
    }

    private static ListNode DeserializeInner(Stream s)
    {
        using var binaryReader = new BinaryReader(s, Encoding, true);

        var map = new Dictionary<int, ListNode>();
        var i = 0;
        var fileSize = binaryReader.BaseStream.Length;

        try
        {
            while (binaryReader.BaseStream.Position < fileSize)
            {
                ListNode randomNode = null;

                var hasRandomReference = binaryReader.ReadBoolean();
                if (hasRandomReference)
                {
                    var randomReference = binaryReader.Read7BitEncodedInt();
                    randomNode = GetOrAddNode(map, randomReference);
                }

                var curNode = GetOrAddNode(map, i);

                var data = binaryReader.ReadString();
                curNode.Data = data;
                curNode.Random = randomNode;

                if (i > 0)
                {
                    curNode.Previous = map[i - 1];
                    curNode.Previous.Next = curNode;
                }

                i++;
            }
        }
        catch (EndOfStreamException)
        {
            throw new FormatException(InvalidFormatExceptionMessage);
        }

        return map[0];
    }

    private static async Task SerializeInner(ListNode head, Stream s)
    {
        var map = new Dictionary<ListNode, int>();

        var cur = head;
        var i = 0;
        while (cur != null)
        {
            map.Add(cur, i);

            i++;
            cur = cur.Next;
        }

        var sw = new BinaryWriter(s, Encoding, true);
        await using (sw.ConfigureAwait(false))
        {
            cur = head;
            while (cur != null)
            {
                var hasRandomReference = cur.Random != null;
                sw.Write(hasRandomReference);

                if (hasRandomReference)
                {
                    sw.Write7BitEncodedInt(map[cur.Random]);
                }

                sw.Write(cur.Data);

                cur = cur.Next;
            }
        }
    }
}