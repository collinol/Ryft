using UnityEngine;

namespace Game.Enemies
{
    [RequireComponent(typeof(Collider2D))]
    public class EnemyClickTarget : MonoBehaviour
    {
        private EnemyBase enemy;

        void Awake()
        {
            enemy = GetComponent<EnemyBase>() ?? GetComponentInParent<EnemyBase>();
        }

        void OnMouseDown()
        {
            var ctrl = FindObjectOfType<Game.Combat.FightSceneController>();
            if (enemy && ctrl) ctrl.OnEnemyClicked(enemy);
        }
    }
}
