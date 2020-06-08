using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Nop.Plugin.Api.DTOs.Base;
using Nop.Plugin.Api.DTOs.Images;
using Nop.Plugin.Api.DTOs.Languages;
using Nop.Plugin.Api.Validators;

namespace Nop.Plugin.Api.DTOs.Categories
{
    [JsonObject(Title = "Category")]
    public class CategoryDto : BaseDto
    {
        private ImageDto _imageDto;
        private List<LocalizedNameDto> _localizedNames;
        private List<int> _storeIds;
        private List<int> _discountIds;
        private List<int> _roleIds;

        [JsonProperty("Name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the localized names
        /// </summary>
        [JsonProperty("LocalizedNames")]
        public List<LocalizedNameDto> LocalizedNames
        {
            get
            {
                return _localizedNames;
            }
            set
            {
                _localizedNames = value;
            }
        }

        /// <summary>
        /// Gets or sets the description
        /// </summary>
        [JsonProperty("Description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value of used category template identifier
        /// </summary>
        [JsonProperty("CategoryTemplateId")]
        public int? CategoryTemplateId { get; set; }

        /// <summary>
        /// Gets or sets the meta keywords
        /// </summary>
        [JsonProperty("MetaKeywords")]
        public string MetaKeywords { get; set; }

        /// <summary>
        /// Gets or sets the meta description
        /// </summary>
        [JsonProperty("MetaDescription")]
        public string MetaDescription { get; set; }

        /// <summary>
        /// Gets or sets the meta title
        /// </summary>
        [JsonProperty("MetaTitle")]
        public string MetaTitle { get; set; }

        /// <summary>
        /// Gets or sets the parent category identifier
        /// </summary>
        [JsonProperty("ParentCategoryId")]
        public int? ParentCategoryId { get; set; }

        /// <summary>
        /// Gets or sets the page size
        /// </summary>
        [JsonProperty("PageSize")]
        public int? PageSize { get; set; }

        /// <summary>
        /// Gets or sets the available customer selectable page size options
        /// </summary>
        [JsonProperty("PageSizeOptions")]
        public string PageSizeOptions { get; set; }

        /// <summary>
        /// Gets or sets the available price ranges
        /// </summary>
        [JsonProperty("PriceRanges")]
        public string PriceRanges { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show the category on home page
        /// </summary>
        [JsonProperty("ShowOnHomePage")]
        public bool? ShowOnHomePage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include this category in the top menu
        /// </summary>
        [JsonProperty("IncludeInTopMenu")]
        public bool? IncludeInTopMenu { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this category has discounts applied
        /// <remarks>The same as if we run category.AppliedDiscounts.Count > 0
        /// We use this property for performance optimization:
        /// if this property is set to false, then we do not need to load Applied Discounts navigation property
        /// </remarks>
        /// </summary>
        [JsonProperty("HasDiscountsApplied")]
        public bool? HasDiscountsApplied { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is published
        /// </summary>
        [JsonProperty("Published")]
        public bool? Published { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity has been deleted
        /// </summary>
        [JsonProperty("Deleted")]
        public bool? Deleted { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        [JsonProperty("DisplayOrder")]
        public int? DisplayOrder { get; set; }

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

        [JsonProperty("RoleIds")]
        public List<int> RoleIds
        {
            get
            {
                return _roleIds;
            }
            set
            {
                _roleIds = value;
            }
        }

        [JsonProperty("DiscountIds")]
        public List<int> DiscountIds
        {
            get
            {
                return _discountIds;
            }
            set
            {
                _discountIds = value;
            }
        }

        [JsonProperty("StoreIds")]
        public List<int> StoreIds
        {
            get
            {
                return _storeIds;
            }
            set
            {
                _storeIds = value;
            }
        }

        [JsonProperty("Image")]
        public ImageDto Image {
            get
            {
                return _imageDto;
            }
            set
            {
                _imageDto = value;
            }
        }

        [JsonProperty("SeName")]
        public string SeName { get; set; }
    }
}