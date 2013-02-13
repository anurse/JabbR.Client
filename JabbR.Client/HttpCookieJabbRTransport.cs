using System;
using System.Net;
using System.Security;
using System.Threading.Tasks;

using Microsoft.AspNet.SignalR.Client.Hubs;

namespace JabbR.Client
{
    public class HttpCookieJabbRTransport : IJabbRTransport
    {
        private const string AuthEndpoint = "account/login";
        private const string JabbrCookieName = "jabbr.userToken";
        private const string UserNameParamName = "username";
        private const string PasswordParamName = "password";

        private Uri _url;
        private CookieContainer _cookieJar;

        public HttpCookieJabbRTransport(Uri url)
        {
            _url = url;

            _cookieJar = new CookieContainer();
        }

        public Task<HubConnection> Connect(string userName, string password)
        {
            var content = String.Format("{0}={1}&{2}={3}", UserNameParamName, Uri.EscapeUriString(userName), PasswordParamName, Uri.EscapeUriString(password));
            var contentBytes = System.Text.Encoding.ASCII.GetBytes(content);

            var request = (HttpWebRequest) WebRequest.Create(_url.AbsoluteUri + AuthEndpoint);
            request.CookieContainer = _cookieJar;
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = contentBytes.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(contentBytes, 0, contentBytes.Length);
            }

            var response = (HttpWebResponse) request.GetResponse();

            HttpStatusCode respStatusCode = response.StatusCode;
            if (respStatusCode < HttpStatusCode.OK || respStatusCode > (HttpStatusCode) 299)
            {
                throw new WebException("The call to the server was not successful.");
            }

            // Verify the cookie
            var cookie = _cookieJar.GetCookies(_url);
            if (cookie == null || cookie[JabbrCookieName] == null)
            {
                throw new SecurityException("Didn't get a cookie from JabbR! Ensure your User Name/Password are correct");
            }

            // Create a hub connection and give it our cookie jar
            var connection = new HubConnection(_url.AbsoluteUri)
            {
                CookieContainer = _cookieJar
            };

            return TaskAsyncHelper.FromResult(connection);
        }
    }
}
