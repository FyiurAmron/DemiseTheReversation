namespace DemiseTheReversation {

using Utils;

public interface IDemiseAsset {
    public FileUtil fileUtil { get; init; }

    public long load() {
        return load( fileUtil.load() );
    }

    public long load( byte[] sourceArray );
    // public void save();
}

}
