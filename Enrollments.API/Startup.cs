using Confluent.Kafka;

using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Enrollments.API.Authentication;
using Enrollments.Infrastructure.Interface.Mongo;
using Enrollments.Infrastructure.Mongo;
using Enrollments.Services;
using Enrollments.Services.Interfaces;
using Keycloak.AuthServices.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using Newtonsoft.Json;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Elasticsearch;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.IdentityModel.Tokens.Jwt;

using System.Collections.Generic;
using WLS.KafkaMessenger.Infrastructure.Interface;
using WLS.KafkaMessenger.Infrastructure;
using WLS.KafkaMessenger.Services.Interfaces;
using WLS.KafkaMessenger.Services;

using WLS.Log.LoggerTransactionPattern;
using DarwinAuthorization;

namespace Enrollments.API
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
            services.AddLogging();
            services.AddCors(
                corsOptions => corsOptions.AddPolicy(
                    "AllowAll",
                    builder => {
                        builder.AllowAnyOrigin()
                               .AllowAnyMethod()
                               .AllowAnyHeader();
                    }
                )
            );
            services.AddControllers();
            services.AddApiVersioning();
            services.AddDarwinAuthzConfiguration();
            services.AddSingleton<IKafkaMessengerService, KafkaMessengerService>();

            string host = Environment.GetEnvironmentVariable("KAFKA_HOST");
            var senders = new List<KafkaSender>
            {
                new KafkaSender
                {
                    Topic = Environment.GetEnvironmentVariable("KAFKA_TOPIC")
                }
            };
            services.AddSingleton<IKafkaConfig>(kc =>
                new KafkaConfig() { Host = host, Sender = senders, Source = "darwin-enrollments" }
            );

            services.AddSingleton(p => new ProducerBuilder<string, string>(new ProducerConfig
            {
                BootstrapServers = host
            }).Build());

            services.AddSingleton<KafkaMessageChannel>();
            services.AddSingleton<GenerateKafkaEventsMutex>();
            services.AddScoped<IKafkaService, KafkaService>();
            services.AddHostedService<SendKafkaMessageService>();
            services.AddHostedService<GenerateKafkaEventsService>();

            ConfigureOpenAPI(services);
            ConfigureVersioning(services);
            RegisterServices(services);
            ConfigureLogging(services);
            ConfigureHttpClients(services);

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            services.AddControllers().AddNewtonsoftJson(
                options => {
                    options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                    options.SerializerSettings.DateFormatString = "yyyy-MM-ddTHH:mm:sszzz";
                }
            );
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCors("AllowAll");

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(
                    settings => {
                        settings.SwaggerEndpoint("/swagger/v4/swagger.json", "v4");
                        settings.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
                    }
                );
            }

            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseDarwinAuthenticationContext();

            app.UseEndpoints(
                endpoints => {
                    endpoints.MapControllers();
                }
            );
        }

        private void ConfigureOpenAPI(IServiceCollection services)
        {
            services.AddSwaggerGen(generatorOptions =>
            {
                generatorOptions.SwaggerDoc("v4", new OpenApiInfo
                {
                    Version = "v4",
                    Title = "Enrollments API",
                    Description = "Enrollments API",
                });
                generatorOptions.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = @"JWT Authorization header using the Bearer scheme. \r\n\r\n
                      Enter 'Bearer' [space] and then your token in the text input below.
                      \r\n\r\nExample: 'Bearer 12345abcdef'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
            });
        }

        private void ConfigureVersioning(IServiceCollection services)
        {
            services.AddVersionedApiExplorer(
                explorerOptions => {
                    explorerOptions.GroupNameFormat = "'v'VVV";
                    explorerOptions.SubstituteApiVersionInUrl = true;
                }
            );
            services.AddApiVersioning(
                versioningOptions => {
                    versioningOptions.DefaultApiVersion = new ApiVersion(4, 0);
                    versioningOptions.AssumeDefaultVersionWhenUnspecified = true;
                    versioningOptions.ReportApiVersions = true;
                }
            );
        }

        private static void RegisterServices(IServiceCollection services)
        {
            services.AddScoped<IEnrollmentService, EnrollmentService>();
            services.AddScoped<IEventService, EventService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ITrainingProgramService, TrainingProgramService>();
            services.AddScoped<IOrganizationService, OrganizationService>();
            services.AddScoped<IMongoHealthCheckService, MongoHealthCheckService>();
            services.AddScoped<ILoggerStateFactory, LoggerStateFactory>();
            services.AddScoped(typeof(IPaginationService<>), typeof(PaginationService<>));

            RegisterRepositories(services);
        }

        private static void RegisterRepositories(IServiceCollection services)
        {
            var connString = Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING");
            var dbName = Environment.GetEnvironmentVariable("MONGO_DB_NAME");
            var tlsCAFilePath = Environment.GetEnvironmentVariable("MONGO_TLS_CA_FILE_PATH");

            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("MONGO_TLS_CA_FILE_PATH"))) {
                // ADD CA certificate to local trust store
                // DO this once - Maybe when your service starts
                X509Store localTrustStore = new X509Store(StoreName.Root);
                X509Certificate2Collection certificateCollection = new X509Certificate2Collection();
                certificateCollection.Import(tlsCAFilePath);

                try
                {
                    localTrustStore.Open(OpenFlags.ReadWrite);
                    localTrustStore.AddRange(certificateCollection);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Root certificate import failed: " + ex.Message);
                    throw;
                }
                finally
                {
                    Console.WriteLine("Mongo Root certificate imported sucessfully");
                    localTrustStore.Close();
                }
            }

            services.AddSingleton<IMongoDBSettings>(
                _ => new MongoDBSettings() {
                    ConnectionString = connString,
                    DatabaseName = dbName
                }
            );
            services.AddSingleton<IMongoClient>(_ => new MongoClient(connString));
            services.AddScoped<IConnectionThrottlingPipeline, ConnectionThrottlingPipeline>();
            services.AddScoped<IMongoTestConnection, MongoTestConnection>();
            services.AddScoped(typeof(IMongoRepository<>), typeof(MongoRepository<>));
        }

        private void ConfigureHttpClients(IServiceCollection services)
        {
            services.AddHttpClient();
        }

        private void ConfigureLogging(IServiceCollection services)
        {
            /* Switching to using "Serilog" log provider for everything
                NOTE: Call to ClearProviders() is what turns off the default Console Logging
                Output to the Console is now controlled by the WriteTo format below
                DevOps can control the Log output with environment variables
                    LOG_MINIMUMLEVEL - values like INFORMATION, WARNING, ERROR
                    LOG_JSON - true means to output log to console in JSON format
            */
            LogLevel level = LogLevel.None;
            var serilogLevel = new LoggingLevelSwitch
            {
                MinimumLevel = LogEventLevel.Information
            };

            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("LOG_MINIMUMLEVEL")))
            {
                Enum.TryParse(Environment.GetEnvironmentVariable("LOG_MINIMUMLEVEL"), out level);
                LogEventLevel eventLevel = LogEventLevel.Information;
                Enum.TryParse(Environment.GetEnvironmentVariable("LOG_MINIMUMLEVEL"), out eventLevel);
                serilogLevel.MinimumLevel = eventLevel;
            }

            bool useJson = Environment.GetEnvironmentVariable("LOG_JSON") == "true";

            var config = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .ReadFrom.Configuration(Configuration);

            if (useJson)
                config.WriteTo.Console(new ElasticsearchJsonFormatter());
            else
                config.WriteTo.Console(outputTemplate: "[{Timestamp:MM-dd-yyyy HH:mm:ss.SSS} {Level:u3}] {Message:lj} {TransactionID}{NewLine}{Exception}", theme: SystemConsoleTheme.Literate);

            if (level != LogLevel.None)
                config.MinimumLevel.ControlledBy(serilogLevel);

            Log.Logger = config.CreateLogger();

            services.AddLogging(
                builder => {
                    builder.ClearProviders();
                    builder.AddSerilog();
                    builder.AddDebug(); //Write to VS Output window (controlled by appsettings "Logging" section)
                }
            );
        }
    }
}
