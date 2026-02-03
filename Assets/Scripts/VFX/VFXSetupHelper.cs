using UnityEngine;

namespace Game.VFX
{
    /// <summary>
    /// Helper component to visualize and configure VFX settings in the Inspector
    /// Attach this to any GameObject in your scene to ensure VFX is set up
    /// </summary>
    [ExecuteInEditMode]
    public class VFXSetupHelper : MonoBehaviour
    {
        [Header("VFX System Status")]
        [SerializeField, Tooltip("Is CardVFXManager initialized?")]
        private bool isVFXManagerActive = false;

        [Header("Test VFX (Runtime Only)")]
        [SerializeField] private bool testDamage = false;
        [SerializeField] private bool testHeal = false;
        [SerializeField] private bool testBuff = false;
        [SerializeField] private bool testArea = false;

        [Header("Configuration")]
        [SerializeField, Range(0.1f, 2f)]
        private float effectDuration = 0.5f;

        [SerializeField, Range(0f, 0.5f)]
        private float cameraShakeMagnitude = 0.1f;

        void Update()
        {
            // Update status
            isVFXManagerActive = CardVFXManager.Instance != null;

            // Test buttons (runtime only)
            if (!Application.isPlaying) return;

            if (testDamage)
            {
                testDamage = false;
                TestDamageEffect();
            }

            if (testHeal)
            {
                testHeal = false;
                TestHealEffect();
            }

            if (testBuff)
            {
                testBuff = false;
                TestBuffEffect();
            }

            if (testArea)
            {
                testArea = false;
                TestAreaEffect();
            }
        }

        private void TestDamageEffect()
        {
            var vfx = CardVFXManager.Instance;
            if (vfx != null)
            {
                vfx.PlayDamageEffect(transform, VFXColors.Physical, 42);
                Debug.Log("Played test damage effect!");
            }
            else
            {
                Debug.LogWarning("CardVFXManager not found! Make sure FightSceneController is in the scene.");
            }
        }

        private void TestHealEffect()
        {
            var vfx = CardVFXManager.Instance;
            if (vfx != null)
            {
                vfx.PlayHealEffect(transform, 25);
                Debug.Log("Played test heal effect!");
            }
        }

        private void TestBuffEffect()
        {
            var vfx = CardVFXManager.Instance;
            if (vfx != null)
            {
                vfx.PlayBuffEffect(transform, VFXColors.Magic);
                Debug.Log("Played test buff effect!");
            }
        }

        private void TestAreaEffect()
        {
            var vfx = CardVFXManager.Instance;
            if (vfx != null)
            {
                vfx.PlayAreaEffect(transform.position, 3f, VFXColors.Engineering);
                Debug.Log("Played test area effect!");
            }
        }

        void OnDrawGizmos()
        {
            // Draw gizmo to show where VFX would appear
            Gizmos.color = isVFXManagerActive ? Color.green : Color.red;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 2f, 0.3f);

            // Draw effect range
            Gizmos.color = new Color(0, 1, 1, 0.3f);
            Gizmos.DrawWireSphere(transform.position, 3f);
        }
    }
}
