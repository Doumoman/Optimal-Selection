using UnityEngine;
using Yarn.Unity;

public class ChapterSceneBinder : MonoBehaviour
{
    [SerializeField] private Transform chapterRoot;
    [SerializeField] private Transform mapRoot;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Transform chapterUIRoot;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private ChapterManager chapterManager;
    [SerializeField] private DialogueRunner _runner;
    

    private void Start()
    {
        Debug.Log("[ChapterSceneBinder] RegisterScene 호출");
        ChapterSceneRefs _chapterSceneRefs = new ChapterSceneRefs
        {
            chapterRoot = chapterRoot,
            mapRoot = mapRoot,
            playerController = playerController,
            chapterUIRoot = chapterUIRoot,
            mainCamera = mainCamera,
            chapterManager = chapterManager
        };

        SingletonManagers.Map.RegisterScene(_chapterSceneRefs);
        SingletonManagers.Story.RegisterRunner(_runner);
        chapterManager.RegisterScene(_chapterSceneRefs);
    }
}
