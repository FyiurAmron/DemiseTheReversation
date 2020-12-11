namespace DemiseTheReversation {

using System;
using System.IO;
using FormUtils;
using Utils;

public class DemiseObjectHandler : DemiseFileHandler<DemiseObject> {
    public DemiseObjectHandler( AutoForm parent ) : base( parent ) {
    }

    public override IDemiseFileHandler open( string filePath ) {
        Console.Out.Write( filePath );
        fileUtil = new( filePath );

        byte[] bytes = File.ReadAllBytes( filePath );

        Console.Out.Write( " =>" );

        using MemoryStream ms = new( bytes );
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
        
        br.readString( 8 );
        
        //br.readInt16();
        //br.readInt16();

        return this;
    }

    public override void unpack( string outputDir ) {
        // throw new System.NotImplementedException();
    }

    public override AutoForm showPreview() {
        createPreviewForm();

        // TODO implement for at least DED Items

        // addSaveMenuAndShow();
        previewForm.Show();

        return previewForm;
    }
}

}
