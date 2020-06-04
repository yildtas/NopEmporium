﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Nop.Plugin.Api.DTOs.ShoppingCarts
{
    public class ShoppingCartItemsRootObject : ISerializableObject
    {
        public ShoppingCartItemsRootObject()
        {
            ShoppingCartItems = new List<ShoppingCartItemDto>();
        }

        [JsonProperty("ShoppingCarts")]
        public IList<ShoppingCartItemDto> ShoppingCartItems { get; set; }

        public string GetPrimaryPropertyName()
        {
            return "ShoppingCarts";
        }

        public Type GetPrimaryPropertyType()
        {
            return typeof (ShoppingCartItemDto);
        }
    }
}