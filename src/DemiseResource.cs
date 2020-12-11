namespace DemiseTheReversation {

using System;
using System.IO;
using Utils;

public sealed class DemiseResource : DemiseAsset {
    public string name { get; init; }
    public int derOffset { get; init; }
    public int derSize { get; init; }
    public int realSize { get; init; }

    public byte[] bytes { get; private set; } = Array.Empty<byte>();

    private readonly byte[] xorMask = new byte[0x200];

    public DemiseResource() {
    }

    public byte[] getBytesFromDer( byte[] derBytes ) {
        byte[] dstBytes = new byte[derSize];
        Array.Copy( derBytes, derOffset, dstBytes, 0, derSize );
        return dstBytes;
    }

    public void initXorMask( int hash ) {
        for ( int i = 0; i < xorMask.Length; i++ ) {
            hash *= 0x0003_43FD; // ditto IMUL
            hash += 0x0026_9EC3;
            int tmp = hash;
            tmp <<= 4;
            tmp >>= 24;
            tmp ^= 0xE3;
            xorMask[i] = (byte) tmp;
        }
    }

    public long loadFromDer( byte[] derBytes ) {
        return load( getBytesFromDer( derBytes ) );
    }

    public override long load( byte[] sourceBytes ) {
        int esi = 0x5D;
        for ( int i = 0; i < derSize; i++ ) {
            int xorMaskIdx = esi & 0x01ff;
            sourceBytes[i] ^= xorMask[xorMaskIdx];
            esi += 0x000D_6543;
            xorMaskIdx = ( esi >> 3 ) & 0x01FF;
            esi ^= xorMask[xorMaskIdx];
        }

        bytes = Zlib.inflateZlibBytes( sourceBytes, realSize );

        return realSize;
    }

    public byte[] save() {
        byte[] deflatedBytes = Zlib.deflate( bytes );

        int esi = 0x5D;
        for ( int i = 0; i < deflatedBytes.Length; i++ ) {
            int xorMaskIdx = esi & 0x01ff;
            deflatedBytes[i] ^= xorMask[xorMaskIdx];
            esi += 0x000D_6543;
            xorMaskIdx = ( esi >> 3 ) & 0x01FF;
            esi ^= xorMask[xorMaskIdx];
        }

        return deflatedBytes;
    }

    public override string ToString() {
        return @$"{name} @ {derOffset} : {derSize} -> {realSize}";
    }

    public void save( string path ) {
        File.WriteAllBytes( $"{path}/{name}", bytes );
    }
}

}
