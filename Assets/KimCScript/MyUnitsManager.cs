using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyUnitsManager : MonoBehaviour
{
    // ���� ������ ���� id�� ������ ����
    public List<int[]> myUnitsIdsNCount = new List<int[]>();

    // Start is called before the first frame update
    void Start()
    {
        // ���� ���ֵ� ���� ����
        myUnitsIdsNCount.Add(new int[] { 1, 1 }); // 1�� 1�� �߰�
        myUnitsIdsNCount.Add(new int[] { 2, 2 }); // 2�� 2�� �߰�
        myUnitsIdsNCount.Add(new int[] { 4, 4 }); // 4�� 4�� �߰�

        // List�� 1���� �迭�� ��ȯ
        //int[] oneDimensionalArray = ConvertTo1DArray(myUnitsIdsNCount);
    }

    //List�� �迭 ������ ���� �� ���� ������ ����
    private void SortUnitsByCount()
    {
        myUnitsIdsNCount.Sort((x, y) => y[1].CompareTo(x[1])); // y[1] > x[1]�̸� ��� ��ȯ
    }



    // List<int[]>�� 1���� �迭�� ��ȯ�ϴ� �޼���
    int[] ConvertTo1DArray(List<int[]> list)
    {
        // �� �迭 ũ�⸦ ��� (������ ��� ����)
        int totalLength = 0;
        foreach (int[] arr in list)
        {
            totalLength += arr[1]; // �� ��° ��(����)�� �� ���̿� ����
        }

        // 1���� �迭 ����
        int[] result = new int[totalLength];



        // ����Ʈ�� �迭���� �ϳ��� 1���� �迭�� ����
        int index = 0;
        foreach (int[] arr in list)
        {
            int id = arr[0];  // �߰��� ������ ID
            int count = arr[1];  // �ش� ID�� �� �� �߰�����

            for (int i = 0; i < count; i++)
            {
                result[index] = id;
                index++;
            }
        }

        return result;
    }
}
