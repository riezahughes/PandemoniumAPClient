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

        public GenericLocationData(string name, uint locationAddress, string levelId, string check, LocationCheckType checkType = LocationCheckType.Byte, bool isChest = false, int id = 0)
        {
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
