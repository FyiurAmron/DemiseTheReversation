namespace DemiseTheReversation {

using System;
using System.Collections.Generic;
using System.IO;
using static DemiseConsts;
using static Misc;

public class DemiseResourceHandler : IDemiseFileHandler {
    private static int calcHash( IEnumerable<byte> assetName, uint seed ) {
        uint esi = seed;
        uint ecx = esi;
        foreach ( byte b in assetName ) {
            rol( ref ecx, 3 );
            ecx ^= b;
            ecx ^= esi;
            ecx ^= 0xABCD_ABCD;
            esi = ecx;
        }

        return (int) esi;
    }

    private string path = "";
    private string fileNameNoExt = "";
    private readonly Map<string, DemiseResource> resources = new();
    private byte[] derBytes = Array.Empty<byte>();
    private int hash2;

    public void open( string filePath ) {
        Console.Out.Write( filePath );
        fileNameNoExt = Path.GetFileNameWithoutExtension( filePath );
        path = Path.GetDirectoryName( filePath )
            ?? throw new ArgumentException( $"invalid path '{filePath}'" );
        hash2 = calcHash( fileNameNoExt.ToUpper().toBytes(), DER_XOR1_DER_NAME_SEED );

        derBytes = File.ReadAllBytes( filePath );
        Console.Out.Write( " =>" );
        // all DER are alike
        using MemoryStream ms = new( derBytes );
        using BinaryReader br = new( ms );
        string magic = br.readString( 8 );
        if ( magic != DER_1_3_MAGIC ) {
            throw new FileFormatException();
        }

        int assetCatalogOffset = br.ReadInt32();
        br.seek( assetCatalogOffset );
        int assetCount = br.ReadInt32();
        int reserved = br.ReadInt32();
        /*
        if ( reserved != 0 ) { // 0 in all the stock Demise DERs
            throw new FileFormatException();
        }
        */

        //byte[] xorMask = new byte[0x200]; // 512
        for ( int i = 0; i < assetCount; i++ ) {
            int nameLen = br.ReadInt32();
            byte[] nameBytes = br.ReadBytes( nameLen ); // in-place on bytes[] is faster, but this is shorter
            applyXorMask( nameBytes, DER_NAME_XOR_MASK1 );
            if ( nameLen > DER_NAME_XOR_MASK1.Length ) { // happens seldom, but does happen indeed
                applyXorMask( nameBytes, DER_NAME_XOR_MASK2, DER_NAME_XOR_MASK1.Length,
                              nameBytes.Length ); // looped mask
            }

            int hash1 = calcHash( nameBytes, DER_XOR1_ASSET_NAME_SEED );
            int hash = hash1 * hash2; // was explicitly IMUL in asm
            hash += 4;
            hash ^= 0x0001_9871;

            DemiseResource res = new() {
                name = nameBytes.toString(),
                derOffset = br.ReadInt32(),
                derSize = br.ReadInt32(),
                realSize = br.ReadInt32(),

                hash = hash,
            };
            //res.readBytes( derBytes );
            Console.Out.WriteLine( res );

            resources.Add( res.name, res );
        }
    }

    public void unpack() {
        string outputDir = $"{path}/{fileNameNoExt}";
        Directory.CreateDirectory( outputDir );
        Console.Out.WriteLine( $"output dir: {outputDir}" );

        foreach ( DemiseResource res in resources.Values ) {
            Console.Out.WriteLine( $"writing: {res.name} ..." );
            res.readBytes( derBytes );
            res.writeToPath( outputDir );
        }
    }
}

}
