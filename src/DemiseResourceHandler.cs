namespace DemiseTheReversation {

using System;
using System.IO;
using System.Windows.Forms;
using FormUtils;

public class DemiseResourceHandler : DemiseFileHandler<DemiseResourceMap> {
    public DemiseResourceHandler( AutoForm parent ) : base( parent ) {
    }

    public override IDemiseFileHandler open( string filePath ) {
        Console.Out.Write( $"loading {filePath} ... " );
        fileUtil = new( filePath );

        demiseAsset = new DemiseResourceMap() {
            fileUtil = fileUtil,
        };

        long readCount = loadAsset();

        Console.Out.WriteLine( $"{readCount} of {fileUtil.length} read." );

        return this;
    }

    public override AutoForm showPreview() {
        createPreviewForm();

        ListBox listBox = new() {
            AutoSize = true,
            MaximumSize = new( 750, 500 ),
            ScrollAlwaysVisible = true,
        };
        listBox.BeginUpdate();
        listBox.Items.AddRange( demiseAsset.toObjects() );
        listBox.EndUpdate();

        previewForm.autoControls.Add( listBox );

        addUnpackMenuAndShow();

        return previewForm;
    }

    public override void unpack( string outputDir ) {
        Directory.CreateDirectory( outputDir );
        Console.Out.WriteLine( $"output dir: {outputDir}" );

        foreach ( DemiseResource res in demiseAsset ) {
            // // in case we need to separate the files based on some magic, we have to e.g.:
            // Utils.FileUtil fu = new( res.name );
            // if ( fu.ext.ToUpper() == DemiseFilenameConsts.FileType.DET.ToString() ) {
            //     DemiseTexture dt = new();
            //     dt.load( res.bytes );
            //     string outputDir2 = $"{outputDir}/{dt.type:X8}/{dt.frameCount:D2}";
            //     Directory.CreateDirectory( outputDir2 );
            //     Console.Out.WriteLine( $"writing: {res.name} ..." );
            //     res.save( outputDir2 );
            //     continue;
            // }

            Console.Out.WriteLine( $"writing: {res.name} ..." );
            res.save( outputDir );
        }
    }
}

}
