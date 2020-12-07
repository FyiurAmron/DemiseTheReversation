using System;
using System.IO;

namespace DemiseTheReversation {

using System.Windows.Forms;
using System.IO;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using static DemiseConsts;
using static Misc;

public partial class MainForm : Form {
    private IDemiseFileHandler currentFileHandler;
    private ScrollableControl panel;

    public MainForm() {
        InitializeComponent(); // needed for Designer

        SuspendLayout();

        AutoSize = true;
        // AutoSizeMode = AutoSizeMode.GrowAndShrink;
        AutoSizeMode = AutoSizeMode.GrowOnly;
        // FormBorderStyle = FormBorderStyle.Sizable;

        FlowLayoutPanel flPanel = new();
        flPanel.BackColor = Color.Indigo;
        panel = flPanel;

        flPanel.AutoSize = true;
        flPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        flPanel.Dock = DockStyle.Top;
        // flPanel.AutoScroll = true;

        if ( panel != this ) {
            Controls.Add( panel );
        }

        // fileOpenEventHandler( null, null );

        MainMenuStrip = createMenus();
        MainMenuStrip.Dock = DockStyle.Top;
        Controls.Add( MainMenuStrip ); // has to be added last to order the docks properly

        ResumeLayout();
    }

    private MenuStrip createMenus() {
        MenuStrip ms = new() {
            RenderMode = ToolStripRenderMode.System
        };

        ToolStripMenuItem fileOpenMenuItem = new(
            "Open", null, fileOpenEventHandler, Keys.Control | Keys.O );
        ToolStripSeparator separatorMenuItem = new();
        ToolStripMenuItem exitMenuItem = new(
            "Exit", null, ( _, _ ) => Application.Exit(), Keys.Control | Keys.Q );

        ToolStripMenuItem fileMenu = new( "File" );
        ( (ToolStripDropDownMenu) fileMenu.DropDown ).ShowImageMargin = false;
        fileMenu.DropDownItems.add( fileOpenMenuItem, separatorMenuItem, exitMenuItem );

        fileMenu.DropDownItems.AddRange( new ToolStripItem[] {
            fileOpenMenuItem, separatorMenuItem, exitMenuItem
        } );

        ms.Items.Add( fileMenu );

        return ms;
    }

    private static string createFilterString() {
        string[] keys = FILE_TYPES.Keys.Select( ( s ) => "*." + s ).ToArray();

        StringBuilder sb = new();
        sb
            .Append( $"Demise assets ({keys.join( ", " )})|{keys.join( ";" )}|" )
            .appendAll( FILE_TYPES.Select( ( kv ) => $"{kv.Value} (*.{kv.Key})|*.{kv.Key}|" ) )
            .Append( "All files (*.*)|*.*" );

        return sb.ToString();
    }

    private void fileOpenEventHandler( object sender, EventArgs eventArgs ) {
        using OpenFileDialog openFileDialog = new() {
            Filter = createFilterString()
        };

        if ( openFileDialog.ShowDialog() != DialogResult.OK ) {
            return;
        }

        string filePath = openFileDialog.FileName;
        string ext = Path.GetExtension( filePath ).Substring( 1 ).ToUpper();

        int idx = openFileDialog.FilterIndex;
        FileType ft; // sadly, no way to genericize enum handling here
        if ( Enum.IsDefined( typeof(FileType), idx ) && idx > (int) FileType.DEMISE ) {
            ft = (FileType) idx;
        } else if ( !Enum.TryParse( ext, out ft ) || ft <= FileType.DEMISE ) {
            throw new ArgumentException( $"unknown extension '{ext}'" );
        }

        switch ( ft ) {
            case FileType.DED:
                openDED( filePath );
                break;
            case FileType.DER:
                currentFileHandler = new DemiseResourceHandler();
                currentFileHandler.open( filePath );
                currentFileHandler.unpack(); // TODO activate this via menu/button
                break;
            case FileType.DEA:
                openDEA( filePath );
                break;
        }
    }

    private byte[] argb1555ToBytes( byte b1, byte b2 ) {
        return new[] {
            (byte) ( b1 & 0b1000_0000 ),
            (byte) ( ( b1 & 0b0111_1100 ) << 1 ),
            (byte) ( ( ( b1 & 0b0000_0011 ) << 6 ) | ( ( b2 & 0b1110_0000 ) >> 2 ) ),
            (byte) ( ( b2 & 0b0001_1111 ) << 3 )
        };
    }

    private (byte, byte) bytesToArgb1555( byte[] bytes ) {
        byte b1 = bytes[0];
        b1 |= (byte) ( bytes[1] >> 1 );
        b1 |= (byte) ( bytes[2] >> 6 );
        byte b2 = (byte) ( bytes[2] << 2 );
        b2 |= (byte) ( bytes[3] >> 3 );
        return ( b1, b2 );
    }

