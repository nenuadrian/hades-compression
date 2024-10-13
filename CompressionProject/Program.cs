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
    // Create a root command with a description
    var rootCommand = new RootCommand("Custom compression library using Huffman encoding.")
        {
            new Argument<string>("input", "The input file to be compressed."),
            new Option<string>("--output", "The output compressed file (optional, default is input.huff)."),
            new Option<bool>("--verbose", "Enable verbose output."),
            new Option<bool>("--decompress", "Decompress.")
        };

    // Set up handler to process arguments
    rootCommand.SetHandler((string input, string output, bool verbose, bool decompress) =>
    {
      if (verbose)
      {
        Console.WriteLine("Verbose mode enabled.");
      }

      // If no output file is provided, default to input.huff
      if (string.IsNullOrWhiteSpace(output))
      {
        output = $"{Path.GetFileNameWithoutExtension(input)}.huff";
      }

      // Read the input file
      var inputData = File.ReadAllBytes(input);

      if (!decompress)
      {
        var compressedData = HuffmanCompress(inputData, verbose);

        // Write the compressed data to the output file
        File.WriteAllBytes(output, compressedData);

        if (verbose)
        {
          Console.WriteLine($"Compressed {input.Length} bytes to {compressedData.Length} bytes.");
          Console.WriteLine($"Compressed file saved to: {output}");
        }
      }
      else
      {
        var decompressedData = HuffmanDecompress(inputData, verbose);

        // Write the compressed data to the output file
        File.WriteAllBytes(output, decompressedData);

        if (verbose)
        {
          Console.WriteLine($"Decompressed {input.Length} bytes to {decompressedData.Length} bytes.");
          Console.WriteLine($"Decompressed file saved to: {output}");
        }
      }



    }, rootCommand.Arguments[0], rootCommand.Options[0], rootCommand.Options[1], rootCommand.Options[2]);

    // Invoke the root command with the parsed arguments
    return await rootCommand.InvokeAsync(args);
  }

  // Huffman Node
  class HuffmanNode
  {
    public byte? ByteValue { get; set; }
    public int Frequency { get; set; }
    public HuffmanNode Left { get; set; }
    public HuffmanNode Right { get; set; }

    // Constructor for leaf nodes
    public HuffmanNode(byte value, int frequency)
    {
      ByteValue = value;
      Frequency = frequency;
    }

    // Constructor for internal nodes
    public HuffmanNode(HuffmanNode left, HuffmanNode right)
    {
      ByteValue = null; // Internal nodes don't hold byte values
      Left = left;
      Right = right;
      Frequency = left.Frequency + right.Frequency;
    }

    public bool IsLeaf => ByteValue.HasValue;
  }

  static HuffmanNode BuildHuffmanTree(Dictionary<byte, int> frequencyTable)
  {
    var priorityQueue = new PriorityQueue<HuffmanNode, int>();

    // Step 1: Create a leaf node for each symbol and add it to the priority queue
    foreach (var entry in frequencyTable)
    {
      priorityQueue.Enqueue(new HuffmanNode(entry.Key, entry.Value), entry.Value);
    }

    // Step 2: Build the Huffman tree
    while (priorityQueue.Count > 1)
    {
      // Remove two nodes of the highest priority (lowest frequency)
      var left = priorityQueue.Dequeue();
      var right = priorityQueue.Dequeue();

      // Create a new internal node with the two removed nodes as children
      var parentNode = new HuffmanNode(left, right);

      // Add the parent node back to the priority queue
      priorityQueue.Enqueue(parentNode, parentNode.Frequency);
    }

    // The remaining node is the root of the Huffman tree
    return priorityQueue.Dequeue();
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

  static byte[] HuffmanCompress(byte[] data, bool verbose)
  {
    // Step 1: Build a frequency table
    var frequencyTable = new Dictionary<byte, int>();
    foreach (var b in data)
    {
      if (!frequencyTable.ContainsKey(b))
        frequencyTable[b] = 0;
      frequencyTable[b]++;
    }

    if (verbose)
    {
      Console.WriteLine("Frequency table built:");
      foreach (var entry in frequencyTable)
      {
        Console.WriteLine($"Byte: {entry.Key} - Frequency: {entry.Value}");
      }
    }

    // Step 2: Build the Huffman tree
    var root = BuildHuffmanTree(frequencyTable);

    // Step 3: Build the Huffman encoding table
    var huffmanTable = new Dictionary<byte, string>();
    BuildHuffmanTable(root, "", huffmanTable);

    if (verbose)
    {
      Console.WriteLine("Huffman encoding table:");
      foreach (var entry in huffmanTable)
      {
        Console.WriteLine($"Byte: {entry.Key} - Code: {entry.Value}");
      }
    }

    // Step 4: Encode the input data
    var encodedString = new StringBuilder();
    foreach (var b in data)
    {
      encodedString.Append(huffmanTable[b]);
    }

    // Convert the encoded string to bytes
    var bitArray = new List<byte>();
    for (int i = 0; i < encodedString.Length; i += 8)
    {
      string byteString = encodedString.ToString(i, Math.Min(8, encodedString.Length - i));
      bitArray.Add(Convert.ToByte(byteString.PadRight(8, '0'), 2)); // Pad right for incomplete bytes
    }

    // Step 5: Write frequency table and encoded data to output
    using (var memoryStream = new MemoryStream())
    using (var writer = new BinaryWriter(memoryStream))
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
      writer.Write(bitArray.ToArray());

      return memoryStream.ToArray();
    }
  }

  static byte[] HuffmanDecompress(byte[] compressedData, bool verbose)
  {
    using (var memoryStream = new MemoryStream(compressedData))
    using (var reader = new BinaryReader(memoryStream))
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

      if (verbose)
      {
        Console.WriteLine("Frequency table read from compressed data:");
        foreach (var entry in frequencyTable)
        {
          Console.WriteLine($"Byte: {entry.Key} - Frequency: {entry.Value}");
        }
      }

      // Step 2: Rebuild the Huffman tree
      var root = BuildHuffmanTree(frequencyTable);

      // Step 3: Read encoded data
      var encodedBits = new List<byte>();
      while (memoryStream.Position < memoryStream.Length)
      {
        encodedBits.Add(reader.ReadByte());
      }

      // Step 4: Decode the data using the Huffman tree
      var decodedBytes = new List<byte>();
      var currentNode = root;
      foreach (byte b in encodedBits)
      {
        // For each byte, read 8 bits
        for (int i = 0; i < 8; i++)
        {
          if (currentNode.IsLeaf)
          {
            decodedBytes.Add(currentNode.ByteValue.Value);
            currentNode = root;
          }

          // Extract the bit (0 or 1) from the byte
          bool bit = (b & (1 << (7 - i))) != 0;

          // Traverse the Huffman tree based on the bit
          currentNode = bit ? currentNode.Right : currentNode.Left;
        }
      }

      // Handle the case where we end on a leaf node
      if (currentNode.IsLeaf)
      {
        decodedBytes.Add(currentNode.ByteValue.Value);
      }

      return decodedBytes.ToArray();
    }
  }
}
