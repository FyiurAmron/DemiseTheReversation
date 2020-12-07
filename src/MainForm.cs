namespace DemiseTheReversation {

using System.Windows.Forms;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static DemiseConsts;
using static Misc;

// sadly, the designer doesn't work ATM for derived classes
public partial class MainForm : AutoForm {
    private IDemiseFileHandler currentFileHandler;

    public MainForm() {
        InitializeComponent(); // needed for Designer

        addMenus( menuStrip );

        IsMdiContainer = true;

        // fileOpenEventHandler( null, null );
    }

    private void addMenus( MenuStrip ms ) {
        SuspendLayout();

        ToolStripMenuItemEx fileOpenMenuItem = new( fileOpenEventHandler ) {
            Text = @"&Open",
            ShortcutKeys = Keys.Control | Keys.O,
        };
        ToolStripMenuItemEx exitMenuItem = new( ( _, _ ) => Application.Exit() ) {
            Text = @"E&xit",
            ShortcutKeys = Keys.Control | Keys.Q,
        };

        ToolStripMenuItemEx fileMenu = new() {
            Text = @"&File",
            DropDownItems = {
                fileOpenMenuItem,
                new ToolStripSeparator(),
                exitMenuItem
            }
        };

        ToolStripMenuItemEx windowMenu = new() { Text = @"&Windows" };

        ms.MdiWindowListItem = windowMenu;

        ms.Items.add( fileMenu, /*actionMenu,*/ windowMenu );

        ResumeLayout();
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
                currentFileHandler = new DemiseResourceHandler( this );
                break;
            case FileType.DEA:
                currentFileHandler = new DemiseAnimationHandler( this );
                break;
        }

        currentFileHandler?.open( filePath );
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
