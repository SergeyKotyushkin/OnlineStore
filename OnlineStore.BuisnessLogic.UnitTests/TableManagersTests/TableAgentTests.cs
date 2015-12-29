using Microsoft.VisualStudio.TestTools.UnitTesting;
using OnlineStore.BuisnessLogic.TableManagers;

namespace OnlineStore.BuisnessLogic.UnitTests.TableManagersTests
{
    [TestClass]
    public class TableAgentTests
    {
        [TestMethod]
        public void GetPageIndex_CheckNumberPages_ReturnExpectedPagesNumbers()
        {
            var expected = new[] {1, 1, 2, 3, 4, 4};

            var agent = new TableAgent<object, object>(null);
            var actual = new[]
            {
                agent.GetNewPageIndex(0, 2, 4), agent.GetNewPageIndex(1, 2, 4), agent.GetNewPageIndex(2, 2, 4),
                agent.GetNewPageIndex(3, 2, 4), agent.GetNewPageIndex(4, 2, 4), agent.GetNewPageIndex(5, 2, 4)
            };

            Assert.AreEqual(expected.Length, actual.Length);
            for (var i = 0; i < expected.Length; i++)
                Assert.AreEqual(expected[i], actual[i]);
        }
        
        [TestMethod]
        public void GetPages_CheckNotNumberPages_ReturnExpectedPagesIndexies()
        {
            var agent = new TableAgent<object, object>(null);

            var expected = new[]
            {
                new[] {2, 3, 4},
                new[] {2, 3, 4},
                new[] {2, 3, 4},
                new[] {1, 2, 3},
                new[] {3, 4, 5, 6, 7, 8, 9, 10},
                new[] {3, 4},
                new[] {3}
            };

            var actual = new[]
            {
                agent.GetPages(2, 4, 3),
                agent.GetPages(3, 4, 3),
                agent.GetPages(4, 4, 3),
                agent.GetPages(1, 4, 3),
                agent.GetPages(3, 10, 8),
                agent.GetPages(3, 5, 2),
                agent.GetPages(3, 5, 1)
            };

            Assert.AreEqual(expected.GetLength(0), actual.Length);
            for (var i = 0; i < expected.Length; i++)
                for (var j = 0; j < expected[i].Length; j++)
                Assert.AreEqual(expected[i][j], actual[i][j]);
        }
    }
}
