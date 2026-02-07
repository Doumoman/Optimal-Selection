using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : IManager
{
    private bool _init = false;
    private GameControls _controls;

    // 이벤트 선언
    public event Action OnMenuPressed;

    public Vector2 MoveDirection => _controls?.Player.Move.ReadValue<Vector2>() ?? Vector2.zero;

    public void Init()
    {
        if (_init) return;
        _init = true;

        _controls = new GameControls();

        // 이벤트 바인딩, 여기에 이벤트 추가
        _controls.Player.Menu.performed += OnMenuPerformed;

        // 입력 활성화
        _controls?.Enable();
    }

    // 외부에서 입력을 껐다 켰다 할 수 있게함
    public void SetInput(bool active)
    {
        if (_controls == null) return;

        if (active)
            _controls.Enable();
        else
            _controls.Disable();
    }

    public void Clear()
    {
        OnMenuPressed = null;
    }

    public void OnDestroy()
    {
        if (_controls != null)
        {
            // 구독 해제
            _controls.Player.Menu.performed -= OnMenuPerformed;

            // 비활성화 및 메모리 해제
            _controls.Disable();
            _controls.Dispose();
            _controls = null;
        }

        OnMenuPressed = null;
        _init = false;
    }

    private void OnMenuPerformed(InputAction.CallbackContext context)
    {
        OnMenuPressed?.Invoke();
    }
}