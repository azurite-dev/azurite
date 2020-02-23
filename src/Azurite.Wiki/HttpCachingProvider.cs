using System;
using System.Net.Http;
using Microsoft.Extensions.Caching.InMemory;

namespace Azurite.Wiki
{
    // public class HttpCachingProvider : IHttpMessageHandlerProvider
    // {
    //     public HttpMessageHandler BuildHandler()
    //     {
    //         return new InMemoryCacheHandler(
    //                 new HttpClientHandler {
    //                     AllowAutoRedirect = true
    //                 },
    //                 Microsoft.Extensions.Caching.Abstractions.CacheExpirationProvider.CreateSimple(
    //                     TimeSpan.FromSeconds(120), 
    //                     TimeSpan.FromSeconds(10), 
    //                     TimeSpan.FromSeconds(5)
    //                 )
    //             );
    //     }
    // }

    public class CachedHttpClient : HttpClient
    {
        private static DelegatingHandler GetCachingHandler() {
            return new InMemoryCacheHandler(
                    new HttpClientHandler {
                        AllowAutoRedirect = true
                    },
                    Microsoft.Extensions.Caching.Abstractions.CacheExpirationProvider.CreateSimple(
                        TimeSpan.FromSeconds(120), 
                        TimeSpan.FromSeconds(10), 
                        TimeSpan.FromSeconds(5)
                    )
                );
        }

        public CachedHttpClient() : base(GetCachingHandler())
        {
        }

        public CachedHttpClient(bool disposeHandler) : base(GetCachingHandler(), disposeHandler)
        {
        }
    }
}