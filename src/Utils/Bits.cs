namespace DemiseTheReversation.Utils {

using System;

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

    // ReSharper disable once InconsistentNaming
    public static (byte, byte, byte, byte) argb4444toBytes( short s ) {
        return (
            (byte) ( ( s & 0x0F ) * 17 ), // blue
            (byte) ( ( ( s >> 4 ) & 0x0F ) * 17 ), // green
            (byte) ( ( ( s >> 8 ) & 0x0F ) * 17 ), // red
            (byte) ( ( s >> 12 ) * 17 ) // alpha
        ); // x / 15 * 255 = 17x
    }

    public static void applyXorMask( byte[] bytes, byte[] mask, int start = 0, int? end = null ) {
        end ??= Math.Min( mask.Length + start, bytes.Length );

        for ( int i = start; i < end; i++ ) {
            bytes[i] ^= mask[( i - start ) % mask.Length];
        }
    }

    public static void rol( ref uint n, int shift ) {
        n = ( n << shift ) | ( n >> ( 32 - shift ) );
    }
}

}
