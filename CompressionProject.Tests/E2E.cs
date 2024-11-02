namespace CompressionProject.Tests;
using System.Text;

public class E2ETest
{
  [Fact]
  public void CompressAndDecompress_ShouldReturnOriginalString()
  {
    string originalString = "This is an example string to be compressed using Huffman encoding.";
    byte[] inputData = Encoding.UTF8.GetBytes(originalString);

    byte[] compressedData = Program.Compress(inputData, verbose: false);
    byte[] decompressedData = Program.Decompress(compressedData, verbose: false);
    string decompressedString = Encoding.UTF8.GetString(decompressedData);

    Assert.Equal(originalString, decompressedString);
  }

  [Fact]
  public void CompressAndDecompress_LongString_ShouldReturnOriginalString()
  {
    string originalString = new string('A', 1000) + new string('B', 1000);
    byte[] inputData = Encoding.UTF8.GetBytes(originalString);

    byte[] compressedData = Program.Compress(inputData, verbose: false);
    byte[] decompressedData = Program.Decompress(compressedData, verbose: false);
    string decompressedString = Encoding.UTF8.GetString(decompressedData);

    Assert.Equal(originalString, decompressedString);
  }

  [Fact]
  public void CompressAndDecompress_LongString_ShouldCompressPatternsAndAchieveGoodCompressionRate()
  {
    string originalString = new string('A', 1000) + new string('B', 1000);
    byte[] inputData = Encoding.UTF8.GetBytes(originalString);

    byte[] compressedData = Program.Compress(inputData, verbose: false);
    byte[] decompressedData = Program.Decompress(compressedData, verbose: false);
    string decompressedString = Encoding.UTF8.GetString(decompressedData);

    double compressionRatio = (double)compressedData.Length / inputData.Length;

    Assert.Equal(originalString, decompressedString);
    Assert.True(compressionRatio < 0.3, "Compression should reduce data size");
  }
}
