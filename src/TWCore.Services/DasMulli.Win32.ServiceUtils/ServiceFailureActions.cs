using System;
using System.Collections.Generic;
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable CheckNamespace

namespace DasMulli.Win32.ServiceUtils
{
    /// <inheritdoc />
    /// <summary>
    /// A managed class that holds data referring to a <see cref="T:DasMulli.Win32.ServiceUtils.ServiceFailureActionsInfo" /> class which has unmanaged resources
    /// </summary>
    public class ServiceFailureActions : IEquatable<ServiceFailureActions>
    {
        #region Properties
        /// <summary>
        /// Reset period
        /// </summary>
        public TimeSpan ResetPeriod { get; }
        /// <summary>
        /// Reboot message
        /// </summary>
        public string RebootMessage { get; }
        /// <summary>
        /// Restart command
        /// </summary>
        public string RestartCommand { get; }
        /// <summary>
        /// Actions
        /// </summary>
        public IReadOnlyCollection<ScAction> Actions { get; }
        #endregion

        #region .ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceFailureActions"/> class.
        /// </summary>
        public ServiceFailureActions(TimeSpan resetPeriod, string rebootMessage, string restartCommand, IReadOnlyCollection<ScAction> actions)
        {
            ResetPeriod = resetPeriod;
            RebootMessage = rebootMessage;
            RestartCommand = restartCommand;
            Actions = actions;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Checks if the ServiceFailureActions is equals to other instance
        /// </summary>
        /// <param name="obj">Other object instance</param>
        /// <returns>True if both actions are equals; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is ServiceFailureActions srvFailure && Equals(srvFailure);
        }

        /// <summary>
        /// Checks if the ServiceFailureActions is equals to other instance
        /// </summary>
        /// <param name="other">Other ServiceFailureActions instance</param>
        /// <returns>True if both actions are equals; otherwise, false.</returns>
        public bool Equals(ServiceFailureActions other)
        {
            if (other is null)
                return false;
            return GetHashCode() == other.GetHashCode();
        }

        /// <summary>
        /// Gets the hashcode of the instance
        /// </summary>
        /// <returns>Hashcode</returns>
        public override int GetHashCode()
        {
            return HashCode
                .Of(ResetPeriod)
                .And(RebootMessage)
                .And(RestartCommand)
                .AndEach(Actions);
        }
        #endregion
    }
}