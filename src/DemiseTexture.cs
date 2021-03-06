namespace DemiseTheReversation {

using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Utils;

public sealed class DemiseTexture : DemiseAsset {
    public const string IWT_1_0_MAGIC = "IWTv1.0\0";
    public const string DET_1_0_MAGIC = "DETv1.0\0";

    public const int PALETTE_ENTRY_COUNT_4_BIT = 1 << 4;
    public const int PALETTE_ENTRY_COUNT_8_BIT = 1 << 8;
    public const int MAX_PALETTE_VALUE = 1 << 8;

    // base file content

    public string magic { get; private set; }

    public int width { get; private set; }
    public int height { get; private set; }

    public int frameCount { get; private set; }

    public int type { get; private set; }

    // other data

    public ByteBackedBitmap[] bitmaps { get; private set; } // RGB_8_INDEXED; palettes 256*4 (RGBA) each

    public Bitmap this[ int i ] => bitmaps[i].bitmap;

    // methods

    public DemiseTexture() {
    }

    public override long load( byte[] sourceBytes ) {
        using MemoryStream ms = new( sourceBytes );
        using BinaryReader br = new( ms );

        magic = br.readString( 8 );
        if ( magic != IWT_1_0_MAGIC && magic != DET_1_0_MAGIC ) {
            throw new FileFormatException();
        }

        frameCount = br.ReadInt32();
        width = br.ReadInt32();
        height = br.ReadInt32();
        type = br.ReadInt32();
        int redMask = br.ReadInt32();
        int greenMask = br.ReadInt32();
        int blueMask = br.ReadInt32();
        int alphaMask = br.ReadInt32();
        // little endian
        // TODO sanity check of the masks based on formats
        // if ( redMask != 0x0000_00FF || greenMask != 0x0000_FF00 || blueMask != 0x00FF_0000 ||
        //     ( alphaMask != unchecked((int) 0xFF00_0000) && alpha != 0 ) ) {
        //     throw new FileFormatException();
        // }

        // // ( type & 1 << 3 ) - 8bpp
        // // ( type & 1 << 4 ) - 16bpp
        bitmaps = new ByteBackedBitmap[frameCount];
        switch ( type ) {
            case 0x0000_0311: // 16-bit greyscale with alpha: V8A8
            {
                // all cases in the wild are single-frame (frameCount == 1)
                if ( frameCount > 1 ) { // no sense in reversing something that neither exists nor is useful anyhow
                    throw new FileFormatException();
                }

                for ( int i = 0; i < frameCount; i++ ) {
                    ByteBackedBitmap bbb = new( width, height, PixelFormat.Format32bppArgb );
                    // wacky conversion since it's basically 16-bit: 8-bit greyscale + 8-bit alpha 
                    bitmaps[i] = bbb;
                    byte[] bmpBytes = bbb.bytes;
                    int idx = 0;
                    for ( int y = 0; y < height; y++ ) {
                        short val = 0;
                        for ( int x = 0; x < bbb.stride / 4; x++, idx += 4 ) {
                            val += br.ReadInt16();

                            ( byte value, byte alpha ) = val.toBytes();

                            bmpBytes[idx + 0] =
                                bmpBytes[idx + 1] =
                                    bmpBytes[idx + 2] = value;
                            bmpBytes[idx + 3] = alpha;
                        }
                    }
                }

                break;
            }
            case 0x0000_0808: // 8-bit indexed no alpha (but compatible): I8->R8G8B8A8
            case 0x0000_0A08: // 8-bit indexed with alpha: I8->R8G8B8A8
            case 0x0000_0108: // 8-bit grayscale: V8
            {
                for ( int i = 0; i < frameCount; i++ ) {
                    bitmaps[i] = new( width, height, PixelFormat.Format8bppIndexed );
                    ByteBackedBitmap bbb = bitmaps[i];

                    bbb.setPalette( ( idx ) =>
                                        ( type == 0x0000_0108 )
                                            ? Color.FromArgb( idx, idx, idx )
                                            // ReSharper disable once AccessToDisposedClosure
                                            : br.readColorRGBA()
                    );
                    // palette has to be read first, since, if used, it's physically first in file
                    byte[] bmpBytes = bbb.bytes;
                    for ( int y = 0, idx = 0; y < height; y++ ) {
                        byte val = 0;
                        for ( int x = 0; x < bbb.stride; x++, idx++ ) {
                            if ( frameCount == 1 ) {
                                val += br.ReadByte();
                            } else {
                                val = (byte) ( br.ReadByte()
                                    + ( ( i == 0 )
                                        ? 0
                                        : bitmaps[i - 1].bytes[idx] ) );
                            }

                            bmpBytes[idx] = val;
                        }
                    }
                }

                break;
            }
            case 0x0000_2015: // 16-bit R5G6B5
            {
                for ( int i = 0; i < frameCount; i++ ) {
                    bitmaps[i] = new( width, height, PixelFormat.Format16bppRgb565 );
                }

                ByteBackedBitmap bbb = bitmaps[0];
                byte[] bmpBytes = bbb.bytes;

                int idx = 0;
                for ( int y = 0; y < height; y++ ) {
                    short val = 0;
                    for ( int x = 0; x < bbb.stride; x += 2, idx += 2 ) {
                        if ( frameCount == 1 ) {
                            val += br.ReadInt16();
                        } else {
                            val = br.ReadInt16();
                        }

                        ( bmpBytes[idx], bmpBytes[idx + 1] ) = val.toBytes();
                    }
                }

                if ( frameCount > 1 ) {
                    for ( int i = 1; i < frameCount; i++ ) {
                        bbb = bitmaps[i];
                        bmpBytes = bbb.bytes;
                        byte[] prvBytes = bitmaps[i - 1].bytes;

                        idx = 0;
                        for ( int y = 0; y < height; y++ ) {
                            for ( int x = 0; x < bbb.stride; x += 2, idx += 2 ) {
                                short val = (short) ( br.ReadInt16() + prvBytes.getShort( idx ) );
                                ( bmpBytes[idx], bmpBytes[idx + 1] ) = val.toBytes();
                            }
                        }
                    }
                }

                break;
            }
            case 0x0000_2213: // 16-bit A4B4G4R4
            { // note: remapped to 32-bit since we don't have native support for it
                for ( int i = 0; i < frameCount; i++ ) {
                    bitmaps[i] = new( width, height, PixelFormat.Format32bppArgb );
                }

                ByteBackedBitmap bbb = bitmaps[0];
                byte[] bmpBytes = bbb.bytes;
                short[] data = new short[height * bbb.stride / 4]; // needed due to aforementioned remapping

                for ( int y = 0, idx = 0; y < height; y++ ) {
                    short val = 0;
                    for ( int x = 0; x < bbb.stride; x += 4, idx += 4 ) {
                        if ( frameCount == 1 ) {
                            val += br.ReadInt16();
                        } else {
                            val = br.ReadInt16();
                        }

                        data[idx / 4] = val;

                        ( bmpBytes[idx + 0], bmpBytes[idx + 1], bmpBytes[idx + 2], bmpBytes[idx + 3] )
                            = Bits.argb4444toBytes( val );
                    }
                }

                if ( frameCount > 1 ) {
                    for ( int i = 1; i < frameCount; i++ ) {
                        bbb = bitmaps[i];
                        bmpBytes = bbb.bytes;

                        for ( int y = 0, idx = 0; y < height; y++ ) {
                            for ( int x = 0; x < bbb.stride; x += 4, idx += 4 ) {
                                data[idx / 4] += br.ReadInt16();

                                ( bmpBytes[idx + 0], bmpBytes[idx + 1], bmpBytes[idx + 2], bmpBytes[idx + 3] )
                                    = Bits.argb4444toBytes( data[idx / 4] );
                            }
                        }
                    }
                }

                break;
            }

            default:
                throw new FileFormatException( $"image type {type:X8} invalid/not supported" );
        }

        return ms.Position;
    }

    public override void Dispose() {
        base.Dispose();

        foreach ( ByteBackedBitmap bbb in bitmaps ) {
            bbb?.Dispose();
        }
    }
}

}
