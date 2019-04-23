
using System.Collections.Generic;

namespace AndersonEnterprise.SqlQueryService.Models
{
    public class NamedQuery
    {
        public NamedQuery()
        {
            QueryColumns = new List<string>();
            QueryDataTypes = new List<string>();
        }
        public string QueryId { get; set; }
        public string QueryName { get; set; }
        public string QueryTableBase { get; set; }
        public string QuerySql { get; set; }
        public int QueryRowsExpected { get; set; }
        public IList<string> QueryColumns { get; set; }
        public IList<string> QueryDataTypes{ get; set; }
    }
}
