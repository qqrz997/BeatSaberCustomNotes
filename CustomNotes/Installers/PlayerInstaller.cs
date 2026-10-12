using System.Reflection;
using CustomNotes.Components;
using CustomNotes.Managers;
using CustomNotes.Models;
using CustomNotes.Utilities;
using SiraUtil.Extras;
using SiraUtil.Objects;
using SiraUtil.Objects.Beatmap;
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
        if (!config.Enabled || !noteAssetLoader.TryGetCustomNote(config.LastNote, out var customNote))
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
            
        if (disable)
        {
            Plugin.Log.Debug("CustomNotes is disabled\n" +
                            $"ghostNotes: {ghostNotes}\n" +
                            $"disappearingArrows: {disappearingArrows}\n" +
                            $"smallCubes: {smallCubes}\n" +
                            $"proMode: {proMode}\n" +
                            $"isMultiplayer: {isMultiplayer}\n" +
                            $"isNoodle: {isNoodle}\n" +
                            $"autoDisable: {autoDisable}");
            return;
        }

        Container.BindInterfacesTo<WatermarkManager>().AsSingle();
        Container.BindInterfacesTo<ScoreSubmissionManager>().AsSingle();

        Container.BindInstance(customNote).AsSingle();
        
        var prefab = customNote.Prefab;
        
        #region Note Setup

        var leftNoteArrow = prefab.Left.NoteArrow.Instantiate();
        var rightNoteArrow = prefab.Right.NoteArrow.Instantiate();
        var leftNoteDot =  prefab.Left.NoteDot.Instantiate();
        var rightNoteDot =  prefab.Right.NoteDot.Instantiate();

        Container.RegisterRedecorator((RedecoratorRegistration)(proMode
            ? new ProModeNoteRegistration(RedecorateNote, DecorationPriority)
            : new BasicNoteRegistration(RedecorateNote, DecorationPriority)));

        Container.BindMemoryPool<NoteInstanceContainer, NoteInstanceContainer.Pool>()
            .WithId(Protocol.LeftArrowPool)
            .WithInitialSize(25)
            .FromComponentInNewPrefab(NoteInstanceContainer.Create(leftNoteArrow));
            
        Container.BindMemoryPool<NoteInstanceContainer, NoteInstanceContainer.Pool>()
            .WithId(Protocol.RightArrowPool)
            .WithInitialSize(25)
            .FromComponentInNewPrefab(NoteInstanceContainer.Create(rightNoteArrow));
            
        Container.BindMemoryPool<NoteInstanceContainer, NoteInstanceContainer.Pool>()
            .WithId(Protocol.LeftDotPool)
            .WithInitialSize(10)
            .FromComponentInNewPrefab(NoteInstanceContainer.Create(leftNoteDot));

        Container.BindMemoryPool<NoteInstanceContainer, NoteInstanceContainer.Pool>()
            .WithId(Protocol.RightDotPool)
            .WithInitialSize(10)
            .FromComponentInNewPrefab(NoteInstanceContainer.Create(rightNoteDot));

        #endregion

        #region Bomb Setup

        var bomb = prefab.Bomb.Instantiate();
        
        Container.RegisterRedecorator(new BombNoteRegistration(bombNoteController =>
        {
            bombNoteController.gameObject.AddComponent<CustomBombController>();
            return bombNoteController;
        }, DecorationPriority));

        if (bomb != null)
        {
            Container.BindMemoryPool<NoteInstanceContainer, NoteInstanceContainer.Pool>()
                .WithId(Protocol.BombPool)
                .WithInitialSize(10)
                .FromComponentInNewPrefab(NoteInstanceContainer.Create(bomb));
        }

        #endregion

        #region Burst Slider Setup
        
        var leftChainSegment = prefab.Left.ChainSegment.Instantiate();
        var rightChainSegment = prefab.Right.ChainSegment.Instantiate();
        
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

        Container.BindMemoryPool<NoteInstanceContainer, NoteInstanceContainer.Pool>()
            .WithId(Protocol.LeftBurstSliderPool).WithInitialSize(40)
            .FromComponentInNewPrefab(NoteInstanceContainer.Create(leftChainSegment));
            
        Container.BindMemoryPool<NoteInstanceContainer, NoteInstanceContainer.Pool>()
            .WithId(Protocol.RightBurstSliderPool).WithInitialSize(40)
            .FromComponentInNewPrefab(NoteInstanceContainer.Create(rightChainSegment));

        #endregion

        #region Burst Slider Head Setup

        var leftChainArrow = prefab.Left.ChainArrow.Instantiate();
        var rightChainArrow = prefab.Right.ChainArrow.Instantiate();
        var leftChainDot = prefab.Left.ChainDot.Instantiate();
        var rightChainDot = prefab.Right.ChainDot.Instantiate();

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

        Container.BindMemoryPool<NoteInstanceContainer, NoteInstanceContainer.Pool>()
            .WithId(Protocol.LeftBurstSliderHeadPool).WithInitialSize(10)
            .FromComponentInNewPrefab(NoteInstanceContainer.Create(leftChainArrow));
            
        Container.BindMemoryPool<NoteInstanceContainer, NoteInstanceContainer.Pool>()
            .WithId(Protocol.RightBurstSliderHeadPool).WithInitialSize(10)
            .FromComponentInNewPrefab(NoteInstanceContainer.Create(rightChainArrow));
            
        Container.BindMemoryPool<NoteInstanceContainer, NoteInstanceContainer.Pool>()
            .WithId(Protocol.LeftBurstSliderHeadDotPool).WithInitialSize(10)
            .FromComponentInNewPrefab(NoteInstanceContainer.Create(leftChainDot));
            
        Container.BindMemoryPool<NoteInstanceContainer, NoteInstanceContainer.Pool>()
            .WithId(Protocol.RightBurstSliderHeadDotPool).WithInitialSize(10)
            .FromComponentInNewPrefab(NoteInstanceContainer.Create(rightChainDot));
            
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
}