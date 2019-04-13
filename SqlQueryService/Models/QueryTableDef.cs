using System.Collections.Generic;

namespace AndersonEnterprise.SqlQueryService.Models
{
    public class QueryTableDef
    {
        public QueryTableDef()
        {
            IncludeColumns = new List<string>();
            FkTableNames = new List<string>();
            PkColumnNames = new List<string>();

            // FULL JOIN requires a key/value
            JoinOn = "";
        }
        public bool IsBaseTable { get; set; }
        public string TableName { get; set; }
        //public string PkColumnName { get; set; }
        public IList<string> IncludeColumns { get; set; }
        public IList<string> FkTableNames { get; set; }
        public IList<string> PkColumnNames { get; set; }
        public string JoinOn { get; set; }

    }
}