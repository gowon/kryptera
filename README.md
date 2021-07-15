# Kryptera

[![Nuget](https://img.shields.io/nuget/dt/Kryptera?color=blue&label=Kryptera%20downloads)](https://www.nuget.org/packages/Kryptera)
[![Nuget](https://img.shields.io/nuget/dt/Kryptera.Tools?color=blue&label=Kryptera.Tools%20downloads)](https://www.nuget.org/packages/Kryptera.Tools)
[![codecov](https://codecov.io/gh/gowon/kryptera/branch/main/graph/badge.svg?token=RJUNMU04ZE)](https://codecov.io/gh/gowon/kryptera)

Kryptera is a .NET Core Tool to quickly encrypt and decrypt files using AEAD AES-256-GCM algorithm from [CryptHash.NET](https://github.com/alecgn/crypthash-net/), as well as an encryption key generator. Kryptera means "encrypt" in [Swedish](https://translate.google.com/?sl=sv&tl=en&text=kryptera&op=translate).

## Install

|Package|Stable|Preview|
|-|-|-|
|Kryptera|[![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/Kryptera)](https://www.nuget.org/packages/Kryptera)|[![feedz.io](https://img.shields.io/badge/endpoint.svg?url=https%3A%2F%2Ff.feedz.io%2Fgowon%2Fpre-release%2Fshield%2FKryptera%2Flatest)](https://f.feedz.io/gowon/pre-release/packages/Kryptera/latest/download)|
|Kryptera.Tools|[![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/Kryptera.Tools)](https://www.nuget.org/packages/Kryptera.Tools)|[![feedz.io](https://img.shields.io/badge/endpoint.svg?url=https%3A%2F%2Ff.feedz.io%2Fgowon%2Fpre-release%2Fshield%2FKryptera.Tools%2Flatest)](https://f.feedz.io/gowon/pre-release/packages/Kryptera.Tools/latest/download)|

The Kryptera library can be obtained by either locally cloning this git repository and building it or via NuGet/Feedz:

```shell
Install-Package Kryptera
```

or

```shell
dotnet add package Kryptera
```

The command-line tool for encrypting and decrypting files can be installed through the `dotnet` CLI:

```shell
dotnet install --global Kryptera.Tools
```

after which it is available for use in your shell as `kryptera`.

## Usage

You can use the `--help` option to get detailed information about the commands.

```shell
kryptera

Usage:
  kryptera [options] [command]

Options:
  -v, --verbosity <Critical|Debug|Error|Information|None|Trace|Warning>  Set output verbosity
  --version                                                              Show version information
  -?, -h, --help                                                         Show help and usage information

Commands:
  encrypt <source>  Encrypt a file or directory using AES-256-GCM
  decrypt <source>  Decrypt a file or directory using AES-256-GCM
  generate          Generate a new AES-256 key
```

### Create an encryption key

You can create a new encryption key using the following command:

```shell
> kryptera generate
tRKvyxIcQA6jV0eUoH/LA1QFlDSNUQRfzDJqU9CSvzM=
```

The output is sent directly to the console, and can be piped directly as input into other applications or scripts.

### Encrypt a file or directory

You can encrypt a file or directory using the following command:

```shell
kryptera encrypt --key tRKvyxIcQA6jV0eUoH/LA1QFlDSNUQRfzDJqU9CSvzM= <SOURCE>
```

The filenames for the encrypted files will have `.aes` appended to them to make them easy to distinguish from the original files.

### Decrypt a file of directory

You can decrypt a file or directory using the following command:

```shell
kryptera decrypt --key tRKvyxIcQA6jV0eUoH/LA1QFlDSNUQRfzDJqU9CSvzM= <SOURCE>
```

If the encrypted files have `.aes` extensions, then the filenames for the decrypted files with have this extension removed. If the encrypted file extensions are not `.aes`, then the filenames of the decrypted files with have `-decrypted` appended to the name right before the extension.

## License

MIT
