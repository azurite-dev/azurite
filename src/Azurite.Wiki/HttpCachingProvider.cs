using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Caching.Abstractions;
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
        public static HttpMessageHandler GetCachingHandler(IServiceProvider provider) {
            return GetCachingHandler(TimeSpan.FromHours(24).TotalSeconds);
        }
        private static DelegatingHandler GetCachingHandler(double successExpiration = 120) {
            return GetCachingHandler(CacheExpirationProvider.CreateSimple(
                        TimeSpan.FromSeconds(successExpiration), 
                        TimeSpan.FromSeconds(10), 
                        TimeSpan.FromSeconds(5)
                    ));
        }

        private static DelegatingHandler GetCachingHandler(IDictionary<HttpStatusCode, TimeSpan> provider) {
            return new InMemoryCacheHandler(
                    new HttpClientHandler {
                        AllowAutoRedirect = true
                    },
                    provider
                );
        }

        public CachedHttpClient() : base(GetCachingHandler())
        {
        }

        public CachedHttpClient(TimeSpan cacheExpiration) : base(GetCachingHandler(cacheExpiration.TotalSeconds))
        {
            
        }

        public CachedHttpClient(bool disposeHandler) : base(GetCachingHandler(), disposeHandler)
        {
        }
    }
}