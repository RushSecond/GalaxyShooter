using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EllipticalBehavior : BasicEnemyBehavior
{
    private bool inEllipse = false; // have we reached the ellipse yet?
    private float angleAroundEllipse = 0f;
    [SerializeField]
    private Vector3 ellipseVertex; // Vertex that enemies will move to
    private Vector3 ellipseCenter;

    [SerializeField]
    private float semiMajorAxis = 4; // height of ellipse from center
    [SerializeField]
    private float semiMinorAxis = 2; // width of ellipse from center

    protected override void Start()
    {
        BehaviorSetup();
        StartCoroutine(FireLaserRoutine());
        ellipseCenter = ellipseVertex - new Vector3(semiMinorAxis, 0, 0);
        transform.position = new Vector3(ellipseVertex.x, -CameraManager.GetCameraBounds().extents.y - 1f, 0);
    }

    void SetPositionByAngle()
    {
        float X = semiMinorAxis * Mathf.Cos(angleAroundEllipse);
        float Y = semiMajorAxis * Mathf.Sin(angleAroundEllipse);

        transform.position = ellipseCenter + new Vector3(X, Y, 0);
    }

    public override void Act()
    {
        if (!inEllipse)
        {
            transform.position += Vector3.up * _mySpeed * Time.deltaTime;
            Vector3 vectorFromCenter = transform.position - ellipseCenter;
            if (vectorFromCenter.y > 0)
            {
                inEllipse = true;
                angleAroundEllipse = Mathf.Deg2Rad * Vector3.Angle(Vector3.right, vectorFromCenter);
                SetPositionByAngle();
            }
            return;
        }

        angleAroundEllipse += _mySpeed * Time.deltaTime / semiMajorAxis;
        SetPositionByAngle();
    }
}
