using APTAT.DeploymentCore;
using System;
using DDT.Application.Extensions;

namespace DDT.Application.Models
{
    public enum DeploymentItemSource
    {
        Domain, Workgroup, Ip
    }

    public class DeploymentItem : SelectableItemWrapper<String>
    {
        private bool _failed;
        private SystemKind? _systemKind;
        private double _percentage;
        public String Name { get; set; }

        public double Percentage
        {
            get { return IsOnline ? _percentage : 100; }
            set
            {
                _percentage = value;
                RaisePropertyChanged(nameof(InProgress));
            }
        }

        private bool _stopping;
        public bool Stopping
        {
            get { return _stopping; }
            set
            {
                _stopping = value;
                RaisePropertyChanged(nameof(InProgress));
            }
        }

        public bool InProgress
        {
            get
            {
                return !Stopping && Percentage > 0 && Percentage < 100;
            }
        }

        public Guid TargetUid { get; set; }
        public override string Value
        {
            get
            {
                if (!IsOnline)
                    return "Computer is not accessible.";

                return base.Value;
            }
            set
            {
                base.Value = value;
            }
        }

        public String IP { get; set; }

        public Boolean IsOnline { get; set; }

        public String Status { get; set; }

        public bool Succeded { get; set; }

        public bool Failed
        {
            get
            {
                if (!IsOnline)
                    return true;
                return _failed;
            }
            set
            {
                _failed = value;
                RaisePropertyChanged(nameof(Failed));
            }
        }

        public DeploymentItemSource Source { get; set; }

        private string _packageName;
        public string PackageName
        {
            get { return _packageName; }
            set
            {
                if (_packageName == value) return;;
                _packageName = value;
                RaisePropertyChanged(nameof(PackageName));
                RaisePropertyChanged(nameof(HasPackageName));
            }
        }

        public bool HasPackageName
        {
            get { return !string.IsNullOrEmpty(PackageName); }
        }
       
        public SystemKind? SystemKind   
        {
            get { return _systemKind; }
            set
            {
                if (_systemKind == value) return;
                _systemKind = value;
                RaisePropertyChanged(nameof(SystemKind));
                RaisePropertyChanged(nameof(OS));
            }
        }

        public string OS { get { return _systemKind.ToFormattedString(Is64); } }

        public string DnsName { get; set; }
        public bool IsAptHost { get; set; }
        public string LdapPath { get; set; }

        public virtual double Height { get { return 32; } }
        public bool? Is64 { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}