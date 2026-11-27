Shader "Portfolio/WindGrass" {
 Properties { _BottomColor("Root color",Color)=(.08,.18,.05,1) _TopColor("Tip color",Color)=(.45,.6,.18,1) _WindStrength("Wind",Range(0,2))=.5 }
 SubShader { Tags { "RenderType"="Opaque" } Cull Off
 CGPROGRAM
 #pragma surface surf Standard vertex:vert addshadow
 #pragma target 3.0
 #include "UnityCG.cginc"
 fixed4 _BottomColor,_TopColor;float _WindStrength;
 struct Input {float2 bladeUV;};
 void vert(inout appdata_full v,out Input o){UNITY_INITIALIZE_OUTPUT(Input,o);float3 w=mul(unity_ObjectToWorld,v.vertex).xyz;float sway=sin(w.x*.7+w.z*.9+_Time.y*1.8)*_WindStrength*v.texcoord.y*v.texcoord.y;v.vertex.x+=sway*.22;v.vertex.z+=sway*.12;o.bladeUV=v.texcoord.xy;}
 void surf(Input i,inout SurfaceOutputStandard o){o.Albedo=lerp(_BottomColor.rgb,_TopColor.rgb,i.bladeUV.y);o.Smoothness=.12;o.Alpha=1;}
 ENDCG }
 Fallback "Diffuse"
}
