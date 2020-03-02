using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Azurite
{
    /// <summary>
    /// App Startup class. Used by the generic host.
    /// </summary>
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddAzuriteServices()
                .AddVersioning()
                .AddSwashbuckle(Configuration)
                .AddThrottlingServices(Configuration);
            
            services.Configure<Microsoft.AspNetCore.Routing.RouteOptions>(c => c.ConstraintMap.Add("timespan", typeof(Infrastructure.TimeSpanConstraint)));
            services.Configure<AzuriteOptions>(Configuration.GetSection("Azurite"));
            services
                .AddControllers()
                .AddJsonOptions(opts => {
                    opts.JsonSerializerOptions.Converters.Add(new Infrastructure.TimeSpanConverter());
                    opts.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
                })
                .AddXmlSerializerFormatters();
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            // app.UseIpRateLimiting();
            app.UseThrottling();
            app.UseSwashbuckle();

            if (System.IO.Directory.Exists(env.WebRootPath)) {
                logger.LogInformation($"Static file path located at {env.WebRootPath}. Enabling static file serving.");
                app.UseDefaultFiles();
                app.UseStaticFiles();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapVersion("/version");
                endpoints.MapControllers();
            });
        }
    }
}
