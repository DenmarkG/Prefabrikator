using Prefabrikator.Runtime;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator.Shapes
{
    public class ShapeFactory
    {
        public delegate ShapeComponent ComponentCreator(GameObject obj);
        private static readonly Dictionary<ShapeType, ComponentCreator> kCreatorByShapeType = new()
        {
            { ShapeType.Line, (obj) => obj.AddComponent<LineComponent>() },
            { ShapeType.Circle, (obj) => obj.AddComponent<CircleComponent>() },
        };

        public static Runtime.IRuntimeShape CreateShape(ShapeType type)
        {
            switch (type)
            {
                case ShapeType.Line:
                    return new Line();
                case ShapeType.Grid:
                    break;
                case ShapeType.Circle:
                    return new Circle();
                case ShapeType.Arc:
                    break;
                case ShapeType.Sphere:
                    break;
                case ShapeType.ScatterSphere:
                    break;
                case ShapeType.ScatterBox:
                    break;
                case ShapeType.ScatterPlane:
                    break;
                case ShapeType.Ellipse:
                    break;
                default:
                    break;
            }

            return default;
        }

        public static ShapeComponent AddShapeComponent(GameObject obj, ShapeType type)
        {
            return kCreatorByShapeType[type].Invoke(obj);
        }

    }
}