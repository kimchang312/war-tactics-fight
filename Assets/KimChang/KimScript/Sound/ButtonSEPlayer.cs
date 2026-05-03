using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonSEPlayer : MonoBehaviour
{
    [SerializeField] private string seKey = "se_btn_click";

    private Button button;
    private bool initialized;

    private void Awake()
    {
        Initialize();
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(PlayClickSE);
    }

    // 사용처: 버튼 클릭 이벤트에 효과음 재생 함수를 한 번만 연결
    private void Initialize()
    {
        if (initialized)
            return;

        button = GetComponent<Button>();

        if (button == null)
            return;

        button.onClick.AddListener(PlayClickSE);
        initialized = true;
    }

    // 사용처: BGMManager가 버튼 이름에 따라 클릭 효과음 키를 지정
    public void SetSEKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return;

        seKey = key;
        Initialize();
    }

    // 사용처: 버튼 클릭 시 공용 또는 지정된 UI 효과음을 재생
    private void PlayClickSE()
    {
        if (BGMManager.Instance == null)
            return;

        BGMManager.Instance.PlaySE(seKey);
    }
}