using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using StudentUsosServer.Features.CampusMap.Repositories;
using StudentUsosServer.Services;
using StudentUsosServer.Services.Interfaces;

namespace StudentUsosServer
{
    public static class AppBuilderExtensions
    {

        public static async Task<WebApplicationBuilder> InitializeConstants(this WebApplicationBuilder builder)
        {
            await Secrets.Initialize();
            return builder;
        }

        public static WebApplicationBuilder RegisterServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<IPushNotificationsService, PushNotificationsService>();
            builder.Services.AddSingleton<UsosInstallationsService>();
            builder.Services.AddScoped<IUsosPushNotificationsService, UsosPushNotificationsService>();
            builder.Services.AddSingleton<IUsosAuthorizationService, UsosAuthorizationService>();
            builder.Services.AddSingleton<IUsosApiService, UsosApiService>();
            return builder;
        }

        public static WebApplicationBuilder RegisterRepositories(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<ICampusMapRepository, CampusMapRepository>();
            return builder;
        }

        public static WebApplicationBuilder ConfigureVersioning(this WebApplicationBuilder builder)
        {
            builder.Services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1);
                options.ReportApiVersions = true;
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddMvc()
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'V";
                options.SubstituteApiVersionInUrl = true;
            });
            return builder;
        }

        public static WebApplication ConfigureSwagger(this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    var apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
                    foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
                    {
                        options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
                    }
                });
            }
            return app;
        }
    }
}
