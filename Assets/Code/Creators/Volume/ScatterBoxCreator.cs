using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using Prefabrikator.Runtime;

namespace Prefabrikator
{
    public class ScatterBoxCreator : ScatterPlaneCreator
    {
        public override ShapeType Shape => ShapeType.ScatterBox;

        private Shared<Vector3> _size = new Shared<Vector3>(new Vector3(5f, 5f, 5f));
        protected override Shared<Vector3> DefaultSize => new Shared<Vector3>(new Vector3(5f, 5f, 5f));

        public ScatterBoxCreator(GameObject target, CustomShape shape, OpenMode openMode)
            : base(target, shape, openMode)
        {
            //
        }

        protected override Dimension GetDimension()
        {
            return Dimension.Three;
        }

        protected override Vector3 EnforceSizeConstraints(Vector3 newSize)
        {
            return newSize;
        }
    }
}
