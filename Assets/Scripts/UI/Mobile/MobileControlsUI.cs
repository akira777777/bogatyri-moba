using TMPro;
using UnityEngine;
using UnityEngine.UI;
using BogatyriMoba.Core;
using BogatyriMoba.Localization;

namespace BogatyriMoba.UI.Mobile
{
    public class MobileControlsUI : MonoBehaviour
    {
        [SerializeField] private VirtualJoystick moveJoystick;
        [SerializeField] private VirtualJoystick aimJoystick;
        [SerializeField] private Button attackButton;
        [SerializeField] private Button superButton;
        [SerializeField] private Button gadgetButton;
        [SerializeField] private TextMeshProUGUI attackLabel;
        [SerializeField] private TextMeshProUGUI superLabel;
        [SerializeField] private TextMeshProUGUI gadgetLabel;

        private PlayerInput _input;

        private void Awake()
        {
            RefreshLabels();
            bool touchPlatform =
#if UNITY_ANDROID || UNITY_IOS
                true;
#else
                false;
#endif
            gameObject.SetActive(touchPlatform);
        }

        public void BindPlayer(PlayerInput input)
        {
            Unbind();

            _input = input;
            if (_input == null) return;

            _input.SetInputSource(PlayerInput.InputSource.Touch);

            if (moveJoystick != null)
                moveJoystick.OnDirectionChanged += OnMove;
            if (aimJoystick != null)
                aimJoystick.OnDirectionChanged += OnAim;

            if (attackButton != null)
                attackButton.onClick.AddListener(OnAttack);
            if (superButton != null)
                superButton.onClick.AddListener(OnSuper);
            if (gadgetButton != null)
                gadgetButton.onClick.AddListener(OnGadget);
        }

        public void Unbind()
        {
            if (moveJoystick != null)
                moveJoystick.OnDirectionChanged -= OnMove;
            if (aimJoystick != null)
                aimJoystick.OnDirectionChanged -= OnAim;

            if (attackButton != null)
                attackButton.onClick.RemoveListener(OnAttack);
            if (superButton != null)
                superButton.onClick.RemoveListener(OnSuper);
            if (gadgetButton != null)
                gadgetButton.onClick.RemoveListener(OnGadget);

            _input = null;
        }

        private void RefreshLabels()
        {
            if (attackLabel != null)
                attackLabel.text = LocalizedUI.Get("controls.attack");
            if (superLabel != null)
                superLabel.text = LocalizedUI.Get("controls.super");
            if (gadgetLabel != null)
                gadgetLabel.text = LocalizedUI.Get("controls.gadget");
        }

        private void OnMove(Vector2 dir) => _input?.SetMoveInput(dir);
        private void OnAim(Vector2 dir) => _input?.SetAimInput(dir);
        private void OnAttack() => _input?.OnAttackButtonDown();
        private void OnSuper() => _input?.OnSuperButtonDown();
        private void OnGadget() => _input?.OnGadgetButtonDown();

        private void OnDestroy() => Unbind();
    }
}
