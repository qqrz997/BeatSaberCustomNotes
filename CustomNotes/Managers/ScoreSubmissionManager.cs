using CustomNotes.Utilities;
using SiraUtil.Submissions;
using Zenject;

namespace CustomNotes.Managers;

internal class ScoreSubmissionManager : IInitializable
{
    private readonly Submission submission;
    private readonly NoteAssetLoader noteAssetLoader;
    private readonly GameplayCoreSceneSetupData gameplayCoreSceneSetupData;

    private ScoreSubmissionManager([InjectOptional] Submission submission, NoteAssetLoader noteAssetLoader, GameplayCoreSceneSetupData gameplayCoreSceneSetupData)
    {
        this.submission = submission;
        this.noteAssetLoader = noteAssetLoader;
        this.gameplayCoreSceneSetupData = gameplayCoreSceneSetupData;
    }

    public void Initialize()
    {
        var activeNote = noteAssetLoader.CustomNoteObjects[noteAssetLoader.SelectedNoteIdx];

        if (activeNote.NoteBomb != null)
        {
            MaterialSwapper.ReplaceMaterialsForGameObject(activeNote.NoteBomb);
        }

        if (submission == null)
        {
            return;
        }
        
        if (gameplayCoreSceneSetupData.gameplayModifiers.ghostNotes)
        {
            submission.DisableScoreSubmission("Custom Notes", "Ghost Notes");
        }
        if (gameplayCoreSceneSetupData.gameplayModifiers.disappearingArrows)
        {
            submission.DisableScoreSubmission("Custom Notes", "Disappearing Arrows");
        }
        if (gameplayCoreSceneSetupData.gameplayModifiers.smallCubes)
        {
            submission.DisableScoreSubmission("Custom Notes", "Small Notes");
        }
        if (gameplayCoreSceneSetupData.beatmapKey.RequiresNoodleExtensions())
        {
            submission.DisableScoreSubmission("Custom Notes", "Noodle Extensions");
        }
    }
}