using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace BalsamicSolutions.ApiServer
{
    /// <summary>
    /// defines a claim (default as a group)
    /// </summary>
    public class CognitoClaimAuthorizationRequirement : IAuthorizationRequirement
    {
        public CognitoClaimAuthorizationRequirement(string groupName)
            : this("cognito:groups", groupName)
        {
        }

        public CognitoClaimAuthorizationRequirement(string claimType, string claimValue)
        {
            ClaimType = claimType;
            ClaimValue = claimValue;
        }

        public string ClaimType { get; private set; }

        public string ClaimValue { get; private set; }
    }
}
