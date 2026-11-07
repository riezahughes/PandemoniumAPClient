using System.Net;
using System.Xml.Linq;
using Archipelago.Core;
using Archipelago.Core.Models;
using PandemoniumArchipelago;
using PandemoniumArchipelago.Models;
using Serilog;

namespace Helpers
{
    public class LocationHelpers
    {
        public static readonly Dictionary<int, string> LevelToKeyName = new Dictionary<int, string>
        {
            { 1, "Skull Fortress Unlock"},
            { 2, "Hollow Stairway Unlock"},
            { 3, "Dungeon Tower Unlock"},
            { 4, "Lost Caves Unlock"},
            { 5, "Fungus Grotto Unlock"},
            { 6, "Acid Pools Unlock"},
            { 7, "Shroom Lord Unlock"},
            { 8, "Burning Desert Unlock"},
            { 9, "Branky Wastes Unlock"},
            { 10, "Spider Forest Unlock"},
            { 11, "Canopy Village Unlock"},
            { 12, "Soldier Barracks Unlock"},
            { 13, "Honcho's Logmill Unlock"},
            { 14, "Goon Honcho Unlock"},
            { 15, "Honcho's Airship Unlock"},
            { 16, "Dragoon Skyfort Unlock"},
            { 17, "Cloud Citadel Unlock"},
            { 18, "Efreet Palace Unlock"},
            { 19, "Frozen Cavern Unlock"},
            { 20, "Storm Temple Unlock"}
        };

        public static readonly Dictionary<string, int> KeyToLevelNumber = new Dictionary<string, int>
        {
            {"Skull Fortress Unlock", 1 },
            {"Hollow Stairway Unlock", 2 },
            {"Dungeon Tower Unlock", 3 },
            {"Lost Caves Unlock", 4 },
            {"Fungus Grotto Unlock", 5 },
            {"Acid Pools Unlock", 6 },
            {"Shroom Lord Unlock", 7 },
            {"Burning Desert Unlock", 8 },
            {"Branky Wastes Unlock", 9 },
            { "Spider Forest Unlock", 10},
            { "Canopy Village Unlock", 11},
            { "Soldier Barracks Unlock", 12},
            { "Honcho's Logmill Unlock", 13},
            { "Goon Honcho Unlock", 14},
            { "Honcho's Airship Unlock", 15},
            { "Dragoon Skyfort Unlock", 16},
            { "Cloud Citadel Unlock", 17},
            { "Efreet Palace Unlock", 18},
            { "Frozen Cavern Unlock", 19},
            { "Storm Temple Unlock", 20}
        };
        // buildLocationsList returns a list of all the valid locations that you need to read from the game
        // while also taking into account what the addresses have to match in order to send out a location.
        // the ID's here must match the ID's in your ap world.

        // It helps to split them out somewhere that you can import and loop over instead of doing them one at a time like this. If you have regions in your ap world
        // then that's a great way to split them up. Just a reminder, the important thing is THAT THE ID'S MATCH THE APWORLD.
        public static List<ILocation> BuildLocationList(Dictionary<string, object> options)
        {

            int base_id = 99270000;
            int region_offset = 1000;

            List<string> table_order = [
                "Menu",
                "Map",
                "Skull Fortress",
                "Hollow Stairway",
                "Dungeon Tower",
                "Lost Caves",
                "Fungus Grotto",
                "Acid Pools",
                "Shroom Lord",
                "Burning Desert",
                "Branky Wastes",
                "Spider Forest",
                "Canopy Village",
                "Soldier Barracks",
                "Honcho's Logmill",
                "Goon Honcho",
                "Honcho's Airship",
                "Dragoon Skyfort",
                "Cloud Citadel",
                "Efreet Palace",
                "Frozen Cavern",
                "Storm Temple",
                "The Wishing Engine",
            ];

            PointChecks.Clear();

            // create a new list of locations thats empty
            List<ILocation> locations = new List<ILocation>();

            Dictionary<string, List<GenericLocationData>> allLevelLocations = new Dictionary<string, List<GenericLocationData>>();

            // Level Locations (same order as table_order; Menu and Map have no locations)
            allLevelLocations.Add("Skull Fortress", GetSkullFortressData());
            allLevelLocations.Add("Hollow Stairway", GetHollowStairwayData());
            allLevelLocations.Add("Dungeon Tower", GetDungeonTowerData());
            allLevelLocations.Add("Lost Caves", GetLostCavesData());
            allLevelLocations.Add("Fungus Grotto", GetFungusGrottoData());
            allLevelLocations.Add("Acid Pools", GetAcidPoolsData());
            allLevelLocations.Add("Shroom Lord", GetShroomLordData());
            allLevelLocations.Add("Burning Desert", GetBurningDesertData());
            allLevelLocations.Add("Branky Wastes", GetBrankyWastesData());
            allLevelLocations.Add("Spider Forest", GetSpiderForestData());
            allLevelLocations.Add("Canopy Village", GetCanopyVillageData());
            allLevelLocations.Add("Soldier Barracks", GetSoldierBarracksData());
            allLevelLocations.Add("Honcho's Logmill", GetHonchosLogmillData());
            allLevelLocations.Add("Goon Honcho", GetGoonHonchoData());
            allLevelLocations.Add("Honcho's Airship", GetHonchosAirshipData());
            allLevelLocations.Add("Dragoon Skyfort", GetDragoonSkyfortData());
            allLevelLocations.Add("Cloud Citadel", GetCloudCitadelData());
            allLevelLocations.Add("Efreet Palace", GetEfreetPalaceData());
            allLevelLocations.Add("Frozen Cavern", GetFrozenCavernData());
            allLevelLocations.Add("Storm Temple", GetStormTempleData());
            allLevelLocations.Add("The Wishing Engine", GetTheWishingEngineData());



            // slot_data always holds every option as 0 or 1, so read the value rather than checking the key exists
            bool enemySanity = GetOption(options, "enemysanity", 0) == 1;
            bool fullTilt = GetOption(options, "full_tilt", 1) == 1;
            bool speedGreed = GetOption(options, "speed_greed", 1) == 1;
            bool multiClear = GetOption(options, "multiclear", 0) == 1;
            bool levelPercentage = GetOption(options, "level_percentage", 0) == 1;
            int percentageStep = GetOption(options, "level_percentage_range", 20);
            bool pointSanity = GetOption(options, "pointsanity", 0) == 1;
            int pointStep = GetOption(options, "pointsanity_range", 50);

            var regional_index = 0;

            foreach (var region_name in table_order.ToList())
            {

                long currentRegionBaseId = base_id + regional_index * region_offset;

                if (allLevelLocations.ContainsKey(region_name))
                {
                    // Retrieve the list of locations for the current region
                    List<GenericLocationData> regionLocations = allLevelLocations[region_name];

                    var location_index = 0;

                    foreach (var loc in regionLocations)
                    {
                        // every entry takes up an ID in the apworld, whether or not it's tracked here, so the index
                        // moves on for every entry (including ones this client doesn't check yet, like the transform gates)
                        int locationId = (int)currentRegionBaseId + location_index;
                        location_index++;

                        List<ILocation> conditionalChoice = new List<ILocation>();

                        if (loc.Name.EndsWith(" Defeated"))
                        {
                            // EnemySanity: enemy memory gets reused between levels, so check we're in the right level
                            // and that the enemy's slot holds its dead value.
                            if (!enemySanity)
                                continue;

                            conditionalChoice.Add(new Location()
                            {
                                Id = -1,
                                Name = "Level ID",
                                Address = Addresses.LevelId,
                                CheckType = LocationCheckType.Byte,
                                CompareType = LocationCheckCompareType.Match,
                                CheckValue = loc.LevelId
                            });

                            // enemy slots for the other parts hold junk until you get there, so check the part too
                            if (loc.Area != null)
                            {
                                conditionalChoice.Add(new Location()
                                {
                                    Id = -1,
                                    Name = "Area Map ID",
                                    Address = Addresses.AreaMapId,
                                    CheckType = LocationCheckType.Byte,
                                    CompareType = LocationCheckCompareType.Match,
                                    CheckValue = loc.Area.MapId.ToString()
                                });

                                if (loc.Area.SubAddress != null && loc.Area.SubValue != null)
                                {
                                    conditionalChoice.Add(new Location()
                                    {
                                        Id = -1,
                                        Name = "Area Sub ID",
                                        Address = loc.Area.SubAddress.Value,
                                        CheckType = LocationCheckType.Byte,
                                        CompareType = LocationCheckCompareType.Match,
                                        CheckValue = loc.Area.SubValue.Value.ToString()
                                    });
                                }
                            }

                            conditionalChoice.Add(new Location()
                            {
                                Id = -1,
                                Name = "Enemy Dead",
                                Address = loc.Address,
                                CheckType = loc.CheckType,
                                NibblePosition = NibblePosition.Lower, // only read for Nibble checks (the fungus bombs)
                                CompareType = LocationCheckCompareType.Match,
                                CheckValue = loc.Check
                            });

                            locations.Add(new CompositeLocation()
                            {
                                Name = loc.Name,
                                Id = locationId,
                                CheckType = LocationCheckType.AND,
                                Conditions = conditionalChoice
                            });
                            continue;
                        }

                        if (loc.Name.EndsWith(" Points Collected"))
                        {
                            // Pointsanity: only the break points pointsanity_range produces exist in the apworld.
                            // Points are counted by the client (see GameStateHelpers.SetUpPointsTracker), not read from memory,
                            // so these aren't monitored locations. They're handed to the tracker, which sends them.
                            if (!pointSanity)
                                continue;

                            int points = int.Parse(loc.Check);
                            if (!PercentageThresholds(pointStep, PointMaximums[region_name]).Contains(points))
                                continue;

                            PointChecks.Add(new PointCheck(byte.Parse(loc.LevelId), points, locationId, loc.Name));
                            continue;
                        }

                        if (loc.Name.Contains(" Percent Cleared: "))
                        {
                            // percentage checks only exist when level_percentage is on, and then only at the
                            // thresholds level_percentage_range produces
                            if (!levelPercentage)
                                continue;

                            HashSet<int> percentages = PercentageThresholds(percentageStep, MaxPercentage(region_name));
                            int percentage = int.Parse(loc.Check);
                            if (!percentages.Contains(percentage))
                                continue;

                            conditionalChoice.Add(new Location()
                            {
                                Id = -1,
                                Name = "Level ID",
                                Address = Addresses.LevelId,
                                CheckType = LocationCheckType.Byte,
                                CompareType = LocationCheckCompareType.Match,
                                CheckValue = loc.LevelId
                            });

                            conditionalChoice.Add(new Location()
                            {
                                Id = -1,
                                Name = "Clear Screen",
                                Address = Addresses.LevelScreenState,
                                CheckType = LocationCheckType.Byte,
                                CompareType = LocationCheckCompareType.Match,
                                CheckValue = "1"
                            });

                            // at least this percentage (the percentage is only written at the end of the level)
                            conditionalChoice.Add(new Location()
                            {
                                Id = -1,
                                Name = "Percentage Reached",
                                Address = Addresses.TreasureCollectedPercent,
                                CheckType = LocationCheckType.Byte,
                                CompareType = LocationCheckCompareType.GreaterThan,
                                CheckValue = (percentage - 1).ToString()
                            });

                            locations.Add(new CompositeLocation()
                            {
                                Name = loc.Name,
                                Id = locationId,
                                CheckType = LocationCheckType.AND,
                                Conditions = conditionalChoice
                            });
                            continue;
                        }

                        if (loc.Name.StartsWith("Full Tilt: ") || loc.Name.StartsWith("Speed Greed: "))
                        {
                            if ((loc.Name.StartsWith("Full Tilt: ") && !fullTilt) || (loc.Name.StartsWith("Speed Greed: ") && !speedGreed))
                                continue;

                            conditionalChoice.Add(new Location()
                            {
                                Id = -1,
                                Name = "Level ID",
                                Address = Addresses.LevelId,
                                CheckType = LocationCheckType.Byte,
                                CompareType = LocationCheckCompareType.Match,
                                CheckValue = loc.LevelId
                            });
                            conditionalChoice.Add(new Location()
                            {
                                Id = -1,
                                Name = "Reaching Check",
                                Address = Addresses.FullTiltAndSpeedGreedCheck,
                                CheckType = LocationCheckType.Byte,
                                CompareType = LocationCheckCompareType.Match,
                                CheckValue = loc.Check
                            });

                            locations.Add(new CompositeLocation()
                            {
                                Name = loc.Name,
                                Id = locationId,
                                CheckType = LocationCheckType.AND,
                                Conditions = conditionalChoice
                            });
                            continue;
                        }

                        bool isLevelClear = loc.Name.StartsWith("Level Cleared: ");
                        bool isCharacterClear = loc.Name.StartsWith("Fargus Cleared: ") || loc.Name.StartsWith("Nikki Cleared: ");
                        if (isLevelClear || isCharacterClear)
                        {
                            // the apworld has either one clear per level, or one per character when multiclear is on
                            if ((isLevelClear && multiClear) || (isCharacterClear && !multiClear))
                                continue;

                            // check level id and if you're on the level cleared screen (and the character, for character clears)
                            conditionalChoice.Add(new Location()
                            {
                                Id = -1,
                                Name = "Level ID",
                                Address = Addresses.LevelId,
                                CheckType = LocationCheckType.Byte,
                                CompareType = LocationCheckCompareType.Match,
                                CheckValue = loc.LevelId
                            });

                            conditionalChoice.Add(new Location()
                            {
                                Id = -1,
                                Name = "Cleared Level Screen",
                                Address = Addresses.LevelScreenState,
                                CheckType = LocationCheckType.Byte,
                                CompareType = LocationCheckCompareType.Match,
                                CheckValue = "1"
                            });

                            if (isCharacterClear)
                            {
                                conditionalChoice.Add(new Location()
                                {
                                    Id = -1,
                                    Name = "Character Choice",
                                    Address = loc.Address,
                                    CheckType = LocationCheckType.Byte,
                                    CompareType = LocationCheckCompareType.Match,
                                    CheckValue = loc.Check
                                });
                            }

                            locations.Add(new CompositeLocation()
                            {
                                Name = loc.Name,
                                Id = locationId,
                                CheckType = LocationCheckType.AND,
                                Conditions = conditionalChoice
                            });
                            continue;
                        }

#if DEBUG
                        //Console.WriteLine($"Not tracked yet: {loc.Name}, id: {locationId}");
#endif
                    }
                }

                regional_index++;
            }
            return locations;
        }

        // TODO: every location below uses Addresses.test as a placeholder. Swap in the real address, level id and check value
        // for each one. The order of every list must match the apworld's Locations.py exactly, because the ID is
        // base_id + (region index * region_offset) + position in the list.

        private static int GetOption(Dictionary<string, object> options, string key, int fallback)
        {
            return int.Parse(options?.GetValueOrDefault(key, fallback)?.ToString() ?? fallback.ToString());
        }

        // matches MAX_PERCENTAGE_OVERRIDES in the apworld: these levels never go above 95% in game
        private static readonly Dictionary<string, int> MaxPercentageOverrides = new Dictionary<string, int>
        {
            { "Canopy Village", 95 },
            { "Soldier Barracks", 95 },
            { "Frozen Cavern", 95 },
        };

        private static int MaxPercentage(string levelName)
        {
            return MaxPercentageOverrides.GetValueOrDefault(levelName, 100);
        }

        // matches percentage_thresholds in the apworld: every multiple of step below the maximum, then the maximum itself
        private static HashSet<int> PercentageThresholds(int step, int maximum = 100)
        {
            step = Math.Clamp(step, 1, 100);
            HashSet<int> thresholds = new HashSet<int>() { maximum };
            for (int percentage = step; percentage < maximum; percentage += step)
            {
                thresholds.Add(percentage);
            }
            return thresholds;
        }

        // matches POINT_MAXIMUMS in the apworld: the most treasure points in each level (boss levels have no point checks)
        private static readonly Dictionary<string, int> PointMaximums = new Dictionary<string, int>
        {
            { "Skull Fortress", 150 }, { "Hollow Stairway", 150 }, { "Dungeon Tower", 150 }, { "Lost Caves", 150 },
            { "Fungus Grotto", 250 }, { "Acid Pools", 250 }, { "Burning Desert", 250 }, { "Branky Wastes", 250 },
            { "Spider Forest", 250 }, { "Canopy Village", 250 }, { "Soldier Barracks", 250 }, { "Honcho's Logmill", 250 },
            { "Honcho's Airship", 350 }, { "Dragoon Skyfort", 350 }, { "Cloud Citadel", 350 }, { "Efreet Palace", 350 },
            { "Frozen Cavern", 350 }, { "Storm Temple", 350 },
        };

        // a pointsanity check: reaching `Points` treasure points while in the level with id `LevelId`
        public record PointCheck(byte LevelId, int Points, int LocationId, string Name);

        // filled by BuildLocationList, read by the points tracker
        public static readonly List<PointCheck> PointChecks = new List<PointCheck>();

        // the apworld appends 1 point up to the level's maximum for every non-boss level, last in the region
        private static List<GenericLocationData> GetPointsData(string levelName)
        {
            string levelId = KeyToLevelNumber[$"{levelName} Unlock"].ToString();

            List<GenericLocationData> pointLocations = new List<GenericLocationData>();
            for (int points = 1; points <= PointMaximums[levelName]; points++)
            {
                pointLocations.Add(new GenericLocationData($"{levelName}: {points} Points Collected", Addresses.PlayerTotalTreasureCollected, levelId, points.ToString(), LocationCheckType.UInt));
            }
            return pointLocations;
        }

        // the apworld appends a Fargus and a Nikki clear for each level clear, after the region's other locations
        private static List<GenericLocationData> GetCharacterClearData(string levelName)
        {
            string levelId = KeyToLevelNumber[$"{levelName} Unlock"].ToString();

            // CharacterSelected reads 1 for Fargus and 0 for Nikki
            return new List<GenericLocationData>()
            {
                new GenericLocationData($"Fargus Cleared: {levelName}", Addresses.CharacterSelected, levelId, "1"),
                new GenericLocationData($"Nikki Cleared: {levelName}", Addresses.CharacterSelected, levelId, "0"),
            };
        }

        // the apworld appends 1% to 100% for every non-boss level, last in the region
        private static List<GenericLocationData> GetPercentageClearData(string levelName)
        {
            string levelId = KeyToLevelNumber[$"{levelName} Unlock"].ToString();

            List<GenericLocationData> percentageLocations = new List<GenericLocationData>();
            for (int percentage = 1; percentage <= 100; percentage++)
            {
                percentageLocations.Add(new GenericLocationData($"{percentage} Percent Cleared: {levelName}", Addresses.TreasureCollectedPercent, levelId, percentage.ToString()));
            }
            return percentageLocations;
        }

        private static List<GenericLocationData> GetSkullFortressData()
        {
            List<GenericLocationData> skullFortressLocations = new List<GenericLocationData>()
            {
                new GenericLocationData("Full Tilt: Skull Fortress", Addresses.FullTiltAndSpeedGreedCheck, "1", "97"),
                new GenericLocationData("Speed Greed: Skull Fortress", Addresses.FullTiltAndSpeedGreedCheck, "1", "96"),
                new GenericLocationData("Level Cleared: Skull Fortress", Addresses.LevelScreenState, "1", "1"),
            };
            skullFortressLocations.AddRange(GetCharacterClearData("Skull Fortress"));
            skullFortressLocations.AddRange(GetPercentageClearData("Skull Fortress"));
            skullFortressLocations.AddRange(GetSkullFortressEnemyData());
            skullFortressLocations.AddRange(GetPointsData("Skull Fortress"));
            return skullFortressLocations;
        }

