using UnityEngine;

public class BattleTestButton : MonoBehaviour
{
    [SerializeField] private string battleId = "DebugBattle";
    [SerializeField] private string enemyId = "DebugEnemy";

    public void StartDebugBattle()
    {
        BattleStartRequest request = new BattleStartRequest
        {
            battleId = battleId,
            enemyId = enemyId,
            enemyWorldObject = null,
            lockWorldInput = false,
            pauseWorldObjects = false,
            playerWorldPosition = Vector3.zero
        };

        SingletonManagers.Battle.StartBattle(request);
    }

    public void EndDebugBattle()
    {
        SingletonManagers.Battle.FinishBattle(BattleResult.Escape);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            StartDebugBattle();
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            EndDebugBattle();
        }
    }
}