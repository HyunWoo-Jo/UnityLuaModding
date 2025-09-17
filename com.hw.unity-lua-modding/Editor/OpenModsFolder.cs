using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class OpenModsFolder
{
    [MenuItem("Tools/Open Mods Folder")]
    public static void OpenFolder() {
        string folderPath = Application.persistentDataPath;

        // Check if the folder exists (폴더가 존재하는지 확인)
        if (!System.IO.Directory.Exists(folderPath)) {
            Debug.LogWarning($"Folder does not exist: {folderPath}");
            return;
        }

        // Open folder using different methods depending on the operating system (운영체제에 따라 다른 방식으로 폴더 열기)
        try {
#if UNITY_EDITOR_WIN
            // Windows
            Process.Start("explorer.exe", folderPath.Replace('/', '\\'));
#elif UNITY_EDITOR_OSX
            // macOS
            Process.Start("open", folderPath);
#elif UNITY_EDITOR_LINUX
            // Linux
            Process.Start("xdg-open", folderPath);
#else
            // Default: Open with system default file explorer (기본값: 시스템 기본 파일 탐색기로 열기)
            Process.Start(folderPath);
#endif
           Debug.Log($"Folder opened: {folderPath}");
        } catch (System.Exception e) {
           Debug.LogError($"Failed to open folder: {e.Message}");
        }
    }
}
