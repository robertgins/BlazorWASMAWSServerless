using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;
using BalsamicSolutions.ApiCommon.Extensions;

namespace BalsamicSolutions.ApiServer.StuffAndNonsense
{
    /// <summary>
    /// simple functions for reading out static resource file
    /// and generating content
    /// </summary>
    public static class RandomStuff
    {
        static readonly Random _RandomNumberGenerator = RandomNumberGenerator();
        static IList<string> _Sentances = null;

        public const string LOREM_IPSUM = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum";


        /// <summary>
        /// generate a random sentance 
        /// </summary>
        /// <param name="minLength">minimum length of the sentance</param>
        /// <param name="maxLength">maximum length </param>
        /// <returns></returns>
        public static string RandomSentance(int minLength = 1, int maxLength = 4096)
        {
            return RandomSentance(Sentences, minLength, maxLength);
        }

        /// <summary>
        /// generate an array of sentances from the library
        /// </summary>
        /// <param name="minLength">-1 is any length</param>
        /// <param name="maxLength">max length</param>
        /// <returns>array of sentances</returns>
        public static string[] RandomSentances(int size, int minLength = 1, int maxLength = 4096)
        {
            List<string> randomSentances = new List<string>();
            while (randomSentances.Count < size)
            {
                randomSentances.Add(RandomSentance(Sentences, minLength, maxLength));
            }
            string[] returnValue = randomSentances.ToArray();
            RandomizeArray(returnValue);
            return returnValue;
        }

        /// <summary>
        /// get a sentance from an array and trim it to the length specified
        /// </summary>
        /// <param name="candidateText"></param>
        /// <param name="minLength"></param>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        public static string RandomSentance(IList<string> candidateText, int minLength = 1, int maxLength = 4096)
        {
            string returnValue = string.Empty;
            while (returnValue.Length < minLength)
            {
                returnValue += candidateText[_RandomNumberGenerator.Next(0, candidateText.Count)];
                for (int idx = 0; idx < _RandomNumberGenerator.Next(0, 3); idx++)
                {
                    returnValue += candidateText[_RandomNumberGenerator.Next(0, candidateText.Count)];
                }
            }
            return returnValue.TrimTo(maxLength);
        }

        /// <summary>
        /// randomize the order of a list object
        /// </summary>
        /// <typeparam name="TObjectType">type of object in list</typeparam>
        /// <param name="shuffleThis">list to randomize</param>
        public static void RandomizeList<TObjectType>(IList<TObjectType> shuffleThis)
        {
            int listIdx = shuffleThis.Count;
            while (listIdx > 1)
            {
                listIdx--;
                int newPos = _RandomNumberGenerator.Next(listIdx + 1);
                TObjectType value = shuffleThis[newPos];
                shuffleThis[newPos] = shuffleThis[listIdx];
                shuffleThis[listIdx] = value;
            }
        }

        /// <summary>
        /// randomize the order of an array
        /// </summary>
        /// <param name="shuffleThis">array to randomize</param>
        public static void RandomizeArray(object[] shuffleThis)
        {
            int listIdx = shuffleThis.Length;
            while (listIdx > 1)
            {
                listIdx--;
                int newPos = _RandomNumberGenerator.Next(listIdx + 1);
                object value = shuffleThis[newPos];
                shuffleThis[newPos] = shuffleThis[listIdx];
                shuffleThis[listIdx] = value;
            }
        }

        /// <summary>
        /// return an array of all candidate sentences
        /// </summary>
        public static IList<string> Sentences
        {
            get
            {
                if (null == _Sentances)
                {
                    _Sentances = LoadText("BalsamicSolutions.ApiServer.StuffAndNonsense.Sentences.txt").AsReadOnly();
                }
                return _Sentances;
            }
        }

        /// <summary>
        /// load embeded resource strings as text
        /// </summary>
        /// <param name="resName"></param>
        /// <returns></returns>
        static List<string> LoadText(string resName)
        {
            List<string> returnValue = new List<string>();
            var thisAssm = Assembly.GetExecutingAssembly();
            Stream ioStream = thisAssm.GetManifestResourceStream(resName);
            using (StreamReader srNames = new StreamReader(ioStream))
            {
                string lineText = srNames.ReadLine();
                while (null != lineText && lineText.Length > 0)
                {
                    returnValue.Add(lineText);
                    lineText = srNames.ReadLine();
                }
            }
            return returnValue;
        }

        public static Random RandomNumberGenerator()
        {
            byte[] randomBytes = new byte[4];

            // Generate 4 random bytes.
            RNGCryptoServiceProvider rngSeed = new RNGCryptoServiceProvider();
            rngSeed.GetBytes(randomBytes);

            // Convert 4 bytes into a 32-bit integer value.
            int randomSeed = (randomBytes[0] & 0x7f) << 24 |
                             randomBytes[1] << 16 |
                             randomBytes[2] << 8 |
                             randomBytes[3];
            return new Random(randomSeed);
        }
    }
}
