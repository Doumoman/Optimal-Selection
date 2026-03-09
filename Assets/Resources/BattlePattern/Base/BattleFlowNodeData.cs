using System;
using UnityEngine;

[Serializable]
public class BattleFlowNodeData
{
    public string id;
    public BattleFlowNodeType nodeType;
    public string nextId;

    public string dialogueKey;

    public AttackPatternSO pattern;

    public float waitTime;

    public BattleBranchConditionType conditionType;
    public int compareValue;
    public string trueNextId;
    public string falseNextId;

    public string eventKey;
}