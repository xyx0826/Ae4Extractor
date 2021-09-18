namespace Ae4Extractor
{
    internal enum TinReadAccessType : ushort
    {
        RawReadAccess,
        ZLibReadAccess,
        FileReadAccess,
        ZStdReadAccess,
        LZ4ReadAccess
    }
}
