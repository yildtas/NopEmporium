using System;
using Newtonsoft.Json;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Api.DTOs.Customers;
using Nop.Plugin.Api.DTOs.Products;
using Nop.Plugin.Api.Validators;
using System.Collections.Generic;
using Nop.Plugin.Api.DTOs.Base;

namespace Nop.Plugin.Api.DTOs.ShoppingCarts
{
    [JsonObject(Title = "ShoppingCartItem")]
    public class ShoppingCartItemDto : BaseDto
    {
        private int? _shoppingCartTypeId;
        private List<ProductItemAttributeDto> _attributes;

        /// <summary>
        /// Gets or sets the selected attributes
        /// </summary>
        [JsonProperty("ProductAttributes")]
        public List<ProductItemAttributeDto> Attributes
        {
            get
            {
                return _attributes;
            }
            set
            {
                _attributes = value;
            }
        }

        /// <summary>
        /// Gets or sets the price enter by a customer
        /// </summary>
        [JsonProperty("CustomerEnteredPrice")]
        public decimal? CustomerEnteredPrice { get; set; }

        /// <summary>
        /// Gets or sets the quantity
        /// </summary>
        [JsonProperty("Quantity")]
        public int? Quantity { get; set; }

        /// <summary>
        /// Gets or sets the rental product start date (null if it's not a rental product)
        /// </summary>
        [JsonProperty("RentalStartDateUtc")]
        public DateTime? RentalStartDateUtc { get; set; }

        /// <summary>
        /// Gets or sets the rental product end date (null if it's not a rental product)
        /// </summary>
        [JsonProperty("RentalEndDateUtc")]
        public DateTime? RentalEndDateUtc { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance creation
        /// </summary>
        [JsonProperty("CreatedOnUtc")]
        public DateTime? CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance update
        /// </summary>
        [JsonProperty("UpdatedOnUtc")]
        public DateTime? UpdatedOnUtc { get; set; }

        /// <summary>
        /// Gets the log type
        /// </summary>
        [JsonProperty("ShoppingCartType")]
        public string ShoppingCartType
        {
            get
            {
                var shoppingCartTypeId = _shoppingCartTypeId;

                if (shoppingCartTypeId != null) return ((ShoppingCartType)shoppingCartTypeId).ToString();

                return null;
            }
            set
            {
                ShoppingCartType shoppingCartType;
                if (Enum.TryParse(value, true, out shoppingCartType))
                {
                    _shoppingCartTypeId = (int)shoppingCartType;
                }
                else _shoppingCartTypeId = null;
            }
        }

        [JsonProperty("ProductId")]
        public int? ProductId { get; set; }

        /// <summary>
        /// Gets or sets the product
        /// </summary>
        [JsonProperty("Product")]
        public ProductDto ProductDto { get; set; }

        [JsonProperty("CustomerId")]
        public int? CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the customer
        /// </summary>
        [JsonProperty("Customer")]
        public CustomerForShoppingCartItemDto CustomerDto { get; set; }
    }
}