using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Api.Constants;
using Nop.Plugin.Api.Models.ShoppingCart;
using System;
using System.Collections.Generic;

namespace Nop.Plugin.Api.Services
{
    public interface IShoppingCartItemApiService
    {
        List<ShoppingCartItem> GetShoppingCartItems(int? customerId = null, DateTime? createdAtMin = null, DateTime? createdAtMax = null,
                                                    DateTime? updatedAtMin = null, DateTime? updatedAtMax = null, int limit = Configurations.DefaultLimit, 
                                                    int page = Configurations.DefaultPageValue);

        ShoppingCartItem GetShoppingCartItem(int id);

        IList<ShoppingCartItem> GetShoppingCart(int customerId,
             int storeId = 0, int? productId = null, DateTime? createdFromUtc = null, DateTime? createdToUtc = null);

        ShoppingCartModel PrepareShoppingCartModel(int customerId, ShoppingCartModel model,
            IList<ShoppingCartItem> cart, bool isEditable = true, bool validateCheckoutAttributes = false);

        OrderTotalsModel PrepareOrderTotalsModel(int customerId,IList<ShoppingCartItem> cart, bool isEditable);

        IList<string> UpdateShoppingCartItem(Customer customer,
           int shoppingCartItemId, string attributesXml,
           decimal customerEnteredPrice, int quantity = 1,
           DateTime? rentalStartDate = null, DateTime? rentalEndDate = null,
           bool resetCheckoutData = true);
    }
}