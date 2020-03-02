using System;
using System.Collections.Generic;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Azurite
{
    public static class EndpointExtensions {
        public static IEndpointConventionBuilder MapVersion(this IEndpointRouteBuilder endpoints, string pattern) {
            var pipeline = endpoints.CreateApplicationBuilder()
                .UseMiddleware<Infrastructure.VersionMiddleware>()
                .Build();
            return endpoints.Map(pattern, pipeline).WithDisplayName("Version number");
        }
    }
    public static class StartupExtensions {
        public static IServiceCollection AddAzuriteServices(this IServiceCollection services) {
            services.AddSingleton<Wiki.WikiSearcher>();
            services.AddSingleton<Index.ShipDbClient>();
            services.AddSingleton<Index.IndexBuilder>();
            services.AddSingleton<Index.IndexedDataProvider>();
            services.AddHttpClient("cached").ConfigurePrimaryHttpMessageHandler(Wiki.CachedHttpClient.GetCachingHandler);
            services.AddSingleton<IShipDataProvider>(GetCacheDataProvider);
            return services;
        }

        private static IShipDataProvider GetCacheDataProvider(IServiceProvider services) {
            var logger = services.GetService<ILogger<IShipDataProvider>>();
            var opts = services.GetService<IOptions<AzuriteOptions>>();
            if (opts.Value.AllowPassthrough) {
                if (System.IO.File.Exists(System.IO.Path.Combine(Environment.CurrentDirectory, "ships.db"))) {
                    return services.GetService<Index.IndexedDataProvider>();
                } else {
                    logger.LogError(412, "Local index was not found! Enabled fallback with HTTP cache. This is not recommended!");
                    logger.LogInformation(412, "Invoke POST /api/v1/index to build the local index and restart the app to enable using the index.");
                    return new Wiki.WikiSearcher(services.GetRequiredService< System.Net.Http.IHttpClientFactory>().CreateClient("cached"));
                }
            } else {
                logger.LogInformation("Passthrough not enabled. Requests will fail if index has not been built!");
                return services.GetRequiredService<Index.IndexedDataProvider>();
            }
        }

        public static IServiceCollection AddThrottlingServices(this IServiceCollection services, IConfiguration config) {
            services.AddMemoryCache();
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
            services.Configure<IpRateLimitOptions>(config.GetSection("RateLimiting"));

            return services;
        }

        public static IServiceCollection AddSwashbuckle(this IServiceCollection services, IConfiguration config) {
            services.AddSwaggerGen(c => {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { 
                    Title = "Azurite", 
                    Version = "v1",
                    Description = "A simple API for Azur Lane ship data",
                    License = new Microsoft.OpenApi.Models.OpenApiLicense {
                        Name = "MIT"
                    }
                });
                c.IgnoreObsoleteActions();
                c.IgnoreObsoleteProperties();
                c.MapType<System.TimeSpan>(() => new Microsoft.OpenApi.Models.OpenApiSchema { Type = "string"});
                foreach (var file in System.IO.Directory.GetFiles(System.AppContext.BaseDirectory, "Azurite*.xml"))
                {
                    c.IncludeXmlComments(file);
                }
            });
            return services;
        }

        public static IServiceCollection AddVersioning(this IServiceCollection services) {
            services.AddVersionedApiExplorer(o => {
                o.GroupNameFormat = "'v'VVV";
                o.SubstituteApiVersionInUrl = true;
            });
            services.AddApiVersioning();
            return services;
        }

        public static IApplicationBuilder UseSwashbuckle(this IApplicationBuilder app) {
            app.UseSwagger(o => {
                o.RouteTemplate = "/api/{documentName}/swagger.json";
            }).UseSwaggerUI(c => {
                c.RoutePrefix = "api/help";
                c.SwaggerEndpoint("/api/v1/swagger.json", "Azurite v1");
            });
            return app;
        }

        public static IApplicationBuilder UseThrottling(this IApplicationBuilder app, bool whitelistSwagger = true) {
            var opts = app.ApplicationServices.GetService<IOptions<IpRateLimitOptions>>();
            opts.Value.EnableEndpointRateLimiting = true;
            if (opts.Value.EndpointWhitelist == null) {
                opts.Value.EndpointWhitelist = new List<string>();
            }
            if (whitelistSwagger) {
                var swagger = app.ApplicationServices.GetService<Swashbuckle.AspNetCore.SwaggerGen.SwaggerGeneratorOptions>();
                foreach (var doc in swagger.SwaggerDocs)
                {
                    opts.Value.EndpointWhitelist.Add($"get:/api/{doc.Key}/swagger.json");
                }
                opts.Value.EndpointWhitelist.Add("get:/api/help*");
            }
            app.UseIpRateLimiting();
            return app;
        }
    }
}
