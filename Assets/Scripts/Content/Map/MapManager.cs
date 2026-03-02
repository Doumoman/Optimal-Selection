using UnityEngine;

public class MapManager : IManager
{
    private bool _init = false;

    public void Init()
    {
        if(_init) return;
        _init = true;



    }

    public void Clear()
    {
        throw new System.NotImplementedException();
    }

    public void OnDestroy()
    {

    }
}
