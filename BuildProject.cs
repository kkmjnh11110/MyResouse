/**  *****************************Description:一键发布*****************************
 *Copyright(C) 2019 by DefaultCompany
 *All rights reserved.
 *ProductName:  Dungeon3
 *Author:       gaoyc，futf-Tony，lilc
 *Version:      1.0
 *UnityVersion: 2018.4.0f1
 *CreateTime:   August 21st, 2019 3:05pm
 */
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CSObjectWrapEditor;
using TJ;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
public static class BuildProject
{
    #region -------------------------------变量-------------------------------
    public static List<string> levels = new List<string>();
    #endregion

    #region -------------------------------菜单功能-------------------------------
    //windows平台打包
    /// <summary>
    /// windows平台打包
    /// </summary>
    [MenuItem("Build/Windows", false, 1)]
    static void BuildWindows()
    {
        BuildAssetBundlesPC();
#if (UNITY_STANDALONE_WIN == true)

#endif
    }

    //android平台打包
    /// <summary>
    /// android平台打包
    /// </summary>
    [MenuItem("Build/Android", false, 2)]
    static void BuildAndroid()
    {
        BuildAssetBundlesAndroid();//打包到外部
    }

    //ios平台打包
    /// <summary>
    /// ios平台打包
    /// </summary>
    [MenuItem("Build/IOS", false, 3)]
    static void BuildIOS()
    {
        BuildAssetBundlesIOS();
#if (UNITY_IOS == true)

#endif
    }

    //删除外部AB包目录
    /// <summary>
    /// 删除外部AB包目录
    /// </summary>
    [MenuItem("Build/Clean ExternalAB", false, 14)]
    static void CleanExternalAB()
    {
        // 判断平台，再删除平台对应的目录
        string currentPath = System.Environment.CurrentDirectory;
        string buildPath = "";
#if UNITY_STANDALONE_WIN
        buildPath = Path.Combine(currentPath, Config.AB_Build_Hotfix_Path_PC).Replace("\\", "/");
#endif

#if UNITY_ANDROID
        buildPath = Path.Combine(currentPath, Config.AB_Build_Hotfix_Path_Android).Replace("\\", "/");
#endif

#if UNITY_IOS
        buildPath = Path.Combine(currentPath, Config.AB_Build_Hotfix_Path_IOS).Replace("\\", "/");
#endif
        DeleteDirectory(buildPath);
    }

    //删除StreamingAsset目录下的AB包
    /// <summary>
    /// 删除StreamingAsset目录下的AB包
    /// </summary>
    [MenuItem("Build/Clean StreamingAssetsAB", false, 15)]
    static void CleanStreamingAssetsAB()
    {
        // 判断平台，再删除平台对应的目录
        DeleteDirectory(Config.AB_Build_Path);
    }

    //导出AB包并拷贝到外部热更位置(PC)
    /// <summary>
    /// 导出AB包并拷贝到外部热更位置(PC)
    /// </summary>
    [MenuItem("Build/Build AssetBundlesPC")]
    static public void BuildAssetBundlesPC()
    {
        // 清理XLua生成的代码,要在切换平台前清除否则切换后会报错.
        DeleteDirectory(Config.XluaGeneratePath);
        AssetDatabase.Refresh();
#if (UNITY_STANDALONE_WIN == false)
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.StandaloneWindows64); //切换平台
        AssetDatabase.Refresh();
#else
        levels.Clear();
        levels.Add("Assets/Scenes/Launch.unity");
        // 重新生成XLua代码
        Generator.GenAll();
        AssetBundleBuilder builder = new AssetBundleBuilder(Config.AB_Build_Hotfix_Path_PC, Config.AssetBundleBuildRulePath);
        builder.Begin();
        builder.ParseRule();
        builder.Export();
        builder.End();
        CopyVersionFile(Config.AB_Build_Hotfix_Path_PC);
        Debug.Log("Build AssetBundles OK!");
        builder = null;

        //删除Assets/streamingAssets下的目录，减少打包PC端时间
        DeleteDirectory(Path.Combine(System.Environment.CurrentDirectory, Config.AB_Build_Path + "/assets").Replace("\\", "/"));

