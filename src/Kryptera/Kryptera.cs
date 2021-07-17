using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Kryptera.Tools")]

namespace Kryptera
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using CryptHash.Net.Encryption.AES.AEAD;
    using CryptHash.Net.Encryption.AES.EncryptionResults;
    using CryptHash.Net.Util;

    /// <summary>
    ///     A utility class containing useful methods for rapidly encrypting and decrypting strings and files using AES with a
    ///     256 bits key in GCM authenticated mode.
    /// </summary>
    public static class Kryptera
    {
        private static Lazy<AEAD_AES_256_GCM> AesAlgorithm => new(() => new AEAD_AES_256_GCM());

        /// <summary>
        ///     Decrypts a base64 encoded input string using AES with a 256 bits key in GCM authenticated mode, deriving the key
        ///     from a provided password string.
        /// </summary>
        /// <param name="base64EncryptedString">The base64 encoded input string to decrypt.</param>
        /// <param name="password">The password string where the encryption key will be derived from.</param>
        /// <returns>A string containing the decrypted contents from <paramref name="base64EncryptedString" />.</returns>
        public static string Decrypt(string base64EncryptedString, string password)
        {
            var result = AesAlgorithm.Value.DecryptString(base64EncryptedString, password);
            return result.DecryptedDataString;
        }

        /// <summary>
        ///     Asynchronously opens a file, reads and decrypts the contents of the file into a string, and then closes the file.
        /// </summary>
        /// <param name="path">The file to open for reading.</param>
        /// <param name="password">The password string where the encryption key will be derived from.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A string containing the decrypted contents of the file.</returns>
        public static async Task<string> DecryptBase64FileAsync(string path, string password,
            CancellationToken cancellationToken = default)
        {
            return await DecryptBase64FileAsync(new FileInfo(path), password, cancellationToken);
        }

        /// <summary>
        ///     Asynchronously opens a file, reads and decrypts the contents of the file into a string, and then closes the file.
        /// </summary>
        /// <param name="fileInfo">The file to open for reading.</param>
        /// <param name="password">The password string where the encryption key will be derived from.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A string containing the decrypted contents of the file.</returns>
        public static async Task<string> DecryptBase64FileAsync(FileInfo fileInfo, string password,
            CancellationToken cancellationToken = default)
        {
            var result = await InternalDecryptFileAsync(fileInfo, password, true, cancellationToken);

            // https://docs.microsoft.com/en-us/dotnet/api/system.text.utf8encoding.getstring?view=net-5.0#examples
            // https://stackoverflow.com/a/4900684/7644876
            return result.Success ? result.DecryptedDataString.TrimStart('\uFEFF', '\u200B') : null;
        }

        /// <summary>
        ///     Asynchronously opens a file, reads and decrypts the contents of the file into a byte array, and then closes the
        ///     file.
        /// </summary>
        /// <param name="path">The file to open for reading.</param>
        /// <param name="password">The password string where the encryption key will be derived from.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A byte array containing the decrypted contents of the file.</returns>
        public static async Task<byte[]> DecryptBase64FileBytesAsync(string path, string password,
            CancellationToken cancellationToken = default)
        {
            return await DecryptBase64FileBytesAsync(new FileInfo(path), password, cancellationToken);
        }

        /// <summary>
        ///     Asynchronously opens a file, reads and decrypts the contents of the file into a byte array, and then closes the
        ///     file.
        /// </summary>
        /// <param name="fileInfo">The file to open for reading.</param>
        /// <param name="password">The password string where the encryption key will be derived from.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A byte array containing the decrypted contents of the file.</returns>
        public static async Task<byte[]> DecryptBase64FileBytesAsync(FileInfo fileInfo, string password,
            CancellationToken cancellationToken = default)
        {
            var result = await InternalDecryptFileAsync(fileInfo, password, true, cancellationToken);
            return result.Success ? result.DecryptedDataBytes : null;
        }

        /// <summary>
        ///     Asynchronously opens a file, reads and decrypts the contents of the file into a string, and then closes the file.
        /// </summary>
        /// <param name="path">The file to open for reading.</param>
        /// <param name="password">The password string where the encryption key will be derived from.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A string containing the decrypted contents of the file.</returns>
        public static async Task<string> DecryptBinaryFileAsync(string path, string password,
            CancellationToken cancellationToken = default)
        {
            return await DecryptBinaryFileAsync(new FileInfo(path), password, cancellationToken);
        }

        /// <summary>
        ///     Asynchronously opens a file, reads and decrypts the contents of the file into a string, and then closes the file.
        /// </summary>
        /// <param name="fileInfo">The file to open for reading.</param>
        /// <param name="password">The password string where the encryption key will be derived from.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A string containing the decrypted contents of the file.</returns>
        public static async Task<string> DecryptBinaryFileAsync(FileInfo fileInfo, string password,
            CancellationToken cancellationToken = default)
        {
            var result = await InternalDecryptFileAsync(fileInfo, password, false, cancellationToken);

            // https://docs.microsoft.com/en-us/dotnet/api/system.text.utf8encoding.getstring?view=net-5.0#examples
            // https://stackoverflow.com/a/4900684/7644876
            return result.Success ? result.DecryptedDataString.TrimStart('\uFEFF', '\u200B') : null;
        }

        /// <summary>
        ///     Asynchronously opens a file, reads and decrypts the contents of the file into a byte array, and then closes the
        ///     file.
        /// </summary>
        /// <param name="path">The file to open for reading.</param>
        /// <param name="password">The password string where the encryption key will be derived from.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A byte array containing the decrypted contents of the file.</returns>
        public static async Task<byte[]> DecryptBinaryFileBytesAsync(string path, string password,
            CancellationToken cancellationToken = default)
        {
            return await DecryptBinaryFileBytesAsync(new FileInfo(path), password, cancellationToken);
        }

        /// <summary>
        ///     Asynchronously opens a file, reads and decrypts the contents of the file into a byte array, and then closes the
        ///     file.
        /// </summary>
        /// <param name="fileInfo">The file to open for reading.</param>
        /// <param name="password">The password string where the encryption key will be derived from.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A byte array containing the decrypted contents of the file.</returns>
        public static async Task<byte[]> DecryptBinaryFileBytesAsync(FileInfo fileInfo, string password,
            CancellationToken cancellationToken = default)
        {
            var result = await InternalDecryptFileAsync(fileInfo, password, false, cancellationToken);
            return result.Success ? result.DecryptedDataBytes : null;
        }

        /// <summary>
        ///     Encrypts an input string using AES with a 256 bits key in GCM authenticated mode, deriving the key from a provided
        ///     password string.
        /// </summary>
        /// <param name="plainString">The input plain string to encrypt.</param>
        /// <param name="password">The password string where the encryption key will be derived from.</param>
        /// <returns>A base64 encoded string containing the encrypted contents from <paramref name="plainString" />.</returns>
        public static string Encrypt(string plainString, string password)
        {
            var result = AesAlgorithm.Value.EncryptString(plainString, password);
            return result.EncryptedDataBase64String;
        }

        /// <summary>
        ///     Asynchronously opens a file, reads and encrypts the contents of the file into a base64 encoded string using AES
        ///     with a 256 bits key in GCM authenticated mode, and then closes the file.
        /// </summary>
        /// <param name="path">The file to open for reading.</param>
        /// <param name="password">The password string where the encryption key will be derived from.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A string array containing the encrypted contents of the file.</returns>
        public static async Task<string> EncryptFileBase64StringAsync(string path, string password,
            CancellationToken cancellationToken = default)
        {
            return await EncryptFileBase64StringAsync(new FileInfo(path), password, cancellationToken);
        }

        /// <summary>
        ///     Asynchronously opens a file, reads and encrypts the contents of the file into a base64 encoded string using AES
        ///     with a 256 bits key in GCM authenticated mode, and then closes the file.
        /// </summary>
        /// <param name="fileInfo">The file to open for reading.</param>
        /// <param name="password">The password string where the encryption key will be derived from.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A string containing the encrypted contents of the file.</returns>
        public static async Task<string> EncryptFileBase64StringAsync(FileInfo fileInfo, string password,
            CancellationToken cancellationToken = default)
        {
            var result = await InternalEncryptFileAsync(fileInfo, password, cancellationToken);
            return result.EncryptedDataBase64String;
        }

        /// <summary>
        ///     Asynchronously opens a file, reads and encrypts the contents of the file into a byte array using AES with a 256
        ///     bits key in GCM authenticated mode, and then closes the file.
        /// </summary>
        /// <param name="path">The file to open for reading.</param>
        /// <param name="password">The password string where the encryption key will be derived from.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A byte array containing the encrypted contents of the file.</returns>
        public static async Task<byte[]> EncryptFileBytesAsync(string path, string password,
            CancellationToken cancellationToken = default)
        {
            return await EncryptFileBytesAsync(new FileInfo(path), password, cancellationToken);
        }

        /// <summary>
        ///     Asynchronously opens a file, reads and encrypts the contents of the file into a byte array using AES with a 256
        ///     bits key in GCM authenticated mode, and then closes the file.
        /// </summary>
        /// <param name="fileInfo">The file to open for reading.</param>
        /// <param name="password">The password string where the encryption key will be derived from.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A byte array containing the encrypted contents of the file.</returns>
        public static async Task<byte[]> EncryptFileBytesAsync(FileInfo fileInfo, string password,
            CancellationToken cancellationToken = default)
        {
            var result = await InternalEncryptFileAsync(fileInfo, password, cancellationToken);
            return result.EncryptedDataBytes;
        }

        /// <summary>
        ///     Generate a base64 encoded 256-bit key to be used as the password string for encryption and decryption.
        /// </summary>
        /// <returns>A base64 encoded 256-bit key.</returns>
        public static string GenerateKey()
        {
            var key = CommonMethods.Generate256BitKey();
            return Convert.ToBase64String(key);
        }

        internal static async Task<AesEncryptionResult> InternalEncryptFileAsync(FileSystemInfo info, string password,
            CancellationToken cancellationToken)
        {
            // https://github.com/dotnet/standard/issues/260#issuecomment-290834776
            // https://docs.microsoft.com/en-us/dotnet/api/system.text.utf8encoding?view=net-5.0#remarks
            var key = new UTF8Encoding(false).GetBytes(password);
            var bytes = await File.ReadAllBytesAsync(info.FullName, cancellationToken);
            return AesAlgorithm.Value.EncryptString(bytes, key);
        }

        internal static async Task<AesDecryptionResult> InternalDecryptFileAsync(FileSystemInfo info, string password,
            bool isBase64,
            CancellationToken cancellationToken)
        {
            // https://github.com/dotnet/standard/issues/260#issuecomment-290834776
            // https://docs.microsoft.com/en-us/dotnet/api/system.text.utf8encoding?view=net-5.0#remarks
            var key = new UTF8Encoding(false).GetBytes(password);
            byte[] bytes;

            if (isBase64)
            {
                bytes = Convert.FromBase64String(await File.ReadAllTextAsync(info.FullName, cancellationToken));
            }
            else
            {
                bytes = await File.ReadAllBytesAsync(info.FullName, cancellationToken);
            }

            return AesAlgorithm.Value.DecryptString(bytes, key);
        }
    }
}