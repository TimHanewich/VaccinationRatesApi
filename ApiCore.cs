using System.Collections.Generic;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using VaccinationRates;
using System.Web;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text;

namespace VaccinationRatesApi
{
    public static class ApiCore
    {
        
        [Function("Rates")]
        public static async Task<HttpResponseData> Rates([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
        {
            //Get the params
            NameValueCollection nvc = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
            string id = nvc.Get("id");

            //Get the data
            VaccinationAreaData global = await VaccinationAreaData.LoadWorldAsync();

            if (id != null)
            {
                VaccinationAreaData[] all = VaccinationAreaDataToList(global);
                foreach (VaccinationAreaData vad in all)
                {
                    if (vad.Id == id)
                    {
                        HttpResponseData resp = HttpResponseData.CreateResponse(req);
                        resp.StatusCode = HttpStatusCode.OK;
                        resp.Headers.Add("Content-Type", "application/json");
                        resp.WriteString(JsonConvert.SerializeObject(vad), Encoding.UTF8);
                        return resp;
                    }
                }

                //if it got this far, it didnt find it
                HttpResponseData respe = HttpResponseData.CreateResponse(req);
                respe.StatusCode = HttpStatusCode.NotFound;
                respe.WriteString("Unble to find area with ID '" + id + "'");
                respe.Headers.Add("Content-Type", "text/plain");
                return respe;
            }
            else //Return the global data if this is the case
            {
                HttpResponseData resp = HttpResponseData.CreateResponse(req);
                resp.StatusCode = HttpStatusCode.OK;
                resp.Headers.Add("Content-Type", "application/json");
                resp.WriteString(JsonConvert.SerializeObject(global), Encoding.UTF8);
                return resp;
            }

        }

        private static VaccinationAreaData[] VaccinationAreaDataToList(VaccinationAreaData root)
        {
            List<VaccinationAreaData> ToReturn = new List<VaccinationAreaData>();

            //Add self
            ToReturn.Add(root);

            //Add children
            foreach (VaccinationAreaData vad in root.ChildAreas)
            {
                ToReturn.AddRange(VaccinationAreaDataToList(vad));
            }

            return ToReturn.ToArray();
        }

    }
}
