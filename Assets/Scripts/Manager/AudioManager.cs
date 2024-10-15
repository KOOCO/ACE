using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;

public class AudioManager : UnitySingleton<AudioManager>
{
    readonly Dictionary<string, AudioClip> soundDic = new();
    readonly Dictionary<string, AudioClip> musicDic = new();

    //AudioPool audioPool;
    AudioSource tittleSourse;
    AudioSource gameSourse;

    public override void Awake()
    {
        base.Awake();
        tittleSourse = gameObject.AddComponent<AudioSource>();
        gameSourse = gameObject.AddComponent<AudioSource>();
    }

    private void Start()
    {
        //audioPool = new AudioPool(transform);
    }

    /// <summary>
    /// 開始加載聲音資源
    /// </summary>
    public void StartLoadAudioAssets()
    {
        StartCoroutine(ILoadAudioAssets());
    }

    /// <summary>
    /// 載入聲音資源
    /// </summary>
    /// <returns></returns>
    private IEnumerator ILoadAudioAssets()
    {
        #region 音效加載

        AudioClip[] soundArray = Resources.LoadAll<AudioClip>("Audio/Sound");
        for (int i = 0; i < soundArray.Length; i++)
        {
            soundDic.Add(soundArray[i].name, soundArray[i]);

            // 每加載一批資源後，暫停一幀並免卡頓
            if ((i + 1) % 10 == 0)
            {
                yield return null;
            }
        }

        #endregion

        #region 音樂加載

        AudioClip[] musicArray = Resources.LoadAll<AudioClip>("Audio/Music");
        for (int i = 0; i < musicArray.Length; i++)
        {
            musicDic.Add(musicArray[i].name, musicArray[i]);

            // 每加載一批資源後，暫停一幀並免卡頓
            if ((i + 1) % 5 == 0)
            {
                yield return null;
            }
        }

        #endregion
    }

    /// <summary>
    /// 獲取音效
    /// </summary>
    /// <param name="soundName"></param>
    /// <returns></returns>
    public AudioClip GetSound(string soundName)
    {
        if (soundDic.ContainsKey(soundName))
        {
            return soundDic[soundName];
        }
        else
        {
            Debug.LogError($"{soundName} Sound Not Find!!!");
            return null;
        }
    }

    /// <summary>
    /// 獲取音樂
    /// </summary>
    /// <param name="soundName"></param>
    /// <returns></returns>
    public AudioClip GetMusic(string musicName)
    {
        if (musicDic.ContainsKey(musicName))
        {
            return musicDic[musicName];
        }
        else
        {
            Debug.LogError($"{musicName} Music Not Find!!!");
            return null;
        }
    }

    /// <summary>
    /// 播放BGM
    /// </summary>
    /// <param name="source"></param>
    public void playTittle()
    {
        tittleSourse.clip = GetMusic("爵士２");
        tittleSourse.loop = true;
        tittleSourse.volume = 0.5f;
        tittleSourse.Play();
    }
    
    /// <summary>
    /// 播放SFX
    /// </summary>
    /// <param name="source"></param>
    public void playSFX(string sfxName)
    {
        gameSourse.clip = GetSound(sfxName);
        gameSourse.loop = false;
        gameSourse.Play();
    }
    
    /// <summary>
    /// 音效播放完畢
    /// </summary>
    /// <param name="source"></param>
    async public void SoundFinished(AudioSource source)
    {
        await Task.Delay((int)(source.clip.length * 1000));
        source.gameObject.SetActive(false);
    }

    /// <summary>
    /// 播放確認音效
    /// </summary>
    public void PlayConfirmClick()
    {
        playSFX("ConfirmClick");
    }

    /// <summary>
    /// 播放取消音效
    /// </summary>
    public void PlayCancelClick()
    {
        playSFX("CancelClick");
    }
    
    /// <summary>
    /// 控制音樂音量
    /// </summary>
    public void setBGMValue(float value)
    {
        tittleSourse.volume = value;
    }
    /// <summary>
    /// 控制音效開關
    /// </summary>
    public void setSFXOnOff(float value)
    {
        gameSourse.volume = value;
    }
}
