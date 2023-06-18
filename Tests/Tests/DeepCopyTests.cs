using System.Threading.Tasks;

using SaberLibrary;

using Tests.Helpers;

using Xunit;

namespace Tests.Tests
{
    public class DeepCopyTests
    {
        [Fact]
        public async Task DeepCopyTest()
        {
            //Arrange
            var linkedList = TestHelper.InitList(100);

            //Act

            var serializer = new LinkedListSerializer();
            var copy = await serializer.DeepCopy(linkedList);

            //Assert
            var cur = linkedList;
            var curCopy = copy;
            var listNodeComparer = new ListNodeComparer();

            while (cur != null)
            {
                Assert.Equal(cur, curCopy, listNodeComparer);

                cur = cur.Next;
                curCopy = curCopy.Next;
            }
        }
    }
}