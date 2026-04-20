using System;
using System.Collections;
using UnityEngine;
using Yarn.Unity;

public class StoryManager : IManager
{
    private DialogueRunner _runner;
    private bool _init = false;

    public void Init()
    {
        if (_init) return;
        _init = true;

        RegisterCommands();
    }

    public void Clear()
    {
        StopStory();
    }

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

        _runner.AddCommandHandler("fade_in",    (System.Func<float, IEnumerator>)  CmdFadeIn);
        _runner.AddCommandHandler("fade_out",   (System.Func<float, IEnumerator>)  CmdFadeOut);

        // TODO: 게임 전용 커맨드 등록
        // _runner.AddCommandHandler("커맨드명", (Action<파라미터타입>) CmdXxx);
    }

    private void UnregisterCommands()
    {
        if (_runner == null) return;

        _runner.RemoveCommandHandler("fade_in");
        _runner.RemoveCommandHandler("fade_out");
    }

    /// <<fade_in 1.0>>  — 화면 페이드 인
    private IEnumerator CmdFadeIn(float duration)
    {
        bool done = false;
        SingletonManagers.UI.GetFade().FadeIn(duration, () => done = true);
        yield return new WaitUntil(() => done);
    }

    /// <<fade_out 1.0>>  — 화면 페이드 아웃
    private IEnumerator CmdFadeOut(float duration)
    {
        bool done = false;
        SingletonManagers.UI.GetFade().FadeOut(duration, () => done = true);
        yield return new WaitUntil(() => done);
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
