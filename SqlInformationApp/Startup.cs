
using System.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Data.SqlClient;
using Dapper;

namespace AndersonEnterprise.SqlInformationApp
{
    using AndersonEnterprise.SqlInformationApp.Controllers.Apis;
    using AndersonEnterprise.SqlQueryService;

    public class Startup
    {
        private IConfiguration Configuration { get; }
        private string DefaultConnectionString { get; }
        private string InfoConnectionString { get; }
        private object StartupException { get; }

        /// <summary>
        /// constructor - make sure the connectionstrings actually work!!
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration)
        {
            if (configuration != null)
            {
                Configuration = configuration;

                DefaultConnectionString = Configuration.GetConnectionString("DefaultConnection");
                InfoConnectionString = Configuration.GetConnectionString("InfoStoreConnection");

                try
                {
                    using (var connection = new SqlConnection(DefaultConnectionString))
                    {
                        connection.Open();
                    }

                    using (var connection = new SqlConnection(InfoConnectionString))
                    {
                        connection.Open();
                        //will throw exception if the table does not exist
                        connection.Query<int>("SELECT COUNT(*) FROM AES.InfoQueryVersion");
                    }
                }
                catch (System.Exception ex)
                {
                    StartupException = ex; // see home controller for the rest of the story!!
                }
            }
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddSingleton<IDbConnection>(new SqlConnection(DefaultConnectionString));
            services.AddSingleton<IInfoQueryService>( new InfoQueryService(Configuration) );
            services.AddSingleton<FooBarsController>(new FooBarsController(Configuration));
            services.AddDbContext<Model1> (options => options.UseSqlServer(DefaultConnectionString));
            services.AddDbContext<Model2>(options => options.UseSqlServer(InfoConnectionString));
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error/Error");                
                app.UseHsts(); // see https://aka.ms/aspnetcore-hsts 
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
