using System;
using System.Net.Http;
using System.Threading.Tasks;
using Azurite.Wiki.Diagnostics;

namespace Azurite.Wiki
{
    public abstract class WikiClientBase
    {
        private string _mobileUserAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 11_0 like Mac OS X) AppleWebKit/604.1.38 (KHTML, like Gecko) Version/11.0 Mobile/15A372 Safari/604.1"; // iPhone X
        private string _desktopUserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:61.0) Gecko/20100101 Firefox/61.0"; //Firefox 61 on Win7
        protected readonly string _overrideUrl;
        protected HttpClient httpClient {get;} = BuildHttpClient();

        public bool IsLocal => false;

        public bool UseMobile { set {
            if (value) {
                // httpClient.DefaultRequestHeaders.UserAgent.Remove()
            }
        }}

        protected static HttpClient BuildHttpClient() {
            return new CachedHttpClient();
        }

        protected WikiClientBase()
        {
        }

        protected WikiClientBase(HttpClient client) : this()
        {
            httpClient = client;
        }

        protected WikiClientBase(string baseUrl)
        {
            _overrideUrl = baseUrl;
        }

        protected async Task<string> GetHtml(Uri url)
        {
            try {
                // if (url.PathAndQuery.Contains("List")) {
                //     httpClient.DefaultRequestHeaders.UserAgent.ParseAdd($"Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:61.0) Gecko/20100101 Firefox/61.0 Azurite/{typeof(WikiSearcher).Assembly.GetName().Version.ToString()}");
                //     // httpClient.DefaultRequestHeaders.UserAgent.ParseAdd($"Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:61.0) Gecko/20100101 Firefox/61.0");
                // } else {
                // httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (iPhone; CPU iPhone OS 11_0 like Mac OS X) AppleWebKit/604.1.38 (KHTML, like Gecko) Version/11.0 Mobile/15A372 Safari/604.1"); // iPhone X
                // }
                var req = new HttpRequestMessage(HttpMethod.Get, url.AbsoluteUri);
                if (url.PathAndQuery.Contains("List")) {
                    req.Headers.UserAgent.ParseAdd(_desktopUserAgent);
                } else {
                    req.Headers.UserAgent.ParseAdd(_mobileUserAgent);
                }
                var resp = await httpClient.SendAsync(req);
                resp.EnsureSuccessStatusCode();
                return await resp.Content.ReadAsStringAsync();
                // var html = await httpClient.GetStringAsync(url.AbsoluteUri);
                // return html;
            }
            catch (Exception e) {
                throw new FetchPageException($"Failed to fetch HTML from request to {url}", e);
            }
        }
    }
}