namespace DemiseTheReversation {

using System;
using FormUtils;

public interface IDemiseFileHandler : IDisposable {
    public IDemiseFileHandler open( string filePath );
    public void unpack( string outputDir );
    public AutoForm showPreview();
}

}
