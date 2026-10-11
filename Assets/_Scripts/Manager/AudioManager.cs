using UnityEngine;
using Ami.BroAudio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    #region [Sounds]
    [Header("Music/BGM")]
    public SoundID BGM;

    [Header("SFX")]
    public SoundID SFX_Buy;
    public SoundID SFX_Fire;
    public SoundID SFX_LeafPop;
    public SoundID SFX_UIPopOpen;
    public SoundID SFX_UIPopClose;
    public SoundID SFX_UIPopHover;
    public SoundID SFX_LeafMoving;
    public SoundID SFX_LeafCollected;
    #endregion

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        PlayAudio(BGM);
    }

    public void PlayAudio(SoundID sound) => BroAudio.Play(sound);
    public void StopAudio(SoundID sound) => BroAudio.Stop(sound);
}