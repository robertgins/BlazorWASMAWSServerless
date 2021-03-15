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
    /// <summary>
    /// model class for a group
    /// </summary>
    public class Group
    {
        public string Description { get; set; }
        public string Name { get; set; }
        public string RoleArn { get; set; }
    }
}