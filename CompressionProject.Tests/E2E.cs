namespace CompressionProject.Tests;
using System.Text;

public class E2ETest
{
  [Fact]
  public void CompressAndDecompress_ShouldReturnOriginalString()
  {
    // Arrange
    string originalString = "This is an example string to be compressed using Huffman encoding.";
    byte[] inputData = Encoding.UTF8.GetBytes(originalString);

    // Act
    byte[] compressedData = Program.Compress(inputData, verbose: false);
    byte[] decompressedData = Program.Decompress(compressedData, verbose: false);
    string decompressedString = Encoding.UTF8.GetString(decompressedData);

    // Assert
    Assert.Equal(originalString, decompressedString);
  }

  [Fact]
  public void CompressAndDecompress_LongString_ShouldReturnOriginalString()
  {
    // Arrange
    string originalString = new string('A', 1000) + new string('B', 1000);
    byte[] inputData = Encoding.UTF8.GetBytes(originalString);

    // Act
    byte[] compressedData = Program.Compress(inputData, verbose: false);
    byte[] decompressedData = Program.Decompress(compressedData, verbose: false);
    string decompressedString = Encoding.UTF8.GetString(decompressedData);

    // Assert
    Assert.Equal(originalString, decompressedString);
  }
}