        private static List<GenericLocationData> GetHollowStairwayData()
        {
            List<GenericLocationData> hollowStairwayLocations = new List<GenericLocationData>()
            {
                new GenericLocationData("Full Tilt: Hollow Stairway", Addresses.FullTiltAndSpeedGreedCheck, "2", "97"),
                new GenericLocationData("Speed Greed: Hollow Stairway", Addresses.FullTiltAndSpeedGreedCheck, "2", "96"),
                new GenericLocationData("Level Cleared: Hollow Stairway", Addresses.LevelScreenState, "2", "1"),
            };
            hollowStairwayLocations.AddRange(GetCharacterClearData("Hollow Stairway"));
            hollowStairwayLocations.AddRange(GetPercentageClearData("Hollow Stairway"));
            hollowStairwayLocations.AddRange(GetHollowStairwayEnemyData());
            hollowStairwayLocations.AddRange(GetPointsData("Hollow Stairway"));
            return hollowStairwayLocations;
        }

        private static List<GenericLocationData> GetDungeonTowerData()
        {
            List<GenericLocationData> dungeonTowerLocations = new List<GenericLocationData>()
            {
                new GenericLocationData("Full Tilt: Dungeon Tower", Addresses.FullTiltAndSpeedGreedCheck, "3", "97"),
                new GenericLocationData("Speed Greed: Dungeon Tower", Addresses.FullTiltAndSpeedGreedCheck, "3", "96"),
                new GenericLocationData("Level Cleared: Dungeon Tower", Addresses.LevelScreenState, "3", "1"),
            };
            dungeonTowerLocations.AddRange(GetCharacterClearData("Dungeon Tower"));
            dungeonTowerLocations.AddRange(GetPercentageClearData("Dungeon Tower"));
            dungeonTowerLocations.AddRange(GetDungeonTowerEnemyData());
            dungeonTowerLocations.AddRange(GetPointsData("Dungeon Tower"));
            return dungeonTowerLocations;
        }

        private static List<GenericLocationData> GetLostCavesData()
        {
            List<GenericLocationData> lostCavesLocations = new List<GenericLocationData>()
            {
                new GenericLocationData("Full Tilt: Lost Caves", Addresses.FullTiltAndSpeedGreedCheck, "4", "97"),
                new GenericLocationData("Speed Greed: Lost Caves", Addresses.FullTiltAndSpeedGreedCheck, "4", "96"),
                new GenericLocationData("Level Cleared: Lost Caves", Addresses.LevelScreenState, "4", "1"),
            };
            lostCavesLocations.AddRange(GetCharacterClearData("Lost Caves"));
            lostCavesLocations.AddRange(GetPercentageClearData("Lost Caves"));
            lostCavesLocations.AddRange(GetLostCavesEnemyData());
            lostCavesLocations.AddRange(GetPointsData("Lost Caves"));
            return lostCavesLocations;
        }

        private static List<GenericLocationData> GetFungusGrottoData()
        {
            List<GenericLocationData> fungusGrottoLocations = new List<GenericLocationData>()
            {
                new GenericLocationData("Full Tilt: Fungus Grotto", Addresses.FullTiltAndSpeedGreedCheck, "5", "97"),
                new GenericLocationData("Speed Greed: Fungus Grotto", Addresses.FullTiltAndSpeedGreedCheck, "5", "96"),
                new GenericLocationData("Level Cleared: Fungus Grotto", Addresses.LevelScreenState, "5", "1"),
                new GenericLocationData("Frog Gate: Fungus Grotto", Addresses.test, "", ""),
            };
            fungusGrottoLocations.AddRange(GetCharacterClearData("Fungus Grotto"));
            fungusGrottoLocations.AddRange(GetPercentageClearData("Fungus Grotto"));
            fungusGrottoLocations.AddRange(GetFungusGrottoEnemyData());
            fungusGrottoLocations.AddRange(GetPointsData("Fungus Grotto"));
            return fungusGrottoLocations;
        }

        private static List<GenericLocationData> GetAcidPoolsData()
        {
            List<GenericLocationData> acidPoolsLocations = new List<GenericLocationData>()
            {
                new GenericLocationData("Full Tilt: Acid Pools", Addresses.FullTiltAndSpeedGreedCheck, "6", "97"),
                new GenericLocationData("Speed Greed: Acid Pools", Addresses.FullTiltAndSpeedGreedCheck, "6", "96"),
                new GenericLocationData("Level Cleared: Acid Pools", Addresses.LevelScreenState, "6", "1"),
                new GenericLocationData("Frog Gate: Acid Pools", Addresses.test, "", ""),
            };
            acidPoolsLocations.AddRange(GetCharacterClearData("Acid Pools"));
            acidPoolsLocations.AddRange(GetPercentageClearData("Acid Pools"));
            acidPoolsLocations.AddRange(GetAcidPoolsEnemyData());
            acidPoolsLocations.AddRange(GetPointsData("Acid Pools"));
            return acidPoolsLocations;
        }

        private static List<GenericLocationData> GetShroomLordData()
        {
            List<GenericLocationData> shroomLordLocations = new List<GenericLocationData>()
            {
                new GenericLocationData("Level Cleared: Shroom Lord", Addresses.LevelScreenState, "7", "1"),
            };
            shroomLordLocations.AddRange(GetCharacterClearData("Shroom Lord"));
            return shroomLordLocations;
        }

        private static List<GenericLocationData> GetBurningDesertData()
        {
            List<GenericLocationData> burningDesertLocations = new List<GenericLocationData>()
            {
                new GenericLocationData("Full Tilt: Burning Desert", Addresses.FullTiltAndSpeedGreedCheck, "8", "97"),
                new GenericLocationData("Speed Greed: Burning Desert", Addresses.FullTiltAndSpeedGreedCheck, "8", "96"),
                new GenericLocationData("Level Cleared: Burning Desert", Addresses.LevelScreenState, "8", "1"),
            };
            burningDesertLocations.AddRange(GetCharacterClearData("Burning Desert"));
            burningDesertLocations.AddRange(GetPercentageClearData("Burning Desert"));
            burningDesertLocations.AddRange(GetBurningDesertEnemyData());
            burningDesertLocations.AddRange(GetPointsData("Burning Desert"));
            return burningDesertLocations;
        }

        private static List<GenericLocationData> GetBrankyWastesData()
        {
            List<GenericLocationData> brankyWastesLocations = new List<GenericLocationData>()
            {
                new GenericLocationData("Full Tilt: Branky Wastes", Addresses.FullTiltAndSpeedGreedCheck, "9", "97"),
                new GenericLocationData("Speed Greed: Branky Wastes", Addresses.FullTiltAndSpeedGreedCheck, "9", "96"),
                new GenericLocationData("Level Cleared: Branky Wastes", Addresses.LevelScreenState, "9", "1"),
                new GenericLocationData("Rhino Gate: Branky Wastes", Addresses.test, "", ""),
            };
            brankyWastesLocations.AddRange(GetCharacterClearData("Branky Wastes"));
            brankyWastesLocations.AddRange(GetPercentageClearData("Branky Wastes"));
            brankyWastesLocations.AddRange(GetBrankyWastesEnemyData());
            brankyWastesLocations.AddRange(GetPointsData("Branky Wastes"));
            return brankyWastesLocations;
        }

        private static List<GenericLocationData> GetSpiderForestData()
        {
            List<GenericLocationData> spiderForestLocations = new List<GenericLocationData>()
            {
                new GenericLocationData("Full Tilt: Spider Forest", Addresses.FullTiltAndSpeedGreedCheck, "10", "97"),
                new GenericLocationData("Speed Greed: Spider Forest", Addresses.FullTiltAndSpeedGreedCheck, "10", "96"),
                new GenericLocationData("Level Cleared: Spider Forest", Addresses.LevelScreenState, "10", "1"),
            };
            spiderForestLocations.AddRange(GetCharacterClearData("Spider Forest"));
            spiderForestLocations.AddRange(GetPercentageClearData("Spider Forest"));
            spiderForestLocations.AddRange(GetSpiderForestEnemyData());
            spiderForestLocations.AddRange(GetPointsData("Spider Forest"));
            return spiderForestLocations;
        }

        private static List<GenericLocationData> GetCanopyVillageData()
        {
            List<GenericLocationData> canopyVillageLocations = new List<GenericLocationData>()
            {
                new GenericLocationData("Full Tilt: Canopy Village", Addresses.FullTiltAndSpeedGreedCheck, "11", "97"),
                new GenericLocationData("Speed Greed: Canopy Village", Addresses.FullTiltAndSpeedGreedCheck, "11", "96"),
                new GenericLocationData("Level Cleared: Canopy Village", Addresses.LevelScreenState, "11", "1"),
                new GenericLocationData("Turtle Gate: Canopy Village", Addresses.test, "", ""),
            };
            canopyVillageLocations.AddRange(GetCharacterClearData("Canopy Village"));
            canopyVillageLocations.AddRange(GetPercentageClearData("Canopy Village"));
            canopyVillageLocations.AddRange(GetCanopyVillageEnemyData());
            canopyVillageLocations.AddRange(GetPointsData("Canopy Village"));
            return canopyVillageLocations;
        }

        private static List<GenericLocationData> GetSoldierBarracksData()
        {
            List<GenericLocationData> soldierBarracksLocations = new List<GenericLocationData>()
            {
                new GenericLocationData("Full Tilt: Soldier Barracks", Addresses.FullTiltAndSpeedGreedCheck, "12", "97"),
                new GenericLocationData("Speed Greed: Soldier Barracks", Addresses.FullTiltAndSpeedGreedCheck, "12", "96"),
                new GenericLocationData("Level Cleared: Soldier Barracks", Addresses.LevelScreenState, "12", "1"),
            };
            soldierBarracksLocations.AddRange(GetCharacterClearData("Soldier Barracks"));
            soldierBarracksLocations.AddRange(GetPercentageClearData("Soldier Barracks"));
            soldierBarracksLocations.AddRange(GetPointsData("Soldier Barracks"));
            return soldierBarracksLocations;
        }

        private static List<GenericLocationData> GetHonchosLogmillData()
        {
            List<GenericLocationData> honchosLogmillLocations = new List<GenericLocationData>()
            {
                new GenericLocationData("Full Tilt: Honcho's Logmill", Addresses.FullTiltAndSpeedGreedCheck, "13", "97"),
                new GenericLocationData("Speed Greed: Honcho's Logmill", Addresses.FullTiltAndSpeedGreedCheck, "13", "96"),
                new GenericLocationData("Level Cleared: Honcho's Logmill", Addresses.LevelScreenState, "13", "1"),
            };
            honchosLogmillLocations.AddRange(GetCharacterClearData("Honcho's Logmill"));
            honchosLogmillLocations.AddRange(GetPercentageClearData("Honcho's Logmill"));
            honchosLogmillLocations.AddRange(GetHonchosLogmillEnemyData());
            honchosLogmillLocations.AddRange(GetPointsData("Honcho's Logmill"));
            return honchosLogmillLocations;
        }

        private static List<GenericLocationData> GetGoonHonchoData()
        {
            List<GenericLocationData> goonHonchoLocations = new List<GenericLocationData>()
            {
                new GenericLocationData("Level Cleared: Goon Honcho", Addresses.LevelScreenState, "14", "1"),
            };
            goonHonchoLocations.AddRange(GetCharacterClearData("Goon Honcho"));
            return goonHonchoLocations;
        }

        private static List<GenericLocationData> GetHonchosAirshipData()
        {
            List<GenericLocationData> honchosAirshipLocations = new List<GenericLocationData>()
            {
                new GenericLocationData("Full Tilt: Honcho's Airship", Addresses.FullTiltAndSpeedGreedCheck, "15", "97"),
                new GenericLocationData("Speed Greed: Honcho's Airship", Addresses.FullTiltAndSpeedGreedCheck, "15", "96"),
                new GenericLocationData("Level Cleared: Honcho's Airship", Addresses.LevelScreenState, "15", "1"),
                new GenericLocationData("Dragon Gate: Honcho's Airship", Addresses.test, "", ""),
            };
            honchosAirshipLocations.AddRange(GetCharacterClearData("Honcho's Airship"));
            honchosAirshipLocations.AddRange(GetPercentageClearData("Honcho's Airship"));
            honchosAirshipLocations.AddRange(GetHonchosAirshipEnemyData());
            honchosAirshipLocations.AddRange(GetPointsData("Honcho's Airship"));
            return honchosAirshipLocations;
        }

        private static List<GenericLocationData> GetDragoonSkyfortData()
        {
            List<GenericLocationData> dragoonSkyfortLocations = new List<GenericLocationData>()
            {
                new GenericLocationData("Full Tilt: Dragoon Skyfort", Addresses.FullTiltAndSpeedGreedCheck, "16", "97"),
                new GenericLocationData("Speed Greed: Dragoon Skyfort", Addresses.FullTiltAndSpeedGreedCheck, "16", "96"),
                new GenericLocationData("Level Cleared: Dragoon Skyfort", Addresses.LevelScreenState, "16", "1"),
            };
            dragoonSkyfortLocations.AddRange(GetCharacterClearData("Dragoon Skyfort"));
            dragoonSkyfortLocations.AddRange(GetPercentageClearData("Dragoon Skyfort"));
            dragoonSkyfortLocations.AddRange(GetDragoonSkyfortEnemyData());
            dragoonSkyfortLocations.AddRange(GetPointsData("Dragoon Skyfort"));
            return dragoonSkyfortLocations;
        }

        private static List<GenericLocationData> GetCloudCitadelData()
        {
            List<GenericLocationData> cloudCitadelLocations = new List<GenericLocationData>()
            {
                new GenericLocationData("Full Tilt: Cloud Citadel", Addresses.FullTiltAndSpeedGreedCheck, "17", "97"),
                new GenericLocationData("Speed Greed: Cloud Citadel", Addresses.FullTiltAndSpeedGreedCheck, "17", "96"),
                new GenericLocationData("Level Cleared: Cloud Citadel", Addresses.LevelScreenState, "17", "1"),
            };
            cloudCitadelLocations.AddRange(GetCharacterClearData("Cloud Citadel"));
            cloudCitadelLocations.AddRange(GetPercentageClearData("Cloud Citadel"));
            cloudCitadelLocations.AddRange(GetCloudCitadelEnemyData());
            cloudCitadelLocations.AddRange(GetPointsData("Cloud Citadel"));
            return cloudCitadelLocations;
        }

        private static List<GenericLocationData> GetEfreetPalaceData()
        {
            List<GenericLocationData> efreetPalaceLocations = new List<GenericLocationData>()
            {
                new GenericLocationData("Full Tilt: Efreet Palace", Addresses.FullTiltAndSpeedGreedCheck, "18", "97"),
                new GenericLocationData("Speed Greed: Efreet Palace", Addresses.FullTiltAndSpeedGreedCheck, "18", "96"),
                new GenericLocationData("Level Cleared: Efreet Palace", Addresses.LevelScreenState, "18", "1"),
            };
            efreetPalaceLocations.AddRange(GetCharacterClearData("Efreet Palace"));
            efreetPalaceLocations.AddRange(GetPercentageClearData("Efreet Palace"));
            efreetPalaceLocations.AddRange(GetEfreetPalaceEnemyData());
            efreetPalaceLocations.AddRange(GetPointsData("Efreet Palace"));
            return efreetPalaceLocations;
        }

        private static List<GenericLocationData> GetFrozenCavernData()
        {
            List<GenericLocationData> frozenCavernLocations = new List<GenericLocationData>()
            {
                new GenericLocationData("Full Tilt: Frozen Cavern", Addresses.FullTiltAndSpeedGreedCheck, "19", "97"),
                new GenericLocationData("Speed Greed: Frozen Cavern", Addresses.FullTiltAndSpeedGreedCheck, "19", "96"),
                new GenericLocationData("Level Cleared: Frozen Cavern", Addresses.LevelScreenState, "19", "1"),
            };
            frozenCavernLocations.AddRange(GetCharacterClearData("Frozen Cavern"));
            frozenCavernLocations.AddRange(GetPercentageClearData("Frozen Cavern"));
            frozenCavernLocations.AddRange(GetFrozenCavernEnemyData());
            frozenCavernLocations.AddRange(GetPointsData("Frozen Cavern"));
            return frozenCavernLocations;
        }

        private static List<GenericLocationData> GetStormTempleData()
        {
            List<GenericLocationData> stormTempleLocations = new List<GenericLocationData>()
            {
                new GenericLocationData("Full Tilt: Storm Temple", Addresses.FullTiltAndSpeedGreedCheck, "20", "97"),
                new GenericLocationData("Speed Greed: Storm Temple", Addresses.FullTiltAndSpeedGreedCheck, "20", "96"),
                new GenericLocationData("Level Cleared: Storm Temple", Addresses.LevelScreenState, "20", "1"),
            };
            stormTempleLocations.AddRange(GetCharacterClearData("Storm Temple"));
            stormTempleLocations.AddRange(GetPercentageClearData("Storm Temple"));
            stormTempleLocations.AddRange(GetStormTempleEnemyData());
            stormTempleLocations.AddRange(GetPointsData("Storm Temple"));
            return stormTempleLocations;
        }

