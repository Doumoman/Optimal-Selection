using UnityEngine;

public class Managers : Singleton<Managers>, IManager
{
    #region Core
    // 기반 관련
    private ResourceManager _resource = new ResourceManager();
    private UIManager _ui = new UIManager();
    private InputManager _input = new InputManager();
    #endregion

    #region Contents
    // 게임 로직 관련

    #endregion

    public static ResourceManager Resource => Instance._resource;
    public static UIManager UI => Instance._ui;
    public static InputManager Input => Instance._input;

    protected override void Awake()
    {
        base.Awake();
        Init(); // 가장 먼저 다른 Manager들 초기화
    }
    private void Update()
    {

    }

    private bool _init = false;
    public void Init()
    {
        if (_init) return;
        _init = true;

        // 매니저 인스턴스 생성 및 초기화 순서 제어
        _resource.Init();
        _input.Init();
        _ui.Init();
    }

    public static void Clear()
    {
        UI.Clear();
        Input.Clear();
        Resource.Clear();
    }

    protected override void OnDestroy() 
    {
        if(_resource != null) _resource.OnDestroy();
        if (_input != null) _input.OnDestroy();
        if (_ui != null) _ui.OnDestroy();

        base.OnDestroy();
    }
}