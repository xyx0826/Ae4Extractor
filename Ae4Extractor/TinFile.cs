using System.Diagnostics;

namespace Ae4Extractor
{
    /// <summary>
    /// Length, location, and compression type about a Tin file.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    internal readonly struct TinFile
    {
        public TinFile(
            string path,
            ulong offset, ulong rawSize, ulong compressedSize,
            TinReadAccessType readAccessType)
        {
            Path = path;
            Offset = offset;
            RawSize = rawSize;
            CompressedSize = compressedSize;
            ReadAccessType = readAccessType;
        }

        /// <summary>
        /// The full path to the file, starting from mount root.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// The offset of the file in its Tin file, in bytes.
        /// </summary>
        public ulong Offset { get; }

        /// <summary>
        /// The raw size of the file.
        /// </summary>
        public ulong RawSize { get; }

        /// <summary>
        /// The compressed size of the file.
        /// </summary>
        public ulong CompressedSize { get; }

        /// <summary>
        /// The access type, or compression, of the file.
        /// </summary>
        public TinReadAccessType ReadAccessType { get; }

        private string DebuggerDisplay => $"{Path}, {ReadAccessType}, {RawSize} bytes";
    }
}
