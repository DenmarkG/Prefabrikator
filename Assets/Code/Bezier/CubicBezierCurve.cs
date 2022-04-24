using UnityEngine;

[System.Serializable]
public class CubicBezierCurve
{
    public BezierPoint Start = new BezierPoint(new Vector3());
    public BezierPoint End = new BezierPoint(new Vector3(5, 0));

    public Vector3 GetPointOnCurve(float t)
    {
        t = Mathf.Clamp01(t);

        // B(t) = ((1 - t)^3 * P0) + (3(1 - t)^2 * tP1) + (3(1-t)^2 * P2) + (t^3 * P3)
        float oneMinusT = (1 - t);
        Vector3 P0 = Start.Position;
        Vector3 P1 = Start.Tangent;
        Vector3 P2 = End.Tangent;
        Vector3 P3 = End.Position;

        Vector3 firstTerm = (oneMinusT * oneMinusT * oneMinusT) * P0;
        Vector3 secondTerm = 3 * t * (oneMinusT * oneMinusT) * P1;
        Vector3 thirdTerm = 3 * t * t * (oneMinusT) * P2;
        Vector3 fourthTerm = t * t * t * P3;

        Vector3 point = firstTerm + secondTerm + thirdTerm + fourthTerm;

        return point;
    }

    public Vector3 GetTangentToCurve(float t)
    {
        // The tangent to the curve is the derivative of the curve at t:
        // B'(t) = 3(1 - t)^2(P1 - P0) + 6(1 - t)t(P2 - P1) + 3t^2(P3 - P2)

        Vector3 P0 = Start.Position;
        Vector3 P1 = Start.Tangent;
        Vector3 P2 = End.Tangent;
        Vector3 P3 = End.Position;

        float oneMinusT = (1 - t);
        Vector3 firstTerm = 3 * (oneMinusT * oneMinusT) * (P1 - P0);
        Vector3 secondTerm = 6 * (oneMinusT) * t * (P2 - P1);
        Vector3 thirdTerm = 3 * (t * t) * (P3 - P2);

        Vector3 tangent = firstTerm + secondTerm + thirdTerm;

        return tangent;
    }
}
