using SiraUtil.Affinity;
using SiraUtil.Logging;
using Zenject;

namespace FCAcc
{
    internal class InitPatch : IAffinity
    {
        [Inject] readonly CustomCounter _counter = null;

        [AffinityPostfix]
        [AffinityPatch(typeof(GoodCutScoringElement), nameof(GoodCutScoringElement.Init))]
        private void Postfix(GoodCutScoringElement __instance)
        {
            if (_counter.isInReplay) _counter.QueueScoringElement(__instance);
        }
    }
}