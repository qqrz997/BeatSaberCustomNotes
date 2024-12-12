using CustomNotes.Managers;
using CustomNotes.Overrides;
using CustomNotes.Settings.Utilities;
using CustomNotes.Utilities;
using SiraUtil.Extras;
using SiraUtil.Objects;
using SiraUtil.Objects.Beatmap;
using System.Reflection;
using UnityEngine;
using Zenject;

namespace CustomNotes.Installers
{
    internal class PlayerInstaller : Installer
    {
        private readonly PluginConfig pluginConfig;
        private readonly NoteAssetLoader noteAssetLoader;
        private readonly GameplayCoreSceneSetupData gameplayCoreSceneSetupData;

        private const int DecorationPriority = 300;

        public PlayerInstaller(PluginConfig pluginConfig, NoteAssetLoader noteAssetLoader, GameplayCoreSceneSetupData gameplayCoreSceneSetupData)
        {
            this.pluginConfig = pluginConfig;
            this.noteAssetLoader = noteAssetLoader;
            this.gameplayCoreSceneSetupData = gameplayCoreSceneSetupData;
        }

        public override void InstallBindings()
        {
            if (!pluginConfig.Enabled || !noteAssetLoader.CustomNoteSelected)
            {
                return;
            }
            
            bool ghostNotes = gameplayCoreSceneSetupData.gameplayModifiers.proMode;
            bool disappearingArrows = gameplayCoreSceneSetupData.gameplayModifiers.disappearingArrows;
            bool smallCubes = gameplayCoreSceneSetupData.gameplayModifiers.smallCubes;
            bool proMode = gameplayCoreSceneSetupData.gameplayModifiers.proMode;
            
            bool isMultiplayer = Container.HasBinding<MultiplayerLevelSceneSetupData>();
            bool isNoodle = gameplayCoreSceneSetupData.RequiresNoodleExtensions();
            bool isIncompatibleMode = ghostNotes || disappearingArrows || smallCubes || isNoodle;
            
            // Disable the mod in cases where scoring would be disabled if custom notes are enabled
            bool autoDisable = pluginConfig.AutoDisable && isIncompatibleMode;
            
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
            MaterialSwapper.ReplaceMaterialsForGameObject(customNote.NoteLeft);
            MaterialSwapper.ReplaceMaterialsForGameObject(customNote.NoteRight);
            MaterialSwapper.ReplaceMaterialsForGameObject(customNote.NoteDotLeft);
            MaterialSwapper.ReplaceMaterialsForGameObject(customNote.NoteDotRight);
            Utils.AddMaterialPropertyBlockController(customNote.NoteLeft);
            Utils.AddMaterialPropertyBlockController(customNote.NoteRight);
            Utils.AddMaterialPropertyBlockController(customNote.NoteDotLeft);
            Utils.AddMaterialPropertyBlockController(customNote.NoteDotRight);

            Container.BindMemoryPool<SiraPrefabContainer, SiraPrefabContainer.Pool>()
                .WithId(Protocol.LeftArrowPool)
                .WithInitialSize(25)
                .FromComponentInNewPrefab(NotePrefabContainer(customNote.NoteLeft));
            
            Container.BindMemoryPool<SiraPrefabContainer, SiraPrefabContainer.Pool>()
                .WithId(Protocol.RightArrowPool)
                .WithInitialSize(25)
                .FromComponentInNewPrefab(NotePrefabContainer(customNote.NoteRight));
            
            if (customNote.NoteDotLeft != null)
            {
                Container.BindMemoryPool<SiraPrefabContainer, SiraPrefabContainer.Pool>()
                    .WithId(Protocol.LeftDotPool)
                    .WithInitialSize(10)
                    .FromComponentInNewPrefab(NotePrefabContainer(customNote.NoteDotLeft));
            }

            if (customNote.NoteDotRight != null)
            {
                Container.BindMemoryPool<SiraPrefabContainer, SiraPrefabContainer.Pool>()
                    .WithId(Protocol.RightDotPool)
                    .WithInitialSize(10)
                    .FromComponentInNewPrefab(NotePrefabContainer(customNote.NoteDotRight));
            }

            #endregion

            #region Bomb Setup

            Container.RegisterRedecorator(new BombNoteRegistration(bombNoteController =>
            {
                bombNoteController.gameObject.AddComponent<CustomBombController>();
                return bombNoteController;
            }, DecorationPriority));

            if (customNote.NoteBomb != null)
            {
                MaterialSwapper.GetMaterials();
                MaterialSwapper.ReplaceMaterialsForGameObject(customNote.NoteBomb);
                Container.BindMemoryPool<SiraPrefabContainer, SiraPrefabContainer.Pool>()
                    .WithId(Protocol.BombPool)
                    .WithInitialSize(10)
                    .FromComponentInNewPrefab(NotePrefabContainer(customNote.NoteBomb));
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

            MaterialSwapper.ReplaceMaterialsForGameObject(customNote.BurstSliderLeft);
            MaterialSwapper.ReplaceMaterialsForGameObject(customNote.BurstSliderRight);
            Utils.AddMaterialPropertyBlockController(customNote.BurstSliderLeft);
            Utils.AddMaterialPropertyBlockController(customNote.BurstSliderRight);

            Container.BindMemoryPool<SiraPrefabContainer, SiraPrefabContainer.Pool>()
                .WithId(Protocol.LeftBurstSliderPool).WithInitialSize(40)
                .FromComponentInNewPrefab(NotePrefabContainer(customNote.BurstSliderLeft));
            
            Container.BindMemoryPool<SiraPrefabContainer, SiraPrefabContainer.Pool>()
                .WithId(Protocol.RightBurstSliderPool).WithInitialSize(40)
                .FromComponentInNewPrefab(NotePrefabContainer(customNote.BurstSliderRight));

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

            MaterialSwapper.ReplaceMaterialsForGameObject(customNote.BurstSliderHeadLeft);
            MaterialSwapper.ReplaceMaterialsForGameObject(customNote.BurstSliderHeadRight);
            MaterialSwapper.ReplaceMaterialsForGameObject(customNote.BurstSliderHeadDotLeft);
            MaterialSwapper.ReplaceMaterialsForGameObject(customNote.BurstSliderHeadDotRight);
            Utils.AddMaterialPropertyBlockController(customNote.BurstSliderHeadLeft);
            Utils.AddMaterialPropertyBlockController(customNote.BurstSliderHeadRight);
            Utils.AddMaterialPropertyBlockController(customNote.BurstSliderHeadDotLeft);
            Utils.AddMaterialPropertyBlockController(customNote.BurstSliderHeadDotRight);

            Container.BindMemoryPool<SiraPrefabContainer, SiraPrefabContainer.Pool>()
                .WithId(Protocol.LeftBurstSliderHeadPool).WithInitialSize(10)
                .FromComponentInNewPrefab(NotePrefabContainer(customNote.BurstSliderHeadLeft));
            
            Container.BindMemoryPool<SiraPrefabContainer, SiraPrefabContainer.Pool>()
                .WithId(Protocol.RightBurstSliderHeadPool).WithInitialSize(10)
                .FromComponentInNewPrefab(NotePrefabContainer(customNote.BurstSliderHeadRight));
            
            Container.BindMemoryPool<SiraPrefabContainer, SiraPrefabContainer.Pool>()
                .WithId(Protocol.LeftBurstSliderHeadDotPool).WithInitialSize(10)
                .FromComponentInNewPrefab(NotePrefabContainer(customNote.BurstSliderHeadDotLeft));
            
            Container.BindMemoryPool<SiraPrefabContainer, SiraPrefabContainer.Pool>()
                .WithId(Protocol.RightBurstSliderHeadDotPool).WithInitialSize(10)
                .FromComponentInNewPrefab(NotePrefabContainer(customNote.BurstSliderHeadDotRight));
            
            #endregion

            #region Set Slider Layer

            if (pluginConfig.HmdOnly || LayerUtils.ForceHmdOnly)
            {
                SliderController RedecorateSliderLayer(SliderController sliderController)
                {
                    sliderController.transform.Find("MeshRenderer").gameObject.layer = 0;
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
            var prefab = new GameObject("CustomNotes" + initialPrefab.name).AddComponent<SiraPrefabContainer>();
            prefab.Prefab = initialPrefab;
            return prefab;
        }
    }
}