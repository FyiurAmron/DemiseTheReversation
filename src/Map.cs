namespace DemiseTheReversation {

using System.Collections.Generic;
using System.Linq;

/// <summary>
/// custom version of Dictionary
/// </summary>
public class Map<TKey, TValue> : Dictionary<TKey, TValue> where TKey : notnull {
    // public Map () : base() { }

    public new TValue? this[ TKey key ] {
        get => TryGetValue( key, out TValue? val ) ? val : default;
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
