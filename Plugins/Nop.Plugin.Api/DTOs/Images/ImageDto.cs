using Newtonsoft.Json;
using Nop.Plugin.Api.Attributes;

namespace Nop.Plugin.Api.DTOs.Images
{
    [ImageValidation]
    public class ImageDto
    {
        [JsonProperty("Src")]
        public string Src { get; set; }

        [JsonProperty("Attachment")]
        public string Attachment { get; set; }

        [JsonIgnore]
        public byte[] Binary { get; set; }

        [JsonIgnore]
        public string MimeType { get; set; }
    }
}