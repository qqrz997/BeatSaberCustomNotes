using Zenject;
using UnityEngine;
using SiraUtil.Objects;
using CustomNotes.Data;
using SiraUtil.Interfaces;
using CustomNotes.Overrides;
using CustomNotes.Utilities;

namespace CustomNotes.Managers
{
    public class CustomBurstSliderController : MonoBehaviour, IColorable, INoteControllerNoteWasCutEvent, INoteControllerNoteWasMissedEvent, INoteControllerDidInitEvent, INoteControllerNoteDidDissolveEvent
    {
        private PluginConfig pluginConfig;

        protected Transform NoteCube;
        private CustomNote customNote;
        private BurstSliderGameNoteController burstSliderGameNoteController;
        private CustomNoteColorNoteVisuals customNoteColorNoteVisuals;

        protected GameObject ActiveNote;
        protected SiraPrefabContainer Container;
        protected SiraPrefabContainer.Pool ActivePool;

        private SiraPrefabContainer.Pool leftBurstSliderPool;
        private SiraPrefabContainer.Pool rightBurstSliderPool;

        public Color Color
        {
            get => customNoteColorNoteVisuals != null ? customNoteColorNoteVisuals._noteColor : Color.white;
            set => SetColor(value);
        }

        [Inject]
        internal void Init(PluginConfig pluginConfig,
            NoteAssetLoader noteAssetLoader,
            [Inject(Id = Protocol.LeftBurstSliderPool)] SiraPrefabContainer.Pool leftBurstSliderPool,
            [Inject(Id = Protocol.RightBurstSliderPool)] SiraPrefabContainer.Pool rightBurstSliderPool)
        {
            this.pluginConfig = pluginConfig;
            this.leftBurstSliderPool = leftBurstSliderPool;
            this.rightBurstSliderPool = rightBurstSliderPool;

            customNote = noteAssetLoader.CustomNoteObjects[noteAssetLoader.SelectedNoteIdx];

            burstSliderGameNoteController = GetComponent<BurstSliderGameNoteController>();
            customNoteColorNoteVisuals = gameObject.GetComponent<CustomNoteColorNoteVisuals>();
            customNoteColorNoteVisuals.enabled = true;

            burstSliderGameNoteController.didInitEvent.Add(this);
            burstSliderGameNoteController.noteWasCutEvent.Add(this);
            burstSliderGameNoteController.noteWasMissedEvent.Add(this);
            burstSliderGameNoteController.noteDidDissolveEvent.Add(this);
            customNoteColorNoteVisuals.didInitEvent += Visuals_DidInit;

            NoteCube = burstSliderGameNoteController.gameObject.transform.Find("NoteCube");

            var noteMesh = GetComponentInChildren<MeshRenderer>();
            if (pluginConfig.UseHmdOnly())
            {
                noteMesh.gameObject.layer = (int)NoteLayer.ThirdPerson;
            }
            else
            {
                // only disable if custom notes display on both hmd and display
                noteMesh.forceRenderingOff = true;
            }
        }

        public void HandleNoteControllerNoteWasMissed(NoteController nc)
        {
            if (Container != null)
            {
                Container.transform.SetParent(null);

                if (nc.noteData.colorType != ColorType.None)
                {
                    Container.Prefab.SetActive(false);
                    ActivePool?.Despawn(Container);
                    Container = null;
                }
            }
        }

        public void HandleNoteControllerDidInit(NoteControllerBase noteController)
        {
            var data = noteController.noteData;
            SpawnThenParent(data.colorType == ColorType.ColorA
                ? leftBurstSliderPool
                : rightBurstSliderPool);
        }

        private void ParentNote(GameObject fakeMesh)
        {
            Container.transform.SetParent(NoteCube);
            fakeMesh.transform.localPosition = Container.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
            Container.transform.localRotation = Quaternion.identity;
            fakeMesh.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f) * pluginConfig.GetNoteSize();
            Container.transform.localScale = Vector3.one;
        }

        private void SpawnThenParent(SiraPrefabContainer.Pool noteModelPool)
        {
            Container = noteModelPool.Spawn();
            Container.Prefab.SetActive(true);
            ActiveNote = Container.Prefab;
            ActivePool = noteModelPool;
            
            var noteLayer = pluginConfig.UseHmdOnly() ? NoteLayer.FirstPerson : NoteLayer.Note;
            ActiveNote.SetLayerRecursively(noteLayer);
            
            ParentNote(ActiveNote);
        }

        protected void SetActiveThenColor(GameObject note, Color color)
        {
            note.SetActive(true);
            if (customNote.Descriptor.UsesNoteColor)
            {
                SetColor(color, true);
            }
        }

        private void Visuals_DidInit(ColorNoteVisuals visuals, NoteControllerBase noteController)
        {
            SetActiveThenColor(ActiveNote, ((CustomNoteColorNoteVisuals)visuals)._noteColor);
            
            // Hide certain parts of the default note which is not required
            if (!pluginConfig.UseHmdOnly())
            {
                customNoteColorNoteVisuals.SetBaseGameVisualsLayer(NoteLayer.Note);
                if (customNote.Descriptor.DisableBaseNoteArrows)
                {
                    customNoteColorNoteVisuals.TurnOffVisuals();
                }
                else if (!pluginConfig.NoteSizeEquals(1))
                {
                    customNoteColorNoteVisuals.ScaleVisuals(pluginConfig.GetNoteSize());
                }
                
                return;
            }

            // HMDOnly code
            customNoteColorNoteVisuals.SetBaseGameVisualsLayer(NoteLayer.ThirdPerson);
            if (!customNote.Descriptor.DisableBaseNoteArrows)
            {
                if (!pluginConfig.NoteSizeEquals(1))
                {
                    // arrows should be enabled in both views, with fake arrows rescaled
                    customNoteColorNoteVisuals.CreateAndScaleFakeVisuals(NoteLayer.FirstPerson, pluginConfig.GetNoteSize());
                }
                else
                {
                    // arrows should be enabled in both views
                    customNoteColorNoteVisuals.CreateFakeVisuals(NoteLayer.FirstPerson);
                }
            }
        }

        protected void OnDestroy()
        {
            if (burstSliderGameNoteController != null)
            {
                burstSliderGameNoteController.didInitEvent.Remove(this);
                burstSliderGameNoteController.noteWasCutEvent.Remove(this);
                burstSliderGameNoteController.noteWasMissedEvent.Remove(this);
                burstSliderGameNoteController.noteDidDissolveEvent.Remove(this);
            }
            if (customNoteColorNoteVisuals != null)
            {
                customNoteColorNoteVisuals.didInitEvent -= Visuals_DidInit;
            }
        }

        public void SetColor(Color color)
        {
            SetColor(color, false);
        }

        public void SetColor(Color color, bool updateMatBlocks)
        {
            if (ActiveNote != null)
            {
                customNoteColorNoteVisuals.SetColor(color, updateMatBlocks);
                Utils.ColorizeCustomNote(color, customNote.Descriptor.NoteColorStrength, ActiveNote);
            }
        }

        public void HandleNoteControllerNoteWasCut(NoteController nc, in NoteCutInfo _)
        {
            HandleNoteControllerNoteWasMissed(nc);
        }

        public void HandleNoteControllerNoteDidDissolve(NoteController noteController)
        {
            HandleNoteControllerNoteWasMissed(noteController);
        }
    }
}