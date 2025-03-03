using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public enum AudioClipType
{
    Click, 
    PopUpSound, //�˾� �㶧 ���� ȿ����
    SuccedSound //�ڵ� 2���� �����ϸ� ���� ȿ����
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Tooltip("����� �����Ŭ���� �־��ּ���. �ݵ�� AudioClipType(Enum class) ������ ���� �־���մϴ�.")]
    [SerializeField] AudioClip[] audioClips;

    //�����Ŭ���� ����� ������Ʈ
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
        //���۽� ���� �Ҵ� �� ������� �ʵ��� ����.
        audioSource = GetComponent<AudioSource>();
        audioSource.Stop();
    }

    //������� �÷��� �ϴ� �Լ�.
    //�ܺο��� �� �Լ��� ����ϸ� �˴ϴ�.
    public void PlayAudio(AudioClipType type, float AudioVolume = 0.5f) 
    {
        audioSource.clip = audioClips[(int)type];
        audioSource.volume = AudioVolume;
        audioSource.Play();
    }

    


}
