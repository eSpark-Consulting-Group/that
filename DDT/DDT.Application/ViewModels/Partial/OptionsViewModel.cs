using System;
using DDT.Application.Transitions;

namespace DDT.Application.ViewModels
{
    public partial class MainViewModel
    {
        public bool? DeployTo
        {
            get
            {
                if (DeployToWindowsServers.GetValueOrDefault() && 
                    DeployToWindowsWorkstations.GetValueOrDefault() && 
                    DeployTox86.GetValueOrDefault() && 
                    DeployTox64.GetValueOrDefault())
                    return true;
                if (!DeployToWindowsServers.GetValueOrDefault() && 
                    !DeployToWindowsWorkstations.GetValueOrDefault() && 
                    !DeployTox86.GetValueOrDefault() && 
                    !DeployTox64.GetValueOrDefault())
                    return false;

                return null;
            }
            set
            {
                DeployToWindowsServers = DeployToWindowsWorkstations = DeployTox86 = DeployTox64 = value;
            }
        }

        public bool? _deployToWindowsServers = false;
        public bool? DeployToWindowsServers
        {
            get { return _deployToWindowsServers; }
            set
            {
                _deployToWindowsServers = value;
                RaisePropertyChanged(nameof(DeployToWindowsServers));
                RaisePropertyChanged(nameof(DeployTo));
            }
        }

        public bool? _deployToWindowsWorkstations = true;
        public bool? DeployToWindowsWorkstations
        {
            get { return _deployToWindowsWorkstations; }
            set
            {
                _deployToWindowsWorkstations = value;
                RaisePropertyChanged(nameof(DeployToWindowsWorkstations));
                RaisePropertyChanged(nameof(DeployTo));
            }
        }

        public bool? _deployTox86 = true;
        public bool? DeployTox86
        {
            get { return _deployTox86; }
            set
            {
                _deployTox86 = value;
                RaisePropertyChanged(nameof(DeployTox86));
                RaisePropertyChanged(nameof(DeployTo));
            }
        }

        public bool? _deployTox64 = true;
        public bool? DeployTox64
        {
            get { return _deployTox64; }
            set
            {
                _deployTox64 = value;
                RaisePropertyChanged(nameof(DeployTox64));
                RaisePropertyChanged(nameof(DeployTo));
            }
        }

        public int DeployTimeout { get; set; } = 20; 

        private Uri _darkThemeUri = new Uri("/DDT.Application;component/Themes/DarkCyan.xaml", UriKind.Relative);
        private Uri _lightThemeUri = new Uri("/DDT.Application;component/Themes/LightCyan.xaml", UriKind.Relative);
        private Uri _light2ThemeUri = new Uri("/DDT.Application;component/Themes/LightBlue.xaml", UriKind.Relative);

        public Uri Theme
        {
            get
            {
                return _darkCyanTheme ? _darkThemeUri : (_lightCyanTheme ? _lightThemeUri : _light2ThemeUri);
            }
        }

        private bool _darkCyanTheme;
        public bool DarkCyanTheme
        {
            get { return _darkCyanTheme; }
            set
            {
                _darkCyanTheme = true;
                _lightCyanTheme = false;
                _lightBlueTheme = false;
                RaisePropertyChanged(nameof(DarkCyanTheme));
                RaisePropertyChanged(nameof(LightCyanTheme));
                RaisePropertyChanged(nameof(LightBlueTheme));
                RaisePropertyChanged(nameof(Theme));
            }
        }

        private bool _lightCyanTheme;
        public bool LightCyanTheme
        {
            get { return _lightCyanTheme; }
            set
            {
                _darkCyanTheme = false;
                _lightCyanTheme = true;
                _lightBlueTheme = false;
                RaisePropertyChanged(nameof(DarkCyanTheme));
                RaisePropertyChanged(nameof(LightCyanTheme));
                RaisePropertyChanged(nameof(LightBlueTheme));
                RaisePropertyChanged(nameof(Theme));
            }
        }

        private bool _lightBlueTheme = true;
        public bool LightBlueTheme
        {
            get { return _lightBlueTheme; }
            set
            {
                _darkCyanTheme = false;
                _lightCyanTheme = false;
                _lightBlueTheme = true;
                RaisePropertyChanged(nameof(DarkCyanTheme));
                RaisePropertyChanged(nameof(LightCyanTheme));
                RaisePropertyChanged(nameof(LightBlueTheme));
                RaisePropertyChanged(nameof(Theme));
            }
        }

        private bool _showAnimations = true;
        public bool ShowAnimations
        {
            get { return _showAnimations; }
            set
            {
                _showAnimations = value;
                TransitionManager.CurrentStrategy = TransitionManager.AvailableStrategies[value ? 1 : 0];
            }
        }
    }
}
