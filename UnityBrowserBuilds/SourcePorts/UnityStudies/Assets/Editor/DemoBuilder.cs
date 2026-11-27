using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
public static class DemoBuilder
{
    [Serializable]sealed class Receipt{public string status,output,unityVersion;public long bytes;public int errors,warnings;}
    public static void Build()
    {
        string output=null;var args=Environment.GetCommandLineArgs();for(int i=0;i<args.Length-1;i++)if(args[i]=="-demoOutput")output=args[i+1];if(string.IsNullOrEmpty(output))throw new ArgumentException("-demoOutput is required");
        var standard=Shader.Find("Standard");if(standard==null)throw new Exception("Editor Standard shader is unavailable");
        const string materialPath="Assets/Resources/Materials/StudyDefault.mat";var material=new Material(standard);material.color=new Color(.5f,.5f,.5f);AssetDatabase.DeleteAsset(materialPath);AssetDatabase.CreateAsset(material,materialPath);
        var settings=AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset");if(settings.Length>0){var serialized=new SerializedObject(settings[0]);var shaders=serialized.FindProperty("m_AlwaysIncludedShaders");if(shaders!=null){foreach(var shader in new[]{standard,Shader.Find("Portfolio/WindGrass")}){if(shader==null)continue;bool found=false;for(int i=0;i<shaders.arraySize;i++)if(shaders.GetArrayElementAtIndex(i).objectReferenceValue==shader)found=true;if(!found){int n=shaders.arraySize;shaders.InsertArrayElementAtIndex(n);shaders.GetArrayElementAtIndex(n).objectReferenceValue=shader;}}serialized.ApplyModifiedPropertiesWithoutUndo();}}AssetDatabase.SaveAssets();
        PlayerSettings.companyName="Daniel Nguyen";PlayerSettings.productName="Unity Studies";PlayerSettings.runInBackground=true;
        PlayerSettings.WebGL.template="PROJECT:Studies";PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Gzip;PlayerSettings.WebGL.decompressionFallback=true;PlayerSettings.WebGL.dataCaching=true;
        EditorUserBuildSettings.development=false;EditorUserBuildSettings.allowDebugging=false;
        var options=new BuildPlayerOptions{scenes=new[]{"Assets/Scenes/Main.unity"},locationPathName=output,target=BuildTarget.WebGL,options=BuildOptions.None};
        var report=BuildPipeline.BuildPlayer(options);var summary=report.summary;var receipt=new Receipt{status=summary.result.ToString(),output=output,unityVersion=Application.unityVersion,bytes=(long)summary.totalSize,errors=(int)summary.totalErrors,warnings=(int)summary.totalWarnings};
        Directory.CreateDirectory(output);File.WriteAllText(Path.Combine(output,"build-receipt.json"),JsonUtility.ToJson(receipt,true));Debug.Log("DEMO_BUILD_RESULT "+receipt.status+" "+receipt.bytes);
        if(summary.result!=BuildResult.Succeeded)throw new Exception("WebGL build failed");
    }
}
