namespace DemiseTheReversation {

public class DemiseConsts {
    public enum FileType {
        DED = 1,
        DER = 2,
    }

    public static string[] XORED_DED_FILES = {
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

    public const int DED_HEADER_END_OFFSET = 24;

    public static readonly byte[] DER_NAME_XOR_MASK1 = {
        0x37, 0x29, 0xC6, 0x57, 0x4D, 0x82, 0x31, 0x18, 0x7C, 0x38, 0x5D, 0x11, 0x3A, 0x4C, 0x88, 0x6B,
        0x94, 0x90, 0xB7, 0xAA, 0x5A, 0x2E, 0xFB, 0x87, 0x18, 0x7B, 0x03, 0xB8, 0xDE, 0x8B,
    };

    public static readonly byte[] DER_NAME_XOR_MASK2 = {
        0x74,
    };

    public const string DER_1_3_MAGIC = "DERv1.3\0";

    public const int DER_XOR1_ASSET_NAME_SEED = 0x0009_1C80;
    public const int DER_XOR1_DER_NAME_SEED = 0x000A_2F3B;
    
    public const int STAT_IDX_MAX = 6;

    public static readonly string[] ITEM_TYPE_NAMES = {
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
}

}
