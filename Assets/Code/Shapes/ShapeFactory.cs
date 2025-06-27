using Prefabrikator.Runtime;

namespace Prefabrikator.Shapes
{
    public class ShapeFactory
    {
        public static IShapeComponent CreateShape(ShapeType type)
        {
            switch (type)
            {
                case ShapeType.Line:
                    return new Line();
                case ShapeType.Grid:
                    break;
                case ShapeType.Circle:
                    break;
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
    }
}