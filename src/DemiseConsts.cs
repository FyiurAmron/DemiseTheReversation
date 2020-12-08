namespace DemiseTheReversation {

using Utils;

public static class DemiseConsts {
    public enum FileType {
        ALL, // meta-value, don't handle directly
        DEMISE, // ditto; leave those 2 on the top please
        DED,
        DER,
        DEA
    }
    
    public static readonly SortedMap<FileType, string> FILE_TYPES = new() {
        [FileType.DED] = "DEmise Data",
        [FileType.DER] = "DEmise Resource archive",
        [FileType.DEA] = "DEmise Animation",
    };

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

    public const int DED_HEADER_END_OFFSET = 24;
    
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
