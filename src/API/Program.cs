using Infrastructure.Extensions;
using Infrastructure.Interfaces;
using API.Filters;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Application.Extensions;
using Domain.Utils;
using StackExchange.Redis;

Env.Load();
Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();

builder.Services.AddControllers();

builder.Services.AddCaseControllers();

builder.Services.AddServicesControllers();

builder.Services.AddInfrastructure(Env.GetString("ConnexionDB"), Env.GetString("ConnexionRedis"));

builder.Services.AddScoped<AuthorizeAuth>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "web_site_Front", configurePolicy: policyBuilder =>
    {
        policyBuilder.WithOrigins(Env.GetString("IP_NOW_FRONTEND"));
        policyBuilder.WithHeaders("Content-Type");
        policyBuilder.WithMethods("GET", "POST", "PATCH");
        policyBuilder.AllowCredentials();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie()
.AddGoogle(options =>
{
    options.ClientId = Env.GetString("ClientId");
    options.ClientSecret = Env.GetString("ClientSecret");
    options.Events = new Microsoft.AspNetCore.Authentication.OAuth.OAuthEvents
    {
        OnRemoteFailure = context =>
        {
            context.Response.Redirect(Env.GetString("IP_NOW_FRONTEND") + "/login");
            context.HandleResponse();
            return Task.CompletedTask;
        }
    };
});

builder.Services.Configure<SecretEnv>(options =>
{
    options.Ip_Now_Frontend = Env.GetString("IP_NOW_FRONTEND");
    options.Ip_Now_Backend = Env.GetString("IP_NOW_BACKENDAPI");
    options.SecretKeyJWT = Env.GetString("SecretKeyJWT");
    options.EMAIL_MDP = Env.GetString("EMAIL_MDP");
    options.EMAIL_USER = Env.GetString("EMAIL_USER");
    options.PORT_SMTP = Env.GetString("PORT_SMTP");
    options.SERVICE = Env.GetString("SERVICE");
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dataContext = scope.ServiceProvider.GetRequiredService<IBackendDbContext>();
    dataContext.Migrate();

    var cacheDB = ConnectionMultiplexer.Connect(Env.GetString("ConnexionRedis"));
    if (!cacheDB.IsConnected)
    {
        Console.WriteLine("Failed to connect to CacheDB, Exiting API :/");
        Environment.Exit(1);
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    using var scope = app.Services.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<IUserSeeder>();
    await seeder.Seed();
}

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.UseCors("web_site_Front");

await app.RunAsync();