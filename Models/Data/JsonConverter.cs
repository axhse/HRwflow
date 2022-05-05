using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HRwflow.Models.Data
{
    public class JsonConverter<TModel> : ValueConverter<TModel, string>
    {
        public JsonConverter() : base(model => ToJson(model), json => FromJson(json))
        { }

        public static TModel FromJson(string json) => JsonSerializer.Deserialize<TModel>(json);

        public static string ToJson(TModel model) => JsonSerializer.Serialize(model);
    }
}
