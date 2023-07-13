
namespace Prefabrikator
{
    public enum ShapeType
    {
        Line,
        Grid,
        Circle,
        Arc,
        Sphere,
#if SPLINE_CREATOR
        Spline,
#endif
        ScatterSphere,
        ScatterBox,
        ScatterPlane,
        Ellipse,
        // polygon
        // Helix
    }
}
