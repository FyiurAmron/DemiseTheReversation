namespace DemiseTheReversation {

using System.Windows.Forms;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static DemiseConsts;
using static Misc;

public partial class MainForm : Form {
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

    private void fileOpenEventHandler( object? sender, EventArgs eventArgs ) {
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
                openDER( filePath );
                break;
        }
    }

    public static readonly Map<int, string> RESOURCE_TYPES = new() {
        [47054] = "item 1",
        [47056] = "item 2",
        [49206] = "portrait 1",
    };

    public class Resource {
        public string name = "";
        public int offset;
        public int length;
        public int type; // still unsure about this... what else could this be? size? bit mask?

        public override string ToString() {
            return @$"[{RESOURCE_TYPES[type] ?? "" + type}] {name} @ {offset} +{length}";
        }
    }

    public static void rol( ref uint n, int shift ) {
        n = ( n << shift ) | ( n >> ( 32 - shift ) );
    }

    public static uint hashDER1( IEnumerable<byte> assetName, int seed ) {
        uint esi = (uint)seed;
        uint ecx = esi;
        foreach ( byte b in assetName ) {
            rol( ref ecx, 3 );
            ecx ^= b;
            ecx ^= esi;
            //ecx ^= unchecked((int) 0xABCDABCD);
            ecx ^= 0xABCDABCD;
            esi = ecx;
        }

        return esi;
    }

    private void openDER( string filePath ) {
        Console.Out.Write( filePath );
        string fileNameNoExt = Path.GetFileNameWithoutExtension( filePath );
        string path = Path.GetDirectoryName( filePath )
            ?? throw new ArgumentException( $"invalid path '{filePath}'" );
        byte[] bytes = File.ReadAllBytes( filePath );
        Console.Out.Write( " =>" );
        // all DER are alike
        using MemoryStream ms = new( bytes );
        using BinaryReader br = new( ms );
        string magic = br.readString( 8 );
        if ( magic != DER_1_3_MAGIC ) {
            throw new FileFormatException();
        }

        int assetCatalogOffset = br.ReadInt32();
        br.seek( assetCatalogOffset );
        int assetCount = br.ReadInt32();
        Console.Out.Write( $"useless int32: {br.ReadInt32()}" ); // check if always 0

        Vector<Resource> resources = new();
        for ( int i = 0; i < assetCount; i++ ) {
            Resource res = new();
            int nameLen = br.ReadInt32();
            byte[] nameBytes = br.ReadBytes( nameLen ); // in-place on bytes[] is faster, but this is shorter
            xorMask( nameBytes, DER_NAME_XOR_MASK1 );
            if ( nameLen > DER_NAME_XOR_MASK1.Length ) { // happens seldom, but does happen indeed
                xorMask( nameBytes, DER_NAME_XOR_MASK2, DER_NAME_XOR_MASK1.Length, nameBytes.Length ); // looped mask
            }

            res.name = nameBytes.toString();
            res.offset = br.ReadInt32();
            res.length = br.ReadInt32();
            res.type = br.ReadInt32();

            resources.Add( res );
        }

        //Directory.CreateDirectory( $"{path}/{fileNameNoExt}" );

        var hash2 = hashDER1( Encoding.ASCII.GetBytes( fileNameNoExt.ToUpper() ), DER_XOR1_RESOURCE_FILE_NAME_SEED );

        foreach ( Resource res in resources ) {
            //Console.Out.WriteLine( res );
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
            xorMask( bytes, DED_ASC_HEADER_XOR_MASK, 12, Math.Min( DED_HEADER_END_OFFSET, bytes.Length ) );
            xorMask( bytes, DED_ASC_XOR_MASK, DED_HEADER_END_OFFSET, bytes.Length );
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
