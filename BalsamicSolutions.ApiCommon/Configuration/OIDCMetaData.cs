using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using System.Net;
using System.Net.Http;
using System.Threading;
namespace BalsamicSolutions.ApiCommon.Configuration
{
    /// <summary>
    /// retrieves OIDC Metadata for the system we are connected to
    /// </summary>
    public class OIDCMetaData
    {

        public async Task LoadMetaDataAsync(string metaDataAddress)
        {

            if (string.IsNullOrWhiteSpace(metaDataAddress)) throw new ArgumentNullException(nameof(metaDataAddress));
            if (!Uri.TryCreate(metaDataAddress, UriKind.Absolute, out Uri metaDataUri))
            {
                throw new ArgumentException("invalid url metaDataAddress");
            }
            using (HttpClient webClient = new HttpClient { BaseAddress = new Uri(metaDataAddress) })
            {
                string jsonText = await webClient.GetStringAsync(metaDataAddress);
                var appDoc = JsonDocument.Parse(jsonText);
                var oidcConfig = appDoc.RootElement;
                AuthorizationEndpoint = oidcConfig.GetProperty("authorization_endpoint").GetString();
                TokenEndpoint = oidcConfig.GetProperty("token_endpoint").GetString();
                UserinfoEndpoint = oidcConfig.GetProperty("userinfo_endpoint").GetString();
                Issuer = oidcConfig.GetProperty("issuer").GetString();
                JSONWebKeySetUri = oidcConfig.GetProperty("jwks_uri").GetString();
                SupportedResponseTypes = oidcConfig.GetProperty("response_types_supported").EnumerateArray().Select(p => p.GetString()).ToArray();
                SupportedResponseTypes = oidcConfig.GetProperty("response_types_supported").EnumerateArray().Select(p => p.GetString()).ToArray();
                SupportedScopes = oidcConfig.GetProperty("scopes_supported").EnumerateArray().Select(p => p.GetString()).ToArray();
                SupportedSubjectTypes = oidcConfig.GetProperty("subject_types_supported").EnumerateArray().Select(p => p.GetString()).ToArray();
                SupportedTokenEndpointAuthenticationMethods = oidcConfig.GetProperty("token_endpoint_auth_methods_supported").EnumerateArray().Select(p => p.GetString()).ToArray();
                //Cognito specific calculation
                string tempStr = new Uri(AuthorizationEndpoint).GetLeftPart(UriPartial.Authority);
                LogoutEndpoint = tempStr + "/logout";
            }

        }

        public string AuthorizationEndpoint { get; private set; }
        public string LogoutEndpoint { get; private set; }
        public string TokenEndpoint { get; private set; }
        public string UserinfoEndpoint { get; private set; }
        public string[] SupportedResponseTypes { get; private set; }
        public string[] SupportedScopes { get; private set; }
        public string[] SupportedSubjectTypes { get; private set; }
        public string[] SupportedTokenEndpointAuthenticationMethods { get; private set; }
        public string Issuer { get; private set; }
        public string JSONWebKeySetUri { get; private set; }
    }
}
