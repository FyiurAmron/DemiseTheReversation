namespace DemiseTheReversation {

using System;
using System.Diagnostics;
using System.Windows.Forms;

public static class App {
    public static bool debugging { get; } = true;

    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    // ReSharper disable once InconsistentNaming
    private static void Main() {
#if !DEBUG // crafted to suppress heuristics about unused code
        debugging = Debugger.IsAttached;
#endif
        if ( !debugging ) {
            Application.ThreadException += ( _, eventArgs ) => {
                Exception ex = eventArgs.Exception;
                StackTrace st = new( ex, true );
                StackFrame sf = st.GetFrame( 0 );
                if ( sf == null ) {
                    throw new SystemException();
                }

                MessageBox.Show(
                    $"{ex.Message}\n -> {sf.GetMethod()}\n @ {sf.GetFileName()}:{sf.GetFileLineNumber()}",
                    ex.GetType().Name,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            };
            Application.SetUnhandledExceptionMode( UnhandledExceptionMode.CatchException );
        }

        //Application.SetHighDpiMode( HighDpiMode.SystemAware );
        //Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault( false );
        Application.Run( new MainForm() );
    }
}

}
