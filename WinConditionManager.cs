using Battle;
using CustomChallenges.UI;
using HarmonyLib;
using ProLib.Attributes;
using ProLib.Loaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolBox.Serialization;
using UnityEngine;
using Worldmap;

namespace CustomChallenges
{
    [SceneModifier]
    public class WinConditionManager : MonoBehaviour
    {
        private WinConditionSaveData _saveData;
        private Challenge _currentChallenge;
        private DataObject _winData;

        private Timer _globalTimer;
        private Timer _battleTimer;

        private int _globalTimeLimit = -1;
        private int _battleTimeLimit = -1;
        private bool _battleActive = false;
        private int _remainingPegs = -1;

        private BattleController _battleController;

        public static WinConditionManager Instance;
        private State _state;

        public enum State
        {
            PENDING_RESULT,
            WIN_TRIGGERED,
            DEFEAT_TRIGGERED
        }

        public void Awake()
        {
            if (Instance == null) Instance = this;
            if (this != Instance)
            {
                Destroy(this);
                return;
            }

            PachinkoBall.OnShotFired += ShotFired;
        }

        public void SetGlobalTimer(Timer timer)
        {
            _globalTimer = timer;
            _globalTimer.warningThreshold = 60 * 5;
            _globalTimer.criticalThreshold = 60 * 2;
            _globalTimer.gameObject.SetActive(false);
        }

        public void SetBattleTimer(Timer timer)
        {
            _battleTimer = timer;
            _battleTimer.warningThreshold = 30;
            _battleTimer.criticalThreshold = 15;
            _battleTimer.gameObject.SetActive(false);
        }

        public void Update()
        {
            if (ChallengeManager.ChallengeActive && ChallengeManager.CurrentChallenge == _currentChallenge)
            {
                if(_state == State.PENDING_RESULT)
                {
                    if (_globalTimer != null && _globalTimeLimit > 0 && _globalTimer.Complete)
                    {
                        TriggerFailure();
                    }
                    else if (_battleTimer != null && _battleTimeLimit > 0 && _battleTimer.Complete)
                    {
                        TriggerFailure();
                    } else if (_remainingPegs >= 0 && CheckRemainingPegs())
                    {
                        TriggerBattleVictory();
                    }
                }
            }
        }

        public bool CheckRemainingPegs()
        {
            if(_battleController != null)
            {
                int count = 0;
                foreach(Peg peg in _battleController.pegManager._allPegs)
                {
                    if (peg != null && peg.pegType != Peg.PegType.DULL && peg.pegType != Peg.PegType.DESTROYED && peg.pegType != Peg.PegType.BOMB)
                    {
                        count++;
                        if (count > _remainingPegs) return false;
                    }
                }
                return true;
            }
            return false;
        }

        public void LoadChallenge(Challenge challenge)
        {
            _globalTimeLimit = -1;
            _battleTimeLimit = -1;
            _remainingPegs = -1;
            _winData = null;
            _currentChallenge = challenge;

            if (challenge != null)
            {
                if (challenge.TryGetEntry<DataObject>(Keys.WIN_CONDITIONS, out DataObject data))
                {
                    _winData = data;

                    if (_winData.TryGetEntry<int>(Keys.GLOBAL_TIME_LIMIT, out int globalTimeLimit))
                        _globalTimeLimit = globalTimeLimit;

                    if (_winData.TryGetEntry<int>(Keys.BATTLE_TIME_LIMIT, out int battleTimeLimit))
                        _battleTimeLimit = battleTimeLimit;

                    if(_winData.TryGetEntry<int>(Keys.REMAINING_PEGS, out int pegs))
                    {
                        _remainingPegs = pegs;
                    }
                }
            }
        }

        public void StartNewGame()
        {
            if(ChallengeManager.ChallengeActive && ChallengeManager.CurrentChallenge == _currentChallenge)
            {
                if (_globalTimer != null && _globalTimeLimit > 0)
                {
                    _globalTimer.SetTimeDuration(_globalTimeLimit);
                    _globalTimer.ResetTimer();
                }

                if (_battleTimer != null && _battleTimeLimit > 0)
                {
                    _battleTimer.SetTimeDuration(_battleTimeLimit);
                    _battleTimer.ResetTimer();
                }
                _state = State.PENDING_RESULT;
            }
        }

