using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace WeatherApp.Helper
{
    public class ApiCaller
    {
        public static async Task<ApiResponse> Get(string url, string authId = null)
        {
            using (var client = new HttpClient())
            {
                // als authid niet bekend is passeer door
                if (!string.IsNullOrWhiteSpace(authId))
                    client.DefaultRequestHeaders.Add("Authorization", authId);

                var request = await client.GetAsync(url);
                if (request.IsSuccessStatusCode)
                {
                    //succesvolle response
                    return new ApiResponse { Response = await request.Content.ReadAsStringAsync() };
                }
                else
                    //onsuccesvolle response, geef reden
                    return new ApiResponse { ErrorMessage = request.ReasonPhrase };
            }
        }

    }

    public class ApiResponse
    {
        
        // als errormessage == nul dan is de Api call succesvol
        public bool Successful => ErrorMessage == null;
        public string ErrorMessage { get; set; }
        public string Response { get; set; }
    }
}
