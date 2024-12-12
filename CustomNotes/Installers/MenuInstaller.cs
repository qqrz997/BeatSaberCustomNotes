﻿using CustomNotes.Managers;
using CustomNotes.Settings.UI;
using Zenject;

namespace CustomNotes.Installers
{
    internal class MenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.Bind<NotePreviewViewController>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<NoteDetailsViewController>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<NoteListViewController>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<NotesFlowCoordinator>().FromNewComponentOnNewGameObject().AsSingle();
            Container.BindInterfacesAndSelfTo<NoteModifierViewController>().AsSingle();
            Container.BindInterfacesTo<CustomNotesViewManager>().AsSingle();
            Container.BindInterfacesTo<MenuButtonManager>().AsSingle();
        }
    }
}