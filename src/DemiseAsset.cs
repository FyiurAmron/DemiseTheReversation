namespace DemiseTheReversation {

using System;
using Utils;

public abstract class DemiseAsset : IDemiseAsset {
    public FileUtil fileUtil { get; init; }

    public long load() {
        return load( fileUtil.load() );
    }

    public virtual void Dispose() {
        GC.SuppressFinalize( this );
    }

    public abstract long load( byte[] sourceBytes );
}

}
