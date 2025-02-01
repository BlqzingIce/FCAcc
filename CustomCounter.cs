using SiraUtil.Logging;
using System.Collections.Generic;
using TMPro;
using Zenject;

namespace FCAcc
{
    public class CustomCounter : CountersPlus.Counters.Custom.BasicCustomCounter
    {
        [Inject] private readonly ScoreController _scoreController = null;
        [Inject] private readonly BeatmapObjectManager _beatmapObjectManager = null;
        [Inject] private readonly PlayerHeadAndObstacleInteraction _playerHeadAndObstacleInteraction = null;
        [Inject] private readonly CountersPlus.Counters.NoteCountProcessors.NoteCountProcessor _noteCountProcessor = null;
        [Inject] private readonly StandardLevelScenesInit _standardLevelScenesInit = null;
        [Inject] private readonly SiraLog _log = null;

        private readonly Queue<ScoringElement> elementQueue = new Queue<ScoringElement>();
        
        private int goodCutCount = 0;
        private int curScoreLeft = 0;
        private int maxScoreLeft = 0;
        private int curScoreRight = 0;
        private int maxScoreRight = 0;
        private float accLeft = 1;
        private float accRight = 1;
        private float accCombined = 1;
        private string accText = "FC : 100.00%\n<color=#FF6B00>100.00% <color=#0061FF>100.00%";

        private int multiplier = 1;
        private int multiplierProgress = 0;
        private int curScore = 0;
        private int notesLeft = 0;
        private int maxScore = 0;
        private string maxText = "<color=#FFFFFF>\nMax : 100.00%";

        private TMP_Text counterTMP = null;

        public override void CounterInit()
        {
            elementQueue.Clear();
            goodCutCount = 0;
            curScoreLeft = 0;
            maxScoreLeft = 0;
            curScoreRight = 0;
            maxScoreRight = 0;
            accLeft = 1;
            accRight = 1;
            accCombined = 1;
            accText = "FC : 100.00%\n<color=#FF6B00>100.00% <color=#0061FF>100.00%";
            
            multiplier = 1;
            multiplierProgress = 0;
            curScore = 0;
            notesLeft = _noteCountProcessor.NoteCount;
            maxScore = ComputeMaxMultipliedScore(notesLeft);
            maxText = "<color=#FFFFFF>\nMax : 100.00%";
            
            counterTMP = CanvasUtility.CreateTextFromSettings(Settings);
            counterTMP.fontSize = 2.6f;
            counterTMP.lineSpacing = -45;
            counterTMP.text = accText + maxText;
            
            if (_standardLevelScenesInit.isInReplay)
            {
                _scoreController.scoringForNoteFinishedEvent += ReplayScoringFinished;
            }
            else
            {
                _scoreController.scoringForNoteFinishedEvent += ScoringFinished;
            }
            _beatmapObjectManager.noteWasCutEvent += HandleNoteWasCut;
            _beatmapObjectManager.noteWasMissedEvent += HandleNoteWasMissed;
            _playerHeadAndObstacleInteraction.headDidEnterObstaclesEvent += HandlePlayerHeadDidEnterObstacles;
        }

        public override void CounterDestroy()
        {
            if (_standardLevelScenesInit.isInReplay)
            {
                _standardLevelScenesInit.isInReplay = false;
                _scoreController.scoringForNoteFinishedEvent -= ReplayScoringFinished;
            }
            else
            {
                _scoreController.scoringForNoteFinishedEvent -= ScoringFinished;
            }
            _beatmapObjectManager.noteWasCutEvent -= HandleNoteWasCut;
            _beatmapObjectManager.noteWasMissedEvent -= HandleNoteWasMissed;
            _playerHeadAndObstacleInteraction.headDidEnterObstaclesEvent -= HandlePlayerHeadDidEnterObstacles;
            CanvasUtility.ClearAllText();
        }



        private void HandleNoteWasCut(NoteController noteController, in NoteCutInfo info)
        {
            if (ShouldProcessNote(noteController.noteData))
            {
                if (info.allIsOK) return;
                notesLeft--;
                DecreaseMultiplier();
                UpdateMaxText();
            }
            else if (noteController.noteData.gameplayType == NoteData.GameplayType.Bomb)
            {
                DecreaseMultiplier();
                UpdateMaxText();
            }
        }

        private void HandleNoteWasMissed(NoteController noteController)
        {
            if (!ShouldProcessNote(noteController.noteData)) return;
            notesLeft--;
            DecreaseMultiplier();
            UpdateMaxText();
        }

