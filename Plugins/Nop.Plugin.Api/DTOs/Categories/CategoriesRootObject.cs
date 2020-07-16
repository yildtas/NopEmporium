using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Nop.Plugin.Api.DTOs.Categories
{
    public class CategoriesRootObject : ISerializableObject
    {
        public CategoriesRootObject()
        {
            Categories = new List<CategoryDto>();
        }

        [JsonProperty("Categories")]
        public IList<CategoryDto> Categories { get; set; }

        public string GetPrimaryPropertyName()
        {
            return "Categories";
        }

        public Type GetPrimaryPropertyType()
        {
            return typeof (CategoryDto);
        }
    }
}