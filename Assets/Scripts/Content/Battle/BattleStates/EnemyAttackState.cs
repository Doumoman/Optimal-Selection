using System.Collections.Generic;
using UnityEngine;

public class EnemyAttackState : BattleStateBase
{
    public override BattleStateType StateType => BattleStateType.EnemyAttack;

    private Dictionary<string, BattleFlowNodeData> _nodes;
    private BattleFlowNodeData _currentNode;

    private float _timer;

    public override void Enter(BattleContext context)
    {
        base.Enter(context);

        Debug.Log("[EnemyAttackState] 적 공격 시작");

        BattleFlowSO flow = context.CurrentFlow;
        if (flow == null)
        {
            Debug.LogError("[EnemyAttackState] CurrentFlow가 없습니다.");
            ChangeState(context, BattleStateType.TurnEnd);
            return;
        }

        if (flow.nodes == null || flow.nodes.Count == 0)
        {
            Debug.LogError($"[EnemyAttackState] flow.nodes가 비어 있습니다. flow={flow.name}");
            ChangeState(context, BattleStateType.TurnEnd);
            return;
        }

        if (string.IsNullOrEmpty(flow.startNodeId))
        {
            Debug.LogError($"[EnemyAttackState] startNodeId가 비어 있습니다. flow={flow.name}");
            ChangeState(context, BattleStateType.TurnEnd);
            return;
        }

        _nodes = new Dictionary<string, BattleFlowNodeData>();

        foreach (var node in flow.nodes)
        {
            if (node == null || string.IsNullOrEmpty(node.id))
            {
                Debug.LogWarning("[EnemyAttackState] id가 비어 있는 노드가 있습니다.");
                continue;
            }

            if (_nodes.ContainsKey(node.id))
            {
                Debug.LogWarning($"[EnemyAttackState] 중복 노드 id: {node.id}");
                continue;
            }

            _nodes.Add(node.id, node);
        }

        if (!_nodes.TryGetValue(flow.startNodeId, out _currentNode))
        {
            Debug.LogError($"[EnemyAttackState] 시작 노드를 찾을 수 없습니다. startNodeId={flow.startNodeId}");
            ChangeState(context, BattleStateType.TurnEnd);
            return;
        }

        ExecuteNode(context);
    }

    public override void Tick(BattleContext context)
    {
        if (_currentNode == null)
            return;

        if (_currentNode.nodeType == BattleFlowNodeType.Wait)
        {
            _timer += Time.deltaTime;

            if (_timer >= _currentNode.waitTime)
                MoveNext(context, _currentNode.nextId);
        }
    }

    void ExecuteNode(BattleContext context)
    {
        switch (_currentNode.nodeType)
        {
            case BattleFlowNodeType.EnemyDialogue:
                context.Manager.ShowEnemySpeechByKey(
                    _currentNode.dialogueKey,
                    () => MoveNext(context, _currentNode.nextId)
                );
                break;

            case BattleFlowNodeType.Pattern:
                if (context.PatternRunner == null)
                {
                    Debug.LogWarning("[EnemyAttackState] PatternRunner가 없습니다.");
                    MoveNext(context, _currentNode.nextId);
                    return;
                }

                context.PatternRunner.Run(
                    _currentNode.pattern,
                    () => MoveNext(context, _currentNode.nextId)
                );
                break;

            case BattleFlowNodeType.SystemDialogue:
                context.Manager.ShowSystemDialogue(
                    _currentNode.dialogueKey,
                    (_) => MoveNext(context, _currentNode.nextId)
                );
                break;

            case BattleFlowNodeType.ConditionBranch:
                HandleBranch(context);
                break;

            case BattleFlowNodeType.Wait:
                _timer = 0f;
                break;

            case BattleFlowNodeType.Event:
                Debug.Log($"[EnemyAttackState] Event Node: {_currentNode.eventKey}");
                MoveNext(context, _currentNode.nextId);
                break;

                MoveNext(context, _currentNode.nextId);
                break;

            case BattleFlowNodeType.EndTurn:
                ChangeState(context, BattleStateType.TurnEnd);
                break;
        }
    }

    void HandleBranch(BattleContext context)
    {
        bool result = false;

        switch (_currentNode.conditionType)
        {
            case BattleBranchConditionType.EnemyHpLessOrEqual:
                result = context.EnemyCurrentHP <= _currentNode.compareValue;
                break;

            case BattleBranchConditionType.PlayerHpLessOrEqual:
                result = context.PlayerCurrentHP <= _currentNode.compareValue;
                break;

            case BattleBranchConditionType.EvilGreaterOrEqual:
                result = context.CurrentEvilGauge >= _currentNode.compareValue;
                break;

            case BattleBranchConditionType.TurnGreaterOrEqual:
                result = context.TurnIndex >= _currentNode.compareValue;
                break;
        }

        string next = result ? _currentNode.trueNextId : _currentNode.falseNextId;

        MoveNext(context, next);
    }

    void MoveNext(BattleContext context, string nextId)
    {
        if (string.IsNullOrEmpty(nextId))
        {
            ChangeState(context, BattleStateType.TurnEnd);
            return;
        }

        if (_nodes == null || !_nodes.TryGetValue(nextId, out _currentNode))
        {
            Debug.LogError($"[EnemyAttackState] 다음 노드를 찾을 수 없습니다. nextId={nextId}");
            ChangeState(context, BattleStateType.TurnEnd);
            return;
        }

        ExecuteNode(context);
    }
    public override void Exit(BattleContext context)
    {
        Debug.Log("[EnemyAttackState] 적 공격 종료");

        _nodes = null;
        _currentNode = null;

        base.Exit(context);
    }
}