using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BalsamicSolutions.ApiCommon
{
    /// <summary>
    /// Interface used for the Sample service
    /// </summary>
    public interface IStuffAndNonsense
    {

        /// <summary>
        /// generate a random sentantce from the library
        /// </summary>
        /// <param name="minLength">-1 is any length</param>
        /// <param name="maxLength">max length</param>
        /// <returns>sentance</returns>
        Task<string> RandomSentance(int minLength, int maxLength);

        /// <summary>
        /// generate an array of sentances from the library
        /// </summary>
        /// <param name="minLength">-1 is any length</param>
        /// <param name="maxLength">max length</param>
        /// <returns>array of sentances</returns>
        Task<string[]> RandomSentances(StuffAndNonsenseOptions options);
    }
}
