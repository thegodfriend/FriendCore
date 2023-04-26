using Modding;
using UnityEngine;

namespace FriendCore
{
    internal class AlterInfectedEnemyEffects : MonoBehaviour
    {
        private bool noBlood = false;
        private Color bloodColor = new Color(1f, 0.31f, 0f);
        private Color flashColor = new Color(1f, 0.31f, 0f);

        public void Awake()
        {
            InfectedEnemyEffects infectedeffects = GetComponent<InfectedEnemyEffects>();
            if (!infectedeffects)
            {
                Destroy(this);
            }
        }

        public void Start()
        {
            On.InfectedEnemyEffects.RecieveHitEffect += InfectedEnemyEffects_RecieveHitEffect;
        }

        public void SetNoBlood(bool value = true)
        {
            noBlood = value;
        }

        public void SetBloodColor(Color newColor)
        {
            bloodColor = newColor;
            SetNoBlood(false);
        }

        public void SetFlashColor(Color newColor)
        {
            flashColor = newColor;
        }

        public void SetColor(Color newBloodColor, Color newFlashColor)
        {
            SetBloodColor(newBloodColor);
            SetFlashColor(newFlashColor);
        }
        public void SetColor(Color newColor)
        {
            SetColor(newColor, newColor);
        }

        private void InfectedEnemyEffects_RecieveHitEffect(On.InfectedEnemyEffects.orig_RecieveHitEffect orig, InfectedEnemyEffects self, float attackDirection)
        {
            GameObject enemy = self.gameObject;
            if (enemy == this.gameObject)
            {
                if (ReflectionHelper.GetField<InfectedEnemyEffects, bool>(self, "didFireThisFrame"))
                {
                    return;
                }

                SpriteFlash spriteFlash = ReflectionHelper.GetField<InfectedEnemyEffects, SpriteFlash>(self, "spriteFlash");
                AudioEvent impactAudio = ReflectionHelper.GetField<InfectedEnemyEffects, AudioEvent>(self, "impactAudio");
                AudioSource audioSourcePrefab = ReflectionHelper.GetField<InfectedEnemyEffects, AudioSource>(self, "audioSourcePrefab");
                Vector3 effectOrigin = ReflectionHelper.GetField<InfectedEnemyEffects, Vector3>(self, "effectOrigin");

                if (spriteFlash != null)
                {
                    spriteFlash.flash(flashColor, 0.9f, 0.01f, 0.01f, 0.25f);
                }
                impactAudio.SpawnAndPlayOneShot(audioSourcePrefab, transform.position);

                if (!noBlood)
                {
                    switch (DirectionUtils.GetCardinalDirection(attackDirection))
                    {
                        case 0:
                            GlobalPrefabDefaults.Instance.SpawnBlood(transform.position + effectOrigin, 3, 4, 10f, 15f, 120f, 150f, bloodColor);
                            GlobalPrefabDefaults.Instance.SpawnBlood(transform.position + effectOrigin, 8, 15, 10f, 25f, 30f, 60f, bloodColor);
                            break;
                        case 1:
                            GlobalPrefabDefaults.Instance.SpawnBlood(transform.position + effectOrigin, 8, 10, 20f, 30f, 80f, 100f, bloodColor);
                            break;
                        case 2:
                            GlobalPrefabDefaults.Instance.SpawnBlood(transform.position + effectOrigin, 3, 4, 10f, 15f, 30f, 60f, bloodColor);
                            GlobalPrefabDefaults.Instance.SpawnBlood(transform.position + effectOrigin, 8, 10, 15f, 25f, 120f, 150f, bloodColor);
                            break;
                        case 3:
                            GlobalPrefabDefaults.Instance.SpawnBlood(transform.position + effectOrigin, 4, 5, 15f, 25f, 140f, 180f, bloodColor);
                            GlobalPrefabDefaults.Instance.SpawnBlood(transform.position + effectOrigin, 4, 5, 15f, 25f, 360f, 400f, bloodColor);
                            break;
                    }
                }

                ReflectionHelper.SetField<InfectedEnemyEffects, bool>(self, "didFireThisFrame", true);

                return;

            }

            orig(self, attackDirection);
        }

        public void OnDestroy()
        {
            On.InfectedEnemyEffects.RecieveHitEffect -= InfectedEnemyEffects_RecieveHitEffect;
        }

    }
}
