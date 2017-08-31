using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace DasMulli.Win32.ServiceUtils
{
    internal class ServiceControlManager : SafeHandle
    {
        internal INativeInterop NativeInterop { get; set; } = Win32Interop.Wrapper;

        internal ServiceControlManager() : base(IntPtr.Zero, ownsHandle: true)
        {
        }

        protected override bool ReleaseHandle()
        {
            return NativeInterop.CloseServiceHandle(handle);
        }

        public override bool IsInvalid
        {
            [System.Security.SecurityCritical]
            get
            {
                return handle == IntPtr.Zero;
            }
        }

        internal static ServiceControlManager Connect(INativeInterop nativeInterop, string machineName, string databaseName, ServiceControlManagerAccessRights desiredAccessRights)
        {
            var mgr = nativeInterop.OpenSCManagerW(machineName, databaseName, desiredAccessRights);

            mgr.NativeInterop = nativeInterop;

            if (mgr.IsInvalid)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            return mgr;
        }

        public ServiceHandle CreateService(string serviceName, string displayName, string binaryPath, ServiceType serviceType, ServiceStartType startupType, ErrorSeverity errorSeverity, Win32ServiceCredentials credentials)
        {
            var service = NativeInterop.CreateServiceW(this, serviceName, displayName, ServiceControlAccessRights.All, serviceType, startupType, errorSeverity,
                binaryPath, null,
                IntPtr.Zero, null, credentials.UserName, credentials.Password);

            service.NativeInterop = NativeInterop;

            if (service.IsInvalid)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            
            return service;
        }

        public ServiceHandle OpenService(string serviceName, ServiceControlAccessRights desiredControlAccess)
        {
            if (!TryOpenService(serviceName, desiredControlAccess, out ServiceHandle service, out Win32Exception errorException))
            {
                throw errorException;
            }
            return service;
        }

        public virtual bool TryOpenService(string serviceName, ServiceControlAccessRights desiredControlAccess, out ServiceHandle serviceHandle, out Win32Exception errorException)
        {
            var service = NativeInterop.OpenServiceW(this, serviceName, desiredControlAccess);

            service.NativeInterop = NativeInterop;

            if (service.IsInvalid)
            {
                errorException = new Win32Exception(Marshal.GetLastWin32Error());
                serviceHandle = null;
                return false;
            }

            serviceHandle = service;
            errorException = null;
            return true;
        }
    }
}