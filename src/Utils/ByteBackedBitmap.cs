namespace DemiseTheReversation.Utils {

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

public sealed class ByteBackedBitmap : IDisposable {
    public Bitmap bitmap { get; }
    public byte[] bytes { get; }
    private GCHandle pinnedBytes;

    public uint stride { get; }
    public uint bitsPerPixel { get; }

    public ByteBackedBitmap( int width, int height, PixelFormat pixelFormat ) {
        bitsPerPixel = ( (uint) pixelFormat << 16 ) >> 24;
        stride = Misc.roundUp( (uint) width * bitsPerPixel, 4 * 8 ) / 8;
        bytes = new byte[height * stride];

        pinnedBytes = GCHandle.Alloc( bytes, GCHandleType.Pinned );

        bitmap = new Bitmap( width, height, (int) stride, pixelFormat, pinnedBytes.AddrOfPinnedObject() );
    }

    public void Dispose() {
        bitmap?.Dispose();
        pinnedBytes.Free();
    }
}

}
