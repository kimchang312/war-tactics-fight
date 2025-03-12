using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardUI : MonoBehaviour
{
    [SerializeField] private GameObject firstRelic;
    [SerializeField] private GameObject secondRelic;
    [SerializeField] private GameObject thirdRelic;

    [SerializeField] private Button rerollBtn;

    private void Start()
    {
        rerollBtn.onClick.AddListener(CreateRelics);

        // 각 유산 오브젝트에 클릭 이벤트 연결
        AddRelicClickListener(firstRelic);
        AddRelicClickListener(secondRelic);
        AddRelicClickListener(thirdRelic);
    }

    // 유산 생성
    public void CreateRelics()
    {
        List<int> selectedRelicIds = new List<int>();

        for (int i = 0; i < 3; i++)
        {
            // 중복 방지를 위해 반복하여 유산 ID 선택
            int relicId;
            do
            {
                relicId = RewardManager.GetRandomWarRelicId();
            }
            while (selectedRelicIds.Contains(relicId) && relicId != -1);

            // 선택된 ID 저장
            if (relicId != -1)
            {
                selectedRelicIds.Add(relicId);
            }

            // 해당 유산 오브젝트 선택
            GameObject relic = i == 0 ? firstRelic : (i == 1 ? secondRelic : thirdRelic);

            // 이미지 설정
            Image relicImage = relic.GetComponent<Image>();
            Sprite sprite = Resources.Load<Sprite>($"KIcon/WarRelic/{relicId}");
            relicImage.sprite = sprite;

            // 오브젝트 이름을 relicId로 설정
            relic.name = $"{relicId}";

            // 텍스트 설정
            TextMeshProUGUI relicText = relic.GetComponentInChildren<TextMeshProUGUI>(true);
            if (relicText != null)
            {
                relicText.text = $"{relicId}";
            }
        }
    }

    // 유산을 선택하면 호출되는 함수
    private void OnRelicSelected(GameObject relicObject)
    {
        if (int.TryParse(relicObject.name, out int relicId))
        {
            // 유산을 보유 목록에 추가
            RogueLikeData.Instance.AcquireRelic(relicId);

            Debug.Log($"유산 ID {relicId}이(가) 보유 목록에 추가되었습니다.");
            // 유산 재생성
            CreateRelics();
        }
        else
        {
            Debug.LogWarning("유산 ID 파싱에 실패했습니다.");
        }
    }

    // 유산 오브젝트에 클릭 이벤트 리스너 추가
    private void AddRelicClickListener(GameObject relic)
    {
        Button relicButton = relic.GetComponent<Button>();
        if (relicButton == null)
        {
            relicButton = relic.AddComponent<Button>(); // 없으면 추가
        }

        // 클릭 이벤트 등록
        relicButton.onClick.AddListener(() => OnRelicSelected(relic));
    }

    // 유산 제거 (필요 시 구현)
    private void ClearRelics()
    {
        // 필요한 경우 유산 제거 로직 작성
    }
}
