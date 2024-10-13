using System.Linq;

class HuffmanNode
{
  public byte? ByteValue { get; set; }
  public int Frequency { get; set; }
  public HuffmanNode Left { get; set; }
  public HuffmanNode Right { get; set; }

  public HuffmanNode(byte value, int frequency)
  {
    ByteValue = value;
    Frequency = frequency;
  }

  public HuffmanNode(HuffmanNode left, HuffmanNode right)
  {
    ByteValue = null; // Internal nodes don't have values
    Left = left;
    Right = right;
    Frequency = left.Frequency + right.Frequency;
  }

  public bool IsLeaf => ByteValue.HasValue;
}
