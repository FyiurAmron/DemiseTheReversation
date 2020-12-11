namespace DemiseTheReversation {

using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using FormUtils;

public class DemiseTextureHandler : DemiseFileHandler<DemiseTexture> {
    private Image[] frames;

    public DemiseTextureHandler( AutoForm parent ) : base( parent ) {
    }

    public override IDemiseFileHandler open( string filePath ) {
        Console.Out.Write( $"loading {filePath} ... " );
        fileUtil = new( filePath );

        demiseAsset = new DemiseTexture() {
            fileUtil = fileUtil
        };

        long srcPtr = loadAsset();

        Console.Out.WriteLine( $"{srcPtr} of {fileUtil.length} read." );

        return this;
    }

    public override AutoForm showPreview() {
        createPreviewForm();
        // TODO refactor to some GfxPreviewForm etc.
        MouseEventHandler mouseHandler = ( _, args ) => {
            if ( args.Button != MouseButtons.Right ) {
                return;
            }

            ColorDialog colorDialog = new();
            if ( colorDialog.ShowDialog() == DialogResult.OK ) {
                previewForm.BackColor = colorDialog.Color;
            }
        };
        previewForm.MouseClick += mouseHandler;
        previewForm.flowLayoutPanel.MouseClick += mouseHandler;

        frames = new Image[demiseAsset.frameCount];

        void addPictureBox( int i ) {
            Bitmap bmp = demiseAsset[i];
            frames[i] = bmp;

            PictureBox pb = new() {
                Image = bmp,
                SizeMode = PictureBoxSizeMode.AutoSize
            };
            pb.MouseClick += mouseHandler;
            // Console.Out.WriteLine( pb.Size );
            previewForm.add( pb );
            // previewForm.Controls.Add( pb );
        }

        for ( int i = 0; i < demiseAsset.frameCount; i++ ) {
            addPictureBox( i );
        }

        // previewForm.flowLayoutPanel.MinimumSize = new( demiseAsset.width + 6, demiseAsset.height + 6 );
        // Console.Out.WriteLine( previewForm.flowLayoutPanel.Size );

        // previewForm.flowLayoutPanel.PerformLayout();
        // previewForm.flowLayoutPanel.Refresh();
        // previewForm.flowLayoutPanel.Invalidate();
        // previewForm.flowLayoutPanel.ResumeLayout(true);

        // previewForm.PerformLayout();
        // previewForm.Refresh();
        // previewForm.Invalidate();
        // previewForm.ResumeLayout(true);
        // previewForm.MinimumSize = new( demiseAsset.width, demiseAsset.height );

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
