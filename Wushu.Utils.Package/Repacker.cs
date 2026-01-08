using System.IO.Compression;
using System.Text;

namespace Wushu.Utils.Package
{
    internal class Repacker(string sourceFolder, string destinationFile)
    {
        private const int CompressionLevel = 9; // Value between 0 and 9. Age of Wushu uses 3 or 4
        private const int TocEntryBaseSize = 27; // All the numerical fields in the TOC entry add up to 27 bytes

        internal void Repack()
        {
            using var fs = new FileStream(destinationFile, FileMode.Create, FileAccess.Write);
            using var bw = new BinaryWriter(fs);

            bw.Write([0x50, 0x43, 0x4B, 0x30, 0x0F, 0x00, 0x00, 0x00, 0x00, 0x00]);

            var files = Directory.GetFiles(sourceFolder, "*", SearchOption.AllDirectories);

            bw.Write((uint)files.Length);
            bw.Write((byte)0x00);

            var startOfCompressedData = fs.Position + 4 + files
                .Select(f => Path.GetRelativePath(sourceFolder, f))
                .Sum(p => TocEntryBaseSize + Encoding.GetEncoding(1252).GetByteCount(p + "\0"));

            bw.Write((uint)startOfCompressedData);

            var tocIndex = fs.Position;
            var compressedFilesIndex = startOfCompressedData;

            foreach (var filePath in files)
            {
                var fileName = Path.GetRelativePath(sourceFolder, filePath);
                var uncompressedFile = File.ReadAllBytes(filePath);

                using var ms = new MemoryStream();
                using (var zlib = new ZLibStream(ms, new ZLibCompressionOptions
                {
                    CompressionStrategy = ZLibCompressionStrategy.Default,
                    CompressionLevel = CompressionLevel,
                }, true))
                {
                    zlib.Write(uncompressedFile, 0, uncompressedFile.Length);
                }
                var compressedFile = ms.ToArray();

                fs.Seek(tocIndex, SeekOrigin.Begin);

                var tocEntrySize = TocEntryBaseSize + Encoding.GetEncoding(1252).GetByteCount(fileName + "\0");
                bw.Write((ushort)tocEntrySize);
                bw.Write((uint)compressedFilesIndex);
                bw.Write((uint)0);
                bw.Write((uint)uncompressedFile.Length);
                bw.Write((uint)compressedFile.Length);
                bw.Write((ushort)DateTime.UtcNow.Year);
                bw.Write((byte)DateTime.UtcNow.Month);
                bw.Write((byte)DateTime.UtcNow.Day);
                bw.Write((byte)DateTime.UtcNow.Hour);
                bw.Write((byte)DateTime.UtcNow.Minute);
                bw.Write((byte)DateTime.UtcNow.Second);
                bw.Write((ushort)0);
                bw.Write(Encoding.GetEncoding(1252).GetBytes(fileName + "\0"));

                tocIndex = fs.Position;

                // write compressed file to stream
                fs.Seek(compressedFilesIndex, SeekOrigin.Begin);
                fs.Write(compressedFile, 0, compressedFile.Length);
                compressedFilesIndex += compressedFile.Length;
            }
        }
    }
}
