// Assets/Editor/GoogleSheetImporter.cs
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class GoogleSheetImporter : EditorWindow
{
    string webAppUrl = "https://script.google.com/macros/s/AKfyc.../exec";

    [MenuItem("Tools/Import StagePresets JSON")]
    static void OpenWindow()
    {
        GetWindow<GoogleSheetImporter>("Import StagePresets");
    }

    void OnGUI()
    {
        GUILayout.Label("StagePresets JSON 내려받기", EditorStyles.boldLabel);
        webAppUrl = EditorGUILayout.TextField("WebApp URL", webAppUrl);

        if (GUILayout.Button("다운로드 및 저장"))
        {
            DownloadAndSave();
        }
    }

    void DownloadAndSave()
    {
        if (string.IsNullOrEmpty(webAppUrl))
        {
            Debug.LogError("[Importer] URL 이 비어 있습니다.");
            return;
        }

        // 1) UnityWebRequest 동기식 전송
        var www = UnityWebRequest.Get(webAppUrl);
        www.SendWebRequest();
        while (!www.isDone)
        {
            // 에디터를 잠시 멈춥니다. JSON이 작으면 금방 끝납니다.
        }

        // 2) 에러 체크
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[Importer] 다운로드 오류: {www.error}");
            return;
        }

        // 3) 로컬 저장 경로 결정 (StreamingAssets)
        string folder = Path.Combine(Application.dataPath, "StreamingAssets");
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        string filePath = Path.Combine(folder, "StagePresets.json");

        // 4) 파일 쓰기
        try
        {
            File.WriteAllText(filePath, www.downloadHandler.text);
            AssetDatabase.Refresh();
            Debug.Log($"[Importer] StagePresets.json 저장 완료: {filePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Importer] 파일 쓰기 실패: {e.Message}");
        }
    }
}
