using BalsamicSolutions.ApiCommon;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BalsamicSolutions.ApiServer.Controllers
{
    /// <summary>
    /// sample remote service
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class StuffAndNonsenseController : ControllerBase, IStuffAndNonsense
    {
        /// <summary>
        /// generate a random sentantce from the library
        /// </summary>
        /// <param name="minLength">minimun length</param>
        /// <param name="maxLength">max length</param>
        /// <returns>sentance</returns>
        public Task<string> RandomSentance(int minLength, int maxLength)
        {
            string returnValue = StuffAndNonsense.RandomStuff.RandomSentance(minLength, maxLength);
            return Task.FromResult(returnValue);
        }

        /// <summary>
        /// generate an array of sentances from the library
        /// </summary>
        /// <param name="minLength">minimun length</param>
        /// <param name="maxLength">max length</param>
        /// <returns>array of sentances</returns>
        public Task<string[]> RandomSentances(StuffAndNonsenseOptions options)
        {
            string[] returnValue = StuffAndNonsense.RandomStuff.RandomSentances(options.Size, options.MinLength, options.MaxLength);
            return Task.FromResult(returnValue);
        }
    }
}
