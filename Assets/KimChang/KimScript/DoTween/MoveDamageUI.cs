using DG.Tweening;
using UnityEngine;

public class MoveDamageUI : MonoBehaviour
{
    private const float BaseWaittingTime = 500f;
    private float waittingTime = BaseWaittingTime;

    private void OnEnable()
    {
        GameSpeedManager.Instance.OnGameSpeedChanged -= ChangeWaittingTime;
        GameSpeedManager.Instance.OnGameSpeedChanged += ChangeWaittingTime;
        ChangeWaittingTime(GameSpeedManager.Instance.GameSpeed);

        transform.DOMoveY(transform.position.y + 40f, waittingTime * 0.001f)
            .SetEase(Ease.OutQuad);
    }

    private void OnDisable()
    {
        GameSpeedManager.Instance.OnGameSpeedChanged -= ChangeWaittingTime;

        transform.DOKill();
        transform.localPosition = Vector3.zero;
    }

    // 사용처: GameSpeedManager의 배속 값이 변경될 때 데미지 텍스트 이동 시간을 기준값 기준으로 재설정한다.
    public void ChangeWaittingTime(float multiple)
    {
        waittingTime = BaseWaittingTime * Mathf.Max(0.01f, multiple);
    }
}