using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour {

    public enum AudioChanel
    {
        Master, Sfx, Music
    };

    public float masterVolumePercent { get; private set;}
    public float sfxVolumePercent { get; private set;}
    public float musicVolumePercent { get; private set;}

    AudioSource sfx2DSource;
    AudioSource[] musicSources;
    int activeMusicSourceIndex;

    Transform playerT;
    Transform audioListener;

    public static AudioManager instance;

    SoundLibrary library;

    void Awake(){
        if (instance != null)
        {
            Destroy(gameObject);
        } 
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            library = GetComponent<SoundLibrary>();

            musicSources = new AudioSource[2];
            for (int i = 0; i < 2; i++)
            {
                GameObject newMusicSource = new GameObject("Music source " + (i + 1));
                musicSources [i] = newMusicSource.AddComponent<AudioSource>();
                newMusicSource.transform.parent = transform;
            }
            GameObject newSfx2DSource = new GameObject("Sfx 2d source");
            sfx2DSource = newSfx2DSource.AddComponent<AudioSource>();
            newSfx2DSource.transform.parent = transform;

            if (FindObjectOfType<Character>() != null)
            {
                playerT = FindObjectOfType<Character>().transform;
            }

            audioListener = FindObjectOfType<AudioListener>().transform;

            masterVolumePercent = PlayerPrefs.GetFloat("master vol", 1);
            sfxVolumePercent = PlayerPrefs.GetFloat("sfx vol", 1);
            musicVolumePercent = PlayerPrefs.GetFloat ("music vol", .8f);

            print("mas" + masterVolumePercent + 
                "\nsfx" + sfxVolumePercent +
                "\nmusic" + musicVolumePercent);
        }
    }

    void Update(){
        if (playerT != null)
        {
            audioListener.position = playerT.position;
        }
    }

    public void SetVolume(float volumePercent, AudioChanel channel){
        switch (channel)
        {
        case AudioChanel.Master:
            masterVolumePercent = volumePercent;
            break;
        case AudioChanel.Sfx:
            sfxVolumePercent = volumePercent;
            break;
        case AudioChanel.Music:
            musicVolumePercent = volumePercent;
            break;
        }

        musicSources [0].volume = masterVolumePercent * musicVolumePercent;
        musicSources [1].volume = masterVolumePercent * musicVolumePercent;

        PlayerPrefs.SetFloat("master vol", masterVolumePercent);
        PlayerPrefs.SetFloat("sfx vol", sfxVolumePercent);
        PlayerPrefs.SetFloat("music vol", musicVolumePercent);
        PlayerPrefs.Save();
    }

    public void PlayMusic(AudioClip clip, float duration = 1){
        activeMusicSourceIndex = 1 - activeMusicSourceIndex;
        musicSources [activeMusicSourceIndex].clip = clip;
        //        musicSources [activeMusicSourceIndex].loop = true;
        musicSources [activeMusicSourceIndex].Play();

        StartCoroutine(AnimateMusicCrossFade(duration));
    }

    public void PlayeSound(AudioClip clip, Vector3 pos){
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, pos, masterVolumePercent * sfxVolumePercent);
        }
    }
    
    public void PlayeSound(string clipName, Vector3 pos){
        PlayeSound(library.GetClipFromName(clipName), pos);
    }

    public void PlayeSound2D(string clipName){
        //PlayeSound(library.GetClipFromName(clipName), pos);
        sfx2DSource.PlayOneShot(library.GetClipFromName(clipName), sfxVolumePercent * masterVolumePercent);
    }

    IEnumerator AnimateMusicCrossFade(float duration){
        float percent = 0;
        float speed = 1 / duration;
        int deactiveMusicSourceIndex = 1 - activeMusicSourceIndex;;

        while (percent < 1){
            percent += Time.deltaTime * speed;
            musicSources [activeMusicSourceIndex].volume = Mathf.Lerp(0, musicVolumePercent * masterVolumePercent, percent);
            musicSources [deactiveMusicSourceIndex].volume = Mathf.Lerp(musicVolumePercent * masterVolumePercent, 0, percent);

            yield return null;
        }

        musicSources [deactiveMusicSourceIndex].Stop();
    }

    void OnLevelWasLoaded(int index) {
        if (playerT == null) {
            if (FindObjectOfType<Character> () != null) {
                playerT = FindObjectOfType<Character> ().transform;
            }
        }
        
        if (audioListener == null) {
            audioListener = FindObjectOfType<AudioListener>().transform;
        }
    }

}
