using SiraUtil.Affinity;
using SiraUtil.Logging;
using Zenject;

namespace FCAcc
{
    internal class InitPatch : IAffinity
    {
        [InjectOptional] readonly CustomCounter _counter = null;

        [AffinityPostfix]
        [AffinityPatch(typeof(GoodCutScoringElement), nameof(GoodCutScoringElement.Init))]
        private void Postfix(GoodCutScoringElement __instance)
        {
            if (_counter == null) return;
            if (_counter.isInReplay) _counter.QueueScoringElement(__instance);
        }
    }
}