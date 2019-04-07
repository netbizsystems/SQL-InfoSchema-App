
using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Dapper;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace AndersonEnterprise.SqlInformationApp.Controllers
{
    using AndersonEnterprise.SqlInformationApp.Models;

    public class HomeController : Controller
    {
        public HomeController(IConfiguration configuration)
        {
            if (configuration == null) throw new ApplicationException("missing configuration during startup");

            var dc = configuration.GetConnectionString("DefaultConnection");
            var ic = configuration.GetConnectionString("InfoStoreConnection");

            try
            {
                using (var connection = new SqlConnection(dc))
                {
                    connection.Open();
                }

                using (var connection = new SqlConnection(ic))
                {
                    connection.Open();
                    //will throw exception if the table does not exist
                    connection.Query<int>( "SELECT COUNT(*) FROM AES.InfoQueryVersion" ); 
                }
            }
            catch (System.Exception ex)
            {
                // this should get picked up in the ErrorController
                throw new ApplicationException("application cannot start.. missing database(s)", ex);
            }
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
