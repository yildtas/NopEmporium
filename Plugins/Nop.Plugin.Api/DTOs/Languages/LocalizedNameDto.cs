using Newtonsoft.Json;

namespace Nop.Plugin.Api.DTOs.Languages
{
    public class LocalizedNameDto
    {
        /// <summary>
        /// Gets or sets the language identifier
        /// </summary>
        [JsonProperty("LanguageId")]
        public int? LanguageId { get; set; }

        /// <summary>
        /// Gets or sets the localized name
        /// </summary>
        [JsonProperty("LocalizedName")]
        public string LocalizedName { get; set; }
    }
}
