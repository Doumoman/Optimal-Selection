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

        Debug.Log($"[EnemyAttackState] flow={flow.name}, startNodeId={flow.startNodeId}, nodeCount={(flow.nodes != null ? flow.nodes.Count : 0)}");

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
            Debug.Log($"[EnemyAttackState] 노드 등록: id={node.id}, type={node.nodeType}, next={node.nextId}");
        }

        if (!_nodes.TryGetValue(flow.startNodeId, out _currentNode))
        {
            Debug.LogError($"[EnemyAttackState] 시작 노드를 찾을 수 없습니다. startNodeId={flow.startNodeId}");
            ChangeState(context, BattleStateType.TurnEnd);
            return;
        }

        Debug.Log($"[EnemyAttackState] 시작 노드 진입: id={_currentNode.id}, type={_currentNode.nodeType}");
        ExecuteNode(context);
    }

    public override void Tick(BattleContext context)
    {
        if (_currentNode == null)
            return;

        if (_currentNode.nodeType == BattleFlowNodeType.Wait)
        {
            _timer += Time.deltaTime;
            Debug.Log($"[EnemyAttackState] Wait 진행중: node={_currentNode.id}, timer={_timer:F2}/{_currentNode.waitTime:F2}");

            if (_timer >= _currentNode.waitTime)
            {
                Debug.Log($"[EnemyAttackState] Wait 종료 -> next={_currentNode.nextId}");
                MoveNext(context, _currentNode.nextId);
            }
        }
    }

    private void ExecuteNode(BattleContext context)
    {
        if (_currentNode == null)
        {
            Debug.LogError("[EnemyAttackState] ExecuteNode 호출 시 _currentNode가 null입니다.");
            ChangeState(context, BattleStateType.TurnEnd);
            return;
        }

        Debug.Log($"[EnemyAttackState] ExecuteNode: id={_currentNode.id}, type={_currentNode.nodeType}, next={_currentNode.nextId}");

        switch (_currentNode.nodeType)
        {
            case BattleFlowNodeType.EnemyDialogue:
                Debug.Log($"[EnemyAttackState] EnemyDialogue 실행: key={_currentNode.dialogueKey}");
                context.Manager.ShowEnemySpeechByKey(
                    _currentNode.dialogueKey,
                    () => MoveNext(context, _currentNode.nextId)
                );
                break;

            case BattleFlowNodeType.Pattern:
                Debug.Log($"[EnemyAttackState] Pattern 실행: pattern={(_currentNode.pattern != null ? _currentNode.pattern.name : "NULL")}");
                if (context.PatternRunner == null)
                {
                    Debug.LogWarning("[EnemyAttackState] PatternRunner가 없습니다.");
                    MoveNext(context, _currentNode.nextId);
                    return;
                }

                if (_currentNode.pattern == null)
                {
                    Debug.LogWarning($"[EnemyAttackState] Pattern 노드인데 pattern이 비어 있습니다. nodeId={_currentNode.id}");
                    MoveNext(context, _currentNode.nextId);
                    return;
                }

                context.PatternRunner.Run(
                    _currentNode.pattern,
                    () => MoveNext(context, _currentNode.nextId)
                );
                break;

            case BattleFlowNodeType.SystemDialogue:
                Debug.Log($"[EnemyAttackState] SystemDialogue 실행: key={_currentNode.dialogueKey}");
                context.Manager.ShowSystemDialogue(
                    _currentNode.dialogueKey,
                    (_) => MoveNext(context, _currentNode.nextId)
                );
                break;

            case BattleFlowNodeType.ConditionBranch:
                Debug.Log($"[EnemyAttackState] ConditionBranch 실행");
                HandleBranch(context);
                break;

            case BattleFlowNodeType.Wait:
                Debug.Log($"[EnemyAttackState] Wait 시작: {_currentNode.waitTime}초");
                _timer = 0f;
                break;

            case BattleFlowNodeType.Event:
                Debug.Log($"[EnemyAttackState] Event 실행: {_currentNode.eventKey}");
                MoveNext(context, _currentNode.nextId);
                break;

            case BattleFlowNodeType.EndTurn:
                Debug.Log("[EnemyAttackState] EndTurn 실행");
                ChangeState(context, BattleStateType.TurnEnd);
                break;

            default:
                Debug.LogWarning($"[EnemyAttackState] 처리되지 않은 노드 타입: {_currentNode.nodeType}");
                ChangeState(context, BattleStateType.TurnEnd);
                break;
        }
    }

    private void HandleBranch(BattleContext context)
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
        Debug.Log($"[EnemyAttackState] 분기 결과={result}, next={next}");
        MoveNext(context, next);
    }

    private void MoveNext(BattleContext context, string nextId)
    {
        Debug.Log($"[EnemyAttackState] MoveNext 호출: nextId={nextId}");

        if (string.IsNullOrEmpty(nextId))
        {
            Debug.Log("[EnemyAttackState] nextId 비어 있음 -> TurnEnd");
            ChangeState(context, BattleStateType.TurnEnd);
            return;
        }

        if (_nodes == null || !_nodes.TryGetValue(nextId, out _currentNode))
        {
            Debug.LogError($"[EnemyAttackState] 다음 노드를 찾을 수 없습니다. nextId={nextId}");
            ChangeState(context, BattleStateType.TurnEnd);
            return;
        }

        Debug.Log($"[EnemyAttackState] 다음 노드 진입: id={_currentNode.id}, type={_currentNode.nodeType}");
        ExecuteNode(context);
    }

    public override void Exit(BattleContext context)
    {
        Debug.Log("[EnemyAttackState] 적 공격 종료");

        _nodes = null;
        _currentNode = null;
        _timer = 0f;

        base.Exit(context);
    }
}