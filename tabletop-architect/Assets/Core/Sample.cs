using System;
using System.Collections.Generic;

namespace TTA
{
    public static class Sample
    {
        public static GameDefinition CreateMonopolyDefinition()
        {
            string[] boardTrack =
            {
                "go", "road_1_group_1", "community_chest_1", "road_2_group_1", "income_tax",
                "station_1", "road_1_group_2", "chance_1", "road_2_group_2", "road_3_group_2",
                "just_visiting", "road_1_group_3", "electric_company", "road_2_group_3", "road_3_group_3",
                "station_2", "road_1_group_4", "community_chest_2", "road_2_group_4", "road_3_group_4",
                "free_parking", "road_1_group_5", "chance_2", "road_2_group_5", "road_3_group_5",
                "station_3", "road_1_group_6", "road_2_group_6", "water_works", "road_3_group_6",
                "go_to_jail", "road_1_group_7", "road_2_group_7", "community_chest_3", "road_3_group_7",
                "station_4", "chance_3", "road_1_group_8", "luxury_tax", "road_2_group_8"
            };

            List<ElementDefinition> elements = new()
            {
                CreateDieDefinition(),
                CreatePawnDefinition("pawn_race_car"),
                CreatePawnDefinition("pawn_top_hat"),
                CreatePawnDefinition("pawn_thimble"),
                CreatePawnDefinition("pawn_terrier"),
                CreatePawnDefinition("pawn_money_sack"),
                CreatePawnDefinition("pawn_cat"),
                CreatePawnDefinition("pawn_penguin"),
                CreatePawnDefinition("pawn_rubber_duck"),
                CreateTwoFaceElement("property_card", "Front", 1, "Back", 0),
                CreateSingleFaceElement("hotel_token", "default", 1),
                CreateBoardDefinition(boardTrack)
            };

            AddChanceCardDefinitions(elements);
            AddCommunityChestCardDefinitions(elements);
            GameData definition = new()
            {
                gameInfo = new GameInfo
                {
                    name = "Monopoly",
                    capsule = "test_capsule",
                    thumbnail = "test_thumbnail",
                    background = "test_background",
                    categories = new[] { "Economic", "Negotiation" },
                    durations = new[]
                    {
                        new GameInfo.Duration
                        {
                            name = "Default",
                            min = 180,
                            max = 180
                        }
                    },
                    playerCounts = new[]
                    {
                        new GameInfo.PlayerCount
                        {
                            name = "Default",
                            min = 2,
                            max = 8
                        }
                    },
                    age = 8
                },
                properties = new[]
                {
                    CreateProperty("money", PropertyScope.Player, ValueKind.Int),
                    CreateProperty("completed_laps", PropertyScope.Player, ValueKind.Int),
                    CreateProperty("Pawn", PropertyScope.Player, ValueKind.ElementId, Value.FromElementId(RuntimeIds.InvalidId))
                },
                globalAreas = new[]
                {
                    CreateArea("table")
                },
                elements = elements.ToArray(),
                rulesets = new[]
                {
                    new RulesetDefinition
                    {
                        key = "default_ruleset",
                        setup = new SetupDefinition
                        {
                            steps = new[]
                            {
                                CreateOperation(
                                    OperationCode.TakeFromBox,
                                    OperationParameter.Create("Key", Value.FromString("board")),
                                    OperationParameter.Create("Area", Value.FromString("table"))),
                                CreateOperation(
                                    OperationCode.TakeFromBox,
                                    OperationParameter.Create("Key", Value.FromString("die")),
                                    OperationParameter.Create("Amount", Value.FromInt(2)),
                                    OperationParameter.Create("Area", Value.FromString("table"))),
                                CreateOperation(
                                    OperationCode.TakeFromBox,
                                    OperationParameter.Create("Tag", Value.FromString("chance_card")),
                                    OperationParameter.Create("Area", Value.FromString("chance_card_pile")),
                                    OperationParameter.Create("AreaOwner", Value.FromString("board"))),
                                CreateOperation(
                                    OperationCode.SetFace,
                                    OperationParameter.Create("Tag", Value.FromString("chance_card")),
                                    OperationParameter.Create("Face", Value.FromString("Back"))),
                                CreateOperation(
                                    OperationCode.TakeFromBox,
                                    OperationParameter.Create("Tag", Value.FromString("community_card")),
                                    OperationParameter.Create("Area", Value.FromString("community_chest_card_pile")),
                                    OperationParameter.Create("AreaOwner", Value.FromString("board"))),
                                CreateOperation(
                                    OperationCode.SetFace,
                                    OperationParameter.Create("Tag", Value.FromString("community_card")),
                                    OperationParameter.Create("Face", Value.FromString("Back"))),
                                CreateRepeatedOperation(
                                    Value.FromBinding("Players"),
                                    OperationCode.SelectElement,
                                    OperationParameter.Create("Target", Value.FromBinding("repeat.Current")),
                                    OperationParameter.Create("FromTag", Value.FromString("pawn")),
                                    OperationParameter.Create("AssignTo", Value.FromString("Pawn"))),
                                CreateRepeatedOperation(
                                    Value.FromBinding("Players"),
                                    OperationCode.WriteProperty,
                                    OperationParameter.Create("Target", Value.FromBinding("repeat.Current")),
                                    OperationParameter.Create("Key", Value.FromString("money")),
                                    OperationParameter.Create("Value", Value.FromInt(1500)),
                                    OperationParameter.Create("Mode", Value.FromString("Add"))),
                                CreateRepeatedOperation(
                                    Value.FromBinding("Players"),
                                    OperationCode.PlaceElement,
                                    OperationParameter.Create("Element", Value.FromBinding("repeat.Current.Pawn")),
                                    OperationParameter.Create("Area", Value.FromString("go")),
                                    OperationParameter.Create("AreaOwner", Value.FromString("board"))),
                                CreateOperation(
                                    OperationCode.DetermineFirstPlayer,
                                    OperationParameter.Create("Participants", Value.FromBinding("Players")),
                                    OperationParameter.Create("Tag", Value.FromString("turn_die")),
                                    OperationParameter.Create("Method", Value.FromString("HighestTotal")))
                            }
                        },
                        play = new PlayDefinition
                        {
                            startPhase = "roll_to_move",
                            phases = new[]
                            {
                                new PhaseDefinition
                                {
                                    key = "roll_to_move",
                                    participants = Value.FromBinding("CurrentPlayer"),
                                    availableActions = new[]
                                    {
                                        new PlayerActionDefinition
                                        {
                                            key = "roll_turn_dice",
                                            operations = new[]
                                            {
                                                CreateOperation(
                                                    OperationCode.Roll,
                                                    OperationParameter.Create("Tag", Value.FromString("turn_die")))
                                            }
                                        }
                                    },
                                    events = new[]
                                    {
                                        new EventRuleDefinition
                                        {
                                            trigger = "OnRolled",
                                            nextPhase = "move_pawn",
                                            operations = new[]
                                            {
                                                CreateOperation(
                                                    OperationCode.WriteTemp,
                                                    OperationParameter.Create("Scope", Value.FromString("Turn")),
                                                    OperationParameter.Create("Key", Value.FromString("moveAmount")),
                                                    OperationParameter.Create("Value", Value.FromBinding("Event.Total")))
                                            }
                                        }
                                    }
                                },
                                new PhaseDefinition
                                {
                                    key = "move_pawn",
                                    participants = Value.FromBinding("CurrentPlayer"),
                                    events = new[]
                                    {
                                        new EventRuleDefinition
                                        {
                                            trigger = "OnPhaseStarted",
                                            operations = new[]
                                            {
                                                CreateOperation(
                                                    OperationCode.Move,
                                                    OperationParameter.Create("Element", Value.FromBinding("CurrentPlayer.Pawn")),
                                                    OperationParameter.Create("StepAmount", Value.FromBinding("Temps.moveAmount")),
                                                    OperationParameter.Create("StepKind", Value.FromString("Forward")),
                                                    OperationParameter.Create("Topology", Value.FromString("board_path")))
                                            }
                                        },
                                        new EventRuleDefinition
                                        {
                                            trigger = "OnAreaPassed",
                                            when = CreateComparisonCondition(
                                                Value.FromBinding("Event.Area"),
                                                ComparisonOperator.Eq,
                                                Value.FromString("go")),
                                            operations = new[]
                                            {
                                                CreateOperation(
                                                    OperationCode.WriteProperty,
                                                    OperationParameter.Create("Target", Value.FromBinding("CurrentPlayer")),
                                                    OperationParameter.Create("Key", Value.FromString("money")),
                                                    OperationParameter.Create("Value", Value.FromInt(200)),
                                                    OperationParameter.Create("Mode", Value.FromString("Add"))),
                                                CreateOperation(
                                                    OperationCode.WriteProperty,
                                                    OperationParameter.Create("Target", Value.FromBinding("CurrentPlayer")),
                                                    OperationParameter.Create("Key", Value.FromString("completed_laps")),
                                                    OperationParameter.Create("Value", Value.FromInt(1)),
                                                    OperationParameter.Create("Mode", Value.FromString("Add")))
                                            }
                                        },
                                        new EventRuleDefinition
                                        {
                                            trigger = "OnMovementCompleted",
                                            nextPhase = "end_turn"
                                        }
                                    }
                                },
                                new PhaseDefinition
                                {
                                    key = "end_turn",
                                    participants = Value.FromBinding("CurrentPlayer"),
                                    events = new[]
                                    {
                                        new EventRuleDefinition
                                        {
                                            trigger = "OnPhaseStarted",
                                            nextPhase = "roll_to_move",
                                            operations = new[]
                                            {
                                                CreateOperation(OperationCode.AdvanceTurn)
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        victoryRules = new[]
                        {
                            new VictoryRuleDefinition
                            {
                                repeat = new RepeatDefinition
                                {
                                    collection = Value.FromBinding("Players")
                                },
                                condition = CreateComparisonCondition(
                                    Value.FromBinding("repeat.Current.completed_laps"),
                                    ComparisonOperator.Gte,
                                    Value.FromInt(3)),
                                winner = Value.FromBinding("repeat.Current")
                            }
                        }
                    }
                }
            };

            return definition;
        }

        private static ElementDefinition CreateDieDefinition()
        {
            return new ElementDefinition
            {
                key = "die",
                tags = new[] { "turn_die" },
                amount = 2,
                randomDistribution = RandomDistribution.Uniform,
                faces = new[]
                {
                    CreateFace("1", 1, true),
                    CreateFace("2", 2),
                    CreateFace("3", 3),
                    CreateFace("4", 4),
                    CreateFace("5", 5),
                    CreateFace("6", 6)
                }
            };
        }

        private static ElementDefinition CreatePawnDefinition(string key)
        {
            return new ElementDefinition
            {
                key = key,
                tags = new[] { "pawn" },
                ownerRequired = true,
                randomDistribution = RandomDistribution.None
            };
        }

        private static ElementDefinition CreateSingleFaceElement(string key, string faceId, int numericValue)
        {
            return new ElementDefinition
            {
                key = key,
                randomDistribution = RandomDistribution.None,
                faces = new[]
                {
                    CreateFace(faceId, numericValue, true)
                }
            };
        }

        private static ElementDefinition CreateTwoFaceElement(string key, string frontId, int frontValue, string backId, int backValue)
        {
            return new ElementDefinition
            {
                key = key,
                randomDistribution = RandomDistribution.None,
                faces = new[]
                {
                    CreateFace(frontId, frontValue, true),
                    CreateFace(backId, backValue)
                }
            };
        }

        private static ElementDefinition CreateBoardDefinition(string[] boardTrack)
        {
            List<AreaDefinition> ownedAreas = new();
            for (int index = 0; index < boardTrack.Length; index++)
                ownedAreas.Add(CreateArea(boardTrack[index]));

            ownedAreas.Add(CreateArea("chance_card_pile"));
            ownedAreas.Add(CreateArea("community_chest_card_pile"));

            return new ElementDefinition
            {
                key = "board",
                randomDistribution = RandomDistribution.None,
                faces = new[]
                {
                    CreateFace("default", 1, true)
                },
                ownedAreas = ownedAreas.ToArray(),
                topologies = new[]
                {
                    new TopologyDefinition
                    {
                        key = "board_path",
                        linearPaths = new[]
                        {
                            new LinearPathDefinition
                            {
                                key = "main_track",
                                areas = boardTrack,
                                loop = true
                            }
                        }
                    }
                }
            };
        }

        private static void AddChanceCardDefinitions(List<ElementDefinition> elements)
        {
            string[] regularCards =
            {
                "chance_advance_to_go_card",
                "chance_advance_to_illinois_avenue_card",
                "chance_advance_to_st_charles_place_card",
                "chance_advance_to_nearest_utility_card",
                "chance_advance_to_nearest_railroad_card_1",
                "chance_advance_to_nearest_railroad_card_2",
                "chance_bank_pays_dividend_card",
                "chance_go_back_three_spaces_card",
                "chance_go_to_jail_card",
                "chance_general_repairs_card",
                "chance_pay_poor_tax_card",
                "chance_take_trip_to_reading_railroad_card",
                "chance_advance_to_boardwalk_card",
                "chance_elected_chairman_card",
                "chance_building_loan_matures_card",
                "chance_get_out_of_jail_free_card"
            };

            for (int index = 0; index < regularCards.Length; index++)
                elements.Add(CreateCardDefinition(regularCards[index], "chance_card"));
        }

        private static void AddCommunityChestCardDefinitions(List<ElementDefinition> elements)
        {
            string[] regularCards =
            {
                "community_chest_advance_to_go_card",
                "community_chest_bank_error_in_your_favor_card",
                "community_chest_doctors_fee_card",
                "community_chest_from_sale_of_stock_card",
                "community_chest_go_to_jail_card",
                "community_chest_holiday_fund_matures_card",
                "community_chest_income_tax_refund_card",
                "community_chest_its_your_birthday_card",
                "community_chest_life_insurance_matures_card",
                "community_chest_hospital_fees_card",
                "community_chest_school_fees_card",
                "community_chest_receive_consultancy_fee_card",
                "community_chest_street_repairs_card",
                "community_chest_second_prize_beauty_contest_card",
                "community_chest_inherit_card"
            };

            for (int index = 0; index < regularCards.Length; index++)
                elements.Add(CreateCardDefinition(regularCards[index], "community_card"));

            elements.Add(CreateCardDefinition("community_chest_get_out_of_jail_free_card", "community_card", ownerRequired: true));
        }

        private static ElementDefinition CreateCardDefinition(string key, string tag, bool ownerRequired = false)
        {
            return new ElementDefinition
            {
                key = key,
                tags = new[] { tag },
                ownerRequired = ownerRequired,
                randomDistribution = RandomDistribution.None,
                faces = new[]
                {
                    CreateFace("Front", 0, true),
                    CreateFace("Back", 0)
                }
            };
        }

        private static AreaDefinition CreateArea(string key)
        {
            return new AreaDefinition
            {
                key = key
            };
        }

        private static PropertyDefinition CreateProperty(string key, PropertyScope scope, ValueKind valueKind, Value defaultValue = null)
        {
            return new PropertyDefinition
            {
                key = key,
                scope = scope,
                valueKind = valueKind,
                defaultValue = defaultValue ?? Value.Null()
            };
        }

        private static ElementFaceDefinition CreateFace(string id, int numericValue, bool isDefault = false)
        {
            return new ElementFaceDefinition
            {
                id = id,
                numericValue = numericValue,
                isDefault = isDefault
            };
        }

        private static OperationDefinition CreateOperation(OperationCode code, params OperationParameter[] parameters)
        {
            return new OperationDefinition
            {
                code = code,
                parameters = parameters ?? Array.Empty<OperationParameter>()
            };
        }

        private static OperationDefinition CreateRepeatedOperation(Value collection, OperationCode code, params OperationParameter[] parameters)
        {
            return new OperationDefinition
            {
                code = code,
                repeat = new RepeatDefinition
                {
                    collection = collection
                },
                parameters = parameters ?? Array.Empty<OperationParameter>()
            };
        }

        private static Condition CreateComparisonCondition(Value left, ComparisonOperator op, Value right)
        {
            return new Condition
            {
                compare = new ComparisonCondition
                {
                    left = left,
                    op = op,
                    right = right
                }
            };
        }
    }
}
