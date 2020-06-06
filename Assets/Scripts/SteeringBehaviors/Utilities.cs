using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace SteeringBehaviors
{
    internal static class Utilities
    {
        internal static float GetDeltaTime()
        {
#if STEERING_BEHAVIORS_SIMULATION
            return UnityEngine.Time.fixedTime;
#else 
            return UnityEngine.Time.deltaTime;
#endif
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static float2 TruncateLength(float2 v, float maxLength)
        {
            float maxLengthSquared = maxLength * maxLength;
            float vecLengthSquared = math.lengthsq(v);
            return (vecLengthSquared <= maxLengthSquared) ? v : v * maxLength / math.sqrt(vecLengthSquared);
        }
    }
}
