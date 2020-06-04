using Nop.Web.Framework.Models;
using System.Collections.Generic;

namespace Nop.Plugin.Api.Models.Catalog
{
    public partial class ManufacturerNavigationModel : BaseNopModel
    {
        public ManufacturerNavigationModel()
        {
            Manufacturers = new List<ManufacturerBriefInfoModel>();
        }

        public IList<ManufacturerBriefInfoModel> Manufacturers { get; set; }

        public int TotalManufacturers { get; set; }
    }

    public partial class ManufacturerBriefInfoModel : BaseNopEntityModel
    {
        public string Name { get; set; }

        public string SeName { get; set; }

        public bool IsActive { get; set; }
    }
}
