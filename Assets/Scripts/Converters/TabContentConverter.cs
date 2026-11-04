using DnD.Model;
using DnD.Model.Inventory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using UnityEngine;

namespace DnD.Converters
{
    public class TabContentConverter : JsonConverter<TabContent>
    {
        public override bool CanWrite => false;

        public override TabContent ReadJson(
            JsonReader reader,
            Type objectType,
            TabContent existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                throw new JsonSerializationException("Tab content cannot be null.");

            JObject obj = JObject.Load(reader);

            var typeToken = obj["contentType"]
                ?? throw new JsonSerializationException("Tab content type is missing.");
            EContentType type = typeToken.ToObject<EContentType>();

            TabContent item = TabContent.Factory(type);
            serializer.Populate(obj.CreateReader(), item);

            return item;
        }

        public override void WriteJson(JsonWriter writer, TabContent value, JsonSerializer serializer)
        {
            throw new NotSupportedException("Use the default writer for tab content.");
        }
    }
}