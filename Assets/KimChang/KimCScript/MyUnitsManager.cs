using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyUnitsManager : MonoBehaviour
{
    // 내가 보유한 유닛 id와 유닛의 갯수
    public List<int[]> myUnitsIdsNCount = new List<int[]>();

    // Start is called before the first frame update
    void Start()
    {
        // 예시 유닛들 추후 삭제
        myUnitsIdsNCount.Add(new int[] { 1, 1 }); // 1을 1개 추가
        myUnitsIdsNCount.Add(new int[] { 2, 2 }); // 2를 2개 추가
        myUnitsIdsNCount.Add(new int[] { 4, 4 }); // 4를 4개 추가

        // List를 1차원 배열로 변환
        //int[] oneDimensionalArray = ConvertTo1DArray(myUnitsIdsNCount);
    }

    //List의 배열 순서를 갯수 가 많은 순으로 변경
    private void SortUnitsByCount()
    {
        myUnitsIdsNCount.Sort((x, y) => y[1].CompareTo(x[1])); // y[1] > x[1]이면 양수 반환
    }



    // List<int[]>를 1차원 배열로 변환하는 메서드
    int[] ConvertTo1DArray(List<int[]> list)
    {
        // 총 배열 크기를 계산 (갯수를 모두 더함)
        int totalLength = 0;
        foreach (int[] arr in list)
        {
            totalLength += arr[1]; // 두 번째 값(갯수)을 총 길이에 더함
        }

        // 1차원 배열 생성
        int[] result = new int[totalLength];



        // 리스트의 배열들을 하나의 1차원 배열로 복사
        int index = 0;
        foreach (int[] arr in list)
        {
            int id = arr[0];  // 추가할 유닛의 ID
            int count = arr[1];  // 해당 ID를 몇 번 추가할지

            for (int i = 0; i < count; i++)
            {
                result[index] = id;
                index++;
            }
        }

        return result;
    }
}
