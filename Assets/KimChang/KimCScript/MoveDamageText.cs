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
        transform.DOMoveY(transform.position.y + 0.25f, 0.5f).SetEase(Ease.OutQuad);
    }
}
