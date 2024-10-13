# compression

[![.NET](https://github.com/nenuadrian/hades-compression/actions/workflows/dotnet.yml/badge.svg)](https://github.com/nenuadrian/hades-compression/actions/workflows/dotnet.yml)

## Build

```
cd Compressionproject
dotnet build
```

## Run

```
cd Compressionproject
dotnet run
```

## Example

### Compression

```
cd Compressionproject
dotnet run -- ../CompressionProject.Tests/inputFile.txt --output ../CompressionProject.Tests/output.test --verbose
```

### Decompress

```
cd Compressionproject
dotnet run -- ../CompressionProject.Tests/output.test --output ../CompressionProject.Tests/outputFile.txt --verbose --decompress
```
