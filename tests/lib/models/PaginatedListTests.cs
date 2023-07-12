using System.Collections.Generic;
using lib.models;
using System.Text.Json.Nodes;
using lib.extensions;
using Xunit;
using FluentAssertions;
using System.Threading.Tasks;
using MockQueryable.Moq;

namespace tests.lib.models
{
    public class PaginatedListTests
    {
        [Fact]
        public void PaginatedList_Serializes_ToCustomJsonObject()
        {
            var list = new List<int> { 1, 2, 3 };
            var paginatedList = new PaginatedList<int>(list, list.Count, 1, list.Count);
            var json = System.Text.Json.JsonSerializer.Serialize(paginatedList, LocalJsonOptions.DefaultOptions);
            #pragma warning disable CS8602
            var JObject = JsonNode.Parse(json).AsObject();
            JObject["pageIndex"].GetValue<int>().Should().Be(paginatedList.PageIndex);
            JObject["totalPages"].GetValue<int>().Should().Be(paginatedList.TotalPages);
            JObject["pageSize"].GetValue<int>().Should().Be(paginatedList.PageSize);
            for (var i = 0; i < JObject["items"].AsArray().Count; i++)
            {
                JObject["items"][i].GetValue<int>().Should().Be(list[i]);
            }
            JObject["type"].GetValue<string>().Should().Be(paginatedList.ListType);
            JObject["totalCount"].GetValue<int>().Should().Be(list.Count);
            #pragma warning restore CS8602
        }

        private class DummyClass
        {
            public int Id { get; set; }
        }

        [Theory]
        [InlineData(12, 1, 1)]
        [InlineData(20, 2, 5)]
        [InlineData(100, 3, 10)]
        public async Task PaginatedList_CreateAsync_ReturnsPaginatedListWithCorrectMetrics(int entries, int pageIndex, int pageSize)
        {
    
            var testData = new List<DummyClass>();
            for (int i = 0; i < entries; i++)
            {
                testData.Add(new DummyClass { Id = i });
            }

            var expectedTotal = testData.Count;
            var expectedTotalPages = (int)System.Math.Ceiling((double)expectedTotal / (double)pageSize);
            var mockQueryable = testData.BuildMock();

            var paginatedList = await mockQueryable.PaginateAsync<DummyClass>(pageIndex, pageSize);

            paginatedList.TotalCount.Should().Be(expectedTotal);
            paginatedList.TotalPages.Should().Be(expectedTotalPages);
            paginatedList.PageIndex.Should().Be(pageIndex);
            paginatedList.PageSize.Should().Be(pageSize);
            paginatedList.Items.Count.Should().Be(pageSize);

        
        }
    }
}