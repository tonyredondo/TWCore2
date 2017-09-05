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
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using TWCore.Collections;
using TWCore.Serialization;

namespace TWCore
{
    /// <summary>
    /// Serializable exception
    /// </summary>
    [Serializable, DataContract]
    public class SerializableException
    {
        #region Properties
        /// <summary>
        /// Gets the original exception type
        /// </summary>
        [XmlAttribute, DataMember]
        public string ExceptionType { get; set; }
        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        [XmlAttribute, DataMember]
        public string Message { get; set; }
        /// <summary>
        /// Gets or sets a link to the help file associated with this exception.
        /// </summary>
        [XmlAttribute, DataMember]
        public string HelpLink { get; set; }
        /// <summary>
        /// Gets or sets HRESULT, a coded numerical value that is assigned to a specific exception.
        /// </summary>
        [XmlAttribute, DataMember]
        public int HResult { get; set; }
        /// <summary>
        /// Gets or sets the name of the application or the object that causes the error.
        /// </summary>
        [XmlAttribute, DataMember]
        public string Source { get; set; }
        /// <summary>
        /// Gets a string representation of the immediate frames on the call stack.
        /// </summary>
        [XmlAttribute, DataMember]
        public string StackTrace { get; set; }
        /// <summary>
        /// Gets a collection of key/value pairs that provide additional user-defined information about the exception
        /// </summary>
        [XmlElement, DataMember]
        public KeyValueCollection Data { get; set; }
        /// <summary>
        /// Gets the System.Exception instance that caused the current exception.
        /// </summary>
        [XmlElement, DataMember]
        public SerializableException InnerException { get; set; }
        #endregion

        #region .ctor
        /// <summary>
        /// Serializable exception
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SerializableException() { }
        /// <summary>
        /// Serializable exception
        /// </summary>
        /// <param name="ex">Original exception</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SerializableException(Exception ex)
        {
            HelpLink = ex.HelpLink;
            HResult = ex.HResult;
            Message = ex.Message?.RemoveInvalidXmlChars();
            Source = ex.Source;
            StackTrace = ex.StackTrace;
            ExceptionType = ex.GetType().FullName;
            if (ex.InnerException != null)
                InnerException = new SerializableException(ex.InnerException);

            if (ex.Data == null) return;
            
            Data = new KeyValueCollection();
            foreach (var key in ex.Data.Keys)
            {
                if (!(key is string) && key.GetType().GetTypeInfo().IsClass) continue;
                    
                var strKey = key.ToString();
                string strValue;
                try
                {
                    strValue = SerializerManager.DefaultTextSerializer.SerializeToString(ex.Data[key], ex.Data[key].GetType())?.RemoveInvalidXmlChars();
                }
                catch (Exception sEx)
                {
                    strValue = sEx.Message?.RemoveInvalidXmlChars();
                }
                if (!Data.Contains(strKey))
                    Data.Add(strKey, strValue);
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Gets a Wrapped exception of the serializable one.
        /// </summary>
        /// <returns>WrappedException object</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WrappedException GetException() => new WrappedException(this);
        #endregion

        /// <inheritdoc />
        /// <summary>
        /// Wrapper exception for a serializable exception object
        /// </summary>
        public class WrappedException : Exception
        {
            #region Properties
            /// <summary>
            /// Wrapped serializable exception
            /// </summary>
            public readonly SerializableException SerializableException;
            /// <inheritdoc />
            /// <summary>
            /// Gets a collection of key/value pairs that provide additional user-defined information about the exception
            /// </summary>
            public override IDictionary Data => SerializableException?.Data?.ToDictionary(k => k.Key, v => v.Value);
            /// <inheritdoc />
            /// <summary>
            /// Gets or sets a link to the help file associated with this exception.
            /// </summary>
            public override string HelpLink
            {
                get => SerializableException.HelpLink;
                set => SerializableException.HelpLink = value;
            }
            /// <summary>
            /// Gets or sets HRESULT, a coded numerical value that is assigned to a specific exception.
            /// </summary>
            public new int HResult
            {
                get => SerializableException.HResult;
                protected set => SerializableException.HResult = value;
            }
            /// <summary>
            /// Gets the System.Exception instance that caused the current exception.
            /// </summary>
            public new Exception InnerException => SerializableException.InnerException != null ? new WrappedException(SerializableException.InnerException) : null;
            /// <inheritdoc />
            /// <summary>
            /// Gets a message that describes the current exception.
            /// </summary>
            public override string Message => SerializableException.Message;
            /// <inheritdoc />
            /// <summary>
            /// Gets or sets the name of the application or the object that causes the error.
            /// </summary>
            public override string Source
            {
                get => SerializableException.Source;
                set => SerializableException.Source = value;
            }
            /// <inheritdoc />
            /// <summary>
            /// Gets a string representation of the immediate frames on the call stack.
            /// </summary>
            public override string StackTrace => SerializableException.StackTrace;
            #endregion

            #region .ctor
            /// <inheritdoc />
            /// <summary>
            /// Wrapper exception for a serializable exception object
            /// </summary>
            /// <param name="exception">Serializable exception to unwrap</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public WrappedException(SerializableException exception)
            {
                SerializableException = exception;
            }
            #endregion
        }
    }
}
