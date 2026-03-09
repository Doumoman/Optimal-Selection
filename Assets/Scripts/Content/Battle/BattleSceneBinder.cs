using UnityEngine;

public class BattleSceneBinder : MonoBehaviour
{
    [SerializeField] private GameObject battleRoot;
    [SerializeField] private GameObject battleUIRoot;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private UI_BattleEnemySpeech enemySpeechUI;
    [SerializeField] private PatternRunner patternRunner;

    private void Awake()
    {
        BattleSceneRefs refs = new BattleSceneRefs
        {
            battleRoot = battleRoot,
            battleUIRoot = battleUIRoot,
            playerController = playerController,
            enemySpeechUI = enemySpeechUI,
            patternRunner = patternRunner
        };
        Managers.Battle.RegisterScene(refs);
    }

    private void Update()
    {
        Managers.Battle.Tick();
    }
}