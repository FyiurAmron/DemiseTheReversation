namespace DemiseTheReversation {

using System;
using System.IO;
using Utils;

public class DemiseObject : DemiseAsset {
    public override long load( byte[] sourceBytes ) {
        using MemoryStream ms = new( sourceBytes );
        using BinaryReader br = new( ms );

        string magic = br.readString( 8 );
        switch ( magic ) {
            case "MDOv1.2\x1A":
            case "MDOv1.3\x1A":
            case "DEOv1.3\x1A":
            case "DEOv1.5\x1A":
                break;
            default:
                throw new FileFormatException( $"unknown magic: {magic}" );
        }

        short matCount = br.ReadInt16();
        short objCount = br.ReadInt16(); // ??

        if ( objCount > 1 ) {
            Console.Out.Write( $"{fileUtil.name} matCount: {matCount} u2: {objCount}\n" );
        }

        return ms.Position;
    }
}

}
