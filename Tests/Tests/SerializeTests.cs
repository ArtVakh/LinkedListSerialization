using System;
using System.IO;
using System.Threading.Tasks;

using FluentAssertions;

using SaberLibrary;

using Tests.Helpers;

using Xunit;

namespace Tests.Tests;

public class SerializeTests
{
    private const int BufferSize = 65536;
    private const int ListLength = 1_000_000;
    private const int Seed = 1;

    [Fact]
    public async Task Serialize_Deserialize_Successful()
    {
        //Arrange
        var serializer = new LinkedListSerializer();
        var linkedList = TestHelper.InitList(ListLength, Seed);

        //Act
        const string fileName = $"{nameof(Serialize_Deserialize_Successful)}.test";

        var stream = new FileStream(
            fileName,
            FileMode.Create,
            FileAccess.ReadWrite,
            FileShare.Read,
            BufferSize,
            FileOptions.Asynchronous | FileOptions.SequentialScan);

        ListNode result;
        await using (stream)
        {
            await serializer.Serialize(linkedList, stream);

            stream.Position = 0;

            result = await serializer.Deserialize(stream);
        }

        //Assert
        var cur = linkedList;
        var curCopy = result;
        var listNodeComparer = new ListNodeComparer();

        while (cur != null)
        {
            Assert.Equal(cur, curCopy, listNodeComparer);

            cur = cur.Next;
            curCopy = curCopy.Next;
        }
    }

    [Fact]
    public async Task Deserialize_WrongFileContent_Failure()
    {
        //Arrange
        var serializer = new LinkedListSerializer();
        const string fileName = $"{nameof(Deserialize_WrongFileContent_Failure)}.test";
        await File.WriteAllTextAsync(fileName, "Some Random Content");

        //Act
        var stream = new FileStream(
            fileName,
            FileMode.Open,
            FileAccess.ReadWrite,
            FileShare.Read,
            BufferSize,
            FileOptions.Asynchronous | FileOptions.SequentialScan);

        await using (stream)
        {
            Func<Task> act = () => serializer.Deserialize(stream);

            //Assert
            var exception = await act.Should().ThrowAsync<FormatException>();
            exception.WithMessage(LinkedListSerializer.InvalidFormatExceptionMessage);
        }
    }

    [Fact]
    public async Task Deserialize_EmptyFile_Failure()
    {
        //Arrange
        var serializer = new LinkedListSerializer();
        const string fileName = $"{nameof(Deserialize_EmptyFile_Failure)}.test";

        //Act
        var stream = new FileStream(
            fileName,
            FileMode.Create,
            FileAccess.ReadWrite,
            FileShare.Read,
            BufferSize,
            FileOptions.Asynchronous | FileOptions.SequentialScan);

        await using (stream)
        {
            Func<Task> act = () => serializer.Deserialize(stream);

            //Assert
            var exception = await act.Should().ThrowAsync<ArgumentException>();
            exception.WithMessage(LinkedListSerializer.EmptyFileExceptionMessage);
        }
    }
}