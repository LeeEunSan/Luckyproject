using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource uiSource; // UI 전용 AudioSource

    [Header("BGM Clips")]
    [SerializeField] private AudioClip mainMenuBGM;
    [SerializeField] private AudioClip inGameBGM;
    [SerializeField] private AudioClip bossBGM; // 보스 전용 BGM

    [Header("UI SFX")]
    [SerializeField] private AudioClip buttonClickSFX;
    [SerializeField] private AudioClip countdownSFX;
    
    [Header("Combat SFX")]
    [SerializeField] private AudioClip heroMeleeAttackSFX;  // 근거리 공격
    [SerializeField] private AudioClip heroRangedAttackSFX; // 원거리 공격
    [SerializeField] private AudioClip enemyDeathSFX;
    
    [Header("Summon SFX")]
    [SerializeField] private AudioClip summonSuccessSFX;    // 소환 성공
    [SerializeField] private AudioClip summonFailSFX;       // 소환 실패
    [SerializeField] private AudioClip summonEpicSFX;       // 에픽 뽑기
    [SerializeField] private AudioClip summonLegendarySFX; // 전설 뽑기
    [SerializeField] private AudioClip summonMythicSFX;     // 신화 뽑기
    [SerializeField] private AudioClip specialMonsterSFX;   // 특별 몬스터 소환
    [SerializeField] private AudioClip heroSpawnSFX;        // 영웅 소환됨
    
    [Header("Enhancement SFX")]
    [SerializeField] private AudioClip enhanceSuccessSFX;   // 강화 성공
    [SerializeField] private AudioClip enhanceMaxSFX;       // 강화 맥스 도달
    
    [Header("Misc SFX")]
    [SerializeField] private AudioClip mergeSFX;

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float bgmVolume = 0.7f;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 1f)] public float uiVolume = 1f;

    private Dictionary<string, AudioClip> bgmDict;
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        // 싱글톤 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSources();
            InitializeBGMDictionary();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeAudioSources()
    {
        // AudioSource가 없으면 생성
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.playOnAwake = false;
            bgmSource.loop = true;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }

        if (uiSource == null)
        {
            uiSource = gameObject.AddComponent<AudioSource>();
            uiSource.playOnAwake = false;
        }

        UpdateVolume();
    }

    private void InitializeBGMDictionary()
    {
        bgmDict = new Dictionary<string, AudioClip>
        {
            { "MainScene", mainMenuBGM },
            { "TestScene", inGameBGM }
        };
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬에 맞는 BGM 재생
        PlaySceneBGM(scene.name);

        // 버튼에 클릭 사운드 자동 추가
        StartCoroutine(AddButtonSoundsDelayed());
    }

    private IEnumerator AddButtonSoundsDelayed()
    {
        // 씬 로드 직후 버튼들이 생성되기를 기다림
        yield return new WaitForEndOfFrame();
        
        Button[] buttons = FindObjectsOfType<Button>();
        foreach (Button btn in buttons)
        {
            btn.onClick.RemoveListener(PlayButtonClick);
            btn.onClick.AddListener(PlayButtonClick);
        }
    }

    // 특정 부모 오브젝트 하위의 버튼들에 사운드 추가
    public void AddButtonSoundsToParent(GameObject parent)
    {
        if (parent == null) return;

        Button[] buttons = parent.GetComponentsInChildren<Button>(true);
        foreach (Button btn in buttons)
        {
            btn.onClick.RemoveListener(PlayButtonClick);
            btn.onClick.AddListener(PlayButtonClick);
        }
    }

    #region BGM Methods
    
    private void PlaySceneBGM(string sceneName)
    {
        if (bgmDict.TryGetValue(sceneName, out AudioClip clip))
        {
            if (clip != null && bgmSource.clip != clip)
            {
                if (fadeCoroutine != null)
                    StopCoroutine(fadeCoroutine);
                    
                fadeCoroutine = StartCoroutine(FadeBGM(clip));
            }
        }
    }

    private IEnumerator FadeBGM(AudioClip newClip, float fadeDuration = 0.5f)
    {
        // Fade out
        float startVolume = bgmSource.volume;
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            bgmSource.volume = Mathf.Lerp(startVolume, 0, t / fadeDuration);
            yield return null;
        }

        // Change clip
        bgmSource.Stop();
        bgmSource.clip = newClip;
        bgmSource.Play();

        // Fade in
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            bgmSource.volume = Mathf.Lerp(0, bgmVolume * masterVolume, t / fadeDuration);
            yield return null;
        }

        bgmSource.volume = bgmVolume * masterVolume;
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    public void PauseBGM()
    {
        bgmSource.Pause();
    }

    public void ResumeBGM()
    {
        bgmSource.UnPause();
    }

    #endregion

    #region SFX Methods

    // UI 사운드
    public void PlayButtonClick()
    {
        PlayUISound(buttonClickSFX);
    }

    public void PlayCountdown()
    {
        PlaySFX(countdownSFX);
    }

    // 전투 사운드
    public void PlayHeroAttack(HeroType heroType)
    {
        if (heroType == HeroType.Melee)
            PlaySFX(heroMeleeAttackSFX);
        else if (heroType == HeroType.Ranged)
            PlaySFX(heroRangedAttackSFX);
    }

    public void PlayEnemyDeath()
    {
        PlaySFX(enemyDeathSFX);
    }

    // 소환 사운드
    public void PlaySummonSuccess()
    {
        PlaySFX(summonSuccessSFX);
    }

    public void PlaySummonFail()
    {
        PlaySFX(summonFailSFX);
    }

    public void PlaySummonByRarity(HeroRarity rarity)
    {
        switch (rarity)
        {
            case HeroRarity.Epic:
                PlaySFX(summonEpicSFX);
                break;
            case HeroRarity.Legendary:
                PlaySFX(summonLegendarySFX);
                break;
            case HeroRarity.Mythic:
                PlaySFX(summonMythicSFX);
                break;
            default:
                PlaySummonSuccess();
                break;
        }
    }

    public void PlayHeroSpawn()
    {
        PlaySFX(heroSpawnSFX);
    }

    public void PlaySpecialMonsterSpawn()
    {
        PlaySFX(specialMonsterSFX);
    }

    // 강화 사운드
    public void PlayEnhanceSuccess()
    {
        PlaySFX(enhanceSuccessSFX);
    }

    public void PlayEnhanceMax()
    {
        PlaySFX(enhanceMaxSFX);
    }

    // 기타 사운드
    public void PlayMerge()
    {
        PlaySFX(mergeSFX);
    }

    // 보스 BGM 관련
    public void PlayBossBGM()
    {
        if (bossBGM != null)
        {
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);
                
            fadeCoroutine = StartCoroutine(FadeBGM(bossBGM));
        }
    }

    public void StopBossAndPlayNormalBGM()
    {
        PlaySceneBGM("TestScene");
    }

    private void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, sfxVolume * masterVolume);
        }
    }

    private void PlayUISound(AudioClip clip)
    {
        if (clip != null && uiSource != null)
        {
            uiSource.PlayOneShot(clip, uiVolume * masterVolume);
        }
    }

    // 3D 위치에서 사운드 재생
    public void PlaySFXAtPosition(AudioClip clip, Vector3 position)
    {
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, position, sfxVolume * masterVolume);
        }
    }

    #endregion

    #region Volume Control

    public void UpdateVolume()
    {
        if (bgmSource != null)
            bgmSource.volume = bgmVolume * masterVolume;
        if (sfxSource != null)
            sfxSource.volume = sfxVolume * masterVolume;
        if (uiSource != null)
            uiSource.volume = uiVolume * masterVolume;
    }

    public void SetMasterVolume(float value)
    {
        masterVolume = Mathf.Clamp01(value);
        UpdateVolume();
        SaveVolumeSettings();
    }

    public void SetBGMVolume(float value)
    {
        bgmVolume = Mathf.Clamp01(value);
        UpdateVolume();
        SaveVolumeSettings();
    }

    public void SetSFXVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);
        UpdateVolume();
        SaveVolumeSettings();
    }

    private void SaveVolumeSettings()
    {
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.SetFloat("BGMVolume", bgmVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.SetFloat("UIVolume", uiVolume);
        PlayerPrefs.Save();
    }

    private void LoadVolumeSettings()
    {
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 0.7f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        uiVolume = PlayerPrefs.GetFloat("UIVolume", 1f);
        UpdateVolume();
    }

    #endregion

    private void Start()
    {
        LoadVolumeSettings();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 디버그용
    [ContextMenu("Test All Sounds")]
    private void TestAllSounds()
    {
        Debug.Log("Testing all sounds...");
        PlayButtonClick();
        StartCoroutine(TestSoundsSequence());
    }

    private IEnumerator TestSoundsSequence()
    {
        yield return new WaitForSeconds(0.5f);
        PlaySummonSuccess();
        yield return new WaitForSeconds(0.5f);
        PlayHeroAttack(HeroType.Melee);
        yield return new WaitForSeconds(0.5f);
        PlayHeroAttack(HeroType.Ranged);
        yield return new WaitForSeconds(0.5f);
        PlayEnemyDeath();
    }
}