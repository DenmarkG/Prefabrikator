using Prefabrikator.Shapes;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator.Runtime
{
    [ExecuteInEditMode]
    public class LineComponent : ShapeComponent
    {
        public static readonly Vector3 DefaultOffset = new Vector3(2f, 0f, 0f);

        public sealed override IShape Shape => _line;
        [SerializeField] private Line _line = new Line(DefaultOffset);

        //public List<Modifier> Modidfiers => new List<Modifier>();

        public void SetOffset(Vector3 offset)
        {
            _line.SetOffset(offset);
        }

        public void SetStart(Vector3 start)
        {
            _line.SetStart(start);
        }        

#if UNITY_EDITOR

        [ContextMenu("Reset Start")]
        private void ResetStart()
        {
            _line.SetStart(this.transform.position);
        }

#endif // UNITY_EDITOR
    }
}