    private void openDEA( string filePath ) {
        Console.Out.Write( filePath );
        string fileName = Path.GetFileName( filePath );
        byte[] sourceArray = File.ReadAllBytes( filePath );

        using MemoryStream ms = new( sourceArray );
        using BinaryReader br = new( ms );

        string magic = br.readString( 8 );
        if ( magic != IWA_1_2_MAGIC && magic != DEA_1_2_MAGIC ) {
            throw new FileFormatException();
        }

        int width = br.ReadInt32(); // + 1;
        int height = br.ReadInt32();
        int delayMs = br.ReadInt32();
        int frameCount = br.ReadInt32();
        int notBook = br.ReadInt32(); // 0 for books, 1 for other

        long sourceIndex = ms.Position; // int really
        long bmpLen = sourceArray.Length - sourceIndex; // ditto
        int frameLength = width * height * sizeof(short);
        byte[][] bmpBytes = new byte[frameCount][];
        bmpBytes[0] = new byte[frameLength];
        Array.Copy( sourceArray, sourceIndex, bmpBytes[0], 0, frameLength );
        long srcPtr = sourceIndex + frameLength;
        for ( int i = 1; i < frameCount; i++ ) {
            Console.Out.WriteLine();
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

        Console.Out.WriteLine( $"{srcPtr} -> {sourceArray.Length}" );

        PictureBox animPb = new() {
            SizeMode = PictureBoxSizeMode.AutoSize
        };
        panel.Controls.Add( animPb );

        Bitmap[] frames = new Bitmap[frameCount * 2];
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

            panel.Controls.Add( pb );
        }

        SuspendLayout();
        for ( int i = 0; i < frameCount; i++ ) {
            addPictureBox( i );
        }

        animPb.Image = AnimImage.createAnimation( animPb, frames, delays );

        ResumeLayout();
    }

    private void openDED( string filePath ) {
        Console.Out.Write( filePath );
        string fileName = Path.GetFileName( filePath );
        byte[] bytes = File.ReadAllBytes( filePath );
        //Stream fileStream = openFileDialog.OpenFile();
        //using BinaryReader reader = new( fileStream );
        //reader.ReadBytes( ... );
        Console.Out.Write( " =>" );

        if ( XORED_DED_FILES.Contains( fileName ) ) {
            //xorMask( bytes, DED_HEADER_XOR_MASK, 0, 24 );
            //xorMask( bytes, DED_XOR_MASK, 24, bytes.Length );
            applyXorMask( bytes, DED_ASC_HEADER_XOR_MASK, 12, Math.Min( DED_HEADER_END_OFFSET, bytes.Length ) );
            applyXorMask( bytes, DED_ASC_XOR_MASK, DED_HEADER_END_OFFSET, bytes.Length );
        }

        if ( fileName == "DEMISEItems.DED" ) {
            using MemoryStream ms = new( bytes );
            using BinaryReader br = new( ms );
            int magic = br.ReadInt32();
            // if ( magic != 0x380A9EFA ) {
            // throw new FileFormatException();
            // }

            br.skip( 8 ); // unknown yet

            Console.Out.WriteLine( $"v{br.ReadInt16()} {br.readString( 6 ).TrimEnd()} rev.{br.ReadInt16()}" );
            short expectedItemCount = br.ReadInt16();

            List<Item> items = new();
            SortedMap<int, Item> itemIndex = new();

            while ( br.isAvailable() ) {
                short nameLen = br.ReadInt16();
                if ( br.ReadInt16() != nameLen ) {
                    throw new FileFormatException();
                }

                Item it = new() {
                    name = br.readString( nameLen ),
                    idx = br.ReadInt16(),
                    att = br.ReadInt16(),
                    def = br.ReadInt16(),
                    price = br.ReadInt32(),
                };

                br.skip( 2 * 32 + 8 );
                it.swings = br.ReadInt16();
                br.skip( 16 );

                it.damMod = br.ReadSingle();
                it.d1 = br.ReadInt16();
                if ( br.ReadInt16() != 0 ) {
                    throw new FileFormatException();
                }

                it.hands = br.ReadInt16();
                it.type = br.ReadInt16();

                br.skip( 3 * 8 );

                for ( int i = 0; i < STAT_IDX_MAX; i++ ) {
                    it.statReq.Add( br.ReadInt16() );
                }

                if ( br.ReadInt16() != 0 ) {
                    throw new FileFormatException();
                }

                for ( int i = 0; i < STAT_IDX_MAX; i++ ) {
                    it.statMod.Add( br.ReadInt16() );
                }

                if ( br.ReadInt16() != 0 ) {
                    throw new FileFormatException();
                }

                br.skip( 140 );

                items.Add( it );
                itemIndex[it.idx] = it;

                Console.Out.WriteLine( it );
            }

            Console.Out.WriteLine( " memory" );

            Console.Out.WriteLine(
                $"items expected: {expectedItemCount} total: {items.Count} idx: [{itemIndex.First().Key}, {itemIndex.Last().Key}]" );
        }

        string newFilePath = filePath + ".decoded";
        File.WriteAllBytes( newFilePath, bytes );
        Console.Out.WriteLine( newFilePath );
    }
}

}
