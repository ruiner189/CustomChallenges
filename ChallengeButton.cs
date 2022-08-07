using PeglinUI.MainMenu;
using PeglinUI.MainMenu.Cruciball;
using Saving;
using System;
using System.Collections.Generic;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;
using ProLib.Extensions;

namespace CustomChallenges
{
    [RequireComponent(typeof(Button))]
    public class ChallengeButton : MonoBehaviour
    {
        private TextMeshProUGUI _text;
        private Button _button;
        private Challenge _challenge;

        public void Start()
        {
            _text = gameObject.GetComponentInChildren<TextMeshProUGUI>();
            _button = gameObject.GetComponent<Button>();
            _button.onClick.AddListener(() => OnClick());
            _text.text = _challenge.Name;

            bool allowCruciball = _challenge.TryGetEntry<bool>(Keys.ALLOW_CRUCIBALL, out bool cruciball) && cruciball;
            bool maxCruciball = allowCruciball && ChallengeManager.IsChallengeMaxCruciball(_challenge);
            bool firstVictory = ChallengeManager.IsChallengeCompleted(_challenge.Id);

            if (maxCruciball || (firstVictory && !allowCruciball))
            {
                Color orange = new Color(242f/256f, 140f/256f, 40f/256f);
                _text.color = orange;
                ButtonHandleHover hover = GetComponent<ButtonHandleHover>();
                if (hover != null)
                    hover._origTextColor = orange;
            }
            else if (firstVictory && allowCruciball)
            {
                _text.color = Color.yellow;
                ButtonHandleHover hover = GetComponent<ButtonHandleHover>();
                if(hover != null)
                    hover._origTextColor = Color.yellow;
            }
        }

        public static GameObject CustomButton;

        public void OnClick()
        {
            ChallengeManager.CurrentChallenge = _challenge;
            GameObject button = GameObject.Find("PlayButton");
            PlayButton playButton = button.GetComponent<PlayButton>();
            playButton._mapControllerSaveData = null;

            SaveFileManager.ReloadSaveFile();

            playButton.OnEnable();
            playButton.ButtonClicked();

            GameObject.Find("ChallengePopup").SetActive(false);
            CustomButton = Camera.main.gameObject.FindChild(
                "Character+CruciballCanvas",
                "CustomStartButton"
                );
            
            if(CustomButton != null)
            {
                CustomButton.SetActive(false);
            }
        }

        public void SetChallengeData(Challenge data)
        {
            _challenge = data;
        }


    }
}
