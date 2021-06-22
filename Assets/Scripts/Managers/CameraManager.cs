using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField]
    float _totalShakingTime = 0.3f;
    [SerializeField]
    float _defaultIntensity = 0.3f;
    [SerializeField]
    float _shakeTime = 0.05f;

    Vector3 basePosition;

    private void Start()
    {
        basePosition = transform.position;
    }

    public void CameraShake()
    {
        StartCoroutine(CameraShakeRoutine(_defaultIntensity, _totalShakingTime, true));
    }

    public void CameraShake(float intensity, float duration, bool fadeOut)
    {
        StartCoroutine(CameraShakeRoutine(intensity, duration, fadeOut));
    }

    IEnumerator CameraShakeRoutine(float intensity, float duration, bool fadeOut)
    {
        float timeRemaining = duration;
        Vector3 shakeDirection = GetShakeDirection();

        while (timeRemaining > 0)
        {
            // get actual world space position we want to move the camera towards      
            float magnitude = fadeOut ? Mathf.Lerp(0, intensity, timeRemaining / duration) : intensity;
            Vector3 shakeTarget = basePosition + shakeDirection * magnitude;

            float currentTime = -_shakeTime;
            while (currentTime < _shakeTime)
            {
                // progress will go 1 -> 0 -> 1
                float progress = Mathf.Abs(currentTime) / _shakeTime;
                // progress == 1, we are at base postiion. at progress == 0, we are at shakeTarget
                Vector3 interpolatePosition = Vector3.Lerp(shakeTarget, basePosition, progress);
                transform.position = interpolatePosition;

                yield return null;
                timeRemaining -= Time.deltaTime;
                currentTime += Time.deltaTime;
            }

            shakeDirection = GetShakeDirection(shakeDirection);
        }
        transform.position = basePosition;
    }

    Vector3 GetShakeDirection() // gets a random direction to start
    {
        return Random.insideUnitCircle.normalized;
    }

    // get a direction somewhat in the opposite direction
    Vector3 GetShakeDirection(Vector3 lastDirection) 
    {
        float randomAngle = Random.Range(120f, 240f);
        return Quaternion.Euler(0, 0, randomAngle) * lastDirection;
    }

    public static Bounds GetCameraBounds()
    {
        Camera myself = Camera.main;
        float height = 2 * myself.orthographicSize;
        float width = height * myself.aspect;
        Vector3 center = Vector3.ProjectOnPlane(myself.transform.position, Vector3.forward);

        Bounds cameraBounds = new Bounds(myself.transform.position, new Vector3(width, height, 50));
        return cameraBounds;
    }

    /// <summary>
    /// Gets a a random point at the right of the screen
    /// </summary>
    public static Vector3 RandomPositionAtRight()
    {
        Bounds cameraBounds = GetCameraBounds();
        float xPosition = cameraBounds.center.x + cameraBounds.extents.x + 1.5f;

        float yMax = cameraBounds.center.y + cameraBounds.extents.y - 1.2f;
        float yPosition = Random.Range(-yMax, yMax);

        return new Vector3(xPosition, yPosition, 0);
    }

    /// <summary>
    /// Returns true if object is within allowed distance of the camera bounds
    /// </summary>
    /// <param name="pointToCheck"></param>
    /// <param name="allowedDistanceOutside"></param>
    /// <returns></returns>
    public static bool IsInsideCameraBounds(Vector3 pointToCheck, float allowedDistanceOutside)
    {
        Bounds bounds = GetCameraBounds();
        bounds.Expand(allowedDistanceOutside);
        return bounds.Contains(pointToCheck);
    }
}
