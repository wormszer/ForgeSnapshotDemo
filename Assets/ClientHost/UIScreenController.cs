using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIScreenController : SceneSingleton<UIScreenController>
{
    [SerializeField]
    private Button _LeftButton;

    [SerializeField]
    private Button _RightButton;

    [SerializeField]
    private Button _JumpButton;

    [SerializeField]
    private UIScreenJoystick _Joystick;

    private bool _LeftButtonState = false;
    public bool LeftButton { get { var tmp = _LeftButtonState; _LeftButtonState = false; return tmp; } protected set { _LeftButtonState = value; } }

    private bool _RightButtonState = false;
    public bool RightButton { get { var tmp = _RightButtonState; _RightButtonState = false; return tmp; } protected set { _RightButtonState = value; } }

    private bool _JumpButtonState = false;
    public bool JumpButton { get { var tmp = _JumpButtonState; _JumpButtonState = false; return tmp; } protected set { _JumpButtonState = value; } }

    public Vector2 Joystick { get { return _Joystick.Direction; } }

    void Start ()
    {
#if !UNITY_IOS && !UNITY_ANDROID
        gameObject.SetActive(false);
#endif
        _LeftButton.onClick.AddListener(OnLeftButtonClicked);
        _RightButton.onClick.AddListener(OnRightButtonClicked);
        _JumpButton.onClick.AddListener(OnJumpButtonClicked);
    }

    private void OnLeftButtonClicked()
    {
        LeftButton = true;
    }

    private void OnRightButtonClicked()
    {
        RightButton = true;
    }

    private void OnJumpButtonClicked()
    {
        JumpButton = true;
    }
}
