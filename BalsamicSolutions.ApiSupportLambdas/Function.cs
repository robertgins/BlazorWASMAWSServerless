//  -----------------------------------------------------------------------------
//   Copyright  (c)  Balsamic Solutions, LLC. All rights reserved.
//   THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF  ANY KIND, EITHER
//   EXPRESS OR IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR
//  -----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using BalsamicSolutions.ApiCommon.Extensions;
using Amazon.Lambda.S3Events;
using BalsamicSolutions.ApiCommon.Configuration;
using Amazon.CloudFront.Model;

//References
//https://docs.aws.amazon.com/apigateway/latest/developerguide/apigateway-use-lambda-authorizer.html
//https://docs.aws.amazon.com/apigateway/latest/developerguide/api-gateway-lambda-authorizer-input.html
//https://docs.aws.amazon.com/apigateway/latest/developerguide/api-gateway-lambda-authorizer-output.html

namespace BalsamicSolutions.ApiSupportLambdas
{
    /// <summary>
    /// step one , load the jsonwebkeys during initaiialziation
    /// step two get the token
    /// step three parse the tokan to claims
    /// step four validate the token
    /// step five validate the issuer
    /// </summary>
    public class Function
    {
        private static readonly string AuthorizationHeaderPrefix = "Bearer ";
        private static JsonWebKeySet _IssuerKeySet = null;
        private static Settings _Settings = null;

        /// <summary>
        /// generic function handler so we can dispatch either the APIGatewayCustomAuthorizerRequest
        /// or the S3Event to the same Lambda instance
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<Stream> HandleEvent(Stream input, ILambdaContext context)
        {
            string textResponse = ""; //defaults to failed
            await Initialize();
            {
                string jsonAsText = string.Empty;
                if (null != input)
                {
                    StreamReader streamReader = new StreamReader(input);
                    jsonAsText = streamReader.ReadToEnd();
                }
                Console.WriteLine($"HandleEvent: received the following JSON: {jsonAsText}");
                //Ok we have the Json, which type of record is it ?
                int posToken = jsonAsText.IndexOf("\"AuthorizationToken\"", StringComparison.OrdinalIgnoreCase);
                if (posToken > -1 && null != _IssuerKeySet)
                {
                    //This is an api gateway request
                    APIGatewayCustomAuthorizerRequest gatewayRequest = jsonAsText.FromJson<APIGatewayCustomAuthorizerRequest>();
                    APIGatewayCustomAuthorizerResponse gatewayResponse = HandleAPIGatewayCustomAuthorizerRequest(gatewayRequest, context);
                    textResponse = gatewayResponse.ToJson();
                }
                else
                {
                    //S3Event
                    posToken = jsonAsText.IndexOf("\"Records\"", StringComparison.OrdinalIgnoreCase);
                    if (posToken > -1 && !string.IsNullOrWhiteSpace(_Settings.CloudFrontPubId)) ;
                    {
                        S3Event s3Event = jsonAsText.FromJson<S3Event>();
                        textResponse = await HandleS3Event(s3Event, context);
                    }
                }
            }
            byte[] byteArray = Encoding.UTF8.GetBytes(textResponse);
            return new MemoryStream(byteArray);
        }

