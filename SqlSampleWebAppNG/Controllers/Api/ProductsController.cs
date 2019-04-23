
using AndersonEnterprise.SqlQueryService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace SqlSampleWebAppNG.Controllers.Api
{
    [Route("api/[controller]")]
    public class ProductsController : InfoQueryController
    {
        public ProductsController(IRunTimeService runtimeService) : base(runtimeService) { /* no httpcontext yet */ }

        public override string QueryName => "products";

        /// <summary>
        /// Get - only required if your query has required parameters
        /// </summary>
        /// <param name="queryName"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        public override IActionResult Get([FromQuery]string queryName = "", int skip = 0, int take = -1)
        {
            // default-value of null will cause this parameter to be ignored
            base.AddSelectParameter(  
                queryStringItem: "color", 
                defaultValue: null, 
                dbColumnName: "a.color" 
            );
            return base.Get( queryName: queryName, skip: skip, take:take );
        }
    }
}
