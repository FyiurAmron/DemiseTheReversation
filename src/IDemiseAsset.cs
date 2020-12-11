namespace DemiseTheReversation {

using System;
using Utils;

public interface IDemiseAsset : IDisposable {
    public FileUtil fileUtil { get; init; }

    public long load();

    public long load( byte[] sourceBytes );
    // public void save();
}

}
