using System.Text.Json.Serialization;

namespace Practice.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ModelType
    {
        Банка_80г,
        Банка_110г,
        Пакет
    }
}
