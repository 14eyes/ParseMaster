﻿namespace ParseMaster;

internal class ConfigResolver
{
    //TODO: i kinda hate this but it works for now :(
    private static readonly Dictionary<string, (string, FileType, bool)> ConfigMap = new()
    {
        [@"\Scene\Point\"] = ("ConfigScene", FileType.Single, true),
        [@"\Scene\SceneNpcBorn\"] = ("ConfigLevelNpcBornPosNoGroup", FileType.Single, true),
        [@"\Scene\SceneNpcBornNoGroup\"] = ("ConfigLevelNpcBornPosNoGroup", FileType.Single, true),
        [@"\Scene\LevelLayout\"] = ("ConfigLevelLayout", FileType.Single, true),
        [@"\Scene\WorldArea\"] = ("ConfigWorldAreaLayout", FileType.Single, true),
        [@"\Voice\Items\"] = ("ConfigExternalVoiceItem", FileType.Dictionary, true),
        [@"\Voice\Lut\"] = ("ConfigExternalVoiceLookupItem", FileType.DictionaryVuit, true),
        [@"\Voice\Emo\"] = ("ConfigExternalVoiceLookupItem", FileType.Single, true),
        [@"\Talk\Activity\"] = ("ConfigActivityDialogGroup", FileType.Single, true),
        [@"\Talk\ActivityGroup\"] = ("ConfigActivityTalkScheme", FileType.Single, true),
        [@"\Talk\Blossom\"] = ("ConfigBlossomDialogGroup", FileType.Single, true),
        [@"\Talk\BlossomGroup\"] = ("ConfigTalkScheme", FileType.Single, true),
        [@"\Talk\Gadget\"] = ("ConfigGadgetDialogGroup", FileType.Single, true),
        [@"\Talk\GadgetGroup\"] = ("ConfigGadgetTalkScheme", FileType.Single, true),
        [@"\Talk\Npc\"] = ("ConfigNarratorDialogGroup", FileType.Single, true),
        [@"\Talk\NpcOther\"] = ("ConfigNarratorDialogGroup", FileType.Single, true),
        [@"\Talk\NpcGroup\"] = ("ConfigNpcTalkScheme", FileType.Single, true),
        [@"\Talk\Coop\"] = ("ConfigCoopDialogGroup", FileType.Single, true),
        [@"\Talk\FreeGroup\"] = ("ConfigFreeDialogGroup", FileType.Single, true),
        [@"\Talk\Other\"] = ("ConfigDialogGroup", FileType.Single, true),
        [@"\Talk\Quest\"] = ("ConfigDialogGroup", FileType.Single, false),
        [@"\Music\Music\"] = ("ConfigMusic", FileType.Single, true),
        [@"\Music\Songs\"] = ("ConfigSong", FileType.List, true),
        [@"\Music\TransitionConditions\"] = ("ConfigMusicCondition", FileType.List, true),
        [@"\LevelDesign\ActionPoints\"] = ("ConfigLevelActionPoint", FileType.Single, true),
        [@"\LevelDesign\Gadgets\"] = ("ConfigLevelGadgetData", FileType.Single, true),
        [@"\LevelDesign\Monsters\"] = ("ConfigLevelMonsterData", FileType.Single, true),
        [@"\LevelDesign\Polygons\"] = ("ConfigLevelPolygons", FileType.Single, true),
        [@"\LevelDesign\Routes\"] = ("ConfigLevelRoute", FileType.Single, true),
        [@"\LevelDesign\SimplePolygons\"] = ("ConfigPolygonArea", FileType.Single, true),
        [@"\LevelDesign\Meta\"] = ("ConfigLevelMeta", FileType.Single, true),
        [@"\LevelDesign\DeathZone\"] = ("TODO", FileType.Single, true), //TODO
        [@"\HomeFurniture\Fishpond\"] = ("ConfigHomeFishpondSet", FileType.Single, true),
        [@"\HomeFurniture\Fishtank\"] = ("ConfigHomeFishtankSet", FileType.Single, true),
        [@"\HomeFurniture\HomeworldGroup\"] = ("ConfigHomeworldFurnitureSet", FileType.Single, true),
        [@"\RandomQuest\"] = ("ConfigRandomQuestGlobalScheme", FileType.Single, true),
        [@"\Global\Embeded\TextMap\"] = ("ConfigTextMapLevel", FileType.Single, true),
        [@"\Global\Embeded\UI\"] = ("ConfigUIGlobal", FileType.Single, true),
        [@"\Global\Embeded\UI\"] = ("ConfigUIGlobal", FileType.Single, true),
        [@"\Global\GlobalValues\"] = ("ConfigGlobalValues", FileType.Single, true),
        [@"\Global\Mark\"] = ("ConfigMarkGlobal", FileType.Single, true),
        [@"\Global\GlobalData"] = ("ConfigModeStateMap", FileType.Single, true),
        [@"\Global\Npc"] = ("TODO", FileType.Single, true), //TODO
        [@"\Audio\Ambience\"] = ("ConfigAudioAmbience", FileType.Single, true),
        [@"\Audio\Area2DAmbience\"] = ("ConfigAudioArea2DAmbience", FileType.List, true),
        [@"\Audio\Avatar\"] = ("ConfigAudioAvatar", FileType.Single, true),
        [@"\Audio\AvatarMove\"] = ("ConfigAudioAvatarMove", FileType.Single, true),
        [@"\Audio\AvatarSpeech\"] = ("ConfigAvatarSpeech", FileType.Single, true),
        [@"\Audio\BaseMove\"] = ("ConfigAudioBaseMove", FileType.Single, true),
        [@"\Audio\Camera\"] = ("ConfigAudioCamera", FileType.Single, true),
        [@"\Audio\CityBlocks\"] = ("ConfigAudioCityBlocks", FileType.Single, true),
        [@"\Audio\Combat\"] = ("ConfigAudioCombat", FileType.Single, true),
        [@"\Audio\Dialog\"] = ("ConfigAudioDialog", FileType.Single, true),
        [@"\Audio\EventCulling\"] = ("ConfigAudioEventCulling", FileType.Single, true),
        [@"\Audio\Gadget\"] = ("ConfigAudioGadget", FileType.Single, true),
        [@"\Audio\GameViewState\"] = ("ConfigAudioGameViewState", FileType.Single, true),
        [@"\Audio\General\"] = ("ConfigAudioGeneral", FileType.Single, true),
        [@"\Audio\Level\"] = ("ConfigAudioLevel", FileType.Single, true),
        [@"\Audio\Listener\"] = ("ConfigAudioListener", FileType.Single, true),
        [@"\Audio\MapInfo\"] = ("ConfigAudioMapInfo", FileType.Single, true),
        [@"\Audio\Midi\"] = ("ConfigAudioMIDI", FileType.Single, true),
        [@"\Audio\Music\"] = ("ConfigAudioMusic", FileType.Single, true),
        [@"\Audio\MutualExclusion\"] = ("ConfigAudioMutualExclusion", FileType.Single, true),
        [@"\Audio\Npc\"] = ("ConfigAudioNpc", FileType.Single, true),
        [@"\Audio\Quest\"] = ("ConfigAudioQuest", FileType.Single, true),
        [@"\Audio\Resource\"] = ("ConfigAudioResource", FileType.Single, true),
        [@"\Audio\Setting\"] = ("ConfigAudioSetting", FileType.Single, true),
        [@"\Audio\Spatial\"] = ("ConfigSpatialAudio", FileType.Single, true),
        [@"\Audio\SurfaceType\"] = ("ConfigAudioSurfaceType", FileType.Single, true),
        [@"\Audio\UI\"] = ("ConfigAudioUi", FileType.Single, true),
        [@"\Audio\Weather\"] = ("ConfigAudioWeather", FileType.Single, true),
        [@"\Ability\"] = ("ConfigAbility", FileType.ListDictionary, true),
        [@"\AbilityActionChannel\"] = ("ConfigPlatformActionTokenChannel", FileType.Single, true),
        [@"\AbilityGroup\"] = ("ConfigAbilityGroup", FileType.Dictionary, true),
        [@"\AbilityPath\"] = ("ConfigAbilityPath", FileType.Single, true),
        [@"\AbilitySystem\"] = ("ConfigAbilitySystem", FileType.Single, true),
        [@"\AnimPattern\"] = ("ConfigAnimPatternPath", FileType.Single, true),
        [@"\AnimatorConfig\"] = ("ConfigAnimator", FileType.Single, true),
        [@"\AttachData\"] = ("ConfigAttachmentData", FileType.Single, true),
        [@"\AudioEmitter\"] = ("ConfigAudioEmitter", FileType.Single, true),
        [@"\Avatar\"] = ("ConfigAvatar", FileType.Single, true),
        [@"\Boundary\"] = ("ConfigBoundary", FileType.Dictionary, true),
        [@"\Climate\"] = ("ConfigClimate", FileType.Single, true),
        [@"\CodexQuest\"] = ("ConfigCodexQuest", FileType.Single, true),
        [@"\Common\ConfigGlobalCombat"] = ("ConfigGlobalCombat", FileType.Single, true),
        [@"\Component\LCBaseIntee"] = ("ConfigLCBaseIntee", FileType.Single, true),
        [@"\Component\LCGadgetIntee"] = ("ConfigLCGadgetIntee", FileType.Single, true),
        [@"\Coop\"] = ("ConfigMainCoopGroup", FileType.Single, true),
        [@"\CrowdGroupInfos\"] = ("ConfigCrowdGroupInfo", FileType.List, true),
        [@"\CrowdSpawnInfos\"] = ("ConfigCrowdSpawnInfos", FileType.Single, true),
        [@"\Cutscene\"] = ("ConfigCutsceneIndex", FileType.Single, true),
        [@"\CustomLevelDungeon\"] = ("ConfigCustomLevelDungeon", FileType.Single, true),
        [@"\DynamicAbilityPreload\"] = ("ConfigDynamicAbilityPreload", FileType.Dictionary, true),
        [@"\Effect\"] = ("ConfigEffectData", FileType.Single, true),
        [@"\EmojiBubble\"] = ("ConfigEmojiBubbleData", FileType.Dictionary, true),
        [@"\EntityBan\"] = ("ConfigEntityBanData", FileType.Single, true),
        [@"\EntityReuse\"] = ("ConfigEntityReuse", FileType.Single, true),
        [@"\Fashion\AvatarCostume\"] = ("ConfigCostumeInfo", FileType.Dictionary, true),
        [@"\Fashion\Flycloak\"] = ("ConfigFlycloakFashion", FileType.Single, true),
        [@"\Gadget\"] = ("ConfigGadget", FileType.Dictionary, true),
        [@"\GadgetPath\"] = ("ConfigGadgetPath", FileType.Single, true),
        [@"\Goddess\"] = ("ConfigResonanceCutSceneMap", FileType.Single, true),
        [@"\GraphicsSetting\"] = ("ConfigPlatformGrahpicsSetting", FileType.Single, true),
        [@"\Guide\"] = ("ConfigGuideTask", FileType.Dictionary, true),
        [@"\GuideContextList\"] = ("ConfigGuideContextList", FileType.Dictionary, true),
        [@"\Homeworld\"] = ("ConfigHomePlaceColPath", FileType.Single, true),
        [@"\HomeworldDefaultSave\"] = ("ConfigHomeworldDefaultSave", FileType.Single, true),
        [@"\HomeworldFurnitureSuit\"] = ("ConfigHomeworldFurnitureSet", FileType.Single, true),
        [@"\IndexDic\"] = ("ConfigIndexDic", FileType.DictionaryVuitVuit, true),
        [@"\Indicator\"] = ("ConfigUIIndicator", FileType.Dictionary, true),
        [@"\InterAction\"] = ("ConfigInterContainer", FileType.Single, true),
        [@"\KeyboardLayout\"] = ("ConfigKeyboardLayout", FileType.Single, true),
        [@"\LanguageSetting\"] = ("ConfigLanguageSetting", FileType.Single, true),
        [@"\LevelEntity\"] = ("ConfigLevelEntity", FileType.Dictionary, true),
        [@"\LogoPage\"] = ("ConfigLogoPageSetting", FileType.Single, true),
        [@"\LuaHackConfig\"] = ("ConfigLuaHack", FileType.Single, true),
        [@"\MainPageDisableInfo\"] = ("ConfigMainPageDisableInfo", FileType.Dictionary, true),
        [@"\Mark\"] = ("ConfigMapGlobal", FileType.Single, true),
        [@"\Monster\"] = ("ConfigMonster", FileType.Single, true),
        [@"\MultiPlatformUIData\"] = ("ConfigMutiPlatformUIData", FileType.Single, true),
        [@"\MusicGame\"] = ("ConfigMusicGame", FileType.Single, true),
        [@"\MusicGameCamera\"] = ("ConfigMusicGameCamera", FileType.Single, true),
        [@"\Npc\"] = ("ConfigNpc", FileType.Single, true),
        [@"\PlayMode\"] = ("ConfigModeStateMap", FileType.Single, true),
        [@"\Preload\"] = ("ConfigFullPreload", FileType.Single, true),
        [@"\PS4\"] = ("ConfigPS4TRC", FileType.Single, true),
        [@"\Quest\"] = ("ConfigMainQuestScheme", FileType.Single, true),
        [@"\QuestBrief\"] = ("ConfigMainQuestBrief", FileType.Single, true),
        [@"\SCameraMove\"] = ("ConfigCameraMove", FileType.Dictionary, true),
        [@"\Schedule\"] = ("ConfigJobData", FileType.Single, true),
        [@"\Shape\"] = ("ConfigBaseShape", FileType.Dictionary, true),
        [@"\Skin\"] = ("ConfigSkin", FileType.Dictionary, true),
        [@"\SoundBank\"] = ("ConfigSoundBankLookup", FileType.Single, true),
        [@"\StageAudio\"] = ("ConfigAudioStageEvents", FileType.Single, true),
        [@"\StreamPolygon\"] = ("ConfigLevelPolygon", FileType.Single, true),
        [@"\Talent\"] = ("ConfigTalentMixin", FileType.DictionaryList, true),
        [@"\Tile\"] = ("ConfigTile", FileType.List, true),
        [@"\UI\"] = ("ConfigUI", FileType.Single, true),
        [@"\Widget\"] = ("ConfigWidget", FileType.Single, true),
        [@"\WidgetNew\"] = ("ConfigMainWidgetToy", FileType.Single, true),
        [@"\PhotographCheck\"] = ("ConfigPhotographCheck", FileType.Single, true),
        [@"\CrowdTextureHash\"] = ("ConfigCrowdTextureHash", FileType.Single, true),
        [@"\ResourceCollection\"] = ("ConfigResourceCollection", FileType.Single, true),
        [@"\GlobalPerf\"] = ("ConfigPlatformPerfSetting", FileType.Single, true),
        [@"\Device\"] = ("ConfigDevice", FileType.Single, true),
        [@"\HDMesh\"] = ("ConfigHDMesh", FileType.Single, true),
        [@"\RegionPerf\"] = ("ConfigRegionPerf", FileType.List, true),
        [@"\QTE\"] = ("ConfigQTEStepNode", FileType.Single, true)
    };

    public static (string?, FileType, bool) Resolve(string path)
    {
        var config = ConfigMap.FirstOrDefault(entry => path.Contains(entry.Key)).Value;

        if (string.IsNullOrEmpty(config.Item1))
        {
            var fileName = Path.GetFileNameWithoutExtension(path);
            if (fileName.EndsWith("Data"))
                return (fileName[..^4], FileType.Packed, false);
        }
        //if (config is null)
        //throw new NotImplementedException(path);

        return config;
    }
}

public enum FileType
{
    Single = 0,
    List = 1,
    ListDictionary = 2,
    Dictionary = 3,
    DictionaryList = 4,

    DictionaryVuit = 5,
    DictionaryVuitVuit = 6,
    Packed = 7
}