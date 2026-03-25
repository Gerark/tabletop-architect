using System;
using System.Collections.Generic;
using UnityEngine;
using TTA.Core;
using TTA.Presenter;

namespace TTA.Game
{
    public static class Sample
    {
        private const string BoardMaterialKey = "dummy.board";
        private const string DieMaterialKey = "dummy.die";
        private const string PawnMaterialKey = "dummy.pawn";
        private const string CardMaterialKey = "dummy.card";
        private const string TokenMaterialKey = "dummy.token";

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
                    CreateArea(
                        "table",
                        CreateAreaPresentation(
                            new Vector3(-1.6f, 0f, 0f),
                            new Vector3(3.2f, 0f, -0.35f),
                            Vector3.up,
                            true,
                            new Vector3(11f, 8f, 1.5f)))
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
                                    OperationParameter.Create("Target", Value.FromBinding("repeat.Current.Pawn")),
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
                                                    OperationParameter.Create("Target", Value.FromBinding("CurrentPlayer.Pawn")),
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

        public static PresentationResourceManifest CreateMonopolyResources()
        {
            return new PresentationResourceManifest
            {
                entries = new[]
                {
                    CreateTextureResource("boardTexture", "textures/board.jpg"),
                }
            };
        }

        private static ElementDefinition CreateDieDefinition()
        {
            return new ElementDefinition
            {
                key = "die",
                tags = new[] { "turn_die" },
                amount = 2,
                randomDistribution = RandomDistribution.Uniform,
                presentation = CreateElementPresentation(
                    PresentationPrimitiveKind.Cube,
                    new Vector3(0.7f, 0.7f, 0.7f),
                    new Color(0.96f, 0.94f, 0.87f),
                    new Vector3(0f, 0f, -0.1f),
                    materialKey: DieMaterialKey),
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
                randomDistribution = RandomDistribution.None,
                presentation = CreatePawnPresentation(key)
            };
        }

        private static ElementDefinition CreateSingleFaceElement(string key, string faceId, int numericValue, ElementPresentationDefinition presentation = null)
        {
            return new ElementDefinition
            {
                key = key,
                randomDistribution = RandomDistribution.None,
                presentation = presentation ?? CreateDefaultTokenPresentation(),
                faces = new[]
                {
                    CreateFace(faceId, numericValue, true)
                }
            };
        }

