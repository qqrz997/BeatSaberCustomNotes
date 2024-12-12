using Zenject;
using UnityEngine;
using CustomNotes.Settings.Utilities;
using CustomNotes.Utilities;
using SiraUtil.Objects;

namespace CustomNotes.Managers
{
    internal class CustomBombController : MonoBehaviour, INoteControllerDidInitEvent, INoteControllerNoteWasCutEvent, INoteControllerNoteWasMissedEvent, INoteControllerNoteDidDissolveEvent
    {
        private PluginConfig pluginConfig;

        private BombNoteController bombNoteController;

        protected Transform BombMesh;
        protected GameObject FakeFirstPersonBombMesh;

        protected GameObject ActiveNote;
        protected SiraPrefabContainer Container;
        protected SiraPrefabContainer.Pool BombPool;

        [Inject]
        internal void Init(PluginConfig pluginConfig, [InjectOptional(Id = Protocol.BombPool)] SiraPrefabContainer.Pool bombContainerPool)
        {
            this.pluginConfig = pluginConfig;

            BombPool = bombContainerPool;

            bombNoteController = GetComponent<BombNoteController>();
            GetComponent<NoteMovement>();

            if (BombPool != null)
            {
                bombNoteController.didInitEvent.Add(this);
                bombNoteController.noteWasCutEvent.Add(this);
                bombNoteController.noteWasMissedEvent.Add(this);
                bombNoteController.noteDidDissolveEvent.Add(this);
            }
    
            BombMesh = gameObject.transform.Find("Mesh");

            if (pluginConfig.HmdOnly || LayerUtils.ForceHmdOnly)
            {
                if (BombPool == null)
                {
                    // create fake bombs for Custom Notes without Custom Bombs
                    FakeFirstPersonBombMesh = Instantiate(BombMesh.gameObject, BombMesh, true);
                    FakeFirstPersonBombMesh.name = "FakeFirstPersonBomb";

                    FakeFirstPersonBombMesh.transform.localScale = Vector3.one;
                    FakeFirstPersonBombMesh.transform.localPosition = Vector3.zero;
                    FakeFirstPersonBombMesh.transform.rotation = Quaternion.identity;
                    FakeFirstPersonBombMesh.layer = (int)NoteLayer.FirstPerson;
                }
            }
            else if (BombPool != null)
            {
                GetComponentInChildren<MeshRenderer>().enabled = false;
            }
        }

        private void DidFinish()
        {
            Container.Prefab.SetActive(false);
            Container.transform.SetParent(null);
            BombPool.Despawn(Container);
        }

        public void HandleNoteControllerDidInit(NoteControllerBase noteController)
        {
            SpawnThenParent(BombPool);
        }

        private void ParentNote(GameObject fakeMesh)
        {
            fakeMesh.SetActive(true);
            Container.transform.SetParent(BombMesh);
            fakeMesh.transform.localPosition = Container.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
            Container.transform.localRotation = Quaternion.identity;
            fakeMesh.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f) * pluginConfig.GetNoteSize();
            Container.transform.localScale = Vector3.one;
        }

        private void SpawnThenParent(SiraPrefabContainer.Pool bombModelPool)
        {
            Container = bombModelPool.Spawn();
            Container.Prefab.SetActive(true);
            ActiveNote = Container.Prefab;
            BombPool = bombModelPool;
            if (pluginConfig.HmdOnly || LayerUtils.ForceHmdOnly)
            {
                ActiveNote.SetLayerRecursively(NoteLayer.FirstPerson);
            }
            else
            {
                ActiveNote.SetLayerRecursively(NoteLayer.Note);
            }
            ParentNote(ActiveNote);
        }

        protected void OnDestroy()
        {
            if (bombNoteController != null)
            {
                bombNoteController.didInitEvent.Remove(this);
                bombNoteController.noteWasCutEvent.Remove(this);
                bombNoteController.noteWasMissedEvent.Remove(this);
                bombNoteController.noteDidDissolveEvent.Remove(this);
            }
            Destroy(FakeFirstPersonBombMesh);
        }

        public void HandleNoteControllerNoteDidDissolve(NoteController _)
        {
            DidFinish();
        }

        public void HandleNoteControllerNoteWasCut(NoteController _, in NoteCutInfo __)
        {
            DidFinish();
        }

        public void HandleNoteControllerNoteWasMissed(NoteController _)
        {
            DidFinish();
        }
    }
}
