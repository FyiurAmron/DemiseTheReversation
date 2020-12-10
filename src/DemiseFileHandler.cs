namespace DemiseTheReversation {

using System;
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
        previewForm.Disposed += ( _, _ ) => Dispose();
        return previewForm;
    }

    protected void addDefaultMenuAndShow() {
        ToolStripMenuItemEx actionUnpackMenuItem = new( ( _, _ ) => unpack( fileUtil.pathNameNoExt ) ) {
            Text = @$"&Unpack {fileUtil.name} to {fileUtil.pathNameNoExt}",
            ShortcutKeys = Keys.Control | Keys.U,
        };
        ToolStripMenuItemEx actionUnpackAsMenuItem = new( ( _, _ ) => unpackTo() ) {
            Text = @$"U&npack {fileUtil.name} to... (subdir /{fileUtil.nameNoExt})",
            ShortcutKeys = Keys.Control | Keys.N,
        };
        ToolStripMenuItemEx fileMenu = new() {
            Text = @"&Action",
            DropDownItems = {
                actionUnpackMenuItem,
                actionUnpackAsMenuItem,
            }
        };
        previewForm.menuStrip.Items.Add( fileMenu );

        previewForm.Show();
    }

    protected long loadAsset() {
        return demiseAsset.load();
    }

    protected void unpackTo() {
        using FolderBrowserDialog folderBrowserDialog = new();

        if ( folderBrowserDialog.ShowDialog() != DialogResult.OK ) {
            return; // cancel
        }

        unpack( $"{folderBrowserDialog.SelectedPath}/{fileUtil.nameNoExt}" );
    }

    public void Dispose() {
        demiseAsset.Dispose();

        GC.SuppressFinalize( this );
    }

    public abstract IDemiseFileHandler open( string filePath );
    public abstract void unpack( string outputDir );
    public abstract AutoForm showPreview();
}

}
