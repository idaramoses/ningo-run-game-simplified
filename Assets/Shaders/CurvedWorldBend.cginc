#ifndef NINGO_CURVED_WORLD_BEND_INCLUDED
#define NINGO_CURVED_WORLD_BEND_INCLUDED
uniform float _CurveStrength;
uniform float _CurveStartOffset;
uniform float4 _CurveOrigin;
uniform float _SideBendStrength;
uniform float _SideBendStart;
uniform float _SideBendLimit;

float4 NingoBendWorld(float4 worldPos)
{
    float verticalDistance = max(0.0, worldPos.z - _CurveOrigin.z - _CurveStartOffset);
    worldPos.y -= verticalDistance * verticalDistance * _CurveStrength;
    float horizontalDistance = max(0.0, worldPos.z - _CurveOrigin.z - _SideBendStart);
    float rawOffset = horizontalDistance * horizontalDistance * abs(_SideBendStrength);
    float limit = max(0.0001, _SideBendLimit);
    // Smooth saturation, no kink at onset or maximum displacement.
    worldPos.x += sign(_SideBendStrength) * limit * rawOffset / (limit + rawOffset);
    return worldPos;
}
#endif
