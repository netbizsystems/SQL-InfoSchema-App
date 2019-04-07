namespace AndersonEnterprise.SqlQueryService.Models
{
    public class SqlQuery
    {
        public string SqlQueryStatement { get; set; }
        public int RowsExpectedMax { get; set; }
    }
}
