using System;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace APTAT.DAL.Storage.Core.Sorting
{
    public sealed class SortingKey
    {
        private readonly string _keyName;
        private readonly ListSortDirection _sortOrder;

        public SortingKey(string keyName, ListSortDirection sortOrder)
        {
            if (keyName == null)
                throw new ArgumentNullException("keyName");
            if (keyName.Length == 0)
                throw new ArgumentException("The value should not be an empty string.", "keyName");

            _keyName = keyName;
            _sortOrder = sortOrder;
        }

        public SortingKey(string sortingKey)
        {
            if (sortingKey == null)
                throw new ArgumentNullException("sortingKey");

            Parse(sortingKey, out _keyName, out _sortOrder);
        }

        public ListSortDirection SortOrder
        {
            get { return _sortOrder; }
        }

        public string KeyName
        {
            get { return _keyName; }
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", KeyName, SortOrder);
        }

        public static implicit operator SortingKey(string sortingKey)
        {
            return new SortingKey(sortingKey);
        }

        public static explicit operator string(SortingKey sortingKey)
        {
            if (sortingKey == null) throw new ArgumentNullException("sortingKey");
            return sortingKey.ToString();
        }

        private static void Parse(string sortingKey, out string keyName, out ListSortDirection sortOrder)
        {
            var words = Regex.Split(sortingKey.Trim(), @"\s+");

            if (words.Length == 0)
            {
                throw new FormatException("The value should not be an empty string.");
            }

            const string theValueHasWrongFormat = "The value has wrong format.";
            if (words.Length > 2)
            {
                throw new FormatException(theValueHasWrongFormat);
            }

            keyName = words[0];
            if (words.Length == 1)
            {
                sortOrder = ListSortDirection.Ascending;
            }
            else
            {
                string sortOrderStr = words[1].ToLowerInvariant();

                switch (sortOrderStr)
                {
                    case "ascending":
                    case "asc":
                        sortOrder = ListSortDirection.Ascending;
                        break;
                    case "descending":
                    case "desc":
                        sortOrder = ListSortDirection.Descending;
                        break;
                    default:
                        throw new FormatException(theValueHasWrongFormat);
                }
            }
        }
    }
}
