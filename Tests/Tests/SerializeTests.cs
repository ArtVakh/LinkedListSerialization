using System.IO;
using System.Text;
using System.Threading.Tasks;

using SaberLibrary;

using Tests.Helpers;

using Xunit;

namespace Tests.Tests;

public class SerializeTests
{
    private const int BufferSize = 65536;

    private readonly LinkedListSerializer serializer;

    public SerializeTests()
    {
        serializer = new LinkedListSerializer();
    }

    [Fact]
    public async Task Serialize_List_Successful()
    {
        //Arrange
        var items = new ListNode[]
        {
            new() {Data = "emptyRandom"},
            new() {Data = ";randomToSelf"},
            new() {Data = ";randomToLast"},
            new() {Data = "randomToZero;"},
        };

        items[1].Random = items[1];
        items[2].Random = items[3];
        items[3].Random = items[0];

        var sb = new StringBuilder();
        sb.AppendFormat(LinkedListSerializer.SerializationFormat, null, items[0].Data);
        sb.AppendFormat(LinkedListSerializer.SerializationFormat, 1, items[1].Data);
        sb.AppendFormat(LinkedListSerializer.SerializationFormat, 3, items[2].Data);
        sb.AppendFormat(LinkedListSerializer.SerializationFormat, 0, items[3].Data);
        var expectedContent = sb.ToString();

        for (var i = 0; i < items.Length; i++)
        {
            if (i > 0)
            {
                items[i].Previous = items[i - 1];
            }

            if (i < items.Length - 1)
            {
                items[i].Next = items[i + 1];
            }
        }


        //Act
        string fileContent;

        const string fileName = $"{nameof(Serialize_List_Successful)}.test";
        var stream = new FileStream(
            fileName,
            FileMode.Create,
            FileAccess.ReadWrite,
            FileShare.Read,
            BufferSize,
            FileOptions.Asynchronous | FileOptions.SequentialScan);

        await using (stream)
        {
            await serializer.Serialize(items[0], stream);

            stream.Position = 0;

            using var sr = new StreamReader(stream);
            fileContent = await sr.ReadToEndAsync();
        }

        //Assert
        Assert.Equal(expectedContent, fileContent);
    }

    [Fact]
    public async Task Deserialize_List_Successful()
    {
        //Arrange
        var expectedItems = new ListNode[]
        {
            new() {Data = "emptyRandom"},
            new() {Data = ";randomToSelf"},
            new() {Data = ";randomToLast"},
            new() {Data = "randomToZero;"},
        };

        expectedItems[1].Random = expectedItems[1];
        expectedItems[2].Random = expectedItems[3];
        expectedItems[3].Random = expectedItems[0];

        var sb = new StringBuilder();
        sb.AppendFormat(LinkedListSerializer.SerializationFormat, null, expectedItems[0].Data);
        sb.AppendFormat(LinkedListSerializer.SerializationFormat, 1, expectedItems[1].Data);
        sb.AppendFormat(LinkedListSerializer.SerializationFormat, 3, expectedItems[2].Data);
        sb.AppendFormat(LinkedListSerializer.SerializationFormat, 0, expectedItems[3].Data);
        var fileContent = sb.ToString();

        for (var i = 0; i < expectedItems.Length; i++)
        {
            if (i > 0)
            {
                expectedItems[i].Previous = expectedItems[i - 1];
            }

            if (i < expectedItems.Length - 1)
            {
                expectedItems[i].Next = expectedItems[i + 1];
            }
        }


        //Act
        ListNode deserializedHead;
        const string fileName = $"{nameof(Deserialize_List_Successful)}.test";
        var stream = new FileStream(
            fileName,
            FileMode.Create,
            FileAccess.ReadWrite,
            FileShare.Read,
            BufferSize,
            options: FileOptions.Asynchronous | FileOptions.SequentialScan);

        await using (var sw = new StreamWriter(stream))
        {
            await sw.WriteAsync(fileContent);
            await sw.FlushAsync();
            stream.Position = 0;

            deserializedHead = await serializer.Deserialize(stream);
        }

        //Assert
        Assert.Equal(fileContent, fileContent);

        var expected = expectedItems[0];
        var curDeserialized = deserializedHead;
        var listNodeComparer = new ListNodeComparer();

        while (expected != null)
        {
            Assert.Equal(expected, curDeserialized, listNodeComparer);

            expected = expected.Next;
            curDeserialized = curDeserialized.Next;
        }
    }
}