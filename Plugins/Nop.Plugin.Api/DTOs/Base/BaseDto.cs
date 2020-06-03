using Newtonsoft.Json;

namespace Nop.Plugin.Api.DTOs.Base
{
    public abstract class BaseDto
    {
        [JsonProperty("Id")]
        public int Id { get; set; }
    }
}