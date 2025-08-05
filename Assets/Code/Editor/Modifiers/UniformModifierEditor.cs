using Prefabrikator.Runtime;
using UnityEngine;

namespace Prefabrikator.Editor
{
    public class UniformModifierEditor : ModifierEditor
    {
        protected ToggleProperty _constrainProperty = null;
        protected FloatProperty _constrainedProperty = null;
        protected Vector3Property _targetProperty = null;

        protected sealed override void OnInspectorUpdate(Modifier modifier, IRuntimeShape target, Transform[] proxies)
        {
            if (modifier is UniformModifier uniform)
            {
                uniform.ShouldConstrain.Set(_constrainProperty.Update());

                if (uniform.ShouldConstrain)
                {
                    float scale = _constrainedProperty.Update();
                    uniform.UniformValue.Set(new Vector3(scale, scale, scale));
                }
                else
                {
                    uniform.UniformValue.Set(_targetProperty.Update());
                }
            }
        }
    }
}