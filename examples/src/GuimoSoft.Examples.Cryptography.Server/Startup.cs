using GuimoSoft.Cryptography.AspNetCore;
using GuimoSoft.Examples.Cryptography.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;

namespace GuimoSoft.Examples.Cryptography.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            isDevelopment = env.IsDevelopment();
        }

        private readonly bool isDevelopment;
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            if (isDevelopment)
                EnvFile.CarregarVariaveis();

            services.AddControllers(options =>
            {
                var repository = new EnvironmentVariableRsaParametersRepository("CERT_", Environment.GetEnvironmentVariable("RSA_PASSWORD"));
                Guid.TryParse(Environment.GetEnvironmentVariable("RSA_DEFAULT_CERTIFICATE"), out var defaultCertificateIdentifier);
                options
                    .AddRsaInputContentType(repository)
                    .AddRsaOutputContentType(repository, defaultCertificateIdentifier);
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "GuimoSoft.Examples.Cryptography.Server", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "GuimoSoft.Examples.Cryptography.Server v1"));
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
