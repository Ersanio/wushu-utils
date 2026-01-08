using System.IO.Compression;
using System.Text;

namespace Wushu.Utils.Package
{
    internal class Unpacker(string sourceFile, string destinationFolder)
    {
        internal void Unpack()
        {
            using var fileStream = File.OpenRead(sourceFile);
            using var binaryReader = new BinaryReader(fileStream);

            var header = binaryReader.ReadBytes(10);
            if (!header.SequenceEqual(new byte[] { 0x50, 0x43, 0x4B, 0x30, 0x0F, 0x00, 0x00, 0x00, 0x00, 0x00 }))
            {
                throw new Exception("Invalid PCK0 file");
            }

            var fileCount = binaryReader.ReadUInt32();
            _ = binaryReader.ReadUInt32(); // Start index of compressed data
            _ = binaryReader.ReadByte(); // Zero byte

            for (var i = 0; i < fileCount; i++)
            {
                var tocIndex = fileStream.Position;

                var tocEntrySize = binaryReader.ReadUInt16(); // [2 bytes] Size of TOC entry
                var compressedFileOffset = binaryReader.ReadUInt32(); // [4 bytes] Offset of compressed file
                _ = binaryReader.ReadUInt32(); // [4 bytes] Zero bytes
                var decompressedFileSize = binaryReader.ReadUInt32(); // [4 bytes] Expected decompressed file size
                var compressedFileSize = binaryReader.ReadUInt32(); // [4 bytes] Compressed file size
                _ = binaryReader.ReadUInt16(); // [2 bytes] Compression datetime: Year
                _ = binaryReader.ReadByte(); // [1 byte] Compression datetime: Month
                _ = binaryReader.ReadByte(); // [1 byte] Compression datetime: Day
                _ = binaryReader.ReadByte(); // [1 byte] Compression datetime: Hour
                _ = binaryReader.ReadByte(); // [1 byte] Compression datetime: Minute
                _ = binaryReader.ReadByte(); // [1 byte] Compression datetime: Second
                _ = binaryReader.ReadUInt16(); // [2 bytes] Zero bytes

                var fileNameLength = tocEntrySize - 27;
                fileStream.Seek(tocIndex + 27, SeekOrigin.Begin);

                var fileName = Encoding.GetEncoding(1252).GetString(binaryReader.ReadBytes(fileNameLength)).TrimEnd('\0');
                Console.WriteLine("Extracting: " + fileName);

                fileStream.Seek(compressedFileOffset, SeekOrigin.Begin);
                if ((int)compressedFileSize > int.MaxValue)
                {
                    throw new Exception($"File too large to read: {fileName}");
                }
                var compressedFile = binaryReader.ReadBytes((int)compressedFileSize);

                using var input = new MemoryStream(compressedFile);
                using var output = new MemoryStream();
                using (var zlib = new ZLibStream(input, CompressionMode.Decompress))
                {
                    zlib.CopyTo(output);
                }

                var decompressed = output.ToArray();

                if (decompressed.Length != decompressedFileSize)
                {
                    Console.WriteLine($"[WARNING] Size mismatch: {fileName}");
                }

                var combinedPath = Path.Combine(destinationFolder, fileName);
                var dir = Path.GetDirectoryName(combinedPath);
                if (!string.IsNullOrEmpty(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                Console.WriteLine($"Writing file: {combinedPath}");
                File.WriteAllBytes(combinedPath, decompressed);

                fileStream.Seek(tocIndex + tocEntrySize, SeekOrigin.Begin);
            }
        }
    }
}
