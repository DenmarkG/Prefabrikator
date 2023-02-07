
namespace Prefabrikator
{
    public enum ShapeType
    {
        Line,
        Grid,
        Circle,
        Arc,
        Sphere,

#if PATH
        Path,
#endif
        ScatterSphere,
        ScatterBox,
        ScatterPlane,
        Ellipse,
        // Box
        // polygon
        // bricklay
        // random inside polygon
        // random inside circle
    }
}
