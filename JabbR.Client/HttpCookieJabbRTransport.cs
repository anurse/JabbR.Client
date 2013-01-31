using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client.Http;
using Microsoft.AspNet.SignalR.Client.Hubs;
using Microsoft.AspNet.SignalR.Client.Transports;

namespace JabbR.Client
{
    public class HttpCookieJabbRTransport : IJabbRTransport
    {
        private const string AuthEndpoint = "account/login";
        private const string JabbrCookieName = "jabbr.userToken";
        private const string UserNameParamName = "username";
        private const string PasswordParamName = "password";

        private Uri _url;
        private HttpClient _client;
        private CookieContainer _cookieJar;

        public HttpCookieJabbRTransport(Uri url)
        {
            _url = url;

            _cookieJar = new CookieContainer();
            _client = new HttpClient(new HttpClientHandler()
            {
                UseCookies = true,
                CookieContainer = _cookieJar
            });
        }

        public async Task<HubConnection> Connect(string userName, string password)
        {
            // Post the credentials to the url
            var resp = await _client.PostAsync(_url.AbsoluteUri + AuthEndpoint, new FormUrlEncodedContent(new[] {
                new KeyValuePair<string, string>(UserNameParamName, userName),
                new KeyValuePair<string, string>(PasswordParamName, password)
            }));
            resp.EnsureSuccessStatusCode();
            
            // Verify the cookie
            var cookie = _cookieJar.GetCookies(_url);
            if (cookie == null || cookie[JabbrCookieName] == null)
            {
                throw new SecurityException("Didn't get a cookie from JabbR! Ensure your User Name/Password are correct");
            }

            // Create the transport
            var transport = new AutoTransport(new DefaultHttpClient());

            // Create a hub connection and give it our cookie jar
            return new HubConnection(_url.AbsoluteUri)
            {
                CookieContainer = _cookieJar
            };
        }
    }
}
