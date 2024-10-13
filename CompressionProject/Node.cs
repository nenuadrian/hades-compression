using System.Linq;

class HuffmanNode
{
  public byte? ByteValue { get; set; }
  public int Frequency { get; set; }
  public HuffmanNode? Left { get; set; }  // Nullable
  public HuffmanNode? Right { get; set; } // Nullable

  // Constructor for leaf nodes
  public HuffmanNode(byte value, int frequency)
  {
    ByteValue = value;
    Frequency = frequency;
    Left = null; // Explicitly setting to null
    Right = null; // Explicitly setting to null
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