        //上一次外部构建出来的目录
        string currentPath = System.Environment.CurrentDirectory;
        string buildPath = Path.Combine(currentPath, "Build/Windows/").Replace("\\", "/");

        //重新构建项目
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = levels.ToArray();
        buildPlayerOptions.locationPathName = buildPath + Application.productName + ".exe";
        buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
        buildPlayerOptions.options = BuildOptions.None;
        Build(buildPlayerOptions);
#endif
    }

    //导出AB包并拷贝到外部热更位置(Android)
    /// <summary>
    /// 导出AB包并拷贝到外部热更位置(Android)
    /// </summary>
    [MenuItem("Build/Build AssetBundlesAndroid")]
    static public void BuildAssetBundlesAndroid()
    {
        // 清理XLua生成的代码,要在切换平台前清除否则切换后会报错.
        DeleteDirectory(Config.XluaGeneratePath);
        AssetDatabase.Refresh();

#if (UNITY_ANDROID == false)
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.Android); //切换平台
        AssetDatabase.Refresh();
        return;
#else
        levels.Clear();
        levels.Add("Assets/Scenes/Launch.unity");
        // 重新生成XLua代码
        Generator.GenAll();
        AssetBundleBuilder builder = new AssetBundleBuilder(Config.AB_Build_Hotfix_Path_Android, Config.AssetBundleBuildRulePath);
        builder.Begin();
        builder.ParseRule();
        builder.Export();
        builder.End();
        // copy version.manifest to target
        CopyVersionFile(Config.AB_Build_Hotfix_Path_Android);
        CopyABFile(Config.AB_Build_Hotfix_Path_Android, Config.AB_Build_Path);
        Debug.Log("Build AssetBundles OK!");
        builder = null;

        string currentPath = System.Environment.CurrentDirectory;
        string buildPath = Path.Combine(currentPath, "Build/Android/").Replace("\\", "/");

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = levels.ToArray();
        buildPlayerOptions.locationPathName = buildPath + Application.productName + ".apk";
        buildPlayerOptions.target = BuildTarget.Android;
        buildPlayerOptions.options = BuildOptions.None;
        Build(buildPlayerOptions);
#endif
    }

    //导出AB包并拷贝到外部热更位置(IOS)
    /// <summary>
    /// 导出AB包并拷贝到外部热更位置(IOS)
    /// </summary>
    [MenuItem("Build/Build AssetBundlesIOS")]
    static public void BuildAssetBundlesIOS()
    {
        // 清理XLua生成的代码,要在切换平台前清除否则切换后会报错.
        DeleteDirectory(Config.XluaGeneratePath);
        AssetDatabase.Refresh();
#if (UNITY_IOS == false)
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.iOS); //切换平台
        AssetDatabase.Refresh();
#else
        levels.Clear();
        levels.Add("Assets/Scenes/Launch.unity");
        // 重新生成XLua代码
        Generator.GenAll();
        AssetBundleBuilder builder = new AssetBundleBuilder(Config.AB_Build_Hotfix_Path_IOS, Config.AssetBundleBuildRulePath);
        builder.Begin();
        builder.ParseRule();
        builder.Export();
        builder.End();
        CopyVersionFile(Config.AB_Build_Hotfix_Path_IOS);
        CopyABFile(Config.AB_Build_Hotfix_Path_IOS, Config.AB_Build_Path);
        Debug.Log("Build AssetBundles OK!");
        builder = null;

        string currentPath = System.Environment.CurrentDirectory;
        string buildPath = Path.Combine(currentPath, "Build/IOS/").Replace("\\", "/");

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = levels.ToArray();
        buildPlayerOptions.locationPathName = buildPath + Application.productName + "Xcode";
        buildPlayerOptions.target = BuildTarget.iOS;
        buildPlayerOptions.options = BuildOptions.None;
        Build(buildPlayerOptions);
