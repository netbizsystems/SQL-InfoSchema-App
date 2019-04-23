using AndersonEnterprise.SqlQueryService.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace SqlQueryServiceConsole
{
    class Program
    {
        /// <summary>
        /// test query api
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            using (var client = new HttpClient())
            {
                // get a list of all queries
                var response1 = client.GetAsync("https://localhost:44315/api/queries").Result;
                List<NamedQuery> jsonResult;
                using (HttpContent content = response1.Content)
                {
                    Task<string> result = content.ReadAsStringAsync();
                    jsonResult = JsonConvert.DeserializeObject<List<NamedQuery>>(value: result.Result);
                }

                // run each query
                foreach (var item in jsonResult)
                {
                    var url = "https://localhost:44315/api/foobars?queryname=" + item.QueryName;
                    var response = client.GetAsync(url).Result;

                    var resultString = String.Empty;
                    using (HttpContent content = response.Content)
                    {
                        // ... Read the string.
                        Task<string> result = content.ReadAsStringAsync();
                        resultString = result.Result;
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            Console.WriteLine(string.Format("Query for {0} resulted in OK.RanToCompletion", item.QueryName));
                        }
                        else
                        {
                            Console.WriteLine(string.Format("Query for {0} failed", item.QueryName));
                        }
                    } 
                }
            }
            Console.ReadKey();
        }
    }
}