        /// <summary>
        /// Initialize the module
        /// </summary>
        /// <returns></returns>
        public async Task Initialize()
        {
            if (null == _Settings)
            {
                _Settings = new Settings();
                if (!string.IsNullOrEmpty(_Settings.CognitoPoolUrl))
                {
                    string cognitoKeySetUrl = _Settings.CognitoPoolUrl.EndsWith("/") ? _Settings.CognitoPoolUrl + ".well-known/jwks.json" : _Settings.CognitoPoolUrl + "/.well-known/jwks.json";
                    using (HttpClient httpClient = new HttpClient())
                    {
                        LogText($"Attmpting to get keys from {cognitoKeySetUrl}");
                        HttpResponseMessage httpResponse = await httpClient.GetAsync(cognitoKeySetUrl);
                        if (httpResponse.IsSuccessStatusCode)
                        {
                            string jsonKeys = await httpResponse.Content.ReadAsStringAsync();
                            _IssuerKeySet = new JsonWebKeySet(jsonKeys);
                            LogText("Keys captured");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// create an authentication response
        /// </summary>
        /// <param name="allowRequest"></param>
        /// <param name="methodArn"></param>
        /// <param name="principalName"></param>
        /// <returns></returns>
        private APIGatewayCustomAuthorizerResponse CreateResponse(bool allowRequest, string methodArn, string principalName)
        {
            if (string.IsNullOrEmpty(principalName)) principalName = "user";
            APIGatewayCustomAuthorizerResponse returnValue = new APIGatewayCustomAuthorizerResponse();
            returnValue.PrincipalID = principalName;
            returnValue.Context = new APIGatewayCustomAuthorizerContextOutput();
            returnValue.PolicyDocument = new APIGatewayCustomAuthorizerPolicy
            {
                Statement = new List<APIGatewayCustomAuthorizerPolicy.IAMPolicyStatement>()
            };
            APIGatewayCustomAuthorizerPolicy.IAMPolicyStatement policyDocument = new APIGatewayCustomAuthorizerPolicy.IAMPolicyStatement();
            policyDocument.Action = new HashSet<string>
            {
                "execute-api:Invoke"
            };
            policyDocument.Effect = (allowRequest) ? "Allow" : "Deny";
            policyDocument.Resource = new HashSet<string>
            {
                methodArn
            };
            returnValue.PolicyDocument.Statement.Add(policyDocument);
            return returnValue;
        }

        /// <summary>
        /// Handles a request from the Api gateway for authentication
        /// step one , load the jsonwebkeys during initaiialziation
        /// step two get the token
        /// step three parse the tokan to claims
        /// step four validate the token
        /// step five validate the issuer
        /// </summary>
        /// <param name="gatewayRequest"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private APIGatewayCustomAuthorizerResponse HandleAPIGatewayCustomAuthorizerRequest(APIGatewayCustomAuthorizerRequest gatewayRequest, ILambdaContext context)
        {
            //default to a failed response
            APIGatewayCustomAuthorizerResponse returnValue = CreateResponse(false, gatewayRequest.MethodArn, "user");
            LogText("start of event " + gatewayRequest.MethodArn);

            //First get the authorization token
            string authToken = ParseAuthorizationToken(gatewayRequest);
            if (string.IsNullOrEmpty(authToken))
            {
                LogText("Unauthorized: missing Authorization token");
            }
            else
            {
                LogText($"Authorization : {authToken}");
                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                try
                {
                    Dictionary<string, string> claims = tokenHandler.ReadJwtToken(authToken).Claims.ToDictionary(claim => claim.Type, claim => claim.Value);
                    LogText($"JWT Claims: {JsonConvert.SerializeObject(claims)}");
                    try
                    {
                        TokenValidationParameters tvParams = new TokenValidationParameters
                        {
                            IssuerSigningKeys = _IssuerKeySet.Keys,
                            ValidIssuer = _Settings.CognitoPoolUrl,
                            ValidateAudience = false
                        };
                        ClaimsPrincipal claimsP = tokenHandler.ValidateToken(authToken, tvParams, out SecurityToken validatedToken);
                        Claim nameClaim = claimsP.FindFirst(_Settings.CognitoNameClaim);
                        if (null != nameClaim && !string.IsNullOrEmpty(nameClaim.Value))
                        {
                            LogText($"Authorized: {nameClaim.Value}");
                            returnValue = CreateResponse(true, gatewayRequest.MethodArn, nameClaim.Value);
                        }
                    }
                    catch (SecurityTokenExpiredException)
                    {
                        LogText("Unauthorized: token expired");
                    }
                    catch (SecurityTokenInvalidAlgorithmException)
                    {
                        LogText("Unauthorized: invalid algorithm");
                    }
                    catch (SecurityTokenInvalidAudienceException)
                    {
                        LogText("Unauthorized: invalid audience");
                    }
                    catch (SecurityTokenInvalidIssuerException)
                    {
                        LogText("Unauthorized: invalid issuer");
                    }
                    catch (SecurityTokenInvalidLifetimeException)
                    {
                        LogText("Unauthorized: invalid lifetime");
                    }
                    catch (SecurityTokenInvalidSignatureException)
                    {
                        LogText("Unauthorized: invalid signature");
                    }
                    catch (SecurityTokenInvalidSigningKeyException)
                    {
                        LogText("Unauthorized: invalid signing keys");
                    }
                    catch (SecurityTokenInvalidTypeException)
                    {
                        LogText("Unauthorized: invalid type");
                    }
                    catch (SecurityTokenNoExpirationException)
                    {
                        LogText("Unauthorized: no expiration");
                    }
                    catch (SecurityTokenNotYetValidException)
                    {
                        LogText("Unauthorized: not yet valid");
                    }
                    catch (SecurityTokenReplayAddFailedException)
                    {
                        LogText("Unauthorized: replay add failed");
                    }
                    catch (SecurityTokenReplayDetectedException)
                    {
                        LogText("Unauthorized: replay detected");
                    }
                    catch (SecurityTokenValidationException)
                    {
                        LogText("Unauthorized: validation failed");
                    }
                    catch (Exception validationError)
                    {
                        LogText(validationError, "Unauthorized: general error");
                    }
                }
                catch (Exception decodeError)
                {
                    LogText(decodeError, $"Failed to decode authtoken {authToken}");
                }
            }
            LogText("End of event");
            return returnValue;
        }

        /// <summary>
        /// handles a notification from S3 and invalidates
        /// the related files in Cloudfront
        /// </summary>
        /// <param name="s3Event"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private async Task<string> HandleS3Event(S3Event s3Event, ILambdaContext context)
        {
            string returnValue = "";
            List<string> objectPaths = s3Event.Records.Select(record => "/" + record.S3.Object.Key).ToList();
            var awsOptions = _Settings.AwsOptions;
            Amazon.CloudFront.AmazonCloudFrontClient cfClient = new Amazon.CloudFront.AmazonCloudFrontClient(awsOptions.Credentials, awsOptions.Region);
            CreateInvalidationRequest invRequest = new CreateInvalidationRequest
            {
                DistributionId = _Settings.CloudFrontPubId,
                InvalidationBatch = new InvalidationBatch
                {
                    CallerReference = Guid.NewGuid().ToString(),
                    Paths = new Paths
                    {
                        Items = objectPaths,
                        Quantity = objectPaths.Count
                    }
                }
            };
            // invalidate CloudFront paths
            CreateInvalidationResponse invResponse = null;
            try
            {
                invResponse = await cfClient.CreateInvalidationAsync(invRequest);
                LogText($"Invalidated {objectPaths.Count:N0} CloudFront paths:\n{string.Join("\n", objectPaths)}");
                returnValue = "Ok";
            }
            catch
            {
                LogText("Failed to invalidate CloudFrontPaths");
                returnValue = invResponse.HttpStatusCode.ToString();
            }
            return returnValue;
        }

        /// <summary>
        /// log stuff
        /// </summary>
        /// <param name="message"></param>
        private void LogText(string message)
        {
            Console.WriteLine(message);
        }

        /// <summary>
        /// log stuff
        /// </summary>
        /// <param name="err"></param>
        /// <param name="message"></param>
        private void LogText(Exception err, string message)
        {
            Console.WriteLine($"----Start Exception-- {message}");
            Console.WriteLine(err.Message);
            Console.WriteLine(err.StackTrace);
            Console.WriteLine($"----End Exception---- {message}");
        }

        /// <summary>
        /// parse the Bearer token
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private string ParseAuthorizationToken(APIGatewayCustomAuthorizerRequest request)
        {
            if (null != request && null != request.Headers)
            {
                //Chrome and Firefox send lowercase authorization headers
                if (!request.Headers.TryGetValue("authorization", out string authorizationHeader))
                {
                    request.Headers.TryGetValue("Authorization", out authorizationHeader);
                }
                if (!string.IsNullOrEmpty(authorizationHeader))
                {
                    //we have one so try to get it
                    if (authorizationHeader.StartsWith(AuthorizationHeaderPrefix))
                    {
                        return authorizationHeader.Substring(AuthorizationHeaderPrefix.Length).Trim();
                    }

                    // not a valid 'Authorization' header value
                    LogText("No Authorization header");
                    return null;
                }
            }

            //last resort return the actual token 
            return request.AuthorizationToken;
        }
    }
}