        // EnemySanity: the apworld appends these after the percentage checks, in this exact order
        private static List<GenericLocationData> GetSkullFortressEnemyData()
        {
            string levelId = "1";
            return new List<GenericLocationData>()
            {
                new GenericLocationData("Skull Fortress: Part 1 - 1st Enemy Defeated", Addresses.SkullFortressPart11stEnemy, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSkullFortressPart1),
                new GenericLocationData("Skull Fortress: Part 1 - 3rd Flower Defeated", Addresses.SkullFortressPart13rdFlower, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSkullFortressPart1),
                new GenericLocationData("Skull Fortress: Part 1 - 5th Flower Defeated", Addresses.SkullFortressPart15thFlower, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSkullFortressPart1),
                new GenericLocationData("Skull Fortress: Part 1 - 4th Flower Defeated", Addresses.SkullFortressPart14thFlower, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSkullFortressPart1),
                new GenericLocationData("Skull Fortress: Part 1 - Goon before 4th Flower Defeated", Addresses.SkullFortressPart1GoonBefore4thFlower, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSkullFortressPart1),
                new GenericLocationData("Skull Fortress: Part 1 - 2nd Brother on the Bridge Defeated", Addresses.SkullFortressPart12ndBrotherOnTheBridge, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSkullFortressPart1),
                new GenericLocationData("Skull Fortress: Part 1 - 1st Brother on the Bridge Defeated", Addresses.SkullFortressPart11stBrotherOnTheBridge, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSkullFortressPart1),
                new GenericLocationData("Skull Fortress: Part 1 - 4th Enemy (Right after 2nd Flower) Defeated", Addresses.SkullFortressPart14thEnemyRightAfter2ndFlower, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSkullFortressPart1),
                new GenericLocationData("Skull Fortress: Part 1 - 1st Flower Defeated", Addresses.SkullFortressPart11stFlower, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSkullFortressPart1),
                new GenericLocationData("Skull Fortress: Part 1 - 2nd Flower Defeated", Addresses.SkullFortressPart12ndFlower, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSkullFortressPart1),
                new GenericLocationData("Skull Fortress: Part 1 - 3rd Enemy Defeated", Addresses.SkullFortressPart13rdEnemy, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSkullFortressPart1),
                new GenericLocationData("Skull Fortress: Part 1 - 2nd Enemy Defeated", Addresses.SkullFortressPart12ndEnemy, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSkullFortressPart1),
                new GenericLocationData("Skull Fortress: Part 2 - 1st Goon Defeated", Addresses.SkullFortressPart21stGoon, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSkullFortressPart2),
                new GenericLocationData("Skull Fortress: Part 2 - Last Goon Defeated", Addresses.SkullFortressPart2LastGoon, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSkullFortressPart2),
                new GenericLocationData("Skull Fortress: Part 2 - One More Goon Defeated", Addresses.SkullFortressPart2OneMoreGoon, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSkullFortressPart2),
                new GenericLocationData("Skull Fortress: Part 2 - 4th Goon Defeated", Addresses.SkullFortressPart24thGoon, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSkullFortressPart2),
                new GenericLocationData("Skull Fortress: Part 2 - 3rd Goon Defeated", Addresses.SkullFortressPart23rdGoon, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSkullFortressPart2),
                new GenericLocationData("Skull Fortress: Part 2 - 2nd Goon (Downstairs) Defeated", Addresses.SkullFortressPart22ndGoonDownstairs, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSkullFortressPart2),
                new GenericLocationData("Skull Fortress: Part 2 - Flower Downstairs Defeated", Addresses.SkullFortressPart2FlowerDownstairs, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSkullFortressPart2),
                new GenericLocationData("Skull Fortress: Part 2 - 1st Bat Defeated", Addresses.SkullFortressPart21stBat, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSkullFortressPart2),
                new GenericLocationData("Skull Fortress: Part 2 - Very Last Bat Defeated", Addresses.SkullFortressPart2VeryLastBat, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSkullFortressPart2),
                new GenericLocationData("Skull Fortress: Part 2 - Last Bat Defeated", Addresses.SkullFortressPart2LastBat, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSkullFortressPart2),
                new GenericLocationData("Skull Fortress: Part 2 - 5th Bat Defeated", Addresses.SkullFortressPart25thBat, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSkullFortressPart2),
                new GenericLocationData("Skull Fortress: Part 2 - 4th Bat Defeated", Addresses.SkullFortressPart24thBat, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSkullFortressPart2),
                new GenericLocationData("Skull Fortress: Part 2 - 6th Bat Defeated", Addresses.SkullFortressPart26thBat, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSkullFortressPart2),
                new GenericLocationData("Skull Fortress: Part 2 - 2nd Bat Defeated", Addresses.SkullFortressPart22ndBat, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSkullFortressPart2),
                new GenericLocationData("Skull Fortress: Part 2 - 3rd Bat Defeated", Addresses.SkullFortressPart23rdBat, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSkullFortressPart2),
                new GenericLocationData("Skull Fortress: Part 3 - 1st of the Last Two Flowers Defeated", Addresses.SkullFortressPart31stOfTheLastTwoFlowers, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSkullFortressPart3),
                new GenericLocationData("Skull Fortress: Part 3 - 2nd of the Last Two Flowers Defeated", Addresses.SkullFortressPart32ndOfTheLastTwoFlowers, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSkullFortressPart3),
                new GenericLocationData("Skull Fortress: Part 3 - 2nd Flower Defeated", Addresses.SkullFortressPart32ndFlower, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSkullFortressPart3),
                new GenericLocationData("Skull Fortress: Part 3 - 2nd Goon Defeated", Addresses.SkullFortressPart32ndGoonAndFungusGrottoPart31stBat, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSkullFortressPart3),
                new GenericLocationData("Skull Fortress: Part 3 - 1st Goon Defeated", Addresses.SkullFortressPart31stGoon, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSkullFortressPart3),
                new GenericLocationData("Skull Fortress: Part 3 - 1st Goon at the End (Upper Way) Defeated", Addresses.SkullFortressPart31stGoonAtTheEndUpperWay, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSkullFortressPart3),
                new GenericLocationData("Skull Fortress: Part 3 - 3rd Goon Defeated", Addresses.SkullFortressPart33rdGoon, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSkullFortressPart3),
                new GenericLocationData("Skull Fortress: Part 3 - 2nd Bat Defeated", Addresses.SkullFortressPart32ndBatAndFungusGrottoPart15thSmallMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSkullFortressPart3),
                new GenericLocationData("Skull Fortress: Part 3 - 1st Bat Defeated", Addresses.SkullFortressPart31stBat, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSkullFortressPart3),
                new GenericLocationData("Skull Fortress: Part 3 - 1st Flower Defeated", Addresses.SkullFortressPart31stFlower, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSkullFortressPart3),
                new GenericLocationData("Skull Fortress: Part 3 - 2nd Goon at the End (Upper Way) Defeated", Addresses.SkullFortressPart32ndGoonAtTheEndUpperWay, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSkullFortressPart3),
                new GenericLocationData("Skull Fortress: Part 3 - 4th Bat Defeated", Addresses.SkullFortressPart34thBat, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSkullFortressPart3),
                new GenericLocationData("Skull Fortress: Part 3 - 5th Bat Defeated", Addresses.SkullFortressPart35thBat, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSkullFortressPart3),
                new GenericLocationData("Skull Fortress: Part 3 - 3rd Bat Defeated", Addresses.SkullFortressPart33rdBat, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSkullFortressPart3),
                new GenericLocationData("Skull Fortress: Part 3 - 4th Goon Defeated", Addresses.SkullFortressPart34thGoon, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSkullFortressPart3),
                new GenericLocationData("Skull Fortress: Part 3 - Goon at the End (Bottom Way) Defeated", Addresses.SkullFortressPart3GoonAtTheEndBottomWay, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSkullFortressPart3),
                new GenericLocationData("Skull Fortress: Part 3 - Very Last Goon Defeated", Addresses.SkullFortressPart3VeryLastGoon, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSkullFortressPart3),
            };
        }

        private static List<GenericLocationData> GetHollowStairwayEnemyData()
        {
            string levelId = "2";
            return new List<GenericLocationData>()
            {
                new GenericLocationData("Hollow Stairway: Part 1 - Bat 1 Defeated", Addresses.HollowStairwayPart1Bat1, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaHollowStairwayPart1),
                new GenericLocationData("Hollow Stairway: Part 1 - Bat 2 Defeated", Addresses.HollowStairwayPart1Bat2, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaHollowStairwayPart1),
                new GenericLocationData("Hollow Stairway: Part 1 - Bat 4 Defeated", Addresses.HollowStairwayPart1Bat4, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaHollowStairwayPart1),
                new GenericLocationData("Hollow Stairway: Part 1 - Bat with a Heart Defeated", Addresses.HollowStairwayPart1BatWithAHeart, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaHollowStairwayPart1),
                new GenericLocationData("Hollow Stairway: Part 1 - Bat 5 Defeated", Addresses.HollowStairwayPart1Bat5, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaHollowStairwayPart1),
                new GenericLocationData("Hollow Stairway: Part 1 - Bat 6 (Staircase of Bats to Ankh) Defeated", Addresses.HollowStairwayPart1Bat6StaircaseOfBatsToAnkh, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaHollowStairwayPart1),
                new GenericLocationData("Hollow Stairway: Part 1 - Bat 7 (Staircase of Bats to Ankh) Defeated", Addresses.HollowStairwayPart1Bat7StaircaseOfBatsToAnkh, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaHollowStairwayPart1),
                new GenericLocationData("Hollow Stairway: Part 1 - Bat 8 (Staircase of Bats to Ankh) Defeated", Addresses.HollowStairwayPart1Bat8StaircaseOfBatsToAnkh, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaHollowStairwayPart1),
                new GenericLocationData("Hollow Stairway: Part 1 - Bat 10 (After the Staircase of Bats) Defeated", Addresses.HollowStairwayPart1Bat10AfterTheStaircaseOfBats, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaHollowStairwayPart1),
                new GenericLocationData("Hollow Stairway: Part 1 - Bat 9 (Staircase of Bats to Ankh) Defeated", Addresses.HollowStairwayPart1Bat9StaircaseOfBatsToAnkh, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaHollowStairwayPart1),
                new GenericLocationData("Hollow Stairway: Part 1 - 2nd Dude after Staircase of Bats Defeated", Addresses.HollowStairwayPart12ndDudeAfterStaircaseOfBats, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaHollowStairwayPart1),
                new GenericLocationData("Hollow Stairway: Part 1 - 1st Dude after Staircase of Bats Defeated", Addresses.HollowStairwayPart11stDudeAfterStaircaseOfBats, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaHollowStairwayPart1),
                new GenericLocationData("Hollow Stairway: Part 1 - Small Dude 6 (1st after Staircase of Bats) Defeated", Addresses.HollowStairwayPart1SmallDude61stAfterStaircaseOfBats, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaHollowStairwayPart1),
                new GenericLocationData("Hollow Stairway: Part 1 - Enemy 1 Defeated", Addresses.HollowStairwayPart1Enemy1, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaHollowStairwayPart1),
                new GenericLocationData("Hollow Stairway: Part 1 - Small Dude 4 Defeated", Addresses.HollowStairwayPart1SmallDude4, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaHollowStairwayPart1),
                new GenericLocationData("Hollow Stairway: Part 1 - Small Dude 7 (2nd after Staircase of Bats) Defeated", Addresses.HollowStairwayPart1SmallDude72ndAfterStaircaseOfBats, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaHollowStairwayPart1),
                new GenericLocationData("Hollow Stairway: Part 1 - Small Dude 5 Defeated", Addresses.HollowStairwayPart1SmallDude5, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaHollowStairwayPart1),
                new GenericLocationData("Hollow Stairway: Part 1 - Small Dude 3 Defeated", Addresses.HollowStairwayPart1SmallDude3, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaHollowStairwayPart1),
                new GenericLocationData("Hollow Stairway: Part 1 - Enemy 2 Defeated", Addresses.HollowStairwayPart1Enemy2, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaHollowStairwayPart1),
                new GenericLocationData("Hollow Stairway: Part 3 - Flower 3 Defeated", Addresses.HollowStairwayPart3Flower3, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaHollowStairwayPart3),
                new GenericLocationData("Hollow Stairway: Part 3 - Flower 8 Defeated", Addresses.HollowStairwayPart3Flower8, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaHollowStairwayPart3),
                new GenericLocationData("Hollow Stairway: Part 3 - Flower 6 Defeated", Addresses.HollowStairwayPart3Flower6, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaHollowStairwayPart3),
                new GenericLocationData("Hollow Stairway: Part 3 - Flower 4 Defeated", Addresses.HollowStairwayPart3Flower4, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaHollowStairwayPart3),
                new GenericLocationData("Hollow Stairway: Part 3 - Flower 2 Defeated", Addresses.HollowStairwayPart3Flower2, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaHollowStairwayPart3),
                new GenericLocationData("Hollow Stairway: Part 3 - Flower 1 Defeated", Addresses.HollowStairwayPart3Flower1, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaHollowStairwayPart3),
                new GenericLocationData("Hollow Stairway: Part 3 - Flower 7 Defeated", Addresses.HollowStairwayPart3Flower7, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaHollowStairwayPart3),
                new GenericLocationData("Hollow Stairway: Part 3 - Flower 5 Defeated", Addresses.HollowStairwayPart3Flower5, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaHollowStairwayPart3),
            };
        }

        private static List<GenericLocationData> GetDungeonTowerEnemyData()
        {
            string levelId = "3";
            return new List<GenericLocationData>()
            {
                new GenericLocationData("Dungeon Tower: Part 1 - 2nd Spider Defeated", Addresses.DungeonTowerPart12ndSpider, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaDungeonTowerPart1),
                new GenericLocationData("Dungeon Tower: Part 1 - 1st Spider Defeated", Addresses.DungeonTowerPart11stSpider, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaDungeonTowerPart1),
                new GenericLocationData("Dungeon Tower: Part 1 - 3rd Spider Defeated", Addresses.DungeonTowerPart13rdSpider, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaDungeonTowerPart1),
                new GenericLocationData("Dungeon Tower: Part 1 - 4th Spider Defeated", Addresses.DungeonTowerPart14thSpider, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaDungeonTowerPart1),
                new GenericLocationData("Dungeon Tower: Part 2 - 3rd Dude Defeated", Addresses.DungeonTowerPart23rdDude, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaDungeonTowerPart2),
                new GenericLocationData("Dungeon Tower: Part 2 - 1st Dude Defeated", Addresses.DungeonTowerPart21stDude, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaDungeonTowerPart2),
                new GenericLocationData("Dungeon Tower: Part 2 - 2nd Dude Defeated", Addresses.DungeonTowerPart22ndDude, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaDungeonTowerPart2),
                new GenericLocationData("Dungeon Tower: Part 2 - 4th Goon Defeated", Addresses.DungeonTowerPart24thGoon, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaDungeonTowerPart2),
                new GenericLocationData("Dungeon Tower: Part 2 - 1st Goon Defeated", Addresses.DungeonTowerPart21stGoon, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaDungeonTowerPart2),
                new GenericLocationData("Dungeon Tower: Part 2 - 2nd Goon Defeated", Addresses.DungeonTowerPart22ndGoon, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaDungeonTowerPart2),
                new GenericLocationData("Dungeon Tower: Part 2 - 3rd Goon Defeated", Addresses.DungeonTowerPart23rdGoon, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaDungeonTowerPart2),
                new GenericLocationData("Dungeon Tower: Part 3 - Dude Defeated", Addresses.DungeonTowerPart3Dude, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaDungeonTowerPart3),
                new GenericLocationData("Dungeon Tower: Part 3 - 1st Dude Defeated", Addresses.DungeonTowerPart31stDude, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaDungeonTowerPart3),
                new GenericLocationData("Dungeon Tower: Part 3 - Bat Defeated", Addresses.DungeonTowerPart3Bat, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaDungeonTowerPart3),
                new GenericLocationData("Dungeon Tower: Part 4 - 1st Goon in the Last Room Defeated", Addresses.DungeonTowerPart41stGoonInTheLastRoom, levelId, "128", LocationCheckType.Byte),
                new GenericLocationData("Dungeon Tower: Part 4 - 2nd Goon in the Last Room Defeated", Addresses.DungeonTowerPart42ndGoonInTheLastRoom, levelId, "128", LocationCheckType.Byte),
            };
        }

