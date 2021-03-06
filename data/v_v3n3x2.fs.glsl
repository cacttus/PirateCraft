#include "v_glsl_version.glsl"
#include "v_forward_header.glsl"

#define PHONG 1
//#define BLINN_PHONG 1
//#define GGX 1
//struct GpuLight {
//    vec3 _pos;
//    float _radius;
//    vec3 _color;
//    float _power; // This would be the falloff curve  ^x
//};
////We may just use HDRI in the future, but for now . 
//struct GpuDirLight {
//    vec3 _dir;
//    float _pad;
//    vec3 _color;
//    float _power;
//};
//
uniform sampler2D _ufTexture2D_Albedo;
uniform sampler2D _ufTexture2D_Normal;

in vec3 _vsNormal;
in vec2 _vsTcoords;
in vec3 _vsVertex;

uniform vec3 _ufCamera_Position;
//uniform int _ufLightModel_Index;
//uniform float _ufLightModel_GGX_X;
//uniform float _ufLightModel_GGX_Y;
//uniform float _ufNormalMap_Blend;
uniform uint _vsPick_Color;


void main(void)
{
int _ufLightModel_Index = 1;
float _ufLightModel_GGX_X=1;
float _ufLightModel_GGX_Y=1;
float _ufNormalMap_Blend=0.5f;

    GpuDirLight sun;
    sun._dir = normalize(vec3(-1,-1,-1));
    sun._color = vec3(244.0f/255.0f, 233.0f/255.0f, 155.0f/255.0f);//F4E99B
    sun._power = 1f;
    
#define NUM_LIGHTS 3
    GpuPointLight lights[NUM_LIGHTS];
    lights[0]._pos = vec3(0,20,0);
    lights[0]._radius = 30.0f;
    lights[0]._color = vec3(.87652f,.89013f,.63232f);
    lights[0]._power = .94; // power within radius of light. 1 = is constant, 0.5 linear, 0 would be no light. <0.5 power falloff >0.5 is slow faloff. y=x^(1/p), p=[0,1], p!=0
    
    lights[1]._pos = _ufCamera_Position;
    lights[1]._radius = 30.0f;
    lights[1]._color = vec3(.98934f,.99149f,.97255f);
    lights[1]._power = .8;// power within radius of light. 1 = is constant, 0.5 linear, 0 would be no light. <0.5 power falloff >0.5 is slow faloff. y=x^(1/p), p=[0,1] ,p!=0

    vec4 tx_albedo = texture(_ufTexture2D_Albedo, vec2(_vsTcoords));

    if(tx_albedo.a < 0.001){
    discard;
    }

    vec3 tx_normal = normalize(texture(_ufTexture2D_Normal, vec2(_vsTcoords)).xyz * 2.0f - 1.0f);
     
    //mat3 mLightMatrix = getLightMatrix(_vsNormal, _vsVertex);
    //vec3 bump_normal = normalize(mLightMatrix * tx_normal);
        
    float nmap_blend = _ufNormalMap_Blend; //This should constant to determine how much we blend normal map vs surf normal, from [0,1] 1 is full bump, 0 is no bump..
    vec3 bump_normal = normalize((1-nmap_blend)*_vsNormal + (nmap_blend)*tx_normal);
    
    vec3 eye = normalize(_ufCamera_Position - _vsVertex);
    float rho = 0.17f; //Albedo [0,1], 1 = 100% white, 0 = black body.
    float E0 = 1f; // Strength [0,1]
    float fSpecIntensity =.3; //[0,1] // I guess tecnhically speaking these two should be controlled by 'roughness'
    float fSpecHardness =20; //[1,inf] 0=n

    vec3 finalDiffuseColor = vec3(0,0,0);
    vec3 finalSpecColor = vec3(0,0,0);
    vec3 vReflect;
    float distribution;

    for(int i=0; i<NUM_LIGHTS; i++) {
        vec3 lightpos_normal = normalize(lights[i]._pos - _vsVertex);
        float ldotn = dot(lightpos_normal, bump_normal);
        float fFragToLightDistance = distance(lights[i]._pos, _vsVertex);

        lights[i]._power = clamp(lights[i]._power, 0.000001f, 0.999999f);

        float fQuadraticAttenuation = 1- pow(clamp(fFragToLightDistance/lights[i]._radius, 0, 1),(lights[i]._power)/(1-lights[i]._power)); //works well: x^(p/(1-p)) where x is pos/radius

        // Lambert
        float Lr_surf = rho * ldotn *  E0;
        finalDiffuseColor += lights[i]._color * Lr_surf * fQuadraticAttenuation; 
    

        if(_ufLightModel_Index==0) {
            //Phong
            vReflect = reflect(-lightpos_normal, bump_normal);
        }
        if(_ufLightModel_Index==1) {
            //Blinn-Phong
            vReflect = (lightpos_normal + bump_normal)*0.5f;
        }
        distribution = clamp( pow(clamp(dot(vReflect, eye), 0,1), fSpecHardness), 0,1 );
        finalSpecColor += lights[i]._color * fSpecIntensity * fQuadraticAttenuation * distribution;// * shadowMul; 
    } 
        
    //Sun
    vec3 sun_normal_surf = sun._dir * -1;
    if(_ufLightModel_Index==0) {
        //Phong
        vReflect = reflect(-sun_normal_surf, bump_normal);
    }
    if(_ufLightModel_Index==1) {
        //Blinn-Phong
        vReflect = (sun_normal_surf + bump_normal)*0.5f;
    }
    finalDiffuseColor += sun._color * dot(sun_normal_surf, bump_normal) *  sun._power; 
    distribution = clamp( pow(clamp(dot(vReflect, eye), 0,1), fSpecHardness), 0,1 );
    finalSpecColor += sun._color * fSpecIntensity * distribution;// * shadowMul; 


    if(_ufLightModel_Index==2) {
        //Flat
        finalDiffuseColor = vec3(1,1,1);
        finalSpecColor = vec3(0,0,0);
    }

        setColorOutput(new vec4(finalDiffuseColor.rgb *  tx_albedo.rgb, 1));
       setPickOutput(0);
}