// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;

namespace EndlessClient.Audio
{
    public class SoundInfo : IDisposable
    {
        private readonly SoundEffect m_effect;

        private readonly List<SoundEffectInstance> m_instances;

        private SoundEffectInstance m_loopingInstance; //there SHOULD only be one of these...

        public SoundInfo(SoundEffect toWrap)
        {
            if (toWrap == null) return;

            m_effect = toWrap;
            m_instances = new List<SoundEffectInstance> { toWrap.CreateInstance() };
            m_loopingInstance = null;
        }

        public SoundEffectInstance GetNextAvailableInstance()
        {
            if (m_effect == null) return null;

            SoundEffectInstance ret = m_instances.Find(_sei => _sei.State == SoundState.Stopped);
            if (ret == null)
                m_instances.Add(ret = m_effect.CreateInstance());
            return ret;
        }

        public void PlayLoopingInstance()
        {
            if (m_effect == null) return;

            if (m_loopingInstance == null)
            {
                m_loopingInstance = m_effect.CreateInstance();
                m_loopingInstance.IsLooped = true;
            }

            m_loopingInstance.Play();
        }

        public void StopLoopingInstance()
        {
            if (m_loopingInstance == null) return;

            m_loopingInstance.Stop(true);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (m_loopingInstance != null)
                    m_loopingInstance.Dispose();

                m_instances.ForEach(_inst =>
                {
                    _inst.Stop();
                    _inst.Dispose();
                });
                m_effect.Dispose();
            }

            GC.SuppressFinalize(this);
        }
    }
}
