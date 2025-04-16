using DG.Tweening;
using UnityEngine;

public class ObjectMover : MonoBehaviour
{
    [SerializeField] private float moveDistance = 1f; // 이동 거리
    [SerializeField] private float moveDuration = 1f; // 이동 속도

    private void Start()
    {
        StartLoopingMovement();
    }

    private void StartLoopingMovement()
    {
        // 현재 위치에서 좌우로 반복 이동
        transform.DOMoveX(transform.position.x + moveDistance, moveDuration)
            .SetEase(Ease.InOutSine) // 부드러운 이동
            .SetLoops(-1, LoopType.Yoyo); // 무한 반복 (좌우 반복)
    }
}
