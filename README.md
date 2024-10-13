# Hades Compression

[![.NET](https://github.com/nenuadrian/hades-compression/actions/workflows/dotnet.yml/badge.svg)](https://github.com/nenuadrian/hades-compression/actions/workflows/dotnet.yml)

Simple huffman compression implementation in .net for learning purposes.

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

## Test

```
cd CompressionProject.Tests && dotnet test --verbosity normal
```

## Example

### Compression

```
cd Compressionproject
dotnet run -- ../CompressionProject.Tests/inputFile.txt --output ../CompressionProject.Tests/output.test --verbose
```

### Decompression

```
cd Compressionproject
dotnet run -- ../CompressionProject.Tests/output.test --output ../CompressionProject.Tests/outputFile.txt --verbose --decompress
```
