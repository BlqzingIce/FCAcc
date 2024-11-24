using Zenject;

namespace FCAcc
{
    internal class PatchInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<InitPatch>().AsSingle();
        }
    }
}