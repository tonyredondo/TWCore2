using System;

// ReSharper disable CheckNamespace

namespace DasMulli.Win32.ServiceUtils
{
    internal delegate void ServiceControlHandler(ServiceControlCommand control, uint eventType, IntPtr eventData, IntPtr eventContext);
}