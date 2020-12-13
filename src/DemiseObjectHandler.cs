namespace DemiseTheReversation {

using System;
using FormUtils;

public class DemiseObjectHandler : DemiseFileHandler<DemiseObject> {
    public DemiseObjectHandler( AutoForm parent ) : base( parent ) {
    }

    public override IDemiseFileHandler open( string filePath ) {
        Console.Out.Write( $"loading {filePath} ... " );
        fileUtil = new( filePath );

        demiseAsset = new DemiseObject() {
            fileUtil = fileUtil
        };
        
        long srcPtr = loadAsset();

        Console.Out.WriteLine( $"{srcPtr} of {fileUtil.length} read." );

        return this;
    }

    public override void unpack( string outputDir ) {
        // throw new System.NotImplementedException();
    }

    public override AutoForm showPreview() {
        createPreviewForm();

        // addSaveMenuAndShow();
        previewForm.Show();

        return previewForm;
    }
}

}
