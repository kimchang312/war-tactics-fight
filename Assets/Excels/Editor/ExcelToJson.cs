using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

public class ExcelToJson
{
    private static readonly string _assetExcelDirPath = "Assets/Excels";
    private static readonly string _excelDirPath = "Excels";
    private static readonly string _excelEnumSheetName = "Enum";
    static Dictionary<string, Dictionary<string, int>> enumMappings;
    static bool emptyValueDetectedInFile = false; // 빈 값이 감지되었는지 여부를 추적하는 변수
    private static string excelDirPath;
    [MenuItem("Assets/Excel to Json", true)]
    static bool CanExcelToJson()
    {
        var selectedAssets = Selection.GetFiltered(typeof(Object), SelectionMode.Assets);
        if (selectedAssets.Length != 1) return false;
        var path = AssetDatabase.GetAssetPath(selectedAssets[0]);
        return path.EndsWith(_excelDirPath);
    }

    [MenuItem("Assets/Excel to Json", false)]
    static void ExecuteExcelToJson()
    {
        var selectedAssets = Selection.GetFiltered(typeof(Object), SelectionMode.Assets);
        if (selectedAssets.Length != 1)
        {
            throw new Exception("Please select a single directory.");
        }
        var path = AssetDatabase.GetAssetPath(selectedAssets[0]);

        string configFilePath = $"{path}/config.txt";
        var config = LoadConfiguration(configFilePath);

        bool useAssets = config.ContainsKey("useAssets") && config["useAssets"].ToLower() == "true";
        excelDirPath = useAssets ? _assetExcelDirPath : _excelDirPath;

        string defaultExcelDirectoryPath = ValidateOrCreateDirectory(config, "defaultExcelDirectoryPath", $"{excelDirPath}/excel_files");
        string defaultLoaderOutputDirectory = ValidateOrCreateDirectory(
            config,
            "defaultLoaderOutputDirectory",
            $"{excelDirPath}/loader_output"
        );
        string defaultJsonOutputDirectory = ValidateOrCreateDirectory(
            config,
            "defaultJsonOutputDirectory",
            $"{excelDirPath}/json_output"
        );
        bool allowMultipleSheets = config.ContainsKey("allowMultipleSheets") && config["allowMultipleSheets"].ToLower() == "true";
        string excelDirectoryPath = ValidateOrCreateDirectory(config, $"excelDirectoryPath", defaultExcelDirectoryPath);
        string loaderOutputDirectory = ValidateOrCreateDirectory(config, "loaderOutputDirectory", defaultLoaderOutputDirectory);
        string jsonOutputDirectory = ValidateOrCreateDirectory(config, "jsonOutputDirectory", defaultJsonOutputDirectory);
        string logDirPath = "excel_logs";
        if (Directory.Exists(logDirPath) == false)
            Directory.CreateDirectory(logDirPath);
        string logFilePath = Path.Combine(logDirPath, $"{DateTime.Now:yyyy-MM-dd}_error_log.txt").Replace("\\", "/");

        // Load Enum definitions and generate Enum C# file
        enumMappings = LoadEnumDefinitionsAndGenerateCs(excelDirectoryPath, loaderOutputDirectory, logFilePath);

        ProcessExcelFiles(
            excelDirectoryPath,
            loaderOutputDirectory,
            jsonOutputDirectory,
            logFilePath,
            allowMultipleSheets
        );

        AssetDatabase.Refresh();
        Debug.Log("Excel to Json conversion complete.");
    }

