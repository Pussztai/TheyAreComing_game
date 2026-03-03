using Raylib_cs;
using System.Numerics;

namespace TheyAreComing {

    public enum WeaponType {
        Pistol,
        SMG,
        Shotgun,
        Rifle,
        Sniper
    }

    public class WeaponDefinition {
        public WeaponType Type;
        public string Name;
        public string Description;
        public int    Cost;
        public float  ShootCooldown;   
        public float  BulletDamage;
        public int    MaxAmmo;
        public float  ReloadTime;
        public int    PelletCount;    
        public float  Spread;         
        public string PixelIcon;       

        public WeaponDefinition(WeaponType type, string name, string desc, int cost,
                                float shootCooldown, float bulletDamage, int maxAmmo,
                                float reloadTime, int pelletCount = 1, float spread = 0f,
                                string pixelIcon = "") {
            Type = type; Name = name; Description = desc; Cost = cost;
            ShootCooldown = shootCooldown; BulletDamage = bulletDamage;
            MaxAmmo = maxAmmo; ReloadTime = reloadTime;
            PelletCount = pelletCount; Spread = spread;
            PixelIcon = pixelIcon;
        }
    }

    public static class WeaponCatalog {
        public static readonly List<WeaponDefinition> All = new() {
            new WeaponDefinition(
                WeaponType.Pistol, "Pistol", "Megbízható, stabil alap fegyver.",
                cost: 0, shootCooldown: 0.35f, bulletDamage: 20f, maxAmmo: 12,
                reloadTime: 1.5f, pixelIcon: "PISTOL"
            ),
            new WeaponDefinition(
                WeaponType.SMG, "SMG", "Gyors tűzgyorsaság, kis sebzés. Nagy tár.",
                cost: 150, shootCooldown: 0.10f, bulletDamage: 10f, maxAmmo: 30,
                reloadTime: 2.0f, pixelIcon: "SMG"
            ),
            new WeaponDefinition(
                WeaponType.Shotgun, "Shotgun", "Közelről halálos, 5 pellet egyszerre.",
                cost: 200, shootCooldown: 0.75f, bulletDamage: 20f, maxAmmo: 8,
                reloadTime: 2.5f, pelletCount: 5, spread: 0.25f, pixelIcon: "SHOTGUN"
            ),
            new WeaponDefinition(
                WeaponType.Rifle, "M4 Rifle", "Nagy sebzés, közepes tűzgyorsaság.",
                cost: 350, shootCooldown: 0.18f, bulletDamage: 40f, maxAmmo: 30,
                reloadTime: 2.2f, pixelIcon: "M4"
            ),
            new WeaponDefinition(
                WeaponType.Sniper, "Sniper", "Lassú, de hatalmas sebzés. Fejlövés!",
                cost: 500, shootCooldown: 1.20f, bulletDamage: 130f, maxAmmo: 5,
                reloadTime: 3.0f, pixelIcon: "SNIPER"
            ),
        };

        public static WeaponDefinition Get(WeaponType t) => All.First(w => w.Type == t);
    }
}
