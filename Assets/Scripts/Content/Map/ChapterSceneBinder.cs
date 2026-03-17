using UnityEngine;

public class ChapterSceneBinder : MonoBehaviour
{
    [SerializeField] private Transform chapterRoot;
    [SerializeField] private Transform mapRoot;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Transform chapterUIRoot;
    [SerializeField] private Camera mainCamera;
    [SerializeField] ChapterManager chapterManager;
    

    private void Awake()
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

        Managers.Map.RegisterScene(_chapterSceneRefs);
        chapterManager.RegisterScene(_chapterSceneRefs);
    }
}
