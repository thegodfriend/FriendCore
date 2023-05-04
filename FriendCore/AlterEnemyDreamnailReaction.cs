using Modding;
using System;
using UnityEngine;

namespace FriendCore
{
    public class AlterEnemyDreamnailReaction : MonoBehaviour
    {

        private EnemyDreamnailReaction _dreamnailReaction;

        private bool noReaction = false;
        private bool noSoul = false;
        private bool noConvo = false;
        private Func<int> soulAmount = delegate ()
        {
            return (GameManager.instance.playerData.GetBool("equippedCharm_30") ? 66 : 33);
        };

        void Awake()
        {
            _dreamnailReaction = GetComponent<EnemyDreamnailReaction>();
            if (!_dreamnailReaction)
            {
                Destroy(this);
            }
        }

        void Start()
        {
            On.EnemyDreamnailReaction.RecieveDreamImpact += EnemyDreamnailReaction_RecieveDreamImpact;
        }

        public void SetNoReaction(bool value = true)
        {
            noReaction = value;
        }

        public void SetNoSoul(bool value = true)
        {
            noSoul = value;
        }

        public void SetNoConvo(bool value = true)
        {
            noConvo = value;
        }

        public void SetConvoTitle(string title)
        {
            ReflectionHelper.SetField<EnemyDreamnailReaction, string>(_dreamnailReaction, "convoTitle", title);
        }

        public void SetConvoAmount(int amount)
        {
            ReflectionHelper.SetField<EnemyDreamnailReaction, int>(_dreamnailReaction, "convoAmount", amount);
        }

        public void SetConvo(string title, int amount)
        {
            SetConvoTitle(title);
            SetConvoAmount(amount);
        }

        public void SetSoulGiven(Func<int> amount)
        {
            soulAmount = amount;
            SetNoSoul(false);
        }
        public void SetSoulGiven(int normalAmount, int wielderCharmAmount)
        {
            SetSoulGiven(delegate ()
            {
                return (GameManager.instance.playerData.GetBool("equippedCharm_30")
                       ? wielderCharmAmount
                       : normalAmount);
            });
        }
        public void SetSoulGiven(int amount)
        {
            SetSoulGiven(delegate ()
            {
                return amount;
            });
        }

        private void EnemyDreamnailReaction_RecieveDreamImpact(On.EnemyDreamnailReaction.orig_RecieveDreamImpact orig, EnemyDreamnailReaction self)
        {
            GameObject enemy = self.gameObject;
            if (enemy == this.gameObject)
            {
                if (noReaction)
                    return;

                if (ReflectionHelper.GetField<EnemyDreamnailReaction, int>(self, "state") != 1)
                {
                    return;
                }
                if (!noSoul)
                {
                    HeroController.instance.AddMPCharge(soulAmount());
                }
                if (!noConvo)
                {
                    ReflectionHelper.CallMethod<EnemyDreamnailReaction>(self, "ShowConvo");
                }
                GameObject dreamImpactPrefab = ReflectionHelper.GetField<EnemyDreamnailReaction, GameObject>(self, "dreamImpactPrefab");
                if (dreamImpactPrefab != null)
                {
                    dreamImpactPrefab.Spawn().transform.position = this.gameObject.transform.position;
                }
                Recoil recoil = this.gameObject.GetComponent<Recoil>();
                if (recoil != null)
                {
                    bool flag = HeroController.instance.transform.localScale.x <= 0f;
                    recoil.RecoilByDirection(flag ? 0 : 2, 2f);
                }
                SpriteFlash sf = this.gameObject.GetComponent<SpriteFlash>();
                if (sf != null)
                {
                    sf.flashDreamImpact();
                }
                ReflectionHelper.SetField<EnemyDreamnailReaction, int>(self, "state", 2);
                ReflectionHelper.SetField<EnemyDreamnailReaction, float>(self, "cooldownTimeRemaining", 0.2f);
            }
            orig(self);
        }


        void OnDestroy()
        {
            On.EnemyDreamnailReaction.RecieveDreamImpact -= EnemyDreamnailReaction_RecieveDreamImpact;
        }

    }
}
