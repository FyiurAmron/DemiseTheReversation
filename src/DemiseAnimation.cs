namespace DemiseTheReversation {

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Utils;

public class DemiseAnimation : IDemiseAsset {
    public const string IWA_1_2_MAGIC = "IWAv1.2\0";
    public const string DEA_1_2_MAGIC = "DEAv1.2\0";

    public FileUtil fileUtil { get; init; }

    public string magic { get; private set; }

    public int width { get; private set; } // ??? sometimes + 1 (stride?)
    public int height { get; private set; }
    public int delayMs { get; private set; }
    public int frameCount { get; private set; }
    public int notBook { get; private set; } // ??? 0 for books, 1 for other

    public byte[][] bmpBytes { get; private set; } // ARGB_1555, Little Endian

    public DemiseAnimation() {
    }

    public long load( byte[] sourceArray ) {
        using MemoryStream ms = new( sourceArray );
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

        // long bmpLen = sourceArray.Length - sourceIndex; // ditto
        int frameLength = width * height * sizeof(short);
        bmpBytes = new byte[frameCount][];
        bmpBytes[0] = new byte[frameLength];
        br.Read( bmpBytes[0] );
        for ( int i = 1; i < bmpBytes.Length; i++ ) {
            bmpBytes[i] = new byte[frameLength];
            byte[] curr = bmpBytes[i],
                   prev = bmpBytes[i - 1];
            br.Read( bmpBytes[i] );
            for ( int j = 0; j < frameLength; j += 2 ) {
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

    public Bitmap getFrameAsBitmap( int i ) {
        Bitmap bmp = new( width, height, // 0,
                          PixelFormat.Format16bppArgb1555 //,
            // Marshal.UnsafeAddrOfPinnedArrayElement( bmpBytes, 0 )
        );

        // TODO/FIXME try using Bitmap c-tor with marshalling

        Rectangle boundsRect = new( 0, 0, width, height );
        BitmapData bmpData = bmp.LockBits( boundsRect, ImageLockMode.WriteOnly, bmp.PixelFormat );
        IntPtr ptr = bmpData.Scan0;

        System.Runtime.InteropServices.Marshal.Copy(
            bmpBytes[i], 0, ptr, bmpBytes[i].Length );
        bmp.UnlockBits( bmpData );

        return bmp;
    }
}

}
