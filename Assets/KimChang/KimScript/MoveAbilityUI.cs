using DG.Tweening;
using UnityEngine;

public class MoveAbilityUI : MonoBehaviour
{
    private Vector3 initialLocalPosition; // 초기 위치 저장용 변수

    private float waittingTime = 800f;

    private void Start()
    {
        // 로컬 좌표 기준으로 초기 위치 저장
        initialLocalPosition = transform.localPosition;
    }

    private void OnEnable()
    {
        // 로컬 x 위치 기준으로 방향 결정
        if (transform.localPosition.x < 0)
        {
            // x+40 로컬 위치로 이동 후 비활성화
            transform.DOLocalMoveX(transform.localPosition.x + 300f, waittingTime/1000f)
                     .SetEase(Ease.OutQuad)
                     .OnComplete(() => gameObject.SetActive(false));
        }
        else
        {
            // x-40 로컬 위치로 이동 후 비활성화
            transform.DOLocalMoveX(transform.localPosition.x - 300f, waittingTime/1000f)
                     .SetEase(Ease.OutQuad)
                     .OnComplete(() => gameObject.SetActive(false));
        }
    }

    private void OnDisable()
    {
        // DOTween 애니메이션 중지
        transform.DOKill();

        // 초기 위치로 되돌리기
        transform.localPosition = initialLocalPosition;
    }

    public void ChangeWaittingTime(float multiple)
    {
        waittingTime *= multiple;
    }
}
