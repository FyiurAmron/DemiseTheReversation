namespace DemiseTheReversation {

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Utils;

public sealed class DemiseResourceMap : DemiseAsset, IEnumerable<DemiseResource> {
    public const string DER_1_3_MAGIC = "DERv1.3\0";

    public const int DER_XOR1_ASSET_NAME_SEED = 0x0009_1C80;
    public const int DER_XOR1_DER_NAME_SEED = 0x000A_2F3B;

    public static readonly byte[] DER_NAME_XOR_MASK1 = {
        0x37, 0x29, 0xC6, 0x57, 0x4D, 0x82, 0x31, 0x18, 0x7C, 0x38, 0x5D, 0x11, 0x3A, 0x4C, 0x88, 0x6B,
        0x94, 0x90, 0xB7, 0xAA, 0x5A, 0x2E, 0xFB, 0x87, 0x18, 0x7B, 0x03, 0xB8, 0xDE, 0x8B,
    };

    public static readonly byte[] DER_NAME_XOR_MASK2 = {
        0x74,
    };

    private readonly Map<string, DemiseResource> resources = new();

    public DemiseResource this[ string key ] {
        get => resources[key];
        set => resources[key] = value;
    }

    public DemiseResourceMap() {
    }

    public IEnumerator<DemiseResource> GetEnumerator() {
        return resources.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

    public object[] toObjects() {
        return resources
               .Values
               .Select( ( res ) => res.name )
               .toObjectArray();
    }

    public override long load( byte[] bytes ) {
        using MemoryStream ms = new( bytes );
        using BinaryReader br = new( ms );
        string magic = br.readString( 8 );
        if ( magic != DER_1_3_MAGIC ) {
            throw new FileFormatException();
        }

        int assetCatalogOffset = br.ReadInt32();
        br.seek( assetCatalogOffset );
        int assetCount = br.ReadInt32();
        int reserved = br.ReadInt32();
        if ( reserved != 0 ) { // 0 in all the stock Demise DERs
            throw new FileFormatException();
        }

        int hash2 = calcHash( fileUtil.nameNoExt.ToUpper().toBytes(), DER_XOR1_DER_NAME_SEED );

        for ( int i = 0; i < assetCount; i++ ) {
            int nameLen = br.ReadInt32();
            byte[] nameBytes = br.ReadBytes( nameLen ); // in-place on bytes[] is faster, but this is shorter
            Bits.applyXorMask( nameBytes, DER_NAME_XOR_MASK1 );
            if ( nameLen > DER_NAME_XOR_MASK1.Length ) { // happens seldom, but does happen indeed
                Bits.applyXorMask( nameBytes, DER_NAME_XOR_MASK2, DER_NAME_XOR_MASK1.Length,
                                   nameBytes.Length ); // looped mask
            }

            int hash1 = calcHash( nameBytes, DER_XOR1_ASSET_NAME_SEED );
            int hash = hash1 * hash2; // was explicitly IMUL in asm
            hash += 4;
            hash ^= 0x0001_9871;

            string assetName = nameBytes.toString();
            DemiseResource res = new() {
                name = assetName,
                derOffset = br.ReadInt32(),
                derSize = br.ReadInt32(),
                realSize = br.ReadInt32(),

                fileUtil = new FileUtil( $"{fileUtil.pathNameNoExt}/{assetName}" )
            };
            res.initXorMask( hash );
            res.loadFromDer( bytes );
            this[res.name] = res;
        }

        return ms.Position;
    }

    private static int calcHash( IEnumerable<byte> assetName, uint seed ) {
        uint esi = seed;
        uint ecx = esi;
        foreach ( byte b in assetName ) {
            Bits.rol( ref ecx, 3 );
            ecx ^= b;
            ecx ^= esi;
            ecx ^= 0xABCD_ABCD;
            esi = ecx;
        }

        return (int) esi;
    }
}

}
