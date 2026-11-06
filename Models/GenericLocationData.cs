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

        public GenericLocationData(string name, uint locationAddress, string levelId, string check, bool isChest = false, int id = 0)
        {
            Name = name;
            Id = id;
            Address = locationAddress;
            LevelId = levelId;
            Check = check;
            IsInChest = isChest;
        }
    }
}
