namespace DemiseTheReversation {

using System.Windows.Forms;
using System.IO;
using System;
using FormUtils;
using Utils;
using static DemiseCore;

// sadly, the designer doesn't work ATM for derived classes
public partial class MainForm : AutoForm {
    public MainForm() {
        InitializeComponent(); // needed for Designer

        addMenus( menuStrip );

        IsMdiContainer = true;
    }

    private void addMenus( MenuStrip ms ) {
        SuspendLayout();

        ToolStripMenuItemEx fileOpenMenuItem = new( fileOpenEventHandler ) {
            Text = @"&Open",
            ShortcutKeys = Keys.Control | Keys.O,
        };
        ToolStripMenuItemEx exitMenuItem = new( ( _, _ ) => Application.Exit() ) {
            Text = @"E&xit",
            ShortcutKeys = Keys.Alt | Keys.F4,
        };
        ToolStripMenuItemEx fileMenu = new() {
            Text = @"&File",
            DropDownItems = {
                fileOpenMenuItem,
                new ToolStripSeparator(),
                exitMenuItem
            }
        };

        ToolStripMenuItemEx closeAllWindowsMenuItem = new(
            ( _, _ ) => MdiChildren.forEach( ( f ) => f.Close() )
        ) {
            Text = @"&Close all",
            ShortcutKeys = Keys.Control | Keys.X,
        };
        ToolStripMenuItemEx windowMenu = new() {
            Text = @"&Windows",
            DropDownItems = {
                closeAllWindowsMenuItem,
            }
        };
        ms.MdiWindowListItem = windowMenu;

        ms.Items.add( fileMenu, /*actionMenu,*/ windowMenu );

        ResumeLayout();
    }

    private void fileOpenEventHandler( object sender, EventArgs eventArgs ) {
        using OpenFileDialog openFileDialog = new() {
            Filter = createDialogFilterString(),
            Multiselect = true,
        };

        if ( openFileDialog.ShowDialog() != DialogResult.OK ) {
            return; // cancel
        }

        foreach ( string filePath in openFileDialog.FileNames ) {
            string ext = Path.GetExtension( filePath ).Substring( 1 ).ToUpper();

            int idx = openFileDialog.FilterIndex;
            FileType ft; // sadly, no way to genericize enum handling here
            if ( Enum.IsDefined( typeof(FileType), idx ) && idx > (int) FileType.DEMISE ) {
                ft = (FileType) idx;
            } else if ( !Enum.TryParse( ext, out ft ) || ft <= FileType.DEMISE ) {
                throw new ArgumentException( $"unknown extension '{ext}'" );
            }

            getHandler( ft, this )
                ?.open( filePath )
                .showPreview();
        }
    }
}

}
