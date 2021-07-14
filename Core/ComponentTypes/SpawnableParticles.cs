/**
@file   PyroDK/Core/ComponentTypes/SpawnableParticles.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-06-12

@brief
  Like Spawnable, but with streamlined behaviors for
  GameObjects with ParticleSystems attached.
**/

using System.Linq;

using UnityEngine;


namespace PyroDK
{

  [AddComponentMenu("PyroDK/Spawnable Particles")]
  //[RequireComponent(typeof(ParticleSystem))]
  [DisallowMultipleComponent]
  public sealed class SpawnableParticles : Spawnable
  {
    [System.Flags]
    public enum SpawnOptions
    {
      None          = (0 << 0),
      StartEmitting = (1 << 0),
      WithChildren  = (1 << 3),
    }

    [System.Flags]
    public enum DespawnOptions
    {
      None          = (0 << 0),
      ClearMemory   = (1 << 0),
      StopEmitting  = (1 << 1),
      WithChildren  = (1 << 3),
    }


  [Header("Child Particle System Options")]
    [SerializeField] [RequiredReference]
    private ParticleSystem[] m_ParticleSystems = new ParticleSystem[0];

    [SerializeField]
    private SpawnOptions   m_SpawnOptions   = SpawnOptions.StartEmitting;
    [SerializeField]
    private DespawnOptions m_DespawnOptions = DespawnOptions.StopEmitting;


    private void OnValidate()
    {
      if (m_ParticleSystems == null     ||
          m_ParticleSystems.Length == 0 ||
         !m_ParticleSystems.Any((ps) => ps))
      {
        m_ParticleSystems = GetComponentsInChildren<ParticleSystem>();
      }
    }


    protected override void PostSpawn()
    {
      if (m_SpawnOptions.HasFlag(SpawnOptions.StartEmitting))
      {
        bool with_children = m_SpawnOptions.HasFlag(SpawnOptions.WithChildren);
        for (int i = 0, ilen = m_ParticleSystems.Length; i < ilen; ++i)
        {
          m_ParticleSystems[i].Play(with_children);
        }
      }
    }

    protected override void PreDespawn()
    {
      if (m_DespawnOptions != DespawnOptions.None)
      {
        bool with_children = m_DespawnOptions.HasFlag(DespawnOptions.WithChildren);

        if (m_DespawnOptions.HasFlag(DespawnOptions.StopEmitting))
        {
          for (int i = 0, ilen = m_ParticleSystems.Length; i < ilen; ++i)
          {
            m_ParticleSystems[i].Stop(with_children, ParticleSystemStopBehavior.StopEmitting);
          }
        }

        if (m_DespawnOptions.HasFlag(DespawnOptions.ClearMemory))
        {
          for (int i = 0, ilen = m_ParticleSystems.Length; i < ilen; ++i)
          {
            m_ParticleSystems[i].Clear(with_children);
          }
        }
      }
    }

  }

}