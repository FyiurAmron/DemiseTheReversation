namespace DemiseTheReversation {

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

public static class Misc {
    public static void xorMask( byte[] bytes, byte[] mask, int start = 0, int? end = null ) {
        end ??= Math.Min( mask.Length + start, bytes.Length );

        for ( int i = start; i < end; i++ ) {
            bytes[i] ^= mask[( i - start ) % mask.Length];
        }
    }
}

public static class MenuExtensions {
    public static void add( this ToolStripItemCollection tsic, params ToolStripItem[] items ) {
        tsic.AddRange( items );
    }
}

public static class ArrayExtensions {
    public static string toString<T>( this IEnumerable<T> array ) {
        return "[" + string.Join( "; ", array ) + "]";
    }

    public static string toString( this byte[] bytes, Encoding? encoding = null ) {
        return ( encoding ?? Encoding.Default ).GetString( bytes );
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

    public static string readString( this BinaryReader binaryReader, int length, Encoding? encoding = null ) {
        return ( encoding ?? Encoding.Default ).GetString( binaryReader.ReadBytes( length ) );
    }
}

}
