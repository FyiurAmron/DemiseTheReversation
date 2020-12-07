namespace DemiseTheReversation {

using System;
using System.IO;
using Utils;

public class DemiseResource {
    public string name { get; init; }
    public int derOffset { get; init; }
    public int derSize { get; init; }
    public int realSize { get; init; }

    public byte[] bytes { get; private set; } = Array.Empty<byte>();

    public int hash; // TEMP

    public DemiseResource() {
    }

    public void readBytes( byte[] derBytes ) {
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

        bytes = new byte[derSize];
        Array.Copy( derBytes, derOffset, bytes, 0, derSize );

        int esi = 0x5D;
        for ( int i = 0; i < derSize; i++ ) {
            int xorMaskIdx = esi & 0x01ff;
            bytes[i] ^= xorMask[xorMaskIdx];
            esi += 0x000D_6543;
            xorMaskIdx = ( esi >> 3 ) & 0x01FF;
            esi ^= xorMask[xorMaskIdx];
        }

        bytes = Zlib.inflateZlibBytes( bytes, realSize );
    }

    public override string ToString() {
        return @$"{name} @ {derOffset} : {derSize} -> {realSize}";
    }

    public void writeToPath( string path ) {
        File.WriteAllBytes( path + "/" + name, bytes );
    }
}

}
