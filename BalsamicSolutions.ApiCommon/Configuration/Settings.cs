//  -----------------------------------------------------------------------------
//   Copyright  (c)  Balsamic Solutions, LLC. All rights reserved.
//   THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF  ANY KIND, EITHER
//   EXPRESS OR IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR
//  -----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Text;
using System.Collections;
using System.Threading.Tasks;
using Amazon.Extensions.NETCore.Setup;
using Amazon;
using Amazon.Runtime;

namespace BalsamicSolutions.ApiCommon.Configuration
{
    /// <summary>
    /// typed class to load settings from appsetting.json or from
    /// envionrment variables
    /// </summary>
    public class Settings
    {
        public const string APP_SETTINGS = "appSettings";
        public const string APP_SETTINGS_FILE_NAME = "appsettings.json";
        private static Settings _Settings;
        private OIDCMetaData _MetaData = null;
        private Dictionary<string, string> _RawValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// ctor from webassembly
        /// </summary>
        public Settings(string appSettings)
        {
            var jDoc = System.Text.Json.JsonDocument.Parse(appSettings);
            var appDoc = jDoc.RootElement.GetProperty("appSettings");
            foreach (var keyPair in appDoc.EnumerateObject())
            {
                _RawValues[keyPair.Name] = keyPair.Value.ToString();
            }
        }

        /// <summary>
        /// general CTOR
        /// </summary>
        public Settings()
        {
            //load Environment first
            LoadEnvironmentSettings();
            //then constants from file
            LoadFileSettings();
        }

        //for when DI fails
        public static Settings Instance
        {
            get
            {
                return _Settings;
            }
            set
            {
                _Settings = value;
            }
        }

        /// <summary>
        /// target API's
        /// </summary>
        public string AuthorizedApiUrl
        {
            get
            {
                _RawValues.TryGetValue("AuthorizedApiUrl", out string returnValue);
                return returnValue;
            }
        }

        /// <summary>
        /// CORS Origins
        /// </summary>
        public string[] AuthorizedClientUrls
        {
            get
            {
                string[] returnValue = new string[0];
                if (_RawValues.TryGetValue("AuthorizedClientUrls", out string rawValue))
                {
                    returnValue = rawValue.Split(',');
                }
                return returnValue;
            }
        }

        /// <summary>
        /// target API's
        /// </summary>
        public string AwsAccessKey
        {
            get
            {
                _RawValues.TryGetValue("AwsAccessKey", out string returnValue);
                return returnValue;
            }
        }

