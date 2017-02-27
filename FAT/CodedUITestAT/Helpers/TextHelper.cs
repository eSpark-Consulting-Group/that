using System;
using System.ComponentModel;

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

        /// <summary> Generates a non-default port value. </summary>
        /// <param name="portType">Set a port type.</param>
        internal static string GenerateNonDefaultPort(PortType portType)
        {
            int port;
            const int maxPort = 65635;
            Random rand = new Random();

            switch (portType)
            {
                case PortType.Tcp:
                    port = Settings.General.Default.DefaultFlsTcpPort;
                    break;
                case PortType.Udp:
                    port = Settings.General.Default.DefaultFlsUdpPort;
                    break;
                default: throw new Exception(String.Format("Unexpected port type '{0}'!", portType));
            }

            port += rand.Next(1, maxPort - port);

            return port.ToString();
        }

        /// <summary> Converts a given enumeration value to its UI analogue.  </summary>
        /// <param name="enumeration">Set an enumeration.</param>
        internal static string ToUi(this Enum enumeration)
        {
            string str;

            var type = enumeration.GetType();
            var memInfo = type.GetMember(enumeration.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes.Length > 0) str = ((DescriptionAttribute) attributes[0]).Description;
            else str = enumeration.ToString();

            return str;
        }

        /// <summary> Removes an "extension" from a given string. </summary>
        internal static string WithoutExtension(this string str)
        {
            int dotPosition = str.LastIndexOf(".");
            return str.Remove(dotPosition);
        }
    }
}
