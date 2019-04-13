
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace AndersonEnterprise.SqlInformationApp.Controllers.Apis
{
    /// <summary>
    /// Get meta-data from the INFORMATION_SCHEMA views
    /// </summary>
    [Route("api/[controller]")]
    public class TableSchemasController : Controller
    {
        private Model1 Context;
        public TableSchemasController(Model1 context)
        {
            Context = context; // InfoSchemaTables
        }

        [HttpGet] // GET: api/InfoSchemaTables
        public IActionResult Get()
        {
            var resourceModel = new List<InfoSchemaTable>();
            resourceModel = GetInfoTablesData();

            var tableConstraints = new List<InfoSchemaTableConstraints>();
            tableConstraints = GetInfoTableConstraintsData();

            var columnConstraints = new List<InfoSchemaConstraintColumnUsage>();
            columnConstraints = GetInfoColumnConstraintsData();

            var referentialConstraints = new List<InfoSchemaReferentialConstraints>();
            referentialConstraints = GetInfoReferentialConstraintsData();

            var constraintUsages = new List<InfoSchemaConstraintTableUsage>();
            constraintUsages = GetInfoConstraintUsagesData();

            var tableColumns = new List<InfoSchemaColumns>();
            tableColumns = GetInfoColumnsData();

            foreach (var infoTable in resourceModel)
            {
                var pks = columnConstraints.Where(x => x.table_name == infoTable.table_name && x.constraint_name.StartsWith("PK_")).ToList();
                if (pks != null && pks.Count > 0)
                {
                    infoTable.pkNames.AddRange(pks.Select(x => x.column_name));
                }

                PopulateFkList( infoTable, 
                    tableConstraints.Where(x => x.constraint_type == "FOREIGN KEY" && x.table_name == infoTable.table_name).ToList(),
                    referentialConstraints,
                    constraintUsages);

                PopulateColumnList(infoTable, tableColumns.Where(x => x.table_name == infoTable.table_name).ToList());
            }

            return Ok(resourceModel.OrderBy(o => o.table_schema).ThenBy(o => o.table_name));
        }

        #region private/protected
        protected List<InfoSchemaTable> GetInfoTablesData()
        {
            return Context.InfoSchemaTables.FromSql("SELECT * FROM INFORMATION_SCHEMA.TABLES").ToList();
        }
        protected List<InfoSchemaTableConstraints> GetInfoTableConstraintsData()
        {
            return Context.InfoSchemaTableConstraints.FromSql("SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS").ToList();
        }
        protected List<InfoSchemaReferentialConstraints> GetInfoReferentialConstraintsData()
        {
            return Context.InfoSchemaReferentialConstraints.FromSql("SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS").ToList();
        }
        protected List<InfoSchemaConstraintTableUsage> GetInfoConstraintUsagesData()
        {
            return Context.InfoSchemaConstraintTableUsages.FromSql("SELECT * FROM INFORMATION_SCHEMA.CONSTRAINT_TABLE_USAGE").ToList();
        }
        protected List<InfoSchemaColumns> GetInfoColumnsData()
        {
            return Context.InfoSchemaColumns.FromSql("SELECT * FROM INFORMATION_SCHEMA.COLUMNS").ToList();
        }
        protected List<InfoSchemaConstraintColumnUsage> GetInfoColumnConstraintsData()
        {
            return Context.InfoSchemaConstraintColumnUsages.FromSql("SELECT * FROM INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE").ToList();
        }

        protected void PopulateFkList(InfoSchemaTable infoTable, List<InfoSchemaTableConstraints> constraintsModel, List<InfoSchemaReferentialConstraints> referentialConstraints, List<InfoSchemaConstraintTableUsage> constraintUsages)
        {
            foreach (var infoTableConstraint in constraintsModel)
            {
                var refConstraint = referentialConstraints.Where(x => x.constraint_name == infoTableConstraint.constraint_name).FirstOrDefault();
                var constraintUsage = constraintUsages.Where(x => x.constraint_name == refConstraint.unique_constraint_name).FirstOrDefault();

                // load infoTable with FK table names
                infoTable.fkList.Add(infoTable.table_schema + "." + constraintUsage.table_name);
            }
        }
        protected void PopulateColumnList(InfoSchemaTable infoTable, List<InfoSchemaColumns> tableColumns)
        {
            foreach (var tableColumn in tableColumns)
            {
                infoTable.columnList.Add(tableColumn.column_name);
            }
        }
        #endregion
    }
}