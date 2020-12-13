namespace DemiseTheReversation {

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using FormUtils;
using Utils;
using static DemiseConsts;

public class DemiseDataHandler : DemiseFileHandler<DemiseData> {
    public static readonly string[] XORED_DED_FILES = {
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

    public static readonly byte[] DED_HEADER_XOR_MASK = {
        0xA7, 0x8F, 0x56, 0xFD, 0x3E, 0x1D, 0x7C, 0xBD, 0xDC, 0xE6, 0xBE, 0x6D,
    };

    public static readonly byte[] DED_ASC_HEADER_XOR_MASK = {
        0x7F, 0xDC, 0xA5, 0xB5, /* DED_ASC_XOR_MASK */ 0x1E, 0x2E, 0x9D, 0xF4, 0xCE, 0x38, 0xB0, 0xC6,
    };

    public static readonly byte[] DED_XOR_MASK = {
        0x80, 0xDD, 0x13, 0x7F, 0x8C, 0xDA, 0x80, 0xC2, 0xA7, 0x51, 0xEA, 0x21, 0x86, 0xA1, 0xAE, 0xF0,
        0x1D, 0xB1, 0xFC, 0x1D, 0xA1, 0x55, 0xDC, 0x9C, 0x47, 0x49, 0x80, 0x8B, 0xBF, 0x60, 0x65, 0xFD,
        0xDA, 0xFD, 0x47,
    };

    public static readonly byte[] DED_ASC_XOR_MASK = {
        0x1E, 0x2E, 0x9D, 0xF4, 0xCE, 0x38, 0xB0, 0xC6,
    };

    public const int DED_HEADER_MID_OFFSET = 12,
                     DED_HEADER_END_OFFSET = 24;

    private Control previewControl;

    public DemiseDataHandler( AutoForm parent ) : base( parent ) {
    }

    public override IDemiseFileHandler open( string filePath ) {
        Console.Out.Write( $"loading {filePath} ... " );
        fileUtil = new( filePath );

        byte[] bytes = fileUtil.load();

        // Console.Out.Write( " =>" );

        using MemoryStream ms = new( bytes );
        using BinaryReader br = new( ms );

        if ( XORED_DED_FILES.Contains( fileUtil.name ) ) {
            uint magic = br.ReadUInt32();
            byte[] xorMaskHeader,
                   xorMaskMain;
            switch ( magic ) {
                case 0x96AB_18DB:
                    // Demise/DTR
                    xorMaskHeader = DED_HEADER_XOR_MASK;
                    xorMaskMain = DED_XOR_MASK;
                    break;
                case 0x4CE1_ABBB:
                    // Demise: Ascension
                    xorMaskHeader = DED_ASC_HEADER_XOR_MASK;
                    xorMaskMain = DED_ASC_XOR_MASK;
                    break;
                case 0x2E30_0006:
                    // Mordor 2/IW etc. - no XORing nor 12-byte header, shouldn't happen due to not in the list
                    throw new FileFormatException();
                default:
                    throw new FileFormatException();
            }

            br.skip( 8 ); // unknown yet
            Bits.applyXorMask( bytes, xorMaskHeader, DED_HEADER_MID_OFFSET,
                               Math.Min( DED_HEADER_END_OFFSET, bytes.Length ) );
            Bits.applyXorMask( bytes, xorMaskMain, DED_HEADER_END_OFFSET, bytes.Length );
        }

        if ( fileUtil.name == "DEMISEItems.DED" ) { // TODO refactor to DemiseItemsData
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
        } else if ( fileUtil.name == "DEMISEInfoItems.DED" ) {
            // no version info here
            ListBox listBox = new() {
                AutoSize = true,
                MaximumSize = new( 750, 500 ),
                ScrollAlwaysVisible = true,
            };
            previewControl = listBox;
            ushort entryCount = br.ReadUInt16();
            int[] offsets = new int[entryCount];
            string[] itemInfos = new string[entryCount];
            for ( int i = 0; i < entryCount; i++ ) {
                offsets[i] = br.ReadInt32();
            }

            for ( int i = 0; i < entryCount; i++ ) {
                br.seek( offsets[i] + 11 );
                int strLen = br.ReadUInt16();
                itemInfos[i] = br.readString( strLen );
            }

            listBox.BeginUpdate();
            listBox.Items.AddRange( itemInfos.toObjectArray() );
            listBox.EndUpdate();
        }

        string newFilePath = filePath + ".decoded";
        File.WriteAllBytes( newFilePath, bytes );
        Console.Out.WriteLine( newFilePath );

        return this;
    }

    public override void unpack( string outputDir ) {
        // basically nothing to do here, as this format isn't usually packed
    }

    public override AutoForm showPreview() {
        createPreviewForm();

        if ( previewControl != null ) {
            previewForm.add( previewControl );
        }
        // TODO implement for at least DED Items

        // addSaveMenuAndShow();
        previewForm.Show();

        return previewForm;
    }
}

}
