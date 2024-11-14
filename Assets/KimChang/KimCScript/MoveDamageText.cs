using System.Collections;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class MoveDamageText : MonoBehaviour
{
    [SerializeField] private TextMeshPro textMesh;

    private void OnEnable()
    {
        // Y+1 ��ġ�� 0.5�� ���� �̵�
        transform.DOMoveY(transform.position.y + 0.5f, 0.5f).SetEase(Ease.OutQuad);

        // �ؽ�Ʈ ������ ���� �ִϸ��̼� (���� ���̵带 ���� Alpha�� ����)
        Color originalColor = textMesh.color;
        textMesh.color = originalColor; // �ʱ� ���� ����

    }
}
