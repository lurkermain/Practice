using System.Text.Json.Serialization;

namespace Practice.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ModelType
    {
        Банка,
        Пакет
    }
}
