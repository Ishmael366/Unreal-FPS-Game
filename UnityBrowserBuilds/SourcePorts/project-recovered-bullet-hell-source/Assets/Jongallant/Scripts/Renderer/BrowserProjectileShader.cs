using UnityEngine; namespace Jongallant.BulletHell { public static class BrowserProjectileShader {public static Shader Find(string desktopName){
#if UNITY_WEBGL && !UNITY_EDITOR
return Resources.Load<Material>("BrowserProjectile").shader;
#else
return Shader.Find(desktopName);
#endif
}}}