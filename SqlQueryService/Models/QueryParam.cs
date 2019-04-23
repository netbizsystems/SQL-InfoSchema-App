using System;
using System.Collections.Generic;
using System.Text;

namespace AndersonEnterprise.SqlQueryService.Models
{
    public class QueryParam
    {
        public string ParamName { get; set; }
        public string ParamValue { get; set; }
        public string DbColumnName { get; set; }
    }
}
