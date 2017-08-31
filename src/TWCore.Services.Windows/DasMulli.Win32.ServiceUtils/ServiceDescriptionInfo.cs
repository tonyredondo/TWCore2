using System.Runtime.InteropServices;

namespace DasMulli.Win32.ServiceUtils
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct ServiceDescriptionInfo
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        private string serviceDescription;

        public ServiceDescriptionInfo(string serviceDescription)
        {
            this.serviceDescription = serviceDescription;
        }

        public string ServiceDescription
        {
            get { return serviceDescription; }
            set { serviceDescription = value; }
        }
    }
}