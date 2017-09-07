﻿/*
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

namespace TWCore.Services
{
    /// <inheritdoc />
    /// <summary>
    /// Service Wrapper
    /// </summary>
    public class ServiceWrapper : IService
    {
        private ServiceStatus _serviceStatus;

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
            Core.Status.Attach(() =>
            {
                var sItem = new StatusItem { Name = "Process Information - Service" };
                var siv = StatusItemValueStatus.Unknown;
                switch (_serviceStatus)
                {
                    case ServiceStatus.Starting:
                        siv = StatusItemValueStatus.Yellow;
                        break;
                    case ServiceStatus.Running:
                        siv = StatusItemValueStatus.Green;
                        break;
                    case ServiceStatus.Stopping:
                        siv = StatusItemValueStatus.Yellow;
                        break;
                    case ServiceStatus.Stopped:
                        siv = StatusItemValueStatus.Red;
                        break;
                    case ServiceStatus.Pausing:
                        siv = StatusItemValueStatus.Yellow;
                        break;
                    case ServiceStatus.Paused:
                        siv = StatusItemValueStatus.Red;
                        break;
                    case ServiceStatus.Continuing:
                        siv = StatusItemValueStatus.Yellow;
                        break;
                }
                sItem.Values.Add("Service Name", ServiceName);
                sItem.Values.Add("Service can Pause and Continue", CanPauseAndContinue ? "Yes" : "No", CanPauseAndContinue ? StatusItemValueStatus.Green : StatusItemValueStatus.Red);
                sItem.Values.Add("Service Status", _serviceStatus, siv);
                sItem.Values.Add("Service Start Arguments", string.Join(" ", args ?? new string[0]));
                return sItem;
            });

            try
            {
                _serviceStatus = ServiceStatus.Starting;
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
            Continuing,
        }
        #endregion
    }

}