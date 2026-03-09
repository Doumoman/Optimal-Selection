using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Battle/Battle Flow")]
public class BattleFlowSO : ScriptableObject
{
    public List<BattleFlowNodeData> nodes = new List<BattleFlowNodeData>();
    public string startNodeId;
}