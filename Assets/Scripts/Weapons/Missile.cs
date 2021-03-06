using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : MonoBehaviour, IProjectile
{
    static HashSet<GameObject> _enemiesTargetedByMissiles = new HashSet<GameObject>();

    GameObject _target;
    LivesComponent _targetScript;

    [SerializeField]
    float _velocity;
    [SerializeField]
    float _turnDegreesPerSecond;
    [SerializeField]
    int _damage = 3;
    [SerializeField]
    GameObject _myExplosion;

    public int GetDamage()
    {
        return _damage;
    }

    void Update()
    {
        if (!CameraManager.IsInsideCameraBounds(transform.position, 2f))
        {
            DestroySelf(false);
            return;
        }

        MoveTowardTarget();
    }

    private void MoveTowardTarget()
    {
        // if target is lost, find a new one
        if (!_target || _targetScript.IsDead || _targetScript == null)
        {
            if (_target) _enemiesTargetedByMissiles.Remove(_target);
            _target = SeekNewTarget();
        }            

        // if still no target, just keep going in our current direction
        if (!_target)
        {
            transform.Translate(Vector3.up * _velocity * Time.deltaTime);
            return;
        }

        // if we have a target, turn toward it
        Vector3 worldDirection = Vector3.Normalize(_target.transform.position - transform.position);
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

    GameObject SeekNewTarget()
    {
        if (tag == "EnemyProjectile") // an enemy missile targets the player
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            _targetScript = playerObject.GetComponent<LivesComponent>();
            return playerObject;
        }

        // player missles target enemies
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        // remove enemies that are already targeted by missiles
        HashSet<GameObject> notTargetEnemies = new HashSet<GameObject>(enemies);
        notTargetEnemies.ExceptWith(_enemiesTargetedByMissiles);

        // allow re-targeting the same enemies if all enemies have been targeted
        IEnumerable enemiesToSearch = notTargetEnemies.Count != 0 ? (IEnumerable)notTargetEnemies: enemies;

        if (enemies.Length == 0) return null;

        float closestDistance = 9999f;
        GameObject closestEnemy = null;
        foreach (GameObject enemy in enemiesToSearch)
        {
            Enemy enemyScript = enemy.GetComponent<Enemy>();
            if (enemyScript == null) continue; // ignore non-scripted hitboxes
            if (enemyScript.enemyLives.IsDead) continue; //ignore dead enemies!

            // if this new enemy is closer, pick it instead
            float distance = Vector3.Distance(enemy.transform.position, transform.position);
            if (closestEnemy == null || distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy;
            }
        }

        if (closestEnemy)
        {
            _targetScript = closestEnemy.GetComponent<LivesComponent>();
            _enemiesTargetedByMissiles.Add(closestEnemy);
        }
        return closestEnemy;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (tag != "EnemyProjectile") return;
        // handle collisions with Player and their projectiles
        if (other.gameObject.tag == "Player")
        {
            PlayerLives playerScript = other.GetComponent<PlayerLives>();
            if (!playerScript) return;
            playerScript.OnTakeDamage();
            DestroySelf(true);
            return;
        }
        if (other.gameObject.tag == "Projectile")
        {
            Destroy(other.gameObject);
            DestroySelf(true);
        }
    }

    void DestroySelf(bool explode)
    {
        _enemiesTargetedByMissiles.Remove(_target);
        if (explode)
            Instantiate(_myExplosion, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    public void OnCollide()
    {
        DestroySelf(true);
    }
}
