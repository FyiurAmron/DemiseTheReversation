namespace DemiseTheReversation {

using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Utils;

// TODO check guildlogbook.dea && guildlibrary.dea for possible quirks/errors/bugs, probably related to strides/alpha?
public sealed class DemiseAnimation : DemiseAsset {
    public const string IWA_1_2_MAGIC = "IWAv1.2\0";
    public const string DEA_1_2_MAGIC = "DEAv1.2\0";

    // base file content

    public string magic { get; private set; }

    public int width { get; private set; } // ??? sometimes + 1 (stride?)
    public int height { get; private set; }
    public int delayMs { get; private set; }
    public int frameCount { get; private set; }
    public int notBook { get; private set; } // ??? 0 for books, 1 for other

    // other data

    public ByteBackedBitmap[] bitmaps { get; private set; } // ARGB_1555, Little Endian

    public Bitmap this[ int i ] => bitmaps[i].bitmap;

    // methods

    public DemiseAnimation() {
    }

    public override long load( byte[] sourceBytes ) {
        using MemoryStream ms = new( sourceBytes );
        using BinaryReader br = new( ms );

        magic = br.readString( 8 );
        if ( magic != IWA_1_2_MAGIC && magic != DEA_1_2_MAGIC ) {
            throw new FileFormatException();
        }

        width = br.ReadInt32();
        height = br.ReadInt32();
        delayMs = br.ReadInt32();
        frameCount = br.ReadInt32();
        notBook = br.ReadInt32();

        bitmaps = new ByteBackedBitmap[frameCount];

        for ( int i = 0; i < bitmaps.Length; i++ ) {
            ByteBackedBitmap bbb = new( width, height, PixelFormat.Format16bppArgb1555 );
            byte[] curr = bbb.bytes;
            br.Read( curr );
            bitmaps[i] = bbb;

            if ( i == 0 ) {
                continue;
            }

            byte[] prev = bitmaps[i - 1].bytes;
            for ( int j = 0; j < curr.Length; j += 2 ) {
                // the format is ARGB_1555, Little Endian
                short curVal = (short) ( ( curr[j + 1] << 8 ) | curr[j] );
                short modVal = (short) ( ( prev[j + 1] << 8 ) | prev[j] );
                curVal += modVal;
                curr[j] = (byte) curVal;
                curr[j + 1] = (byte) ( curVal >> 8 );
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
