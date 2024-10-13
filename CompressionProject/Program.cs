using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class Program
{
  static async Task<int> Main(string[] args)
  {
    var rootCommand = new RootCommand("Custom compression library using Huffman encoding.")
        {
            new Argument<string>("input", "The input file to be compressed."),
            new Option<string>("--output", "The output compressed file (optional, default is input.huff)."),
            new Option<bool>("--verbose", "Enable verbose output."),
            new Option<bool>("--decompress", "Decompress the input.")
        };

    rootCommand.SetHandler((string input, string output, bool verbose, bool decompress) =>
    {
      if (string.IsNullOrWhiteSpace(output))
      {
        output = $"{Path.GetFileNameWithoutExtension(input)}.huff";
      }

      // Read the input file
      var inputData = File.ReadAllBytes(input);

      if (!decompress)
      {
        var compressedData = Compress(inputData, verbose);

        File.WriteAllBytes(output, compressedData);

        if (verbose)
        {
          Console.WriteLine($"Compressed {input.Length} bytes to {compressedData.Length} bytes.");
          Console.WriteLine($"Compressed file saved to: {output}");
        }
      }
      else
      {
        var decompressedData = Decompress(inputData, verbose);

        File.WriteAllBytes(output, decompressedData);

        if (verbose)
        {
          Console.WriteLine($"Decompressed {input.Length} bytes to {decompressedData.Length} bytes.");
          Console.WriteLine($"Decompressed file saved to: {output}");
        }
      }
    }, rootCommand.Arguments[0], rootCommand.Options[0], rootCommand.Options[1], rootCommand.Options[2]);

    return await rootCommand.InvokeAsync(args);
  }

  static Dictionary<byte, int> BuildFrequencyTable(byte[] data)
  {
    var frequencyTable = new Dictionary<byte, int>();

    foreach (var b in data)
    {
      if (!frequencyTable.ContainsKey(b))
      {
        frequencyTable[b] = 0;
      }
      frequencyTable[b]++;
    }

    return frequencyTable;
  }


  static HuffmanNode BuildHuffmanTree(Dictionary<byte, int> frequencyTable)
  {
    var priorityQueue = new List<HuffmanNode>();

    foreach (var entry in frequencyTable)
    {
      priorityQueue.Add(new HuffmanNode(entry.Key, entry.Value));
    }

    priorityQueue = priorityQueue.OrderBy(node => node.Frequency).ToList();

    while (priorityQueue.Count > 1)
    {
      var left = priorityQueue[0];
      var right = priorityQueue[1];
      priorityQueue.RemoveAt(0);
      priorityQueue.RemoveAt(0);

      var parentNode = new HuffmanNode(left, right);
      priorityQueue.Add(parentNode);

      priorityQueue = priorityQueue.OrderBy(node => node.Frequency).ToList();
    }

    return priorityQueue[0];
  }

  static void BuildHuffmanTable(HuffmanNode node, string bitString, Dictionary<byte, string> huffmanTable)
  {
    if (node.IsLeaf)
    {
      huffmanTable[node.ByteValue.Value] = bitString;
    }
    else
    {
      BuildHuffmanTable(node.Left, bitString + "0", huffmanTable);
      BuildHuffmanTable(node.Right, bitString + "1", huffmanTable);
    }
  }

  static Dictionary<byte, string> CreateHuffmanTable(HuffmanNode root)
  {
    var huffmanTable = new Dictionary<byte, string>();
    BuildHuffmanTable(root, "", huffmanTable);
    return huffmanTable;
  }

  static byte[] EncodeData(byte[] data, Dictionary<byte, string> huffmanTable)
  {
    var encodedString = new System.Text.StringBuilder();

    foreach (var b in data)
    {
      encodedString.Append(huffmanTable[b]);
    }

    // Convert the encoded bit string to byte array
    var bitArray = new List<byte>();

    for (int i = 0; i < encodedString.Length; i += 8)
    {
      string byteString = encodedString.ToString(i, Math.Min(8, encodedString.Length - i));
      bitArray.Add(Convert.ToByte(byteString.PadRight(8, '0'), 2));
    }

    return bitArray.ToArray();
  }

  static byte[] Compress(byte[] data, bool verbose)
  {
    // Step 1: Build frequency table
    var frequencyTable = BuildFrequencyTable(data);

    // Step 2: Build Huffman Tree
    var root = BuildHuffmanTree(frequencyTable);

    // Step 3: Build Huffman Table
    var huffmanTable = CreateHuffmanTable(root);

    // Step 4: Encode Data
    var encodedData = EncodeData(data, huffmanTable);

    // Step 5: Write Frequency Table and Encoded Data
    using (var memoryStream = new System.IO.MemoryStream())
    using (var writer = new System.IO.BinaryWriter(memoryStream))
    {
      // Write frequency table size
      writer.Write(frequencyTable.Count);

      // Write frequency table
      foreach (var entry in frequencyTable)
      {
        writer.Write(entry.Key);
        writer.Write(entry.Value);
      }

      // Write encoded data
      writer.Write(encodedData);

      return memoryStream.ToArray();
    }
  }

  static byte[] Decompress(byte[] compressedData, bool verbose)
  {
    using (var memoryStream = new System.IO.MemoryStream(compressedData))
    using (var reader = new System.IO.BinaryReader(memoryStream))
    {
      // Step 1: Read frequency table
      int frequencyTableSize = reader.ReadInt32();
      var frequencyTable = new Dictionary<byte, int>();

      for (int i = 0; i < frequencyTableSize; i++)
      {
        byte key = reader.ReadByte();
        int value = reader.ReadInt32();
        frequencyTable[key] = value;
      }

      // Step 2: Rebuild Huffman Tree
      var root = BuildHuffmanTree(frequencyTable);

      // Step 3: Decode Data
      var currentNode = root;
      var decodedBytes = new List<byte>();
      var bits = new List<bool>();

      // Read all bytes from the memory stream after frequency table
      while (memoryStream.Position < memoryStream.Length)
      {
        byte b = reader.ReadByte();
        for (int i = 7; i >= 0; i--)
        {
          bits.Add((b & (1 << i)) != 0);
        }
      }

      foreach (var bit in bits)
      {
        currentNode = bit ? currentNode.Right : currentNode.Left;

        if (currentNode.IsLeaf)
        {
          decodedBytes.Add(currentNode.ByteValue.Value);
          currentNode = root;
        }
      }

      return decodedBytes.ToArray();
    }
  }
}
