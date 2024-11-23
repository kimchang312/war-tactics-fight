using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MoveDamageUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;

    private void OnEnable()
    {
        // Y+1 위치로 0.5초 동안 이동
        transform.DOMoveY(transform.position.y + 5f, 0.5f).SetEase(Ease.OutQuad);
    }
}
