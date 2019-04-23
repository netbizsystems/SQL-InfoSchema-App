using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace SqlSampleWebApp.Models
{
    public class Products_ViewModel
    {
        [DisplayName("FooBarBaaaaaaz")]
        public System.Int32 ProductID { get; set; }
        public System.String Name { get; set; }
        public System.String ProductNumber { get; set; }
        public System.String Color { get; set; }
        public System.Decimal StandardCost { get; set; }
        public System.Decimal ListPrice { get; set; }
        public System.String Size { get; set; }
        public System.Decimal Weight { get; set; }
        public System.Int32 ProductCategoryID { get; set; }
        public System.Int32 ProductModelID { get; set; }
        public System.DateTime SellStartDate { get; set; }
        public System.DateTime SellEndDate { get; set; }
        public System.DateTime DiscontinuedDate { get; set; }
        public System.Byte[] ThumbNailPhoto { get; set; }
        public System.String ThumbnailPhotoFileName { get; set; }
        public System.Guid rowguid { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public System.Int32 ProductCategoryID_B { get; set; }
        public System.Int32 ParentProductCategoryID { get; set; }
        public System.String Name_B { get; set; }
        public System.Guid rowguid_B { get; set; }
        public System.DateTime ModifiedDate_B { get; set; }
        public System.Int32 ProductModelID_C { get; set; }
        public System.String Name_C { get; set; }
        public System.String CatalogDescription { get; set; }
        public System.Guid rowguid_C { get; set; }
        public System.DateTime ModifiedDate_C { get; set; }
    }
}
