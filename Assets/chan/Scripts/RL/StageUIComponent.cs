using UnityEngine;
using UnityEngine.UI;

public class StageUIComponent : MonoBehaviour
{
    private void Awake()
    {
        Button btn = GetComponent<Button>();
        if (btn == null)
        {
            Debug.LogWarning("StageUIComponent: Button 컴포넌트 없음. 자동 추가.");
            btn = gameObject.AddComponent<Button>();
        }
        Image img = GetComponent<Image>();
        if (img == null)
        {
            Debug.LogWarning("StageUIComponent: Image 컴포넌트 없음. 자동 추가.");
            img = gameObject.AddComponent<Image>();
            img.color = Color.white;
        }
    }

    public void SetInteractable(bool value)
    {
        Button btn = GetComponent<Button>();
        if (btn != null)
            btn.interactable = value;
    }
}
