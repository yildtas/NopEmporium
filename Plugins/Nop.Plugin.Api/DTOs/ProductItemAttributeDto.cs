using Newtonsoft.Json;
using Nop.Plugin.Api.DTOs.Base;

namespace Nop.Plugin.Api.DTOs
{
    [JsonObject(Title = "Attribute")]
    public class ProductItemAttributeDto 
    {
        [JsonProperty("Key")]
        public int Key { get; set; }

        [JsonProperty("Value")]
        public string Value { get; set; }

    }
}
