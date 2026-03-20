Shader "HighlightPlus/Geometry/SeeThrough" {
	Properties {
		_MainTex ("Texture", any) = "white" {}
		_SeeThrough ("See Through", Range(0, 1)) = 0.8
		_SeeThroughTintColor ("See Through Tint Color", Vector) = (1,0,0,0.8)
		_SeeThroughNoise ("Noise", Float) = 1
		_Color ("Color", Vector) = (1,1,1,1)
		_CutOff ("CutOff", Float) = 0.5
		_SeeThroughStencilRef ("Stencil Ref", Float) = 2
		_SeeThroughStencilComp ("Stencil Comp", Float) = 5
		_SeeThroughStencilPassOp ("Stencil Pass Operation", Float) = 0
		_SeeThroughDepthOffset ("Depth Offset", Float) = 0
		_SeeThroughMaxDepth ("Max Depth", Float) = 0
		_SeeThroughTexture ("Mask Texture", 2D) = "white" {}
		_SeeThroughTextureScale ("Mask Texture Scale", Float) = 1
		_Cull ("Cull Mode", Float) = 2
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		sampler2D _MainTex;
		fixed4 _Color;
		struct Input
		{
			float2 uv_MainTex;
		};
		
		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
}