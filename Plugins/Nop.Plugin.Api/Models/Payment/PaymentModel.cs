using Newtonsoft.Json;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Api.Models.Payment
{
    [JsonObject(Title = "Payment")]
    public partial class PaymentModel : BaseNopModel
    {
        public PaymentModel()
        {

        }

        [JsonProperty("Amount")]
        public int? Amount { get; set; }

        [JsonProperty("CustomerId")]
        public int CustomerId { get; set; }

        [JsonProperty("StripeToken")] 
        public string StripeToken { get; set; }

        [JsonProperty("Currency")]
        public string Currency { get; set; }

        [JsonProperty("Description")]
        public string Description { get; set; }

        [JsonProperty("ReceiptEmail")]
        public string ReceiptEmail { get; set; }
    }
}