    static Dictionary<string, string> LoadConfiguration(string configFilePath)
    {
        var config = new Dictionary<string, string>();

        if (File.Exists(configFilePath))
        {
            var lines = File.ReadAllLines(configFilePath);
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#"))
                {
                    continue; // 주석이거나 빈 줄은 무시
                }

                var cleanLine = line.Split('#')[0].Trim(); // 주석 부분을 제거하고 정리
                var parts = cleanLine.Split(new[] { '=' }, 2);
                if (parts.Length == 2)
                {
                    config[parts[0].Trim()] = parts[1].Trim();
                }
            }
        }
        else
        {
            config["defaultExcelDirectoryPath"] = "Excels/excel_files";
            config["defaultLoaderOutputDirectory"] = "Excels/loader_output";
            config["defaultJsonOutputDirectory"] = "Excels/json_output";
            config["excelDirectoryPath"] = "Excels/excel_files";
            config["loaderOutputDirectory"] = "Excels/loader_output";
            config["jsonOutputDirectory"] = "Excels/json_output";
            config["allowMultipleSheets"] = "false";
            config["useAssets"] = "true";

            using (var sw = File.CreateText(configFilePath))
            {
                sw.WriteLine("# excel_logs directory is root directory of the project");
                sw.WriteLine();

                sw.WriteLine("# 기본 디렉토리 설정");
                sw.WriteLine($"defaultExcelDirectoryPath={config["defaultExcelDirectoryPath"]} # 엑셀 파일 디렉토리 기본 경로");
                sw.WriteLine($"defaultLoaderOutputDirectory={config["defaultLoaderOutputDirectory"]} # 로더 클래스 출력 디렉토리 기본 경로");
                sw.WriteLine($"defaultJsonOutputDirectory={config["defaultJsonOutputDirectory"]} # JSON 파일 출력 디렉토리 기본 경로");
                sw.WriteLine();

                sw.WriteLine("# 사용자 지정 디렉토리 설정");
                sw.WriteLine($"excelDirectoryPath={config["excelDirectoryPath"]} # 엑셀 파일 디렉토리 경로");
                sw.WriteLine($"loaderOutputDirectory={config["loaderOutputDirectory"]} # 로더 클래스 출력 디렉토리 경로");
                sw.WriteLine($"jsonOutputDirectory={config["jsonOutputDirectory"]} # JSON 파일 출력 디렉토리 경로");
                sw.WriteLine();

                sw.WriteLine("# 다중 시트 설정");
                sw.WriteLine("allowMultipleSheets=false # 다중 시트를 허용할지 여부 (true/false)");
                sw.WriteLine();

                sw.WriteLine("# Assets 사용 설정 (Root 폴더가 Assets인 경우 true)");
                sw.WriteLine("useAssets=true # Assets 폴더 사용 여부 (true/false)");

            }

            AssetDatabase.Refresh();
        }

