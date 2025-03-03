using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public enum AudioClipType
{
    Click, 
    PopUpSound, //팝업 뜰때 나는 효과음
    SuccedSound //코딩 2에서 성공하면 나는 효과음
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Tooltip("사용한 오디오클립을 넣어주세요. 반드시 AudioClipType(Enum class) 순서에 따라 넣어야합니다.")]
    [SerializeField] AudioClip[] audioClips;

    //오디오클립을 재생할 컴포넌트
    private AudioSource audioSource;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        //시작시 변수 할당 및 재생되지 않도록 설정.
        audioSource = GetComponent<AudioSource>();
        audioSource.Stop();
    }

    //오디오를 플레이 하는 함수.
    //외부에서 이 함수를 사용하면 됩니다.
    public void PlayAudio(AudioClipType type, float AudioVolume = 0.5f) 
    {
        audioSource.clip = audioClips[(int)type];
        audioSource.volume = AudioVolume;
        audioSource.Play();
    }

    


}
