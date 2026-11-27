using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.Build;
using UnityEngine;
public static class NativeBatchBuilder
{
 [Serializable]class Receipt{public string status,output,unityVersion;public long bytes;public int errors,warnings;public string[] scenes;}
 public static void Build(){
  var args=Environment.GetCommandLineArgs();string output=null;for(int i=0;i<args.Length-1;i++)if(args[i]=="-demoOutput")output=args[i+1];if(output==null)throw new Exception("Missing output path");
  var port=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("BrowserProjectPort")).FirstOrDefault(t=>t!=null);if(port!=null)port.GetMethod("Prepare").Invoke(null,null);
  var scenes=EditorBuildSettings.scenes.Where(s=>s.enabled&&File.Exists(s.path)).Select(s=>s.path).ToArray();
  if(scenes.Length==0)scenes=AssetDatabase.FindAssets("t:Scene",new[]{"Assets"}).Select(AssetDatabase.GUIDToAssetPath).Where(p=>!p.Contains("/Editor/")&&!p.Contains("/Examples/")&&!p.Contains("/DemoScenes/")).OrderBy(p=>Rank(p)).ThenBy(p=>p).ToArray();
  if(scenes.Length==0)throw new Exception("NO_PLAYABLE_SCENE: This source contains no runtime scene.");
  PlayerSettings.SetStackTraceLogType(LogType.Exception,StackTraceLogType.Full);
  PlayerSettings.SetIl2CppCodeGeneration(NamedBuildTarget.WebGL,Il2CppCodeGeneration.OptimizeSize);
  PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Gzip;PlayerSettings.WebGL.decompressionFallback=true;PlayerSettings.WebGL.dataCaching=true;PlayerSettings.companyName="Daniel Nguyen";PlayerSettings.runInBackground=true;EditorUserBuildSettings.development=false;
  var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=scenes,locationPathName=output,target=BuildTarget.WebGL,options=File.Exists("Assets/BrowserDebug.txt")?BuildOptions.Development:BuildOptions.None});var s=report.summary;Directory.CreateDirectory(output);var receipt=new Receipt{status=s.result.ToString(),output=output,unityVersion=Application.unityVersion,bytes=(long)s.totalSize,errors=(int)s.totalErrors,warnings=(int)s.totalWarnings,scenes=scenes};File.WriteAllText(Path.Combine(output,"build-receipt.json"),JsonUtility.ToJson(receipt,true));Debug.Log("NATIVE_DEMO_RESULT "+receipt.status+" "+receipt.bytes);if(s.result!=BuildResult.Succeeded)throw new Exception("Build failed");
 }
 static int Rank(string p){string n=Path.GetFileNameWithoutExtension(p).ToLowerInvariant();if(n.Contains("mainmenu")||n=="menu"||n.Contains("titlescreen")||n=="title")return 0;if(n=="main"||n.Contains("gamescene")||n.Contains("level01")||n.Contains("level1"))return 1;if(n.Contains("sample"))return 3;return 2;}
}
