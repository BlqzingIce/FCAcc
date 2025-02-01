using SiraUtil.Affinity;
using SiraUtil.Logging;
using Zenject;

namespace FCAcc
{
    internal class GoodCutInitPatch : IAffinity
    {
        [InjectOptional] readonly CustomCounter _counter = null;
        [Inject] private readonly StandardLevelScenesInit _standardLevelScenesInit = null;

        [AffinityPostfix]
        [AffinityPatch(typeof(GoodCutScoringElement), nameof(GoodCutScoringElement.Init))]
        private void Postfix(GoodCutScoringElement __instance)
        {
            if (_counter == null) return;
            if (_standardLevelScenesInit.isInReplay) _counter.QueueScoringElement(__instance);
        }
    }

    internal class StandardLevelScenesInit : IAffinity
    {
        internal bool isInReplay = false;
        
        [AffinityPostfix]
        [AffinityPatch(typeof(StandardLevelScenesTransitionSetupDataSO), "InitAndSetupScenes")]
        private void Postfix(StandardLevelScenesTransitionSetupDataSO __instance)
        {
            isInReplay = "Replay".Equals(__instance.gameMode);
        }
    }
}