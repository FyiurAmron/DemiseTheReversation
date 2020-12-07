namespace DemiseTheReversation.Utils {

using System.Collections.Generic;

/// <summary>
/// custom version of List
/// </summary>
public class Vector<T> : List<T> {
    // public Vector () { }

    public override string ToString() {
        return "[" + string.Join( "; ", this ) + "]";
    }
}

}
