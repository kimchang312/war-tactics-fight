using System.Collections;
using System.Text;
using GoogleSheetsToUnity;
using GoogleSheetsToUnity.ThirdPary;
using TinyJSON;
using UnityEngine;
using UnityEngine.Networking; // Make sure to include this

public delegate void OnSpreadSheetLoaded(GstuSpreadSheet sheet);

namespace GoogleSheetsToUnity
{
    public partial class SpreadsheetManager
    {
        static GoogleSheetsToUnityConfig _config;

        public static GoogleSheetsToUnityConfig Config
        {
            get
            {
                if (_config == null)
                {
                    _config = (GoogleSheetsToUnityConfig)Resources.Load("GSTU_Config");
                }

                return _config;
            }
            set { _config = value; }
        }

        public static void ReadPublicSpreadsheet(GSTU_Search searchDetails, OnSpreadSheetLoaded callback)
        {
            if (string.IsNullOrEmpty(Config.API_Key))
            {
                Debug.Log("Missing API Key, please enter this in the config settings");
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("https://sheets.googleapis.com/v4/spreadsheets");
            sb.Append("/" + searchDetails.sheetId);
            sb.Append("/values");
            sb.Append("/" + searchDetails.worksheetName + "!" + searchDetails.startCell + ":" + searchDetails.endCell);
            sb.Append("?key=" + Config.API_Key);

            if (Application.isPlaying)
            {
                new Task(Read(sb.ToString(), searchDetails.titleColumn, searchDetails.titleRow, callback));
            }
#if UNITY_EDITOR
            else
            {
                EditorCoroutineRunner.StartCoroutine(Read(sb.ToString(), searchDetails.titleColumn, searchDetails.titleRow, callback));
            }
#endif
        }

        static IEnumerator Read(string url, string titleColumn, int titleRow, OnSpreadSheetLoaded callback)
        {
            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Error: " + www.error);
                }
                else
                {
                    ValueRange rawData = JSON.Load(www.downloadHandler.text).Make<ValueRange>();
                    GSTU_SpreadsheetResponce response = new GSTU_SpreadsheetResponce(rawData);

                    GstuSpreadSheet spreadSheet = new GstuSpreadSheet(response, titleColumn, titleRow);

                    callback?.Invoke(spreadSheet);
                }
            }
        }
    }
}
