using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SettingsUI : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Language Settings")]
    [SerializeField] private TMP_Dropdown languageDropdown;

    [Header("Ingame Buttons Group")]
    [SerializeField] private GameObject ingameButtonsArea; // '전투포기', '저장' 버튼을 담은 부모 오브젝트
    [SerializeField] private Button giveUpButton;
    [SerializeField] private Button saveExitButton;

    [Header("Confirmation Popup")]
    [SerializeField] private GameObject confirmGiveUpPopup; // 새로 만든 팝업 오브젝트 연결

    private void OnEnable()
    {
        // 5-2. 데이터 로드 및 UI 반영
        var data = RogueLikeData.Instance;
        if (data != null)
        {
            masterSlider.value = data.MasterVolume;
            bgmSlider.value = data.BgmVolume;
            sfxSlider.value = data.SfxVolume;

            // 언어 인덱스 설정 (0:한국어, 1:영어, 2:일본어)
            languageDropdown.value = data.GetLanguage();
        }

        // 5-3. 씬 체크 및 버튼 활성화 제어
        string sceneName = SceneManager.GetActiveScene().name;

        // 타이틀 씬이 아닐 때만 전투 포기/저장 버튼을 보여줌
        bool isIngame = (sceneName != "Title");
        ingameButtonsArea.SetActive(isIngame);

        // 저장 버튼은 미구현 상태이므로 상호작용만 꺼둠
        saveExitButton.interactable = false;

        // 설정창이 켜질 때 확인 팝업은 무조건 꺼진 상태로 초기화
        if (confirmGiveUpPopup != null)
        {
            confirmGiveUpPopup.SetActive(false);
        }
    }

    #region UI Event Functions
    // 슬라이더 조절 시 데이터 업데이트
    public void UpdateMasterVolume(float value) => RogueLikeData.Instance.MasterVolume = value;
    public void UpdateBgmVolume(float value) => RogueLikeData.Instance.BgmVolume = value;
    public void UpdateSfxVolume(float value) => RogueLikeData.Instance.SfxVolume = value;

    // 언어 변경 시 데이터 업데이트
    public void UpdateLanguage(int index) => RogueLikeData.Instance.SetLanguage(index);

    // 설정창 닫기 (Resume)
    public void OnClickClose()
    {
        this.gameObject.SetActive(false);
    }

    // 1. 설정창 하단의 '전투 포기' 버튼을 누르면 호출됨 (팝업 띄우기)
    public void OnClickGiveUpRequest()
    {
        confirmGiveUpPopup.SetActive(true);
    }

    // 2. 팝업창에서 '취소'를 누르면 호출됨 (팝업 닫기)
    public void OnClickCancelGiveUp()
    {
        confirmGiveUpPopup.SetActive(false);
    }

    // 3. 팝업창에서 '포기(Yes)'를 누르면 호출됨 (실제 처리)
    public void OnClickConfirmGiveUp()
    {
        // TODO: 포기 시 적용할 페널티 처리 로직 (기력 감소 등 데이터 저장)

        confirmGiveUpPopup.SetActive(false); // 팝업 닫기
        this.gameObject.SetActive(false);    // 설정창 전체 닫기
        SceneManager.LoadScene("Title");     // 타이틀로 이동
    }

    #endregion

}
