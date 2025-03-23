using UnityEngine;
using UnityEngine.UI;

public class StageUIComponent : MonoBehaviour
{
    

    public void SetInteractable(bool value)
    {
        Button btn = GetComponent<Button>();
        if (btn != null)
            btn.interactable = value;
    }
}
