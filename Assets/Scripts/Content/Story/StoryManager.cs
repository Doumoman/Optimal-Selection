using System;
using System.Collections;
using UnityEngine;
using Yarn.Unity;

public class StoryManager : IManager
{
    private const string FADE_IN = "fade_in";
    private const string FADE_OUT = "fade_out";
    private const string WAIT_FOR_INTERACTION = "wait_for_interaction";

    private DialogueRunner _runner;
    private bool _init = false;

    private string _waitingInteractionName = String.Empty;
    private bool _isInteractionCompleted = false;

    public void Init()
    {
        if (_init) return;
        _init = true;
    }

    public void Clear() { }
    public void OnDestroy()
    {
        UnregisterCommands();
        _runner = null;
        _init = false;
    }

    /// <summary>
    /// 씬 바인더에서 DialogueRunner를 주입한다.
    /// </summary>
    public void RegisterRunner(DialogueRunner runner)
    {
        if (_runner != null)
            UnregisterCommands();

        _runner = runner;
        RegisterCommands();
    }

    /// <summary>
    /// 특정 스토리 노드를 시작한다.
    /// </summary>
    public async void StartStory(string nodeName, Action onComplete = null)
    {
        if (_runner == null) return;
        if (!_runner.Dialogue.NodeExists(nodeName)) return;

        await _runner.StartDialogue(nodeName);

        if (onComplete != null)
        {
            onComplete?.Invoke();
        }

    }

    public async void StopStory()
    {
        if (_runner != null && _runner.IsDialogueRunning)
            await _runner.Stop();
    }

    public bool IsRunning => _runner != null && _runner.IsDialogueRunning;

    private void RegisterCommands()
    {
        if (_runner == null) return;

        _runner.AddCommandHandler(FADE_IN,    (System.Func<float, IEnumerator>)  CmdFadeIn);
        _runner.AddCommandHandler(FADE_OUT,   (System.Func<float, IEnumerator>)  CmdFadeOut);
        _runner.AddCommandHandler(WAIT_FOR_INTERACTION, (System.Func<string, IEnumerator>) CmdWaitForInteraction);

        // TODO: 게임 전용 커맨드 등록
        // _runner.AddCommandHandler("커맨드명", (Action<파라미터타입>) CmdXxx);
    }

    private void UnregisterCommands()
    {
        if (_runner == null) return;

        _runner.RemoveCommandHandler(FADE_IN);
        _runner.RemoveCommandHandler(FADE_OUT);
        _runner.RemoveCommandHandler(WAIT_FOR_INTERACTION);

        // TODO: 게임 전용 커맨드 해제
        // _runner.RemoveCommandHandler("커맨드 명");
    }

    #region Yarn Spinner에서 사용할 커맨드 함수들
    private IEnumerator CmdFadeIn(float duration) // 화면 페이드 인
    {
        bool done = false;
        SingletonManagers.UI.GetFade().FadeIn(duration, () => done = true);
        yield return new WaitUntil(() => done);
    }

    private IEnumerator CmdFadeOut(float duration) // 화면 페이드 아웃
    {
        bool done = false;
        SingletonManagers.UI.GetFade().FadeOut(duration, () => done = true);
        yield return new WaitUntil(() => done);
    }

    private IEnumerator CmdWaitForInteraction(string interactionName) // 대화 도중 특정 상호작용을 수행할 때 까지 대화 멈춤
    {
        _waitingInteractionName = interactionName;
        _isInteractionCompleted = false;

        yield return new WaitUntil(() => _isInteractionCompleted); // 상호작용이 끝날 때 까지 대기

        _waitingInteractionName = string.Empty;
        _isInteractionCompleted = false;
    }

    #endregion

    /// <summary>
    /// 외부에서 특정 상호작용에 성공하면 이름과 함께 호출
    /// </summary>
    public void NotifyInteractionCompleted(string interactionName)
    {
        // 현재 스토리가 기다리고 있는 상호작용 이름과 일치한다면 
        if (_waitingInteractionName == interactionName)
        {
            _isInteractionCompleted = true;
        }
    }

    public Coroutine RunCoroutine(IEnumerator routine)
    {
        return SingletonManagers.Instance.StartCoroutine(routine);
    }

    public void StopCoroutine(Coroutine coroutine)
    {
        if (coroutine != null)
            SingletonManagers.Instance.StopCoroutine(coroutine);
    }
}
