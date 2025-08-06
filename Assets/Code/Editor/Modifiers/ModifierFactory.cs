using Prefabrikator.Runtime;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator.Editor
{
    public static class ModifierFactory
    {
        public delegate Modifier CreatorFunction(IRuntimeShape creator);
        private static Dictionary<string, CreatorFunction> _creators = new Dictionary<string, CreatorFunction>()
        {
            //{ ModifierType.ScaleRandom, (array) => { return new RandomScaleModifier(array); } },
            //{ ModifierType.ScaleUniform, (array) => { return new UniformScaleModifier(array); } },
            //{ ModifierType.RotationRandom, (array) => { return new RandomRotation(array); } },
            //{ ModifierType.RotationUniform, (array) => { return new UniformRotation(array); } },
            //{ ModifierType.FollowCurve, (array) => { return new FollowCurveModifier(array); } },
            //{ ModifierType.RadialNoise, (array) => { return new RadialNoise(array); } },
            //{ ModifierType.IncrementalRotation, (array) => { return new IncrementalRotationModifier(array); } },
            //{ ModifierType.IncrementalScale, (array) => { return new IncrementalScaleModifier(array); } },
            //{ ModifierType.PositionNoise, (array) => { return new PositionNoiseModifier(array); } },
            //{ ModifierType.DropToFloor, (array) => { return new DropModifier(array); } },
        };

        public static Modifier CreateModifier(string modifierName, IRuntimeShape array)
        {
            if (_creators.TryGetValue(modifierName, out CreatorFunction func))
            {
                return func(array);
            }

            return null;
        }
    }
}