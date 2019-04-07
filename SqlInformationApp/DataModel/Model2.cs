
using AndersonEnterprise.SqlInformationApp.DataModel.Entities;
using Microsoft.EntityFrameworkCore;

namespace AndersonEnterprise.SqlInformationApp
{
    /// <summary>
    /// 
    /// </summary>
    public partial class Model2 : DbContext
    {
        public virtual DbSet<Table> Tables { get; set; }

        public Model2(DbContextOptions<Model2> options) : base(options) { }
    }
}