        public AWSOptions AwsOptions
        {
            get
            {
                RegionEndpoint endPoint = GetEndPoint(AwsRegion);
                AWSCredentials awsCreds = GetCredentials(AwsSecretKey, AwsAccessKey);
                AWSOptions returnValue = new AWSOptions
                {
                    Region = endPoint,
                    Credentials = awsCreds
                };
                return returnValue;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public string AwsRegion
        {
            get
            {
                _RawValues.TryGetValue("AwsRegion", out string returnValue);
                return returnValue;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public string AwsSecretKey
        {
            get
            {
                _RawValues.TryGetValue("AwsSecretKey", out string returnValue);
                return returnValue;
            }
        }

        /// <summary>
        /// cloud front publication id
        /// </summary>
        public string CloudFrontPubId
        {
            get
            {
                _RawValues.TryGetValue("CloudFrontPubId", out string returnValue);

                return returnValue;
            }
        }

        /// <summary>
        /// client ID in pool 7hr3i27r24gkegasbmf9t9oh9j
        /// </summary>
        public string CognitoClientId
        {
            get
            {
                _RawValues.TryGetValue("CognitoClientId", out string returnValue);

                return returnValue;
            }
        }

        /// <summary>
        /// Name claim "cognito:username"
        /// </summary>
        public string CognitoNameClaim
        {
            get
            {
                _RawValues.TryGetValue("CognitoNameClaim", out string returnValue);
                return returnValue;
            }
        }

        /// <summary>
        /// Pool URL https://cognito-idp.us-east-1.amazonaws.com/us-east-1_mSGIMVY8H
        /// </summary>
        public string CognitoPoolUrl
        {
            get
            {
                _RawValues.TryGetValue("CognitoPoolUrl", out string returnValue);
                return returnValue;
            }
        }

        /// <summary>
        /// client requested scopes
        /// </summary>
        public string[] CognitoScopes
        {
            get
            {
                string[] returnValue = new string[0];
                if (_RawValues.TryGetValue("CognitoScopes", out string rawValue))
                {
                    returnValue = rawValue.Split(',');
                }
                return returnValue;
            }
        }

        /// <summary>
        /// role/group to validate
        /// </summary>
        public string CognitoWeatherCenterGroupName
        {
            get
            {
                _RawValues.TryGetValue("CognitoWeatherCenterGroupName", out string returnValue);
                return returnValue;
            }
        }

        /// <summary>
        /// client inactivity timeout
        /// </summary>
        public int InactivityTimeoutInMinutes
        {
            get
            {
                int returnValue = 0;
                if (_RawValues.TryGetValue("InactivityTimeoutInMinutes", out string tempValue))
                {
                    if (int.TryParse(tempValue, out int numValue))
                    {
                        if (numValue > 0) returnValue = numValue;
                    }
                }
                return returnValue;
            }
        }

        public OIDCMetaData MetaData
        {
            get
            {
                return _MetaData;
            }
        }

        /// <summary>
        /// Convient place to store calculated field
        /// </summary>
        public string RedirectUri { get; set; }

        public async Task LoadOidcMetaDataAsync()
        {
            if (null == _MetaData)
            {
                string metaDataUrl = $"{CognitoPoolUrl}/.well-known/openid-configuration";
                _MetaData = new OIDCMetaData();
                await _MetaData.LoadMetaDataAsync(metaDataUrl);
            }
        }

        private AWSCredentials GetCredentials(string secretKey, string accessKey)
        {
            if (string.IsNullOrWhiteSpace(secretKey) || string.IsNullOrWhiteSpace(accessKey))
            {
                //This is correct for Lambda
                return new InstanceProfileAWSCredentials();
            }
            else
            {
                return new BasicAWSCredentials(accessKey, secretKey);
            }
        }

        private RegionEndpoint GetEndPoint(string awsRegion)
        {
            if (string.IsNullOrWhiteSpace(awsRegion))
            {
                return FallbackRegionFactory.GetRegionEndpoint();
            }
            else
            {
                return RegionEndpoint.GetBySystemName(awsRegion);
            }
        }

        /// <summary>
        /// load from Environment
        /// </summary>
        private void LoadEnvironmentSettings()
        {
            IDictionary environmentVariables = Environment.GetEnvironmentVariables();
            var keys = environmentVariables.Keys
                .OfType<string>()
                .Select(ev => new { parsedKey = string.IsNullOrWhiteSpace(ev) ? "" : ev.Replace("__", ":"), originalKey = ev })
                .ToArray();

            foreach (var evkey in keys)
            {
                _RawValues[evkey.parsedKey] = environmentVariables[evkey.originalKey] as string;
            }
        }

        /// <summary>
        /// load from settings file
        /// </summary>
        private void LoadFileSettings()
        {
            string localPath = AppDomain.CurrentDomain.BaseDirectory;
            var configurationBuilder = new ConfigurationBuilder().SetBasePath(localPath);
            configurationBuilder.AddJsonFile(APP_SETTINGS_FILE_NAME, optional: false, reloadOnChange: false);
            var appSettings = configurationBuilder.Build();

            foreach (var keyValuePair in appSettings.AsEnumerable())
            {
                if (null != keyValuePair.Value)
                {
                    string key = keyValuePair.Key.Replace(APP_SETTINGS, string.Empty).TrimStart(new char[] { ':', '_' });
                    if (!string.IsNullOrWhiteSpace(key))
                    {
                        _RawValues[key] = keyValuePair.Value;
                    }
                }
            }
        }
    }
}