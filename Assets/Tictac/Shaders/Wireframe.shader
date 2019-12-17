Shader "Custom/Wireframe"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [PowerSlider(3.0)]
        _WireWidth ("Wire Width", Range(0., 0.34)) = 0.05
        _WireColor ("Wire Color", color) = (1., 1., 1., 1.)
        _InnerColor ("Inner Color", color) = (0., 0., 0., 1.)
    }
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque" 
        }
 
        Pass
        {
            Cull Back
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma geometry geom
            #include "UnityCG.cginc"
 
            struct v2g 
            {
                float4 pos : SV_POSITION;
            };
 
            struct g2f 
            {
                float4 pos : SV_POSITION;
                float3 bary : TEXCOORD0;
            };
 
            v2g vert(appdata_base v) 
            {
                v2g o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }
 
            [maxvertexcount(3)]
            void geom(triangle v2g IN[3], inout TriangleStream<g2f> triStream) 
            {
                g2f o;
                o.pos = IN[0].pos;
                o.bary = float3(1., 0., 0.);
                triStream.Append(o);
                o.pos = IN[1].pos;
                o.bary = float3(0., 0., 1.);
                triStream.Append(o);
                o.pos = IN[2].pos;
                o.bary = float3(0., 1., 0.);
                triStream.Append(o);
            }
 
            float _WireWidth;
            fixed4 _WireColor;
            fixed4 _InnerColor;
 
            fixed4 frag(g2f i) : SV_Target 
            {
                if (!any(bool3(i.bary.x < _WireWidth, i.bary.y < _WireWidth, i.bary.z < _WireWidth)))
                {
                    if (_InnerColor.a <= 0.)
                        discard;

                    return _InnerColor;
                }
 
                return _WireColor;
            }
 
            ENDCG
        }
    }
}