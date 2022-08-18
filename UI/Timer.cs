using ProLib.Attributes;
using ProLib.Extensions;
using ProLib.Loaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CustomChallenges.UI
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    [SceneModifier]
    public class Timer : MonoBehaviour
    {
        private float _secondsRemaining;
        private float _duration;
        private TextMeshProUGUI _text;
        public bool Complete => _secondsRemaining <= 0;
        public bool _active = false;

        public float warningThreshold = 30;
        public float criticalThreshold = 10;

        public Color warningColor = new Color(1, 165f / 255f, 0);
        public Color criticalColor = Color.red;
        public Color defaultColor = Color.white;

        public void Awake()
        {
            _text = gameObject.GetComponent<TextMeshProUGUI>();
            _text.fontSize = 16;
            _text.font = Plugin.GetFont(0, "ChevyRay - Bird Seed SDF.lfs");
        }

        public Color GetColor()
        {
            if (_secondsRemaining <= criticalThreshold)
                return criticalColor;
            else if (_secondsRemaining <= warningThreshold)
                return warningColor;
            
           return defaultColor;
        }

        public void Update()
        {
            if (_active && !Complete && SceneManager.GetActiveScene().name == SceneLoader.Battle)
            {
                _secondsRemaining -= Time.deltaTime;
                if (_secondsRemaining < 0) _secondsRemaining = 0;
            }

            UpdateDisplay();
        }

        public void UpdateDisplay()
        {
            _text.color = GetColor();
            float minutes = Mathf.FloorToInt(_secondsRemaining / 60);
            float seconds = Mathf.FloorToInt(_secondsRemaining % 60);

            String format = String.Format("{00:00}{1:00}", minutes, seconds);

            _text.text = $"{format[0]}{format[1]}:{format[2]}{format[3]}";
        }

        public void SetTimeRemaining(float seconds)
        {
            _secondsRemaining = seconds;
        }

        public void SetTimeDuration(float seconds)
        {
            _duration = seconds;
        }

        public float GetRemainingTime()
        {
            return _secondsRemaining;
        }

        public void ResetTimer()
        {
            _secondsRemaining = _duration;
            _active = false;
        }

        public void SetActive(bool active)
        {
            _active = active;
        }

        public static Timer CreateTimer(Transform parent = null)
        {
            GameObject obj = new GameObject("Timer");
            obj.AddComponent<TextMeshProUGUI>();
            Timer timer = obj.AddComponent<Timer>();

            if(parent != null)
            {
                obj.transform.position = new Vector3(0, 0, 90);
                obj.transform.SetParent(parent, true);
                obj.transform.localScale = new Vector3(1, 1, 1);
            }
            return timer;
        }

        public static void OnSceneLoaded(String scene, bool firstLoad)
        {
            if(scene == SceneLoader.PostMainMenu)
            {
                GameObject ui = GameObject.FindGameObjectWithTag("PersistentUI");
                GameObject right = ui.FindChild("RightUI");
                if(right != null)
                {
                    WinConditionManager.Instance.StartNewGame();
                    return;
                }

                right = new GameObject("RightUI", typeof(CanvasRenderer));
                right.transform.position = new Vector3(0, 0, 0);
                right.layer = LayerMask.NameToLayer("UI");
                right.transform.SetParent(ui.transform, true);
                right.transform.localScale = new Vector3(1, 1, 1);
                right.transform.position = new Vector3(12.3f, -8f, 180);

                GameObject container = new GameObject("TimerContainer");
                container.transform.position = new Vector3(0, 0, 0);
                container.transform.SetParent(right.transform, true);
                VerticalLayoutGroup layout = container.AddComponent<VerticalLayoutGroup>();
                layout.childForceExpandHeight = false;
                layout.childForceExpandWidth = false;
                layout.spacing = 0f;
                layout.childAlignment = TextAnchor.UpperRight;
                container.transform.localScale = new Vector3(1, 1, 1);
                container.transform.localPosition = new Vector3(0, 0, 180);
                ((RectTransform)container.transform).sizeDelta = new Vector2(60, 40);


                WinConditionManager.Instance.SetGlobalTimer(CreateTimer(container.transform));
                WinConditionManager.Instance.SetBattleTimer(CreateTimer(container.transform));
                WinConditionManager.Instance.StartNewGame();
            }
        }
    }
}
