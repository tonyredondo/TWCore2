/*
Copyright 2015-2018 Daniel Adrian Redondo Suarez

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
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TWCore.Settings;
using TWCore.Threading;
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global

namespace TWCore.Diagnostics.Log.Storages
{
    /// <inheritdoc cref="ILogStorage" />
    /// <summary>
    /// Sends the Logs items via email using SMTP
    /// </summary>
    [SettingsContainer("MailLogStorage")]
    public class MailLogStorage: SettingsBase, ILogStorage
    {
        private readonly List<ILogItem> _buffer = new List<ILogItem>();
        private static bool _waiting;
        private static SmtpClient _smtp;

        #region Settings
        /// <summary>
        /// SMTP Host
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// SMTP Port
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Use default SMTP Credentials
        /// </summary>
        public bool UseDefaultCredentials { get; set; }

        /// <summary>
        /// SMTP Credentials : User
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// SMTP Credentials : Password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Use SSL
        /// </summary>
        public bool UseSSL { get; set; }

        /// <summary>
        /// SMTP Timeout
        /// </summary>
        public int Timeout { get; set; }

        /// <summary>
        /// SMTP Timeout
        /// </summary>
        public int BufferTimeoutInSeconds { get; set; } = 60;

        /// <summary>
        /// Log Level to send to tracked chats
        /// </summary>
        public LogLevel LevelAllowed { get; set; } = LogLevel.Error;

        /// <summary>
        /// AddressFromDisplay
        /// </summary>
        public string AddressFromDisplay { get; set; }
        /// <summary>
        /// AddressFrom
        /// </summary>
        public string AddressFrom { get; set; }
        /// <summary>
        /// AddressBcc
        /// </summary>
        public string AddressBcc { get; set; }
        /// <summary>
        /// AddressCc
        /// </summary>
        public string AddressCc { get; set; }
        /// <summary>
        /// AddressTo
        /// </summary>
        public string AddressTo { get; set; }

        #endregion

        #region .ctor
        /// <inheritdoc />
        /// <summary>
        /// Sends the Logs items via email using SMTP
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MailLogStorage()
        {
            try 
            {
                SetSMTP(Host, Port, UseDefaultCredentials, User, Password, UseSSL, Timeout);
            }
            catch(Exception)
            {
                _smtp = null;
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Allow change the smtp config
        /// </summary>
        /// <param name="host">Smtp server Host</param>
        /// <param name="port">Smtp server Port</param>
        /// <param name="useDefaultCredentials">Smtp server use default creadentials</param>
        /// <param name="user">Smtp server Username</param>
        /// <param name="password">Smtp server Password</param>
        /// <param name="useSSL">Smtp server use SSL</param>
        /// <param name="timeout">Smtp server Timeout</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetSMTP(string host, int port, bool useDefaultCredentials, string user = null, string password = null, bool useSSL = false, int timeout = 30)
        {
            if (!_waiting)
            {
                _smtp = new SmtpClient
                {
                    DeliveryFormat = SmtpDeliveryFormat.International,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Host = host?.Trim(),
                    Port = port,
                    UseDefaultCredentials = useDefaultCredentials
                };
                if (!useDefaultCredentials)
                    _smtp.Credentials = new NetworkCredential(user?.Trim(), password?.Trim());
                _smtp.EnableSsl = useSSL;
                _smtp.Timeout = (int)TimeSpan.FromSeconds(timeout).TotalMilliseconds;
                Core.Log.InfoDetail("SMTP changed sucessfully");
            }
            else
                Core.Log.InfoBasic("Couldn't change the SMTP value because its in use");
        }
        /// <summary>
        /// Allow change the smtp config
        /// </summary>
        /// <param name="smtp"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetSMTP(SmtpClient smtp)
        {
            if (!_waiting)
            {
                _smtp = smtp;
                Core.Log.InfoDetail("SMTP changed sucessfully");
            }
            else
                Core.Log.InfoBasic("Couldn't change the SMTP value because its in use");
        }

        /// <inheritdoc />
        /// <summary>
        /// Writes a log item empty line
        /// </summary>
        public Task WriteEmptyLineAsync()
        {
            return Task.CompletedTask;
        }
        /// <inheritdoc />
        /// <summary>
        /// Writes a log item to the storage
        /// </summary>
        /// <param name="item">Log Item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task WriteAsync(ILogItem item)
        {
            if (!LevelAllowed.HasFlag(item.Level) || item.Message.Contains("SMTPERROR")) return Task.CompletedTask;

            lock (_buffer)
                _buffer.Add(item);
            if (_waiting) return Task.CompletedTask;
            _waiting = true;
            Task.Delay(BufferTimeoutInSeconds * 1000).ContinueWith(t =>
            {
                SendEmail();
            });
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        /// <summary>
        /// Dispose the current object resources
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            SendEmail();
            _buffer.Clear();
            _smtp.Dispose();
        }
        #endregion

        #region Private Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SendEmail()
        {
            var message = new MailMessage
            {
                BodyEncoding = Encoding.UTF8,
                SubjectEncoding = Encoding.UTF8
            };

            var addFDisplay = AddressFromDisplay?.Trim();
            message.From = string.IsNullOrEmpty(addFDisplay) ? 
                new MailAddress(AddressFrom?.Trim()) : 
                new MailAddress(AddressFrom?.Trim(), addFDisplay);

            AddressBcc = AddressBcc?.Replace(",", ";").Replace("|", ";");
            AddressCc = AddressCc?.Replace(",", ";").Replace("|", ";");
            AddressTo = AddressTo?.Replace(",", ";").Replace("|", ";");

            AddressBcc.SplitAndTrim(';').Each(a => { if (!string.IsNullOrEmpty(a)) { message.Bcc.Add(new MailAddress(a)); } });
            AddressCc.SplitAndTrim(';').Each(a => { if (!string.IsNullOrEmpty(a)) { message.CC.Add(new MailAddress(a)); } });
            AddressTo.SplitAndTrim(';').Each(a => { if (!string.IsNullOrEmpty(a)) { message.To.Add(new MailAddress(a)); } });

            var msg = string.Empty;

            lock (_buffer)
            {
                var first = _buffer.First();
                message.Subject = _buffer.Count == 1 ? 
                    string.Format("{0} {1} {2}: {3}", first.Timestamp.ToString("dd/MM/yyyy HH:mm:ss"), first.MachineName, first.ApplicationName, first.Message) : 
                    string.Format("{0} {1} {2}: Messages", first.Timestamp.ToString("dd/MM/yyyy HH:mm:ss"), first.MachineName, first.ApplicationName);
                foreach (var innerItem in _buffer)
                {
                    msg += string.Format("{0}\r\nMachine Name: {1} [{2}]\r\nAplicationName: {3}\r\nMessage: {4}", innerItem.Timestamp.ToString("dd/MM/yyyy HH:mm:ss"), innerItem.MachineName, innerItem.EnvironmentName, innerItem.ApplicationName, innerItem.Message);
                    if (innerItem.Exception != null)
                    {
                        if (!string.IsNullOrEmpty(innerItem.Exception.ExceptionType))
                            msg += "\r\nException: " + innerItem.Exception.ExceptionType;
                        if (!string.IsNullOrEmpty(innerItem.Exception.StackTrace))
                            msg += "\r\nStack Trace: " + innerItem.Exception.StackTrace;
                    }
                    if (innerItem != _buffer.Last())
                        msg += "\r\n-------------------------------\r\n";
                }
                _buffer.Clear();
            }
            message.Body = msg;

            if (_smtp == null) return;
            try
            {
                _smtp.Send(message);
            }
            catch (Exception e)
            {
                Core.Log.Error("SMTPERROR: {0}", e.Message);
            }
            finally
            {
                _waiting = false;
            }
        }
        #endregion
    }
}