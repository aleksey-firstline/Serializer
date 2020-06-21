using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Serializer
{
    public class ListSerializer : IListSerializer
    {
        private const int IntBytesSize = 4;

        public async Task Serialize(ListNode head, Stream s)
        {
            var actions = new List<Func<Stream, Task>>();
            var nodes = new Dictionary<ListNode, int>();

            var index = 0;
            var utf8 = new UTF8Encoding();
            var bytes = new List<byte>();

            while (head != null)
            {
                var dataBytes = utf8.GetBytes(head.Data ?? string.Empty);
                var dataBytesLength = BitConverter.GetBytes(dataBytes.Length);
                var indexBytes = BitConverter.GetBytes(index);

                bytes.AddRange(dataBytesLength);
                bytes.AddRange(dataBytes);
                bytes.AddRange(indexBytes);

                nodes.Add(head, index);

                if (head.Random != null)
                {
                    var random = head.Random;
                    var nodeIndex = index;
                    actions.Add(async a =>
                    {
                        var randomIndex = nodes[random];
                        var nodeIndexBytes = BitConverter.GetBytes(nodeIndex);
                        var randomIndexBytes = BitConverter.GetBytes(randomIndex);

                        await a.WriteAsync(nodeIndexBytes, 0, nodeIndexBytes.Length);
                        await a.WriteAsync(randomIndexBytes, 0, randomIndexBytes.Length);
                    });
                }

                index++;
                head = head.Next;
            }

            var nodeCountBytes = BitConverter.GetBytes(index);
            await s.WriteAsync(nodeCountBytes, 0, nodeCountBytes.Length);
            await s.WriteAsync(bytes.ToArray(), 0, bytes.Count);

            foreach (var action in actions)
            {
                await action.Invoke(s);
            }
        }

        public async Task<ListNode> Deserialize(Stream s)
        {
            s.Position = 0;
            var utf8 = new UTF8Encoding();

            var bytes = new byte[IntBytesSize];
            await s.ReadAsync(bytes, 0, bytes.Length);
            var nodeCount = BitConverter.ToInt32(bytes, 0);

            var nodes = new Dictionary<int, ListNode>();
            while (s.Position < s.Length)
            {
                for (var i = 0; i < nodeCount; i++)
                {
                    var note = new ListNode();

                    bytes = new byte[IntBytesSize];
                    await s.ReadAsync(bytes, 0, bytes.Length);
                    var dataBytesLength = BitConverter.ToInt32(bytes, 0);

                    bytes = new byte[dataBytesLength];
                    await s.ReadAsync(bytes, 0, dataBytesLength);
                    note.Data = utf8.GetString(bytes);

                    bytes = new byte[IntBytesSize];
                    await s.ReadAsync(bytes, 0, bytes.Length);
                    var index = BitConverter.ToInt32(bytes, 0);

                    if (i > 0)
                    {
                        nodes[i - 1].Next = note;
                        note.Previous = nodes[i - 1];
                    }

                    nodes.Add(index, note);
                }

                if (s.Position < s.Length)
                {
                    bytes = new byte[IntBytesSize];
                    await s.ReadAsync(bytes, 0, bytes.Length);
                    var nodeIndex = BitConverter.ToInt32(bytes, 0);

                    await s.ReadAsync(bytes, 0, bytes.Length);
                    var randomIndex = BitConverter.ToInt32(bytes, 0);

                    nodes[nodeIndex].Random = nodes[randomIndex];
                }
            }

            return nodes[0];
        }

        public async Task<ListNode> DeepCopy(ListNode head)
        {
            return await Task.Run(() =>
            {
                var actions = new List<Action<Dictionary<ListNode, ListNode>>>();
                var nodes = new Dictionary<ListNode, ListNode>();
                var copyNode = new ListNode();
                var tempNode = copyNode;
                
                while (head != null)
                {
                    tempNode.Data = head.Data;
                    nodes.Add(head, tempNode);

                    if (head.Random != null)
                    {
                        var key = head.Random;
                        var targetNode = tempNode;
                        actions.Add(n =>
                        {
                            var random = nodes[key];
                            targetNode.Random = random;
                        });
                    }

                    head = head.Next;

                    if (head != null)
                    {
                        tempNode.Next = new ListNode
                        {
                            Previous = tempNode
                        };

                        tempNode = tempNode.Next;
                    }
                }

                foreach (var action in actions)
                {
                    action?.Invoke(nodes);
                }

                return copyNode;
            });
        }
    }
}
