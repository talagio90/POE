using System;
using System.Collections.Generic;

namespace PoeFixer;

public class MemoriesofzanaPatch : IPatch
{
    public string Extension => "*.otc|*.ao|*.aoc";

    // QUAN TRỌNG: Viết hoa chữ cái đầu để khớp với Index của Game (Metadata/Chest/...)
    public string[] FilesToPatch => [
        "metadata/chests/memoriesofzana/zanainfluencemapchest.otc",
        "metadata/chests/memoriesofzana/zanainfluencemapchest.ao",
        "metadata/chests/memoriesofzana/zanainfluencemapchest.aoc",
    ];

    public string[] DirectoriesToPatch => [];

    public bool ShouldPatch(Dictionary<string, bool> bools, Dictionary<string, float> floats)
    {
        bools.TryGetValue("MemoriesofzanaChestEnabled", out bool enabled);
        return enabled;
    }

    public string? PatchFile(string text)
    {
        // 1. Nhận diện Smuggler's Cache (.otc)
        if (text.Contains("Metadata/Chests/Chest") || text.Contains("BaseEvents", StringComparison.OrdinalIgnoreCase))
        {
            return @"version 2
extends ""Metadata/Chests/Chest""

BaseEvents
{
	on_construction_complete =
	""
		DisableTargetable();
		Delay( 0.05, { PlayAnimation( random_variance ); } );
	""
}

Render
{
	label_position = ""middle,      middle,      middle""
	label_position_offset = ""0,     0,      220""
}

WorldDescription
{
	header_image = ""Art/2DArt/UIImages/InGame/CustomBorderFrames/CustomTitleRed""
	border_image = ""Art/2DArt/UIImages/InGame/CustomBorderFrames/CustomFrameRed""
	body_text = ""VillageMineralBodyAmber""
	body_text_colour = ""255,      255,      255,      255""
	border_colour = ""0,      0,      0,      0""
}

ProximityTrigger
{
	radius = 260
	condition = ""players""
	on_triggered = ""PlayTextAudio( NavaliOnThePaleCourt , Metadata/NPC/League/Prophecy/NavaliWild );""
}
}";
        }
        // nhận diện file ao
        if (text.Contains("fx_start/fxRIG.amd") && !text.Contains("ClientAnimationController"))
        {
            return @"version 2
extends ""Metadata/Parent""

AttachedAnimatedObject
{
	attached_object = ""<root> Metadata/Terrain/Leagues/Crucible/Objects/crucible_barrier.ao""
}

AnimationController
{
	metadata = ""Art/Models/Effects/encounter_forge_object/EncounterForgeObject_rig.amd""
	default_animation = ""idle""
}

Hull
{
	walk_tri = ""-8 -6 -6 14 8 -6""
	walk_tri = ""5 14 8 -6 -6 14""
	proj_tri = ""-8 -6 -6 14 8 -6""
	proj_tri = ""5 14 8 -6 -6 14""
}";
        }