        private void HandlePlayerHeadDidEnterObstacles()
        {
            DecreaseMultiplier();
            UpdateMaxText();
        }



        internal void QueueScoringElement(ScoringElement element)
        {
            elementQueue.Enqueue(element);
        }

        private void FlushQueue()
        {
            while (elementQueue.Count > 0)
            {
                ScoringFinished(elementQueue.Dequeue());
            }
        }

        private void ReplayScoringFinished(ScoringElement scoringElement)
        {
            FlushQueue();
        }

        private void ScoringFinished(ScoringElement scoringElement)
        {
            if (!ShouldProcessNote(scoringElement.noteData)) return;
            if (scoringElement.cutScore == 0) return;
            goodCutCount++;
            if (scoringElement.noteData.colorType == ColorType.ColorA)
            {
                curScoreLeft += scoringElement.cutScore * GetMaxMultiplier(goodCutCount);
                maxScoreLeft += scoringElement.maxPossibleCutScore * GetMaxMultiplier(goodCutCount);
                accLeft = curScoreLeft / (float)maxScoreLeft;
            }
            else
            {
                curScoreRight += scoringElement.cutScore * GetMaxMultiplier(goodCutCount);
                maxScoreRight += scoringElement.maxPossibleCutScore * GetMaxMultiplier(goodCutCount);
                accRight = curScoreRight / (float)maxScoreRight;
            }
            accCombined = (curScoreLeft + curScoreRight) / (float)(maxScoreLeft + maxScoreRight);
            UpdateAccText();

            notesLeft--;
            IncreaseMultiplier();
            curScore += scoringElement.cutScore * (_standardLevelScenesInit.isInReplay ? multiplier : scoringElement.multiplier);
            UpdateMaxText();
        }



        private void UpdateAccText()
        {
            accText = $"FC : {accCombined * 100:F2}%\n<color=#FF6B00>{accLeft * 100:F2}% <color=#0061FF>{accRight * 100:F2}%";
            counterTMP.text = accText + maxText;
        }

        private void UpdateMaxText()
        {
            int curMultiplier = multiplier;
            int curProgress = multiplierProgress;
            int curNote = 0;
            int multiplierPenalty = 0;
            while (curNote < notesLeft)
            {
                if (curMultiplier == 8)
                {
                    break;
                }
                if (curProgress < curMultiplier * 2)
                {
                    curProgress++;
                }
                if (curProgress >= curMultiplier * 2)
                {
                    curMultiplier *= 2;
                    curProgress = 0;
                }
                multiplierPenalty += 115 * (8 - curMultiplier);
                ++curNote;
            }
            float maxPossibleScore = curScore + accCombined * (115 * 8 * notesLeft - multiplierPenalty);
            maxText = $"<color=#FFFFFF>\nMax : {maxPossibleScore / maxScore * 100:F2}%";
            counterTMP.text = accText + maxText;
        }



        private void IncreaseMultiplier()
        {
            if (multiplier >= 8) return;
            if (multiplierProgress < multiplier * 2)
            {
                multiplierProgress++;
            }
            if (multiplierProgress < multiplier * 2) return;
            multiplier *= 2;
            multiplierProgress = 0;
        }

        private void DecreaseMultiplier()
        {
            if (multiplierProgress > 0)
            {
                multiplierProgress = 0;
            }
            if (multiplier > 1)
            {
                multiplier /= 2;
            }
        }



        private static bool ShouldProcessNote(NoteData data)
        {
            return data.gameplayType == NoteData.GameplayType.Normal || data.gameplayType == NoteData.GameplayType.BurstSliderHead;
        }

        private static int GetMaxMultiplier(int noteCount)
        {
            if (noteCount > 13) return 8;
            if (noteCount > 5) return 4;
            if (noteCount > 1) return 2;
            return 1;
        }

        private static int ComputeMaxMultipliedScore(int noteCount)
        {
            if (noteCount == 0) return 0;
            if (noteCount == 1) return 115;
            if (noteCount <= 5) return 115 * 1 + 230 * (noteCount - 1);
            if (noteCount <= 13) return 115 * 1 + 230 * 4 + 460 * (noteCount - 5);
            return 115 * 1 + 230 * 4 + 460 * 8 + 920 * (noteCount - 13);
            // 1 = 1
            // 2, 3, 4, 5 = 2
            // 6, 7, 8, 9, 10, 11, 12, 13 = 4
            // 14+ = 8
        }
    }
}