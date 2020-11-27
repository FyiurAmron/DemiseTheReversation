using System.Windows.Forms;
using System.IO;
using System;

namespace DemiseReversed {

using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class MenuExtensions {
    public static void add( this ToolStripItemCollection tsic, params ToolStripItem[] items ) {
        tsic.AddRange( items );
    }
}

public static class StreamExtensions {
    public static void skip( this BinaryReader binaryReader, int offset ) {
        binaryReader.seek( offset, SeekOrigin.Current );
    }

    public static void seek( this BinaryReader binaryReader, int offset, SeekOrigin origin = SeekOrigin.Begin ) {
        binaryReader.BaseStream.Seek( offset, origin );
    }

    public static bool isAvailable( this BinaryReader binaryReader ) {
        Stream stream = binaryReader.BaseStream;
        return stream.Position < stream.Length;
    }
}

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

    public byte[] DED_XOR_MASK = {
        0x21, 0x86, 0xA1, 0xAE, 0xF0, 0x1D, 0xB1, 0xFC, 0x1D, 0xA1, 0x55, 0xDC, 0x9C, 0x47, 0x49, 0x80,
        0x8B, 0xBF, 0x60, 0x65, 0xFD, 0xDA, 0xFD, 0x47, 0x80, 0xDD, 0x13, 0x7F, 0x8C, 0xDA, 0x80, 0xC2,
        0xA7, 0x51, 0xEA,
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

    public class Item {
        public string name;
    }

    private void openDER( string filePath ) {
        throw new NotImplementedException();
    }

    private void openDED( string filePath ) {
        string fileName = Path.GetFileName( filePath );

        Console.Out.Write( filePath );
        byte[] bs = File.ReadAllBytes( filePath );
        //Stream fileStream = openFileDialog.OpenFile();
        //using BinaryReader reader = new( fileStream );
        //reader.ReadBytes( ... );
        Console.Out.Write( " =>" );

        if ( CODED_DED_FILES.Contains( fileName ) ) {
            for ( int i = 0; i < bs.Length; i++ ) {
                bs[i] ^= DED_XOR_MASK[i % DED_XOR_MASK.Length];
            }
        }

        if ( fileName == "DEMISEItems.DED" ) {
            using MemoryStream ms = new( bs );
            using BinaryReader br = new( ms );
            int magic = br.ReadInt32();
            if ( magic != 0x380A9EFA ) {
                throw new FileFormatException();
            }

            br.skip( 20 ); // unknown magic

            List<Item> items = new();
            while ( br.isAvailable() ) {
                short nameLen = br.ReadInt16();
                if ( br.ReadInt16() != nameLen ) {
                    throw new FileFormatException();
                }

                Item i = new() {
                    name = Encoding.Default.GetString( br.ReadBytes( nameLen ) )
                };
                br.skip( 304 );
                // Console.Out.WriteLine( i.name );
            }
        }

        Console.Out.Write( " memory" );

        // string newFilePath = filePath + ".decoded";
        // File.WriteAllBytes( newFilePath, bs );
        // Console.Out.WriteLine( newFilePath );
    }
}

}
