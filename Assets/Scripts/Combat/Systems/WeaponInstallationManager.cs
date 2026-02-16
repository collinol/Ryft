using UnityEngine;

namespace Game.Combat
{
    public enum WeaponType
    {
        None,
        Minigun,       // Attack cards hit 1 additional time for 50% damage
        Railgun,       // Single-target attacks also hit all other enemies for 50%
        Flamethrower,  // Attack cards apply 2 Burning
        CryoCannon,    // Attack cards apply 2 Frozen
    }

    /// <summary>
    /// Manages weapon installations for the Cyborg Engineer subclass.
    /// Only one weapon can be active at a time. Persists until combat ends or replaced.
    /// </summary>
    public class WeaponInstallationManager : MonoBehaviour
    {
        public static WeaponInstallationManager Instance { get; private set; }

        public WeaponType CurrentWeapon { get; private set; } = WeaponType.None;
        public bool HasWeapon => CurrentWeapon != WeaponType.None;

        /// <summary>Flat bonus damage from devices (Overcharged Capacitor).</summary>
        public int BonusFlatDamage
        {
            get
            {
                var dm = DeviceManager.Instance;
                return dm != null ? dm.GetTotalBonusDamage() : 0;
            }
        }

        void Awake() => Instance = this;

        public void InstallWeapon(WeaponType weapon)
        {
            if (CurrentWeapon != WeaponType.None)
                Debug.Log($"[Weapon] Replacing {CurrentWeapon} with {weapon}");

            CurrentWeapon = weapon;
            Debug.Log($"[Weapon] Installed {weapon}");
        }

        public void RemoveWeapon()
        {
            Debug.Log($"[Weapon] Removed {CurrentWeapon}");
            CurrentWeapon = WeaponType.None;
        }
    }
}
