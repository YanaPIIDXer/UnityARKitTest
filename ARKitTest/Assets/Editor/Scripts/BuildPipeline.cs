using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;

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
		var Options = new BuildPlayerOptions();
        Options.locationPathName = "Builds/UnityARKitTest";
        Options.scenes = AssetDatabase.GetAllAssetPaths().Where(_ => Path.GetExtension(_) == ".unity").ToArray();
		Options.target = BuildTarget.iOS;
		Options.options = BuildOptions.Development;

		string PrevTeamID = PlayerSettings.iOS.appleDeveloperTeamID;

		Debug.Log("Start Build.");
		
		var Summary = BuildPipeline.BuildPlayer(Options).summary;

		PlayerSettings.iOS.appleDeveloperTeamID = PrevTeamID;
		PlayerSettings.iOS.appleDeveloperTeamID = LocalSettings.AppleDeveloperTeamID;

		if(Summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
		{
			Debug.Log("Fuck!!");
			return;
		}

		Debug.Log("Build Success!");
	}
}
