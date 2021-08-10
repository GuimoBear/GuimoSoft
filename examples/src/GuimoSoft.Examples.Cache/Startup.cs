using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Timeout;
using System;
using GuimoSoft.Cache;
using GuimoSoft.Examples.Cache.Utils;
using GuimoSoft.Examples.Cache.ValueObjects;
using GuimoSoft.Logger.AspNetCore;

namespace GuimoSoft.Examples.Cache
{
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
            services.AddApiLogger();

            var policy = Policy
                .Handle<Exception>()
                .RetryAsync(10)
                .WrapAsync(Policy
                    .TimeoutAsync<WeatherForecast>(TimeSpan.FromSeconds(1), TimeoutStrategy.Pessimistic));

            services.AddInMemoryCache<WeatherForecastRequest, WeatherForecast>(configs =>
            {
                configs
                    .WithTTL(TimeSpan.FromSeconds(20))
                    .WithCleaner(TimeSpan.FromMinutes(1), TimeSpan.FromSeconds(2))
                    .WithKeyEqualityComparer(WeatherForecastRequestEqualityComparer.Instance)
                    .ShareValuesBetweenKeys(WeatherForecastEqualityComparer.Instance)
                    .UsingValueFactoryProxy(ValueFactory.Instance.Resilient(policy));
            });

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "GuimoSoft.Examples.Cache", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "GuimoSoft.Examples.Logger v1"));
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
