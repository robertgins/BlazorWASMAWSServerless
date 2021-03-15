//  -----------------------------------------------------------------------------
//   Copyright  (c)  Balsamic Solutions, LLC. All rights reserved.
//   THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF  ANY KIND, EITHER
//   EXPRESS OR IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR
//  -----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;

namespace BalsamicSolutions.ApiCommon
{
    //https://docs.aws.amazon.com/cognito-user-identity-pools/latest/APIReference/API_ListUsers.html

    /// <summary>
    /// Model class for a user
    /// </summary>
    public class User
    {
        public Dictionary<string, string> Attributes { get; set; }
        public string Email { get; set; }
        public string FamilyName { get; set; }
        public string GivenName { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string Status { get; set; }
        public string UserName { get; set; }
    }
}