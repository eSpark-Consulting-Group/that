using System;

namespace FAT.Application.Models
{
    public struct ScanStatus : IComparable
    {
        private readonly ScanStatusEnum _value;


        public ScanStatus(ScanStatusEnum value)
        {
            _value = value;
        }

        public override string ToString()
        {
            if (_value == ScanStatusEnum.InProgress)
            {
                return "In Progress";
            }


            return _value.ToString();
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override Boolean Equals(Object obj)
        {
            if (obj is ScanStatus)
            {
                ScanStatus status = (ScanStatus)obj;

                if (status._value == _value)
                {
                    return true;
                }
            }

            return false;
        }


        public int CompareTo(object obj)
        {
            return 0;
        }
    }
}