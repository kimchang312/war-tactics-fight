using UnityEngine;

public class ItemInformation : MonoBehaviour
{
    public ItemInfoData data = new();

    // 사용처: 풀에서 재사용되는 아이콘의 이전 설명 데이터를 제거
    public void Clear()
    {
        data = new ItemInfoData
        {
            isItem = false
        };
    }

    // 사용처: 버프/디버프 아이콘에 툴팁 표시용 데이터를 주입
    public void SetBuffDeBuff(int id, int grade, int duration, string name, string description)
    {
        data = new ItemInfoData
        {
            isItem = false,
            isRelic = false,
            isBuffDeBuff = true,
            buffDeBuffId = id,
            buffDeBuffGrade = grade,
            buffDeBuffDuration = duration,
            buffDeBuffName = name,
            buffDeBuffDescription = description
        };
    }
}