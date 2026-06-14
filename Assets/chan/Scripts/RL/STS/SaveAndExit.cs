using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveAndExit : MonoBehaviour
{
    public MapGenerator mapGenerator; // 에디터에서 할당

    public void OnSaveAndExit()
    {
        // mapGenerator가 null이면 동적으로 찾기
        if (mapGenerator == null)
            mapGenerator = FindObjectOfType<MapGenerator>();
        if (mapGenerator == null)
        {
            Debug.LogError("mapGenerator를 찾을 수 없습니다!");
            return;
        }
        if (mapGenerator.NodeDictionary == null)
        {
            Debug.LogError("NodeDictionary가 null입니다!");
            return;
        }
        SaveData saveData = new();
        if (!saveData.SaveGame(mapGenerator))
        {
            Debug.LogWarning("저장에 실패했거나 맵 저장 데이터가 완전하지 않습니다.");
            return;
        }

        // 타이틀 씬으로 이동
        SceneManager.LoadScene("Title");
    }
}