#endif
    }

    #endregion

    #region -------------------------------调用方法-------------------------------
    //切换平台
    /// <summary>
    /// 切换平台
    /// </summary>
    /// <param name="BuildTargetName"></param>
    static void SwitchBuildTarget(string BuildTargetName)
    {
        // 清理XLua生成的代码,要在切换平台前清除否则切换后会报错.
        DeleteDirectory(Config.XluaGeneratePath);

        if (BuildTargetName == "Windows")
        {
#if (UNITY_STANDALONE_WIN == false)
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.StandaloneWindows64); //切换平台
#endif
        }
        else if (BuildTargetName == "Android")
        {
#if (UNITY_ANDROID == false)
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.Android); //切换平台
#endif
        }
        else if (BuildTargetName == "iOS")
        {
#if (UNITY_IOS == false)
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.iOS); //切换平台
#endif
        }

    }

    //监听切换场景事件
    public class ActiveBuildTargetListener : IActiveBuildTargetChanged
    {
        public int callbackOrder { get { return 0; } }
        public void OnActiveBuildTargetChanged(BuildTarget previousTarget, BuildTarget newTarget)
        {
            Debug.Log("转换到什么平台： " + newTarget);

            if (newTarget == BuildTarget.StandaloneWindows64)
            {
                levels.Clear();
                levels.Add("Assets/Scenes/Launch.unity");

                // 重新生成XLua代码
                Generator.GenAll();
                AssetDatabase.Refresh();
                AssetBundleBuilder builder = new AssetBundleBuilder(Config.AB_Build_Hotfix_Path_PC, Config.AssetBundleBuildRulePath);
                builder.Begin();
                builder.ParseRule();
                builder.Export();
                builder.End();
                CopyVersionFile(Config.AB_Build_Hotfix_Path_PC);
                Debug.Log("Build AssetBundles OK!");
                builder = null;
                AssetDatabase.Refresh();
                //删除Assets/streamingAssets下的目录，减少打包PC端时间
                DeleteDirectory(Path.Combine(System.Environment.CurrentDirectory, Config.AB_Build_Path + "/assets").Replace("\\", "/"));
                AssetDatabase.Refresh();
                //上一次外部构建出来的目录
                string currentPath = System.Environment.CurrentDirectory;
                string buildPath = Path.Combine(currentPath, "Build/Windows/").Replace("\\", "/");

                //重新构建项目
                BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
                buildPlayerOptions.scenes = levels.ToArray();
                buildPlayerOptions.locationPathName = buildPath + Application.productName + ".exe";
                buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
                buildPlayerOptions.options = BuildOptions.None;
                Build(buildPlayerOptions);
            }
            else if (newTarget == BuildTarget.Android)
            {
                levels.Clear();
                levels.Add("Assets/Scenes/Launch.unity");
                // 重新生成XLua代码
                Generator.GenAll();
                AssetDatabase.Refresh();

                AssetBundleBuilder builder = new AssetBundleBuilder(Config.AB_Build_Hotfix_Path_Android, Config.AssetBundleBuildRulePath);
                builder.Begin();
                builder.ParseRule();
                builder.Export();
                builder.End();
                // copy version.manifest to target
                CopyVersionFile(Config.AB_Build_Hotfix_Path_Android);
                CopyABFile(Config.AB_Build_Hotfix_Path_Android, Config.AB_Build_Path);
                Debug.Log("Build AssetBundles OK!");
                AssetDatabase.Refresh();
                string currentPath = System.IO.Directory.GetCurrentDirectory();
                string buildPath = Path.Combine(currentPath, "Build/Android/").Replace("\\", "/");

                BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
                buildPlayerOptions.scenes = levels.ToArray();
                buildPlayerOptions.locationPathName = buildPath + Application.productName + ".apk";
                buildPlayerOptions.target = BuildTarget.Android;
                buildPlayerOptions.options = BuildOptions.None;
                Build(buildPlayerOptions);
                return;
            }
            else if (newTarget == BuildTarget.iOS)
            {
                levels.Clear();
                levels.Add("Assets/Scenes/Launch.unity");
                // 重新生成XLua代码
                Generator.GenAll();
                AssetDatabase.Refresh();
                AssetBundleBuilder builder = new AssetBundleBuilder(Config.AB_Build_Hotfix_Path_IOS, Config.AssetBundleBuildRulePath);
                builder.Begin();
                builder.ParseRule();
                builder.Export();
                builder.End();
                CopyVersionFile(Config.AB_Build_Hotfix_Path_IOS);
                CopyABFile(Config.AB_Build_Hotfix_Path_IOS, Config.AB_Build_Path);
                Debug.Log("Build AssetBundles OK!");
                builder = null;
                AssetDatabase.Refresh();
                string currentPath = System.Environment.CurrentDirectory;
                string buildPath = Path.Combine(currentPath, "Build/IOS/").Replace("\\", "/");

                BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
                buildPlayerOptions.scenes = levels.ToArray();
                buildPlayerOptions.locationPathName = buildPath + Application.productName + "Xcode";
                buildPlayerOptions.target = BuildTarget.iOS;
                buildPlayerOptions.options = BuildOptions.None;
                Build(buildPlayerOptions);
            }
        }
    }

    //发布
    /// <summary>
    /// 发布
    /// </summary>
    /// <param name="buildPlayerOptions">发布选项</param>
    public static void Build(BuildPlayerOptions buildPlayerOptions)
    {
        try
        {
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            //构建完成后的事件监听
            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log("Build succeeded: " + summary.totalSize + " bytes" + " output:" + summary.outputPath);
                // #if UNITY_STANDALONE_WIN
                if (buildPlayerOptions.target == BuildTarget.StandaloneWindows64)
                {
                    string dataPath = "Build/Windows/" + Application.productName + "_Data/StreamingAssets/assetbundles";
                    // CopyABFile(Config.AB_Build_Hotfix_Path_PC, Config.PC_StreamingAssets_Path);
                    CopyABFile(Config.AB_Build_Hotfix_Path_PC, dataPath);
                }
                // #endif       
            }

            if (summary.result == BuildResult.Failed)
            {
                Debug.LogError("Build failed! total errors:" + summary.totalErrors);
            }
        }
        catch (System.Exception)
        {

            throw;
        }
    }

    //删除上次的文件
    /// <summary>
    /// 删除上次的文件
    /// </summary>
    /// <param name="path">删除路径</param>
    public static void DeleteDirectory(string path)
    {
        DirectoryInfo info = new DirectoryInfo(path);
        if (info.Exists)
        {
            info.Delete(true);
        }
    }

    //xlua和ab包打包
    /// <summary>
    /// xlua和ab包打包
    /// </summary>
    static void PreBuild()
    {
        // 要打包的场景
        levels.Clear();
        // foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        // {
        //     if (scene.path == "Assets/Scenes/Launch.unity")
        //     {
        levels.Add("Assets/Scenes/Launch.unity");
        // }
        // }

        // 重新生成XLua代码
        Generator.GenAll();

        // 清理ab包
        // DeleteDirectory(Application.streamingAssetsPath);

        // 重新打ab包
        FrameworkMenuItems.BuildAssetBundles();

        // copy version.manifest to target
        CopyVersionFile(Config.AB_Build_Path);

    }

    //拷贝版本文件
    /// <summary>
    /// 拷贝版本文件
    /// </summary>
    public static void CopyVersionFile(string TargetPath)
    {
        string verfile = Path.Combine(System.Environment.CurrentDirectory, "Assets/version.manifest").Replace("\\", "/");
        string tgtfile = Path.Combine(System.Environment.CurrentDirectory, TargetPath + "/version.manifest").Replace("\\", "/");

        File.Copy(verfile, tgtfile, true);
        Debug.Log("copy verfile from:" + verfile + " to:" + tgtfile);
    }

    //复制文件夹所有文件
    /// <summary>
    /// 复制文件夹所有文件
    /// </summary>
    public static void CopyABFile(string CopyPath1, string CopyPath2)
    {
        string path1 = Path.Combine(System.Environment.CurrentDirectory, CopyPath1).Replace("\\", "/");
        string path2 = Path.Combine(System.Environment.CurrentDirectory, CopyPath2).Replace("\\", "/");
        DeleteDirectory(path2);//先删除一遍
        CopyFolder(path1, path2);
    }

    //复制文件夹所有文件
    /// <summary>
    /// 复制文件夹所有文件
    /// </summary>
    /// <param name="sourcePath">源目录</param>
    /// <param name="destPath">目的目录</param>
    public static void CopyFolder(string sourcePath, string destPath)
    {
        if (Directory.Exists(sourcePath))
        {
            if (!Directory.Exists(destPath))
            {
                Directory.CreateDirectory(destPath);
            }
            //获得源文件下所有文件
            List<string> files = new List<string>(Directory.GetFiles(sourcePath));
            files.ForEach(c =>
            {
                string destFile = Path.Combine(new string[] { destPath, Path.GetFileName(c) }).Replace("\\", "/");
                //Split分割字符串，只复制meta
                if ((destFile).Split('.').Length > 1)
                {
                    string destFile2 = (destFile).Split('.')[(destFile).Split('.').Length - 1];
                    if (destFile2 != "meta")
                    {
                        File.Copy(c, destFile, true);//覆盖模式
                    }
                }
                else
                {
                    File.Copy(c, destFile, true);//覆盖模式  
                }

            });
            //获得源文件下所有目录文件
            List<string> folders = new List<string>(Directory.GetDirectories(sourcePath));
            folders.ForEach(c =>
            {
                string destDir = Path.Combine(new string[] { destPath, Path.GetFileName(c) }).Replace("\\", "/");
                //采用递归的方法实现
                CopyFolder(c, destDir);
            });

        }
    }
    #endregion

    #region -------------------------------命令行模式编译项目-------------------------------
    // 命令行模式编译项目
    public static void BuildFromCliForWindow()
    {
        string currentPath = System.Environment.CurrentDirectory;
        string buildPath = Path.Combine(currentPath, "Build/Windows/").Replace("\\", "/");
        DirectoryInfo dir = new DirectoryInfo(buildPath);
        if (dir.Exists)
        {
            dir.Delete(true);
        }
        else
        {
            dir.Create();
        }

        // regenerate xlua code
        DeleteDirectory(Config.XluaGeneratePath);
        Generator.GenAll();

        // 重新打ab包
        BuildAssetBundlesFromCli();

        // copy version.manifest to target
        CopyVersionFile(Config.AB_Build_Path);

        levels.Clear();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (!scene.enabled)
                continue;
            levels.Add(scene.path);
        }

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = levels.ToArray();
        buildPlayerOptions.locationPathName = buildPath + Application.productName + ".exe";
        buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
        buildPlayerOptions.options = BuildOptions.None;

        Build(buildPlayerOptions);
    }

    public static void BuildFromCliForAndroid()
    {
        string currentPath = System.Environment.CurrentDirectory;
        string buildPath = Path.Combine(currentPath, "Build/Android/").Replace("\\", "/");
        DirectoryInfo dir = new DirectoryInfo(buildPath);
        if (dir.Exists)
        {
            dir.Delete(true);
        }
        else
        {
            dir.Create();
        }

        // regenerate xlua code
        DeleteDirectory(Config.XluaGeneratePath);
        Generator.GenAll();

        BuildAssetBundlesFromCli();

        // copy version.manifest to target
        CopyVersionFile(Config.AB_Build_Path);

        levels.Clear();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (!scene.enabled)
                continue;
            levels.Add(scene.path);
        }

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = levels.ToArray();
        buildPlayerOptions.locationPathName = buildPath + Application.productName + ".apk";
        buildPlayerOptions.target = BuildTarget.Android;
        buildPlayerOptions.options = BuildOptions.None;

        Build(buildPlayerOptions);
    }

    public static void BuildFromCliForAndroid2()
    {
        string currentPath = System.Environment.CurrentDirectory;
        string buildPath = Path.Combine(currentPath, "Build/Android/").Replace("\\", "/");
        DirectoryInfo dir = new DirectoryInfo(buildPath);
        if (dir.Exists)
        {
            dir.Delete(true);
        }
        else
        {
            dir.Create();
        }

        // regenerate xlua code
        DeleteDirectory(Config.XluaGeneratePath);
        Generator.GenAll();

        // BuildAssetBundlesFromCli();

        // copy version.manifest to target
        CopyVersionFile(Config.AB_Build_Path);

        levels.Clear();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (!scene.enabled)
                continue;
            levels.Add(scene.path);
        }

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = levels.ToArray();
        buildPlayerOptions.locationPathName = buildPath + Application.productName + ".apk";
        buildPlayerOptions.target = BuildTarget.Android;
        buildPlayerOptions.options = BuildOptions.None;

        Build(buildPlayerOptions);
    }

    public static void BuildAssetBundlesFromCli()
    {
        // 重新打ab包
        DeleteDirectory(Application.streamingAssetsPath);
        FrameworkMenuItems.BuildAssetBundles();
    }
    #endregion
}