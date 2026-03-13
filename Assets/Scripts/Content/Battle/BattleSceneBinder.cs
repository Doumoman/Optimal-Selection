using UnityEngine;

public class BattleSceneBinder : MonoBehaviour
{
    [SerializeField] private GameObject battleRoot;
    [SerializeField] private GameObject battleUIRoot;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private UI_BattleEnemySpeech enemySpeechUI;
    [SerializeField] private PatternRunner patternRunner;
    [SerializeField] private BattleFieldView battleFieldView;

    private void Awake()
    {
        Debug.Log("[BattleSceneBinder] RegisterScene 호출");
        BattleSceneRefs refs = new BattleSceneRefs
        {
            battleRoot = battleRoot,
            battleUIRoot = battleUIRoot,
            playerController = playerController,
            enemySpeechUI = enemySpeechUI,
            patternRunner = patternRunner,
            battleFieldView = battleFieldView
        };
        Managers.Battle.RegisterScene(refs);
    }

    private void Update()
    {
        Managers.Battle.Tick();
    }
}