using CustomNotes.Managers;
using CustomNotes.Utilities;
using SiraUtil.Extras;
using SiraUtil.Objects;
using SiraUtil.Objects.Beatmap;
using System.Reflection;
using CustomNotes.Components;
using UnityEngine;
using Zenject;

namespace CustomNotes.Installers;

internal class PlayerInstaller : Installer
{
    private readonly PluginConfig config;
    private readonly NoteAssetLoader noteAssetLoader;
    private readonly GameplayCoreSceneSetupData gameplayCoreSceneSetupData;

    private const int DecorationPriority = 300;

    private PlayerInstaller(PluginConfig config, NoteAssetLoader noteAssetLoader, GameplayCoreSceneSetupData gameplayCoreSceneSetupData)
    {
        this.config = config;
        this.noteAssetLoader = noteAssetLoader;
        this.gameplayCoreSceneSetupData = gameplayCoreSceneSetupData;
    }

    public override void InstallBindings()
    {
        if (!config.Enabled || !noteAssetLoader.CustomNoteSelected)
        {
            return;
        }
            
        bool ghostNotes = gameplayCoreSceneSetupData.gameplayModifiers.ghostNotes;
        bool disappearingArrows = gameplayCoreSceneSetupData.gameplayModifiers.disappearingArrows;
        bool smallCubes = gameplayCoreSceneSetupData.gameplayModifiers.smallCubes;
        bool proMode = gameplayCoreSceneSetupData.gameplayModifiers.proMode;
            
        bool isMultiplayer = Container.HasBinding<MultiplayerLevelSceneSetupData>();
        bool isNoodle = gameplayCoreSceneSetupData.beatmapKey.RequiresNoodleExtensions();
        bool isIncompatibleMode = ghostNotes || disappearingArrows || smallCubes || isNoodle;
            
        // Disable the mod in cases where scoring would be disabled if custom notes are enabled
        bool autoDisable = config.AutoDisable && isIncompatibleMode;
            
        bool disable = autoDisable || (isMultiplayer && (ghostNotes || disappearingArrows));

        Plugin.Log.Debug($"ghostNotes: {ghostNotes}\n" +
                         $"disappearingArrows: {disappearingArrows}\n" +
                         $"smallCubes: {smallCubes}\n" +
                         $"proMode: {proMode}\n" +
                         $"isMultiplayer: {isMultiplayer}\n" +
                         $"isNoodle: {isNoodle}\n" +
                         $"isIncompatibleMode: {isIncompatibleMode}\n" +
                         $"autoDisable: {autoDisable}\n" +
                         $"disable: {disable}\n");
            
        if (disable)
        {
            Plugin.Log.Debug("CustomNotes is disabled");
            return;
        }

        Container.BindInterfacesTo<WatermarkManager>().AsSingle();
        Container.BindInterfacesTo<ScoreSubmissionManager>().AsSingle();
            
        var customNote = noteAssetLoader.CustomNoteObjects[noteAssetLoader.SelectedNoteIdx];

        #region Note Setup

        Container.RegisterRedecorator((RedecoratorRegistration)(proMode
            ? new ProModeNoteRegistration(RedecorateNote, DecorationPriority)
            : new BasicNoteRegistration(RedecorateNote, DecorationPriority)));
                
        MaterialSwapper.GetMaterials();
        MaterialSwapper.ReplaceMaterialsForGameObject(customNote.Left.NoteArrow);
        MaterialSwapper.ReplaceMaterialsForGameObject(customNote.Right.NoteArrow);
        MaterialSwapper.ReplaceMaterialsForGameObject(customNote.Left.NoteDot);
        MaterialSwapper.ReplaceMaterialsForGameObject(customNote.Right.NoteDot);
        Utils.AddMaterialPropertyBlockController(customNote.Left.NoteArrow);
        Utils.AddMaterialPropertyBlockController(customNote.Right.NoteArrow);
        Utils.AddMaterialPropertyBlockController(customNote.Left.NoteDot);
        Utils.AddMaterialPropertyBlockController(customNote.Right.NoteDot);

        Container.BindMemoryPool<SiraPrefabContainer, SiraPrefabContainer.Pool>()
            .WithId(Protocol.LeftArrowPool)
            .WithInitialSize(25)
            .FromComponentInNewPrefab(NotePrefabContainer(customNote.Left.NoteArrow));
            
        Container.BindMemoryPool<SiraPrefabContainer, SiraPrefabContainer.Pool>()
            .WithId(Protocol.RightArrowPool)
            .WithInitialSize(25)
            .FromComponentInNewPrefab(NotePrefabContainer(customNote.Right.NoteArrow));
            
        if (customNote.Left.NoteDot != null)
        {
            Container.BindMemoryPool<SiraPrefabContainer, SiraPrefabContainer.Pool>()
                .WithId(Protocol.LeftDotPool)
                .WithInitialSize(10)
                .FromComponentInNewPrefab(NotePrefabContainer(customNote.Left.NoteDot));
        }

        if (customNote.Right.NoteDot != null)
        {
            Container.BindMemoryPool<SiraPrefabContainer, SiraPrefabContainer.Pool>()
                .WithId(Protocol.RightDotPool)
                .WithInitialSize(10)
                .FromComponentInNewPrefab(NotePrefabContainer(customNote.Right.NoteDot));
        }

        #endregion

        #region Bomb Setup

        Container.RegisterRedecorator(new BombNoteRegistration(bombNoteController =>
        {
            bombNoteController.gameObject.AddComponent<CustomBombController>();
            return bombNoteController;
        }, DecorationPriority));

        if (customNote.Bomb != null)
        {
            MaterialSwapper.GetMaterials();
            MaterialSwapper.ReplaceMaterialsForGameObject(customNote.Bomb);
            Container.BindMemoryPool<SiraPrefabContainer, SiraPrefabContainer.Pool>()
                .WithId(Protocol.BombPool)
                .WithInitialSize(10)
                .FromComponentInNewPrefab(NotePrefabContainer(customNote.Bomb));
        }

        #endregion

        #region Burst Slider Setup

        Container.RegisterRedecorator(new BurstSliderNoteRegistration(burstSliderGameNoteController =>
        {
            burstSliderGameNoteController.gameObject.AddComponent<CustomBurstSliderController>();

            var originalNoteVisuals = burstSliderGameNoteController.GetComponent<ColorNoteVisuals>();
            
            var customNoteVisuals = burstSliderGameNoteController.gameObject.AddComponent<CustomNoteColorNoteVisuals>();
            customNoteVisuals.enabled = false;
            
            foreach (var fieldInfo in originalNoteVisuals.GetType()
                         .GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic))
            {
                fieldInfo.SetValue(customNoteVisuals, fieldInfo.GetValue(originalNoteVisuals));
            }

            Object.Destroy(originalNoteVisuals);

            return burstSliderGameNoteController;
        }, DecorationPriority));

