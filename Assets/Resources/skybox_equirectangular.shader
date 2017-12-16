// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Skybox/Equirectangular" {

/* The Properties block contains shader variables (textures, colors etc.) that will be saved as part of the Material */
	Properties{
		_Tint("Tint Color", Color) = (.5, .5, .5, .5)
		[Gamma] _Exposure("Exposure", Range(0, 8)) = 1.0
		_Rotation("Rotation", Range(0, 360)) = 0
		[NoScaleOffset] _Tex("Panorama (HDR)", 2D) = "grey" {}
	}

/* Code of the shader executed for every pixel of the image */
    SubShader {
        /* Tags tell Unity certain properties of the shader */
        Tags{ "Queue" = "Background" "RenderType" = "Background" "PreviewType" = "Skybox" }
        Cull Off ZWrite Off
    
    

/* Each SubShader is composed of a number of passes, and each Pass represents an execution of the vertex and fragment code for the same object rendered with the material of the shader. */
        Pass{
    
/* These keywords surround portions of HLSL code within the vertex and fragment shaders. */
            CGPROGRAM
                // Vertex and fragment shader
                #pragma vertex vert
                #pragma fragment frag
                
                #include "UnityCG.cginc"
                
                    sampler2D _Tex; // Texture used
                    half4 _Tex_HDR;
                    half4 _Tint; // Color
                    
                    half _Exposure;
                    float _Rotation;
                
                    float4 RotateAroundYInDegrees(float4 vertex, float degrees)
                    {
                        float alpha = degrees * UNITY_PI / 180.0;
                        float sina, cosa;
                        sincos(alpha, sina, cosa);
                        float2x2 m = float2x2(cosa, -sina, sina, cosa);
                        return float4(mul(m, vertex.xz), vertex.yw).xzyw;
                    }
                
                    struct appdata_t {
                        float4 vertex : POSITION;
                    };
                
                    struct v2f {
                        float4 vertex : SV_POSITION;
                        float3 texcoord : TEXCOORD0;
                    };
                
/* The Vertex Shader is a program that runs on each vertex of the 3D model.*/  
                    v2f vert(appdata_t v)
                    {
                        v2f o;
                        o.vertex = UnityObjectToClipPos(RotateAroundYInDegrees(v.vertex, _Rotation));
                        o.texcoord = v.vertex.xyz;
                        return o;
                    }
                
/* The Fragment Shader is a program that runs on each and every pixel that object occupies on-screen, and is usually used to calculate and output the color of each pixel */
                    fixed4 frag(v2f i) : SV_Target
                    {
                        float3 dir = normalize(i.texcoord);
                        float2 longlat = float2(atan2(dir.x, dir.z) + UNITY_PI, acos(-dir.y));
                        float2 uv = longlat / float2(2.0 * UNITY_PI, UNITY_PI);
                        half4 tex = tex2D(_Tex, uv);
                        half3 c = DecodeHDR(tex, _Tex_HDR);
                        c = c * _Tint.rgb * unity_ColorSpaceDouble.rgb;
                        c *= _Exposure;
                
                        return half4(c, 1);
                    }
                ENDCG
        }
	}
		Fallback Off
}