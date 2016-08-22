using System;
using System.Globalization;

namespace APTAT.DeploymentCore.Messages
{
    [Serializable]
    public sealed class FormattedText : IEquatable<FormattedText>, ITranslatable
    {
        private readonly ResourceText _format;
        private readonly object[] _arguments;
        private static readonly Type[] _supportedArgumentTypes = 
		{ 
			(typeof(byte)),
			(typeof(bool)),
			(typeof(short)),
			(typeof(int)),
			(typeof(long)),
			(typeof(decimal)),
			(typeof(double)),
			(typeof(string)),
			(typeof(DateTime)),
			(typeof(sbyte)),
			(typeof(uint)),
			(typeof(ulong)),
			(typeof(ushort)),			
			(typeof(ResourceText)),
			(typeof(FormattedText))
		};

        public static readonly FormattedText Empty = new FormattedText(ResourceText.Empty);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="format">A string resource which represent a composite format string.</param>
        /// <param name="arguments">An object array containing zero or more objects to format.
        /// The following types of arguments item are supported: 
        /// byte, bool,	short, int, long, decimal, double, string, DateTime, sbyte, uint, ulong, ushort, ResourceText, FormattedText
        /// </param>
        /// <exception cref="ArgumentNullException">formatStr or arguments parameter is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">arguments array contains an item which type is not supported</exception>
        public FormattedText(ResourceText format, params object[] arguments)
        {
            if (format == null)
            {
                throw new ArgumentNullException(nameof(format));
            }
            if (arguments == null)
            {
                throw new ArgumentNullException(nameof(arguments));
            }
            foreach (var value in arguments)
            {
                if (value != null && Array.IndexOf(_supportedArgumentTypes, value.GetType()) < 0)
                {
                    throw new ArgumentException("Type of argument object is not supported", nameof(arguments));
                }
            }

            _format = format;
            _arguments = arguments;
        }


        internal ResourceText Format
        {
            get
            {
                return _format;
            }
        }

        internal object[] Arguments
        {
            get
            {
                return _arguments;
            }
        }

        public override string ToString()
        {
            return FormatText(_format, _arguments, null);
        }

        public string ResourceId
        {
            get { return _format.ResourceId; }
        }

        public string ToString(IMessageTranslatorCollection messageTranslatorCollection)
        {
            return FormatText(_format, _arguments, messageTranslatorCollection);
        }
        public string ToString(IMessageTranslatorCollection messageTranslatorCollection, CultureInfo cultureInfo)
        {
            return FormatText(_format, _arguments, messageTranslatorCollection, cultureInfo);
        }
        private static string FormatText(ResourceText format, object[] args, IMessageTranslatorCollection messageTranslators, CultureInfo cultureInfo = null)
        {
            string result;
            string formatStr;

            if (cultureInfo == null) cultureInfo = CultureInfo.CurrentUICulture;

            // translate and format arguments also if it is possible
            object[] arguments = Array.ConvertAll(args, argument =>
            {
                if (argument is FormattedText)
                {
                    var formattedText = (FormattedText)argument;
                    return formattedText.ToString(messageTranslators, cultureInfo);
                }
                if (argument is ResourceText)
                {
                    var resourseText = (ResourceText)argument;
                    string stringText;
                    if (!resourseText.TryTranslate(messageTranslators, cultureInfo, out stringText))
                        stringText = resourseText.ToString();
                    return stringText;
                }
                return argument is ITranslatable ?
                    ((ITranslatable)argument).ToString(messageTranslators)
                    : argument;
            });

            // try to format using translated format string and current culture
            if (format.TryTranslate(messageTranslators, cultureInfo, out formatStr))
            {
                if (TryFormatString(cultureInfo, formatStr, arguments, out result))
                {
                    return result;
                }
            }

            // otherwise, try to format using default format string and inavariant culture
            formatStr = format.ToString();
            if (TryFormatString(CultureInfo.InvariantCulture, formatStr, arguments, out result))
            {
                return result;
            }

            // otherwise, return default format string
            result = formatStr;

            return result;
        }


        public override bool Equals(object obj)
        {
            return this.Equals(obj as FormattedText);
        }

        public override int GetHashCode()
        {
            return this._format.GetHashCode();
        }

        public static bool IsNullOrEmpty(FormattedText value)
        {
            return value == null || Empty.Equals(value);
        }

        #region IEquatable<FormattedText> Members

        public bool Equals(FormattedText other)
        {
            return other == null ?
                false :
                object.ReferenceEquals(this, other) ||
                (this._format.Equals(other._format) && AreEqual(this._arguments, other._arguments));
        }

        private static bool AreEqual(object[] arrA, object[] arrB)
        {
            if (!object.ReferenceEquals(arrA, arrB))
            {
                if (arrA.Length != arrB.Length)
                    return false;

                for (int i = 0; i < arrA.Length; i++)
                {
                    object objA = arrA[i];
                    object objB = arrB[i];
                    if (!object.Equals(objA, objB))
                        return false;
                }
            }
            return true;
        }

        private static bool TryFormatString(IFormatProvider formatProvider, string format, object[] args, out string result)
        {
            result = string.Empty;
            try
            {
                result = string.Format(formatProvider, format, args);
            }
            catch (FormatException)
            {
                return false;
            }

            return true;
        }

        #endregion
    }
}
