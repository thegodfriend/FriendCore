using System.Collections;
using UnityEngine;

namespace FriendCore
{
    public partial class AlterHealthManager
    {

        private bool regenEnabled = false;

        private float _regenPauseTime = 1f;
        private float _regenRate = 0.5f;
        private int _regenPerTick = 1;

        private int regenPausers = 0;

        public void SetRegenEnabled(bool enabled = true)
        {
            regenEnabled = enabled;
            StartCoroutine(Regeneration());
        }

        public void SetRegenPauseTimeOnHit(float t)
        {
            _regenPauseTime = t;
        }

        public void SetRegenTickRate(float r)
        {
            _regenRate = r;
        }

        public void SetRegenHpPerTick(int i)
        {
            _regenPerTick = i;
        }

        public void SetRegen(float pauseTimeOnHit, float tickRate, int hpPerTick)
        {
            SetRegenPauseTimeOnHit(pauseTimeOnHit);
            SetRegenTickRate(tickRate);
            SetRegenHpPerTick(hpPerTick);
            SetRegenEnabled();
        }

        IEnumerator Regeneration()
        {
            while (regenEnabled)
            {
                yield return new WaitForSeconds(_regenRate);
                if (!(regenPausers > 0))
                {
                    _hm.hp = Mathf.Min(_hm.hp + _regenPerTick, _maxHp);
                }
            }
        }

        IEnumerator PauseRegen(float t)
        {
            regenPausers += 1;
            yield return new WaitForSeconds(t);
            regenPausers -= 1;
        }

    }
}
