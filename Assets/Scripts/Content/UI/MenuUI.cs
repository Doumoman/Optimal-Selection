using UnityEngine;

public class MenuUI : MonoBehaviour
{
    // TODO: UIManager 구현 필요!@@

    [SerializeField] private GameObject menuPanel;
    [SerializeField] private PlayerController player;

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
