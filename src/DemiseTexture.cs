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
        int unknown1 = br.ReadInt32();
        Console.Out.WriteLine( unknown1 );
        int unknown2 = br.ReadInt32();
        int unknown3 = br.ReadInt32();
        int unknown4 = br.ReadInt32();
        int unknown5 = br.ReadInt32();
        // little endian
        // if ( red != 0x0000_00FF || green != 0x0000_FF00 || blue != 0x00FF_0000 ||
        //     ( alpha != unchecked((int) 0xFF00_0000) && alpha != 0 ) ) {
        //     throw new FileFormatException();
        // }

        bitmaps = new ByteBackedBitmap[frameCount];

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
