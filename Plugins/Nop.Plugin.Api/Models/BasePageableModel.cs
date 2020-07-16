using Newtonsoft.Json;
using Nop.Core;
using Nop.Plugin.Api.DTOs.Base;
using Nop.Web.Framework.UI.Paging;
using System;

namespace Nop.Plugin.Api.Models
{
    /// <summary>
    /// Base class for pageable models
    /// </summary>
    public abstract class BasePageableModel : BaseDto, IPageableModel
    {
        #region Methods

        /// <summary>
        /// Ctor
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="pagedList">Entities (models)</param>
        public virtual void LoadPagedList<T>(IPagedList<T> pagedList)
        {
            FirstItem = (pagedList.PageIndex * pagedList.PageSize) + 1;
            HasNextPage = pagedList.HasNextPage;
            HasPreviousPage = pagedList.HasPreviousPage;
            LastItem = Math.Min(pagedList.TotalCount, ((pagedList.PageIndex * pagedList.PageSize) + pagedList.PageSize));
            PageNumber = pagedList.PageIndex + 1;
            PageSize = pagedList.PageSize;
            TotalItems = pagedList.TotalCount;
            TotalPages = pagedList.TotalPages;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The current page index (starts from 0)
        /// </summary>
        public int PageIndex
        {
            get
            {
                if (PageNumber > 0)
                    return PageNumber - 1;

                return 0;
            }
        }

        /// <summary>
        /// The current page number (starts from 1)
        /// </summary>
        [JsonProperty("PageNumber")]
        public int PageNumber { get; set; }

        /// <summary>
        /// The number of items in each page.
        /// </summary>
        [JsonProperty("PageSize")]
        public int PageSize { get; set; }

        /// <summary>
        /// The total number of items.
        /// </summary>
        [JsonProperty("TotalItems")]
        public int TotalItems { get; set; }

        /// <summary>
        /// The total number of pages.
        /// </summary>
        [JsonProperty("TotalPages")]
        public int TotalPages { get; set; }

        /// <summary>
        /// The index of the first item in the page.
        /// </summary>
        [JsonProperty("FirstItem")]
        public int FirstItem { get; set; }

        /// <summary>
        /// The index of the last item in the page.
        /// </summary>
        [JsonProperty("LastItem")]
        public int LastItem { get; set; }

        /// <summary>
        /// Whether there are pages before the current page.
        /// </summary>
        [JsonProperty("HasPreviousPage")]
        public bool HasPreviousPage { get; set; }

        /// <summary>
        /// Whether there are pages after the current page.
        /// </summary>
        [JsonProperty("HasNextPage")]
        public bool HasNextPage { get; set; }

        #endregion
    }
}
