//  -----------------------------------------------------------------------------
//   Copyright  (c)  Balsamic Solutions, LLC. All rights reserved.
//   THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF  ANY KIND, EITHER
//   EXPRESS OR IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR
//  -----------------------------------------------------------------------------
using BalsamicSolutions.ApiCommon.Configuration;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace BalsamicSolutions.BlazorClient.Modules
{
    /// <summary>
    /// updates the api request to support Cors
    /// </summary>
    public class ApiAuthorizationMessageHandler : AuthorizationMessageHandler
    {
        //private readonly IAccessTokenProvider _Provider = null;
        //private readonly NavigationManager _Navigation;

        public ApiAuthorizationMessageHandler(IAccessTokenProvider provider, NavigationManager navigation, Settings settings)
        : base(provider, navigation)
        {
            //_Provider = provider;
            //_Navigation = navigation;
            ConfigureHandler(authorizedUrls: new[] { settings.AuthorizedApiUrl });
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.SetBrowserRequestMode(BrowserRequestMode.Cors);
            return await base.SendAsync(request, cancellationToken);
        }
    }
}