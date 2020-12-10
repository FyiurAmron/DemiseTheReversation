namespace DemiseTheReversation.Utils {

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

public static class StringExtensions {
    public static byte[] toBytes( this string s, Encoding encoding = null ) {
        return ( encoding ?? Misc.defaultEncoding ).GetBytes( s );
    }

    public static string join( this IEnumerable<string> strings, string separator ) {
        return string.Join( separator, strings );
    }

    public static StringBuilder appendAll( this StringBuilder sb, IEnumerable<string> strings ) {
        strings.forEach( ( s ) => sb.Append( s ) );

        return sb;
    }
}

public static class IEnumerableExtensions {
    public static void forEach<T>( this IEnumerable<T> iEnumerable, Action<T> action ) {
        foreach ( T t in iEnumerable ) {
            action( t );
        }
    }

    public static string toString<T>( this IEnumerable<T> iEnumerable ) {
        return "[" + string.Join( "; ", iEnumerable ) + "]";
    }

    public static object[] toObjectArray<T>( this IEnumerable<T> iEnumerable ) {
        T[] tArr = iEnumerable.ToArray();
        object[] arr = new object[tArr.Length];

        Array.Copy( tArr, arr, tArr.Length );

        return arr;
    }
}

public static class ArrayExtensions {
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
