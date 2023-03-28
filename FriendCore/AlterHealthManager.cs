using Modding;
using System;
using UnityEngine;

namespace JunkCore
{
    public partial class AlterHealthManager : MonoBehaviour
    {

        private HealthManager _hm;
        private int _maxHp;

        private Func<int> soulOnHit = delegate () {
            PlayerData _pd = PlayerData.instance;
            int soulNum;
            if (_pd.GetInt("MPCharge") < _pd.GetInt("maxHP"))
            {
                soulNum = 11;
                if (_pd.GetBool("equippedCharm_20"))
                {
                    soulNum += 3;
                }
                if (_pd.GetBool("equippedCharm_21"))
                {
                    soulNum += 8;
                }
            }
            else
            {
                soulNum = 6;
                if (_pd.GetBool("equippedCharm_20"))
                {
                    soulNum += 2;
                }
                if (_pd.GetBool("equippedCharm_21"))
                {
                    soulNum += 6;
                }
            }
            return soulNum;
        };
        /// <summary>
        /// A tracker int to be used in creative ways.
        /// </summary>
        public int soulGivenTracker;

        public void Awake()
        {
            _hm = GetComponent<HealthManager>();
            if (!_hm)
            {
                Destroy(this);
            }

            _maxHp = _hm.hp;
        }

        public void Start()
        {
            On.HealthManager.TakeDamage += HealthManager_TakeDamage;
        }

        public void SetSoulOnHit(Func<int> amount)
        {
            soulOnHit = amount;
        }
        public void SetSoulOnHit(int mainBase, int mainCatcherIncrease, int mainEaterIncrease,
                                 int orbBase, int orbCatcherIncrease, int orbEaterIncrease)
        {
            SetSoulOnHit(delegate ()
            {
                PlayerData _pd = PlayerData.instance;
                int soulNum;
                if (_pd.GetInt("MPCharge") < _pd.GetInt("maxHP"))
                {
                    soulNum = mainBase;
                    if (_pd.GetBool("equippedCharm_20"))
                    {
                        soulNum += mainCatcherIncrease;
                    }
                    if (_pd.GetBool("equippedCharm_21"))
                    {
                        soulNum += mainEaterIncrease;
                    }
                }
                else
                {
                    soulNum = orbBase;
                    if (_pd.GetBool("equippedCharm_20"))
                    {
                        soulNum += orbCatcherIncrease;
                    }
                    if (_pd.GetBool("equippedCharm_21"))
                    {
                        soulNum += orbEaterIncrease;
                    }
                }
                return soulNum;
            });
        }

        public void SetEnemyType(int enemyType)
        {
            ReflectionHelper.SetField<HealthManager, int>(_hm, "enemyType", enemyType);
        }

        public void SetMaxHp(int maxHp)
        {
            _maxHp = maxHp;
        }

