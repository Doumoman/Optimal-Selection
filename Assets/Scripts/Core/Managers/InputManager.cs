using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : IManager
{
    private bool _init = false;
    private GameControls _controls;

    // 이벤트 선언
    public event Action OnMenuPressed;       // 메뉴 열기
    public event Action<Vector2> OnMove;     // 플레이어 이동
    public event Action OnSneakPressed;      // 플레이어 숙이기
    public event Action<Vector2> OnInput; // UI 이동
    public event Action OnSubmitPressed;     // 확인 (Enter)
    public event Action OnCancelPressed;     // 취소 (Esc)


    public Vector2 MoveDirection => _controls?.GamePlay.Move.ReadValue<Vector2>() ?? Vector2.zero;

    public void Init()
    {
        if (_init) return;
        _init = true;

        _controls = new GameControls();

        // 이벤트 바인딩, 여기에 이벤트 추가
        _controls.GamePlay.Menu.performed += ctv => OnMenuPressed?.Invoke();
        _controls.GamePlay.Sneak.performed += ctv => OnSneakPressed?.Invoke();
        _controls.GamePlay.Move.performed += ctx => OnMove?.Invoke(ctx.ReadValue<Vector2>());
        _controls.GamePlay.Move.canceled += ctx => OnMove?.Invoke(Vector2.zero);
        _controls.UI.Input.performed += ctx => OnInput?.Invoke(ctx.ReadValue<Vector2>());
        _controls.UI.Submit.performed += ctx => OnSubmitPressed?.Invoke();
        _controls.UI.Cancel.performed += ctx => OnCancelPressed?.Invoke();

        // 초기에는 플레이어 활성화, UI 비활성화
        _controls?.Enable();
        _controls.UI.Disable();

        SetInputMode(false);
    }

    // 외부에서 전체 입력을 껐다 켰다 할 수 있게함
    public void SetInput(bool active)
    {
        if (_controls == null) return;

        if (active)
            _controls.Enable();
        else
            _controls.Disable();
    }

    // 모드 전환 기능 (캐릭터 조작 <-> UI 조작)
    public void SetInputMode(bool isUI)
    {
        if (isUI)
        {
            _controls.GamePlay.Disable();
            _controls.UI.Enable();
        }
        else
        {
            _controls.UI.Disable();
            _controls.GamePlay.Enable();
        }
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
            _controls.GamePlay.Menu.performed -= ctv => OnMenuPressed?.Invoke();
            _controls.GamePlay.Sneak.performed -= ctv => OnSneakPressed?.Invoke();
            _controls.GamePlay.Move.performed -= ctx => OnMove?.Invoke(ctx.ReadValue<Vector2>());
            _controls.UI.Input.performed -= ctx => OnInput?.Invoke(ctx.ReadValue<Vector2>());
            _controls.UI.Submit.performed -= ctx => OnSubmitPressed?.Invoke();
            _controls.UI.Cancel.performed -= ctx => OnCancelPressed?.Invoke();

            // 비활성화 및 메모리 해제
            _controls.Disable();
            _controls.Dispose();
            _controls = null;
        }

        OnMenuPressed = null;
        _init = false;
    }
}