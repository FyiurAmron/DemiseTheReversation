namespace DemiseTheReversation {

using Utils;
using static DemiseConsts;

public class Item {
    public string name;
    public short idx;
    public short att, def;

    public int price; // negative -> cursed

    //public bool cursed;
    public readonly Vector<short> statReq = new();
    public readonly Vector<short> statMod = new();
    public short swings;
    public float damMod; // ?? for non-weapons
    public short d1;
    public short hands;
    public short type;

    public override string ToString() {
        return $"{name} {{{idx}}} A{att}/D{def} [{damMod}]x{swings} {hands}-handed {d1} {ITEM_TYPE_NAMES[type]} " +
            $"req {statReq} mod {statMod}"; // + " ${cost}";
    }
}

}
