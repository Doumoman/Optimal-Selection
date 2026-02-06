using TMPro;
using UnityEngine;

public class MenuUI : UI_Popup
{
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private PlayerController player;

    [Header("Stats Info")]
    [SerializeField] private TextMeshProUGUI txtLocation;
    [SerializeField] private TextMeshProUGUI txtHP;
    [SerializeField] private TextMeshProUGUI txtGold;

    [Header("Button Info")]
    [SerializeField] private GameObject[] menuButtons; // 0:인벤토리, 1:미션, 2:상태
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.yellow;

    private void Start()
    {
        menuPanel.SetActive(false);
        InputManager.Instance.OnMenuPressed += TogglePopup;
    }
    private void OnDestroy()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnMenuPressed -= TogglePopup;
        }
    }

    private void TogglePopup()
    {
        bool isActive = !menuPanel.activeSelf;
        menuPanel.SetActive(isActive);

        if (player != null)
        {
            player.isInputBlocked = isActive;
        }

        Time.timeScale = isActive ? 0.0f : 1.0f; // 임시
    }
}
