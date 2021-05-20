namespace Kryptera
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using CryptHash.Net.Encryption.AES.AEAD;
    using CryptHash.Net.Encryption.AES.EncryptionResults;

    public static class Kryptera
    {
        private static Lazy<AEAD_AES_256_GCM> AesAlgorithm => new Lazy<AEAD_AES_256_GCM>(() => new AEAD_AES_256_GCM());

        public static async Task<string> DecryptBase64FileAsync(string path, string password,
            CancellationToken cancellationToken = default)
        {
            return await DecryptBase64FileAsync(new FileInfo(path), password, cancellationToken);
        }

        public static async Task<string> DecryptBase64FileAsync(FileInfo fileInfo, string password,
            CancellationToken cancellationToken = default)
        {
            var result = await InternalDecryptAsync(fileInfo, password, true, cancellationToken);

            // https://docs.microsoft.com/en-us/dotnet/api/system.text.utf8encoding.getstring?view=net-5.0#examples
            // https://stackoverflow.com/a/4900684/7644876
            return result.Success ? result.DecryptedDataString.TrimStart('\uFEFF', '\u200B') : null;
        }

        public static async Task<byte[]> DecryptBase64FileBytesAsync(string path, string password,
            CancellationToken cancellationToken = default)
        {
            return await DecryptBase64FileBytesAsync(new FileInfo(path), password, cancellationToken);
        }

        public static async Task<byte[]> DecryptBase64FileBytesAsync(FileInfo fileInfo, string password,
            CancellationToken cancellationToken = default)
        {
            var result = await InternalDecryptAsync(fileInfo, password, true, cancellationToken);
            return result.Success ? result.DecryptedDataBytes : null;
        }

        public static async Task<string> DecryptBinaryFileAsync(string path, string password,
            CancellationToken cancellationToken = default)
        {
            return await DecryptBinaryFileAsync(new FileInfo(path), password, cancellationToken);
        }

        public static async Task<string> DecryptBinaryFileAsync(FileInfo fileInfo, string password,
            CancellationToken cancellationToken = default)
        {
            var result = await InternalDecryptAsync(fileInfo, password, false, cancellationToken);

            // https://docs.microsoft.com/en-us/dotnet/api/system.text.utf8encoding.getstring?view=net-5.0#examples
            // https://stackoverflow.com/a/4900684/7644876
            return result.Success ? result.DecryptedDataString.TrimStart('\uFEFF', '\u200B') : null;
        }

        public static async Task<byte[]> DecryptBinaryFileBytesAsync(string path, string password,
            CancellationToken cancellationToken = default)
        {
            return await DecryptBinaryFileBytesAsync(new FileInfo(path), password, cancellationToken);
        }

        public static async Task<byte[]> DecryptBinaryFileBytesAsync(FileInfo fileInfo, string password,
            CancellationToken cancellationToken = default)
        {
            var result = await InternalDecryptAsync(fileInfo, password, false, cancellationToken);
            return result.Success ? result.DecryptedDataBytes : null;
        }

        public static async Task<string> EncryptFileBase64StringAsync(string path, string password,
            CancellationToken cancellationToken = default)
        {
            return await EncryptFileBase64StringAsync(new FileInfo(path), password, cancellationToken);
        }

        public static async Task<string> EncryptFileBase64StringAsync(FileInfo fileInfo, string password,
            CancellationToken cancellationToken = default)
        {
            var result = await InternalEncryptAsync(fileInfo, password, cancellationToken);
            return result.EncryptedDataBase64String;
        }

        public static async Task<byte[]> EncryptFileBytesAsync(string path, string password,
            CancellationToken cancellationToken = default)
        {
            return await EncryptFileBytesAsync(new FileInfo(path), password, cancellationToken);
        }

        public static async Task<byte[]> EncryptFileBytesAsync(FileInfo fileInfo, string password,
            CancellationToken cancellationToken = default)
        {
            var result = await InternalEncryptAsync(fileInfo, password, cancellationToken);
            return result.EncryptedDataBytes;
        }

        internal static async Task<AesEncryptionResult> InternalEncryptAsync(FileSystemInfo info, string password,
            CancellationToken cancellationToken)
        {
            var key = Convert.FromBase64String(password);
            var bytes = await File.ReadAllBytesAsync(info.FullName, cancellationToken);
            return AesAlgorithm.Value.EncryptString(bytes, key);
        }

        internal static async Task<AesDecryptionResult> InternalDecryptAsync(FileSystemInfo info, string password,
            bool isBase64,
            CancellationToken cancellationToken)
        {
            var key = Convert.FromBase64String(password);
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