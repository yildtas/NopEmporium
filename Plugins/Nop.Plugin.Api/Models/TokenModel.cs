using Newtonsoft.Json;
using Nop.Plugin.Api.DTOs.Base;

namespace Nop.Plugin.Api.Models
{
    [JsonObject(Title = "token")]
    public class TokenModel : BaseDto
    {
        [JsonProperty("customer_user_name")]
        public string CustomerUserName { get; set; }
        [JsonProperty("customer_password")]
        public string CustomerPassword { get; set; }
        [JsonProperty("api_user_name")]
        public string ApiUserName { get; set; }
        [JsonProperty("api_password")]
        public string ApiPassword { get; set; }
        [JsonProperty("customer_id")]
        public int CustomerId { get; set; }
        [JsonProperty("customer_guid")]
        public string CustomerGuid { get; set; }
        [JsonProperty("customer_token")]
        public string Token { get; set; }


    }
}
