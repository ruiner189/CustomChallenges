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
using I2.Loc;

namespace CustomChallenges
{
    [RequireComponent(typeof(Button))]
    public class WeeklyChallengeButton : ChallengeButton
    {
        public override void Start()
        {
            _button = gameObject.GetComponent<Button>();
            _button.onClick.AddListener(() => OnClick());
        }
    }
}
