// ReSharper disable InconsistentNaming

namespace DemiseTheReversation {

using Utils;

public class DemiseFilenameConsts {
    public enum FileType {
        ALL, // meta-value, don't handle directly
        DEMISE, // ditto; leave those 2 on the top please
        DED,
        DER,
        DEA,
        DET,
    }

    public static readonly SortedMap<FileType, string> FILE_TYPES = new() {
        [FileType.DED] = "DEmise Data",
        [FileType.DER] = "DEmise Resource archive",
        [FileType.DEA] = "DEmise Animation",
        [FileType.DET] = "DEmise Texture",
    };
}

}
