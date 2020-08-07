using UnityEngine;
using UnityEditor;

/// <summary>
/// ビルドパイプライン
/// </summary>
public class GameBuildPipeline
{
    /// <summary>
    /// iOSビルド
    /// </summary>
    [MenuItem("GameBuildPipeline/iOS Build")]
    public static void BuildForIOS()
    {
        string TargetPath = "Builds/UnityARKitTest";
        string[] Levels = new string[] { "SampleScene" };
        Debug.Log(TargetPath);
    }
}
