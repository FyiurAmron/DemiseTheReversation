namespace DemiseTheReversation {

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Utils;

public sealed class DemiseTexture : DemiseAsset {
    public const string IWT_1_0_MAGIC = "IWTv1.0\0";
    public const string DET_1_0_MAGIC = "DETv1.0\0";

    public const int PALETTE_ENTRY_COUNT = 1 << 8; // 8 bits

    // base file content

    public string magic { get; private set; }

    public int width { get; private set; }
    public int height { get; private set; }

    public int frameCount { get; private set; }

    // other data

    public ByteBackedBitmap[] bitmaps { get; private set; } // RGB_8_INDEXED; palettes 256*4 (RGBA) each

    public Bitmap this[ int i ] => bitmaps[i].bitmap;

    // methods

    public DemiseTexture() {
    }

    public override long load( byte[] sourceArray ) {
        using MemoryStream ms = new( sourceArray );
        using BinaryReader br = new( ms );

        magic = br.readString( 8 );
        if ( magic != IWT_1_0_MAGIC && magic != DET_1_0_MAGIC ) {
            throw new FileFormatException();
        }

        frameCount = br.ReadInt32();
        width = br.ReadInt32();
        height = br.ReadInt32();
        int algo = br.ReadInt32();
        Console.Out.WriteLine( algo );
        int unknown2 = br.ReadInt32();
        int unknown3 = br.ReadInt32();
        int unknown4 = br.ReadInt32();
        int unknown5 = br.ReadInt32();
        // little endian
        // if ( redMask != 0x0000_00FF || greenMask != 0x0000_FF00 || blueMask != 0x00FF_0000 ||
        //     ( alphaMask != unchecked((int) 0xFF00_0000) && alpha != 0 ) ) {
        //     throw new FileFormatException();
        // }

        bitmaps = new ByteBackedBitmap[frameCount];
        // algo == 0x0000_0108 -> 1 byte/pixel, no index + header(40)
        if ( algo == 0x0000_0808 || algo == 0x0000_0A08 /* with alpha */ ) {
            for ( int i = 0; i < frameCount; i++ ) {
                ByteBackedBitmap bbb = new( width, height, PixelFormat.Format8bppIndexed );
                bitmaps[i] = bbb;

                ColorPalette cp = bbb.bitmap.Palette;
                Color[] paletteEntries = cp.Entries;

                for ( int idx = 0; idx < PALETTE_ENTRY_COUNT; idx++ ) {
                    // needed due to endian swizzle
                    byte r = br.ReadByte(),
                         g = br.ReadByte(),
                         b = br.ReadByte(),
                         a = br.ReadByte();
                    paletteEntries[idx] = Color.FromArgb( a, r, g, b );
                }

                bbb.bitmap.Palette = cp;
            }

            for ( int i = 0; i < frameCount; i++ ) {
                ByteBackedBitmap bbb = bitmaps[i];
                byte[] bmpBytes = bbb.bytes;
                int idx = 0;
                for ( int y = 0; y < height; y++ ) {
                    for ( int x = 0; x < bbb.stride; x++, idx++ ) {
                        bmpBytes[idx] = br.ReadByte();
                        if ( x > 0 ) {
                            bmpBytes[idx] += bmpBytes[idx - 1];
                        }
                    }
                }
            }
        } else if ( algo == 0x0000_2015 ) {
            for ( int i = 0; i < frameCount; i++ ) {
                ByteBackedBitmap bbb = new( width, height, PixelFormat.Format16bppRgb565 );
                byte[] curr = bbb.bytes;
                bitmaps[i] = bbb;

                int idx = 0;
                for ( int y = 0; y < height; y++ ) {
                    for ( int x = 0; x < bbb.stride; x += 2, idx += 2 ) {
                        curr[idx] = br.ReadByte();
                        curr[idx + 1] = br.ReadByte();
                        short curVal = (short) ( ( curr[idx + 1] << 8 ) | curr[idx] );
                        if ( x > 0 ) {
                            short prevVal = (short) ( ( curr[idx - 1] << 8 ) | curr[idx - 2] );
                            curVal += prevVal;
                        }

                        curr[idx] = (byte) curVal;
                        curr[idx + 1] = (byte) ( curVal >> 8 );
                    }
                }
            }
        } else if ( algo == 0x0000_2213 // 2 bytes/pixel, no index +40
            || algo == 0x0000_0311 ) {
            // TODO implement, currently unsupported
            for ( int i = 0; i < frameCount; i++ ) {
                ByteBackedBitmap bbb = new( width, height, PixelFormat.Format16bppRgb565 );
                byte[] curr = bbb.bytes;
                bitmaps[i] = bbb;

                int idx = 0;
                for ( int y = 0; y < height; y++ ) {
                    for ( int x = 0; x < bbb.stride; x += 2, idx += 2 ) {
                        curr[idx] = br.ReadByte();
                        curr[idx + 1] = br.ReadByte();
                        short curVal = (short) ( ( curr[idx + 1] << 8 ) | curr[idx] );
                        if ( x > 0 ) {
                            short prevVal = (short) ( ( curr[idx - 1] << 8 ) | curr[idx - 2] );
                            curVal += prevVal;
                        }

                        curr[idx] = (byte) curVal;
                        curr[idx + 1] = (byte) ( curVal >> 8 );
                    }
                }
            }
        } else {
            throw new NotSupportedException();
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
