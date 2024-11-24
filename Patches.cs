using SiraUtil.Affinity;
using SiraUtil.Logging;
using Zenject;

namespace FCAcc
{
    internal class InitPatch : IAffinity
    {
        [Inject] readonly CustomCounter _counter;

        [AffinityPostfix]
        [AffinityPatch(typeof(GoodCutScoringElement), nameof(GoodCutScoringElement.Init))]
        private void Postfix(GoodCutScoringElement __instance)
        {
            if (ScoreSaberUtil.isInReplay) _counter.QueueScoringElement(__instance);
        }
    }
}