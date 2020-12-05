namespace DemiseTheReversation {

using System.Windows.Forms;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using static DemiseConsts;
using static Misc;

public partial class MainForm : Form {
    private IDemiseFileHandler currentFileHandler;

    public MainForm() {
        InitializeComponent();

        createMenus();
    }

    private void createMenus() {
        MenuStrip ms = new();

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

        MainMenuStrip = ms;
        Controls.Add( ms );
    }

    private void fileOpenEventHandler( object sender, EventArgs eventArgs ) {
        using OpenFileDialog openFileDialog = new() {
            Filter =
                @"Demise assets (*.DED, *.DER)|*.DED;*.DER|DED files (*.DED)|*.DED|DER files (*.DER)|*.DER|All files (*.*)|*.*",
        };

        if ( openFileDialog.ShowDialog() != DialogResult.OK ) {
            return;
        }

        string filePath = openFileDialog.FileName;
        string ext = Path.GetExtension( filePath );

        FileType ft = openFileDialog.FilterIndex switch {
            2 => FileType.DED,
            3 => FileType.DER,
            _ => ext switch {
                ".DED" => FileType.DED,
                ".DER" => FileType.DER,
                _ => throw new ArgumentException( $"unknown extension '{ext}'" )
            }
        };

        switch ( ft ) {
            case FileType.DED:
                openDED( filePath );
                break;
            case FileType.DER:
                currentFileHandler = new DemiseResourceHandler();
                currentFileHandler.open( filePath );
                currentFileHandler.unpack(); // TODO activate this via menu/button
                break;
        }
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
