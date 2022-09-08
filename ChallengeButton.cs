using PeglinUI.MainMenu;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;
using ProLib.Extensions;
using I2.Loc;

namespace CustomChallenges
{
    [RequireComponent(typeof(Button))]
    public class ChallengeButton : MonoBehaviour
    {
        protected TextMeshProUGUI _text;
        protected Button _button;
        protected Challenge _challenge;
        protected RectTransform _rect;

        public virtual void Start()
        {
            if(_challenge == null)
            {
                Plugin.Log.LogError("Challenge Button created with no challenge! Deleting");
                Destroy(this);
                return;
            }

            _text = gameObject.GetComponentInChildren<TextMeshProUGUI>();

            _rect = _text.transform as RectTransform;

            _rect.sizeDelta = new Vector2(175, 24);

            _button = gameObject.GetComponent<Button>();
            _button.onClick.AddListener(() => OnClick());
            _text.text = _challenge.Name;
            _text.enableWordWrapping = false;

            if ( _challenge.ContainsKey(Properties.LOCALIZATION_NAME) || (_challenge.TryGetEntry<bool>(Properties.USE_EXTERNAL_LOCALIZATION, out bool external) && external))
            {
                Localize localize = _text.gameObject.AddComponent<Localize>();
                localize.SetTerm($"Challenges/{_challenge.Id}_name", "Assets/Silver SDF-lfs");
            }

            bool allowCruciball = _challenge.TryGetEntry<bool>(Properties.ALLOW_CRUCIBALL, out bool cruciball) && cruciball;
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

        private const float MAX_WIDTH = 200;

        public void Update()
        {
            if(_rect != null)
            {
                float width = _text.GetPreferredValues().x;
                float targetWidth = Mathf.Clamp(width, 20, MAX_WIDTH);
                _rect.offsetMax = new Vector2(_rect.offsetMin.x + targetWidth, _rect.offsetMax.y) ;
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

            GameObject.Find("ChallengePopup")?.SetActive(false);
            CustomButton = Camera.main.gameObject.FindChild(
                "Character+CruciballCanvas",
                "CustomStartButton"
                );
            
            if(CustomButton != null)
            {
                CustomButton.SetActive(false);
            }

            GameObject panel = Camera.main.gameObject.FindChild(
                "Character+CruciballCanvas",
                "ClassDetailsPanel"
                );

            SetTitleText(panel.FindChild("ClassTitle"));
            SetDescriptionText(panel.FindChild("ClassDescription"));
            SetFooterText(panel.FindChild("MoreClassesSoon!"));

            WinConditionManager.Instance.LoadChallenge(_challenge);
        }

        public void SetTitleText(GameObject title)
        {
            title.GetComponent<TextMeshProUGUI>().text = _challenge.Name;
            Localize localize = title.GetComponent<Localize>();

            if (_challenge.ContainsKey(Properties.LOCALIZATION_NAME) || (_challenge.TryGetEntry<bool>(Properties.USE_EXTERNAL_LOCALIZATION, out bool external) && external))
            {
                localize.SetTerm($"Challenges/{_challenge.Id}_name");
                localize.enabled = true;
            }
            else
            {
                localize.enabled = false;
            }
        }

        public void SetDescriptionText(GameObject description)
        {
            description.GetComponent<TextMeshProUGUI>().text = _challenge.Description ?? "";
            Localize localize = description.GetComponent<Localize>();

            if (_challenge.ContainsKey(Properties.LOCALIZATION_DESCRIPTION) || (_challenge.TryGetEntry<bool>(Properties.USE_EXTERNAL_LOCALIZATION, out bool external) && external))
            {
                localize.SetTerm($"Challenges/{_challenge.Id}_desc");
                localize.enabled = true;
            }
            else
            {
                localize.enabled = false;
            }
        }

        public void SetFooterText(GameObject footer)
        {
            Localize localize = footer.GetComponent<Localize>();
            TextMeshProUGUI text = footer.GetComponent<TextMeshProUGUI>();
            if (_challenge.TryGetEntry<bool>(Properties.ALLOW_CRUCIBALL, out bool cruciball) && cruciball)
            {
                localize.SetTerm($"Menu/CruciballAllowed");
                localize.enabled = true;
                text.enabled = true;
            }
            else
            {
                localize.enabled = false;
                text.enabled = false;
            }
        }

        public void SetChallengeData(Challenge data)
        {
            _challenge = data;
        }


    }
}
