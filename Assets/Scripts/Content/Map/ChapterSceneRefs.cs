using UnityEngine;
using Yarn.Unity;
[System.Serializable]
public class ChapterSceneRefs
{
    public Transform chapterRoot;
    public Transform mapRoot;
    public PlayerFSM playerFSM;
    public Transform chapterUIRoot;
    public Camera mainCamera;
    public ChapterManager chapterManager;
    public DialogueRunner _runner;
}
