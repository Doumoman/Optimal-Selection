using UnityEngine;

public class BattleTrigger : MonoBehaviour
{
    [SerializeField] private string battleId;
    [SerializeField] private string enemyId;
    [SerializeField] private string flowId;

    private bool _triggered;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_triggered) return;
        if (!other.CompareTag("Player")) return;

        _triggered = true;

        BattleStartRequest request = new BattleStartRequest
        {
            battleId = battleId,
            enemyId = enemyId,
            enemyWorldObject = gameObject,
            playerWorldPosition = other.transform.position,
            lockWorldInput = true,
            pauseWorldObjects = true
        };

        Managers.Battle.StartBattle(request);
    }

    public void ResetTrigger()
    {
        _triggered = false;
    }
}