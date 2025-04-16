using UnityEngine;
using UnityEngine.UI;

public class BlacksmithButton : MonoBehaviour
{
    public BlacksmithUI blacksmithUI; // ✅ 대장간 UI 스크립트 참조
    private Button button;

    private void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(ToggleUI);
    }

    private void ToggleUI()
    {
        if (blacksmithUI != null)
        {
            blacksmithUI.ToggleUI(); // ✅ 대장간 UI 애니메이션 실행
        }
    }
}
