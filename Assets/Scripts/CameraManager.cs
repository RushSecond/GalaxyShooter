using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField]
    float _totalShakingTime = 0.3f;
    [SerializeField]
    float _shakeIntensity = 0.3f;
    [SerializeField]
    float _shakeTime = 0.05f;

    Vector3 basePosition;

    private void Start()
    {
        basePosition = transform.position;
    }

    public void CameraShake()
    {
        StartCoroutine(CameraShakeRoutine());
    }

    IEnumerator CameraShakeRoutine()
    {
        float timeRemaining = _totalShakingTime;
        Vector3 shakeDirection = GetShakeDirection();
        WaitForEndOfFrame wait = new WaitForEndOfFrame();

        while (timeRemaining > 0)
        {
            // get actual world space position we want to move the camera towards
            Vector3 shakeTarget = basePosition + shakeDirection * _shakeIntensity;
            float currentTime = -_shakeTime;
            while (currentTime < _shakeTime)
            {
                // progress will go 1 -> 0 -> 1
                float progress = Mathf.Abs(currentTime) / _shakeTime;
                // progress == 1, we are at base postiion. at progress == 0, we are at shakeTarget
                Vector3 interpolatePosition = Vector3.Lerp(shakeTarget, basePosition, progress);
                transform.position = interpolatePosition;

                yield return wait;
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
}