        private static ElementDefinition CreateTwoFaceElement(string key, string frontId, int frontValue, string backId, int backValue, ElementPresentationDefinition presentation = null)
        {
            return new ElementDefinition
            {
                key = key,
                randomDistribution = RandomDistribution.None,
                presentation = presentation ?? CreateCardPresentation(new Color(0.92f, 0.82f, 0.58f)),
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
                ownedAreas.Add(CreateArea(boardTrack[index], CreateBoardTrackAreaPresentation(index)));

            ownedAreas.Add(CreateArea(
                "chance_card_pile",
                CreateAreaPresentation(
                    new Vector3(1.1f, 1.1f, 0f),
                    new Vector3(0f, 0.03f, -0.01f),
                    Vector3.zero,
                    true,
                    new Vector3(0.9f, 1.1f, 0.6f))));
            ownedAreas.Add(CreateArea(
                "community_chest_card_pile",
                CreateAreaPresentation(
                    new Vector3(-1.1f, -1.1f, 0f),
                    new Vector3(0f, 0.03f, -0.01f),
                    Vector3.zero,
                    true,
                    new Vector3(0.9f, 1.1f, 0.6f))));

            return new ElementDefinition
            {
                key = "board",
                randomDistribution = RandomDistribution.None,
                presentation = CreateElementPresentation(
                    PresentationPrimitiveKind.Cube,
                    new Vector3(5.6f, 0.2f, 5.6f),
                    new Color(0.75f, 0.66f, 0.46f),
                    Vector3.zero,
                    materialKey: BoardMaterialKey),
                faces = new[]
                {
                    CreateFace("default", 1, true, "boardTexture")
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
                presentation = string.Equals(tag, "chance_card", StringComparison.Ordinal)
                    ? CreateCardPresentation(new Color(0.93f, 0.78f, 0.58f))
                    : CreateCardPresentation(new Color(0.75f, 0.87f, 0.95f)),
                faces = new[]
                {
                    CreateFace("Front", 0, true),
                    CreateFace("Back", 0)
                }
            };
        }

        private static AreaDefinition CreateArea(string key, AreaPresentationDefinition presentation = null)
        {
            return new AreaDefinition
            {
                key = key,
                presentation = presentation ?? new AreaPresentationDefinition()
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

        private static ElementFaceDefinition CreateFace(string id, int numericValue, bool isDefault = false, string textureKey = null)
        {
            return new ElementFaceDefinition
            {
                id = id,
                numericValue = numericValue,
                isDefault = isDefault,
                presentation = CreateFacePresentation(textureKey)
            };
        }

        private static ElementFacePresentationDefinition CreateFacePresentation(string textureKey)
        {
            return new ElementFacePresentationDefinition
            {
                textureKey = textureKey ?? string.Empty
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

        private static ElementPresentationDefinition CreateElementPresentation(
            PresentationPrimitiveKind primitive,
            Vector3 scale,
            Color color,
            Vector3 localOffset,
            Vector3 localEulerAngles = default,
            string materialKey = null)
        {
            return new ElementPresentationDefinition
            {
                primitive = primitive,
                localScale = scale,
                localOffset = localOffset,
                localEulerAngles = localEulerAngles,
                materialKey = materialKey ?? string.Empty,
                color = color
            };
        }

        private static AreaPresentationDefinition CreateAreaPresentation(
            Vector3 anchor,
            Vector3 itemOffset,
            Vector3 normal = default,
            bool hasBoxCollider = false,
            Vector3 boxColliderSize = default,
            Vector3 boxColliderCenter = default)
        {
            return new AreaPresentationDefinition
            {
                anchor = anchor,
                itemOffset = itemOffset,
                normal = normal == Vector3.zero ? Vector3.forward : normal.normalized,
                hasBoxCollider = hasBoxCollider,
                boxColliderSize = boxColliderSize == Vector3.zero ? Vector3.one : boxColliderSize,
                boxColliderCenter = boxColliderCenter
            };
        }

        private static ElementPresentationDefinition CreatePawnPresentation(string key)
        {
            Color color = key switch
            {
                "pawn_race_car" => new Color(0.84f, 0.2f, 0.18f),
                "pawn_top_hat" => new Color(0.16f, 0.18f, 0.22f),
                "pawn_thimble" => new Color(0.7f, 0.72f, 0.76f),
                "pawn_terrier" => new Color(0.58f, 0.44f, 0.31f),
                "pawn_money_sack" => new Color(0.73f, 0.61f, 0.24f),
                "pawn_cat" => new Color(0.91f, 0.53f, 0.22f),
                "pawn_penguin" => new Color(0.22f, 0.42f, 0.75f),
                "pawn_rubber_duck" => new Color(0.95f, 0.84f, 0.18f),
                _ => new Color(0.62f, 0.62f, 0.62f)
            };

            return CreateElementPresentation(
                PresentationPrimitiveKind.Capsule,
                new Vector3(0.19f, 0.3f, 0.19f),
                color,
                Vector3.zero,
                materialKey: PawnMaterialKey);
        }

        private static ElementPresentationDefinition CreateCardPresentation(Color color)
        {
            return CreateElementPresentation(
                PresentationPrimitiveKind.Cube,
                new Vector3(0.55f, 0.8f, 0.08f),
                color,
                new Vector3(0f, 0f, -0.08f),
                materialKey: CardMaterialKey);
        }

        private static ElementPresentationDefinition CreateDefaultTokenPresentation()
        {
            return CreateElementPresentation(
                PresentationPrimitiveKind.Cube,
                new Vector3(0.35f, 0.35f, 0.2f),
                new Color(0.82f, 0.72f, 0.5f),
                new Vector3(0f, 0f, -0.08f),
                materialKey: TokenMaterialKey);
        }

        private static AreaPresentationDefinition CreateBoardTrackAreaPresentation(int index)
        {
            float edge = 0.4125f;
            const float stepCount = 10f;
            float x;
            float y;
            float w = 0.13f;
            float h = 0.08f;
            float offsetPos = 0.025f;

            if (index % 10 == 0)
            {
                edge += offsetPos;
                x = index switch
                {
                    0 => edge,
                    10 => -edge,
                    20 => -edge,
                    30 => edge,
                    _ => 0f
                };
                y = index switch
                {
                    0 => -edge,
                    10 => -edge,
                    20 => edge,
                    30 => edge,
                    _ => 0f
                };
                return CreateAreaPresentation(
                    new Vector3(x, 0f, y),
                    Vector3.zero,
                    Vector3.up,
                    true,
                    new Vector3(0.13f, 0.13f, 0.1f));
            }

            if (index < 10)
            {
                float t = index / stepCount;
                x = Mathf.Lerp(edge, -edge, t);
                y = -edge - offsetPos;
            }
            else if (index < 20)
            {
                float t = (index - 10) / stepCount;
                x = -edge - offsetPos;
                y = Mathf.Lerp(-edge, edge, t);
                (w, h) = (h, w);
            }
            else if (index < 30)
            {
                float t = (index - 20) / stepCount;
                x = Mathf.Lerp(-edge, edge, t);
                y = edge + offsetPos;
            }
            else
            {
                float t = (index - 30) / stepCount;
                x = edge + offsetPos;
                y = Mathf.Lerp(edge, -edge, t);
                (w, h) = (h, w);
            }

            return CreateAreaPresentation(
                new Vector3(x, 0f, y),
                Vector3.zero,
                Vector3.up,
                true,
                new Vector3(w, h, 0.1f));
        }

        private static PresentationResourceEntry CreateTextureResource(string key, string path)
        {
            return new PresentationResourceEntry
            {
                key = key,
                kind = PresentationResourceKind.Texture,
                path = path
            };
        }
    }
}
