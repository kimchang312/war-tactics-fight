using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Difficulty : MonoBehaviour
{
    public static int enemyGold; // 적의 골드를 저장할 정적 변수
    /*
     임시로 골드에 대한 내용이 보이게 설정.
     추후 적 라인업 구성을 구현할때 적절한 방법으로 연결 예정.
     난이도 별 버튼을 누르면 미리 정해놓은 적의 골드와 함께 씬 전환

    */


    // 쉬움 버튼 클릭 시 호출될 함수
    public void SetEasyDifficulty()
    {
        SetEnemyGoldAndLoadScene(2500,"쉬움");
    }

    // 보통 버튼 클릭 시 호출될 함수
    public void SetNormalDifficulty()
    {
        SetEnemyGoldAndLoadScene(3500,"보통");
    }

    // 어려움 버튼 클릭 시 호출될 함수
    public void SetHardDifficulty()
    {
        SetEnemyGoldAndLoadScene(5000,"어려움");
    }

    // 도전 버튼 클릭 시 호출될 함수
    public void SetChallengeDifficulty()
    {
        SetEnemyGoldAndLoadScene(6000,"도전");
    }

    // 적의 골드를 설정하고 씬을 로드하는 공통 함수
    private void SetEnemyGoldAndLoadScene(int goldAmount,string difficulty)
    {
        enemyGold = goldAmount;
        PlayerData.Instance.enemyFunds= goldAmount;
        PlayerData.Instance.difficulty = difficulty;
        SceneManager.LoadScene("Faction");
    }
}
