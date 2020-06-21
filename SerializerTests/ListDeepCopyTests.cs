using Serializer;
using System;
using System.Collections.Generic;
using Xunit;

namespace SerializerTests
{
    public class ListDeepCopyTests
    {

        public static IEnumerable<object[]> ExamplesAndOutcomesNodes =>
            new List<object[]>
            {
                NodeDataShouldBeCorrect(),
                NodeNextShouldBeNull(),
                NodePreviousShouldBeNull(),
                NodeRandomShouldBeNull(),
                NodeNextDataShouldBeCorrect(),
                NodePreviousDataShouldBeCorrect(),
                NodeNextRandomShouldBeCorrect()
            };

        private static object[] NodeDataShouldBeCorrect()
        {
            return new object[]
            {
                new ListNode
                {
                    Data = "Test"
                },

                new Action<ListNode, ListNode>((node, expectedNode) =>
                {
                    Assert.NotNull(expectedNode);
                    Assert.Equal(expectedNode.Data, node.Data);
                }),
            };
        }

        private static object[] NodeNextShouldBeNull()
        {
            return new object[]
            {
                new ListNode(),

                new Action<ListNode, ListNode>((node, expectedNode) =>
                {
                    Assert.NotNull(expectedNode);
                    Assert.Null(expectedNode.Next);
                }),
            };
        }

        private static object[] NodePreviousShouldBeNull()
        {
            return new object[]
            {
                new ListNode(),

                new Action<ListNode, ListNode>((node, expectedNode) =>
                {
                    Assert.NotNull(expectedNode);
                    Assert.Null(expectedNode.Previous);
                }),
            };
        }

        private static object[] NodeRandomShouldBeNull()
        {
            return new object[]
            {
                new ListNode(),

                new Action<ListNode, ListNode>((node, expectedNode) =>
                {
                    Assert.NotNull(expectedNode);
                    Assert.Null(expectedNode.Random);
                }),
            };
        }

        private static object[] NodeNextDataShouldBeCorrect()
        {
            var node = new ListNode();
            var node2 = new ListNode
            {
                Data = "Test2"
            };

            node.Next = node2;
            node2.Previous = node;

            return new object[]
            {
                node,
                new Action<ListNode, ListNode>((node, expectedNode) =>
                {
                    Assert.NotNull(expectedNode);
                    Assert.NotNull(expectedNode.Next);
                    Assert.Equal(expectedNode.Next.Data, node.Next.Data);
                }),
            };
        }

        private static object[] NodePreviousDataShouldBeCorrect()
        {
            var node = new ListNode()
            {
                Data = "Test1"
            };
            var node2 = new ListNode();

            node.Next = node2;
            node2.Previous = node;

            return new object[]
            {
                node,
                new Action<ListNode, ListNode>((node, expectedNode) =>
                {
                    Assert.NotNull(expectedNode);
                    Assert.NotNull(expectedNode.Next);
                    Assert.Equal(expectedNode.Next.Previous.Data, node.Data);
                }),
            };
        }

        private static object[] NodeNextRandomShouldBeCorrect()
        {
            var node = new ListNode()
            {
                Data = "Test1"
            };
            var node2 = new ListNode();

            node.Next = node2;
            node.Next.Random = node;
            node2.Previous = node;

            return new object[]
            {
                node,
                new Action<ListNode, ListNode>((node, expectedNode) =>
                {
                    Assert.NotNull(expectedNode);
                    Assert.NotNull(expectedNode.Next);
                    Assert.Equal(expectedNode.Next.Random.Data, node.Data);
                }),
            };
        }

        [Theory]
        [MemberData(nameof(ExamplesAndOutcomesNodes))]
        public async void DeepCopyTest(ListNode arrange, Action<ListNode, ListNode> assert)
        {
            var listSerializer = new ListSerializer();
            var result = await listSerializer.DeepCopy(arrange);

            assert(arrange, result);
        }
    }
}