        // 3. Nhận diện Coin Cache AOC (.aoc)
        if (text.Contains("ClientAnimationController") && text.Contains("fx_start/fxRIG.ast"))
        {
            return @"version 2
extends ""Metadata/Parent""

ClientAnimationController
{
	skeleton = ""Art/Models/Effects/encounter_forge_object/EncounterForgeObject_rig.ast""
	socket = ""aux_env_01""
		parent = ""root_jntBnd""
		translation = ""653.76 0 -198.208""
	socket = ""aux_cyl_reduce""
		parent = ""pool_cyl_jntBnd""
		translation = ""8.9375 0 8.91994""
}

SkinMesh
{
	skin = ""Art/Models/Effects/encounter_forge_object/EncounterForgeObject.sm""
		lava_flow_tubeShape = ""Art/Models/Effects/encounter_forge_object/Textures/EncounterForge_MoltenTube_01.mat:0""
}

BoneGroups
{
	bone_group = ""pool_midline false pool_mid_jntBnd pool_line_jntBnd""
	bone_group = ""grd_midline false grd_mid_jntBnd grd_line_jntBnd""
	bone_group = ""grd_circle false grd_circle_1_jntBnd grd_mid_jntBnd grd_circle_2_jntBnd""
	bone_group = ""grd_cyl false grd_mid_jntBnd grd_cyl_jntBnd""
	bone_group = ""root_up false root_jntBnd grd_mid_jntBnd""
	bone_group = ""env_cyl false root_jntBnd aux_env_01""
	bone_group = ""pool_cyl false pool_mid_jntBnd aux_cyl_reduce""
}


ParticleEffects
{
	animations = '[
		{
			""name"": ""idle"",
			""events"": [
				{
					""type"": ""ParticleEffectEventType"",
					""time"": 0.0,
					""filename"": ""Metadata/Effects/Spells/monsters_effects/League_Crucible/attachments/fx/fountain_pool_midline.pet"",
					""bone_group"": ""pool_cyl""
				},
				{
					""type"": ""ParticleEffectEventType"",
					""time"": 0.0,
					""filename"": ""Metadata/Effects/Spells/monsters_effects/League_Crucible/attachments/fx/fountain_grd_midline.pet"",
					""bone_group"": ""grd_midline""
				},
				{
					""type"": ""ParticleEffectEventType"",
					""time"": 0.0,
					""filename"": ""Metadata/Effects/Spells/monsters_effects/League_Crucible/attachments/fx/fountain_root_up_idle.pet"",
					""bone_group"": ""root_up""
				},
				{
					""type"": ""ParticleEffectEventType"",
					""time"": 0.0,
					""filename"": ""Metadata/Effects/Spells/monsters_effects/League_Crucible/attachments/fx/env_ash.pet"",
					""bone_group"": ""env_cyl""
				},
				{
					""type"": ""ParticleEffectEventType"",
					""time"": 0.0007149999728426337,
					""filename"": ""Metadata/Effects/Spells/monsters_effects/League_Crucible/attachments/fx/fountain_grd_cyl_idle_combat_active.pet"",
					""bone_group"": ""grd_cyl""
				}
			]
			}
	]'
	}


SoundEvents
{
	animations = '[
		{
			""name"": ""idle"",
			""events"": [
				{
					""type"": ""SoundEventType"",
					""time"": 0.0,
					""filename"": ""Audio/Sound Effects/Environment/CrucibleLeague/CrucibleForge/CrucibleIdle.loop.ogg"",
					""bone_name"": ""grd_line_jntBnd""
				}
			]
		}
	]'
}

WindEvents
{
	animations = '[
		{
			""name"": ""idle"",
			""events"": [
				{
					""type"": ""WindEventType"",
					""time"": 0.0010000000474974514,
					""shape"": ""FireSource"",
					""initial_phase"": 1.0,
					""bone_name"": ""root_jntBnd""
				},
				{
					""type"": ""WindEventType"",
					""time"": 0.10000000149011612,
					""shape"": ""FireSource"",
					""initial_phase"": 1.0,
					""bone_name"": ""root_jntBnd""
				},
				{
					""type"": ""WindEventType"",
					""time"": 0.20000000298023225,
					""shape"": ""FireSource"",
					""initial_phase"": 1.0,
					""bone_name"": ""root_jntBnd""
				},
				{
					""type"": ""WindEventType"",
					""time"": 0.30000001192092898,
					""shape"": ""FireSource"",
					""initial_phase"": 1.0,
					""bone_name"": ""root_jntBnd""
				},
				{
					""type"": ""WindEventType"",
					""time"": 0.4000000059604645,
					""shape"": ""FireSource"",
					""initial_phase"": 1.0,
					""bone_name"": ""root_jntBnd""
				},
				{
					""type"": ""WindEventType"",
					""time"": 0.5,
					""shape"": ""FireSource"",
					""initial_phase"": 1.0,
					""bone_name"": ""root_jntBnd""
				},
				{
					""type"": ""WindEventType"",
					""time"": 0.6000000238418579,
					""shape"": ""FireSource"",
					""initial_phase"": 1.0,
					""bone_name"": ""root_jntBnd""
				},
				{
					""type"": ""WindEventType"",
					""time"": 0.699999988079071,
					""shape"": ""FireSource"",
					""initial_phase"": 1.0,
					""bone_name"": ""root_jntBnd""
				},
				{
					""type"": ""WindEventType"",
					""time"": 0.800000011920929,
					""shape"": ""FireSource"",
					""initial_phase"": 1.0,
					""bone_name"": ""root_jntBnd""
				},
				{
					""type"": ""WindEventType"",
					""time"": 0.8999999761581421,
					""shape"": ""FireSource"",
					""initial_phase"": 1.0,
					""bone_name"": ""root_jntBnd""
				}
			]
		}
	]'
}";
        }

        return text;
    }
}