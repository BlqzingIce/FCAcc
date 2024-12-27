using Zenject;

namespace FCAcc
{
    internal class PlayerInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<InitPatch>().AsSingle();
        }
    }
}