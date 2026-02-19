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
    public event Action<Vector2> OnInput;    // UI 이동
    public event Action OnSubmitPressed;     // 확인 (Enter)
    public event Action OnCancelPressed;     // 취소 (Esc)

    public Vector2 MoveDirection => _controls?.GamePlay.Move.ReadValue<Vector2>() ?? Vector2.zero;

    public void Init()
    {
        if (_init) return;
        _init = true;

        _controls = new GameControls();

        _controls.GamePlay.Menu.performed += HandleMenuPerformed;
        _controls.GamePlay.Sneak.performed += HandleSneakPerformed;
        _controls.GamePlay.Move.performed += HandleMovePerformed;
        _controls.GamePlay.Move.canceled += HandleMoveCanceled;

        _controls.UI.Input.performed += HandleUIInputPerformed;
        _controls.UI.Submit.performed += HandleSubmitPerformed;
        _controls.UI.Cancel.performed += HandleCancelPerformed;

        // 초기에는 플레이어 활성화, UI 비활성화
        _controls?.Enable();
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
        if (_controls == null) return;

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
        OnMove = null;
        OnSneakPressed = null;
        OnInput = null;
        OnSubmitPressed = null;
        OnCancelPressed = null;
    }

    public void OnDestroy()
    {
        if (_controls != null)
        {
            // 💡 명시적 메서드로 완벽하게 구독 해제
            _controls.GamePlay.Menu.performed -= HandleMenuPerformed;
            _controls.GamePlay.Sneak.performed -= HandleSneakPerformed;
            _controls.GamePlay.Move.performed -= HandleMovePerformed;
            _controls.GamePlay.Move.canceled -= HandleMoveCanceled;

            _controls.UI.Input.performed -= HandleUIInputPerformed;
            _controls.UI.Submit.performed -= HandleSubmitPerformed;
            _controls.UI.Cancel.performed -= HandleCancelPerformed;

            // 비활성화 및 메모리 해제
            _controls.Disable();
            _controls.Dispose();
            _controls = null;
        }

        Clear();
        _init = false;
    }

    #region Input Action Handlers (명시적 콜백 메서드들)

    private void HandleMenuPerformed(InputAction.CallbackContext context) => OnMenuPressed?.Invoke();
    private void HandleSneakPerformed(InputAction.CallbackContext context) => OnSneakPressed?.Invoke();
    private void HandleMovePerformed(InputAction.CallbackContext context) => OnMove?.Invoke(context.ReadValue<Vector2>());
    private void HandleMoveCanceled(InputAction.CallbackContext context) => OnMove?.Invoke(Vector2.zero);

    private void HandleUIInputPerformed(InputAction.CallbackContext context) => OnInput?.Invoke(context.ReadValue<Vector2>());
    private void HandleSubmitPerformed(InputAction.CallbackContext context) => OnSubmitPressed?.Invoke();
    private void HandleCancelPerformed(InputAction.CallbackContext context) => OnCancelPressed?.Invoke();

    #endregion
}