        return config;
    }

    static string ValidateOrCreateDirectory(Dictionary<string, string> config, string key, string defaultDirectory)
    {
        string path;
        bool useAssets = config.ContainsKey("useAssets") && config["useAssets"].ToLower() == "true";
        if (config.ContainsKey(key))
        {
            path = useAssets ? $"Assets/{config[key]}" : config[key];
        }
        else
        {
            path = defaultDirectory;
        }

        if (path.StartsWith("Assets/") == false)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
        else
        {
            if (AssetDatabase.IsValidFolder(path) == false)
            {
                string dirName = Path.GetFileName(path);
                string parentPath = Path.GetDirectoryName(path);
                AssetDatabase.CreateFolder(parentPath, dirName);
            }
        }

        return path;
    }

    static IWorkbook LoadBook(string excelPath)
    {
        using (FileStream stream = File.Open(excelPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
            if (Path.GetExtension(excelPath) == ".xls") return new HSSFWorkbook(stream);
            else return new XSSFWorkbook(stream);
        }
    }
    static IEnumerable<ISheet> LoadSheets(IWorkbook workbook)
    {
        for (int i = 0; i < workbook.NumberOfSheets; i++)
        {
            yield return workbook.GetSheetAt(i);
        }
    }

    static Dictionary<string, Dictionary<string, int>> LoadEnumDefinitionsAndGenerateCs(string excelDir, string loaderDir,
        string logFilePath)
    {
        var enumDefinitions = new Dictionary<string, Dictionary<string, int>>();
        var enumFilePath = Path.Combine(excelDir, "Enum.xlsx");

        if (File.Exists(enumFilePath))
        {
            try
            {
                var sb = new StringBuilder();
                CsWarningHeader(sb);
                sb.AppendLine("using System;");
                sb.AppendLine();
                sb.AppendLine("public static class DesignEnums");
                sb.AppendLine("{");


                IWorkbook workbook = LoadBook(enumFilePath);

                var worksheet = workbook.GetSheet(_excelEnumSheetName);

                if (worksheet == null)
                {
                    throw new Exception($"Sheet '{_excelEnumSheetName}' not found in Enum definitions file.");
                }

                for (int i = 0; i <= worksheet.LastRowNum; i += 2)
                {
                    IRow row = worksheet.GetRow(i);
                    IRow descRow = worksheet.GetRow(i + 1);
                    // first cell is the enum name
                    ICell entryCell = row.GetCell(0);
                    ICell descCell = descRow.GetCell(0);
                    if (entryCell == null || entryCell.CellType == CellType.Blank) break;
                    if (descCell == null || descCell.CellType == CellType.Blank) break;
                    string enumName = GetCellText(entryCell);
                    string enumDesc = GetCellText(worksheet.GetRow(i + 1).GetCell(0));
                    if (enumDefinitions.ContainsKey(enumName))
                    {
                        throw new Exception($"Duplicate enum name '{enumName}' found in Enum definitions.");
                    }
                    if (!enumDesc.StartsWith("#"))
                    {
                        continue;
                    }

                    var enumValues = new Dictionary<string, int>();
                    int index = 0;
                    sb.AppendLine($"    /// <summary>");
                    string[] descLines = enumDesc.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                    foreach (var line in descLines)
                    {
                        sb.AppendLine($"    /// {line}");
                    }
                    sb.AppendLine($"    /// </summary>");
                    sb.AppendLine($"    public enum {enumName}");
                    sb.AppendLine("    {");

                    // enum values
                    for (int col = 1; col <= row.LastCellNum; col++)
                    {
                        ICell valueCell = row.GetCell(col);
                        if (valueCell == null || valueCell.CellType == CellType.Blank) break;
                        string value = GetCellText(valueCell);
                        if (enumValues.ContainsKey(value))
                        {
                            throw new Exception($"Duplicate value '{value}' found in enum '{enumName}'.");
                        }

                        enumValues[value] = index;
                        string[] valueDescLines = GetCellText(descRow.GetCell(col)).Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                        sb.AppendLine($"        /// <summary>");
                        foreach (var line in valueDescLines)
                        {
                            sb.AppendLine($"        /// {line}");
                        }
                        sb.AppendLine($"        /// </summary>");
                        sb.AppendLine($"        {value} = {index},");
                        index++;
                    }

                    sb.AppendLine("    }");
                    enumDefinitions[enumName] = enumValues;
                }

                sb.AppendLine("}");

                var enumOutputPath = Path.Combine(loaderDir, "DesignEnums.cs");
                File.WriteAllText(enumOutputPath, sb.ToString());
                Debug.Log($"Enum definitions file generated\n");
            }
            catch (Exception ex)
            {
                LogError(logFilePath, $"Error loading Enum definitions from file {enumFilePath}: {ex.Message}\n{ex.StackTrace}");
                Debug.LogError($"Error loading Enum definitions from file {enumFilePath}: {ex.Message}");
            }
        }

        return enumDefinitions;
    }

    static void ProcessExcelFiles(string excelDir, string loaderDir, string jsonDir, string logFilePath, bool allowMultipleSheets)
    {
        var excelFiles = Directory.GetFiles(excelDir, "*.xlsx");

        int totalFiles = excelFiles.Length;
        int errorFiles = 0;
        int processedFiles = 0;
        
        List<string> classNames = new List<string>();

        foreach (var excelFilePath in excelFiles)
        {
            if (Path.GetFileName(excelFilePath).StartsWith("~") ||
                Path.GetFileName(excelFilePath).Equals("Enum.xlsx", StringComparison.OrdinalIgnoreCase) ||
                Path.GetFileName(excelFilePath).EndsWith(".meta"))
            {
                Debug.Log($"Skipping file: {excelFilePath}\n");
                continue;
            }

            bool success = GenerateClassAndJsonFromExcel(
                excelFilePath,
                loaderDir,
                jsonDir,
                logFilePath,
                allowMultipleSheets,
                out string className
            );
            classNames.Add(className);
            if (success)
            {
                processedFiles++;
            }
            else
            {
                errorFiles++;
            }
        }
        
        GenerateDataLoaderCs(classNames, loaderDir);

        Debug.Log($"Total files processed: {totalFiles}");
        Debug.Log($"Successfully processed files: {processedFiles}");
        Debug.Log($"Files with errors: {errorFiles}");


    }

    static void GenerateDataLoaderCs(List<string> classNames, string loaderDir)
    {
        if (classNames.Count <= 0)
        {
            return;
        }
        
        StringBuilder sb = new StringBuilder();
        CsWarningHeader(sb);
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using UnityEngine;");
        // Data Base 
        sb.AppendLine("[Serializable]");
        sb.AppendLine("public abstract class BaseJsonData");
        sb.AppendLine("{");
        sb.AppendLine("    public int key;");
        sb.AppendLine("}");
        sb.AppendLine();
        
        // Loader Interface
        sb.AppendLine("public abstract class JsonLoader");
        sb.AppendLine("{");
        sb.AppendLine("    public List<BaseJsonData> ItemsList { get; protected set; }");
        sb.AppendLine("    public Dictionary<int, BaseJsonData> ItemsDict { get; protected set; }");
        sb.AppendLine();
        sb.AppendLine("    protected void AddDatas<T>(List<T> items) where T : BaseJsonData");
        sb.AppendLine("    {");
        sb.AppendLine("        ItemsList = items.ConvertAll(x => x as BaseJsonData);");
        sb.AppendLine("        ItemsDict = new Dictionary<int, BaseJsonData>();");
        sb.AppendLine("        foreach (var item in ItemsList)");
        sb.AppendLine("        {");
        sb.AppendLine("            ItemsDict.Add(item.key, item);");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    public T GetByKey<T>(int key) where T : BaseJsonData");
        sb.AppendLine("    {");
        sb.AppendLine("        if (ItemsDict.ContainsKey(key))");
        sb.AppendLine("        {");
        sb.AppendLine("            return ItemsDict[key] as T;");
        sb.AppendLine("        }");
        sb.AppendLine("        return null;");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    public T GetByIndex<T>(int index) where T : BaseJsonData");
        sb.AppendLine("    {");
        sb.AppendLine("        if (index >= 0 && index < ItemsList.Count)");
        sb.AppendLine("        {");
        sb.AppendLine("            return ItemsList[index] as T;");
        sb.AppendLine("        }");
        sb.AppendLine("        return null;");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    public List<T> GetAll<T>() where T : BaseJsonData");
        sb.AppendLine("    {");
        sb.AppendLine("        return ItemsList.ConvertAll(x => x as T);");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    public int Count()");
        sb.AppendLine("    {");
        sb.AppendLine("        return ItemsList.Count;");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        
        // DataLoader
        sb.AppendLine("public class DataLoader");
        sb.AppendLine("{");
        sb.AppendLine("    private Dictionary<string, JsonLoader> _loaders = new Dictionary<string, JsonLoader>();");
        sb.AppendLine();
        sb.AppendLine("    // loadFunc is json file loader function");
        sb.AppendLine("    // ex) Func<string, TextAsset> loadFunc = () => Resources.Load<TextAsset>();");
        sb.AppendLine("    public DataLoader(Func<string, TextAsset> loadFunc)");
        sb.AppendLine("    {");
        foreach (var className in classNames)
        {
            sb.AppendLine($"        _loaders.Add(nameof({className}), new {className}Loader(loadFunc));");
        }
        sb.AppendLine("    }");
        sb.AppendLine();
        
        sb.AppendLine("    public T GetByKey<T>(int key) where T : BaseJsonData");
        sb.AppendLine("    {");
        sb.AppendLine("        if (_loaders.ContainsKey(typeof(T).Name))");
        sb.AppendLine("        {");
        sb.AppendLine("            return _loaders[typeof(T).Name].GetByKey<T>(key);");
        sb.AppendLine("        }");
        sb.AppendLine("        return null;");
        sb.AppendLine("    }");
        
        sb.AppendLine("    public T GetByIndex<T>(int index) where T : BaseJsonData");
        sb.AppendLine("    {");
        sb.AppendLine("        if (_loaders.ContainsKey(typeof(T).Name))");
        sb.AppendLine("        {");
        sb.AppendLine("            return _loaders[typeof(T).Name].GetByIndex<T>(index);");
        sb.AppendLine("        }");
        sb.AppendLine("        return null;");
        sb.AppendLine("    }");
        
        sb.AppendLine("    public int Count<T>() where T : BaseJsonData");
        sb.AppendLine("    {");
        sb.AppendLine("        if (_loaders.ContainsKey(typeof(T).Name))");
        sb.AppendLine("        {");
        sb.AppendLine("            return _loaders[typeof(T).Name].ItemsList.Count;");
        sb.AppendLine("        }");
        sb.AppendLine("        return 0;");
        sb.AppendLine("    }");
        
        sb.AppendLine("    public List<T> GetAll<T>() where T : BaseJsonData");
        sb.AppendLine("    {");
        sb.AppendLine("        if (_loaders.ContainsKey(typeof(T).Name))");
        sb.AppendLine("        {");
        sb.AppendLine("            return _loaders[typeof(T).Name].GetAll<T>();");
        sb.AppendLine("        }");
        sb.AppendLine("        return null;");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        
        var dataLoaderOutputPath = Path.Combine(loaderDir, "DataLoader.cs");
        File.WriteAllText(dataLoaderOutputPath, sb.ToString());
        Debug.Log($"Main loader file generated at {dataLoaderOutputPath}");
    }
    
    static string GetCellText(ICell cell)
    {
        if (cell == null) return null;
        switch (cell.CellType)
        {
            case CellType.Numeric:
                return cell.NumericCellValue.ToString(CultureInfo.CurrentCulture);
            case CellType.String:
                return cell.StringCellValue;
            case CellType.Boolean:
                return cell.BooleanCellValue.ToString();
            case CellType.Formula:
                return cell.CellFormula;
            case CellType.Unknown:
            case CellType.Error:
            case CellType.Blank:
            default:
                return null;
        }
    }
    static bool GenerateClassAndJsonFromExcel(string excelPath, string loaderDir, string jsonDir, string logFilePath,
        bool allowMultipleSheets, out string className)
    {
        className = null;
        try
        {
            emptyValueDetectedInFile = false; // 새로운 파일을 처리할 때마다 초기화

            IWorkbook workbook = LoadBook(excelPath);
            IEnumerable<ISheet> sheets = allowMultipleSheets ? LoadSheets(workbook) : new[] { workbook.GetSheetAt(0) };

            foreach (var worksheet in sheets)
            {
                try
                {
                    var rows = worksheet.LastRowNum;
                    if (rows < 3)
                    {
                        continue;
                    }

                    className = allowMultipleSheets
                        ? MakeValidClassName($"{Path.GetFileNameWithoutExtension(excelPath)}_{worksheet.SheetName}")
                        : MakeValidClassName(Path.GetFileNameWithoutExtension(excelPath));
                    var sb = new StringBuilder();
                    CsWarningHeader(sb);
                    sb.AppendLine("using System;");
                    sb.AppendLine("using System.Collections.Generic;");
                    sb.AppendLine("using System.IO;");
                    sb.AppendLine("using UnityEngine;");
                    sb.AppendLine();
                    sb.AppendLine($"[Serializable]");
                    sb.AppendLine($"public class {className} : BaseJsonData");
                    sb.AppendLine("{");

                    var headers = worksheet.GetRow(0).Cells;
                    var types = worksheet.GetRow(1).Cells;
                    var descriptions = worksheet.GetRow(2).Cells;

                    if (!headers.ElementAt(0).StringCellValue.Equals("key", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new Exception("The first column must be 'key'.");
                    }

                    if (!types.ElementAt(0).StringCellValue.Equals("int", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new Exception("The type of the first column must be 'int'.");
                    }

                    for (int i = 0; i < headers.Count; i++)
                    {
                        var variableName = headers.ElementAt(i).StringCellValue;
                        var dataType = types.ElementAt(i).StringCellValue;
                        var description = descriptions.ElementAtOrDefault(i)?.StringCellValue ?? "No description provided.";
                        
                        if (variableName == "key")
                        {
                            continue;
                        }

                        if ((dataType.StartsWith("Enum<") || dataType.StartsWith("List<Enum<")) && enumMappings == null)
                        {
                            throw new Exception($"Enum definition file not found, but type {dataType} requires it.");
                        }

                        if (dataType.StartsWith("Enum<"))
                        {
                            var enumTypeName = dataType.Substring(5, dataType.Length - 6);
                            if (!enumMappings.ContainsKey(enumTypeName))
                            {
                                throw new Exception($"Enum type '{enumTypeName}' not found in Enum definitions.");
                            }
                            dataType = $"DesignEnums.{enumTypeName}";
                        }

                        if (dataType.StartsWith("List<Enum<"))
                        {
                            var enumTypeName = dataType.Substring(10, dataType.Length - 12);
                            if (!enumMappings.ContainsKey(enumTypeName))
                            {
                                throw new Exception($"Enum type '{enumTypeName}' not found in Enum definitions.");
                            }
                            dataType = $"List<DesignEnums.{enumTypeName}>";
                        }

                        sb.AppendLine($"    /// <summary>");
                        string[] lines = description.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                        foreach (var line in lines)
                        {
                            sb.AppendLine($"    /// {line}");
                        }
                        sb.AppendLine($"    /// </summary>");
                        sb.AppendLine($"    public {dataType} {variableName};");
                        sb.AppendLine();
                    }

                    sb.AppendLine("}");

                    sb.AppendLine($"public class {className}Loader : JsonLoader");
                    sb.AppendLine("{");
                    sb.AppendLine($"    public {className}Loader(Func<string, TextAsset> loadFunc)");
                    sb.AppendLine("    {");
                    sb.AppendLine($"        AddDatas(loadFunc(nameof({className})).text);");
                    sb.AppendLine("    }");
                    sb.AppendLine();
                    sb.AppendLine("    private void AddDatas(string jsonData)");
                    sb.AppendLine("    {");
                    sb.AppendLine("        base.AddDatas(JsonUtility.FromJson<Wrapper>(jsonData).Items);");
                    sb.AppendLine("    }");
                    sb.AppendLine();
                    sb.AppendLine($"    [Serializable]");
                    sb.AppendLine($"    private class Wrapper");
                    sb.AppendLine("    {");
                    sb.AppendLine($"        public List<{className}> Items;");
                    sb.AppendLine("    }");
                    sb.AppendLine();
                    sb.AppendLine("}");

                    var jsonArray = new List<Dictionary<string, object>>();
                    var keySet = new HashSet<int>();

                    for (int i = 3; i <= worksheet.LastRowNum; i++)
                    {
                        bool isKeyEmpty = false;
                        var row = worksheet.GetRow(i);
                        var rowDict = new Dictionary<string, object>();

                        for (int j = 0; j < headers.Count; j++)
                        {
                            var variableName = headers.ElementAt(j).StringCellValue;
                            var dataType = types.ElementAt(j).StringCellValue;
                            var cellValue = GetCellText(row.GetCell(j));

                            if (variableName == "key" && (string.IsNullOrWhiteSpace(cellValue) || cellValue.StartsWith("#")))
                            {
                                isKeyEmpty = true;
                                break;
                            }

                            var convertedValue = ConvertToType(
                                cellValue,
                                dataType,
                                variableName,
                                logFilePath,
                                excelPath,
                                worksheet.SheetName
                            );

                            if (variableName == "key" && !keySet.Add((int)convertedValue))
                            {
                                throw new Exception(
                                    $"Duplicate key value '{convertedValue}' found in sheet '{worksheet.SheetName}' of file '{excelPath}'"
                                );
                            }

                            rowDict[variableName] = convertedValue;
                        }
                        if (isKeyEmpty == false)
                            jsonArray.Add(rowDict);
                    }

                    var classCode = sb.ToString();
                    var loaderOutputPath = Path.Combine(loaderDir, $"{className}.cs");
                    File.WriteAllText(loaderOutputPath, classCode);
                    Debug.Log($"Class file generated at {className}");
                    var jsonOutputPath = Path.Combine(jsonDir, $"{className}.json");
                    var wrapper = new { Items = jsonArray };
                    var settings = new JsonSerializerSettings
                    {
                        Formatting = Formatting.Indented,
                        StringEscapeHandling = StringEscapeHandling.Default
                    };
                    var jsonData = JsonConvert.SerializeObject(wrapper, settings);
                    File.WriteAllText(jsonOutputPath, jsonData);
                    Debug.Log($"JSON file generated at {className}\n");

                    // 파일 처리 후, 빈 값이 감지되었을 때 메시지 출력
                    if (emptyValueDetectedInFile)
                    {
                        Debug.LogWarning($"Warning: Empty values detected in file '{excelPath}'. Default values were used.");
                        LogError(logFilePath, $"Empty values detected in file '{excelPath}'. Default values were used.");
                    }
                }
                catch (Exception ex)
                {
                    LogError(
                        logFilePath,
                        $"Error processing sheet {worksheet.SheetName} in file {excelPath}: {ex.Message}\n{ex.StackTrace}"
                    );
                    Debug.LogError($"Error processing sheet {worksheet.SheetName} in file {excelPath}: {ex.Message}");
                    return false;
                }
            }
            return true;
        }
        catch (Exception ex)
        {
            LogError(logFilePath, $"Error processing file {excelPath}: {ex.Message}\n{ex.StackTrace}");
            Debug.LogError($"Error processing file {excelPath}: {ex.Message}");
            return false;
        }
    }

    static object ConvertToType(string value, string type, string variableName, string logFilePath, string excelPath, string sheetName)
    {
        try
        {
            // 빈 칸일 경우 기본값 설정
            if (string.IsNullOrWhiteSpace(value))
            {
                // 파일 내에서 처음으로 빈 칸이 발견되면 플래그 설정
                if (!emptyValueDetectedInFile)
                {
                    emptyValueDetectedInFile = true;
                }

                // 리스트 타입에서 빈 칸을 허용하지 않도록 메시지 출력
                if (type.StartsWith("List<"))
                {
                    throw new Exception(
                        $"Empty values are not supported for list types. Variable '{variableName}' in sheet '{sheetName}' of file '{excelPath}' contains an empty value."
                    );
                }

                // Enum 타입에서 빈 칸을 허용하지 않도록 메시지 출력
                if (type.StartsWith("Enum<"))
                {
                    throw new Exception(
                        $"Empty values are not supported for Enum types. Variable '{variableName}' in sheet '{sheetName}' of file '{excelPath}' contains an empty value."
                    );
                }

                // 타입에 따른 기본값 처리
                switch (type)
                {
                    case "int":
                        return 0; // 기본값 0
                    case "long":
                        return 0L; // 기본값 0
                    case "float":
                        return 0.0f; // 기본값 0.0f
                    case "double":
                        return 0.0; // 기본값 0.0
                    case "bool":
                        return false; // 기본값 false
                    case "string":
                        return ""; // 기본값 빈 문자열
                    default:
                        throw new Exception($"Unsupported data type: {type}");
                }
            }

            if (type.StartsWith("List<Enum<"))
            {
                var enumTypeName = type.Substring(10, type.Length - 12);
                if (!enumMappings.ContainsKey(enumTypeName))
                {
                    throw new Exception($"Enum type '{enumTypeName}' not found in Enum definitions.");
                }
                var enumMap = enumMappings[enumTypeName];
                string listText = value.TrimStart('[').TrimEnd(']');
                if (string.IsNullOrWhiteSpace(listText)) return Array.Empty<object>();
                return listText.Split(',').Select(v => enumMap[v.Trim()]).ToList();
            }
            else if (type.StartsWith("List<"))
            {
                var itemType = type.Substring(5, type.Length - 6);
                string listText = value.TrimStart('[').TrimEnd(']');
                if (string.IsNullOrWhiteSpace(listText)) return Array.Empty<object>();
                if (itemType == "int")
                {
                    return listText.Split(',').Select(int.Parse).ToList();
                }
                else if (itemType == "long")
                {
                    return listText.Split(',').Select(long.Parse).ToList();
                }
                else if (itemType == "float")
                {
                    return listText.Split(',').Select(float.Parse).ToList();
                }
                else if (itemType == "double")
                {
                    return listText.Split(',').Select(double.Parse).ToList();
                }
                else if (itemType == "bool")
                {
                    return listText.Split(',').Select(bool.Parse).ToList();
                }
                else
                {
                    return listText.Split(',').Select(v => v.Trim()).ToList();
                }
            }
            else if (type == "int")
            {
                return int.Parse(value);
            }
            else if (type == "long")
            {
                return long.Parse(value);
            }
            else if (type == "float")
            {
                return float.Parse(value);
            }
            else if (type == "double")
            {
                return double.Parse(value);
            }
            else if (type == "bool")
            {
                return bool.Parse(value);
            }
            else if (type.StartsWith("Enum<"))
            {
                var enumTypeName = type.Substring(5, type.Length - 6);
                if (!enumMappings.ContainsKey(enumTypeName))
                {
                    throw new Exception($"Enum type '{enumTypeName}' not found in Enum definitions.");
                }
                var enumMap = enumMappings[enumTypeName];
                return enumMap[value];
            }
            else if (type == "string")
            {
                return value;
            }
            else
            {
                throw new Exception($"Unsupported data type: {type}");
            }
        }
        catch (Exception ex)
        {
            LogError(
                logFilePath,
                $"Error converting value '{value}' for variable '{variableName}' in sheet '{sheetName}' of file '{excelPath}': {ex.Message}\n{ex.StackTrace}"
            );
            throw;
        }
    }

    static string MakeValidClassName(string name)
    {
        var sb = new StringBuilder();
        foreach (var c in name)
        {
            if (char.IsLetterOrDigit(c) || c == '_')
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }

    static void LogError(string logFilePath, string message)
    {
        try
        {
            using (StreamWriter sw = File.AppendText(logFilePath))
            {
                sw.WriteLine($"{DateTime.Now}: {message}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to write to log file: {ex.Message}");
        }
    }

    static void CsWarningHeader(StringBuilder sb)
    {
        sb.AppendLine("// ************************************************************************");
        sb.AppendLine("// Auto generated class from Excel file");
        sb.AppendLine("// Do not modify this file directly.");
        sb.AppendLine("// ************************************************************************");
    }
}