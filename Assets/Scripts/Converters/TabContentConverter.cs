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
        public override TabContent ReadJson(
            JsonReader reader,
            Type objectType,
            TabContent existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            JObject obj = JObject.Load(reader);

            EContentType type = obj["contentType"].ToObject<EContentType>();

            TabContent item = TabContent.Factory(type);
            serializer.Populate(obj.CreateReader(), item);

            return item;
        }

        public override void WriteJson(JsonWriter writer, TabContent value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}