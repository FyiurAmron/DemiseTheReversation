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
        short groupCount = br.ReadInt16(); // ??

        if ( groupCount > 1 ) {
            Console.Out.Write( $"{fileUtil.name} matCount: {matCount} u2: {groupCount}\n" );
        }

        Map<string, DemiseObjectMaterial> materials = new();
        for ( int i = 0; i < matCount; i++ ) {
            DemiseObjectMaterial mat = new();
            mat.name = br.readString( 32 );
            mat.textureFilename = br.readString( 32 );
            br.ReadByte(); // R
            br.ReadByte(); // G
            br.ReadByte(); // B
            br.ReadByte(); // R
            br.ReadByte(); // G
            br.ReadByte(); // B
            Console.Out.Write( br.ReadInt32() + " " ); // always 0?
            Console.Out.Write( br.ReadInt32() + "\n" ); // always 0?
            br.ReadSingle();
            br.ReadInt16();
            br.ReadInt16();
            br.ReadInt16();
        }

        for ( int i = 0; i < groupCount; i++ ) {
            DemiseObjectGroup grp = new();
            grp.name = br.readString( 32 );
            br.ReadInt16();
            br.ReadInt16();
            br.ReadInt16();
            br.ReadInt16();
            br.ReadInt16();
            br.ReadInt16();
            br.ReadInt16();
            br.ReadInt16();
            br.ReadSingle();
            br.ReadSingle();
            br.ReadSingle();
            br.ReadSingle();
            // br.ReadSingle();
            // br.ReadSingle();
        }
        // now the frames begin: 32float, 32b string

        return ms.Position;
    }
}

public class DemiseObjectMaterial {
    public string name;
    public string textureFilename;
}

public class DemiseObjectGroup {
    public string name;
}

}
