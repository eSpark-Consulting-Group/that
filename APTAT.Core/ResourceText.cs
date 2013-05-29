using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using APTAT.Core.Messages;

namespace APTAT.Core
{
    /// <summary>
    /// Represents a string resource
    /// </summary>
    [Serializable]
    public sealed class ResourceText : IEquatable<ResourceText>, ITranslatable
    {
        private readonly string _sourceName;
        private readonly string _resourceId;
        private readonly string _text;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sourceName">Name of message source</param>
        /// <param name="resourceId">Resource identificator</param>
        /// <param name="text">Default value of string resource</param>
        /// <exception cref="ArgumentNullException">resourceId or text parameter is null</exception>
        internal ResourceText(string sourceName, string resourceId, string text)
        {
            if (resourceId == null)
            {
                throw new ArgumentNullException("resourceId");
            }
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }

            _sourceName = sourceName == null ? string.Empty : sourceName;
            _resourceId = resourceId;
            _text = text;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="resourceId">Resource identificator</param>
        /// <param name="text">Default value of string resource</param>
        /// <exception cref="ArgumentNullException">resourceId or text parameter is null</exception>
        internal ResourceText(string resourceId, string text)
            : this(string.Empty, resourceId, text)
        { }

        internal string SourceName
        {
            get
            {
                return _sourceName;
            }
        }

        internal string ResourceId
        {
            get
            {
                return _resourceId;
            }
        }

        internal string Text
        {
            get
            {
                return _text;
            }
        }

        public override string ToString()
        {
            return _text;
        }

        public string ToString(IMessageTranslatorCollection messageTranslatorCollection)
        {
            string result;
            TryTranslate(messageTranslatorCollection, out result);
            return result;
        }

        public static readonly ResourceText Empty = new ResourceText(string.Empty, string.Empty);

        public override bool Equals(object obj)
        {
            return this.Equals(obj as ResourceText);
        }

        public override int GetHashCode()
        {
            return this._sourceName.GetHashCode() ^ this._resourceId.GetHashCode() ^ this._text.GetHashCode();
        }

        public static bool IsNullOrEmpty(ResourceText value)
        {
            return value == null || Empty.Equals(value);
        }

        #region IEquatable<ResourceText> Members

        public bool Equals(ResourceText other)
        {
            return other == null ?
                false :
                object.ReferenceEquals(this, other) ||
                (this._sourceName.Equals(other._sourceName) && this._resourceId.Equals(other._resourceId) && this._text.Equals(other._text));
        }

        #endregion

        public bool TryTranslate(IMessageTranslatorCollection messageTranslatorCollection, out string translation)
        {
            translation = null;

            if (messageTranslatorCollection != null)
            {
                IMessageTranslator messageTranslator = messageTranslatorCollection[_sourceName];

                if (messageTranslator != null)
                {
                    translation = messageTranslator.Translate(_resourceId);
                }
            }

            bool translationNotFound = translation == null;
            if (translationNotFound)
            {
                translation = _text;
            }
            return !translationNotFound;
        }
        public bool TryTranslate(IMessageTranslatorCollection messageTranslatorCollection, CultureInfo cultureInfo, out string translation)
        {
            translation = null;

            if (messageTranslatorCollection != null)
            {
                IMessageTranslator messageTranslator = messageTranslatorCollection[_sourceName];

                if (messageTranslator != null)
                {
                    translation = messageTranslator.Translate(_resourceId, cultureInfo);
                }
            }

            bool translationNotFound = translation == null;
            if (translationNotFound)
            {
                translation = _text;
            }
            return !translationNotFound;
        }
    }



    interface ITranslatable
    {
        string ToString(IMessageTranslatorCollection messageTranslatorCollection);
    }
}
