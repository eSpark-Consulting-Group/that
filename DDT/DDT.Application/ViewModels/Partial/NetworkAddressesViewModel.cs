using System.Collections.ObjectModel;
using System.Linq;
using System;
using Telerik.Windows.Controls;
using DDT.Application.Models;
using System.Collections.Generic;

namespace DDT.Application.ViewModels
{
    public partial class MainViewModel
    {
        partial void InitializeNetworkAddresses()
        {
            AddNetworkAddressCommand = new DelegateCommand(AddNetworkAddress, CanAddNetworkAddress);
            ClearNetworkAddressCommand = new DelegateCommand(ClearNetworkAddress);

            NetworkAddresses = new ObservableCollection<IpAddressWrapper>();
            NetworkAddressItems = new List<DeploymentItem>();

            _validator = new IpAddressValidator();
        }

        public DelegateCommand AddNetworkAddressCommand { get; private set; }
        public DelegateCommand ClearNetworkAddressCommand { get; private set; }

        private ObservableCollection<IpAddressWrapper> _networkAddresses;
        public ObservableCollection<IpAddressWrapper> NetworkAddresses { get; private set; }

        public List<DeploymentItem> NetworkAddressItems { get; private set; }

        private IpAddressValidator _validator;

        private string _networkAddress;
        public string NetworkAddress
        {
            get { return _networkAddress; }
            set
            {
                _networkAddress = value;
                RaisePropertyChanged(nameof(NetworkAddress));
                AddNetworkAddressCommand.InvalidateCanExecute();
                ErrorMessage = null;
            }
        }

        public string NetworkAddressExample
        {
            get
            {
                return @"Example: 10.0.0.0 or 10.0.0.0/8 or 10.0.0.0-10.255.255.255 or Endpoint name";
            }
        }

        public string NetworkAddressHeader
        {
            get
            {
                if (!HasNetworkAddress)
                    return "Discover computers by specifying IP addresses or endpoint names";

                return string.Join(", ", NetworkAddresses.Where(_ => !string.IsNullOrEmpty(_.Value)).Select(_ => _.Value));
            }
        }

        public bool HasNetworkAddress
        {
            get { return NetworkAddresses.Count > 0 && NetworkAddresses.Any(_ => !string.IsNullOrEmpty(_.Value)); }
        }

        private void AddNetworkAddress(object o)
        {
            Action refresh = () =>
            {
                RaisePropertyChanged(nameof(HasNetworkAddress));

                ErrorMessage = null;
                ChangeScreenCommand.InvalidateCanExecute();
            };

            string validationMessage;
            var wrapper = new IpAddressWrapper
            {
                Value = NetworkAddress,
                ValueChangedAction = refresh,
                RemoveCommand = new DelegateCommand(_ =>
                {
                    NetworkAddresses.Remove(_ as IpAddressWrapper);
                    refresh();
                })
            };

            if (_validator.Validate1(wrapper, out validationMessage) && 
                _validator.Validate2(wrapper, out validationMessage))
            {
                if (NetworkAddresses.Any(_ => _.Value.ToLower() == wrapper.Value.ToLower()))
                {
                    ErrorMessage = "Please add unique network addresses that are not in the list";
                    return;
                }
                NetworkAddresses.Add(wrapper);
                NetworkAddress = null;

                refresh();
            }
            else
            {
                ErrorMessage = validationMessage;
            }
        }

        private bool ValidateNetworkAddresses(out string validationMessage)
        {
            validationMessage = null;
            var values = new List<string>();
            foreach (var wrapper in NetworkAddresses)
            {
                if (!_validator.Validate1(wrapper, out validationMessage) ||
                    !_validator.Validate2(wrapper, out validationMessage))  
                    return false;

                if (!values.Contains(wrapper.Value.ToLower()))
                    values.Add(wrapper.Value.ToLower());
                else
                {
                    validationMessage = "Please add unique network addresses that are not in the list";
                    return false;
                }
            }

            return true;
        }

        private bool CanAddNetworkAddress(object o)
        {
            return !string.IsNullOrEmpty(NetworkAddress);
        }

        private void ClearNetworkAddress(object o)
        {
            if (HasNetworkAddress)
                RestoreNetworkAddressMemento(true);
        }

        private void SaveNetworkAddressMemento()
        {
            _networkAddresses = new ObservableCollection<IpAddressWrapper>();
            foreach (var item in NetworkAddresses)
            {
                string validationMessage;
                var wrapper = new IpAddressWrapper
                {
                    Value = item.Value,
                    ValueChangedAction = item.ValueChangedAction,
                    RemoveCommand = item.RemoveCommand
                };
                if (_validator.Validate1(wrapper, out validationMessage) &&
                    _validator.Validate2(wrapper, out validationMessage))
                    _networkAddresses.Add(wrapper);
            }
        }

        private void RestoreNetworkAddressMemento(bool reset = false)
        {
            NetworkAddresses = !reset ? _networkAddresses : new ObservableCollection<IpAddressWrapper>();
            RaisePropertyChanged(nameof(NetworkAddresses));

            RaisePropertyChanged(nameof(NetworkAddressHeader));
            RaisePropertyChanged(nameof(HasNetworkAddress));
            ChangeScreenCommand.InvalidateCanExecute();
        }
    }
}