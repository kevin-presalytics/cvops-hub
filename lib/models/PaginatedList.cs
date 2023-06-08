using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Reflection;

namespace lib.models
{
    public class PaginatedList<T> : List<T>
    {
        [JsonPropertyName("pageIndex")]
        public int PageIndex { get; private set; }
        [JsonPropertyName("totalPages")]
        public int TotalPages { get; private set; }
        [JsonPropertyName("pageSize")]
        public int PageSize { get; private set;}
        [JsonPropertyName("type")]
        public string ListType { 
            get { 
                return typeof(T).Name;
            }
        }
        [JsonPropertyName("items")]
        public List<T> items { 
            get { 
                return this.ToList();
            }
        }

        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);

            this.AddRange(items);
        }

        public bool HasPreviousPage
        {
            get
            {
                return (PageIndex > 1);
            }
        }

        public bool HasNextPage
        {
            get
            {
                return (PageIndex < TotalPages);
            }
        }

        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
        {
            var count = await source.CountAsync();
            var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }
    }

    public class PaginatedListJsonConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            if (!typeToConvert.IsGenericType)
            {
                return false;
            }

            if (typeToConvert.GetGenericTypeDefinition() != typeof(PaginatedList<>))
            {
                return false;
            }
            return true;
        }

        public override JsonConverter CreateConverter(
            Type type,
            JsonSerializerOptions options)
        {
            Type[] listType = type.GetGenericArguments();

            JsonConverter converter = (JsonConverter)Activator.CreateInstance(
                typeof(PaginatedListConverterInner<>).MakeGenericType(listType),
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                args: new object[] { options },
                culture: null)!;

            return converter;
        }

        private class PaginatedListConverterInner<T> : JsonConverter<PaginatedList<T>> where T : notnull
        {
            private readonly JsonConverter<T> _valueConverter;
            private readonly Type _itemsType;

            public PaginatedListConverterInner(JsonSerializerOptions options)
            {
                // For performance, use the existing converter.
                _valueConverter = (JsonConverter<T>)options.GetConverter(typeof(T));
                
                // Get the type of the items in the dictionary.
                _itemsType = typeof(T);
            }

            public override PaginatedList<T> Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                // Hub only emits paginated lists, so we don't need to implement this
               throw new NotImplementedException();
            }

            public override void Write(Utf8JsonWriter writer, PaginatedList<T> paginatedList, JsonSerializerOptions options)
            {
                writer.WriteStartObject();
                writer.WriteNumber("pageIndex", paginatedList.PageIndex);
                writer.WriteNumber("totalPages", paginatedList.TotalPages);
                writer.WriteNumber("pageSize", paginatedList.PageSize);
                writer.WriteString("type", paginatedList.ListType);
                writer.WritePropertyName("items");
                writer.WriteStartArray();
                foreach (var item in paginatedList)
                {
                    _valueConverter.Write(writer, item, options);
                }
                writer.WriteEndArray();
                writer.WriteEndObject();
            }
        }
    }

    public static class PaginatedListExtensions
    {
        public static async Task<PaginatedList<T>> PaginateAsync<T>(this IQueryable<T> source, int pageIndex, int pageSize)
        {
            return await PaginatedList<T>.CreateAsync(source, pageIndex, pageSize);
        }
    }
}