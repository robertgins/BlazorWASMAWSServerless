//  -----------------------------------------------------------------------------
//   Copyright  (c)  Balsamic Solutions, LLC. All rights reserved.
//   THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF  ANY KIND, EITHER
//   EXPRESS OR IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR
//  -----------------------------------------------------------------------------
using BalsamicSolutions.ApiCommon.Configuration;
using BalsamicSolutions.BlazorClient.Modules;
using BalsamicSolutions.BlazorClient.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BalsamicSolutions.BlazorClient
{
    public class Program
    {
        /// <summary>
        /// Blazor startup
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");
            //download appsettings and parse them
            string appSettings = await GetappSettings(builder);
            Settings setttings = new Settings(appSettings);
            //we need Opend ID Connect metadata so load it
            await setttings.LoadOidcMetaDataAsync();
            Settings.Instance = setttings;

            setttings.RedirectUri = $"{builder.HostEnvironment.BaseAddress.TrimEnd('/')}/authentication/login-callback";
            builder.Services.AddSingleton(sp => setttings);
            builder.Services.AddTransient<ApiAuthorizationMessageHandler>();

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(setttings.AuthorizedApiUrl) });

            builder.Services.AddScoped<StuffAndNonsenseService>();
            builder.Services.AddTelerikBlazor();

            builder.Services.AddOidcAuthentication(options =>
            {
                options.UserOptions.NameClaim = setttings.CognitoNameClaim;
                options.ProviderOptions.Authority = setttings.CognitoPoolUrl;
                options.ProviderOptions.MetadataUrl = $"{setttings.CognitoPoolUrl}/.well-known/openid-configuration";
                options.ProviderOptions.ClientId = setttings.CognitoClientId;
                foreach (string scopeName in setttings.CognitoScopes)
                {
                    options.ProviderOptions.DefaultScopes.Add(scopeName);
                }
                options.ProviderOptions.ResponseType = "code";
                options.ProviderOptions.RedirectUri = $"{builder.HostEnvironment.BaseAddress.TrimEnd('/')}/authentication/login-callback";
                options.ProviderOptions.PostLogoutRedirectUri = $"{ builder.HostEnvironment.BaseAddress.TrimEnd('/')}/authentication/logout-callback";
            });

            builder.Services.AddAuthorizationCore();

            builder.Services.AddHttpClient("APIClient", client => client.BaseAddress = new Uri(setttings.AuthorizedApiUrl)).AddHttpMessageHandler<ApiAuthorizationMessageHandler>();

            await builder.Build().RunAsync();
        }

        /// <summary>
        /// Load appsettings.json from the  wwwroot folder
        /// TODO make this an extension
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        private static async Task<string> GetappSettings(WebAssemblyHostBuilder builder)
        {
            // read JSON file as a stream for configuration, mostly an expirement to see
            // if we can pull data into the startup
            var client = new HttpClient() { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
            // the appsettings file must be in 'wwwroot'
            using (var response = await client.GetAsync(Settings.APP_SETTINGS_FILE_NAME))
            {
                string returnValue = await response.Content.ReadAsStringAsync();
                return returnValue;
            }
        }
    }
}