namespace DemiseTheReversation {

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Windows.Forms;

public static class Misc {
    public static void applyXorMask( byte[] bytes, byte[] mask, int start = 0, int? end = null ) {
        end ??= Math.Min( mask.Length + start, bytes.Length );

        for ( int i = start; i < end; i++ ) {
            bytes[i] ^= mask[( i - start ) % mask.Length];
        }
    }

    public static void rol( ref uint n, int shift ) {
        n = ( n << shift ) | ( n >> ( 32 - shift ) );
    }

    public static byte[] inflate( Stream deflatedInput, int bufSize ) {
        DeflateStream ds = new( deflatedInput, CompressionMode.Decompress );
        byte[] bytes = new byte[bufSize];
        ds.Read( bytes );
        return bytes;
    }

    public static byte[] inflateZlibBytes( byte[] zlibDeflatedBytes, int bufSize ) {
        using MemoryStream deflatedInput = new( zlibDeflatedBytes, 2, zlibDeflatedBytes.Length - 2 );
        // offsets to skip zlib header (2 bytes) & ADLER32 CRC footer (4 bytes)
        return inflate( deflatedInput, bufSize );
    }

    public static Encoding defaultEncoding { get; set; } = Encoding.ASCII;
}

public static class MenuExtensions {
    public static void add( this ToolStripItemCollection tsic, params ToolStripItem[] items ) {
        tsic.AddRange( items );
    }
}

public static class StringExtensions {
    public static byte[] toBytes( this string s, Encoding encoding = null ) {
        return ( encoding ?? Misc.defaultEncoding ).GetBytes( s );
    }
}

public static class ArrayExtensions {
    public static string toString<T>( this IEnumerable<T> array ) {
        return "[" + string.Join( "; ", array ) + "]";
    }

    public static string toString( this byte[] bytes, Encoding encoding = null ) {
        return ( encoding ?? Misc.defaultEncoding ).GetString( bytes );
    }
}

public static class StreamExtensions {
    public static void skip( this BinaryReader binaryReader, int offset ) {
        binaryReader.seek( offset, SeekOrigin.Current );
    }

    public static void seek( this BinaryReader binaryReader, int offset, SeekOrigin origin = SeekOrigin.Begin ) {
        binaryReader.BaseStream.Seek( offset, origin );
    }

    public static bool isAvailable( this BinaryReader binaryReader ) {
        Stream stream = binaryReader.BaseStream;
        return stream.Position < stream.Length;
    }

    public static string readString( this BinaryReader binaryReader, int length, Encoding encoding = null ) {
        return ( encoding ?? Encoding.Default ).GetString( binaryReader.ReadBytes( length ) );
    }
}

}
