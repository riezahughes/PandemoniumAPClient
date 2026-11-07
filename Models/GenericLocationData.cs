using Archipelago.Core.Models;

namespace PandemoniumArchipelago.Models
{
    public class GenericLocationData
    {
        public string Name { get; set; }
        public int Id { get; set; }

        public uint Address { get; set; }

        public string LevelId { get; set; }

        public string Check { get; set; }

        public bool IsInChest { get; set; }

        // how the address is read when checking it (enemies use Byte, UShort or the lower Nibble)
        public LocationCheckType CheckType { get; set; }

        // the part of the level this location lives in, if known. Enemy checks only count while it is active
        public AreaState? Area { get; set; }

        public GenericLocationData(string name, uint locationAddress, string levelId, string check, LocationCheckType checkType = LocationCheckType.Byte, bool isChest = false, int id = 0, AreaState? area = null)
        {
            Area = area;
            Name = name;
            Id = id;
            Address = locationAddress;
            LevelId = levelId;
            Check = check;
            IsInChest = isChest;
            CheckType = checkType;
        }
    }
}
