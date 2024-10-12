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
            new Option<bool>("--verbose", "Enable verbose output.")
        };

        // Set up handler to process arguments
        rootCommand.SetHandler((string input, string output, bool verbose) =>
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

            // Perform Huffman encoding
            var compressedData = HuffmanCompress(inputData, verbose);

            // Write the compressed data to the output file
            File.WriteAllBytes(output, compressedData);

            if (verbose)
            {
                Console.WriteLine($"Compressed {input.Length} bytes to {compressedData.Length} bytes.");
                Console.WriteLine($"Compressed file saved to: {output}");
            }

        }, rootCommand.Arguments[0], rootCommand.Options[0], rootCommand.Options[1]);

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

    // Helper function to build the Huffman tree
    static HuffmanNode BuildHuffmanTree(Dictionary<byte, int> frequencyTable)
    {
        var priorityQueue = new SortedSet<HuffmanNode>(Comparer<HuffmanNode>.Create((a, b) =>
        {
            int result = a.Frequency.CompareTo(b.Frequency);
            return result == 0 ? 1 : result; // Prevent duplicate keys in the SortedSet
        }));

        // Step 1: Create a leaf node for each symbol and add it to the priority queue
        foreach (var entry in frequencyTable)
        {
            priorityQueue.Add(new HuffmanNode(entry.Key, entry.Value));
        }

        // Step 2: Build the Huffman tree
        while (priorityQueue.Count > 1)
        {
            // Remove two nodes of the highest priority (lowest frequency)
            var left = priorityQueue.First();
            priorityQueue.Remove(left);
            var right = priorityQueue.First();
            priorityQueue.Remove(right);

            // Create a new internal node with the two removed nodes as children
            var parentNode = new HuffmanNode(left, right);
            priorityQueue.Add(parentNode);
        }

        // The remaining node is the root of the Huffman tree
        return priorityQueue.First();
    }

    // Recursive function to build the Huffman encoding table (byte -> bit sequence)
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

    // Huffman compression function
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

        return bitArray.ToArray();
    }
}