        private static List<GenericLocationData> GetLostCavesEnemyData()
        {
            string levelId = "4";
            return new List<GenericLocationData>()
            {
                new GenericLocationData("Lost Caves: Part 1 - Sharp Thing 13 Defeated", Addresses.LostCavesPart1SharpThing13, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart1),
                new GenericLocationData("Lost Caves: Part 1 - 5th Bat Defeated", Addresses.LostCavesPart15thBat, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart1),
                new GenericLocationData("Lost Caves: Part 1 - 4th Bat Defeated", Addresses.LostCavesPart14thBat, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart1),
                new GenericLocationData("Lost Caves: Part 1 - 5th Flower Defeated", Addresses.LostCavesPart15thFlower, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart1),
                new GenericLocationData("Lost Caves: Part 1 - 6th Flower Defeated", Addresses.LostCavesPart16thFlower, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart1),
                new GenericLocationData("Lost Caves: Part 1 - Sharp Thing 1 Defeated", Addresses.LostCavesPart1SharpThing1, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart1),
                new GenericLocationData("Lost Caves: Part 1 - 3rd Flower Defeated", Addresses.LostCavesPart13rdFlower, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart1),
                new GenericLocationData("Lost Caves: Part 1 - 3rd Spider Defeated", Addresses.LostCavesPart13rdSpider, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart1),
                new GenericLocationData("Lost Caves: Part 1 - 2nd Spider Defeated", Addresses.LostCavesPart12ndSpider, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart1),
                new GenericLocationData("Lost Caves: Part 1 - 1st Spider Defeated", Addresses.LostCavesPart11stSpider, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart1),
                new GenericLocationData("Lost Caves: Part 1 - 4th Flower Defeated", Addresses.LostCavesPart14thFlower, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart1),
                new GenericLocationData("Lost Caves: Part 1 - 2nd Flower Defeated", Addresses.LostCavesPart12ndFlower, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart1),
                new GenericLocationData("Lost Caves: Part 1 - 1st Flower Defeated", Addresses.LostCavesPart11stFlower, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart1),
                new GenericLocationData("Lost Caves: Part 1 - Sharp Thing 9 Defeated", Addresses.LostCavesPart1SharpThing9, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart1),
                new GenericLocationData("Lost Caves: Part 1 - 1st Bat Defeated", Addresses.LostCavesPart11stBat, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart1),
                new GenericLocationData("Lost Caves: Part 1 - 6th Spider Defeated", Addresses.LostCavesPart16thSpider, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart1),
                new GenericLocationData("Lost Caves: Part 1 - Sharp Thing 6 Defeated", Addresses.LostCavesPart1SharpThing6, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart1),
                new GenericLocationData("Lost Caves: Part 1 - Sharp Thing 13 (2) Defeated", Addresses.LostCavesPart1SharpThing132, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart1),
                new GenericLocationData("Lost Caves: Part 1 - Sharp Thing 5 Defeated", Addresses.LostCavesPart1SharpThing5, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart1),
                new GenericLocationData("Lost Caves: Part 1 - 2nd Bat Defeated", Addresses.LostCavesPart12ndBat, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart1),
                new GenericLocationData("Lost Caves: Part 1 - Sharp Thing 3 Defeated", Addresses.LostCavesPart1SharpThing3, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart1),
                new GenericLocationData("Lost Caves: Part 1 - Sharp Thing 8 Defeated", Addresses.LostCavesPart1SharpThing8, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart1),
                new GenericLocationData("Lost Caves: Part 1 - Sharp Thing 12 Defeated", Addresses.LostCavesPart1SharpThing12, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart1),
                new GenericLocationData("Lost Caves: Part 1 - Sharp Thing 10 Defeated", Addresses.LostCavesPart1SharpThing10, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart1),
                new GenericLocationData("Lost Caves: Part 1 - Sharp Thing 11 Defeated", Addresses.LostCavesPart1SharpThing11, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart1),
                new GenericLocationData("Lost Caves: Part 1 - Sharp Thing 7 Defeated", Addresses.LostCavesPart1SharpThing7, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart1),
                new GenericLocationData("Lost Caves: Part 1 - Sharp Thing 5 (2) Defeated", Addresses.LostCavesPart1SharpThing52, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart1),
                new GenericLocationData("Lost Caves: Part 1 - Sharp Thing 4 Defeated", Addresses.LostCavesPart1SharpThing4, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart1),
                new GenericLocationData("Lost Caves: Part 1 - 3rd Bat Defeated", Addresses.LostCavesPart13rdBat, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart1),
                new GenericLocationData("Lost Caves: Part 1 - Sharp Thing 2 Defeated", Addresses.LostCavesPart1SharpThing2, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart1),
                new GenericLocationData("Lost Caves: Part 1 - 5th Spider Defeated", Addresses.LostCavesPart15thSpider, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart1),
                new GenericLocationData("Lost Caves: Part 1 - 4th Spider Defeated", Addresses.LostCavesPart14thSpider, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart1),
                new GenericLocationData("Lost Caves: Part 2 - Spider 1 Defeated", Addresses.LostCavesPart2Spider1, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart2),
                new GenericLocationData("Lost Caves: Part 2 - Bat 3 Defeated", Addresses.LostCavesPart2Bat3, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart2),
                new GenericLocationData("Lost Caves: Part 2 - Bat 1 Defeated", Addresses.LostCavesPart2Bat1, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart2),
                new GenericLocationData("Lost Caves: Part 2 - Sharp Thing 7 Defeated", Addresses.LostCavesPart2SharpThing7, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart2),
                new GenericLocationData("Lost Caves: Part 2 - Sharp Thing 6 Defeated", Addresses.LostCavesPart2SharpThing6, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart2),
                new GenericLocationData("Lost Caves: Part 2 - Spider 2 Defeated", Addresses.LostCavesPart2Spider2, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart2),
                new GenericLocationData("Lost Caves: Part 2 - Bat 2 Defeated", Addresses.LostCavesPart2Bat2, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart2),
                new GenericLocationData("Lost Caves: Part 2 - Bat 4 Defeated", Addresses.LostCavesPart2Bat4, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart2),
                new GenericLocationData("Lost Caves: Part 2 - Sharp Thing 2 Defeated", Addresses.LostCavesPart2SharpThing2, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart2),
                new GenericLocationData("Lost Caves: Part 2 - Spider 8 Defeated", Addresses.LostCavesPart2Spider8, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart2),
                new GenericLocationData("Lost Caves: Part 2 - Spider 9 Defeated", Addresses.LostCavesPart2Spider9, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart2),
                new GenericLocationData("Lost Caves: Part 2 - Spider 7 Defeated", Addresses.LostCavesPart2Spider7, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart2),
                new GenericLocationData("Lost Caves: Part 2 - Sharp Thing 5 Defeated", Addresses.LostCavesPart2SharpThing5, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart2),
                new GenericLocationData("Lost Caves: Part 2 - Sharp Thing 4 Defeated", Addresses.LostCavesPart2SharpThing4, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart2),
                new GenericLocationData("Lost Caves: Part 2 - Sharp Thing 3 Defeated", Addresses.LostCavesPart2SharpThing3, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart2),
                new GenericLocationData("Lost Caves: Part 2 - Sharp Thing 1 Defeated", Addresses.LostCavesPart2SharpThing1, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart2),
                new GenericLocationData("Lost Caves: Part 2 - Spider 5 Defeated", Addresses.LostCavesPart2Spider5, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart2),
                new GenericLocationData("Lost Caves: Part 2 - Spider 6 Defeated", Addresses.LostCavesPart2Spider6, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart2),
                new GenericLocationData("Lost Caves: Part 2 - Spider 4 Defeated", Addresses.LostCavesPart2Spider4, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart2),
                new GenericLocationData("Lost Caves: Part 2 - Spider 3 Defeated", Addresses.LostCavesPart2Spider3, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart2),
                new GenericLocationData("Lost Caves: Part 3 - Flower 3 Defeated", Addresses.LostCavesPart3Flower3, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart3),
                new GenericLocationData("Lost Caves: Part 3 - Flower 2 Defeated", Addresses.LostCavesPart3Flower2, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart3),
                new GenericLocationData("Lost Caves: Part 3 - Flower 1 Defeated", Addresses.LostCavesPart3Flower1, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart3),
                new GenericLocationData("Lost Caves: Part 3 - Flower 6 Defeated", Addresses.LostCavesPart3Flower6, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart3),
                new GenericLocationData("Lost Caves: Part 3 - Bat 2 Defeated", Addresses.LostCavesPart3Bat2, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart3),
                new GenericLocationData("Lost Caves: Part 3 - Bat 1 Defeated", Addresses.LostCavesPart3Bat1, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart3),
                new GenericLocationData("Lost Caves: Part 3 - Spider 3 Defeated", Addresses.LostCavesPart3Spider3, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart3),
                new GenericLocationData("Lost Caves: Part 3 - Spider 4 Defeated", Addresses.LostCavesPart3Spider4, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart3),
                new GenericLocationData("Lost Caves: Part 3 - Spider 5 Defeated", Addresses.LostCavesPart3Spider5, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart3),
                new GenericLocationData("Lost Caves: Part 3 - Spider 2 Defeated", Addresses.LostCavesPart3Spider2, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart3),
                new GenericLocationData("Lost Caves: Part 3 - Flower 5 Defeated", Addresses.LostCavesPart3Flower5, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart3),
                new GenericLocationData("Lost Caves: Part 3 - Flower 4 Defeated", Addresses.LostCavesPart3Flower4, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart3),
                new GenericLocationData("Lost Caves: Part 3 - Spider 1 Defeated", Addresses.LostCavesPart3Spider1, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart3),
                new GenericLocationData("Lost Caves: Part 4 - Spider 1 Defeated", Addresses.LostCavesPart4Spider1, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart4),
                new GenericLocationData("Lost Caves: Part 4 - Sharp Thing 2 Defeated", Addresses.LostCavesPart4SharpThing2, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart4),
                new GenericLocationData("Lost Caves: Part 4 - Flower 7 Defeated", Addresses.LostCavesPart4Flower7, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart4),
                new GenericLocationData("Lost Caves: Part 4 - Flower 8 Defeated", Addresses.LostCavesPart4Flower8, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart4),
                new GenericLocationData("Lost Caves: Part 4 - Flower 6 Defeated", Addresses.LostCavesPart4Flower6, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart4),
                new GenericLocationData("Lost Caves: Part 4 - Sharp Thing 1 Defeated", Addresses.LostCavesPart4SharpThing1, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart4),
                new GenericLocationData("Lost Caves: Part 4 - Flower 3 Defeated", Addresses.LostCavesPart4Flower3, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart4),
                new GenericLocationData("Lost Caves: Part 4 - Flower 5 Defeated", Addresses.LostCavesPart4Flower5, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart4),
                new GenericLocationData("Lost Caves: Part 4 - Flower 4 Defeated", Addresses.LostCavesPart4Flower4, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart4),
                new GenericLocationData("Lost Caves: Part 4 - Flower 2 Defeated", Addresses.LostCavesPart4Flower2, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart4),
                new GenericLocationData("Lost Caves: Part 4 - Flower 1 Defeated", Addresses.LostCavesPart4Flower1, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaLostCavesPart4),
            };
        }

        private static List<GenericLocationData> GetFungusGrottoEnemyData()
        {
            string levelId = "5";
            return new List<GenericLocationData>()
            {
                new GenericLocationData("Fungus Grotto: Part 1 - 1st Fungus Bomb Defeated", Addresses.FungusGrottoPart11stFungusBomb, levelId, "1", LocationCheckType.Nibble, area: Addresses.AreaFungusGrottoPart1),
                new GenericLocationData("Fungus Grotto: Part 1 - 4th Fungus Bomb Defeated", Addresses.FungusGrottoPart14thFungusBomb, levelId, "1", LocationCheckType.Nibble, area: Addresses.AreaFungusGrottoPart1),
                new GenericLocationData("Fungus Grotto: Part 1 - 2nd Fungus Bomb Defeated", Addresses.FungusGrottoPart12ndFungusBomb, levelId, "1", LocationCheckType.Nibble, area: Addresses.AreaFungusGrottoPart1),
                new GenericLocationData("Fungus Grotto: Part 1 - 3rd Fungus Bomb Defeated", Addresses.FungusGrottoPart13rdFungusBomb, levelId, "1", LocationCheckType.Nibble, area: Addresses.AreaFungusGrottoPart1),
                new GenericLocationData("Fungus Grotto: Part 1 - 1st Snail Defeated", Addresses.FungusGrottoPart11stSnail, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaFungusGrottoPart1),
                new GenericLocationData("Fungus Grotto: Part 1 - 1st Shooting Mushroom (1st Enemy) Defeated", Addresses.FungusGrottoPart11stShootingMushroom1stEnemy, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaFungusGrottoPart1),
                new GenericLocationData("Fungus Grotto: Part 1 - 1st Bat Defeated", Addresses.FungusGrottoPart11stBat, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaFungusGrottoPart1),
                new GenericLocationData("Fungus Grotto: Part 1 - 3rd Snail (1st Upstairs) Defeated", Addresses.FungusGrottoPart13rdSnail1stUpstairs, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaFungusGrottoPart1),
                new GenericLocationData("Fungus Grotto: Part 1 - 2nd Small Mushroom Defeated", Addresses.FungusGrottoPart12ndSmallMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaFungusGrottoPart1),
                new GenericLocationData("Fungus Grotto: Part 1 - 1st Small Mushroom Defeated", Addresses.FungusGrottoPart11stSmallMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaFungusGrottoPart1),
                new GenericLocationData("Fungus Grotto: Part 1 - 2nd Snail Defeated", Addresses.FungusGrottoPart12ndSnail, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaFungusGrottoPart1),
                new GenericLocationData("Fungus Grotto: Part 1 - 3rd Small Mushroom Defeated", Addresses.FungusGrottoPart13rdSmallMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaFungusGrottoPart1),
                new GenericLocationData("Fungus Grotto: Part 1 - 4th Small Mushroom Defeated", Addresses.FungusGrottoPart14thSmallMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaFungusGrottoPart1),
                new GenericLocationData("Fungus Grotto: Part 1 - 5th Small Mushroom Defeated", Addresses.SkullFortressPart32ndBatAndFungusGrottoPart15thSmallMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaFungusGrottoPart1),
                new GenericLocationData("Fungus Grotto: Part 2 - 1st Fungus Bomb Defeated", Addresses.FungusGrottoPart21stFungusBomb, levelId, "1", LocationCheckType.Nibble, area: Addresses.AreaFungusGrottoPart2),
                new GenericLocationData("Fungus Grotto: Part 2 - 2nd Shooting Mushroom Defeated", Addresses.FungusGrottoPart22ndShootingMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaFungusGrottoPart2),
                new GenericLocationData("Fungus Grotto: Part 2 - 2nd Small Mushroom Defeated", Addresses.FungusGrottoPart22ndSmallMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaFungusGrottoPart2),
                new GenericLocationData("Fungus Grotto: Part 2 - 3rd Small Mushroom Defeated", Addresses.FungusGrottoPart23rdSmallMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaFungusGrottoPart2),
                new GenericLocationData("Fungus Grotto: Part 2 - 4th Small Mushroom Defeated", Addresses.FungusGrottoPart24thSmallMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaFungusGrottoPart2),
                new GenericLocationData("Fungus Grotto: Part 2 - 5th Small Mushroom Defeated", Addresses.FungusGrottoPart25thSmallMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaFungusGrottoPart2),
                new GenericLocationData("Fungus Grotto: Part 2 - 6th Small Mushroom Defeated", Addresses.FungusGrottoPart26thSmallMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaFungusGrottoPart2),
                new GenericLocationData("Fungus Grotto: Part 2 - 1st Small Mushroom Defeated", Addresses.FungusGrottoPart21stSmallMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaFungusGrottoPart2),
                new GenericLocationData("Fungus Grotto: Part 2 - 1st Bat Defeated", Addresses.FungusGrottoPart21stBat, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaFungusGrottoPart2),
                new GenericLocationData("Fungus Grotto: Part 2 - 1st Shooting Mushroom Defeated", Addresses.FungusGrottoPart21stShootingMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaFungusGrottoPart2),
                new GenericLocationData("Fungus Grotto: Part 3 - 5th Fungus Bomb Defeated", Addresses.FungusGrottoPart35thFungusBomb, levelId, "1", LocationCheckType.Nibble, area: Addresses.AreaFungusGrottoPart3),
                new GenericLocationData("Fungus Grotto: Part 3 - 4th Fungus Bomb Defeated", Addresses.FungusGrottoPart34thFungusBomb, levelId, "1", LocationCheckType.Nibble, area: Addresses.AreaFungusGrottoPart3),
                new GenericLocationData("Fungus Grotto: Part 3 - 3rd Fungus Bomb Defeated", Addresses.FungusGrottoPart33rdFungusBomb, levelId, "1", LocationCheckType.Nibble, area: Addresses.AreaFungusGrottoPart3),
                new GenericLocationData("Fungus Grotto: Part 3 - 2nd Fungus Bomb Defeated", Addresses.FungusGrottoPart32ndFungusBomb, levelId, "1", LocationCheckType.Nibble, area: Addresses.AreaFungusGrottoPart3),
                new GenericLocationData("Fungus Grotto: Part 3 - 1st Fungus Bomb Defeated", Addresses.FungusGrottoPart31stFungusBomb, levelId, "1", LocationCheckType.Nibble, area: Addresses.AreaFungusGrottoPart3),
                new GenericLocationData("Fungus Grotto: Part 3 - 3rd Small Mushroom Defeated", Addresses.FungusGrottoPart33rdSmallMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaFungusGrottoPart3),
                new GenericLocationData("Fungus Grotto: Part 3 - 5th Small Mushroom Defeated", Addresses.FungusGrottoPart35thSmallMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaFungusGrottoPart3),
                new GenericLocationData("Fungus Grotto: Part 3 - 1st Small Mushroom Defeated", Addresses.FungusGrottoPart31stSmallMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaFungusGrottoPart3),
                new GenericLocationData("Fungus Grotto: Part 3 - 1st Snail Defeated", Addresses.FungusGrottoPart31stSnail, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaFungusGrottoPart3),
                new GenericLocationData("Fungus Grotto: Part 3 - 2nd Shooting Mushroom Defeated", Addresses.FungusGrottoPart32ndShootingMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaFungusGrottoPart3),
                new GenericLocationData("Fungus Grotto: Part 3 - 3rd Shooting Mushroom Defeated", Addresses.FungusGrottoPart33rdShootingMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaFungusGrottoPart3),
                new GenericLocationData("Fungus Grotto: Part 3 - 1st Shooting Mushroom Defeated", Addresses.FungusGrottoPart31stShootingMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaFungusGrottoPart3),
                new GenericLocationData("Fungus Grotto: Part 3 - 4th Shooting Mushroom Defeated", Addresses.FungusGrottoPart34thShootingMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaFungusGrottoPart3),
                new GenericLocationData("Fungus Grotto: Part 3 - 2nd Snail Defeated", Addresses.FungusGrottoPart32ndSnail, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaFungusGrottoPart3),
                new GenericLocationData("Fungus Grotto: Part 3 - 8th Small Mushroom Defeated", Addresses.FungusGrottoPart38thSmallMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaFungusGrottoPart3),
                new GenericLocationData("Fungus Grotto: Part 3 - 7th Small Mushroom Defeated", Addresses.FungusGrottoPart37thSmallMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaFungusGrottoPart3),
                new GenericLocationData("Fungus Grotto: Part 3 - 6th Small Mushroom Defeated", Addresses.FungusGrottoPart36thSmallMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaFungusGrottoPart3),
                new GenericLocationData("Fungus Grotto: Part 3 - 4th Small Mushroom Defeated", Addresses.FungusGrottoPart34thSmallMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaFungusGrottoPart3),
                new GenericLocationData("Fungus Grotto: Part 3 - 2nd Small Mushroom Defeated", Addresses.FungusGrottoPart32ndSmallMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaFungusGrottoPart3),
                new GenericLocationData("Fungus Grotto: Part 3 - 1st Bat Defeated", Addresses.SkullFortressPart32ndGoonAndFungusGrottoPart31stBat, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaFungusGrottoPart3),
                new GenericLocationData("Fungus Grotto: Part 3 - 3rd Snail Defeated", Addresses.FungusGrottoPart33rdSnail, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaFungusGrottoPart3),
                new GenericLocationData("Fungus Grotto: Part 4 - 3rd Fungus Bomb Defeated", Addresses.FungusGrottoPart43rdFungusBomb, levelId, "1", LocationCheckType.Nibble, area: Addresses.AreaFungusGrottoPart4),
                new GenericLocationData("Fungus Grotto: Part 4 - 2nd Fungus Bomb Defeated", Addresses.FungusGrottoPart42ndFungusBomb, levelId, "1", LocationCheckType.Nibble, area: Addresses.AreaFungusGrottoPart4),
                new GenericLocationData("Fungus Grotto: Part 4 - 4th Fungus Bomb Defeated", Addresses.FungusGrottoPart44thFungusBomb, levelId, "1", LocationCheckType.Nibble, area: Addresses.AreaFungusGrottoPart4),
                new GenericLocationData("Fungus Grotto: Part 4 - 1st Fungus Bomb Defeated", Addresses.FungusGrottoPart41stFungusBomb, levelId, "1", LocationCheckType.Nibble, area: Addresses.AreaFungusGrottoPart4),
                new GenericLocationData("Fungus Grotto: Part 4 - 1st Bat Defeated", Addresses.FungusGrottoPart41stBat, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaFungusGrottoPart4),
                new GenericLocationData("Fungus Grotto: Part 4 - 1st Shooting Mushroom Defeated", Addresses.FungusGrottoPart41stShootingMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaFungusGrottoPart4),
                new GenericLocationData("Fungus Grotto: Part 4 - 2nd Shooting Mushroom Defeated", Addresses.FungusGrottoPart42ndShootingMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaFungusGrottoPart4),
                new GenericLocationData("Fungus Grotto: Part 4 - 3rd Shooting Mushroom Defeated", Addresses.FungusGrottoPart43rdShootingMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaFungusGrottoPart4),
            };
        }

