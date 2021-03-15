using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Extensions.NETCore.Setup;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Core;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using BalsamicSolutions.ApiCommon.Configuration;

namespace BalsamicSolutions.ApiServer
{
    public class Startup
    {
        private readonly string ConfiguredClientOrigins = "_ConfiguredClientOrigins";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public static IConfiguration Configuration { get; private set; }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            Settings setttings = app.ApplicationServices.GetService<Settings>();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors(ConfiguredClientOrigins);

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Welcome to the Balsamic Solutions Demo Api");
                });
            });
        }

        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
             Settings setttings = new Settings();
            LambdaLogger.Log("AuthorizedClientUrls: " + string.Join(",", setttings.AuthorizedClientUrls));
            LambdaLogger.Log("CognitoPoolUrl: " + string.Join(",", setttings.CognitoPoolUrl));

            services.AddSingleton(sp => setttings);
            services.AddCors(options =>
            {
                options.AddPolicy(name: ConfiguredClientOrigins,
                  builder =>
                  {
                      builder.WithOrigins(setttings.AuthorizedClientUrls);
                      builder.AllowAnyMethod().AllowAnyHeader().AllowCredentials();
                  });
            });
            services.AddAuthorization(
                options =>
                {
                    AuthorizationPolicyBuilder defaultAuthorizationPolicyBuilder = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme);
                    defaultAuthorizationPolicyBuilder = defaultAuthorizationPolicyBuilder.RequireAuthenticatedUser();
                    options.DefaultPolicy = defaultAuthorizationPolicyBuilder.Build();
                    options.AddPolicy("InWeatherCenter", policy => policy.Requirements.Add(new CognitoClaimAuthorizationRequirement(setttings.CognitoWeatherCenterGroupName)));
                });

            services.AddSingleton<IAuthorizationHandler, CognitoClaimAuthorizationHandler>();
            services.AddAuthentication(
                options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                }).AddJwtBearer(o =>
                {
                    o.Authority = setttings.CognitoPoolUrl;
                    o.Audience = setttings.CognitoClientId;

                    o.RequireHttpsMetadata = false;
                    o.MetadataAddress = $"{setttings.CognitoPoolUrl}/.well-known/openid-configuration";
                    o.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        // Because this was done already in the Gateway Authorizer we dont
                        // need to do any "expensive" additional validation
                        ValidateAudience = false,
                        ValidateIssuerSigningKey = false,
                        ValidateTokenReplay = false,
                        ValidateLifetime = false,
                        NameClaimType = setttings.CognitoNameClaim
                    };
                });

            services.AddDefaultAWSOptions(setttings.AwsOptions);
            services.AddAWSService<IAmazonDynamoDB>();
            services.AddTransient<IDynamoDBContext, DynamoDBContext>();
            // Add S3 to the ASP.NET Core dependency injection framework.
            services.AddAWSService<Amazon.S3.IAmazonS3>();

            services.AddControllers();
        }
    }
}
