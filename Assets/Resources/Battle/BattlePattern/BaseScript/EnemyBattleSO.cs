using UnityEngine;

[CreateAssetMenu(menuName = "Battle/Enemy Battle Data")]
public class EnemyBattleSO : ScriptableObject
{
    public string enemyId;
    public string displayName;

    [Header("Base Stats")]
    public int maxHP = 50;
    public int attack = 5;
    public int defense = 1;

    [Header("Prefabs")]
    public GameObject battlePrefab;

    [Header("Flow")]
    public BattleFlowSO defaultFlow;

    [Header("Default Dialogue Keys")]
    public string introKey;
    public string lowHpKey;
    public string mercyAvailableKey;
}