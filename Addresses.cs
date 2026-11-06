namespace PandemoniumArchipelago
{
    class Addresses
    {
        // mark all your addresses here for easy access. Then you can just use Addresses.test, as an example.

        public const uint test = 0x00001234;
        public const uint BiosLoaded = 0x001ffffb;
        public const uint WeaponArray1 = test;
        public const uint ItemIndexStorage = 0x00070EC0;
        public const uint CreditsAddress = 0x00093b55;
        public const uint PlayerLives = 0x00093118;
        public const uint LevelScreenState = 0x001ffba6;
        public const uint CharacterChoice = 0x001bcd15;
        public const uint LevelId = 0x0007202c;
        public const uint FullTiltAndSpeedGreedCheck = 0x0007207c;
        public const uint TreasureCollectedPercent = 0x001fff3c;
        public const uint LevelChoice = 0x00097393; // byte, high byte of the halfword at 0x97392 (0x0115: 01 = level choice, 15 = max choice)
        public const uint LevelMaxChoice = 0x00097392; // byte
        public const uint LevelSelectState = 0x00097392; // ushort: low byte = max choice (0x15), high byte = level choice (0 = none)
        public const uint HasStartedGame = 0x00701659; // byte
        public const uint LevelCoinPointCollectedCount = 0x00111728;
    }
}