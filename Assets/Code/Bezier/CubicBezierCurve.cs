using UnityEngine;

namespace Prefabrikator
{
    [System.Serializable]
    public static class CubicBezierCurve // #DG: Rename this
    {
        public static Vector3 GetPointOnCurve(ControlPoint start, ControlPoint end, float t)
        {
            t = Mathf.Clamp01(t);

            // B(t) = ((1 - t)^3 * P0) + (3(1 - t)^2 * tP1) + (3(1-t)^2 * P2) + (t^3 * P3)
            float oneMinusT = (1 - t);
            Vector3 P0 = start.Position;
            Vector3 P1 = start.Tangent;
            Vector3 P2 = end.Tangent;
            Vector3 P3 = end.Position;

            Vector3 firstTerm = (oneMinusT * oneMinusT * oneMinusT) * P0;
            Vector3 secondTerm = 3 * t * (oneMinusT * oneMinusT) * P1;
            Vector3 thirdTerm = 3 * t * t * (oneMinusT) * P2;
            Vector3 fourthTerm = t * t * t * P3;

            Vector3 point = firstTerm + secondTerm + thirdTerm + fourthTerm;

            return point;
        }

        public static Vector3 GetPointOnCurve(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            t = Mathf.Clamp01(t);

            // B(t) = ((1 - t)^3 * P0) + (3(1 - t)^2 * tP1) + (3(1-t)^2 * P2) + (t^3 * P3)
            float oneMinusT = (1 - t);

            // #DG: consolodate code
            Vector3 firstTerm = (oneMinusT * oneMinusT * oneMinusT) * p0;
            Vector3 secondTerm = 3 * t * (oneMinusT * oneMinusT) * p1;
            Vector3 thirdTerm = 3 * t * t * (oneMinusT) * p2;
            Vector3 fourthTerm = t * t * t * p3;

            Vector3 point = firstTerm + secondTerm + thirdTerm + fourthTerm;

            return point;
        }

        public static Vector3 GetTangentToCurve(ControlPoint start, ControlPoint end, float t)
        {
            // The tangent to the curve is the derivative of the curve at t:
            // B'(t) = 3(1 - t)^2(P1 - P0) + 6(1 - t)t(P2 - P1) + 3t^2(P3 - P2)

            Vector3 P0 = start.Position;
            Vector3 P1 = start.Tangent;
            Vector3 P2 = end.Tangent;
            Vector3 P3 = end.Position;

            float oneMinusT = (1 - t);
            Vector3 firstTerm = 3 * (oneMinusT * oneMinusT) * (P1 - P0);
            Vector3 secondTerm = 6 * (oneMinusT) * t * (P2 - P1);
            Vector3 thirdTerm = 3 * (t * t) * (P3 - P2);

            Vector3 tangent = firstTerm + secondTerm + thirdTerm;

            return tangent;
        }

        public static Vector3 GetTangentToCurve(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            // The tangent to the curve is the derivative of the curve at t:
            // B'(t) = 3(1 - t)^2(P1 - P0) + 6(1 - t)t(P2 - P1) + 3t^2(P3 - P2)

            float oneMinusT = (1 - t);
            Vector3 firstTerm = 3 * (oneMinusT * oneMinusT) * (p1 - p0);
            Vector3 secondTerm = 6 * (oneMinusT) * t * (p2 - p1);
            Vector3 thirdTerm = 3 * (t * t) * (p3 - p2);

            Vector3 tangent = firstTerm + secondTerm + thirdTerm;

            return tangent;
        }
    }
}