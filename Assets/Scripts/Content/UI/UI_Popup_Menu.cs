using TMPro;
using UnityEngine;

public class UI_Popup_Menu : UI_Popup
{
    [SerializeField] private GameObject menuPanel;

    [Header("Stats Info")]
    [SerializeField] private TextMeshProUGUI txtLocation;
    [SerializeField] private TextMeshProUGUI txtHP;
    [SerializeField] private TextMeshProUGUI txtGold;

    [Header("Button Info")]
    [SerializeField] private GameObject[] menuButtons;

    public override void Init()
    {
        base.Init();
    }

}
