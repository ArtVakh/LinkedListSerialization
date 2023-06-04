using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;

namespace SaberLibrary;

//Specify your class\file name and complete implementation.
public class LinkedListSerializer : IListSerializer
{
    private readonly struct ListNodeData
    {
        public readonly string data;
        public readonly int? randomIndex;

        public ListNodeData(string data, int? randomIndex)
        {
            this.data = data;
            this.randomIndex = randomIndex;
        }
    }

    public const string SerializationFormat = "{0};{1}\n";

    private const string InvalidFormatExceptionMessage = "Invalid file format";

    //the constructor with no parameters is required and no other constructors can be used.
    [SuppressMessage("ReSharper", "EmptyConstructor")]
    public LinkedListSerializer()
    {
        //...
    }

    public Task<ListNode> DeepCopy(ListNode head)
    {
        if (head == null)
        {
            return null;
        }

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

        return Task.FromResult(newHead);
    }

    public async Task<ListNode> Deserialize(Stream s)
    {
        using var sr = new StreamReader(s, leaveOpen: true);

        var firstLine = await sr.ReadLineAsync().ConfigureAwait(false);
        if (firstLine == null)
        {
            return null;
        }

        var map = new Dictionary<int, ListNode>();
        var head = ProcessNode(firstLine, map, 0);

        var i = 1;
        await foreach (var line in GetLines(sr).ConfigureAwait(false))
        {
            var curNode = ProcessNode(line, map, i);

            curNode.Previous = map[i - 1];
            map[i - 1].Next = curNode;
            i++;
        }

        return head;
    }

    public async Task Serialize(ListNode head, Stream s)
    {
        if (head == null)
        {
            return;
        }

        var map = new Dictionary<ListNode, int?>();

        var cur = head;
        var i = 0;
        while (cur != null)
        {
            map.Add(cur, i);

            i++;
            cur = cur.Next;
        }

        var sw = new StreamWriter(s, leaveOpen: true);
        await using (sw.ConfigureAwait(false))
        {
            cur = head;
            while (cur != null)
            {
                var randomReference = cur.Random == null ? null : map[cur.Random];

                await sw.WriteAsync(string.Format(SerializationFormat, randomReference, cur.Data)).ConfigureAwait(false);
                cur = cur.Next;
            }
        }
    }

    private static ListNode ProcessNode(string line, Dictionary<int, ListNode> map, int position)
    {
        if (!map.TryGetValue(position, out var curNode))
        {
            curNode = new ListNode();
            map.Add(position, curNode);
        }

        var nodeData = GetNodeData(line);
        ListNode randomNode = null;
        if (nodeData.randomIndex.HasValue)
        {
            var randomIndex = nodeData.randomIndex.Value;

            if (!map.TryGetValue(randomIndex, out randomNode))
            {
                randomNode = new ListNode();
                map.Add(randomIndex, randomNode);
            }
        }

        curNode.Data = nodeData.data;
        curNode.Random = randomNode;

        return curNode;
    }

    private static ListNodeData GetNodeData(string line)
    {
        var delimiterIndex = line.IndexOf(';');
        if (delimiterIndex == -1)
        {
            throw new FormatException(InvalidFormatExceptionMessage);
        }

        var data = string.Empty;

        if (delimiterIndex < line.Length - 1)
        {
            data = line[(delimiterIndex + 1)..];
        }

        if (delimiterIndex == 0)
        {
            return new ListNodeData(data, null);
        }

        var numberString = line[..delimiterIndex];
        if (!int.TryParse(numberString, out var number))
        {
            throw new FormatException(InvalidFormatExceptionMessage);
        }

        return new ListNodeData(data, number);
    }

    private static async IAsyncEnumerable<string> GetLines(TextReader stream)
    {
        while (true)
        {
            var line = await stream.ReadLineAsync();
            if (line == null)
            {
                break;
            }

            yield return line;
        }
    }
}