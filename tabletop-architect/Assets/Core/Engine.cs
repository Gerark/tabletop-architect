using UnityEngine;
using TTA.DataDefinition;

namespace TTA
{
    public class Engine
    {
        GameData _data;

        public Engine()
        {
            _buildData();
        }

        public GameData GetData()
        {
            return _data;
        }

        private void _buildData()
        {
            _data = new()
            {
                gameInfo = new GameInfo
                {
                    name = "Monopoly",
                    capsule = "test_capsule",
                    thumbnail = "test_thumbnail",
                    background = "test_background",
                    categories = new string[] { "Economic", "Negotiation" },
                    durations = new GameInfo.Duration[]
                    {
                        new GameInfo.Duration { name = "Default", min = 180, max = 180 }
                    },
                    playerCounts = new GameInfo.PlayerCount[]
                    {
                        new GameInfo.PlayerCount { name = "Default", min = 2, max = 8 }
                    },
                    age = 8
                },
                elements = new Element[]
                {
                    new() {
                        key = "die",
                        definition = new()
                        {
                            ownerRequired = false,
                            faces = new ElementFace[]
                            {
                                new()
                                {
                                    id = "1",
                                    value = 1
                                },
                                new()
                                {
                                    id = "2",
                                    value = 2
                                },
                                new()
                                {
                                    id = "3",
                                    value = 3
                                },
                                new()
                                {
                                    id = "4",
                                    value = 4
                                },
                                new()
                                {
                                    id = "5",
                                    value = 5
                                },
                                new()
                                {
                                    id = "6",
                                    value = 6
                                }
                            },
                            randomDistribution = RandomDistribution.Uniform
                        },
                    },
                    new() {
                        key = "pawn_race_car",
                        tags = new string[] { "pawn" },
                        definition = new()
                        {
                            ownerRequired = true,
                            randomDistribution = RandomDistribution.None
                        }
                    },
                    new() {
                        key = "pawn_top_hat",
                        tags = new string[] { "pawn" },
                        definition = new()
                        {
                            ownerRequired = true,
                            randomDistribution = RandomDistribution.None
                        }
                    },
                    new() {
                        key = "pawn_thimble",
                        tags = new string[] { "pawn" },
                        definition = new()
                        {
                            ownerRequired = true,
                            randomDistribution = RandomDistribution.None
                        }
                    },
                    new() {
                        key = "pawn_terrier",
                        tags = new string[] { "pawn" },
                        definition = new()
                        {
                            ownerRequired = true,
                            randomDistribution = RandomDistribution.None
                        }
                    },
                    new() {
                        key = "pawn_money_sack",
                        tags = new string[] { "pawn" },
                        definition = new()
                        {
                            ownerRequired = true,
                            randomDistribution = RandomDistribution.None
                        }
                    },
                    new() {
                        key = "pawn_cat",
                        tags = new string[] { "pawn" },
                        definition = new()
                        {
                            ownerRequired = true,
                            randomDistribution = RandomDistribution.None
                        }
                    },
                    new() {
                        key = "pawn_penguin",
                        tags = new string[] { "pawn" },
                        definition = new()
                        {
                            ownerRequired = true,
                            randomDistribution = RandomDistribution.None
                        }
                    },
                    new() {
                        key = "pawn_rubber_duck",
                        tags = new string[] { "pawn" },
                        definition = new()
                        {
                            ownerRequired = true,
                            randomDistribution = RandomDistribution.None
                        }
                    },

                    new() {
                        key = "property_card",
                        definition = new()
                        {
                            faces = new ElementFace[]
                            {
                                new()
                                {
                                    id = "Front",
                                    value = 1
                                },
                                new()
                                {
                                    id = "Back",
                                    value = 0
                                }
                            },
                            randomDistribution = RandomDistribution.None
                        }
                    },
                    new() {
                        key = "hotel_token",
                        definition = new()
                        {
                            faces = new ElementFace[]
                            {
                                new()
                                {
                                    id = "default",
                                    value = 1
                                }
                            },
                            ownerRequired = false,
                            randomDistribution = RandomDistribution.None
                        }
                    },
                    new() {
                        key = "board",
                        definition = new()
                        {
                            faces = new ElementFace[]
                            {
                                new()
                                {
                                    id = "default",
                                    value = 1
                                }
                            },
                            ownerRequired = false,
                            randomDistribution = RandomDistribution.None
                        }
                    },
                    new() {
                        key = "chance_advance_to_go_card",
                        tags = new string[] { "chance_card" },
                        definition = new()
                        {
                            faces = new ElementFace[]
                            {
                                new() { id = "Front" },
                                new() { id = "Back" }
                            },
                            randomDistribution = RandomDistribution.None
                        }
                    },
                    new() {
                        key = "chance_advance_to_illinois_avenue_card",
                        tags = new string[] { "chance_card" },
                        definition = new()
                        {
                            faces = new ElementFace[]
                            {
                                new() { id = "Front" },
                                new() { id = "Back" }
                            },
                            randomDistribution = RandomDistribution.None
                        }
                    },
                    new() {
                        key = "chance_advance_to_st_charles_place_card",
                        tags = new string[] { "chance_card" },
                        definition = new()
                        {
                            faces = new ElementFace[]
                            {
                                new() { id = "Front" },
                                new() { id = "Back" }
                            },
                            randomDistribution = RandomDistribution.None
                        }
                    },
                    new() {
                        key = "chance_advance_to_nearest_utility_card",
                        tags = new string[] { "chance_card" },
                        definition = new()
                        {
                            faces = new ElementFace[]
                            {
                                new() { id = "Front" },
                                new() { id = "Back" }
                            },
                            randomDistribution = RandomDistribution.None
                        }
                    },
                    new() {
                        key = "chance_advance_to_nearest_railroad_card_1",
                        tags = new string[] { "chance_card" },
                        definition = new()
                        {
                            faces = new ElementFace[]
                            {
                                new() { id = "Front" },
                                new() { id = "Back" }
                            },
                            randomDistribution = RandomDistribution.None
                        }
                    },
                    new() {
                        key = "chance_advance_to_nearest_railroad_card_2",
                        tags = new string[] { "chance_card" },
                        definition = new()
                        {
                            faces = new ElementFace[]
                            {
                                new() { id = "Front" },
                                new() { id = "Back" }
                            },
                            randomDistribution = RandomDistribution.None
                        }
                    },
                    new() {
                        key = "chance_bank_pays_dividend_card",
                        tags = new string[] { "chance_card" },
                        definition = new()
                        {
                            faces = new ElementFace[]
                            {
                                new() { id = "Front" },
                                new() { id = "Back" }
                            },
                            randomDistribution = RandomDistribution.None
                        }
                    },
                    new() {
                        key = "chance_get_out_of_jail_free_card",
                        tags = new string[] { "chance_card" },
                        definition = new()
                        {
                            ownerRequired = true,
                            faces = new ElementFace[]
                            {
                                new() { id = "Front" },
                                new() { id = "Back" }
                            },
                            randomDistribution = RandomDistribution.None
                        }
                    },
                    new() {
                        key = "chance_go_back_three_spaces_card",
                        tags = new string[] { "chance_card" },
                        definition = new()
                        {
                            faces = new ElementFace[]
                            {
                                new() { id = "Front" },
                                new() { id = "Back" }
                            },
                            randomDistribution = RandomDistribution.None
                        }
                    },
                    new() {
                        key = "chance_go_to_jail_card",
                        tags = new string[] { "chance_card" },
                        definition = new()
                        {
                            faces = new ElementFace[]
                            {
                                new() { id = "Front" },
                                new() { id = "Back" }
                            },
                            randomDistribution = RandomDistribution.None
                        }
                    },
                    new() {
                        key = "chance_general_repairs_card",
                        tags = new string[] { "chance_card" },
                        definition = new()
                        {
                            faces = new ElementFace[]
                            {
                                new() { id = "Front" },
                                new() { id = "Back" }
                            },
                            randomDistribution = RandomDistribution.None
                        }
                    },
                    new() {
                        key = "chance_pay_poor_tax_card",
                        tags = new string[] { "chance_card" },
                        definition = new()
                        {
                            faces = new ElementFace[]
                            {
                                new() { id = "Front" },
                                new() { id = "Back" }
                            },
                            randomDistribution = RandomDistribution.None
                        }
                    },
                    new() {
                        key = "chance_take_trip_to_reading_railroad_card",
                        tags = new string[] { "chance_card" },
                        definition = new()
                        {
                            faces = new ElementFace[]
                            {
                                new() { id = "Front" },
                                new() { id = "Back" }
                            },
                            randomDistribution = RandomDistribution.None
                        }
                    },
                    new() {
                        key = "chance_advance_to_boardwalk_card",
                        tags = new string[] { "chance_card" },
                        definition = new()
                        {
                            faces = new ElementFace[]
                            {
                                new() { id = "Front" },
                                new() { id = "Back" }
                            },
                            randomDistribution = RandomDistribution.None
                        }
                    },
                    new() {
                        key = "chance_elected_chairman_card",
                        tags = new string[] { "chance_card" },
                        definition = new()
                        {
                            faces = new ElementFace[]
                            {
                                new() { id = "Front" },
                                new() { id = "Back" }
                            },
                            randomDistribution = RandomDistribution.None
                        }
                    },
                    new() {
                        key = "chance_building_loan_matures_card",
                        tags = new string[] { "chance_card" },
                        definition = new()
                        {
                            faces = new ElementFace[]
                            {
                                new() { id = "Front" },
                                new() { id = "Back" }
                            },
                            randomDistribution = RandomDistribution.None
                        }
                    },

                    new() {
                        key = "community_chest_advance_to_go_card",
                        tags = new string[] { "community_card" },
                        definition = new()
                        {
                            faces = new ElementFace[]
                            {
                                new() { id = "Front" },
                                new() { id = "Back" }
                            },
                            randomDistribution = RandomDistribution.None
                        }
                    },
                    new() {
                        key = "community_chest_bank_error_in_your_favor_card",
                        tags = new string[] { "community_card" },
                        definition = new()
                        {
                            faces = new ElementFace[]
                            {
                                new() { id = "Front" },
                                new() { id = "Back" }
                            },
                            randomDistribution = RandomDistribution.None
                        }
                    },
                    new() {
                        key = "community_chest_doctors_fee_card",
                        tags = new string[] { "community_card" },
                        definition = new()
                        {
                            faces = new ElementFace[]
                            {
                                new() { id = "Front" },
                                new() { id = "Back" }
                            },
                            randomDistribution = RandomDistribution.None
                        }
                    },
                    new() {
                        key = "community_chest_from_sale_of_stock_card",
                        tags = new string[] { "community_card" },
                        definition = new()
                        {
                            faces = new ElementFace[]
                            {
                                new() { id = "Front" },
                                new() { id = "Back" }
                            },
                            randomDistribution = RandomDistribution.None
                        }
                    },
                    new() {
                        key = "community_chest_get_out_of_jail_free_card",
                        tags = new string[] { "community_card" },
                        definition = new()
                        {
                            ownerRequired = true,
                            faces = new ElementFace[]
                            {
                                new() { id = "Front" },
                                new() { id = "Back" }
                            },
                            randomDistribution = RandomDistribution.None
                        }
                    },
                    new() {
                        key = "community_chest_go_to_jail_card",
                        tags = new string[] { "community_card" },
                        definition = new()
                        {
                            faces = new ElementFace[]
                            {
                                new() { id = "Front" },
                                new() { id = "Back" }
                            },
                            randomDistribution = RandomDistribution.None
                        }
                    },
                    new() {
                        key = "community_chest_holiday_fund_matures_card",
                        tags = new string[] { "community_card" },
                        definition = new()
                        {
                            faces = new ElementFace[]
                            {
                                new() { id = "Front" },
                                new() { id = "Back" }
                            },
                            randomDistribution = RandomDistribution.None
                        }
                    },
                    new() {
                        key = "community_chest_income_tax_refund_card",
                        tags = new string[] { "community_card" },
                        definition = new()
                        {
                            faces = new ElementFace[]
                            {
                                new() { id = "Front" },
                                new() { id = "Back" }
                            },
                            randomDistribution = RandomDistribution.None
                        }
                    },
                    new() {
                        key = "community_chest_its_your_birthday_card",
                        tags = new string[] { "community_card" },
                        definition = new()
                        {
                            faces = new ElementFace[]
                            {
                                new() { id = "Front" },
                                new() { id = "Back" }
                            },
                            randomDistribution = RandomDistribution.None
                        }
                    },
                    new() {
                        key = "community_chest_life_insurance_matures_card",
                        tags = new string[] { "community_card" },
                        definition = new()
                        {
                            faces = new ElementFace[]
                            {
                                new() { id = "Front" },
                                new() { id = "Back" }
                            },
                            randomDistribution = RandomDistribution.None
                        }
                    },
                    new() {
                        key = "community_chest_hospital_fees_card",
                        tags = new string[] { "community_card" },
                        definition = new()
                        {
                            faces = new ElementFace[]
                            {
                                new() { id = "Front" },
                                new() { id = "Back" }
                            },
                            randomDistribution = RandomDistribution.None
                        }
                    },
                    new() {
                        key = "community_chest_school_fees_card",
                        tags = new string[] { "community_card" },
                        definition = new()
                        {
                            faces = new ElementFace[]
                            {
                                new() { id = "Front" },
                                new() { id = "Back" }
                            },
                            randomDistribution = RandomDistribution.None
                        }
                    },
                    new() {
                        key = "community_chest_receive_consultancy_fee_card",
                        tags = new string[] { "community_card" },
                        definition = new()
                        {
                            faces = new ElementFace[]
                            {
                                new() { id = "Front" },
                                new() { id = "Back" }
                            },
                            randomDistribution = RandomDistribution.None
                        }
                    },
                    new() {
                        key = "community_chest_street_repairs_card",
                        tags = new string[] { "community_card" },
                        definition = new()
                        {
                            faces = new ElementFace[]
                            {
                                new() { id = "Front" },
                                new() { id = "Back" }
                            },
                            randomDistribution = RandomDistribution.None
                        }
                    },
                    new() {
                        key = "community_chest_second_prize_beauty_contest_card",
                        tags = new string[] { "community_card" },
                        definition = new()
                        {
                            faces = new ElementFace[]
                            {
                                new() { id = "Front" },
                                new() { id = "Back" }
                            },
                            randomDistribution = RandomDistribution.None
                        }
                    },
                    new() {
                        key = "community_chest_inherit_card",
                        tags = new string[] { "community_card" },
                        definition = new()
                        {
                            faces = new ElementFace[]
                            {
                                new() { id = "Front" },
                                new() { id = "Back" }
                            },
                            randomDistribution = RandomDistribution.None
                        }
                    },
                },

                resources = new Resource[]
                {
                    new()
                    {
                        key = "money",
                        type = ResourceType.Number,
                        scope = ResourceScope.Player
                    }
                },

                areas = new Area[]
                {
                    new() { key = "road_1_group_1" },
                    new() { key = "road_2_group_1" },
                    new() { key = "road_1_group_2" },
                    new() { key = "road_2_group_2" },
                    new() { key = "road_3_group_2" },
                    new() { key = "road_1_group_3" },
                    new() { key = "road_2_group_3" },
                    new() { key = "road_3_group_3" },
                    new() { key = "road_1_group_4" },
                    new() { key = "road_2_group_4" },
                    new() { key = "road_3_group_4" },
                    new() { key = "road_1_group_5" },
                    new() { key = "road_2_group_5" },
                    new() { key = "road_3_group_5" },
                    new() { key = "road_1_group_6" },
                    new() { key = "road_2_group_6" },
                    new() { key = "road_3_group_6" },
                    new() { key = "road_1_group_7" },
                    new() { key = "road_2_group_7" },
                    new() { key = "road_3_group_7" },
                    new() { key = "road_1_group_8" },
                    new() { key = "road_2_group_8" },
                    new() { key = "go" },
                    new() { key = "go_to_jail" },
                    new() { key = "prison" },
                    new() { key = "just_visiting" },
                    new() { key = "free_parking" },
                    new() { key = "community_chest_1" },
                    new() { key = "community_chest_2" },
                    new() { key = "community_chest_3" },
                    new() { key = "station_1" },
                    new() { key = "station_2" },
                    new() { key = "station_3" },
                    new() { key = "station_4" },
                    new() { key = "chance_1" },
                    new() { key = "chance_2" },
                    new() { key = "chance_3" },
                    new() { key = "luxury_tax" },
                    new() { key = "income_tax" },
                    new() { key = "electric_company" },
                    new() { key = "water_works" },

                    new() { key = "chance_card_pile" },
                    new() { key = "community_chest_card_pile" },
                },

                topology = new()
                {
                    linearPaths = new LinearPath[]
                    {
                        new()
                        {
                            key = "board_path",
                            areas = new string[]
                            {
                                "go", "road_1_group_1", "community_chest_1", "road_2_group_1", "income_tax",
                                "station_1", "road_1_group_2", "chance_1", "road_2_group_2", "road_3_group_2",
                                "just_visiting", "road_1_group_3", "electric_company", "road_2_group_3", "road_3_group_3",
                                "station_2", "road_1_group_4", "community_chest_2", "road_2_group_4", "road_3_group_4",
                                "free_parking", "road_1_group_5", "chance_2", "road_2_group_5", "road_3_group_5",
                                "station_3", "road_1_group_6", "road_2_group_6", "water_works", "road_3_group_6",
                                "go_to_jail", "road_1_group_7", "road_2_group_7", "community_chest_3", "road_3_group_7",
                                "station_4", "chance_3", "road_1_group_8", "luxury_tax", "road_2_group_8"
                            }
                        }
                    }
                },

                rulesets = new Ruleset[]
                {
                    new()
                    {
                        key = "default_ruleset",
                        when = new()
                        {
                            Compare = new()
                            {
                                Left = Value.FromBinding("playersCount"),
                                Op = ComparisonOperator.Gt,
                                Right = Value.FromInt(1)
                            }
                        },
                        setup = new()
                        {
                            steps = new Operation[]
                            {
                                new()
                                {
                                    action = "PlaceElement",
                                    parameters = new[]
                                    {
                                        Param.New("Key", Value.FromString("board")),
                                        Param.New("Area", Value.FromString("table")),
                                        Param.New("Face", Value.FromString("front")),
                                    }
                                },
                                new()
                                {
                                    action = "PlaceElements",
                                    parameters = new[]
                                    {
                                        Param.New("Tag", Value.FromString("chance_card")),
                                        Param.New("Area", Value.FromString("chance_card_pile")),
                                        Param.New("Face", Value.FromString("back")),
                                    }
                                },
                                new()
                                {
                                    action = "PlaceElements",
                                    parameters = new[]
                                    {
                                        Param.New("Tag", Value.FromString("community_card")),
                                        Param.New("Area", Value.FromString("community_chest_card_pile")),
                                        Param.New("Face", Value.FromString("back")),
                                    }
                                },
                                new()
                                {
                                    repeat = new()
                                    {
                                        Collection = Value.FromBinding("Players"),
                                    },
                                    action = "SelectElement",
                                    parameters = new[]
                                    {
                                        Param.New("Target", Value.FromBinding("repeat.Current")),
                                        Param.New("FromTag", Value.FromString("pawn")),
                                        Param.New("AssignTo", Value.FromString("Pawn")),
                                    }
                                },
                                new()
                                {
                                    repeat = new()
                                    {
                                        Collection = Value.FromBinding("Players"),
                                    },
                                    action = "GiveResource",
                                    parameters = new[]
                                    {
                                        Param.New("Target", Value.FromBinding("repeat.Current")),
                                        Param.New("Resource", Value.FromString("money")),
                                        Param.New("Amount", Value.FromInt(1500)),
                                    }
                                },
                                new()
                                {
                                    repeat = new()
                                    {
                                        Collection = Value.FromBinding("Players"),
                                    },
                                    action = "PlaceElement",
                                    parameters = new[]
                                    {
                                        Param.New("Element", Value.FromBinding("repeat.Current.Pawn")),
                                        Param.New("Area", Value.FromString("go")),
                                    }
                                },
                                new()
                                {
                                    action = "DetermineFirstPlayer",
                                    parameters = new[]
                                    {
                                        Param.New("Participants", Value.FromBinding("Players")),
                                        Param.New("Tag", Value.FromString("turn_die")),
                                        Param.New("Method", Value.FromString("HighestTotal")),
                                    }
                                }
                            }
                        },
                        play = new()
                        {
                            startPhase = "roll_to_move",
                            phases = new Phase[]
                            {
                                new()
                                {
                                    key = "roll_to_move",
                                    participants = Value.FromBinding("CurrentPlayer"),
                                    availableActions = new PlayerAction[]
                                    {
                                        new()
                                        {
                                            action = "Roll",
                                            parameters = new[]
                                            {
                                                Param.New("Tag", Value.FromString("turn_die")),
                                                Param.New("NumberOfDice", Value.FromInt(2)),
                                            }
                                        }
                                    },
                                    events = new EventRule[]
                                    {
                                        new()
                                        {
                                            trigger = "OnRolled",
                                            nextPhase = "move_pawn",
                                            operations = new Operation[]
                                            {
                                                new()
                                                {
                                                    action = "StoreValue",
                                                    parameters = new[]
                                                    {
                                                        Param.New("Key", Value.FromString("moveAmount")),
                                                        Param.New("Value", Value.FromBinding("Event.Total"))
                                                    }
                                                }
                                            }
                                        }
                                    }
                                },
                                new()
                                {
                                    key = "move_pawn",
                                    events = new EventRule[]
                                    {
                                        new()
                                        {
                                            trigger = "OnPhaseStarted",
                                            operations = new Operation[]
                                            {
                                                new()
                                                {
                                                    action = "Move",
                                                    parameters = new[]
                                                    {
                                                        Param.New("Element", Value.FromBinding("CurrentPlayer.Pawn")),
                                                        Param.New("StepAmount", Value.FromBinding("moveAmount")),
                                                        Param.New("StepKind", Value.FromString("Forward")),
                                                        Param.New("Topology", Value.FromString("board_path")),
                                                    }
                                                }
                                            }
                                        },
                                        new()
                                        {
                                            trigger = "OnAreaPassed",
                                            when = new()
                                            {
                                                Compare = new()
                                                {
                                                    Left = Value.FromBinding("Event.Area"),
                                                    Op = ComparisonOperator.Eq,
                                                    Right = Value.FromString("go")
                                                }
                                            },
                                            operations = new Operation[]
                                            {
                                                new()
                                                {
                                                    action = "GiveResource",
                                                    parameters = new[]
                                                    {
                                                        Param.New("Target", Value.FromBinding("CurrentPlayer")),
                                                        Param.New("Resource", Value.FromString("money")),
                                                        Param.New("Amount", Value.FromInt(200)),
                                                    }
                                                },
                                                new()
                                                {
                                                    action = "GiveResource",
                                                    parameters = new[]
                                                    {
                                                        Param.New("Target", Value.FromBinding("CurrentPlayer")),
                                                        Param.New("Resource", Value.FromString("completed_laps")),
                                                        Param.New("Amount", Value.FromInt(1)),
                                                    }
                                                }
                                            }
                                        },
                                        new()
                                        {
                                            trigger = "OnMovementCompleted",
                                            nextPhase = "end_turn"
                                        }
                                    }
                                }
                            }
                        },
                        victoryRules = new VictoryRule[]
                        {
                            new()
                            {
                                repeat = new()
                                {
                                    Collection = Value.FromBinding("Players"),
                                },
                                condition = new()
                                {
                                    Compare = new()
                                    {
                                        Left = Value.FromBinding("repeat.Current.completed_laps"),
                                        Op = ComparisonOperator.Gte,
                                        Right = Value.FromInt(3)
                                    }
                                },
                                winner = Value.FromBinding("repeat.Current")
                            }
                        }
                    }
                },
            };
        }
    }
}
