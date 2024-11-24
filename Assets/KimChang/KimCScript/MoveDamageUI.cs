using DG.Tweening;
using TMPro;
using UnityEngine;

public class MoveDamageUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;

    private void OnEnable()
    {

        // Y+1 위치로 0.5초 동안 이동
        transform.DOMoveY(transform.position.y + 40f, 0.5f)
                 .SetEase(Ease.OutQuad);
    }

    private void OnDisable()
    {
        // 비활성화 시 수행할 작업
        transform.DOKill(); // DOTween 애니메이션 중지
        transform.localPosition = Vector3.zero; // 위치 초기화 예시
    }

}
