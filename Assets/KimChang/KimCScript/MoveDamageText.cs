using System.Collections;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class MoveDamageText : MonoBehaviour
{
    [SerializeField] private TextMeshPro textMesh;

    private void OnEnable()
    {
        // Y+1 위치로 0.5초 동안 이동
        transform.DOMoveY(transform.position.y + 0.5f, 0.5f).SetEase(Ease.OutQuad);

        // 텍스트 색상의 투명도 애니메이션 (색상 페이드를 위해 Alpha값 조절)
        Color originalColor = textMesh.color;
        textMesh.color = originalColor; // 초기 색상 설정

    }
}