        private static List<GenericLocationData> GetAcidPoolsEnemyData()
        {
            string levelId = "6";
            return new List<GenericLocationData>()
            {
                new GenericLocationData("Acid Pools: Part 1 - 1st Snail Defeated", Addresses.AcidPoolsPart11stSnail, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaAcidPoolsPart1),
                new GenericLocationData("Acid Pools: Part 1 - 2nd Snail Defeated", Addresses.AcidPoolsPart12ndSnail, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaAcidPoolsPart1),
                new GenericLocationData("Acid Pools: Part 1 - 4th Shooting Mushroom Defeated", Addresses.AcidPoolsPart14thShootingMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaAcidPoolsPart1),
                new GenericLocationData("Acid Pools: Part 1 - 3rd Shooting Mushroom Defeated", Addresses.AcidPoolsPart13rdShootingMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaAcidPoolsPart1),
                new GenericLocationData("Acid Pools: Part 1 - 2nd Shooting Mushroom Defeated", Addresses.AcidPoolsPart12ndShootingMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaAcidPoolsPart1),
                new GenericLocationData("Acid Pools: Part 1 - 1st Small Mushroom Defeated", Addresses.AcidPoolsPart11stSmallMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaAcidPoolsPart1),
                new GenericLocationData("Acid Pools: Part 1 - 2nd Small Mushroom Defeated", Addresses.AcidPoolsPart12ndSmallMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaAcidPoolsPart1),
                new GenericLocationData("Acid Pools: Part 1 - 3rd Small Mushroom Defeated", Addresses.AcidPoolsPart13rdSmallMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaAcidPoolsPart1),
                new GenericLocationData("Acid Pools: Part 1 - 4th Small Mushroom Defeated", Addresses.AcidPoolsPart14thSmallMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaAcidPoolsPart1),
                new GenericLocationData("Acid Pools: Part 1 - 5th Small Mushroom Defeated", Addresses.AcidPoolsPart15thSmallMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaAcidPoolsPart1),
                new GenericLocationData("Acid Pools: Part 1 - 6th Small Mushroom Defeated", Addresses.AcidPoolsPart16thSmallMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaAcidPoolsPart1),
                new GenericLocationData("Acid Pools: Part 1 - 10th Small Mushroom Defeated", Addresses.AcidPoolsPart110thSmallMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaAcidPoolsPart1),
                new GenericLocationData("Acid Pools: Part 1 - 7th Small Mushroom Defeated", Addresses.AcidPoolsPart17thSmallMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaAcidPoolsPart1),
                new GenericLocationData("Acid Pools: Part 1 - 11th Small Mushroom Defeated", Addresses.AcidPoolsPart111thSmallMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaAcidPoolsPart1),
                new GenericLocationData("Acid Pools: Part 1 - 12th Small Mushroom Defeated", Addresses.AcidPoolsPart112thSmallMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaAcidPoolsPart1),
                new GenericLocationData("Acid Pools: Part 1 - 8th Small Mushroom Defeated", Addresses.AcidPoolsPart18thSmallMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaAcidPoolsPart1),
                new GenericLocationData("Acid Pools: Part 1 - 9th Small Mushroom Defeated", Addresses.AcidPoolsPart19thSmallMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaAcidPoolsPart1),
                new GenericLocationData("Acid Pools: Part 1 - 1st Shooting Mushroom Defeated", Addresses.AcidPoolsPart11stShootingMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaAcidPoolsPart1),
                new GenericLocationData("Acid Pools: Part 2 - 2nd Small Dude Defeated", Addresses.AcidPoolsPart22ndSmallDude, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaAcidPoolsPart2),
                new GenericLocationData("Acid Pools: Part 2 - 6th Bat Defeated", Addresses.AcidPoolsPart26thBat, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaAcidPoolsPart2),
                new GenericLocationData("Acid Pools: Part 2 - 4th Bat Defeated", Addresses.AcidPoolsPart24thBat, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaAcidPoolsPart2),
                new GenericLocationData("Acid Pools: Part 2 - 5th Bat Defeated", Addresses.AcidPoolsPart25thBat, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaAcidPoolsPart2),
                new GenericLocationData("Acid Pools: Part 2 - Shooting Mushroom at the End Defeated", Addresses.AcidPoolsPart2ShootingMushroomAtTheEnd, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaAcidPoolsPart2),
                new GenericLocationData("Acid Pools: Part 2 - 7th Bat Defeated", Addresses.AcidPoolsPart27thBat, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaAcidPoolsPart2),
                new GenericLocationData("Acid Pools: Part 2 - 4th Small Dude Defeated", Addresses.AcidPoolsPart24thSmallDude, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaAcidPoolsPart2),
                new GenericLocationData("Acid Pools: Part 2 - 8th Small Dude Defeated", Addresses.AcidPoolsPart28thSmallDude, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaAcidPoolsPart2),
                new GenericLocationData("Acid Pools: Part 2 - 5th Small Dude Defeated", Addresses.AcidPoolsPart25thSmallDude, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaAcidPoolsPart2),
                new GenericLocationData("Acid Pools: Part 2 - 1st Small Dude Defeated", Addresses.AcidPoolsPart21stSmallDude, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaAcidPoolsPart2),
                new GenericLocationData("Acid Pools: Part 2 - 7th Small Dude Defeated", Addresses.AcidPoolsPart27thSmallDude, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaAcidPoolsPart2),
                new GenericLocationData("Acid Pools: Part 2 - 3rd Small Dude Defeated", Addresses.AcidPoolsPart23rdSmallDude, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaAcidPoolsPart2),
                new GenericLocationData("Acid Pools: Part 2 - 6th Small Dude Defeated", Addresses.AcidPoolsPart26thSmallDude, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaAcidPoolsPart2),
                new GenericLocationData("Acid Pools: Part 2 - 2nd Small Mushroom Defeated", Addresses.AcidPoolsPart22ndSmallMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaAcidPoolsPart2),
                new GenericLocationData("Acid Pools: Part 2 - 12th Small Mushroom Defeated", Addresses.AcidPoolsPart212thSmallMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaAcidPoolsPart2),
                new GenericLocationData("Acid Pools: Part 2 - 11th Small Mushroom Defeated", Addresses.AcidPoolsPart211thSmallMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaAcidPoolsPart2),
                new GenericLocationData("Acid Pools: Part 2 - 10th Small Mushroom Defeated", Addresses.AcidPoolsPart210thSmallMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaAcidPoolsPart2),
                new GenericLocationData("Acid Pools: Part 2 - 9th Small Mushroom Defeated", Addresses.AcidPoolsPart29thSmallMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaAcidPoolsPart2),
                new GenericLocationData("Acid Pools: Part 2 - 8th Small Mushroom Defeated", Addresses.AcidPoolsPart28thSmallMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaAcidPoolsPart2),
                new GenericLocationData("Acid Pools: Part 2 - 7th Small Mushroom Defeated", Addresses.AcidPoolsPart27thSmallMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaAcidPoolsPart2),
                new GenericLocationData("Acid Pools: Part 2 - 5th Small Mushroom Defeated", Addresses.AcidPoolsPart25thSmallMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaAcidPoolsPart2),
                new GenericLocationData("Acid Pools: Part 2 - 3rd Small Mushroom Defeated", Addresses.AcidPoolsPart23rdSmallMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaAcidPoolsPart2),
                new GenericLocationData("Acid Pools: Part 2 - 4th Small Mushroom Defeated", Addresses.AcidPoolsPart24thSmallMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaAcidPoolsPart2),
                new GenericLocationData("Acid Pools: Part 2 - 6th Small Mushroom Defeated", Addresses.AcidPoolsPart26thSmallMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaAcidPoolsPart2),
                new GenericLocationData("Acid Pools: Part 2 - 3rd Bat Defeated", Addresses.AcidPoolsPart23rdBat, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaAcidPoolsPart2),
                new GenericLocationData("Acid Pools: Part 2 - 2nd Bat Defeated", Addresses.AcidPoolsPart22ndBat, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaAcidPoolsPart2),
                new GenericLocationData("Acid Pools: Part 2 - 1st Bat Defeated", Addresses.AcidPoolsPart21stBat, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaAcidPoolsPart2),
                new GenericLocationData("Acid Pools: Part 2 - 1st Small Mushroom Defeated", Addresses.AcidPoolsPart21stSmallMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaAcidPoolsPart2),
            };
        }

        private static List<GenericLocationData> GetBurningDesertEnemyData()
        {
            string levelId = "8";
            return new List<GenericLocationData>()
            {
                new GenericLocationData("Burning Desert: Part 1 - 5th Dude Defeated", Addresses.BurningDesertPart15thDude, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBurningDesertPart1),
                new GenericLocationData("Burning Desert: Part 1 - 2nd Bee Defeated", Addresses.BurningDesertPart12ndBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBurningDesertPart1),
                new GenericLocationData("Burning Desert: Part 1 - 1st Bee Defeated", Addresses.BurningDesertPart11stBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBurningDesertPart1),
                new GenericLocationData("Burning Desert: Part 1 - 5th Bee Defeated", Addresses.BurningDesertPart15thBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBurningDesertPart1),
                new GenericLocationData("Burning Desert: Part 1 - 4th Bee Defeated", Addresses.BurningDesertPart14thBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBurningDesertPart1),
                new GenericLocationData("Burning Desert: Part 1 - 6th Dude Defeated", Addresses.BurningDesertPart16thDude, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBurningDesertPart1),
                new GenericLocationData("Burning Desert: Part 1 - 4th Dude Defeated", Addresses.BurningDesertPart14thDude, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBurningDesertPart1),
                new GenericLocationData("Burning Desert: Part 1 - 3rd Bee Defeated", Addresses.BurningDesertPart13rdBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBurningDesertPart1),
                new GenericLocationData("Burning Desert: Part 1 - 2nd Dude Defeated", Addresses.BurningDesertPart12ndDude, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBurningDesertPart1),
                new GenericLocationData("Burning Desert: Part 1 - 4th Kenga Defeated", Addresses.BurningDesertPart14thKenga, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBurningDesertPart1),
                new GenericLocationData("Burning Desert: Part 1 - 3rd Kenga Defeated", Addresses.BurningDesertPart13rdKenga, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBurningDesertPart1),
                new GenericLocationData("Burning Desert: Part 1 - 3rd Dude Defeated", Addresses.BurningDesertPart13rdDude, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBurningDesertPart1),
                new GenericLocationData("Burning Desert: Part 1 - 1st Dude Defeated", Addresses.BurningDesertPart11stDude, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBurningDesertPart1),
                new GenericLocationData("Burning Desert: Part 1 - 2nd Kenga Defeated", Addresses.BurningDesertPart12ndKenga, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBurningDesertPart1),
                new GenericLocationData("Burning Desert: Part 1 - 1st Kenga Defeated", Addresses.BurningDesertPart11stKenga, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBurningDesertPart1),
                new GenericLocationData("Burning Desert: Part 2 - 1st Kenga Defeated", Addresses.BurningDesertPart21stKenga, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBurningDesertPart2),
                new GenericLocationData("Burning Desert: Part 2 - 1st Hedgehog Defeated", Addresses.BurningDesertPart21stHedgehog, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBurningDesertPart2),
                new GenericLocationData("Burning Desert: Part 2 - 2nd Bee Defeated", Addresses.BurningDesertPart22ndBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBurningDesertPart2),
                new GenericLocationData("Burning Desert: Part 2 - 1st Dude Defeated", Addresses.BurningDesertPart21stDude, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBurningDesertPart2),
                new GenericLocationData("Burning Desert: Part 2 - 2nd Dude Defeated", Addresses.BurningDesertPart22ndDude, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBurningDesertPart2),
                new GenericLocationData("Burning Desert: Part 2 - 2nd Bully Defeated", Addresses.BurningDesertPart22ndBully, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBurningDesertPart2),
                new GenericLocationData("Burning Desert: Part 2 - 1st Bee Defeated", Addresses.BurningDesertPart21stBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBurningDesertPart2),
                new GenericLocationData("Burning Desert: Part 2 - 1st Bully Defeated", Addresses.BurningDesertPart21stBully, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBurningDesertPart2),
                new GenericLocationData("Burning Desert: Part 3 - 1st Hedgehog Defeated", Addresses.BurningDesertPart31stHedgehog, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBurningDesertPart3),
                new GenericLocationData("Burning Desert: Part 3 - 3rd Bee Defeated", Addresses.BurningDesertPart33rdBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBurningDesertPart3),
                new GenericLocationData("Burning Desert: Part 3 - 1st Hedgehog (2) Defeated", Addresses.BurningDesertPart31stHedgehog2, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBurningDesertPart3),
                new GenericLocationData("Burning Desert: Part 3 - 1st Bully Defeated", Addresses.BurningDesertPart31stBullyAndBrankyWastesPart31stHedgehog, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBurningDesertPart3),
                new GenericLocationData("Burning Desert: Part 3 - 8th Bee Defeated", Addresses.BurningDesertPart38thBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBurningDesertPart3),
                new GenericLocationData("Burning Desert: Part 3 - 7th Bee Defeated", Addresses.BurningDesertPart37thBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBurningDesertPart3),
                new GenericLocationData("Burning Desert: Part 3 - 6th Bee Defeated", Addresses.BurningDesertPart36thBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBurningDesertPart3),
                new GenericLocationData("Burning Desert: Part 3 - 5th Bee Defeated", Addresses.BurningDesertPart35thBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBurningDesertPart3),
                new GenericLocationData("Burning Desert: Part 3 - 2nd Bee Defeated", Addresses.BurningDesertPart32ndBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBurningDesertPart3),
                new GenericLocationData("Burning Desert: Part 3 - 1st Bee Defeated", Addresses.BurningDesertPart31stBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBurningDesertPart3),
                new GenericLocationData("Burning Desert: Part 3 - 1st Dude Defeated", Addresses.BurningDesertPart31stDude, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBurningDesertPart3),
                new GenericLocationData("Burning Desert: Part 3 - 4th Bee Defeated", Addresses.BurningDesertPart34thBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBurningDesertPart3),
                new GenericLocationData("Burning Desert: Part 4 - 3rd Bee Defeated", Addresses.BurningDesertPart43rdBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBurningDesertPart4),
                new GenericLocationData("Burning Desert: Part 4 - 1st Hedgehog Defeated", Addresses.BurningDesertPart41stHedgehog, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBurningDesertPart4),
                new GenericLocationData("Burning Desert: Part 4 - 1st Bee Defeated", Addresses.BurningDesertPart41stBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBurningDesertPart4),
                new GenericLocationData("Burning Desert: Part 4 - 2nd Bee Defeated", Addresses.BurningDesertPart42ndBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBurningDesertPart4),
                new GenericLocationData("Burning Desert: Part 4 - 1st Kenga Defeated", Addresses.BurningDesertPart41stKenga, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBurningDesertPart4),
                new GenericLocationData("Burning Desert: Part 4 - 7th Bee Defeated", Addresses.BurningDesertPart47thBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBurningDesertPart4),
                new GenericLocationData("Burning Desert: Part 4 - 8th Bee Defeated", Addresses.BurningDesertPart48thBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBurningDesertPart4),
                new GenericLocationData("Burning Desert: Part 4 - 2nd Kenga Defeated", Addresses.BurningDesertPart42ndKenga, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBurningDesertPart4),
                new GenericLocationData("Burning Desert: Part 4 - 9th Bee Defeated", Addresses.BurningDesertPart49thBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBurningDesertPart4),
                new GenericLocationData("Burning Desert: Part 4 - 10th Bee Defeated", Addresses.BurningDesertPart410thBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBurningDesertPart4),
                new GenericLocationData("Burning Desert: Part 4 - 2nd Kenga (2) Defeated", Addresses.BurningDesertPart42ndKenga2, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBurningDesertPart4),
                new GenericLocationData("Burning Desert: Part 4 - 6th Bee Defeated", Addresses.BurningDesertPart46thBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBurningDesertPart4),
                new GenericLocationData("Burning Desert: Part 4 - 5th Bee Defeated", Addresses.BurningDesertPart45thBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBurningDesertPart4),
                new GenericLocationData("Burning Desert: Part 4 - 4th Bee Defeated", Addresses.BurningDesertPart44thBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBurningDesertPart4),
            };
        }

        private static List<GenericLocationData> GetBrankyWastesEnemyData()
        {
            string levelId = "9";
            return new List<GenericLocationData>()
            {
                new GenericLocationData("Branky Wastes: Part 1 - 5th Bee (Low Path) Defeated", Addresses.BrankyWastesPart15thBeeLowPath, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBrankyWastesPart1),
                new GenericLocationData("Branky Wastes: Part 1 - 6th Bee (Low Path) Defeated", Addresses.BrankyWastesPart16thBeeLowPath, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBrankyWastesPart1),
                new GenericLocationData("Branky Wastes: Part 1 - 7th Bee (Low Path) Defeated", Addresses.BrankyWastesPart17thBeeLowPath, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBrankyWastesPart1),
                new GenericLocationData("Branky Wastes: Part 1 - 1st Kenga Defeated", Addresses.BrankyWastesPart11stKenga, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBrankyWastesPart1),
                new GenericLocationData("Branky Wastes: Part 1 - 1st Hedgehog Defeated", Addresses.BrankyWastesPart11stHedgehog, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBrankyWastesPart1),
                new GenericLocationData("Branky Wastes: Part 1 - 4th Bee Defeated", Addresses.BrankyWastesPart14thBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBrankyWastesPart1),
                new GenericLocationData("Branky Wastes: Part 1 - 3rd Bee Defeated", Addresses.BrankyWastesPart13rdBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBrankyWastesPart1),
                new GenericLocationData("Branky Wastes: Part 1 - 2nd Bee Defeated", Addresses.BrankyWastesPart12ndBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBrankyWastesPart1),
                new GenericLocationData("Branky Wastes: Part 1 - 1st Bee Defeated", Addresses.BrankyWastesPart11stBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBrankyWastesPart1),
                new GenericLocationData("Branky Wastes: Part 1 - 8th Bee (Upper Path) Defeated", Addresses.BrankyWastesPart18thBeeUpperPath, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBrankyWastesPart1),
                new GenericLocationData("Branky Wastes: Part 1 - 1st Dude Defeated", Addresses.BrankyWastesPart11stDude, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBrankyWastesPart1),
                new GenericLocationData("Branky Wastes: Part 1 - 2nd Dude Defeated", Addresses.BrankyWastesPart12ndDude, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBrankyWastesPart1),
                new GenericLocationData("Branky Wastes: Part 2 - 1st Bee Defeated", Addresses.BrankyWastesPart21stBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBrankyWastesPart2),
                new GenericLocationData("Branky Wastes: Part 2 - 2nd Bee Defeated", Addresses.BrankyWastesPart22ndBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBrankyWastesPart2),
                new GenericLocationData("Branky Wastes: Part 2 - 3rd Bee Defeated", Addresses.BrankyWastesPart23rdBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBrankyWastesPart2),
                new GenericLocationData("Branky Wastes: Part 2 - One More Dude Defeated", Addresses.BrankyWastesPart2OneMoreDude, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBrankyWastesPart2),
                new GenericLocationData("Branky Wastes: Part 2 - 2nd Dude Defeated", Addresses.BrankyWastesPart22ndDude, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBrankyWastesPart2),
                new GenericLocationData("Branky Wastes: Part 2 - 4th Bully Defeated", Addresses.BrankyWastesPart24thBully, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBrankyWastesPart2),
                new GenericLocationData("Branky Wastes: Part 2 - 3rd Bully Defeated", Addresses.BrankyWastesPart23rdBully, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBrankyWastesPart2),
                new GenericLocationData("Branky Wastes: Part 2 - 2nd Bully Defeated", Addresses.BrankyWastesPart22ndBully, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBrankyWastesPart2),
                new GenericLocationData("Branky Wastes: Part 2 - 1st Bully Defeated", Addresses.BrankyWastesPart21stBully, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBrankyWastesPart2),
                new GenericLocationData("Branky Wastes: Part 3 - 1st Hedgehog Defeated", Addresses.BurningDesertPart31stBullyAndBrankyWastesPart31stHedgehog, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBrankyWastesPart3),
                new GenericLocationData("Branky Wastes: Part 3 - 2nd Dude Defeated", Addresses.BrankyWastesPart32ndDudeAndCloudCitadelPart11stSmallDude, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBrankyWastesPart3),
                new GenericLocationData("Branky Wastes: Part 3 - 5th Dude Defeated", Addresses.BrankyWastesPart35thDude, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBrankyWastesPart3),
                new GenericLocationData("Branky Wastes: Part 3 - 3rd Dude Defeated", Addresses.BrankyWastesPart33rdDude, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBrankyWastesPart3),
                new GenericLocationData("Branky Wastes: Part 3 - 4th Dude Defeated", Addresses.BrankyWastesPart34thDude, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBrankyWastesPart3),
                new GenericLocationData("Branky Wastes: Part 3 - 1st Dude Defeated", Addresses.BrankyWastesPart31stDude, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBrankyWastesPart3),
                new GenericLocationData("Branky Wastes: Part 3 - 1st Bully Defeated", Addresses.BrankyWastesPart31stBully, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBrankyWastesPart3),
                new GenericLocationData("Branky Wastes: Part 4 - 6th Bee Defeated", Addresses.BrankyWastesPart46thBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBrankyWastesPart4),
                new GenericLocationData("Branky Wastes: Part 4 - 1st Bee Defeated", Addresses.BrankyWastesPart41stBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBrankyWastesPart4),
                new GenericLocationData("Branky Wastes: Part 4 - 2nd Bee Defeated", Addresses.BrankyWastesPart42ndBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBrankyWastesPart4),
                new GenericLocationData("Branky Wastes: Part 4 - 3rd Bee Defeated", Addresses.BrankyWastesPart43rdBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBrankyWastesPart4),
                new GenericLocationData("Branky Wastes: Part 4 - 4th Bee Defeated", Addresses.BrankyWastesPart44thBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBrankyWastesPart4),
                new GenericLocationData("Branky Wastes: Part 4 - 5th Bee Defeated", Addresses.BrankyWastesPart45thBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBrankyWastesPart4),
                new GenericLocationData("Branky Wastes: Part 4 - 7th Bee Defeated", Addresses.BrankyWastesPart47thBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBrankyWastesPart4),
                new GenericLocationData("Branky Wastes: Part 4 - 8th Bee Defeated", Addresses.BrankyWastesPart48thBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBrankyWastesPart4),
                new GenericLocationData("Branky Wastes: Part 4 - 4th Kenga Defeated", Addresses.BrankyWastesPart44thKenga, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBrankyWastesPart4),
                new GenericLocationData("Branky Wastes: Part 4 - 3rd Kenga Defeated", Addresses.BrankyWastesPart43rdKenga, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBrankyWastesPart4),
                new GenericLocationData("Branky Wastes: Part 4 - 1st Kenga Defeated", Addresses.BrankyWastesPart41stKenga, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBrankyWastesPart4),
                new GenericLocationData("Branky Wastes: Part 4 - 2nd Kenga Defeated", Addresses.BrankyWastesPart42ndKenga, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaBrankyWastesPart4),
            };
        }

