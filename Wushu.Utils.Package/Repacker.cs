using System.IO.Compression;
using System.Text;

namespace Wushu.Utils.Package
{
    internal class Repacker(string sourceFolder, string destinationFile)
    {
        private const int CompressionLevel = 9; // Value between 0 and 9. Age of Wushu uses 3 or 4
        private const int TocEntryBaseSize = 27; // All the numerical fields in the TOC entry add up to 27 bytes
        private static readonly Encoding Cp1252 = Encoding.GetEncoding(1252);

        private readonly struct TocEntry
        {
            public readonly ushort EntrySize { get; init; }
            public readonly uint CompressedDataOffset { get; init; }
            public readonly uint UncompressedSize { get; init; }
            public readonly uint CompressedSize { get; init; }
            public readonly string FileName { get; init; }
        }

        internal void Repack()
        {
            var directory = Path.GetDirectoryName(destinationFile)!;
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using var fs = new FileStream(destinationFile, FileMode.Create, FileAccess.Write);
            using var bw = new BinaryWriter(fs);

            bw.Write([0x50, 0x43, 0x4B, 0x30, 0x0F, 0x00, 0x00, 0x00, 0x00, 0x00]);

            var files = Directory.GetFiles(sourceFolder, "*", SearchOption.AllDirectories);

            bw.Write((uint)files.Length);
            bw.Write((byte)0x00);

            var startOfCompressedData = fs.Position + 4 + files
                .Select(f => Path.GetRelativePath(sourceFolder, f))
                .Sum(p => TocEntryBaseSize + Cp1252.GetByteCount(p + "\0"));

            bw.Write((uint)startOfCompressedData);

            var tocIndex = fs.Position;

            fs.Seek(startOfCompressedData, SeekOrigin.Begin);
            var tocEntries = new List<TocEntry>();
            foreach (var filePath in files)
            {
                var fileName = Path.GetRelativePath(sourceFolder, filePath);
                var uncompressedFile = File.ReadAllBytes(filePath);
                Console.WriteLine("Packaging: " + fileName);

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

                tocEntries.Add(new TocEntry
                {
                    EntrySize = (ushort)(TocEntryBaseSize + Cp1252.GetByteCount(fileName + "\0")),
                    CompressedDataOffset = (uint)fs.Position,
                    UncompressedSize = (uint)uncompressedFile.Length,
                    CompressedSize = (uint)compressedFile.Length,
                    FileName = fileName.Replace(Path.DirectorySeparatorChar, '\\'),
                });

                fs.Write(compressedFile, 0, compressedFile.Length);
                var ratio = uncompressedFile.Length == 0 ? 100 : (double)compressedFile.Length / uncompressedFile.Length * 100;
                Console.WriteLine($"Wrote {fileName} ({uncompressedFile.Length} -> {compressedFile.Length} bytes, {ratio:F2}% compression ratio)");
            }

            fs.Seek(tocIndex, SeekOrigin.Begin);
            foreach (var entry in tocEntries)
            {
                bw.Write((ushort)entry.EntrySize);
                bw.Write((uint)entry.CompressedDataOffset);
                bw.Write((uint)0x00000000);
                bw.Write((uint)entry.UncompressedSize);
                bw.Write((uint)entry.CompressedSize);
                bw.Write((ushort)DateTime.UtcNow.Year);
                bw.Write((byte)DateTime.UtcNow.Month);
                bw.Write((byte)DateTime.UtcNow.Day);
                bw.Write((byte)DateTime.UtcNow.Hour);
                bw.Write((byte)DateTime.UtcNow.Minute);
                bw.Write((byte)DateTime.UtcNow.Second);
                bw.Write((ushort)0x0000);
                bw.Write(Cp1252.GetBytes(entry.FileName + "\0"));
            }
        }
    }
}
