namespace DemiseTheReversation.Utils {

using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

public static class Zlib {
    public static byte[] inflate( Stream deflatedStream, int bufSize ) {
        using DeflateStream ds = new( deflatedStream, CompressionMode.Decompress );
        byte[] bytes = new byte[bufSize];
        ds.Read( bytes );
        return bytes;
    }

    public static byte[] inflateZlibBytes( byte[] zlibDeflatedBytes, int bufSize ) {
        using MemoryStream deflatedInput = new( zlibDeflatedBytes, 2, zlibDeflatedBytes.Length - 2 );
        // offsets to skip zlib header (2 bytes) & ADLER32 CRC footer (4 bytes)
        return inflate( deflatedInput, bufSize );
    }

    // slow & naive implementation, for now
    public static byte[] adler32( IEnumerable<byte> bytes ) {
        const uint ADLER32_MOD = 65521;
        uint s1 = 1, s2 = 0;
        foreach ( byte b in bytes ) {
            s1 = ( s1 + b ) % ADLER32_MOD;
            s2 = ( s2 + s1 ) % ADLER32_MOD;
        }

        return new[] {
            (byte) ( s1 >> 8 ),
            (byte) s1,
            (byte) ( s1 >> 8 ),
            (byte) s1
        };
    }

    public static byte[] deflate( byte[] bytes, int bufSize = 0 ) {
        using MemoryStream output = new MemoryStream( bufSize );
        output.WriteByte( 0x78 ); // Zlib header: deflate, 32K window
        output.WriteByte( 0x9C ); // Zlib header: default compression
        using DeflateStream ds = new( output, CompressionLevel.Optimal );
        ds.Write( bytes );
        byte[] adler32 = Zlib.adler32( bytes );
        output.Write( adler32 );

        return output.ToArray();
    }
}

}