        private static List<GenericLocationData> GetSpiderForestEnemyData()
        {
            string levelId = "10";
            return new List<GenericLocationData>()
            {
                new GenericLocationData("Spider Forest: Part 1 - 1st Snail Defeated", Addresses.SpiderForestPart11stSnail, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSpiderForestPart1),
                new GenericLocationData("Spider Forest: Part 1 - 2nd Bee or Part 3 - 4th Spider Defeated", Addresses.SpiderForestPart12ndBeeOrPart34thSpider, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSpiderForestPart1),
                new GenericLocationData("Spider Forest: Part 1 - 2nd Shooting Mushroom Defeated", Addresses.SpiderForestPart12ndShootingMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSpiderForestPart1),
                new GenericLocationData("Spider Forest: Part 1 - 3rd Bee Defeated", Addresses.SpiderForestPart13rdBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSpiderForestPart1),
                new GenericLocationData("Spider Forest: Part 1 - 1st Bee Defeated", Addresses.SpiderForestPart11stBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSpiderForestPart1),
                new GenericLocationData("Spider Forest: Part 1 - 2nd Snail Defeated", Addresses.SpiderForestPart12ndSnail, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSpiderForestPart1),
                new GenericLocationData("Spider Forest: Part 1 - Shooting Mushroom Defeated", Addresses.SpiderForestPart1ShootingMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSpiderForestPart1),
                new GenericLocationData("Spider Forest: Part 2 - 7th Spider Defeated", Addresses.SpiderForestPart27thSpider, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSpiderForestPart2),
                new GenericLocationData("Spider Forest: Part 2 - 2nd Bee Defeated", Addresses.SpiderForestPart22ndBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSpiderForestPart2),
                new GenericLocationData("Spider Forest: Part 2 - 10th Spider Defeated", Addresses.SpiderForestPart210thSpider, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSpiderForestPart2),
                new GenericLocationData("Spider Forest: Part 2 - 11th Spider Defeated", Addresses.SpiderForestPart211thSpider, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSpiderForestPart2),
                new GenericLocationData("Spider Forest: Part 2 - 12th Spider Defeated", Addresses.SpiderForestPart212thSpider, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSpiderForestPart2),
                new GenericLocationData("Spider Forest: Part 2 - 1st Bee Defeated", Addresses.SpiderForestPart21stBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSpiderForestPart2),
                new GenericLocationData("Spider Forest: Part 2 - 1st Spider Defeated", Addresses.SpiderForestPart21stSpider, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSpiderForestPart2),
                new GenericLocationData("Spider Forest: Part 2 - 8th Spider Defeated", Addresses.SpiderForestPart28thSpider, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSpiderForestPart2),
                new GenericLocationData("Spider Forest: Part 2 - 9th Spider Defeated", Addresses.SpiderForestPart29thSpider, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSpiderForestPart2),
                new GenericLocationData("Spider Forest: Part 2 - 2nd Spider Defeated", Addresses.SpiderForestPart22ndSpider, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSpiderForestPart2),
                new GenericLocationData("Spider Forest: Part 2 - 5th Spider Defeated", Addresses.SpiderForestPart25thSpider, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSpiderForestPart2),
                new GenericLocationData("Spider Forest: Part 2 - 6th Spider Defeated", Addresses.SpiderForestPart26thSpider, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSpiderForestPart2),
                new GenericLocationData("Spider Forest: Part 2 - 3rd Spider Defeated", Addresses.SpiderForestPart23rdSpider, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSpiderForestPart2),
                new GenericLocationData("Spider Forest: Part 2 - 4th Spider Defeated", Addresses.SpiderForestPart24thSpider, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSpiderForestPart2),
                new GenericLocationData("Spider Forest: Part 2 - 13th Spider Defeated", Addresses.SpiderForestPart213thSpider, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSpiderForestPart2),
                new GenericLocationData("Spider Forest: Part 3 - 3rd Bee Defeated", Addresses.SpiderForestPart33rdBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSpiderForestPart3),
                new GenericLocationData("Spider Forest: Part 3 - 4th Bee Defeated", Addresses.SpiderForestPart34thBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSpiderForestPart3),
                new GenericLocationData("Spider Forest: Part 3 - 5th Spider Defeated", Addresses.SpiderForestPart35thSpider, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSpiderForestPart3),
                new GenericLocationData("Spider Forest: Part 3 - 2nd Spider Defeated", Addresses.SpiderForestPart32ndSpider, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSpiderForestPart3),
                new GenericLocationData("Spider Forest: Part 3 - 1st Bee Defeated", Addresses.SpiderForestPart31stBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSpiderForestPart3),
                new GenericLocationData("Spider Forest: Part 3 - 2nd Bee Defeated", Addresses.SpiderForestPart32ndBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSpiderForestPart3),
                new GenericLocationData("Spider Forest: Part 3 - 1st Spider Defeated", Addresses.SpiderForestPart31stSpider, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSpiderForestPart3),
                new GenericLocationData("Spider Forest: Part 3 - 3rd Spider Defeated", Addresses.SpiderForestPart33rdSpider, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSpiderForestPart3),
                new GenericLocationData("Spider Forest: Part 3 - 1st Shooting Mushroom Defeated", Addresses.SpiderForestPart31stShootingMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaSpiderForestPart3),
                new GenericLocationData("Spider Forest: Part 3 - 1st Dude Defeated", Addresses.SpiderForestPart31stDude, levelId, "522", LocationCheckType.UShort, area: Addresses.AreaSpiderForestPart3),
                new GenericLocationData("Spider Forest: Part 3 - 2nd Dude Defeated", Addresses.SpiderForestPart32ndDude, levelId, "522", LocationCheckType.UShort, area: Addresses.AreaSpiderForestPart3),
            };
        }

        private static List<GenericLocationData> GetCanopyVillageEnemyData()
        {
            string levelId = "11";
            return new List<GenericLocationData>()
            {
                new GenericLocationData("Canopy Village: Part 1 - 3rd Bee Defeated", Addresses.CanopyVillagePart13rdBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCanopyVillagePart1),
                new GenericLocationData("Canopy Village: Part 1 - 2nd Bee or Part 5 - 7th Bee Defeated", Addresses.CanopyVillagePart12ndBeeOrPart57thBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCanopyVillagePart1),
                new GenericLocationData("Canopy Village: Part 1 - 4th Bee Defeated", Addresses.CanopyVillagePart14thBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCanopyVillagePart1),
                new GenericLocationData("Canopy Village: Part 1 - 2nd Shooting Mushroom Defeated", Addresses.CanopyVillagePart12ndShootingMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCanopyVillagePart1),
                new GenericLocationData("Canopy Village: Part 1 - 1st Bee Defeated", Addresses.CanopyVillagePart11stBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCanopyVillagePart1),
                new GenericLocationData("Canopy Village: Part 1 - 1st Shooting Mushroom Defeated", Addresses.CanopyVillagePart11stShootingMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCanopyVillagePart1),
                new GenericLocationData("Canopy Village: Part 2 - 1st Spider Defeated", Addresses.CanopyVillagePart21stSpider, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCanopyVillagePart2),
                new GenericLocationData("Canopy Village: Part 2 - 1st Dude Defeated", Addresses.CanopyVillagePart21stDude, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCanopyVillagePart2),
                new GenericLocationData("Canopy Village: Part 2 - 1st Snail Defeated", Addresses.CanopyVillagePart21stSnail, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCanopyVillagePart2),
                new GenericLocationData("Canopy Village: Part 2 - 1st Bee Defeated", Addresses.CanopyVillagePart21stBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCanopyVillagePart2),
                new GenericLocationData("Canopy Village: Part 3 - 2nd Shooting Mushroom Defeated", Addresses.CanopyVillagePart32ndShootingMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCanopyVillagePart3),
                new GenericLocationData("Canopy Village: Part 3 - 3rd Shooting Mushroom Defeated", Addresses.CanopyVillagePart33rdShootingMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCanopyVillagePart3),
                new GenericLocationData("Canopy Village: Part 3 - 1st Spider Defeated", Addresses.CanopyVillagePart31stSpider, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCanopyVillagePart3),
                new GenericLocationData("Canopy Village: Part 3 - 2nd Bee Defeated", Addresses.CanopyVillagePart32ndBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCanopyVillagePart3),
                new GenericLocationData("Canopy Village: Part 3 - 5th Shooting Mushroom Defeated", Addresses.CanopyVillagePart35thShootingMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCanopyVillagePart3),
                new GenericLocationData("Canopy Village: Part 3 - 1st Bee Defeated", Addresses.CanopyVillagePart31stBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCanopyVillagePart3),
                new GenericLocationData("Canopy Village: Part 3 - 2nd Spider Defeated", Addresses.CanopyVillagePart32ndSpider, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCanopyVillagePart3),
                new GenericLocationData("Canopy Village: Part 3 - 3rd Spider Defeated", Addresses.CanopyVillagePart33rdSpider, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCanopyVillagePart3),
                new GenericLocationData("Canopy Village: Part 3 - 4th Shooting Mushroom Defeated", Addresses.CanopyVillagePart34thShootingMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCanopyVillagePart3),
                new GenericLocationData("Canopy Village: Part 3 - 3rd Bee Defeated", Addresses.CanopyVillagePart33rdBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCanopyVillagePart3),
                new GenericLocationData("Canopy Village: Part 3 - 1st Shooting Mushroom Defeated", Addresses.CanopyVillagePart31stShootingMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCanopyVillagePart3),
                new GenericLocationData("Canopy Village: Part 4 - 2nd Bee Defeated", Addresses.CanopyVillagePart42ndBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCanopyVillagePart4),
                new GenericLocationData("Canopy Village: Part 4 - 3rd Bee Defeated", Addresses.CanopyVillagePart43rdBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCanopyVillagePart4),
                new GenericLocationData("Canopy Village: Part 4 - 4th Bee Defeated", Addresses.CanopyVillagePart44thBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCanopyVillagePart4),
                new GenericLocationData("Canopy Village: Part 4 - 5th Bee Defeated", Addresses.CanopyVillagePart45thBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCanopyVillagePart4),
                new GenericLocationData("Canopy Village: Part 4 - 1st Shooting Mushroom Defeated", Addresses.CanopyVillagePart41stShootingMushroom, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCanopyVillagePart4),
                new GenericLocationData("Canopy Village: Part 4 - 1st Bee Defeated", Addresses.CanopyVillagePart41stBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCanopyVillagePart4),
                new GenericLocationData("Canopy Village: Part 5 - 3rd Shooting Dude Defeated", Addresses.CanopyVillagePart53rdShootingDude, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCanopyVillagePart5),
                new GenericLocationData("Canopy Village: Part 5 - 6th Bee Defeated", Addresses.CanopyVillagePart56thBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCanopyVillagePart5),
                new GenericLocationData("Canopy Village: Part 5 - 1st Dude Defeated", Addresses.CanopyVillagePart51stDude, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCanopyVillagePart5),
                new GenericLocationData("Canopy Village: Part 5 - 1st Bee Defeated", Addresses.CanopyVillagePart51stBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCanopyVillagePart5),
                new GenericLocationData("Canopy Village: Part 5 - 2nd Bee Defeated", Addresses.CanopyVillagePart52ndBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCanopyVillagePart5),
                new GenericLocationData("Canopy Village: Part 5 - 1st Snail Defeated", Addresses.CanopyVillagePart51stSnail, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCanopyVillagePart5),
                new GenericLocationData("Canopy Village: Part 5 - 1st Shooting Dude Defeated", Addresses.CanopyVillagePart51stShootingDude, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCanopyVillagePart5),
                new GenericLocationData("Canopy Village: Part 5 - 2nd Snail Defeated", Addresses.CanopyVillagePart52ndSnail, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCanopyVillagePart5),
                new GenericLocationData("Canopy Village: Part 5 - 3rd Bee Defeated", Addresses.CanopyVillagePart53rdBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCanopyVillagePart5),
                new GenericLocationData("Canopy Village: Part 5 - 4th Bee Defeated", Addresses.CanopyVillagePart54thBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCanopyVillagePart5),
                new GenericLocationData("Canopy Village: Part 5 - 5th Bee Defeated", Addresses.CanopyVillagePart55thBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCanopyVillagePart5),
                new GenericLocationData("Canopy Village: Part 5 - 2nd Shooting Dude Defeated", Addresses.CanopyVillagePart52ndShootingDude, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCanopyVillagePart5),
                new GenericLocationData("Canopy Village: Part 6 - 1st Shooting Dude Defeated", Addresses.CanopyVillagePart61stShootingDude, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCanopyVillagePart6),
                new GenericLocationData("Canopy Village: Part 6 - 2nd Shooting Dude Defeated", Addresses.CanopyVillagePart62ndShootingDude, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCanopyVillagePart6),
            };
        }

        private static List<GenericLocationData> GetHonchosLogmillEnemyData()
        {
            string levelId = "13";
            return new List<GenericLocationData>()
            {
                new GenericLocationData("Honcho's Logmill: Part 1 - 1st Soldier Defeated", Addresses.HonchosLogmillPart11stSoldier, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaHonchosLogmillPart1),
                new GenericLocationData("Honcho's Logmill: Part 1 - 1st Bee Defeated", Addresses.HonchosLogmillPart11stBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaHonchosLogmillPart1),
                new GenericLocationData("Honcho's Logmill: Part 1 - 2nd Shooting Dude Defeated", Addresses.HonchosLogmillPart12ndShootingDude, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaHonchosLogmillPart1),
                new GenericLocationData("Honcho's Logmill: Part 1 - 1st Shooting Dude Defeated", Addresses.HonchosLogmillPart11stShootingDude, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaHonchosLogmillPart1),
                new GenericLocationData("Honcho's Logmill: Part 1 - 2nd Bee Defeated", Addresses.HonchosLogmillPart12ndBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaHonchosLogmillPart1),
                new GenericLocationData("Honcho's Logmill: Part 1 - 3rd Bee Defeated", Addresses.HonchosLogmillPart13rdBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaHonchosLogmillPart1),
                new GenericLocationData("Honcho's Logmill: Part 2 - 1st Shooting Dude Defeated", Addresses.HonchosLogmillPart21stShootingDude, levelId, "128", LocationCheckType.Byte),
                new GenericLocationData("Honcho's Logmill: Part 2 - 1st Soldier Defeated", Addresses.HonchosLogmillPart21stSoldier, levelId, "128", LocationCheckType.Byte),
                new GenericLocationData("Honcho's Logmill: Part 2 - 2nd Shooting Dude Defeated", Addresses.HonchosLogmillPart22ndShootingDude, levelId, "128", LocationCheckType.Byte),
                new GenericLocationData("Honcho's Logmill: Part 2 - 3rd Shooting Dude Defeated", Addresses.HonchosLogmillPart23rdShootingDude, levelId, "128", LocationCheckType.Byte),
                new GenericLocationData("Honcho's Logmill: Part 2 - 2nd Bee Defeated", Addresses.HonchosLogmillPart22ndBee, levelId, "128", LocationCheckType.Byte),
                new GenericLocationData("Honcho's Logmill: Part 2 - 1st Bee Defeated", Addresses.HonchosLogmillPart21stBee, levelId, "128", LocationCheckType.Byte),
                new GenericLocationData("Honcho's Logmill: Part 2 - 1st Soldier (2) Defeated", Addresses.HonchosLogmillPart21stSoldier2, levelId, "128", LocationCheckType.Byte),
                new GenericLocationData("Honcho's Logmill: Part 3 - 1st Bee Defeated", Addresses.HonchosLogmillPart31stBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaHonchosLogmillPart3),
                new GenericLocationData("Honcho's Logmill: Part 3 - 2nd Shooting Dude Defeated", Addresses.HonchosLogmillPart32ndShootingDude, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaHonchosLogmillPart3),
                new GenericLocationData("Honcho's Logmill: Part 3 - 3rd Shooting Dude Defeated", Addresses.HonchosLogmillPart33rdShootingDude, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaHonchosLogmillPart3),
                new GenericLocationData("Honcho's Logmill: Part 3 - 1st Dude Defeated", Addresses.HonchosLogmillPart31stDude, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaHonchosLogmillPart3),
                new GenericLocationData("Honcho's Logmill: Part 3 - 1st Shooting Dude Defeated", Addresses.HonchosLogmillPart31stShootingDude, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaHonchosLogmillPart3),
                new GenericLocationData("Honcho's Logmill: Part 3 - 2nd Bee Defeated", Addresses.HonchosLogmillPart32ndBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaHonchosLogmillPart3),
            };
        }

