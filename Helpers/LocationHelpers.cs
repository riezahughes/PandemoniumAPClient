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



            var regional_index = 0;

            var debug_levelCount = 0;
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
                        int locationId = (int)currentRegionBaseId + location_index;

                        List<ILocation> conditionalChoice = new List<ILocation>();
                        List<ILocation> treasureCheck = new List<ILocation>();
                        List<ILocation> finalStatement = new List<ILocation>();

                        if (loc.Name.ToLower().Contains("Percent Cleared"))
                        {
                            conditionalChoice.Add(new Location()
                            {
                                Id = -1,
                                Name = "Level ID",
                                Address = Addresses.LevelId,
                                CheckType = LocationCheckType.Byte,
                                CompareType = LocationCheckCompareType.Match,
                                CheckValue = loc.LevelId
                            });



                            treasureCheck.Add(new Location()
                            {
                                Id = -1,
                                Name = "Percentage Range",
                                Address = Addresses.TreasureCollectedPercent,
                                CheckType = LocationCheckType.Byte,
                                CompareType = LocationCheckCompareType.GreaterThan,
                                CheckValue = loc.Check
                            });

                            treasureCheck.Add(new Location()
                            {
                                Id = -1,
                                Name = "Percentage Range",
                                Address = Addresses.TreasureCollectedPercent,
                                CheckType = LocationCheckType.Byte,
                                CompareType = LocationCheckCompareType.GreaterThan,
                                CheckValue = loc.Check
                            });

                            CompositeLocation treasureLocationChoice = new CompositeLocation()
                            {
                                Name = "Character Clear",
                                Id = locationId,
                                CheckType = LocationCheckType.OR,
                                Conditions = treasureCheck
                            };


                            finalStatement.Add(treasureLocationChoice);
                            finalStatement.Add(conditionalChoice[0]);

                            CompositeLocation location = new CompositeLocation()
                            {
                                Name = "Character Clear",
                                Id = locationId,
                                CheckType = LocationCheckType.AND,
                                Conditions = finalStatement
                            };



                            location_index++;
                            continue;

                        }

                        if ((loc.Name.ToLower().Contains("Full Tilt".ToLower()) && options.ContainsKey("fullTiltLocations")) || (loc.Name.ToLower().Contains("Speed Greed".ToLower()) && options.ContainsKey("speedGreedLocations")))
                        {
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

                            CompositeLocation location = new CompositeLocation()
                            {
                                Name = "Bonus Level Clear",
                                Id = locationId,
                                CheckType = LocationCheckType.AND,
                                Conditions = conditionalChoice
                            };

                            location_index++;
                            continue;

                        }

                        if (loc.Name.ToLower().Contains("Cleared".ToLower()))
                        {
                            // track the character clears by level id and character
                            // check level id, the current character and if you're on the level cleared screen.

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

                            if (!options.ContainsKey("includeCharacterLocations"))
                            {
                                conditionalChoice.Add(new Location()
                                {
                                    Id = -1,
                                    Name = "Character Choice",
                                    Address = Addresses.CharacterChoice,
                                    CheckType = LocationCheckType.Byte,
                                    CompareType = LocationCheckCompareType.Match,
                                    CheckValue = loc.Check
                                });
                            }

                            CompositeLocation location = new CompositeLocation()
                            {
                                Name = "Character Clear",
                                Id = locationId,
                                CheckType = LocationCheckType.AND,
                                Conditions = conditionalChoice
                            };

                            location_index++;
                            continue;
                        }

                    }
                    {
#if DEBUG
                        //Console.WriteLine($"Could not add {loc.Name}, id: {loc.Id}");
#endif
                        location_index++;
                    }
                }

                regional_index++;
            }
            return locations;
        }

        // TODO: every location below uses Addresses.test as a placeholder. Swap in the real address, level id and check value
        // for each one. The order of every list must match the apworld's Locations.py exactly, because the ID is
        // base_id + (region index * region_offset) + position in the list.

        // the apworld appends a Fargus and a Nikki clear for each level clear, after the region's other locations
        private static List<GenericLocationData> GetCharacterClearData(string levelName)
        {
            string levelId = KeyToLevelNumber.ContainsKey(levelName) ? KeyToLevelNumber[levelName].ToString() : "0";

            return new List<GenericLocationData>()
            {
                new GenericLocationData($"Fargus Cleared: {levelName}", Addresses.CharacterChoice, levelId, "1"),
                new GenericLocationData($"Nikki Cleared: {levelName}", Addresses.CharacterChoice, levelId, "0"),
            };
        }

        // the apworld appends 1% to 100% for every non-boss level, last in the region
        private static List<GenericLocationData> GetPercentageClearData(string levelName)
        {
            List<GenericLocationData> percentageLocations = new List<GenericLocationData>();
            for (int percentage = 1; percentage <= 100; percentage++)
            {
                percentageLocations.Add(new GenericLocationData($"{percentage} Percent Cleared: {levelName}", Addresses.test, "", ""));
            }
            return percentageLocations;
        }

        private static List<GenericLocationData> GetSkullFortressData()
        {
            List<GenericLocationData> skullFortressLocations = new List<GenericLocationData>()
            {
                new GenericLocationData("Full Tilt: Skull Fortress", Addresses.FullTiltAndSpeedGreedCheck, "1", "61"),
                new GenericLocationData("Speed Greed: Skull Fortress", Addresses.FullTiltAndSpeedGreedCheck, "1", "60"),
                new GenericLocationData("Level Cleared: Skull Fortress", Addresses.LevelScreenState, "1", "1"),
            };
            skullFortressLocations.AddRange(GetCharacterClearData("Skull Fortress"));
            skullFortressLocations.AddRange(GetPercentageClearData("Skull Fortress"));
            return skullFortressLocations;
        }

        private static List<GenericLocationData> GetHollowStairwayData()
        {
            List<GenericLocationData> hollowStairwayLocations = new List<GenericLocationData>()
            {
                new GenericLocationData("Full Tilt: Hollow Stairway", Addresses.FullTiltAndSpeedGreedCheck, "2", "61"),
                new GenericLocationData("Speed Greed: Hollow Stairway", Addresses.FullTiltAndSpeedGreedCheck, "2", "60"),
                new GenericLocationData("Level Cleared: Hollow Stairway", Addresses.LevelScreenState, "2", "1"),
            };
            hollowStairwayLocations.AddRange(GetCharacterClearData("Hollow Stairway"));
            hollowStairwayLocations.AddRange(GetPercentageClearData("Hollow Stairway"));
            return hollowStairwayLocations;
        }

        private static List<GenericLocationData> GetDungeonTowerData()
        {
            List<GenericLocationData> dungeonTowerLocations = new List<GenericLocationData>()
            {
                new GenericLocationData("Full Tilt: Dungeon Tower", Addresses.FullTiltAndSpeedGreedCheck, "3", "61"),
                new GenericLocationData("Speed Greed: Dungeon Tower", Addresses.FullTiltAndSpeedGreedCheck, "3", "60"),
                new GenericLocationData("Level Cleared: Dungeon Tower", Addresses.LevelScreenState, "3", "1"),
            };
            dungeonTowerLocations.AddRange(GetCharacterClearData("Dungeon Tower"));
            dungeonTowerLocations.AddRange(GetPercentageClearData("Dungeon Tower"));
            return dungeonTowerLocations;
        }

        private static List<GenericLocationData> GetLostCavesData()
        {
            List<GenericLocationData> lostCavesLocations = new List<GenericLocationData>()
            {
                new GenericLocationData("Full Tilt: Lost Caves", Addresses.FullTiltAndSpeedGreedCheck, "4", "61"),
                new GenericLocationData("Speed Greed: Lost Caves", Addresses.FullTiltAndSpeedGreedCheck, "4", "60"),
                new GenericLocationData("Level Cleared: Lost Caves", Addresses.LevelScreenState, "4", "1"),
            };
            lostCavesLocations.AddRange(GetCharacterClearData("Lost Caves"));
            lostCavesLocations.AddRange(GetPercentageClearData("Lost Caves"));
            return lostCavesLocations;
        }

        private static List<GenericLocationData> GetFungusGrottoData()
        {
            List<GenericLocationData> fungusGrottoLocations = new List<GenericLocationData>()
            {
                new GenericLocationData("Full Tilt: Fungus Grotto", Addresses.FullTiltAndSpeedGreedCheck, "5", "61"),
                new GenericLocationData("Speed Greed: Fungus Grotto", Addresses.FullTiltAndSpeedGreedCheck, "5", "60"),
                new GenericLocationData("Level Cleared: Fungus Grotto", Addresses.LevelScreenState, "5", "1"),
                new GenericLocationData("Frog Gate: Fungus Grotto", Addresses.test, "", ""),
            };
            fungusGrottoLocations.AddRange(GetCharacterClearData("Fungus Grotto"));
            fungusGrottoLocations.AddRange(GetPercentageClearData("Fungus Grotto"));
            return fungusGrottoLocations;
        }

        private static List<GenericLocationData> GetAcidPoolsData()
        {
            List<GenericLocationData> acidPoolsLocations = new List<GenericLocationData>()
            {
                new GenericLocationData("Full Tilt: Acid Pools", Addresses.FullTiltAndSpeedGreedCheck, "6", "61"),
                new GenericLocationData("Speed Greed: Acid Pools", Addresses.FullTiltAndSpeedGreedCheck, "6", "60"),
                new GenericLocationData("Level Cleared: Acid Pools", Addresses.LevelScreenState, "6", "1"),
                new GenericLocationData("Frog Gate: Acid Pools", Addresses.test, "", ""),
            };
            acidPoolsLocations.AddRange(GetCharacterClearData("Acid Pools"));
            acidPoolsLocations.AddRange(GetPercentageClearData("Acid Pools"));
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
                new GenericLocationData("Full Tilt: Burning Desert", Addresses.FullTiltAndSpeedGreedCheck, "8", "61"),
                new GenericLocationData("Speed Greed: Burning Desert", Addresses.FullTiltAndSpeedGreedCheck, "8", "60"),
                new GenericLocationData("Level Cleared: Burning Desert", Addresses.LevelScreenState, "8", "1"),
            };
            burningDesertLocations.AddRange(GetCharacterClearData("Burning Desert"));
            burningDesertLocations.AddRange(GetPercentageClearData("Burning Desert"));
            return burningDesertLocations;
        }

        private static List<GenericLocationData> GetBrankyWastesData()
        {
            List<GenericLocationData> brankyWastesLocations = new List<GenericLocationData>()
            {
                new GenericLocationData("Full Tilt: Branky Wastes", Addresses.FullTiltAndSpeedGreedCheck, "9", "61"),
                new GenericLocationData("Speed Greed: Branky Wastes", Addresses.FullTiltAndSpeedGreedCheck, "9", "60"),
                new GenericLocationData("Level Cleared: Branky Wastes", Addresses.LevelScreenState, "9", "1"),
                new GenericLocationData("Rhino Gate: Branky Wastes", Addresses.test, "", ""),
            };
            brankyWastesLocations.AddRange(GetCharacterClearData("Branky Wastes"));
            brankyWastesLocations.AddRange(GetPercentageClearData("Branky Wastes"));
            return brankyWastesLocations;
        }

        private static List<GenericLocationData> GetSpiderForestData()
        {
            List<GenericLocationData> spiderForestLocations = new List<GenericLocationData>()
            {
                new GenericLocationData("Full Tilt: Spider Forest", Addresses.FullTiltAndSpeedGreedCheck, "10", "61"),
                new GenericLocationData("Speed Greed: Spider Forest", Addresses.FullTiltAndSpeedGreedCheck, "10", "60"),
                new GenericLocationData("Level Cleared: Spider Forest", Addresses.LevelScreenState, "10", "1"),
            };
            spiderForestLocations.AddRange(GetCharacterClearData("Spider Forest"));
            spiderForestLocations.AddRange(GetPercentageClearData("Spider Forest"));
            return spiderForestLocations;
        }

        private static List<GenericLocationData> GetCanopyVillageData()
        {
            List<GenericLocationData> canopyVillageLocations = new List<GenericLocationData>()
            {
                new GenericLocationData("Full Tilt: Canopy Village", Addresses.FullTiltAndSpeedGreedCheck, "11", "61"),
                new GenericLocationData("Speed Greed: Canopy Village", Addresses.FullTiltAndSpeedGreedCheck, "11", "60"),
                new GenericLocationData("Level Cleared: Canopy Village", Addresses.LevelScreenState, "11", "1"),
                new GenericLocationData("Turtle Gate: Canopy Village", Addresses.test, "", ""),
            };
            canopyVillageLocations.AddRange(GetCharacterClearData("Canopy Village"));
            canopyVillageLocations.AddRange(GetPercentageClearData("Canopy Village"));
            return canopyVillageLocations;
        }

        private static List<GenericLocationData> GetSoldierBarracksData()
        {
            List<GenericLocationData> soldierBarracksLocations = new List<GenericLocationData>()
            {
                new GenericLocationData("Full Tilt: Soldier Barracks", Addresses.FullTiltAndSpeedGreedCheck, "12", "61"),
                new GenericLocationData("Speed Greed: Soldier Barracks", Addresses.FullTiltAndSpeedGreedCheck, "12", "60"),
                new GenericLocationData("Level Cleared: Soldier Barracks", Addresses.LevelScreenState, "12", "1"),
            };
            soldierBarracksLocations.AddRange(GetCharacterClearData("Soldier Barracks"));
            soldierBarracksLocations.AddRange(GetPercentageClearData("Soldier Barracks"));
            return soldierBarracksLocations;
        }

        private static List<GenericLocationData> GetHonchosLogmillData()
        {
            List<GenericLocationData> honchosLogmillLocations = new List<GenericLocationData>()
            {
                new GenericLocationData("Full Tilt: Honcho's Logmill", Addresses.FullTiltAndSpeedGreedCheck, "13", "61"),
                new GenericLocationData("Speed Greed: Honcho's Logmill", Addresses.FullTiltAndSpeedGreedCheck, "13", "60"),
                new GenericLocationData("Level Cleared: Honcho's Logmill", Addresses.LevelScreenState, "13", "1"),
            };
            honchosLogmillLocations.AddRange(GetCharacterClearData("Honcho's Logmill"));
            honchosLogmillLocations.AddRange(GetPercentageClearData("Honcho's Logmill"));
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
                new GenericLocationData("Full Tilt: Honcho's Airship", Addresses.FullTiltAndSpeedGreedCheck, "15", "61"),
                new GenericLocationData("Speed Greed: Honcho's Airship", Addresses.FullTiltAndSpeedGreedCheck, "15", "60"),
                new GenericLocationData("Level Cleared: Honcho's Airship", Addresses.LevelScreenState, "15", "1"),
                new GenericLocationData("Dragon Gate: Honcho's Airship", Addresses.test, "", ""),
            };
            honchosAirshipLocations.AddRange(GetCharacterClearData("Honcho's Airship"));
            honchosAirshipLocations.AddRange(GetPercentageClearData("Honcho's Airship"));
            return honchosAirshipLocations;
        }

        private static List<GenericLocationData> GetDragoonSkyfortData()
        {
            List<GenericLocationData> dragoonSkyfortLocations = new List<GenericLocationData>()
            {
                new GenericLocationData("Full Tilt: Dragoon Skyfort", Addresses.FullTiltAndSpeedGreedCheck, "16", "61"),
                new GenericLocationData("Speed Greed: Dragoon Skyfort", Addresses.FullTiltAndSpeedGreedCheck, "16", "60"),
                new GenericLocationData("Level Cleared: Dragoon Skyfort", Addresses.LevelScreenState, "16", "1"),
            };
            dragoonSkyfortLocations.AddRange(GetCharacterClearData("Dragoon Skyfort"));
            dragoonSkyfortLocations.AddRange(GetPercentageClearData("Dragoon Skyfort"));
            return dragoonSkyfortLocations;
        }

        private static List<GenericLocationData> GetCloudCitadelData()
        {
            List<GenericLocationData> cloudCitadelLocations = new List<GenericLocationData>()
            {
                new GenericLocationData("Full Tilt: Cloud Citadel", Addresses.FullTiltAndSpeedGreedCheck, "17", "61"),
                new GenericLocationData("Speed Greed: Cloud Citadel", Addresses.FullTiltAndSpeedGreedCheck, "17", "60"),
                new GenericLocationData("Level Cleared: Cloud Citadel", Addresses.LevelScreenState, "17", "1"),
            };
            cloudCitadelLocations.AddRange(GetCharacterClearData("Cloud Citadel"));
            cloudCitadelLocations.AddRange(GetPercentageClearData("Cloud Citadel"));
            return cloudCitadelLocations;
        }

        private static List<GenericLocationData> GetEfreetPalaceData()
        {
            List<GenericLocationData> efreetPalaceLocations = new List<GenericLocationData>()
            {
                new GenericLocationData("Full Tilt: Efreet Palace", Addresses.FullTiltAndSpeedGreedCheck, "18", "61"),
                new GenericLocationData("Speed Greed: Efreet Palace", Addresses.FullTiltAndSpeedGreedCheck, "18", "60"),
                new GenericLocationData("Level Cleared: Efreet Palace", Addresses.LevelScreenState, "18", "1"),
            };
            efreetPalaceLocations.AddRange(GetCharacterClearData("Efreet Palace"));
            efreetPalaceLocations.AddRange(GetPercentageClearData("Efreet Palace"));
            return efreetPalaceLocations;
        }

        private static List<GenericLocationData> GetFrozenCavernData()
        {
            List<GenericLocationData> frozenCavernLocations = new List<GenericLocationData>()
            {
                new GenericLocationData("Full Tilt: Frozen Cavern", Addresses.FullTiltAndSpeedGreedCheck, "19", "61"),
                new GenericLocationData("Speed Greed: Frozen Cavern", Addresses.FullTiltAndSpeedGreedCheck, "19", "60"),
                new GenericLocationData("Level Cleared: Frozen Cavern", Addresses.LevelScreenState, "19", "1"),
            };
            frozenCavernLocations.AddRange(GetCharacterClearData("Frozen Cavern"));
            frozenCavernLocations.AddRange(GetPercentageClearData("Frozen Cavern"));
            return frozenCavernLocations;
        }

        private static List<GenericLocationData> GetStormTempleData()
        {
            List<GenericLocationData> stormTempleLocations = new List<GenericLocationData>()
            {
                new GenericLocationData("Full Tilt: Storm Temple", Addresses.FullTiltAndSpeedGreedCheck, "20", "61"),
                new GenericLocationData("Speed Greed: Storm Temple", Addresses.FullTiltAndSpeedGreedCheck, "20", "60"),
                new GenericLocationData("Level Cleared: Storm Temple", Addresses.LevelScreenState, "20", "1"),
            };
            stormTempleLocations.AddRange(GetCharacterClearData("Storm Temple"));
            stormTempleLocations.AddRange(GetPercentageClearData("Storm Temple"));
            return stormTempleLocations;
        }

        private static List<GenericLocationData> GetTheWishingEngineData()
        {
            // "Game Completed" sits at index 0 in the apworld but is an event, so it has no ID and isn't a location here.
            // If real locations are ever added to this region, they start at index 1.
            return new List<GenericLocationData>();
        }
    }
}