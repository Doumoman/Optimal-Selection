using UnityEngine;

[System.Serializable]
public class BattleStartRequest
{
    public string battleId;
    public string enemyId;
    public GameObject enemyWorldObject;

    public bool lockWorldInput = true;
    public bool pauseWorldObjects = true;

    public Vector3 playerWorldPosition;
}