        private static List<GenericLocationData> GetHonchosAirshipEnemyData()
        {
            string levelId = "15";
            return new List<GenericLocationData>()
            {
                new GenericLocationData("Honcho's Airship: Part 1 - 4th Dude Defeated", Addresses.HonchosAirshipPart14thDude, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaHonchosAirshipPart1),
                new GenericLocationData("Honcho's Airship: Part 1 - 1st Dude Defeated", Addresses.HonchosAirshipPart11stDude, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaHonchosAirshipPart1),
                new GenericLocationData("Honcho's Airship: Part 1 - 2nd Dude Defeated", Addresses.HonchosAirshipPart12ndDude, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaHonchosAirshipPart1),
                new GenericLocationData("Honcho's Airship: Part 1 - 3rd Dude Defeated", Addresses.HonchosAirshipPart13rdDude, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaHonchosAirshipPart1),
                new GenericLocationData("Honcho's Airship: Part 2 - 5th Bee Defeated", Addresses.HonchosAirshipPart25thBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaHonchosAirshipPart2),
                new GenericLocationData("Honcho's Airship: Part 2 - 3rd Small Dude Defeated", Addresses.HonchosAirshipPart23rdSmallDude, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaHonchosAirshipPart2),
                new GenericLocationData("Honcho's Airship: Part 2 - 2nd Small Dude Defeated", Addresses.HonchosAirshipPart22ndSmallDude, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaHonchosAirshipPart2),
                new GenericLocationData("Honcho's Airship: Part 2 - 6th Bee Defeated", Addresses.HonchosAirshipPart26thBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaHonchosAirshipPart2),
                new GenericLocationData("Honcho's Airship: Part 2 - 1st Bee Defeated", Addresses.HonchosAirshipPart21stBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaHonchosAirshipPart2),
                new GenericLocationData("Honcho's Airship: Part 2 - 3rd Bee Defeated", Addresses.HonchosAirshipPart23rdBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaHonchosAirshipPart2),
                new GenericLocationData("Honcho's Airship: Part 2 - 4th Bee Defeated", Addresses.HonchosAirshipPart24thBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaHonchosAirshipPart2),
                new GenericLocationData("Honcho's Airship: Part 2 - 2nd Bee Defeated", Addresses.HonchosAirshipPart22ndBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaHonchosAirshipPart2),
                new GenericLocationData("Honcho's Airship: Part 2 - 1st Small Dude Defeated", Addresses.HonchosAirshipPart21stSmallDude, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaHonchosAirshipPart2),
                new GenericLocationData("Honcho's Airship: Part 2 - Bomb 6 Defeated", Addresses.HonchosAirshipPart2Bomb6, levelId, "20743", LocationCheckType.UShort, area: Addresses.AreaHonchosAirshipPart2),
                new GenericLocationData("Honcho's Airship: Part 2 - Bomb 7 Defeated", Addresses.HonchosAirshipPart2Bomb7, levelId, "20743", LocationCheckType.UShort, area: Addresses.AreaHonchosAirshipPart2),
                new GenericLocationData("Honcho's Airship: Part 2 - Bomb 4 Defeated", Addresses.HonchosAirshipPart2Bomb4, levelId, "20743", LocationCheckType.UShort, area: Addresses.AreaHonchosAirshipPart2),
                new GenericLocationData("Honcho's Airship: Part 2 - Bomb 5 Defeated", Addresses.HonchosAirshipPart2Bomb5, levelId, "20743", LocationCheckType.UShort, area: Addresses.AreaHonchosAirshipPart2),
                new GenericLocationData("Honcho's Airship: Part 2 - Bomb 3 Defeated", Addresses.HonchosAirshipPart2Bomb3, levelId, "20743", LocationCheckType.UShort, area: Addresses.AreaHonchosAirshipPart2),
                new GenericLocationData("Honcho's Airship: Part 2 - Bomb 2 Defeated", Addresses.HonchosAirshipPart2Bomb2, levelId, "20743", LocationCheckType.UShort, area: Addresses.AreaHonchosAirshipPart2),
                new GenericLocationData("Honcho's Airship: Part 2 - Bomb 8 Defeated", Addresses.HonchosAirshipPart2Bomb8, levelId, "20743", LocationCheckType.UShort, area: Addresses.AreaHonchosAirshipPart2),
                new GenericLocationData("Honcho's Airship: Part 2 - Bomb 9 Defeated", Addresses.HonchosAirshipPart2Bomb9, levelId, "20743", LocationCheckType.UShort, area: Addresses.AreaHonchosAirshipPart2),
                new GenericLocationData("Honcho's Airship: Part 2 - Bomb 10 Defeated", Addresses.HonchosAirshipPart2Bomb10, levelId, "20743", LocationCheckType.UShort, area: Addresses.AreaHonchosAirshipPart2),
                new GenericLocationData("Honcho's Airship: Part 2 - Bomb 1 Defeated", Addresses.HonchosAirshipPart2Bomb1, levelId, "20743", LocationCheckType.UShort, area: Addresses.AreaHonchosAirshipPart2),
                new GenericLocationData("Honcho's Airship: Part 3 - Bomb 17 Defeated", Addresses.HonchosAirshipPart3Bomb17, levelId, "20743", LocationCheckType.UShort, area: Addresses.AreaHonchosAirshipPart3),
                new GenericLocationData("Honcho's Airship: Part 3 - Bomb 12 Defeated", Addresses.HonchosAirshipPart3Bomb12, levelId, "4359", LocationCheckType.UShort, area: Addresses.AreaHonchosAirshipPart3),
                new GenericLocationData("Honcho's Airship: Part 3 - Bomb 4 Defeated", Addresses.HonchosAirshipPart3Bomb4, levelId, "20743", LocationCheckType.UShort, area: Addresses.AreaHonchosAirshipPart3),
                new GenericLocationData("Honcho's Airship: Part 3 - Bomb 5 Defeated", Addresses.HonchosAirshipPart3Bomb5, levelId, "20743", LocationCheckType.UShort, area: Addresses.AreaHonchosAirshipPart3),
                new GenericLocationData("Honcho's Airship: Part 3 - Bomb 20 Defeated", Addresses.HonchosAirshipPart3Bomb20, levelId, "20743", LocationCheckType.UShort, area: Addresses.AreaHonchosAirshipPart3),
                new GenericLocationData("Honcho's Airship: Part 3 - Bomb 19 Defeated", Addresses.HonchosAirshipPart3Bomb19, levelId, "20743", LocationCheckType.UShort, area: Addresses.AreaHonchosAirshipPart3),
                new GenericLocationData("Honcho's Airship: Part 3 - Bomb 21 Defeated", Addresses.HonchosAirshipPart3Bomb21, levelId, "20743", LocationCheckType.UShort, area: Addresses.AreaHonchosAirshipPart3),
                new GenericLocationData("Honcho's Airship: Part 3 - Bomb 22 Defeated", Addresses.HonchosAirshipPart3Bomb22, levelId, "20743", LocationCheckType.UShort, area: Addresses.AreaHonchosAirshipPart3),
                new GenericLocationData("Honcho's Airship: Part 3 - Bomb 23 Defeated", Addresses.HonchosAirshipPart3Bomb23, levelId, "20743", LocationCheckType.UShort, area: Addresses.AreaHonchosAirshipPart3),
                new GenericLocationData("Honcho's Airship: Part 3 - Bomb 6 Defeated", Addresses.HonchosAirshipPart3Bomb6, levelId, "20743", LocationCheckType.UShort, area: Addresses.AreaHonchosAirshipPart3),
                new GenericLocationData("Honcho's Airship: Part 3 - Bomb 9 Defeated", Addresses.HonchosAirshipPart3Bomb9, levelId, "20743", LocationCheckType.UShort, area: Addresses.AreaHonchosAirshipPart3),
                new GenericLocationData("Honcho's Airship: Part 3 - Bomb 13 Defeated", Addresses.HonchosAirshipPart3Bomb13, levelId, "4359", LocationCheckType.UShort, area: Addresses.AreaHonchosAirshipPart3),
                new GenericLocationData("Honcho's Airship: Part 3 - Bomb 14 Defeated", Addresses.HonchosAirshipPart3Bomb14, levelId, "4359", LocationCheckType.UShort, area: Addresses.AreaHonchosAirshipPart3),
                new GenericLocationData("Honcho's Airship: Part 3 - Bomb 18 Defeated", Addresses.HonchosAirshipPart3Bomb18, levelId, "20743", LocationCheckType.UShort, area: Addresses.AreaHonchosAirshipPart3),
                new GenericLocationData("Honcho's Airship: Part 3 - Bomb 1 Defeated", Addresses.HonchosAirshipPart3Bomb1, levelId, "20743", LocationCheckType.UShort, area: Addresses.AreaHonchosAirshipPart3),
                new GenericLocationData("Honcho's Airship: Part 3 - Bomb 2 Defeated", Addresses.HonchosAirshipPart3Bomb2, levelId, "20743", LocationCheckType.UShort, area: Addresses.AreaHonchosAirshipPart3),
                new GenericLocationData("Honcho's Airship: Part 3 - Bomb 3 Defeated", Addresses.HonchosAirshipPart3Bomb3, levelId, "20743", LocationCheckType.UShort, area: Addresses.AreaHonchosAirshipPart3),
                new GenericLocationData("Honcho's Airship: Part 3 - Bomb 16 Defeated", Addresses.HonchosAirshipPart3Bomb16, levelId, "4359", LocationCheckType.UShort, area: Addresses.AreaHonchosAirshipPart3),
                new GenericLocationData("Honcho's Airship: Part 3 - Bomb 15 Defeated", Addresses.HonchosAirshipPart3Bomb15, levelId, "4359", LocationCheckType.UShort, area: Addresses.AreaHonchosAirshipPart3),
                new GenericLocationData("Honcho's Airship: Part 3 - Bomb 8 Defeated", Addresses.HonchosAirshipPart3Bomb8, levelId, "20743", LocationCheckType.UShort, area: Addresses.AreaHonchosAirshipPart3),
                new GenericLocationData("Honcho's Airship: Part 3 - Bomb 10 Defeated", Addresses.HonchosAirshipPart3Bomb10, levelId, "20743", LocationCheckType.UShort, area: Addresses.AreaHonchosAirshipPart3),
                new GenericLocationData("Honcho's Airship: Part 3 - Bomb 11 Defeated", Addresses.HonchosAirshipPart3Bomb11, levelId, "20743", LocationCheckType.UShort, area: Addresses.AreaHonchosAirshipPart3),
                new GenericLocationData("Honcho's Airship: Part 3 - Bomb 8 (2) Defeated", Addresses.HonchosAirshipPart3Bomb82, levelId, "20743", LocationCheckType.UShort, area: Addresses.AreaHonchosAirshipPart3),
            };
        }

        private static List<GenericLocationData> GetDragoonSkyfortEnemyData()
        {
            string levelId = "16";
            return new List<GenericLocationData>()
            {
                new GenericLocationData("Dragoon Skyfort: Part 1 - 3rd Dude Defeated", Addresses.DragoonSkyfortPart13rdDude, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaDragoonSkyfortPart1),
                new GenericLocationData("Dragoon Skyfort: Part 1 - 2nd Dude Defeated", Addresses.DragoonSkyfortPart12ndDude, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaDragoonSkyfortPart1),
                new GenericLocationData("Dragoon Skyfort: Part 1 - 1st Dude Defeated", Addresses.DragoonSkyfortPart11stDude, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaDragoonSkyfortPart1),
                new GenericLocationData("Dragoon Skyfort: Part 1 - 1st Shark Defeated", Addresses.DragoonSkyfortPart11stShark, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaDragoonSkyfortPart1),
                new GenericLocationData("Dragoon Skyfort: Part 1 - 4th Shark Defeated", Addresses.DragoonSkyfortPart14thShark, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaDragoonSkyfortPart1),
                new GenericLocationData("Dragoon Skyfort: Part 1 - 3rd Shark Defeated", Addresses.DragoonSkyfortPart13rdShark, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaDragoonSkyfortPart1),
                new GenericLocationData("Dragoon Skyfort: Part 1 - 2nd Shark Defeated", Addresses.DragoonSkyfortPart12ndShark, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaDragoonSkyfortPart1),
                new GenericLocationData("Dragoon Skyfort: Part 1 - Bomb 6 Defeated", Addresses.DragoonSkyfortPart1Bomb6, levelId, "20743", LocationCheckType.UShort, area: Addresses.AreaDragoonSkyfortPart1),
                new GenericLocationData("Dragoon Skyfort: Part 1 - Bomb 7 Defeated", Addresses.DragoonSkyfortPart1Bomb7, levelId, "20743", LocationCheckType.UShort, area: Addresses.AreaDragoonSkyfortPart1),
                new GenericLocationData("Dragoon Skyfort: Part 1 - Bomb 8 Defeated", Addresses.DragoonSkyfortPart1Bomb8, levelId, "20743", LocationCheckType.UShort, area: Addresses.AreaDragoonSkyfortPart1),
                new GenericLocationData("Dragoon Skyfort: Part 1 - Bomb 9 Defeated", Addresses.DragoonSkyfortPart1Bomb9, levelId, "20743", LocationCheckType.UShort, area: Addresses.AreaDragoonSkyfortPart1),
                new GenericLocationData("Dragoon Skyfort: Part 1 - Bomb 5 Defeated", Addresses.DragoonSkyfortPart1Bomb5, levelId, "20743", LocationCheckType.UShort, area: Addresses.AreaDragoonSkyfortPart1),
                new GenericLocationData("Dragoon Skyfort: Part 1 - Bomb 1 Defeated", Addresses.DragoonSkyfortPart1Bomb1, levelId, "20743", LocationCheckType.UShort, area: Addresses.AreaDragoonSkyfortPart1),
                new GenericLocationData("Dragoon Skyfort: Part 1 - Bomb 2 Defeated", Addresses.DragoonSkyfortPart1Bomb2, levelId, "20743", LocationCheckType.UShort, area: Addresses.AreaDragoonSkyfortPart1),
                new GenericLocationData("Dragoon Skyfort: Part 1 - Bomb 3 Defeated", Addresses.DragoonSkyfortPart1Bomb3, levelId, "20743", LocationCheckType.UShort, area: Addresses.AreaDragoonSkyfortPart1),
                new GenericLocationData("Dragoon Skyfort: Part 1 - Bomb 4 Defeated", Addresses.DragoonSkyfortPart1Bomb4, levelId, "20743", LocationCheckType.UShort, area: Addresses.AreaDragoonSkyfortPart1),
                new GenericLocationData("Dragoon Skyfort: Part 2 - 3rd Dude Defeated", Addresses.DragoonSkyfortPart23rdDude, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaDragoonSkyfortPart2),
                new GenericLocationData("Dragoon Skyfort: Part 2 - 2nd Bee Defeated", Addresses.DragoonSkyfortPart22ndBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaDragoonSkyfortPart2),
                new GenericLocationData("Dragoon Skyfort: Part 2 - 1st Bee Defeated", Addresses.DragoonSkyfortPart21stBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaDragoonSkyfortPart2),
                new GenericLocationData("Dragoon Skyfort: Part 2 - 3rd Bee Defeated", Addresses.DragoonSkyfortPart23rdBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaDragoonSkyfortPart2),
                new GenericLocationData("Dragoon Skyfort: Part 2 - 1st Dude Defeated", Addresses.DragoonSkyfortPart21stDude, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaDragoonSkyfortPart2),
                new GenericLocationData("Dragoon Skyfort: Part 2 - 2nd Dude Defeated", Addresses.DragoonSkyfortPart22ndDude, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaDragoonSkyfortPart2),
                new GenericLocationData("Dragoon Skyfort: Part 2 - Bomb 1 Defeated", Addresses.DragoonSkyfortPart2Bomb1, levelId, "4359", LocationCheckType.UShort, area: Addresses.AreaDragoonSkyfortPart2),
                new GenericLocationData("Dragoon Skyfort: Part 2 - Bomb 3 Defeated", Addresses.DragoonSkyfortPart2Bomb3, levelId, "20743", LocationCheckType.UShort, area: Addresses.AreaDragoonSkyfortPart2),
                new GenericLocationData("Dragoon Skyfort: Part 2 - Bomb 2 Defeated", Addresses.DragoonSkyfortPart2Bomb2, levelId, "20743", LocationCheckType.UShort, area: Addresses.AreaDragoonSkyfortPart2),
                new GenericLocationData("Dragoon Skyfort: Part 3 - 1st Shark Defeated", Addresses.DragoonSkyfortPart31stShark, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaDragoonSkyfortPart3),
                new GenericLocationData("Dragoon Skyfort: Part 3 - Bomb 13 Defeated", Addresses.DragoonSkyfortPart3Bomb13, levelId, "20743", LocationCheckType.UShort, area: Addresses.AreaDragoonSkyfortPart3),
                new GenericLocationData("Dragoon Skyfort: Part 3 - Bomb 19 Defeated", Addresses.DragoonSkyfortPart3Bomb19, levelId, "20743", LocationCheckType.UShort, area: Addresses.AreaDragoonSkyfortPart3),
                new GenericLocationData("Dragoon Skyfort: Part 3 - Bomb 14 Defeated", Addresses.DragoonSkyfortPart3Bomb14, levelId, "20743", LocationCheckType.UShort, area: Addresses.AreaDragoonSkyfortPart3),
                new GenericLocationData("Dragoon Skyfort: Part 3 - Bomb 15 Defeated", Addresses.DragoonSkyfortPart3Bomb15, levelId, "20743", LocationCheckType.UShort, area: Addresses.AreaDragoonSkyfortPart3),
                new GenericLocationData("Dragoon Skyfort: Part 3 - Bomb 5 Defeated", Addresses.DragoonSkyfortPart3Bomb5, levelId, "20743", LocationCheckType.UShort, area: Addresses.AreaDragoonSkyfortPart3),
                new GenericLocationData("Dragoon Skyfort: Part 3 - Bomb 4 Defeated", Addresses.DragoonSkyfortPart3Bomb4, levelId, "20743", LocationCheckType.UShort, area: Addresses.AreaDragoonSkyfortPart3),
                new GenericLocationData("Dragoon Skyfort: Part 3 - Bomb 16 Defeated", Addresses.DragoonSkyfortPart3Bomb16, levelId, "20743", LocationCheckType.UShort, area: Addresses.AreaDragoonSkyfortPart3),
                new GenericLocationData("Dragoon Skyfort: Part 3 - Bomb 3 Defeated", Addresses.DragoonSkyfortPart3Bomb3, levelId, "20743", LocationCheckType.UShort, area: Addresses.AreaDragoonSkyfortPart3),
                new GenericLocationData("Dragoon Skyfort: Part 3 - Bomb 17 Defeated", Addresses.DragoonSkyfortPart3Bomb17, levelId, "20743", LocationCheckType.UShort, area: Addresses.AreaDragoonSkyfortPart3),
                new GenericLocationData("Dragoon Skyfort: Part 3 - Bomb 6 Defeated", Addresses.DragoonSkyfortPart3Bomb6, levelId, "20743", LocationCheckType.UShort, area: Addresses.AreaDragoonSkyfortPart3),
                new GenericLocationData("Dragoon Skyfort: Part 3 - Bomb 8 Defeated", Addresses.DragoonSkyfortPart3Bomb8, levelId, "20743", LocationCheckType.UShort, area: Addresses.AreaDragoonSkyfortPart3),
                new GenericLocationData("Dragoon Skyfort: Part 3 - Bomb 9 Defeated", Addresses.DragoonSkyfortPart3Bomb9, levelId, "20743", LocationCheckType.UShort, area: Addresses.AreaDragoonSkyfortPart3),
                new GenericLocationData("Dragoon Skyfort: Part 3 - Bomb 7 Defeated", Addresses.DragoonSkyfortPart3Bomb7, levelId, "20743", LocationCheckType.UShort, area: Addresses.AreaDragoonSkyfortPart3),
                new GenericLocationData("Dragoon Skyfort: Part 3 - Bomb 18 Defeated", Addresses.DragoonSkyfortPart3Bomb18, levelId, "20743", LocationCheckType.UShort, area: Addresses.AreaDragoonSkyfortPart3),
                new GenericLocationData("Dragoon Skyfort: Part 3 - Bomb 2 Defeated", Addresses.DragoonSkyfortPart3Bomb2, levelId, "20743", LocationCheckType.UShort, area: Addresses.AreaDragoonSkyfortPart3),
                new GenericLocationData("Dragoon Skyfort: Part 3 - Bomb 10 Defeated", Addresses.DragoonSkyfortPart3Bomb10, levelId, "20743", LocationCheckType.UShort, area: Addresses.AreaDragoonSkyfortPart3),
                new GenericLocationData("Dragoon Skyfort: Part 3 - Bomb 11 Defeated", Addresses.DragoonSkyfortPart3Bomb11, levelId, "20743", LocationCheckType.UShort, area: Addresses.AreaDragoonSkyfortPart3),
                new GenericLocationData("Dragoon Skyfort: Part 3 - Bomb 12 Defeated", Addresses.DragoonSkyfortPart3Bomb12, levelId, "20743", LocationCheckType.UShort, area: Addresses.AreaDragoonSkyfortPart3),
                new GenericLocationData("Dragoon Skyfort: Part 3 - Bomb 20 Defeated", Addresses.DragoonSkyfortPart3Bomb20, levelId, "20743", LocationCheckType.UShort, area: Addresses.AreaDragoonSkyfortPart3),
                new GenericLocationData("Dragoon Skyfort: Part 3 - Bomb 21 Defeated", Addresses.DragoonSkyfortPart3Bomb21, levelId, "20743", LocationCheckType.UShort, area: Addresses.AreaDragoonSkyfortPart3),
                new GenericLocationData("Dragoon Skyfort: Part 3 - Bomb 1 Defeated", Addresses.DragoonSkyfortPart3Bomb1, levelId, "4359", LocationCheckType.UShort, area: Addresses.AreaDragoonSkyfortPart3),
                new GenericLocationData("Dragoon Skyfort: Part 4 - 1st Small Dude Defeated", Addresses.DragoonSkyfortPart41stSmallDude, levelId, "128", LocationCheckType.Byte),
                new GenericLocationData("Dragoon Skyfort: Part 4 - 1st Bee Defeated", Addresses.DragoonSkyfortPart41stBee, levelId, "128", LocationCheckType.Byte),
                new GenericLocationData("Dragoon Skyfort: Part 4 - 2nd Bee Defeated", Addresses.DragoonSkyfortPart42ndBee, levelId, "128", LocationCheckType.Byte),
                new GenericLocationData("Dragoon Skyfort: Part 4 - Bomb 13 Defeated", Addresses.DragoonSkyfortPart4Bomb13, levelId, "20743", LocationCheckType.UShort),
                new GenericLocationData("Dragoon Skyfort: Part 4 - Bomb 3 Defeated", Addresses.DragoonSkyfortPart4Bomb3, levelId, "4359", LocationCheckType.UShort),
                new GenericLocationData("Dragoon Skyfort: Part 4 - Bomb 12 Defeated", Addresses.DragoonSkyfortPart4Bomb12, levelId, "20743", LocationCheckType.UShort),
                new GenericLocationData("Dragoon Skyfort: Part 4 - Bomb 11 Defeated", Addresses.DragoonSkyfortPart4Bomb11, levelId, "20743", LocationCheckType.UShort),
                new GenericLocationData("Dragoon Skyfort: Part 4 - Bomb 10 Defeated", Addresses.DragoonSkyfortPart4Bomb10, levelId, "20743", LocationCheckType.UShort),
                new GenericLocationData("Dragoon Skyfort: Part 4 - Bomb 9 Defeated", Addresses.DragoonSkyfortPart4Bomb9, levelId, "20743", LocationCheckType.UShort),
                new GenericLocationData("Dragoon Skyfort: Part 4 - Bomb 8 Defeated", Addresses.DragoonSkyfortPart4Bomb8, levelId, "20743", LocationCheckType.UShort),
                new GenericLocationData("Dragoon Skyfort: Part 4 - Bomb 7 Defeated", Addresses.DragoonSkyfortPart4Bomb7, levelId, "20743", LocationCheckType.UShort),
                new GenericLocationData("Dragoon Skyfort: Part 4 - Bomb 4 Defeated", Addresses.DragoonSkyfortPart4Bomb4, levelId, "4359", LocationCheckType.UShort),
                new GenericLocationData("Dragoon Skyfort: Part 4 - Bomb 5 Defeated", Addresses.DragoonSkyfortPart4Bomb5, levelId, "4359", LocationCheckType.UShort),
                new GenericLocationData("Dragoon Skyfort: Part 4 - Bomb 6 Defeated", Addresses.DragoonSkyfortPart4Bomb6, levelId, "4359", LocationCheckType.UShort),
                new GenericLocationData("Dragoon Skyfort: Part 4 - Bomb 2 Defeated", Addresses.DragoonSkyfortPart4Bomb2, levelId, "20743", LocationCheckType.UShort),
                new GenericLocationData("Dragoon Skyfort: Part 4 - Bomb 1 Defeated", Addresses.DragoonSkyfortPart4Bomb1, levelId, "20743", LocationCheckType.UShort),
            };
        }

