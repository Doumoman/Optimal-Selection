using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : Singleton<InputManager>, IManager
{
    private GameControls _controls;

    public event Action OnMenuPressed;

    public Vector2 MoveDirection => _controls?.Player.Move.ReadValue<Vector2>() ?? Vector2.zero;

    private bool _init = false;
    public void Init()
    {
        if(_init) return;
        _init = true;

        _controls = new GameControls();

        // 이벤트 바인딩, 여기에 이벤트 추가
        _controls.Player.Menu.performed += OnMenuPerformed;
    }

    protected override void Awake()
    {
        base.Awake();
    }

    private void OnEnable()
    {
        _controls?.Enable();
    }

    private void OnDisable()
    {
        _controls?.Disable();
    }

    protected override void OnDestroy()
    {
        if (_controls != null)
        {
            _controls.Player.Menu.performed -= OnMenuPerformed;
            _controls.Dispose();
        }

        base.OnDestroy(); // 부모의 종료 처리(플래그 설정 등) 실행
    }

    private void OnMenuPerformed(InputAction.CallbackContext context)
    {
        OnMenuPressed?.Invoke();
    }
}