/*
Copyright 2015-2017 Daniel Adrian Redondo Suarez

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
 */

using System;
using TWCore.Diagnostics.Status;
// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable IntroduceOptionalParameters.Global
// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace TWCore.Services
{
    /// <inheritdoc />
    /// <summary>
    /// Service Wrapper
    /// </summary>
    public class ServiceWrapper : IService
    {
        private ServiceStatus _serviceStatus;
        private string[] _args;

        #region Properties
        /// <summary>
        /// Service
        /// </summary>
        public IService Service { get; }
        /// <summary>
        /// Service Name
        /// </summary>
        public string ServiceName { get; }
        #endregion

        #region .ctor
        /// <summary>
        /// Service Wrapper
        /// </summary>
        /// <param name="service">Service</param>
        /// <param name="serviceName">Service name</param>
        public ServiceWrapper(IService service, string serviceName)
        {
            Service = service;
            ServiceName = serviceName;
            Core.Data[nameof(ServiceWrapper)] = this;
        }
        ~ServiceWrapper()
        {
            //Console.WriteLine("SERVICE WRAPPER HAS BEEN DISPOSED!!!");
        }
        #endregion

        #region Static Method
        private void RegisterStatus()
        {
            Core.Status.Attach(() =>
            {
                var sItem = new StatusItem { Name = "Application Information\\" };
                var siv = StatusItemValueStatus.Unknown;
                switch (_serviceStatus)
                {
                    case ServiceStatus.Starting:
                        siv = StatusItemValueStatus.Warning;
                        break;
                    case ServiceStatus.Running:
                        siv = StatusItemValueStatus.Ok;
                        break;
                    case ServiceStatus.Stopping:
                        siv = StatusItemValueStatus.Warning;
                        break;
                    case ServiceStatus.Stopped:
                        siv = StatusItemValueStatus.Error;
                        break;
                    case ServiceStatus.Pausing:
                        siv = StatusItemValueStatus.Warning;
                        break;
                    case ServiceStatus.Paused:
                        siv = StatusItemValueStatus.Error;
                        break;
                    case ServiceStatus.Continuing:
                        siv = StatusItemValueStatus.Warning;
                        break;
                }
                sItem.Values.Add("Service Information",
                    new StatusItemValueItem("Status", _serviceStatus, siv, false),
                    new StatusItemValueItem("Can Pause and Continue", CanPauseAndContinue ? "Yes" : "No", CanPauseAndContinue ? StatusItemValueStatus.Ok : StatusItemValueStatus.Error, false),
                    new StatusItemValueItem("Arguments", string.Join(" ", _args ?? new string[0]))
                );
                return sItem;
            });
        }
        #endregion

        #region IService
        /// <inheritdoc />
        /// <summary>
        /// Get if the service support pause and continue
        /// </summary>
        public bool CanPauseAndContinue => Service.CanPauseAndContinue;
        /// <inheritdoc />
        /// <summary>
        /// On Service Start method
        /// </summary>
        /// <param name="args">Start arguments</param>
        public void OnStart(string[] args)
        {
            RegisterStatus();
            try
            {
                _serviceStatus = ServiceStatus.Starting;
                _args = args;
                Service.OnStart(args);
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
            }
            finally
            {
                _serviceStatus = ServiceStatus.Running;
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// On Service Stops method
        /// </summary>
        public void OnStop()
        {
            try
            {
                _serviceStatus = ServiceStatus.Stopping;
                Service.OnStop();
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
            }
            finally
            {
                _serviceStatus = ServiceStatus.Stopped;
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// On Continue from pause method
        /// </summary>
        public void OnContinue()
        {
            try
            {
                _serviceStatus = ServiceStatus.Continuing;
                Service.OnContinue();
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
            }
            finally
            {
                _serviceStatus = ServiceStatus.Running;
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// On Pause method
        /// </summary>
        public void OnPause()
        {
            try
            {
                _serviceStatus = ServiceStatus.Pausing;
                Service.OnPause();
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
            }
            finally
            {
                _serviceStatus = ServiceStatus.Paused;
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// On shutdown requested method
        /// </summary>
        public void OnShutdown()
        {
            try
            {
                _serviceStatus = ServiceStatus.Stopping;
                Service.OnShutdown();
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
            }
            finally
            {
                _serviceStatus = ServiceStatus.Stopped;
            }
        }
        #endregion

        #region Nested Types
        /// <summary>
        /// Service Status
        /// </summary>
        public enum ServiceStatus
        {
            Starting,
            Running,
            Stopping,
            Stopped,
            Pausing,
            Paused,
            Continuing
        }
        #endregion
    }

}
