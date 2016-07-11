using System;

namespace CodedUITestAT
{
    /// <summary> Contains helper methods related to work with strings and texts. </summary>
    internal static class TextHelper
    {
        /// <summary> Generates a new unique name. </summary>
        internal static string GenerateName()
        {
            return @"TestName" + DateTime.Now.ToString("yyyyMMddHHmmss");
        }

        /// <summary> Puts the first letter of each word, separated by spaces, in a given text to upper case. </summary>
        /// <param name="str">Set a text.</param>
        internal static string ToUpperFirst(this string str)
        {
            char splitChar = ' ';
            string newStr = String.Empty;

            string[] words = str.Split(splitChar);
            foreach (string word in words)
			{
              string firstLetter;
			  if (word.Length > 1) firstLetter = word.Remove(1).ToUpper();
              else firstLetter = word.ToUpper();

              if (word.Length > 0) newStr += firstLetter + word.Substring(1) + splitChar;
              else newStr += firstLetter + splitChar;
			}

            return newStr.Remove(newStr.Length - 1);
        }
    }
}
