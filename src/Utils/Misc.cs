namespace DemiseTheReversation.Utils {

using System.Text;

public static class Misc {
    public static Encoding defaultEncoding { get; set; } = Encoding.ASCII;

    public static uint roundUp( uint n, uint multiple ) {
        return ( n + ( multiple - 1 ) ) / multiple * multiple;
    }
}

}
