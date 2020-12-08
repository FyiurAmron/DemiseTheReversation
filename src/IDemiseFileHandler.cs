namespace DemiseTheReversation {

using FormUtils;

public interface IDemiseFileHandler {
    public IDemiseFileHandler open( string filePath );
    public void unpack( string outputDir );
    public AutoForm showPreview();
}

}
