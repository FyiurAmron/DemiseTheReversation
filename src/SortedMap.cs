namespace DemiseTheReversation {

using System.Collections.Generic;
using System.Linq;

/// <summary>
/// custom version of Dictionary
/// </summary>
public class SortedMap<TKey, TValue> : SortedDictionary<TKey, TValue> where TKey : notnull {
    // public Map () : base() { }

    public override string ToString() {
        return "{ " + string.Join( "; ", this.Select( x => x.Key + " => " + x.Value ) ) + " }";
    }

    public void add( KeyValuePair<TKey, TValue> keyValuePair ) {
        Add( keyValuePair.Key, keyValuePair.Value );
    }
}

}
