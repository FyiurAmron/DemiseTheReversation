namespace DemiseTheReversation {

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

public class DemiseAnimationHandler : IDemiseFileHandler {
    private readonly AutoForm parent;

    private string path;
    private string fileNameNoExt;
    private Image[] frames;

    public DemiseAnimationHandler( AutoForm parent ) {
        this.parent = parent;
    }

    public void open( string filePath ) {
        Console.Out.Write( filePath );
        string fileName = Path.GetFileName( filePath );
        fileNameNoExt = Path.GetFileNameWithoutExtension( filePath );
        path = Path.GetDirectoryName( filePath )
            ?? throw new ArgumentException( $"invalid path '{filePath}'" );
        byte[] sourceArray = File.ReadAllBytes( filePath );

        using MemoryStream ms = new( sourceArray );
        using BinaryReader br = new( ms );

        string magic = br.readString( 8 );
        if ( magic != DemiseConsts.IWA_1_2_MAGIC && magic != DemiseConsts.DEA_1_2_MAGIC ) {
            throw new FileFormatException();
        }

        int width = br.ReadInt32(); // + 1;
        int height = br.ReadInt32();
        int delayMs = br.ReadInt32();
        int frameCount = br.ReadInt32();
        int notBook = br.ReadInt32(); // 0 for books, 1 for other

        long sourceIndex = ms.Position; // int really
        // long bmpLen = sourceArray.Length - sourceIndex; // ditto
        int frameLength = width * height * sizeof(short);
        byte[][] bmpBytes = new byte[frameCount][];
        bmpBytes[0] = new byte[frameLength];
        Array.Copy( sourceArray, sourceIndex, bmpBytes[0], 0, frameLength );
        long srcPtr = sourceIndex + frameLength;
        for ( int i = 1; i < frameCount; i++ ) {
            bmpBytes[i] = new byte[frameLength];
            Array.Copy( bmpBytes[i - 1], bmpBytes[i], frameLength );
            for ( int j = 0; j < frameLength; j += 2, srcPtr += 2 ) {
                short prev = (short) ( ( bmpBytes[i][j + 1] << 8 ) | bmpBytes[i][j] );
                short mods = (short) ( ( sourceArray[srcPtr + 1] << 8 ) | sourceArray[srcPtr] );
                prev += mods;
                bmpBytes[i][j] = (byte) prev;
                bmpBytes[i][j + 1] = (byte) ( prev >> 8 );
            }
        }

        Console.Out.WriteLine( $"{srcPtr} of {sourceArray.Length} read" );

        AutoForm previewForm = new( mdiParent: parent ) {
            Text = fileName,
        };

        PictureBox animPb = new() {
            SizeMode = PictureBoxSizeMode.AutoSize
        };
        previewForm.autoControls.Add( animPb );

        frames = new Image[frameCount * 2];
        int[] delays = new int[frameCount * 2];

        void addPictureBox( int i ) {
            Bitmap bmp = new( width, height, // 0,
                              PixelFormat.Format16bppArgb1555 //,
                // Marshal.UnsafeAddrOfPinnedArrayElement( bmpBytes, 0 )
            );

            Rectangle BoundsRect = new( 0, 0, width, height );
            BitmapData bmpData = bmp.LockBits( BoundsRect, ImageLockMode.WriteOnly, bmp.PixelFormat );
            IntPtr ptr = bmpData.Scan0;

            System.Runtime.InteropServices.Marshal.Copy( bmpBytes[i], 0, ptr, bmpBytes[0].Length );
            bmp.UnlockBits( bmpData );

            frames[i] = bmp;
            frames[frameCount * 2 - i - 1] = bmp;
            delays[i] = delayMs;
            delays[frameCount * 2 - i - 1] = delayMs;

            PictureBox pb = new() {
                Image = bmp,
                SizeMode = PictureBoxSizeMode.AutoSize
            };

            previewForm.autoControls.Add( pb );
        }

        for ( int i = 0; i < frameCount; i++ ) {
            addPictureBox( i );
        }

        animPb.Image = AnimImage.createAnimation( animPb, frames, delays );

        ToolStripMenuItemEx actionOpenMenuItem = new( ( _, _ ) => unpack() ) {
            Text = @$"&Unpack {fileName}",
            ShortcutKeys = Keys.Control | Keys.U,
        };
        ToolStripMenuItemEx fileMenu = new() {
            Text = @"&Action",
            DropDownItems = { actionOpenMenuItem }
        };
        previewForm.menuStrip.Items.Add( fileMenu );

        previewForm.Show();
    }

    public void unpack() {
        string outputDir = $"{path}/{fileNameNoExt}";
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
