# Hades Compression

[![.NET](https://github.com/nenuadrian/hades-compression/actions/workflows/dotnet.yml/badge.svg)](https://github.com/nenuadrian/hades-compression/actions/workflows/dotnet.yml)

Simple huffman compression implementation in .net for learning purposes, achieving higher compression ratio the more patterns are in the input data as expected.

![logo](docs/hades.jpg)

| **Step**                  | **Function/Method**             | **Description**                                                                                                                                               |
|---------------------------|----------------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Build Frequency Table     | `BuildFrequencyTable`           | Reads input data byte-by-byte and counts occurrences to build a frequency table, mapping each byte to its frequency.                                          |
| Build Huffman Tree        | `BuildHuffmanTree`              | Constructs a binary tree based on the frequency table; nodes with lower frequencies are closer to the root, forming the structure for bit encoding.           |
| Generate Huffman Codes    | `CreateHuffmanTable`            | Recursively assigns binary codes to each byte by traversing the Huffman tree; '0' for left traversal and '1' for right traversal.                             |
| Encode Data               | `EncodeData`                    | Replaces each byte in the input data with its binary Huffman code from the Huffman table, creating a single encoded bit string.                               |
| Compress Data             | `Compress`                      | Combines the frequency table and encoded data into a compressed format; writes the frequency table size, entries, and encoded data to a byte array.           |
| Read Frequency Table      | `Decompress` (step 1)           | Reads the compressed data's frequency table to reconstruct the Huffman tree; necessary for decoding the bit sequence back to original bytes.                  |
| Rebuild Huffman Tree      | `Decompress` (step 2)           | Uses the read frequency table to recreate the Huffman tree structure used during compression.                                                                 |
| Decode Data               | `Decompress` (step 3)           | Traverses the Huffman tree based on each bit in the encoded data; left for '0', right for '1', reaching leaf nodes to retrieve the original byte values.      |
| Verbose Output (Optional) | Conditional on `--verbose` flag | Provides additional details about compression and decompression process, including input and output sizes, when the verbose option is enabled.                |


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

They are present in the `CompressionProject.Tests` folder, validating compression and reversal of compression work correctly and that when more patterns are present in the data the compression rate is better.

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