        public void Load()
        {
            WinConditionSaveData saveData = (WinConditionSaveData) DataSerializer.Load<SaveObjectData>(WinConditionSaveData.KEY);
            _saveData = saveData;
            if (saveData != null)
            {
                _globalTimer?.SetTimeDuration(_globalTimeLimit);
                _globalTimer?.SetTimeRemaining(saveData.GlobalTimeRemaining);
            }
        }

        public void StartNewBattle()
        {
            if(ChallengeManager.ChallengeActive && ChallengeManager.CurrentChallenge == _currentChallenge)
            {
                if (_battleTimer != null && _battleTimeLimit > 0)
                {
                    _battleTimer.ResetTimer();
                    _battleTimer.gameObject.SetActive(true);
                }
                if (_globalTimer != null && _globalTimeLimit > 0)
                {
                    _globalTimer.gameObject.SetActive(true);
                }

                _battleController = Resources.FindObjectsOfTypeAll<BattleController>().FirstOrDefault();
                _state = State.PENDING_RESULT;
                _battleActive = true;
            }
        }

        public void ShotFired(Vector2 aimVector)
        {
            if (_battleActive)
            {
                _globalTimer?.SetActive(true);
                _battleTimer?.SetActive(true);
            }
        }

        public void BattleEnded()
        {
            _globalTimer?.SetActive(false);
            _battleTimer?.SetActive(false);
            _battleActive = false;
        }

        public void TriggerFailure()
        {
            PlayerHealthController healthController = Resources.FindObjectsOfTypeAll<PlayerHealthController>().FirstOrDefault();
            if(healthController != null)
            {
                healthController.DealUnblockableDamage(healthController.MaxHealth);
                _state = State.DEFEAT_TRIGGERED;
                BattleEnded();
            }
        }

        public void TriggerBattleVictory()
        {
            if(_battleController != null)
            {
                _battleController.TriggerVictory();
                _state = State.WIN_TRIGGERED;
                BattleEnded();
            }
        }

        public void Save()
        {
            if(ChallengeManager.ChallengeActive && _winData != null)
            {
                float remainingTime = _globalTimer != null ? _globalTimer.GetRemainingTime() : -1;
                WinConditionSaveData save = new WinConditionSaveData(remainingTime);
                save.Save();
            }
        }

        public static void LateOnSceneLoaded(String sceneName, bool firstLoad)
        {
            if (sceneName == SceneLoader.PostMainMenu)
            {
                Instance.StartNewGame();
            }

            if (sceneName == SceneLoader.Battle)
            {
                Instance.StartNewBattle();
            } else
            {
                if (Instance._battleTimer != null )
                {
                    Instance._battleTimer?.gameObject.SetActive(false);
                    Instance._globalTimer?.gameObject.SetActive(false);

                }
            }
        }

        [HarmonyPatch(typeof(BattleController), nameof(BattleController.TriggerVictory))]
        public static class EndTimerOnBattleEnd
        {
            public static void Prefix()
            {
                Instance.BattleEnded();
            }
        }

        [HarmonyPatch(typeof(MapController), nameof(MapController.SaveData))]
        public static class SaveData
        {
            public static void Prefix()
            {
                Instance?.Save();
            }
        }

        [HarmonyPatch(typeof(MapController), nameof(MapController.RebuildDataFromMapControllerSave))]
        public static class LoadData
        {
            public static void Prefix()
            {
                Instance?.Load();
            }
        }
    }

    public class WinConditionSaveData : SaveObjectData
    {
        public const String KEY = "CustomChallenges.WinCondition";

        [SerializeField]
        private float _globalTimeRemaining;

        public override string Name => KEY;

        public WinConditionSaveData(float globalTimeRemaining = -1f) : base(true)
        {
            _globalTimeRemaining = globalTimeRemaining;
        }

        public float GlobalTimeRemaining => _globalTimeRemaining;

    }
}
