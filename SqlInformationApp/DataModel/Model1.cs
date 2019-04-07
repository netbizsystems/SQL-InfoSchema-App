
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace AndersonEnterprise.SqlInformationApp
{

    /// <summary>
    /// 
    /// </summary>
    public partial class Model1 : DbContext
    {
        #region infoschema queries
        public DbQuery<InfoSchemaTable> InfoSchemaTables { get; set; }
        public DbQuery<InfoSchemaTableConstraints> InfoSchemaTableConstraints { get; set; }
        public DbQuery<InfoSchemaReferentialConstraints> InfoSchemaReferentialConstraints { get; set; }
        public DbQuery<InfoSchemaConstraintTableUsage> InfoSchemaConstraintTableUsages { get; set; }
        public DbQuery<InfoSchemaColumns> InfoSchemaColumns { get; set; }
        public DbQuery<InfoSchemaConstraintColumnUsage> InfoSchemaConstraintColumnUsages { get; set; }
        #endregion

        public Model1(DbContextOptions<Model1> options) : base(options) { }
    }

    //https://stackoverflow.com/questions/35631903/raw-sql-query-without-dbset-entity-framework-core

    #region nested infoschema classes
    public class InfoSchemaTable
    {
        public InfoSchemaTable()
        {
            fkList = new List<string>();
            columnList = new List<string>();
            columnListX = new List<string>();
        }
        public string table_name { get; set; }
        public string table_type { get; set; }
        public string table_schema { get; set; }
        [NotMapped]
        public IList<string> fkList { get; set; }
        [NotMapped]
        public IList<string> columnList { get; set; }
        [NotMapped]
        public IList<string> columnListX { get; set; }
        [NotMapped]
        public string pkName { get; set; }
    }
    public class InfoSchemaTableConstraints
    {
        public InfoSchemaTableConstraints()
        {

        }
        public string constraint_name { get; set; }
        public string table_name { get; set; }
        public string constraint_type { get; set; }
        public string table_schema { get; set; }
    }
    public class InfoSchemaReferentialConstraints
    {
        public string constraint_name { get; set; }
        public string unique_constraint_name { get; set; }
    }
    public class InfoSchemaConstraintTableUsage
    {
        public string constraint_name { get; set; }
        public string table_name { get; set; }
    }
    public class InfoSchemaColumns
    {
        public string table_schema { get; set; }
        public string column_name { get; set; }
        public string table_name { get; set; }
    }
    public class InfoSchemaConstraintColumnUsage
    {
        public string constraint_name { get; set; }
        public string table_name { get; set; }
        public string column_name { get; set; }
    }
    #endregion
}
