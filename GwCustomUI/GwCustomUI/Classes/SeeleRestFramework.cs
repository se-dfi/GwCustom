using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace GwCustomUI.Classes {
    /// <summary>
    /// seele REST Framework und SERA
    /// </summary>
    public static class SeeleRestFramework {
        /// <summary>
        /// URL und Timeout zum seele rest framework
        /// </summary>
        private static readonly RestClientOptions restoptions = new(CommonUtil.GetConfig().RestUrlCustom) {
            Timeout = TimeSpan.FromMilliseconds(CommonUtil.GetConfig().RestTimeout)
        };

        /// <summary>
        /// RestSharp Client instanzieren
        /// </summary>
        /// <returns>RestSharp Client</returns>
        private static RestClient GetRestClient() {
            return new RestClient(restoptions);
        }

        /// <summary>
        /// Signatur- und Layoutdatenbank abholen und speichern
        /// </summary>
        /// <param name="target">Zielpfad für die Datenbankdatei</param>
        /// <returns>Datenbankdatei existiert</returns>
        public static async Task<bool> GetDatabase(string target, List<string> users) {
            RestClient client = GetRestClient();

            RestRequest request = new("api/signatur", Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddJsonBody(users);

            RestResponse response;
            try {
                response = await client.PostAsync(request).ConfigureAwait(false);

                if (response.StatusCode.Equals(HttpStatusCode.OK) && response.RawBytes != null) {
                    await File.WriteAllBytesAsync(target, response.RawBytes).ConfigureAwait(false);
                }
            } catch (HttpRequestException) {
                return false;
            }

            return File.Exists(target);
        }
    }
}