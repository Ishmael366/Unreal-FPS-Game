Shader "Portfolio/BrowserProjectile" {Properties {_MainTex("Texture",2D)="white"{} _Color("Color",Color)=(1,1,1,1) _ZIndex("Z",Float)=.01} SubShader {Tags {"Queue"="Transparent" "RenderType"="Transparent"} Blend SrcAlpha OneMinusSrcAlpha Cull Off ZWrite Off Pass {CGPROGRAM
#pragma target 3.5
#pragma vertex vert
#pragma fragment frag
#pragma multi_compile_instancing
#include "UnityCG.cginc"
struct appdata {float4 vertex:POSITION;float2 uv:TEXCOORD0;UNITY_VERTEX_INPUT_INSTANCE_ID};struct v2f {float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};sampler2D _MainTex;float4 _MainTex_ST;
UNITY_INSTANCING_BUFFER_START(Props)
UNITY_DEFINE_INSTANCED_PROP(float4,_Color)
UNITY_INSTANCING_BUFFER_END(Props)
v2f vert(appdata v){UNITY_SETUP_INSTANCE_ID(v);v2f o;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=TRANSFORM_TEX(v.uv,_MainTex);o.color=UNITY_ACCESS_INSTANCED_PROP(Props,_Color);return o;}
fixed4 frag(v2f i):SV_Target{return tex2D(_MainTex,i.uv)*i.color;}
ENDCG
}}}