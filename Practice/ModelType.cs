using System.Text.Json.Serialization;

namespace Practice
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ModelType
    {
        Банка,
        Пакет

    }
}
