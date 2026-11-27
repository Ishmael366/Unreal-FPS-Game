using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
public static class PortfolioWebGLBuild {
public static void Run() {
try {
var args = System.Environment.GetCommandLineArgs();
var output = args[System.Array.IndexOf(args, "-portfolioOutput") + 1];
var scenes = System.IO.File.ReadAllLines("portfolio-scenes.txt").Where(p => !String.IsNullOrWhiteSpace(p)).ToArray();
Debug.Log("PORTFOLIO_ORIGINAL_BACKGROUND " + PlayerSettings.runInBackground);
PlayerSettings.runInBackground = true;
PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
PlayerSettings.WebGL.dataCaching = true;
PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly;
PlayerSettings.defaultWebScreenWidth = 1280;
PlayerSettings.defaultWebScreenHeight = 720;
var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = scenes, locationPathName = output, target = BuildTarget.WebGL, options = BuildOptions.None });
Debug.Log("PORTFOLIO_BUILD_RESULT " + report.summary.result + " " + report.summary.totalSize);
EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 1);
} catch (Exception e) { Debug.LogException(e); EditorApplication.Exit(1); }
}
}
