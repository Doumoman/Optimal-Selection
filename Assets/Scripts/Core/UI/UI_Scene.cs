using UnityEngine;

public class UI_Scene : UI_Base
{
    public override void Init()
    {
        SingletonManagers.UI.SetCanvas(gameObject, false);
    }
}
