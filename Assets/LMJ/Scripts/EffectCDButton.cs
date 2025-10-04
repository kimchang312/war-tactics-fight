using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectCDButton : MonoBehaviour
{
    [SerializeField] private EffectCDPlayer player; // 플레이어 지정
    [SerializeField] private EffectCD cd;           // 재생할 CD 지정

    // 버튼 클릭 시 호출
    public void PlayCD()
    {
        if (player == null || cd == null)
        {
            Debug.LogWarning("EffectCDButton: player 또는 cd가 비어 있음!");
            return;
        }

        player.Play(cd);
    }
}
