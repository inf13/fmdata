using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using FMData.Responses;

namespace FMData
{
    /// <summary>
    /// FileMaker Data API Client
    /// </summary>
    public class FMDataClient : IFMDataClient, IDisposable
    {
        private readonly HttpClient _client;
        private readonly string _fmsUri;
        private readonly string _fileName;
        private readonly string _userName;
        private readonly string _password;

        private string dataToken;

        public bool IsAuthenticated => !String.IsNullOrEmpty(dataToken);

        /// <summary>
        /// FM Data Constructor. Injects a new plain old <see ref="HttpClient"/> instance to the class.
        /// </summary>
        /// <param name="fmsUri">FileMaker Server HTTP Uri Endpoint.</param>
        /// <param name="file">Name of the FileMaker Database to connect to.</param>
        /// <param name="user">Account to connect with.</param>
        /// <param name="pass">Account to connect with.</param>
        /// <param name="initialLayout">Layout to use for the initial authentication request.</param>
        public FMDataClient(string fmsUri, string file, string user, string pass, string initialLayout) : this(new HttpClient(), fmsUri, file, user, pass, initialLayout) { }

        /// <summary>
        /// FM Data Constructor. Injects a new plain old <see ref="HttpClient"> instance to the class.
        /// </summary>
        /// <param name="client">An <see ref="HttpClient"/> instance to utilize for the liftime of this Data Client.</param>
        /// <param name="fmsUri">FileMaker Server HTTP Uri Endpoint.</param>
        /// <param name="file">Name of the FileMaker Database to connect to.</param>
        /// <param name="user">Account to connect with.</param>
        /// <param name="pass">Account to connect with.</param>
        /// <param name="initialLayout">Layout to use for the initial authentication request.</param>
        public FMDataClient(HttpClient client, string fmsUri, string file, string user, string pass, string initialLayout)
        {
            _client = client;

            _fmsUri = fmsUri;
            _fileName = file;
            _userName = user;
            _password = pass;

            var authResponse = RefreshTokenAsync(_userName, _password, initialLayout);
            authResponse.Wait();
            if (authResponse.Result.Result == "OK")
            {
                dataToken = authResponse.Result.Token;
            }
        }

        public string AuthEndpoint(string fileName) => $"{_fmsUri}/fmi/rest/api/auth/{fileName}";
        public string FindEndpoint(string fileName, string layout) => $"{_fmsUri}/fmi/rest/api/find/{fileName}/{layout}";

        public async Task<AuthResponse> RefreshTokenAsync(string username, string password, string layout)
        {
            // parameter checks
            if (string.IsNullOrEmpty(username)) throw new ArgumentException("Username is a required parameter.");
            if (string.IsNullOrEmpty(password)) throw new ArgumentException("Password is a required parameter.");
            if (string.IsNullOrEmpty(layout)) throw new ArgumentException("Layout is a required parameter.");

            // build up the request object/content
            var str = $"{{ \"user\": \"{username}\", \"password\" : \"{password}\", \"layout\": \"{layout}\" }}";
            var httpContent = new StringContent(str, Encoding.UTF8, "application/json");
            // run the post action
            var response = await _client.PostAsync(AuthEndpoint(_fileName), httpContent);

            // process the response
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<AuthResponse>(responseJson);
                this.dataToken = responseObject.Token;
                return responseObject;
            }
            // something bad happened. TODO: improve non-OK response handling
            throw new Exception("Could not authenticate.");
        }

        public async Task<BaseDataResponse> LogoutAsync()
        {
            // add a default request header of our data token to nuke
            _client.DefaultRequestHeaders.Add("FM-Data-token", this.dataToken);
            var response = await _client.DeleteAsync(AuthEndpoint(_fileName));

            // process the response
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<BaseDataResponse>(responseJson);
                return responseObject;
            }

            throw new Exception("Could not logout.");
        }

        public async Task<FindResponse> FindAsync(FindRequest req)
        {
            // var req = new FindRequest<T>();
            // req.Query = findParameters;

            if(string.IsNullOrEmpty(req.Layout)) throw new ArgumentException("Layout is required on the find request.");

            var httpContent = new StringContent(req.ToJson(), Encoding.UTF8, "application/json");
            httpContent.Headers.Add("FM-Data-token", this.dataToken);
            var response = await _client.PostAsync(FindEndpoint(_fileName, req.Layout), httpContent);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<FindResponse>(responseJson);
                return responseObject;
            }

            throw new Exception("Find Rquest Error");
        }

        /// <summary>
        /// Dispose resources opened for this instance of the data client.
        /// </summary>
        public void Dispose()
        {
            if (_client != null)
            {
                _client.Dispose();
            }
        }
    }
}