using System.Runtime.InteropServices;
// ReSharper disable ConvertToAutoProperty

// ReSharper disable CheckNamespace

namespace DasMulli.Win32.ServiceUtils
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct ServiceStatus
    {
        private ServiceType serviceType;
        private ServiceState state;
        private ServiceAcceptedControlCommandsFlags acceptedControlCommands;
        private int win32ExitCode;
        private uint serviceSpecificExitCode;
        private uint checkPoint;
        private uint waitHint;

        public ServiceType ServiceType
        {
            get => serviceType;
            set => serviceType = value;
        }

        public ServiceState State
        {
            get => state;
            set => state = value;
        }

        public ServiceAcceptedControlCommandsFlags AcceptedControlCommands
        {
            get => acceptedControlCommands;
            set => acceptedControlCommands = value;
        }

        public int Win32ExitCode
        {
            get => win32ExitCode;
            set => win32ExitCode = value;
        }

        public uint ServiceSpecificExitCode
        {
            get => serviceSpecificExitCode;
            set => serviceSpecificExitCode = value;
        }

        public uint CheckPoint
        {
            get => checkPoint;
            set => checkPoint = value;
        }

        public uint WaitHint
        {
            get => waitHint;
            set => waitHint = value;
        }

        public ServiceStatus(ServiceType serviceType, ServiceState state, ServiceAcceptedControlCommandsFlags acceptedControlCommands, int win32ExitCode, uint serviceSpecificExitCode, uint checkPoint, uint waitHint)
        {
            this.serviceType = serviceType;
            this.state = state;
            this.acceptedControlCommands = acceptedControlCommands;
            this.win32ExitCode = win32ExitCode;
            this.serviceSpecificExitCode = serviceSpecificExitCode;
            this.checkPoint = checkPoint;
            this.waitHint = waitHint;
        }
    }
}