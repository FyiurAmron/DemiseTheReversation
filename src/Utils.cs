namespace DemiseTheReversation {

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Encoder = System.Drawing.Imaging.Encoder;

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

public static class Bits {
    public static byte[] argb1555ToBytes( byte b1, byte b2 ) {
        return new[] {
            (byte) ( b1 & 0b1000_0000 ),
            (byte) ( ( b1 & 0b0111_1100 ) << 1 ),
            (byte) ( ( ( b1 & 0b0000_0011 ) << 6 ) | ( ( b2 & 0b1110_0000 ) >> 2 ) ),
            (byte) ( ( b2 & 0b0001_1111 ) << 3 )
        };
    }

    public static (byte, byte) bytesToArgb1555( byte[] bytes ) {
        byte b1 = bytes[0];
        b1 |= (byte) ( bytes[1] >> 1 );
        b1 |= (byte) ( bytes[2] >> 6 );
        byte b2 = (byte) ( bytes[2] << 2 );
        b2 |= (byte) ( bytes[3] >> 3 );
        return ( b1, b2 );
    }
}

// simple animation code for WinForms courtesy of Hans Passant
/*
// usage:
pictureBox1.Image = CreateAnimation( pictureBox1,
    new Image[] { Properties.Resources.Frame1, Properties.Resources.Frame2, Properties.Resources.Frame3 },
    new int[] { 1000, 2000, 300 }
);
*/
public static class AnimImage {
    public static Image createAnimation( Control ctl, Image[] frames, int[] delays ) {
        var ms = new MemoryStream();
        var codec = ImageCodecInfo.GetImageEncoders().First( i => i.MimeType == "image/tiff" );

        EncoderParameters encoderParameters = new( 2 ) {
            Param = {
                [0] = new EncoderParameter( Encoder.SaveFlag, (long) EncoderValue.MultiFrame ),
                [1] = new EncoderParameter( Encoder.Quality, (long) EncoderValue.CompressionLZW )
            }
        };
        frames[0].Save( ms, codec, encoderParameters );

        encoderParameters = new EncoderParameters( 1 ) {
            Param = {[0] = new EncoderParameter( Encoder.SaveFlag, (long) EncoderValue.FrameDimensionPage )}
        };
        for ( int i = 1; i < frames.Length; i++ ) {
            frames[0].SaveAdd( frames[i], encoderParameters );
        }

        encoderParameters.Param[0] = new EncoderParameter( Encoder.SaveFlag, (long) EncoderValue.Flush );
        frames[0].SaveAdd( encoderParameters );

        ms.Position = 0;
        Image img = Image.FromStream( ms );
        animate( ctl, img, delays );
        return img;
    }

    private static void animate( Control ctl, Image img, int[] delays ) {
        int frame = 0;
        var tmr = new Timer() {
            Interval = delays[0],
            Enabled = true
        };
        tmr.Tick += ( _, _ ) => {
            frame++;
            if ( frame >= delays.Length ) {
                frame = 0;
            }

            img.SelectActiveFrame( FrameDimension.Page, frame );
            tmr.Interval = delays[frame];
            ctl.Invalidate();
        };
        ctl.Disposed += ( _, _ ) => tmr.Dispose();
    }
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