        MaterialSwapper.ReplaceMaterialsForGameObject(customNote.Left.ChainSegment);
        MaterialSwapper.ReplaceMaterialsForGameObject(customNote.Right.ChainSegment);
        Utils.AddMaterialPropertyBlockController(customNote.Left.ChainSegment);
        Utils.AddMaterialPropertyBlockController(customNote.Right.ChainSegment);

        Container.BindMemoryPool<SiraPrefabContainer, SiraPrefabContainer.Pool>()
            .WithId(Protocol.LeftBurstSliderPool).WithInitialSize(40)
            .FromComponentInNewPrefab(NotePrefabContainer(customNote.Left.ChainSegment));
            
        Container.BindMemoryPool<SiraPrefabContainer, SiraPrefabContainer.Pool>()
            .WithId(Protocol.RightBurstSliderPool).WithInitialSize(40)
            .FromComponentInNewPrefab(NotePrefabContainer(customNote.Right.ChainSegment));

        #endregion

        #region Burst Slider Head Setup

        Container.RegisterRedecorator(new BurstSliderHeadNoteRegistration(gameNoteController =>
        {
            gameNoteController.gameObject.AddComponent<CustomNoteController>();

            var originalNoteVisuals = gameNoteController.GetComponent<ColorNoteVisuals>();

            var customNoteVisuals = gameNoteController.gameObject.AddComponent<CustomNoteColorNoteVisuals>();
            customNoteVisuals.enabled = false;
            
            // Copy the values of the original note to the custom note
            foreach (var fieldInfo in originalNoteVisuals.GetType()
                         .GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic))
            {
                fieldInfo.SetValue(customNoteVisuals, fieldInfo.GetValue(originalNoteVisuals));
            }

            Object.Destroy(originalNoteVisuals);

            return gameNoteController;
        }, DecorationPriority));

        MaterialSwapper.ReplaceMaterialsForGameObject(customNote.Left.ChainArrow);
        MaterialSwapper.ReplaceMaterialsForGameObject(customNote.Right.ChainArrow);
        MaterialSwapper.ReplaceMaterialsForGameObject(customNote.Left.ChainDot);
        MaterialSwapper.ReplaceMaterialsForGameObject(customNote.Right.ChainDot);
        Utils.AddMaterialPropertyBlockController(customNote.Left.ChainArrow);
        Utils.AddMaterialPropertyBlockController(customNote.Right.ChainArrow);
        Utils.AddMaterialPropertyBlockController(customNote.Left.ChainDot);
        Utils.AddMaterialPropertyBlockController(customNote.Right.ChainDot);

        Container.BindMemoryPool<SiraPrefabContainer, SiraPrefabContainer.Pool>()
            .WithId(Protocol.LeftBurstSliderHeadPool).WithInitialSize(10)
            .FromComponentInNewPrefab(NotePrefabContainer(customNote.Left.ChainArrow));
            
        Container.BindMemoryPool<SiraPrefabContainer, SiraPrefabContainer.Pool>()
            .WithId(Protocol.RightBurstSliderHeadPool).WithInitialSize(10)
            .FromComponentInNewPrefab(NotePrefabContainer(customNote.Right.ChainArrow));
            
        Container.BindMemoryPool<SiraPrefabContainer, SiraPrefabContainer.Pool>()
            .WithId(Protocol.LeftBurstSliderHeadDotPool).WithInitialSize(10)
            .FromComponentInNewPrefab(NotePrefabContainer(customNote.Left.ChainDot));
            
        Container.BindMemoryPool<SiraPrefabContainer, SiraPrefabContainer.Pool>()
            .WithId(Protocol.RightBurstSliderHeadDotPool).WithInitialSize(10)
            .FromComponentInNewPrefab(NotePrefabContainer(customNote.Right.ChainDot));
            
        #endregion

        #region Set Slider Layer

        if (config.HmdOnly)
        {
            SliderController RedecorateSliderLayer(SliderController sliderController)
            {
                var meshRenderer = sliderController.GetComponentInChildren<MeshRenderer>();
                meshRenderer.gameObject.layer = 0;
                return sliderController;
            }
            Container.RegisterRedecorator(new ShortSliderNoteRegistration(RedecorateSliderLayer, DecorationPriority));
            Container.RegisterRedecorator(new MediumSliderNoteRegistration(RedecorateSliderLayer, DecorationPriority));
            Container.RegisterRedecorator(new LongSliderNoteRegistration(RedecorateSliderLayer, DecorationPriority));
        }

        #endregion
    }

    private static GameNoteController RedecorateNote(GameNoteController gameNoteController)
    {
        gameNoteController.gameObject.AddComponent<CustomNoteController>();

        var originalNoteVisuals = gameNoteController.GetComponent<ColorNoteVisuals>();
        var customNoteVisuals = gameNoteController.gameObject.AddComponent<CustomNoteColorNoteVisuals>();

        customNoteVisuals.enabled = false;

        // Copy the values of the original note to the custom note
        foreach (var fieldInfo in originalNoteVisuals.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic))
        {
            fieldInfo.SetValue(customNoteVisuals, fieldInfo.GetValue(originalNoteVisuals));
        }

        Object.Destroy(originalNoteVisuals);

        return gameNoteController;
    }

    private static SiraPrefabContainer NotePrefabContainer(GameObject initialPrefab)
    {
        var prefab = new GameObject($"{initialPrefab.name}SiraContainer").AddComponent<SiraPrefabContainer>();
        prefab.Prefab = initialPrefab;
        return prefab;
    }
}