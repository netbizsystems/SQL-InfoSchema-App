
using AndersonEnterprise.SqlQueryService;
using Microsoft.AspNetCore.Mvc;

namespace AndersonEnterprise.SqlInformationApp.Controllers.Apis
{
    /// <summary>
    /// FooBars - generalized QueryName data resolver for use in this app
    /// </summary>
    [Route("api/[controller]")] //api/foobars
    public class FooBarsController : InfoQueryController
    {
        public FooBarsController(IRunTimeService runTimeService) : base(runTimeService) { }

        /// <summary>
        /// QueryName - empty tells base-class to get queryname from the querystring
        /// </summary>
        public override string QueryName => string.Empty;

    }
}