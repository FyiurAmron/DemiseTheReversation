namespace DemiseTheReversation {

using System.Windows.Forms;

public static class App {
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [System.STAThread]
    // ReSharper disable once InconsistentNaming
    private static void Main() {
        //Application.SetHighDpiMode( HighDpiMode.SystemAware );
        //Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault( false );
        Application.Run( new MainForm() );
    }
}

}
