// ReSharper disable InconsistentNaming

namespace DemiseTheReversation {

using System;
using System.Linq;
using System.Text;
using FormUtils;
using Utils;

public class DemiseCore {
    public enum FileType {
        ALL, // meta-value, don't handle directly
        DEMISE, // ditto; leave those 2 on the top please
        DED,
        DER,
        DEA,
        DET,
        DEO,
    }

    public static readonly SortedMap<FileType, string> FILE_TYPE_NAMES = new() {
        [FileType.DED] = "DEmise Data",
        [FileType.DER] = "DEmise Resource archive",
        [FileType.DEA] = "DEmise Animation",
        [FileType.DET] = "DEmise Texture",
        [FileType.DEO] = "DEmise 3D Object",
    };

    public static IDemiseFileHandler getHandler( FileType ft, AutoForm parentForm ) {
        return ft switch {
            FileType.DED => new DemiseDataHandler( parentForm ),
            FileType.DER => new DemiseResourceHandler( parentForm ),
            FileType.DEA => new DemiseAnimationHandler( parentForm ),
            FileType.DET => new DemiseTextureHandler( parentForm ),
            FileType.DEO => new DemiseObjectHandler( parentForm ),
            _ => throw new NotSupportedException()
        };
    }

    public static string createDialogFilterString() {
        string[] keys = FILE_TYPE_NAMES.Keys.Select( ( s ) => "*." + s ).ToArray();

        StringBuilder sb = new();
        sb
            .Append( $"Demise assets ({keys.join( ", " )})|{keys.join( ";" )}|" )
            .appendAll( FILE_TYPE_NAMES.Select( ( kv ) => $"{kv.Value} (*.{kv.Key})|*.{kv.Key}|" ) )
            .Append( "All files (*.*)|*.*" );

        return sb.ToString();
    }
}

}
