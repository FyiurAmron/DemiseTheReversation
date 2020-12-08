namespace DemiseTheReversation.Utils {

using System;
using System.IO;

public class FileUtil {
    public string pathName { get; }
    public string pathNameNoExt { get; }
    public string path { get; }
    public string name { get; }
    public string nameNoExt { get; }
    public int length { get; private set; }

    public FileUtil( string pathName ) {
        this.pathName = pathName;
        name = Path.GetFileName( pathName );
        nameNoExt = Path.GetFileNameWithoutExtension( pathName );
        path = Path.GetDirectoryName( pathName )
            ?? throw new ArgumentException( $"invalid path '{pathName}'" );
        pathNameNoExt = $"{path}/{nameNoExt}";
    }

    public byte[] load() {
        byte[] bytes = File.ReadAllBytes( pathName );
        length = bytes.Length;
        return bytes;
    }

    public void save( byte[] bytes ) {
        File.WriteAllBytes( pathName, bytes );
    }
}

}
