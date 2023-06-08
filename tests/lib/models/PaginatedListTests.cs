using System.Collections.Generic;
using lib.models;
using System.Text.Json.Nodes;
using lib.extensions;
using Xunit;
using FluentAssertions;

namespace tests.lib.models
{
    public class PaginatedListTests
    {
        [Fact]
        public void PaginatedList_Serializes_ToCustomJsonObject()
        {
            var list = new List<int> { 1, 2, 3 };
            var paginatedList = new PaginatedList<int>(list, list.Count, 1, list.Count);
            var json = System.Text.Json.JsonSerializer.Serialize(paginatedList, LocalJsonOptions.GetOptions());
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
            #pragma warning restore CS8602
        }
    }
}