using UnityEngine;

public class Managers : Singleton<Managers>, IManager
{
    #region Core
    // 기반 관련
    private ResourceManager _resource;
    private UIManager _ui;
    private InputManager _input;
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
        Init();
    }

    private bool _init = false;
    public void Init()
    {
        if( _init ) return;
        _init = true;

        // 인스턴스 할당
        _resource = new ResourceManager();
        _ui = UIManager.Instance;
        _input = InputManager.Instance;

        // 계층 구조 정리
        _ui.transform.SetParent(transform);
        _input.transform.SetParent(transform);

        // 매니저 초기화 순서 제어
        _resource.Init();
        _ui.Init();
        _input.Init();
    }

    // 씬 이동 시 데이터 정리
    public static void Clear()
    {
        Resource.Clear();
        UI.Clear();
    }
}