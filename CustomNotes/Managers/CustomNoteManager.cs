using CustomNotes.Data;
using CustomNotes.Settings.Utilities;
using CustomNotes.Utilities;
using SiraUtil.Submissions;
using Zenject;

namespace CustomNotes.Managers
{
    internal class CustomNoteManager : IInitializable
    {
        private readonly Submission _submission;
        private readonly NoteAssetLoader _noteAssetLoader;
        private readonly GameplayCoreSceneSetupData _gameplayCoreSceneSetupData;

        internal CustomNoteManager([InjectOptional] Submission submission, NoteAssetLoader noteAssetLoader, GameplayCoreSceneSetupData gameplayCoreSceneSetupData)
        {
            _submission = submission;
            _noteAssetLoader = noteAssetLoader;
            _gameplayCoreSceneSetupData = gameplayCoreSceneSetupData;
        }

        private BeatmapLevel beatmapLevel;
        private BeatmapKey beatmapKey;

        public void Initialize()
        {
            beatmapLevel = _gameplayCoreSceneSetupData.beatmapLevel;
            beatmapKey = _gameplayCoreSceneSetupData.beatmapKey;

            CustomNote activeNote = _noteAssetLoader.CustomNoteObjects[_noteAssetLoader.SelectedNote];

            if (activeNote.NoteBomb != null)
            {
                MaterialSwapper.ReplaceMaterialsForGameObject(activeNote.NoteBomb);
            }

            if (_gameplayCoreSceneSetupData.gameplayModifiers.ghostNotes)
            {
                _submission?.DisableScoreSubmission("Custom Notes", "Ghost Notes");
            }
            if (_gameplayCoreSceneSetupData.gameplayModifiers.disappearingArrows)
            {
                _submission?.DisableScoreSubmission("Custom Notes", "Disappearing Arrows");
            }
            if (_gameplayCoreSceneSetupData.gameplayModifiers.smallCubes)
            {
                _submission?.DisableScoreSubmission("Custom Notes", "Small Notes");
            }
            if (Utils.IsNoodleMap(beatmapLevel, beatmapKey))
            {
                _submission?.DisableScoreSubmission("Custom Notes", "Noodle Extensions");
            }
        }
    }
}