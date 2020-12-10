namespace DemiseTheReversation {

using System.Windows.Forms;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FormUtils;
using Utils;
using static DemiseFilenameConsts;

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

        ToolStripMenuItemEx windowMenu = new() { Text = @"&Windows" };

        ms.MdiWindowListItem = windowMenu;

        ms.Items.add( fileMenu, /*actionMenu,*/ windowMenu );

        ResumeLayout();
    }

    private static string createFilterString() {
        string[] keys = FILE_TYPES.Keys.Select( ( s ) => "*." + s ).ToArray();

        StringBuilder sb = new();
        sb
            .Append( $"Demise assets ({keys.join( ", " )})|{keys.join( ";" )}|" )
            .appendAll( FILE_TYPES.Select( ( kv ) => $"{kv.Value} (*.{kv.Key})|*.{kv.Key}|" ) )
            .Append( "All files (*.*)|*.*" );

        return sb.ToString();
    }

    private void fileOpenEventHandler( object sender, EventArgs eventArgs ) {
        using OpenFileDialog openFileDialog = new() {
            Filter = createFilterString(),
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

            IDemiseFileHandler currentFileHandler = null;
            switch ( ft ) {
                case FileType.DED:
                    DemiseDataHandler.openDED( filePath );
                    break;
                case FileType.DER:
                    currentFileHandler = new DemiseResourceHandler( this );
                    break;
                case FileType.DEA:
                    currentFileHandler = new DemiseAnimationHandler( this );
                    break;
                case FileType.DET:
                    currentFileHandler = new DemiseTextureHandler( this );
                    break;
                default:
                    throw new NotSupportedException();
            }

            currentFileHandler?
                .open( filePath )
                .showPreview();
        }
    }
}

}
