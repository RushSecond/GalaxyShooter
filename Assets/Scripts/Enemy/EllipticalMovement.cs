using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EllipticalMovement : EnemyMovement
{
    private bool inEllipse = false;
    private float angleAroundEllipse = 0f;
    private Vector3 ellipseVertex;
    private Vector3 ellipseCenter;
    private float majorAxis=8, minorAxis=4;

    public EllipticalMovement(Enemy enemyScript) : base(enemyScript)
    {
        ellipseVertex = new Vector3(7, 0, 0);
        ellipseCenter = ellipseVertex - new Vector3(minorAxis/2, 0, 0);

        _myTransform.position = new Vector3(7, -SpawnManager._screenBoundsY - 3f, 0);
    }

    public override void Move(float speed)
    {
        if(!inEllipse)
        {
            _myTransform.position += Vector3.up * speed * Time.deltaTime;
            if (_myTransform.position.y > 0)
            {
                inEllipse = true;
                Vector3 vectorFromCenter = _myTransform.position - ellipseCenter;
                angleAroundEllipse = Mathf.Deg2Rad * Vector3.Angle(Vector3.right, vectorFromCenter);
                SetPositionByAngle();             
            }
            return;
        }

        angleAroundEllipse += speed * 2 * Time.deltaTime / majorAxis;
        SetPositionByAngle();
    }

    void SetPositionByAngle()
    {
        float X = minorAxis/2 * Mathf.Cos(angleAroundEllipse);
        float Y = majorAxis/2 * Mathf.Sin(angleAroundEllipse);

        _myTransform.position = ellipseCenter + new Vector3(X, Y, 0);
    }
}
