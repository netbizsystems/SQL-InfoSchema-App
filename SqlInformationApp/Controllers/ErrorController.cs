
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace AndersonEnterprise.SqlInformationApp.Controllers
{
    using AndersonEnterprise.SqlInformationApp.Models;
    using Microsoft.AspNetCore.Diagnostics;

    public class ErrorController : Controller
    {

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                ErrorException = exceptionFeature.Error
            });
        }
    }
}
