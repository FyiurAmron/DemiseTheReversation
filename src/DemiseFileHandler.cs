namespace DemiseTheReversation {

using System.Windows.Forms;
using FormUtils;
using Utils;

public abstract class DemiseFileHandler<T> : IDemiseFileHandler where T : IDemiseAsset {
    protected readonly AutoForm parent;
    protected FileUtil fileUtil;
    protected AutoForm previewForm;
    protected T demiseAsset;

    protected DemiseFileHandler( AutoForm parent ) {
        this.parent = parent;
    }

    protected AutoForm createPreviewForm() {
        previewForm = new( mdiParent: parent ) {
            Text = fileUtil.name,
        };
        return previewForm;
    }

    protected void addDefaultMenuAndShow() {
        ToolStripMenuItemEx actionOpenMenuItem = new( ( _, _ ) => unpack() ) {
            Text = @$"&Unpack {fileUtil.name}",
            ShortcutKeys = Keys.Control | Keys.U,
        };
        ToolStripMenuItemEx fileMenu = new() {
            Text = @"&Action",
            DropDownItems = { actionOpenMenuItem }
        };
        previewForm.menuStrip.Items.Add( fileMenu );

        previewForm.Show();
    }

    protected long loadAsset() {
        return demiseAsset.load();
    }

    public abstract IDemiseFileHandler open( string filePath );
    public abstract void unpack();
    public abstract AutoForm showPreview();
}

}
