namespace DemiseTheReversation {

using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using FormUtils;

public class DemiseAnimationHandler : DemiseFileHandler<DemiseAnimation> {
    private Image[] frames;

    public DemiseAnimationHandler( AutoForm parent ) : base( parent ) {
    }

    public override IDemiseFileHandler open( string filePath ) {
        Console.Out.Write( $"loading {filePath} ... " );
        fileUtil = new( filePath );

        demiseAsset = new DemiseAnimation() {
            fileUtil = fileUtil
        };
        
        long srcPtr = loadAsset();

        Console.Out.WriteLine( $"{srcPtr} of {fileUtil.length} read." );

        return this;
    }

    public override AutoForm showPreview() {
        createPreviewForm();

        PictureBox animPb = new() {
            SizeMode = PictureBoxSizeMode.AutoSize
        };
        previewForm.add( animPb );

        frames = new Image[demiseAsset.frameCount * 2];
        int[] delays = new int[demiseAsset.frameCount * 2];

        void addPictureBox( int i ) {
            Bitmap bmp = demiseAsset[i];

            int i2 = demiseAsset.frameCount * 2 - i - 1;

            frames[i] = bmp;
            frames[i2] = bmp;
            delays[i] = demiseAsset.delayMs;
            delays[i2] = demiseAsset.delayMs;

            PictureBox pb = new() {
                Image = bmp,
                SizeMode = PictureBoxSizeMode.AutoSize
            };

            previewForm.add( pb );
        }

        for ( int i = 0; i < demiseAsset.frameCount; i++ ) {
            addPictureBox( i );
        }

        animPb.Image = AnimImage.createAnimation( animPb, frames, delays );
        
        addUnpackMenuAndShow();
        
        return previewForm;
    }

    public override void unpack( string outputDir ) {
        Directory.CreateDirectory( outputDir );
        Console.Out.WriteLine( $"output dir: {outputDir}" );

        for ( int i = 0; i < frames.Length; i++ ) {
            string fileName = $"{i:D3}.png";
            Console.Out.WriteLine( $"writing: {fileName}..." );
            Image image = frames[i];
            image.Save( $"{outputDir}/{fileName}" );
        }
    }
}

}
