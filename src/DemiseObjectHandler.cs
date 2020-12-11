namespace DemiseTheReversation {

using System;
using System.IO;
using FormUtils;

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
