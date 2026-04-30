using UnityEngine;

public class BattleSceneBinder : MonoBehaviour
{
    [SerializeField] private GameObject battleRoot;
    [SerializeField] private GameObject battleUIRoot;
    [SerializeField] private PlayerFSM playerFSM;
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
            playerFSM = playerFSM,
            enemySpeechUI = enemySpeechUI,
            patternRunner = patternRunner,
            battleFieldView = battleFieldView
        };
        SingletonManagers.Battle.RegisterScene(refs);
    }

    private void Update()
    {
        SingletonManagers.Battle.Tick();
    }
}