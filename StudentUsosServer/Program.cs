using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.EntityFrameworkCore;
using StudentUsosServer;
using StudentUsosServer.Configuration;
using StudentUsosServer.Conventions;
using StudentUsosServer.Database;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers((options) =>
{
    options.Conventions.Add(new EndpointPrefixConvention("api"));
});

builder.Services.AddDbContext<MainDBContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("MainLocalDB")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(setup =>
{
    setup.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"));
});
builder.Services.ConfigureOptions<SwaggerConfigureOptions>();

builder.ConfigureVersioning();

builder.RegisterServices();

await builder.InitializeConstants();

var app = builder.Build();

app.ConfigureSwagger();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

await app.MigrateDatabase();

string serviceAccountJsonPath = $"Resources/{Secrets.Default.FirebaseServiceAccountJsonFileName}";
FirebaseApp.Create(new AppOptions()
{
    Credential = GoogleCredential.FromFile(serviceAccountJsonPath)
});

//USOS API send POST requests to this endpoint with undefined content type which leads to issues
//this middleware is used to enable reading the body multiple times since the only stable
//way the endpoint works is by reading Request.Body manually
app.Use(async (context, next) =>
{
    if (context.Request.Path.ToString().ToLower().Contains("/usosnotificationhub/receive"))
    {
        context.Request.EnableBuffering();
    }
    await next.Invoke();
});

app.Run();
