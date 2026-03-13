using UnityEngine;

[System.Serializable]
public class BattleSceneRefs
{
    public GameObject battleRoot;
    public GameObject battleUIRoot;
    public PlayerController playerController;
    public UI_BattleEnemySpeech enemySpeechUI;
    public PatternRunner patternRunner;
    public BattleFieldView battleFieldView;

    public Transform enemyAnchor;
    public Transform playerSoulSpawn;
    public Transform bulletRoot;
    public GameObject battleBackground;
}