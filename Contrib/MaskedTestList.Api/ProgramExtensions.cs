using Dapr.Client;
using Dapr.Extensions.Configuration;
using Infrastructure.Api;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.OpenApi.Models;
using Polly;
using RecAll.Contrib.MaksedTestList.Api.Filters;
using RecAll.Contrib.MaksedTestList.Api.Services;
using Serilog;
using TheSalLab.GeneralReturnValues;

namespace RecAll.Contrib.MaksedTestList.Api;

public static class ProgramExtensions
{
      public static readonly string AppName = typeof(ProgramExtensions).Namespace;

    public static void AddCustomConfiguration(
        this WebApplicationBuilder builder) {
        builder.Configuration.AddDaprSecretStore("recall-secretstore",
            new DaprClientBuilder().Build());
    }

    public static void AddCustomSerilog(this WebApplicationBuilder builder) {
        var seqServerUrl = builder.Configuration["Serilog:SeqServerUrl"];

        Log.Logger = new LoggerConfiguration().ReadFrom
            .Configuration(builder.Configuration).WriteTo.Console().WriteTo
            .Seq(seqServerUrl).Enrich.WithProperty("ApplicationName", AppName)
            .CreateLogger();

        builder.Host.UseSerilog();
    }

    public static void AddCustomSwagger(this WebApplicationBuilder builder) {
        builder.Services.AddSwaggerGen(options => {
            options.AddSecurityDefinition("oauth2",
                new OpenApiSecurityScheme {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows {
                        Implicit = new OpenApiOAuthFlow {
                            AuthorizationUrl =
                                new Uri(
                                    $"{builder.Configuration["IdentityServer"]}/connect/authorize"),
                            TokenUrl =
                                new Uri(
                                    $"{builder.Configuration["IdentityServer"]}/connect/token"),
                            Scopes = new Dictionary<string, string> {
                                ["MaskedTestList"] = "MaskedTestList",
                            }
                        }
                    }
                });

            options.OperationFilter<AuthorizeCheckOperationFilter>();
        });
    }


    public static void UseCustomSwagger(this WebApplication app) {
        app.UseSwagger();
        app.UseSwaggerUI(options => {
            options.OAuthClientId("MaskedTestListApiSwaggerUI");
            options.OAuthAppName("MaskedTestListApiSwaggerUI");
        });
    }


    public static void
        AddCustomHealthChecks(this WebApplicationBuilder builder) =>
        builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy()).AddDapr()
            .AddSqlServer(
                builder.Configuration["ConnectionStrings:MaskedTestListContext"]!,
                name: "MaskedTestListDb-check", tags: new[] { "MaskedTestListDb" })
            .AddUrlGroup(
                new Uri(builder.Configuration["IdentityServerHealthCheck"]),
                "IdentityServerHealthCheck", tags: new[] { "IdentityServer" });


    public static void AddCustomApplicationServices(
        this WebApplicationBuilder builder) {
        builder.Services.AddScoped<IIdentityService, IdentityService>();
    }

    public static void AddCustomDatabase(this WebApplicationBuilder builder) {
        builder.Services.AddDbContext<MaskedTestListContext>(p =>
            p.UseSqlServer(
                builder.Configuration["ConnectionStrings:MaskedTestListContext"]));
    }

    public static void ApplyDatabaseMigration(this WebApplication app) {
        using var scope = app.Services.CreateScope();

        var retryPolicy = CreateRetryPolicy();
        var context =
            scope.ServiceProvider.GetRequiredService<MaskedTestListContext>();

        retryPolicy.Execute(context.Database.Migrate);
    }
    
    public static void AddCustomIdentityService(
        this WebApplicationBuilder builder) {
        //JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Remove("sub");
        var identityServerUrl = builder.Configuration["IdentityServer"];
        builder.Services.AddAuthentication(options => {
            options.DefaultAuthenticateScheme =
                JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme =
                JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options => {
            options.Authority = identityServerUrl;
            options.RequireHttpsMetadata = false;
            options.Audience = "MaskedTestList";
            options.TokenValidationParameters.ValidateIssuer = false;
            options.TokenValidationParameters.SignatureValidator = (token, _) => new JsonWebToken(token);

        });
    }


    private static Policy CreateRetryPolicy() {
        return Policy.Handle<Exception>().WaitAndRetryForever(
            sleepDurationProvider: _ => TimeSpan.FromSeconds(5),
            onRetry: (exception, retry, _) => {
                Console.WriteLine(
                    "Exception {0} with message {1} detected during database migration (retry attempt {2})",
                    exception.GetType().Name, exception.Message, retry);
            });
    }

    public static void AddInvalidModelStateResponseFactory(
        this WebApplicationBuilder builder) {
        builder.Services.AddOptions().PostConfigure<ApiBehaviorOptions>(
            options => {
                options.InvalidModelStateResponseFactory = context =>
                    new OkObjectResult(ServiceResult
                        .CreateInvalidParameterResult(
                            new ValidationProblemDetails(context.ModelState)
                                .Errors.Select(p =>
                                    $"{p.Key}: {string.Join(" / ", p.Value)}"))
                        .ToServiceResultViewModel());
            });
    }
}