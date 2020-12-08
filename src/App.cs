namespace DemiseTheReversation {

using System.Diagnostics;
using System.Windows.Forms;

public static class App {
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [System.STAThread]
    // ReSharper disable once InconsistentNaming
    private static void Main() {
        if ( !Debugger.IsAttached ) {
            Application.ThreadException += ( _, eventArgs ) =>
                MessageBox.Show(
                    eventArgs.Exception.Message,
                    eventArgs.Exception.GetType().Name,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            Application.SetUnhandledExceptionMode( UnhandledExceptionMode.CatchException );
        }

        //Application.SetHighDpiMode( HighDpiMode.SystemAware );
        //Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault( false );
        Application.Run( new MainForm() );
    }
}

}
