//  -----------------------------------------------------------------------------
//   Copyright  (c)  Balsamic Solutions, LLC. All rights reserved.
//   THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF  ANY KIND, EITHER
//   EXPRESS OR IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR
//  -----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;

namespace BalsamicSolutions.ApiSupportLambdas
{
    /// <summary>
    /// simple class for deserializing the Authorization header
    /// </summary>
    public class Header
    {
        //--- Properties ---
        public string Authorization { get; set; }
    }
}