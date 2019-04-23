
using AndersonEnterprise.SqlQueryService;
using Microsoft.AspNetCore.Mvc;

namespace SqlSampleWebApp.Controllers.Api
{
    [Route("api/[controller]")]
    public class ProductsController : InfoQueryController
    {
        public ProductsController(IRunTimeService runtimeService) : base(runtimeService) { /* foo? */ }

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
            base.AddSelectParameter( "color", null, "a.color" );

            return base.Get( skip:skip, take:take );
        }
    }
}
