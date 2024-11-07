using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Difficulty : MonoBehaviour
{
    public static int enemyGold; // ���� ��带 ������ ���� ����
    /*
     �ӽ÷� ��忡 ���� ������ ���̰� ����.
     ���� �� ���ξ� ������ �����Ҷ� ������ ������� ���� ����.
     ���̵� �� ��ư�� ������ �̸� ���س��� ���� ���� �Բ� �� ��ȯ

    */


    // ���� ��ư Ŭ�� �� ȣ��� �Լ�
    public void SetEasyDifficulty()
    {
        SetEnemyGoldAndLoadScene(2500,"����");
    }

    // ���� ��ư Ŭ�� �� ȣ��� �Լ�
    public void SetNormalDifficulty()
    {
        SetEnemyGoldAndLoadScene(3500,"����");
    }

    // ����� ��ư Ŭ�� �� ȣ��� �Լ�
    public void SetHardDifficulty()
    {
        SetEnemyGoldAndLoadScene(5000,"�����");
    }

    // ���� ��ư Ŭ�� �� ȣ��� �Լ�
    public void SetChallengeDifficulty()
    {
        SetEnemyGoldAndLoadScene(6000,"����");
    }

    // ���� ��带 �����ϰ� ���� �ε��ϴ� ���� �Լ�
    private void SetEnemyGoldAndLoadScene(int goldAmount,string difficulty)
    {
        enemyGold = goldAmount;
        PlayerData.Instance.enemyFunds= goldAmount;
        PlayerData.Instance.difficulty = difficulty;
        SceneManager.LoadScene("Faction");
    }
}
