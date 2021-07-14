/**
@file   PyroDK/Audio/AssetTypes/AudioEventRef.cs
@author Levi Perez (Pyr3z)
@author levi@leviperez.dev
@date   2020-11-11

@brief
  --
**/

#pragma warning disable CS0649

using UnityEngine;

#if FMOD
using FMODUnity;
using FMOD;
using FMOD.Studio;
#endif


namespace PyroDK.Audio
{

  #if FMOD
  [CreateAssetMenu(menuName = "PyroDK/Audio/Event Reference (FMOD)", order = -120)]
  #endif
  public class AudioEventRef : BaseAsset
  {
    public enum InstanceTracking
    {
      None,
      MapWithGameObjects,
      OnlyAllowOneInstance
    }


    #if FMOD

  [Header("FMOD Event Reference")]
    [SerializeField] [EventRef]
    private string m_EventPath;

  [Header("Options")]
    [SerializeField]
    private bool m_Preload = false;
    [SerializeField]
    private InstanceTracking m_InstanceTracking = InstanceTracking.None;


    [System.NonSerialized]
    private EventDescription m_EventDesc;
    [System.NonSerialized]
    private EventInstance m_UniqueInstance;
    [System.NonSerialized]
    private HashMap<GameObject, EventInstance> m_TrackedInstances = new HashMap<GameObject, EventInstance>();



    public void Post()
    {
      if (MakeInstance(out EventInstance inst))
      {
        _ = inst.start();

        if (m_InstanceTracking != InstanceTracking.OnlyAllowOneInstance)
        {
          _ = m_UniqueInstance.release();
        }
      }
    }

    public void Post3D(GameObject obj)
    {
      if (m_InstanceTracking == InstanceTracking.OnlyAllowOneInstance &&
          m_UniqueInstance.isValid() &&
          m_UniqueInstance.getPlaybackState(out PLAYBACK_STATE state) == RESULT.OK)
      {
        if (state == PLAYBACK_STATE.PLAYING)
        {
          RuntimeManager.AttachInstanceToGameObject(m_UniqueInstance,
                                                    obj.transform,
                                                    obj.GetComponent<Rigidbody>());
          return;
        }

        _ = m_UniqueInstance.release();
      }

      if (MakeInstance(out EventInstance inst))
      {
        RuntimeManager.AttachInstanceToGameObject(inst,
                                                  obj.transform,
                                                  obj.GetComponent<Rigidbody>());
        _ = inst.start();
        
        if (m_InstanceTracking != InstanceTracking.OnlyAllowOneInstance &&
           ( m_InstanceTracking != InstanceTracking.MapWithGameObjects || Logging.Assert(m_TrackedInstances.Remap(obj, inst)) ))
        {
          _ = inst.release();
        }
      }
      else
      {
        $"Failed to instantiate FMOD event \"{m_EventPath}\"!"
          .LogError(this);
      }
    }

    public void Stop3D(GameObject obj)
    {
      if (m_InstanceTracking == InstanceTracking.OnlyAllowOneInstance)
      {
        if (m_UniqueInstance.isValid())
        {
          _ = m_UniqueInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
          _ = m_UniqueInstance.release();
          m_UniqueInstance.clearHandle();
        }
      }
      else if ( m_InstanceTracking == InstanceTracking.MapWithGameObjects &&
                m_TrackedInstances.Find(obj, out EventInstance inst))
      {
        if (inst.isValid())
        {
          _ = inst.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }

        m_TrackedInstances.Unmap(obj);
      }
      else
      {
        $"Stop3D() called, but \"Track Attached Postings\" is not set, or nothing is tracked. Results undefined!"
          .LogWarning(this);

        StopLast(allow_fadout: true);
      }
    }

    public void StopLast()
    {
      StopLast(allow_fadout: true);
    }

    public void StopLast(bool allow_fadout) // warning: potentially expensive!
    {
      if (m_InstanceTracking == InstanceTracking.OnlyAllowOneInstance)
      {
        if (m_UniqueInstance.isValid() &&
            m_UniqueInstance.getPlaybackState(out PLAYBACK_STATE state) == RESULT.OK &&
            state != PLAYBACK_STATE.STOPPED &&
            state != PLAYBACK_STATE.STOPPING)
        {
          _ = m_UniqueInstance.stop(allow_fadout ?  FMOD.Studio.STOP_MODE.ALLOWFADEOUT :
                                                    FMOD.Studio.STOP_MODE.IMMEDIATE);
          _ = m_UniqueInstance.release();
          m_UniqueInstance.clearHandle();
        }

        return;
      }

      if (m_EventDesc.getInstanceList(out EventInstance[] insts) != RESULT.OK ||
          insts.Length == 0)
      {
        return;
      }

      for (int i = insts.Length - 1; i >= 0; --i)
      {
        if (insts[i].getPlaybackState(out PLAYBACK_STATE state) == RESULT.OK &&
            state != PLAYBACK_STATE.STOPPED &&
            state != PLAYBACK_STATE.STOPPING)
        {
          insts[i].stop(allow_fadout ?  FMOD.Studio.STOP_MODE.ALLOWFADEOUT :
                                        FMOD.Studio.STOP_MODE.IMMEDIATE);
          return;
        }
      }

      $"Could not find instance of FMOD Event \"{m_EventPath}\" to stop."
        .LogWarning(this);
    }


    private void Awake()
    {
      if (m_Preload)
      {
        _ = FindEvent();
      }
    }


    private bool MakeInstance(out EventInstance inst)
    {
      inst = default;
      return  ( m_InstanceTracking != InstanceTracking.OnlyAllowOneInstance || !m_UniqueInstance.isValid() ) &&
                FindEvent() &&
                m_EventDesc.createInstance(out inst) == RESULT.OK &&
              ( m_UniqueInstance = inst ).isValid();
    }

    private bool FindEvent()
    {
      if (m_EventPath.IsEmpty())
        return false;

      if (m_EventDesc.isValid())
        return true;

      m_EventDesc = RuntimeManager.GetEventDescription(m_EventPath);

      if (Logging.Assert(m_EventDesc.isValid(), $"FMOD Event path \"{m_EventPath}\" could not be resolved."))
        return false;

      if (m_Preload)
      {
        m_EventDesc.loadSampleData();

        RuntimeManager.StudioSystem.update();

        m_EventDesc.getSampleLoadingState(out LOADING_STATE state);

        while (state == LOADING_STATE.LOADING)
        {
          System.Threading.Thread.Sleep(1);
          m_EventDesc.getSampleLoadingState(out state);
        }
      }

      return true;
    }


    private static bool IsStaleInstance(GameObject obj, EventInstance inst)
    {
      return  ( !obj || !inst.isValid() ) ||
              ( RESULT.OK != inst.getPlaybackState(out PLAYBACK_STATE state) || state == PLAYBACK_STATE.STOPPED );
    }


    #else   // !FMOD

    // still supply fields, so we don't accidentally lose serialized values:

    [SerializeField]
    private string m_EventPath;
    [SerializeField]
    private bool m_Preload;
    [SerializeField]
    private InstanceTracking m_InstanceTracking;

    // dummies:

    public void Post()
    {
      "Are you missing \"FMOD\" in the preprocessor defs?"
        .LogError(this);
    }
    public void PostOn(GameObject _ )
    {
      "Are you missing \"FMOD\" in the preprocessor defs?"
        .LogError(this);
    }
    public void StopLast(bool _ )
    {
      "Are you missing \"FMOD\" in the preprocessor defs?"
        .LogError(this);
    }

    #endif  // FMOD
  }

}