        private void HealthManager_TakeDamage(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitInstance)
        {
            GameObject enemy = self.gameObject;

            if (enemy == this.gameObject)
            {
                // 
                Vector3 effectOrigin = ReflectionHelper.GetField<HealthManager, Vector3>(self, "effectOrigin");

                if (hitInstance.AttackType == AttackTypes.Acid && ReflectionHelper.GetField<HealthManager, bool>(self, "ignoreAcid"))
                {
                    return;
                }
                if (CheatManager.IsInstaKillEnabled)
                {
                    hitInstance.DamageDealt = 9999;
                }
                int cardinalDirection = DirectionUtils.GetCardinalDirection(hitInstance.GetActualDirection(self.gameObject.transform));
                ReflectionHelper.SetField<HealthManager, int>(self, "directionOfLastAttack", cardinalDirection);
                FSMUtility.SendEventToGameObject(base.gameObject, "HIT", false);
                FSMUtility.SendEventToGameObject(hitInstance.Source, "HIT LANDED", false);
                FSMUtility.SendEventToGameObject(base.gameObject, "TOOK DAMAGE", false);

                GameObject sendHitTo = ReflectionHelper.GetField<HealthManager, GameObject>(self, "sendHitTo");
                if (sendHitTo != null)
                {
                    FSMUtility.SendEventToGameObject(sendHitTo, "HIT", false);
                }

                Recoil recoil = ReflectionHelper.GetField<HealthManager, Recoil>(self, "recoil");
                if (recoil != null)
                {
                    recoil.RecoilByDirection(cardinalDirection, hitInstance.MagnitudeMultiplier);
                }

                switch (hitInstance.AttackType)
                {
                    case AttackTypes.Nail:
                    case AttackTypes.NailBeam:
                        {
                            int enemyType = ReflectionHelper.GetField<HealthManager, int>(self, "enemyType");
                            if (hitInstance.AttackType == AttackTypes.Nail && enemyType != 3 && enemyType != 6)
                            {
                                SoulGain();
                            }
                            Vector3 position = (hitInstance.Source.transform.position + self.gameObject.transform.position) * 0.5f + effectOrigin;
                            ReflectionHelper.GetField<HealthManager, GameObject>(self, "strikeNailPrefab").Spawn(position, Quaternion.identity);
                            GameObject slashImpactPrefab = ReflectionHelper.GetField<HealthManager, GameObject>(self, "slashImpactPrefab").Spawn(position, Quaternion.identity);
                            switch (cardinalDirection)
                            {
                                case 0:
                                    slashImpactPrefab.transform.SetRotation2D((float)UnityEngine.Random.Range(340, 380));
                                    slashImpactPrefab.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
                                    break;
                                case 1:
                                    slashImpactPrefab.transform.SetRotation2D((float)UnityEngine.Random.Range(70, 110));
                                    slashImpactPrefab.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
                                    break;
                                case 2:
                                    slashImpactPrefab.transform.SetRotation2D((float)UnityEngine.Random.Range(340, 380));
                                    slashImpactPrefab.transform.localScale = new Vector3(-1.5f, 1.5f, 1f);
                                    break;
                                case 3:
                                    slashImpactPrefab.transform.SetRotation2D((float)UnityEngine.Random.Range(250, 290));
                                    slashImpactPrefab.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
                                    break;
                            }
                            break;
                        }
                    case AttackTypes.Generic:
                        ReflectionHelper.GetField<HealthManager, GameObject>(self, "strikeNailPrefab").Spawn(self.gameObject.transform.position + effectOrigin, Quaternion.identity).transform.SetPositionZ(0.0031f);
                        break;
                    case AttackTypes.Spell:
                        ReflectionHelper.GetField<HealthManager, GameObject>(self, "fireballHitPrefab").Spawn(self.gameObject.transform.position + effectOrigin, Quaternion.identity).transform.SetPositionZ(0.0031f);
                        break;
                    case AttackTypes.SharpShadow:
                        ReflectionHelper.GetField<HealthManager, GameObject>(self, "sharpShadowImpactPrefab").Spawn(self.gameObject.transform.position + effectOrigin, Quaternion.identity);
                        break;
                }
                InfectedEnemyEffects hitEffectReceiver = ReflectionHelper.GetField<HealthManager, InfectedEnemyEffects>(self, "hitEffectReceiver");
                if (hitEffectReceiver != null && hitInstance.AttackType != AttackTypes.RuinsWater)
                {
                    hitEffectReceiver.RecieveHitEffect(hitInstance.GetActualDirection(self.gameObject.transform));
                }
                int num = Mathf.RoundToInt((float)hitInstance.DamageDealt * hitInstance.Multiplier);
                if (ReflectionHelper.GetField<HealthManager, bool>(self, "damageOverride"))
                {
                    num = 1;
                }
                self.hp = Mathf.Max(self.hp - num, -50);
                if (regenEnabled)
                {
                    StartCoroutine(PauseRegen(_regenPauseTime));
                }
                if (self.hp > 0)
                {
                    ReflectionHelper.CallMethod<HealthManager>(self, "NonFatalHit", hitInstance.IgnoreInvulnerable);
                    PlayMakerFSM stunControlFSM = ReflectionHelper.GetField<HealthManager, PlayMakerFSM>(self, "stunControlFSM");
                    if (stunControlFSM)
                    {
                        stunControlFSM.SendEvent("STUN DAMAGE");
                        return;
                    }
                }
                else
                {
                    self.Die(new float?(hitInstance.GetActualDirection(self.gameObject.transform)), hitInstance.AttackType, hitInstance.IgnoreInvulnerable);
                }

                return;
            }

            orig(self, hitInstance);
        }
        
        private void SoulGain()
        {

            PlayerData _pd = PlayerData.instance;

            int soulNum = soulOnHit();

            int @int = _pd.GetInt("MPReserve");
            soulNum = ReflectionHelper.CallMethod<ModHooks, int>("OnSoulGain", soulNum);
            _pd.AddMPCharge(soulNum);
            GameCameras.instance.soulOrbFSM.SendEvent("MP GAIN");

            if (_pd.GetInt("MPReserve") != @int)
            {
                GameManager.instance.soulVessel_fsm.SendEvent("MP RESERVE UP");
            }

        }

        
        public void OnDestroy()
        {
            On.HealthManager.TakeDamage -= HealthManager_TakeDamage;
        }
    }
}
