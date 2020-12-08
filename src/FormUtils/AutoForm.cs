namespace DemiseTheReversation.FormUtils {

using System.Windows.Forms;

public class AutoForm : Form {
    public sealed override bool AutoSize {
        get => base.AutoSize;
        set => base.AutoSize = value;
    }

    public Control.ControlCollection autoControls => flowLayoutPanel.Controls;

    public MenuStrip menuStrip { get; }
    public FlowLayoutPanel flowLayoutPanel { get; }

    // ReSharper disable once InconsistentNaming
    public new Form MdiParent {
        get => base.MdiParent;
        private init => base.MdiParent = value;
    }

    public AutoForm( bool hasMenu = true, Form mdiParent = null, bool hasFlowLayout = true ) {
        SuspendLayout();

        // Font = new Font( new FontFamily( "Microsoft Sans Serif" ), 8f );

        MdiParent = mdiParent;

        AutoSize = true;
        // AutoSizeMode = AutoSizeMode.GrowAndShrink;
        AutoSizeMode = AutoSizeMode.GrowOnly;
        // FormBorderStyle = FormBorderStyle.Sizable;

        if ( hasFlowLayout ) {
            flowLayoutPanel = new() {
                // BackColor = Color.Black
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Top,
                //AutoScroll = true
            };

            Controls.Add( flowLayoutPanel );
        }

        if ( hasMenu ) {
            menuStrip = new() {
                RenderMode = ToolStripRenderMode.System,
                Dock = DockStyle.Top,
                Visible = ( mdiParent == null ),
            };
            MainMenuStrip = menuStrip;
            Controls.Add( MainMenuStrip ); // has to be added last to order the docks properly
        }

        ResumeLayout();
    }

    public void add( params Control[] items ) {
        flowLayoutPanel.Controls.AddRange( items );
    }
}

}