        private static List<GenericLocationData> GetCloudCitadelEnemyData()
        {
            string levelId = "17";
            return new List<GenericLocationData>()
            {
                new GenericLocationData("Cloud Citadel: Part 1 - 1st Small Dude Defeated", Addresses.BrankyWastesPart32ndDudeAndCloudCitadelPart11stSmallDude, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCloudCitadelPart1),
                new GenericLocationData("Cloud Citadel: Part 1 - 2nd Small Dude Defeated", Addresses.CloudCitadelPart12ndSmallDude, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCloudCitadelPart1),
                new GenericLocationData("Cloud Citadel: Part 1 - 1st Bee Defeated", Addresses.CloudCitadelPart11stBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCloudCitadelPart1),
                new GenericLocationData("Cloud Citadel: Part 1 - 3rd Small Dude Defeated", Addresses.CloudCitadelPart13rdSmallDude, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCloudCitadelPart1),
                new GenericLocationData("Cloud Citadel: Part 2 - 5th Small Dude Defeated", Addresses.CloudCitadelPart25thSmallDude, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCloudCitadelPart2),
                new GenericLocationData("Cloud Citadel: Part 2 - 3rd Bee Defeated", Addresses.CloudCitadelPart23rdBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCloudCitadelPart2),
                new GenericLocationData("Cloud Citadel: Part 2 - 2nd Whirlwind Defeated", Addresses.CloudCitadelPart22ndWhirlwind, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCloudCitadelPart2),
                new GenericLocationData("Cloud Citadel: Part 2 - 1st Whirlwind Defeated", Addresses.CloudCitadelPart21stWhirlwind, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCloudCitadelPart2),
                new GenericLocationData("Cloud Citadel: Part 2 - 3rd Whirlwind Defeated", Addresses.CloudCitadelPart23rdWhirlwind, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCloudCitadelPart2),
                new GenericLocationData("Cloud Citadel: Part 2 - 4th Small Dude Defeated", Addresses.CloudCitadelPart24thSmallDude, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCloudCitadelPart2),
                new GenericLocationData("Cloud Citadel: Part 2 - 3rd Small Dude Defeated", Addresses.CloudCitadelPart23rdSmallDude, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCloudCitadelPart2),
                new GenericLocationData("Cloud Citadel: Part 2 - 1st Bee Defeated", Addresses.CloudCitadelPart21stBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCloudCitadelPart2),
                new GenericLocationData("Cloud Citadel: Part 2 - 2nd Bee Defeated", Addresses.CloudCitadelPart22ndBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCloudCitadelPart2),
                new GenericLocationData("Cloud Citadel: Part 2 - 1st Small Dude Defeated", Addresses.CloudCitadelPart21stSmallDude, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCloudCitadelPart2),
                new GenericLocationData("Cloud Citadel: Part 2 - 6th Small Dude Defeated", Addresses.CloudCitadelPart26thSmallDude, levelId, "522", LocationCheckType.UShort, area: Addresses.AreaCloudCitadelPart2),
                new GenericLocationData("Cloud Citadel: Part 2 - Small Dude 2 Defeated", Addresses.CloudCitadelPart2SmallDude2, levelId, "522", LocationCheckType.UShort, area: Addresses.AreaCloudCitadelPart2),
                new GenericLocationData("Cloud Citadel: Part 3 - 2nd Whirlwind Defeated", Addresses.CloudCitadelPart32ndWhirlwind, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCloudCitadelPart3),
                new GenericLocationData("Cloud Citadel: Part 3 - 1st Whirlwind Defeated", Addresses.CloudCitadelPart31stWhirlwind, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCloudCitadelPart3),
                new GenericLocationData("Cloud Citadel: Part 3 - 1st Bee Defeated", Addresses.CloudCitadelPart31stBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCloudCitadelPart3),
                new GenericLocationData("Cloud Citadel: Part 3 - 2nd Bee Defeated", Addresses.CloudCitadelPart32ndBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCloudCitadelPart3),
                new GenericLocationData("Cloud Citadel: Part 3 - 3rd Bee Defeated", Addresses.CloudCitadelPart33rdBee, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCloudCitadelPart3),
                new GenericLocationData("Cloud Citadel: Part 3 - 1st Small Dude Defeated", Addresses.CloudCitadelPart31stSmallDude, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCloudCitadelPart3),
                new GenericLocationData("Cloud Citadel: Part 4 - 1st Small Dude Defeated", Addresses.CloudCitadelPart41stSmallDude, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCloudCitadelPart4),
                new GenericLocationData("Cloud Citadel: Part 4 - 3rd Whirlwind Defeated", Addresses.CloudCitadelPart43rdWhirlwind, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCloudCitadelPart4),
                new GenericLocationData("Cloud Citadel: Part 4 - 4th Whirlwind Defeated", Addresses.CloudCitadelPart44thWhirlwind, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCloudCitadelPart4),
                new GenericLocationData("Cloud Citadel: Part 4 - 1st Whirlwind Defeated", Addresses.CloudCitadelPart41stWhirlwind, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCloudCitadelPart4),
                new GenericLocationData("Cloud Citadel: Part 4 - 2nd Shark Defeated", Addresses.CloudCitadelPart42ndShark, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCloudCitadelPart4),
                new GenericLocationData("Cloud Citadel: Part 4 - 1st Shark Defeated", Addresses.CloudCitadelPart41stShark, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCloudCitadelPart4),
                new GenericLocationData("Cloud Citadel: Part 4 - 2nd Whirlwind Defeated", Addresses.CloudCitadelPart42ndWhirlwind, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaCloudCitadelPart4),
                new GenericLocationData("Cloud Citadel: Part 4 - Bomb 3 Defeated", Addresses.CloudCitadelPart4Bomb3, levelId, "20743", LocationCheckType.UShort, area: Addresses.AreaCloudCitadelPart4),
                new GenericLocationData("Cloud Citadel: Part 4 - Bomb 2 Defeated", Addresses.CloudCitadelPart4Bomb2, levelId, "20743", LocationCheckType.UShort, area: Addresses.AreaCloudCitadelPart4),
                new GenericLocationData("Cloud Citadel: Part 4 - Bomb 1 Defeated", Addresses.CloudCitadelPart4Bomb1, levelId, "20743", LocationCheckType.UShort, area: Addresses.AreaCloudCitadelPart4),
            };
        }

        private static List<GenericLocationData> GetEfreetPalaceEnemyData()
        {
            string levelId = "18";
            return new List<GenericLocationData>()
            {
                new GenericLocationData("Efreet Palace: Part 1 - Shark 1 Defeated", Addresses.EfreetPalacePart1Shark1, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaEfreetPalacePart1),
                new GenericLocationData("Efreet Palace: Part 1 - Shark 2 Defeated", Addresses.EfreetPalacePart1Shark2, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaEfreetPalacePart1),
                new GenericLocationData("Efreet Palace: Part 1 - Small Dude 1 Defeated", Addresses.EfreetPalacePart1SmallDude1, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaEfreetPalacePart1),
                new GenericLocationData("Efreet Palace: Part 1 - Small Dude 2 Defeated", Addresses.EfreetPalacePart1SmallDude2, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaEfreetPalacePart1),
                new GenericLocationData("Efreet Palace: Part 1 - Small Dude 3 Defeated", Addresses.EfreetPalacePart1SmallDude3, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaEfreetPalacePart1),
                new GenericLocationData("Efreet Palace: Part 1 - Small Dude 4 Defeated", Addresses.EfreetPalacePart1SmallDude4, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaEfreetPalacePart1),
                new GenericLocationData("Efreet Palace: Part 1 - Small Dude 5 Defeated", Addresses.EfreetPalacePart1SmallDude5, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaEfreetPalacePart1),
                new GenericLocationData("Efreet Palace: Part 1 - Small Dude 6 Defeated", Addresses.EfreetPalacePart1SmallDude6, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaEfreetPalacePart1),
                new GenericLocationData("Efreet Palace: Part 1 - Small Dude 7 Defeated", Addresses.EfreetPalacePart1SmallDude7, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaEfreetPalacePart1),
                new GenericLocationData("Efreet Palace: Part 1 - Small Dude 8 Defeated", Addresses.EfreetPalacePart1SmallDude8, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaEfreetPalacePart1),
                new GenericLocationData("Efreet Palace: Part 1 - Shark 3 Defeated", Addresses.EfreetPalacePart1Shark3, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaEfreetPalacePart1),
                new GenericLocationData("Efreet Palace: Part 3 - Small Dude 3 Defeated", Addresses.EfreetPalacePart3SmallDude3, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaEfreetPalacePart3),
                new GenericLocationData("Efreet Palace: Part 3 - Small Dude 4 Defeated", Addresses.EfreetPalacePart3SmallDude4, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaEfreetPalacePart3),
                new GenericLocationData("Efreet Palace: Part 3 - Bee 2 Defeated", Addresses.EfreetPalacePart3Bee2, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaEfreetPalacePart3),
                new GenericLocationData("Efreet Palace: Part 3 - Bee 3 Defeated", Addresses.EfreetPalacePart3Bee3, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaEfreetPalacePart3),
                new GenericLocationData("Efreet Palace: Part 3 - Shark 1 Defeated", Addresses.EfreetPalacePart3Shark1, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaEfreetPalacePart3),
                new GenericLocationData("Efreet Palace: Part 3 - Whirlwind Defeated", Addresses.EfreetPalacePart3Whirlwind, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaEfreetPalacePart3),
                new GenericLocationData("Efreet Palace: Part 3 - Small Dude 3 (2) Defeated", Addresses.EfreetPalacePart3SmallDude32, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaEfreetPalacePart3),
                new GenericLocationData("Efreet Palace: Part 3 - Bee 4 Defeated", Addresses.EfreetPalacePart3Bee4, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaEfreetPalacePart3),
                new GenericLocationData("Efreet Palace: Part 3 - Bee 5 Defeated", Addresses.EfreetPalacePart3Bee5, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaEfreetPalacePart3),
                new GenericLocationData("Efreet Palace: Part 3 - Small Dude 1 Defeated", Addresses.EfreetPalacePart3SmallDude1, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaEfreetPalacePart3),
                new GenericLocationData("Efreet Palace: Part 3 - Small Dude 2 Defeated", Addresses.EfreetPalacePart3SmallDude2, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaEfreetPalacePart3),
                new GenericLocationData("Efreet Palace: Part 3 - Shark 2 Defeated", Addresses.EfreetPalacePart3Shark2, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaEfreetPalacePart3),
                new GenericLocationData("Efreet Palace: Part 3 - Bee 1 Defeated", Addresses.EfreetPalacePart3Bee1, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaEfreetPalacePart3),
            };
        }

        private static List<GenericLocationData> GetFrozenCavernEnemyData()
        {
            string levelId = "19";
            return new List<GenericLocationData>()
            {
                new GenericLocationData("Frozen Cavern: Part 1 - Whirlwind 1 Defeated", Addresses.FrozenCavernPart1Whirlwind1, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaFrozenCavernPart1),
                new GenericLocationData("Frozen Cavern: Part 1 - Whirlwind 2 Defeated", Addresses.FrozenCavernPart1Whirlwind2, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaFrozenCavernPart1),
                new GenericLocationData("Frozen Cavern: Part 1 - Small Dude 2 Defeated", Addresses.FrozenCavernPart1SmallDude2, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaFrozenCavernPart1),
                new GenericLocationData("Frozen Cavern: Part 1 - Small Dude 1 Defeated", Addresses.FrozenCavernPart1SmallDude1, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaFrozenCavernPart1),
            };
        }

        private static List<GenericLocationData> GetStormTempleEnemyData()
        {
            string levelId = "20";
            return new List<GenericLocationData>()
            {
                new GenericLocationData("Storm Temple: Part 1 - Walking Dude Defeated", Addresses.StormTemplePart1WalkingDude, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaStormTemplePart1),
                new GenericLocationData("Storm Temple: Part 2 - Shadow 5 Defeated", Addresses.StormTemplePart2Shadow5, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaStormTemplePart2),
                new GenericLocationData("Storm Temple: Part 2 - Shadow 6 Defeated", Addresses.StormTemplePart2Shadow6, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaStormTemplePart2),
                new GenericLocationData("Storm Temple: Part 2 - Shadow 1 Defeated", Addresses.StormTemplePart2Shadow1, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaStormTemplePart2),
                new GenericLocationData("Storm Temple: Part 2 - Shadow 4 Defeated", Addresses.StormTemplePart2Shadow4, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaStormTemplePart2),
                new GenericLocationData("Storm Temple: Part 2 - Shadow 2 Defeated", Addresses.StormTemplePart2Shadow2, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaStormTemplePart2),
                new GenericLocationData("Storm Temple: Part 2 - Shadow 3 Defeated", Addresses.StormTemplePart2Shadow3, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaStormTemplePart2),
                new GenericLocationData("Storm Temple: Part 3 - Shadow 10 Defeated", Addresses.StormTemplePart3Shadow10, levelId, "192", LocationCheckType.Byte, area: Addresses.AreaStormTemplePart3),
                new GenericLocationData("Storm Temple: Part 3 - Shadow 11 Defeated", Addresses.StormTemplePart3Shadow11, levelId, "192", LocationCheckType.Byte, area: Addresses.AreaStormTemplePart3),
                new GenericLocationData("Storm Temple: Part 3 - Shadow 9 Defeated", Addresses.StormTemplePart3Shadow9, levelId, "192", LocationCheckType.Byte, area: Addresses.AreaStormTemplePart3),
                new GenericLocationData("Storm Temple: Part 3 - Shadow 8 Defeated", Addresses.StormTemplePart3Shadow8, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaStormTemplePart3),
                new GenericLocationData("Storm Temple: Part 3 - Shadow 1 Defeated", Addresses.StormTemplePart3Shadow1, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaStormTemplePart3),
                new GenericLocationData("Storm Temple: Part 3 - Shadow 2 Defeated", Addresses.StormTemplePart3Shadow2, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaStormTemplePart3),
                new GenericLocationData("Storm Temple: Part 3 - Shadow 3 Defeated", Addresses.StormTemplePart3Shadow3, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaStormTemplePart3),
                new GenericLocationData("Storm Temple: Part 3 - Shadow 6 Defeated", Addresses.StormTemplePart3Shadow6, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaStormTemplePart3),
                new GenericLocationData("Storm Temple: Part 3 - Shadow 7 Defeated", Addresses.StormTemplePart3Shadow7, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaStormTemplePart3),
                new GenericLocationData("Storm Temple: Part 3 - Shadow 4 Defeated", Addresses.StormTemplePart3Shadow4, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaStormTemplePart3),
                new GenericLocationData("Storm Temple: Part 3 - Shadow 5 Defeated", Addresses.StormTemplePart3Shadow5, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaStormTemplePart3),
                new GenericLocationData("Storm Temple: Part 3 - Small Dude 2 Defeated", Addresses.StormTemplePart3SmallDude2, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaStormTemplePart3),
                new GenericLocationData("Storm Temple: Part 3 - Small Dude 3 Defeated", Addresses.StormTemplePart3SmallDude3, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaStormTemplePart3),
                new GenericLocationData("Storm Temple: Part 3 - Small Dude 1 Defeated", Addresses.StormTemplePart3SmallDude1, levelId, "128", LocationCheckType.Byte, area: Addresses.AreaStormTemplePart3),
            };
        }
        private static List<GenericLocationData> GetTheWishingEngineData()
        {
            // "Game Completed" sits at index 0 in the apworld but is an event, so it has no ID and isn't a location here.
            // If real locations are ever added to this region, they start at index 1.
            return new List<GenericLocationData>();
        }
    }
}
