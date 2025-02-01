using Zenject;

namespace FCAcc
{
    internal class PlayerInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<GoodCutInitPatch>().AsSingle();
        }
    }
    
    internal class AppInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<StandardLevelScenesInit>().AsSingle();
        }
    }
}