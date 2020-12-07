namespace DemiseTheReversation.Utils {

using System.Collections.Generic;
using System.Linq;

/// <summary>
/// custom version of SortedDictionary
/// </summary>
public class SortedMap<TKey, TValue> : SortedDictionary<TKey, TValue> where TKey : notnull {
    // public SortedMap () : base() { }
    
    public new TValue this[ TKey key ] {
        get => TryGetValue( key, out TValue val ) ? val : default;
        set => base[key] = value!;
    }

    public override string ToString() {
        return "{ " + string.Join( "; ", this.Select( x => x.Key + " => " + x.Value ) ) + " }";
    }

    public void add( KeyValuePair<TKey, TValue> keyValuePair ) {
        Add( keyValuePair.Key, keyValuePair.Value );
    }
}

}
