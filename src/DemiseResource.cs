namespace DemiseTheReversation {

using System;
using System.IO;
using Utils;

public class DemiseResource : IDemiseAsset {
    public string name { get; init; }
    public int derOffset { get; init; }
    public int derSize { get; init; }
    public int realSize { get; init; }

    public byte[] bytes { get; private set; } = Array.Empty<byte>();

    public int hash;

    public FileUtil fileUtil { get; init; }

    public DemiseResource() {
    }

    public byte[] getBytesFromDer( byte[] derBytes ) {
        byte[] dstBytes = new byte[derSize];
        Array.Copy( derBytes, derOffset, dstBytes, 0, derSize );
        return dstBytes;
    }

    public long loadFromDer( byte[] derBytes ) {
        return load( getBytesFromDer( derBytes ) );
    }

    public long load( byte[] sourceArray ) {
        byte[] xorMask = new byte[0x200];
        for ( int i = 0; i < xorMask.Length; i++ ) {
            hash *= 0x0003_43FD; // ditto IMUL
            hash += 0x0026_9EC3;
            int tmp = hash;
            tmp <<= 4;
            tmp >>= 24;
            tmp ^= 0xE3;
            xorMask[i] = (byte) tmp;
        }

        int esi = 0x5D;
        for ( int i = 0; i < derSize; i++ ) {
            int xorMaskIdx = esi & 0x01ff;
            sourceArray[i] ^= xorMask[xorMaskIdx];
            esi += 0x000D_6543;
            xorMaskIdx = ( esi >> 3 ) & 0x01FF;
            esi ^= xorMask[xorMaskIdx];
        }

        bytes = Zlib.inflateZlibBytes( sourceArray, realSize );

        return realSize;
    }

    public override string ToString() {
        return @$"{name} @ {derOffset} : {derSize} -> {realSize}";
    }

    public void save( string path ) {
        File.WriteAllBytes( $"{path}/{name}", bytes );
    }
}

}
