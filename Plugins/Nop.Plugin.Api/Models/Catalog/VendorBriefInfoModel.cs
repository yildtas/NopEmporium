using Nop.Web.Framework.Models;
using System.Collections.Generic;

namespace Nop.Plugin.Api.Models.Catalog
{
    public partial class VendorNavigationModel : BaseNopModel
    {
        public VendorNavigationModel()
        {
            Vendors = new List<VendorBriefInfoModel>();
        }

        public IList<VendorBriefInfoModel> Vendors { get; set; }

        public int TotalVendors { get; set; }
    }

    public partial class VendorBriefInfoModel : BaseNopEntityModel
    {
        public string Name { get; set; }

        public string SeName { get; set; }
    }
}
