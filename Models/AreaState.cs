namespace PandemoniumArchipelago.Models
{
    // The memory state that says "the player is currently in this part of a level" (see AreaIds.md).
    // Enemy slots for other parts hold junk until you reach them, so enemy checks only count while their area is active.
    public class AreaState
    {
        // value expected at Addresses.AreaMapId (not unique on its own: every part 2 and 4 is 0x7e)
        public int MapId { get; }

        // optional second byte that separates parts sharing a map id
        public uint? SubAddress { get; }
        public int? SubValue { get; }

        public AreaState(int mapId, uint? subAddress = null, int? subValue = null)
        {
            MapId = mapId;
            SubAddress = subAddress;
            SubValue = subValue;
        }
    }
}
