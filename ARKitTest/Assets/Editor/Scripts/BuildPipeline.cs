using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using System.Collections.Generic;

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
        RunBuildPipline();
    }

    /// <summary>
    /// ビルドパイプラインの実行
    /// </summary>
    private static void RunBuildPipline()
    {
        Debug.Log("Start Build.");

        // ↓XCodeプロジェクト転送処理実装の為一旦封印。
        /*
        var Options = new BuildPlayerOptions();
        Options.locationPathName = "Builds/UnityARKitTest";
        Options.scenes = AssetDatabase.GetAllAssetPaths().Where(_ => Path.GetExtension(_) == ".unity").ToArray();
        Options.target = BuildTarget.iOS;
        Options.options = BuildOptions.Development;

        string PrevTeamID = PlayerSettings.iOS.appleDeveloperTeamID;

        var Summary = BuildPipeline.BuildPlayer(Options).summary;

        PlayerSettings.iOS.appleDeveloperTeamID = PrevTeamID;
        PlayerSettings.iOS.appleDeveloperTeamID = LocalSettings.AppleDeveloperTeamID;

        if (Summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log("Build Failed...");
            return;
        }
		*/
        Debug.Log("Build Success!");

        Debug.Log("Upload XCode Project.");

        if (!UploadXCodeProject())
        {
            Debug.Log("XCode Project Upload Failed...");
            return;
        }

        Debug.Log("Upload Success!");
    }

    /// <summary>
    /// XCodeのプロジェクトをアップロード
    /// </summary>
	/// <returns>成功したか？</returns>
    private static bool UploadXCodeProject()
    {
        Debug.Log("SSH Connection...");
        using (var Client = MakeSFTPConnection(LocalSettings.SSHHost, LocalSettings.SSHUserName, LocalSettings.SSHPassword))
        {
            if (Client == null)
            {
                Debug.Log("Connection Failed...");
                return false;
            }
            Debug.Log("Connection Success!");
        }

        return true;
    }

    /// <summary>
    /// SFTP接続
    /// </summary>
    /// <param name="Host">ホスト</param>
    /// <param name="UserName">ユーザ名</param>
    /// <param name="Password">パスワード</param>
    /// <returns>SftpClinetのインスタンス</returns>
    private static SftpClient MakeSFTPConnection(string Host, string UserName, string Password)
    {
        var AuthMethod = new PasswordAuthenticationMethod(UserName, Password);
        var ConnInfo = new ConnectionInfo(Host, UserName, AuthMethod);
        SftpClient Client = new SftpClient(ConnInfo);
        try
        {
            var ConnTask = Task.Run(() => Client.Connect());
            while (!ConnTask.IsCompleted)
            {
                EditorUtility.DisplayProgressBar("Upload XCode Project", "Connecting SFTP...", 0.0f);
            }
            EditorUtility.ClearProgressBar();
        }
        catch
        {
            return null;
        }
        return Client;
    }

    /// <summary>
    /// ディレクトリのアップロード
    /// </summary>
    /// <param name="Client">SFTPクライアント</param>
    /// <param name="LocalPath">ローカルのパス</param>
    /// <param name="RemotePath">リモートのパス</param>
    private static void UploadDirectory(SftpClient Client, string LocalPath, string RemotePath)
    {
        if (!Client.Exists(RemotePath))
        {
            Client.CreateDirectory(RemotePath);
        }

        IEnumerable<FileSystemInfo> Infos = new DirectoryInfo(LocalPath).EnumerateFileSystemInfos();
        foreach (FileSystemInfo Info in Infos)
        {
            string RemoteFilePath = RemotePath + "/" + Info.Name;
            if (Info.Attributes.HasFlag(FileAttributes.Directory))
            {
                UploadDirectory(Client, Info.FullName, RemoteFilePath);
            }
            else
            {
                using (var Fs = new FileStream(Info.FullName, FileMode.Open))
                {
                    Debug.Log("Upload:" + RemoteFilePath);
                    var Result = Client.BeginUploadFile(Fs, RemoteFilePath);
                    while (!Result.IsCompleted)
                    {
                        float Progress = (float)Fs.Position / Fs.Length;
                        EditorUtility.DisplayProgressBar("Update File", RemoteFilePath, Progress);
                    }
                    EditorUtility.ClearProgressBar();

                    Client.EndUploadFile(Result);
                    if (Info.Extension == ".sh")
                    {
                        Client.ChangePermissions(RemoteFilePath, 755);
                        Debug.Log("Added execute permission:" + RemoteFilePath);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 指定したディレクトリの完全な消去
    /// ※SftpClient.DeleteDirectoryメソッドはディレクトリの中身が空でないと失敗するらしいので。
    /// </summary>
    /// <param name="Client">SFTPクライアント</param>
    /// <param name="TargetPath">対象パス</param>
    private static void DeleteDirectory(SftpClient Client, string TargetPath)
    {
        foreach (SftpFile TargetFile in Client.ListDirectory(TargetPath))
        {
            if ((TargetFile.Name == ".") || (TargetFile.Name == "..")) { continue; }

            if (TargetFile.IsDirectory)
            {
                DeleteDirectory(Client, TargetFile.FullName);
            }
            else
            {
                Debug.Log("Delete File:" + TargetFile.FullName);
                Client.DeleteFile(TargetFile.FullName);
            }
        }

        Debug.Log("Delete Directory:" + TargetPath);
        Client.DeleteDirectory(TargetPath);
    }
}
