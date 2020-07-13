using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Api.Models.Media;
using Nop.Web.Framework.Models;
using System.Collections.Generic;

namespace Nop.Plugin.Api.Models.ShoppingCart
{
    [JsonObject(Title = "Payment")]
    public partial class PaymentModel : BaseNopModel
    {
        public PaymentModel()
        {

        }

        [JsonProperty("Amount")]
        public double Amount { get; set; }

        [JsonProperty("CustomerId")]
        public string CustomerId { get; set; }

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
