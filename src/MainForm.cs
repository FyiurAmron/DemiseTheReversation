using System.Windows.Forms;
using System.IO;
using System;

namespace DemiseTheReversation {

using System.Collections.Generic;
using System.Linq;
using System.Text;

public enum FileType {
    DED = 1,
    DER = 2,
}

public partial class MainForm : Form {
    public string[] CODED_DED_FILES = {
        "DEMISETextures.DED",
        "DEMISEBountyData.DED",
        "DEMISEData.DED",
        "DEMISEDungeon.DED",
        "DEMISEDungeonGroups.DED",
        "DEMISEGuildTitles.DED",
        "DEMISEInfoDungeon.DED",
        "DEMISEInfoGeneral.DED",
        "DEMISEInfoItems.DED",
        "DEMISEInfoMonsters.DED",
        "DEMISEInfoQuest.DED",
        "DEMISEInfoRaceGuild.DED",
        "DEMISEInfoSpell.DED",
        "DEMISEInfoStory.DED",
        "DEMISEInfoTalk.DED",
        "DEMISEMazeItemIndex.DED",
        "DEMISELanguages.DED",
        "DEMISEMainStoryline.DED",
        "DEMISEMazeItems.DED",
        "DEMISEMonsters.DED",
        "DEMISEObjects.DED",
        "DEMISEObjectsI.DED",
        "DEMISEQuestData.DED",
        "DEMISESketchData.DED",
        "DEMISESoundData.DED",
        "DEMISESpells.DED",
        "DEMISESystemData.DED",
        "DEMISECharacters.DED",
        "DEMISEMultiConfig.DED",
        //"DEMISEGameNews.DED",
        //"DEMISEGameNewsIndex.DED",
        //"DEMISEAutomap.DED",
        "DEMISEMazeMonsters.DED",
        "DEMISELibrary.DED",
        "DEMISEHallOfRecords.DED",
        "DEMISEGuildMasters.DED",
        //"DEMISEGuildLogs.DED",
        "DEMISEStoryIndex.DED",
        "DEMISEStore.DED",
        "DEMISECompanionStore.DED",
        "DEMISEGuildSpells.DED",
        "DEMISEObjectsISP.DED",
        "DEMISEItems.DED",
        "DEMISEGameBounties.DED",
        //"DEMISEPartyData.DED",
        "DEMISEBountyIndex.DED",
        "DEMISEItemIndex.DED",
        "DEMISEAutomapAnnotations.DED",
        "DEMISEAutomapAnnotationsIndex.DED",
    };

    public byte[] DED_HEADER_XOR_MASK = {
        0xA7, 0x8F, 0x56, 0xFD, 0x3E, 0x1D, 0x7C, 0xBD, 0xDC, 0xE6, 0xBE, 0x6D,
    };

    public byte[] DED_ASC_HEADER_XOR_MASK = {
        /*0xb9,0xb6,0x85,0xd7,*/ 0x7F, 0xDC, 0xA5, 0xB5, /* DED_ASC_XOR_MASK */ 0x1E, 0x2E, 0x9D, 0xF4, 0xCE, 0x38,
        0xB0, 0xC6,
    };

    public byte[] DED_XOR_MASK = {
        0x80, 0xDD, 0x13, 0x7F, 0x8C, 0xDA, 0x80, 0xC2, 0xA7, 0x51, 0xEA, 0x21, 0x86, 0xA1, 0xAE, 0xF0,
        0x1D, 0xB1, 0xFC, 0x1D, 0xA1, 0x55, 0xDC, 0x9C, 0x47, 0x49, 0x80, 0x8B, 0xBF, 0x60, 0x65, 0xFD,
        0xDA, 0xFD, 0x47,
    };

    public byte[] DED_ASC_XOR_MASK = {
        0x1E, 0x2E, 0x9D, 0xF4, 0xCE, 0x38, 0xB0, 0xC6,
    };

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
            Filter = "DED files (*.DED)|*.DED|DER files (*.DER)|*.DER|All files (*.*)|*.*",
        };

        if ( openFileDialog.ShowDialog() != DialogResult.OK ) {
            return;
        }

        string filePath = openFileDialog.FileName;
        string ext = Path.GetExtension( filePath );

        FileType ft = openFileDialog.FilterIndex switch {
            1 => FileType.DED,
            2 => FileType.DER,
            _ => ext switch {
                "DED" => FileType.DED,
                "DER" => FileType.DER,
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

    public const int STAT_IDX_MAX = 6;

    public static string[] ITEM_TYPE_NAMES = {
        "Te-Waza",
        "Dagger",
        "Cross",
        "Sword",
        "Staff",
        "Mace",
        "Axe",
        "Hammer",
        "Leather Armor",
        "Chain Armor",
        "Plate Armor",
        "Shield",
        "Cap",
        "Helmet",
        "Gloves",
        "Gauntlets",
        "Cloak",
        "Bracers",
        "Sash",
        "Belt",
        "Boots",
        "Ring",
        "Amulet",
        "Potion",
        "Scroll",
        "Tome",
        "Dust",
        "Crystal",
        "Rod",
        "Stone",
        "Sphere",
        "Cube",
        "Artifact",
        "Miscellaneous Item",
        "Guild Crest",
        "Treatise",
        //
        "Weapon",
        "Armor",
        "Shield",
        "Head Protection",
        "Hand Protection",
        "Cloth",
        "Foot Protection",
        "Wrist Protection",
        "Garment",
        "Ornament",
        "Liquid",
        "Particles",
        "Parchment",
        "Book",
        "Stick",
        "Neck Ornament",
        "Round Object",
        "Square Object",
        "Waist Protection",
        "Old Object",
        "Element",
        "Glittering Object",
        "Crest",
        "Miscellaneous Object",
    };

    public class Item {
        public string name = "";
        public short idx;
        public short att, def;

        public int price; // negative -> cursed

        //public bool cursed;
        public Vector<short> statReq = new();
        public Vector<short> statMod = new();
        public short swings;
        public float damMod; // ?? for non-weapons
        public short d1;
        public short hands;
        public short type;

        public override string ToString() {
            return $"{name} {{{idx}}} A{att}/D{def} [{damMod}]x{swings} {hands}-handed {d1} {ITEM_TYPE_NAMES[type]} " +
                $"req {statReq} mod {statMod}"; // + " ${cost}";
        }
    }

    private void openDER( string filePath ) {
        throw new NotImplementedException();
    }

    private void xorMask( byte[] bytes, byte[] mask, int start, int end ) {
        for ( int i = start; i < end; i++ ) {
            bytes[i] ^= mask[( i - start ) % mask.Length];
        }
    }
    
    public const int HEADER_END_OFFSET = 24;

    private void openDED( string filePath ) {
        string fileName = Path.GetFileName( filePath );

        Console.Out.Write( filePath );
        byte[] bytes = File.ReadAllBytes( filePath );
        //Stream fileStream = openFileDialog.OpenFile();
        //using BinaryReader reader = new( fileStream );
        //reader.ReadBytes( ... );
        Console.Out.Write( " =>" );

        if ( CODED_DED_FILES.Contains( fileName ) ) {
            //xorMask( bytes, DED_HEADER_XOR_MASK, 0, 24 );
            //xorMask( bytes, DED_XOR_MASK, 24, bytes.Length );
            xorMask( bytes, DED_ASC_HEADER_XOR_MASK, 12, Math.Min( HEADER_END_OFFSET, bytes.Length ) );
            xorMask( bytes, DED_ASC_XOR_MASK, HEADER_END_OFFSET, bytes.Length );
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
