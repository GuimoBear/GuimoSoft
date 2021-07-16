using GuimoSoft.Bus.Kafka;
using GuimoSoft.Bus.Kafka.Common;
using GuimoSoft.Examples.Bus.Kafka.Handlers.HelloMessage;
using GuimoSoft.Examples.Bus.Kafka.Messages;
using GuimoSoft.Examples.Bus.Kafka.Utils;
using GuimoSoft.Examples.Bus.Kafka.Utils.Serializers;
using GuimoSoft.Logger;
using GuimoSoft.Logger.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;

namespace GuimoSoft.Examples.Bus.Kafka
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

            services.AddApiLogger();

            services.AddOptions<KafkaOptions>()
                .Configure(opt =>
                {
                    opt.KafkaBootstrapServers = Environment.GetEnvironmentVariable("KAFKA_HOSTS");
                    opt.ConsumerGroupId = Environment.GetEnvironmentVariable("KAFKA_CONSUMER_GROUP_ID");
                });

            var wrapper = services
                .AddKafkaProducer()
                .AddKafkaConsumer(typeof(Startup))
                .WithDefaultSerializer(EncryptedJsonSerializer.Instance)
                .WithLogger(prov => new BusLogger(prov.GetRequiredService<IApiLogger<BusLogger>>()));

            wrapper.WithMessageMiddleware<HelloMessage, HelloMessageMiddleware>();

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "GuimoSoft.Examples.Bus.Kafka", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "GuimoSoft.Examples.Bus.Kafka v1"));
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
