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
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
// ReSharper disable ClassWithVirtualMembersNeverInherited.Global

namespace TWCore.Security
{
    /// <summary>
    /// Symetric key encription/decription provider
    /// </summary>
    public class SymmetricKeyProvider
    {
        private readonly SymmetricAlgorithm _alg;

        /// <summary>
        /// Default Salt value
        /// </summary>
        public static readonly byte[] DefaultSalt = { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 };

        #region Properties
        /// <summary>
        /// Name of the algorithm
        /// </summary>
        public AlgorithmName Name { get; private set; }
        /// <summary>
        /// Mode of the algorithm
        /// </summary>
        public AlgorithmMode Mode { get; private set; }
        /// <summary>
        /// Padding of the algorithm
        /// </summary>
        public AlgorithmPadding Padding { get; private set; }
        /// <summary>
        /// Salt to use on key derivation
        /// </summary>
        public byte[] Salt { get; private set; }
        /// <summary>
        /// Encoding to use when converting string to bytes and viceversa
        /// </summary>
        public Encoding Encoding { get; set; } = Encoding.UTF8;
        #endregion

        #region .ctor
        /// <summary>
        /// Symetric key encription/decription provider
        /// </summary>
        /// <param name="name">Name of the algorithm</param>
        /// <param name="mode">Mode of the algorithm</param>
        /// <param name="padding">Padding of the algorithm</param>
        /// <param name="salt">Salt to use on key derivation</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SymmetricKeyProvider(AlgorithmName name = AlgorithmName.Aes, AlgorithmMode mode = AlgorithmMode.Cbc, AlgorithmPadding padding = AlgorithmPadding.Pkcs7, byte[] salt = null)
        {
            Name = name;
            Mode = mode;
            Padding = padding;
            Salt = salt ?? DefaultSalt;
            switch (Name)
            {
                case AlgorithmName.Aes:
                    _alg = Aes.Create();
                    break;
                case AlgorithmName.TripleDes:
                    _alg = TripleDES.Create();
                    break;
            }
            switch (Mode)
            {
                case AlgorithmMode.Cbc:
                    _alg.Mode = CipherMode.CBC;
                    break;
                case AlgorithmMode.Ecb:
                    _alg.Mode = CipherMode.ECB;
                    break;
            }
            switch (Padding)
            {
                case AlgorithmPadding.None:
                    _alg.Padding = PaddingMode.None;
                    break;
                case AlgorithmPadding.Pkcs7:
                    _alg.Padding = PaddingMode.PKCS7;
                    break;
                case AlgorithmPadding.Zeros:
                    _alg.Padding = PaddingMode.Zeros;
                    break;
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Performs an encryption process on the data using a password
        /// </summary>
        /// <param name="data">Data to be encrypted</param>
        /// <param name="password">Password to encrypt the data</param>
        /// <returns>Data encrypted</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string Encrypt(string data, string password) => Convert.ToBase64String(Encrypt(Encoding.GetBytes(data), Encoding.GetBytes(password)));
        /// <summary>
        /// Performs an encryption process on the data using a password
        /// </summary>
        /// <param name="data">Data to be encrypted</param>
        /// <param name="password">Password to encrypt the data</param>
        /// <returns>Data encrypted</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string Encrypt(string data, byte[] password) => Convert.ToBase64String(Encrypt(Encoding.GetBytes(data), password));
        /// <summary>
        /// Performs an encryption process on the data using a password
        /// </summary>
        /// <param name="data">Data to be encrypted</param>
        /// <param name="password">Password to encrypt the data</param>
        /// <returns>Data encrypted</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[] Encrypt(byte[] data, string password) => Encrypt(data, Encoding.GetBytes(password));
        /// <summary>
        /// Performs an encryption process on the data using a password
        /// </summary>
        /// <param name="data">Data to be encrypted</param>
        /// <param name="password">Password to encrypt the data</param>
        /// <returns>Data encrypted</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual byte[] Encrypt(byte[] data, byte[] password)
        {
            var pdb = new Rfc2898DeriveBytes(password, Salt, 1000);
            using (var ms = new MemoryStream())
            {
                lock (_alg)
                {
                    _alg.Key = pdb.GetBytes(32);
                    _alg.IV = pdb.GetBytes(16);
                    using (var cs = new CryptoStream(ms, _alg.CreateEncryptor(), CryptoStreamMode.Write))
                        cs.Write(data, 0, data.Length);
                }
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Performs a decryption process on the data using a password
        /// </summary>
        /// <param name="data">Data to be decrypted</param>
        /// <param name="password">Password to decrypt the data</param>
        /// <returns>Data decrypted</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string Decrypt(string data, string password) => GetString(Decrypt(Convert.FromBase64String(data), Encoding.GetBytes(password)));
        /// <summary>
        /// Performs a decryption process on the data using a password
        /// </summary>
        /// <param name="data">Data to be decrypted</param>
        /// <param name="password">Password to decrypt the data</param>
        /// <returns>Data decrypted</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string Decrypt(string data, byte[] password) => GetString(Decrypt(Convert.FromBase64String(data), password));
        /// <summary>
        /// Performs a decryption process on the data using a password
        /// </summary>
        /// <param name="data">Data to be decrypted</param>
        /// <param name="password">Password to decrypt the data</param>
        /// <returns>Data decrypted</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[] Decrypt(byte[] data, string password) => Decrypt(data, Encoding.GetBytes(password));
        /// <summary>
        /// Performs a decryption process on the data using a password
        /// </summary>
        /// <param name="data">Data to be decrypted</param>
        /// <param name="password">Password to decrypt the data</param>
        /// <returns>Data decrypted</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual byte[] Decrypt(byte[] data, byte[] password)
        {
            var pdb = new Rfc2898DeriveBytes(password, Salt, 1000);
            using (var ms = new MemoryStream())
            {
                lock (_alg)
                {
                    _alg.Key = pdb.GetBytes(32);
                    _alg.IV = pdb.GetBytes(16);
                    using (var cs = new CryptoStream(ms, _alg.CreateDecryptor(), CryptoStreamMode.Write))
                        cs.Write(data, 0, data.Length);
                }
                return ms.ToArray();
            }
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Get the string value from a byte array using the encoding
        /// </summary>
        /// <param name="data">Data to be converted</param>
        /// <returns>String value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string GetString(byte[] data) => Encoding.GetString(data);
        #endregion

        #region Inner Enums
        /// <summary>
        /// Enumeration describing symmetric algorithms names
        /// </summary>
        public enum AlgorithmName
        {
            /// <summary>
            /// AES algorithm
            /// </summary>
            Aes = 0,
            /// <summary>
            /// The TRIPLEDES algorithm.
            /// </summary>
            TripleDes = 2,
        }

        /// <summary>
        /// Enumeration describing symmethic cipher modes.
        /// </summary>
        public enum AlgorithmMode
        {
            /// <summary>
            /// The CBC mode.
            /// </summary>
            Cbc = 0,
            /// <summary>
            /// The ECB mode.
            /// </summary>
            Ecb = 1
        }

        /// <summary>
        /// Enumeration to describe cipher block padding options
        /// </summary>
        public enum AlgorithmPadding
        {
            /// <summary>
            /// Use no padding at all.
            /// </summary>
            None = 0,
            /// <summary>
            /// The PKCS #7 padding string consists of a sequence of bytes, each of which is 
            /// equal to the total number of padding bytes added.
            /// </summary>
            Pkcs7 = 1,
            /// <summary>
            /// The padding string consists of bytes set to zero.
            /// </summary>
            Zeros = 2
        }
        #endregion
    }
}
