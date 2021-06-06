using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : MonoBehaviour, IProjectile
{
    Transform _target;
    EnemyLives _enemyScript;

    [SerializeField]
    float _velocity;
    [SerializeField]
    float _maxVelocity;
    [SerializeField]
    float _turnDegreesPerSecond;
    [SerializeField]
    int _damage = 3;

    public int GetDamage()
    {
        return _damage;
    }

    void Update()
    {
        if (!CameraManager.IsInsideCameraBounds(transform.position, 2f))
            Destroy(gameObject);

        // if target is lost, find a new one
        if (!_target || _enemyScript == null || _enemyScript.IsDead)
            _target = SeekNewTarget();

        // if still no target, just keep going in our current direction
        if (!_target)
        {
            transform.Translate(Vector3.up * _velocity * Time.deltaTime);
            return;
        }

        // if we have a target, turn toward it
        Vector3 worldDirection = Vector3.Normalize(_target.position - transform.position);
        Vector3 localDirection = transform.InverseTransformDirection(worldDirection);
 
        Vector3 Z_axis = Vector3.forward; // We want to rotate around z-axis

        // how many degrees will it take to rotate toward the enemy?
        float targetAngleMeasure = Vector3.SignedAngle(Vector3.up, localDirection, Z_axis);

        // try to rotate there, but limit it by our turn degrees per second
        float angleToRotate = Mathf.Sign(targetAngleMeasure) * 
            Mathf.Min(Mathf.Abs(targetAngleMeasure), _turnDegreesPerSecond * Time.deltaTime);
        transform.Rotate(Z_axis, angleToRotate);

        // Then move toward it
        transform.Translate(Vector3.up * _velocity * Time.deltaTime);
    }

    Transform SeekNewTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        if (enemies.Length == 0) return null;

        float closestDistance = 9999f;
        Transform closestEnemy = null;
        foreach (GameObject enemy in enemies)
        {
            Enemy enemyScript = enemy.GetComponent<Enemy>();
            if (enemyScript == null) continue; // ignore non-scripted hitboxes
            if (enemyScript.enemyLives.IsDead) continue; //ignore dead enemies!

            // if this new enemy is closer, pick it instead
            float distance = Vector3.Distance(enemy.transform.position, transform.position);
            if (closestEnemy == null || distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy.transform;
            }
        }

        if (closestEnemy)
            _enemyScript = closestEnemy.GetComponent<EnemyLives>();
        return closestEnemy;
    }
}
