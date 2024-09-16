using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("-----------[Core ]")]
    public int maxLevel;
    public int score;
    public bool isOver;

    [Header("-----------[Object Pooling ]")]
    [SerializeField] Dongle lastDongle;
    [SerializeField] GameObject donglePrefab;
    [SerializeField] GameObject effectPrefab;
    [SerializeField] Transform dongleGroup;
    [SerializeField] Transform effectGroup;
    [Range(1, 30)]
    public int poolSize;
    public int poolCursor;
    public List<Dongle> donglePool;
    public List<ParticleSystem> effectPool;

    [Header("-----------[Audio ]")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioSource[] sfxPlayer;
    [SerializeField] AudioClip[] sfxClip;
    public enum Sfx { LevelUp, Next, Attach, Button, Over };
    int sfxCursor;

    [Header("-----------[UI ]")]
    [SerializeField] GameObject startGroup;
    [SerializeField] GameObject endGroup;
    [SerializeField] Text scoreText;
    [SerializeField] Text maxScoreText;
    [SerializeField] Text subScoreText;

    [Header("-----------[ETC ]")]
    public GameObject[] hiddenObj;


    void Awake()
    {
        Application.targetFrameRate = 60;

        donglePool = new List<Dongle>();
        effectPool = new List<ParticleSystem>();
        for (int index = 0; index < poolSize; index++)
        {
            MakeDongle();
        }

        if (!PlayerPrefs.HasKey("MaxScore"))
        {
            PlayerPrefs.SetInt("MaxScore", 0);
        }

        maxScoreText.text = PlayerPrefs.GetInt("MaxScore").ToString();
    }
    public void GameStart()
    {
        for(int index = 0; index < hiddenObj.Length; index++) 
        {
            hiddenObj[index].SetActive(true);
        }

        startGroup.SetActive(false);

        audioSource.Play();
        SfxPlay(Sfx.Button);

        Invoke("NextDongle", 1.5f);
    }

    //오브젝트 풀링 진행
    Dongle MakeDongle()
    {
        //이펙트 생성
        GameObject instantEffectObj = Instantiate(effectPrefab, effectGroup);
        instantEffectObj.name = "Effect" + effectPool.Count;
        ParticleSystem instantEffect = instantEffectObj.GetComponent<ParticleSystem>();
        effectPool.Add(instantEffect);

        //새로운 동글을 동글 그룹에 상속하여 생성
        GameObject instantDongleObj = Instantiate(donglePrefab, dongleGroup);
        instantDongleObj.name = "Dongle" + donglePool.Count;
        Dongle instantDongle = instantDongleObj.GetComponent<Dongle>();
        instantDongle.gameManager = this;
        instantDongle.effect = instantEffect;
        donglePool.Add(instantDongle);

        return instantDongle;
    }

    Dongle GetDongle()
    {
        for(int index = 0; index < donglePool.Count; index++)
        {
            poolCursor = (poolCursor + 1) % donglePool.Count;
            if (!donglePool[poolCursor].gameObject.activeSelf)
            {
                return donglePool[poolCursor];
            }
        }

        //다 활성화 되어있어서 넘겨줄 것이 없으면?
        return MakeDongle();
    }

    void NextDongle()
    {
        if(isOver)
        {
            return;
        }

        lastDongle = GetDongle();
        lastDongle.level = Random.Range(0, maxLevel);

        //오브젝트 활성화
        lastDongle.gameObject.SetActive(true);

        SfxPlay(Sfx.Next);
        StartCoroutine("WaitNext");
    }
    //코루틴 생성
    IEnumerator WaitNext()
    {
        while(lastDongle != null)
        {
            yield return null;
        }
        //2.5초 휴식
        yield return new WaitForSeconds(2.5f);

        NextDongle();
    }

    public void TouchDown()
    {
        if(lastDongle == null) 
        {
            return;
        }

        lastDongle.Drag();
    }
    public void TouchUp()
    {
        if (lastDongle == null)
        {
            return;
        }

        lastDongle.Drop();
        lastDongle = null;
    }

    public void GameOver()
    { 
        if(isOver)
        {
            return;
        }
        isOver = true;

        StartCoroutine("GameOverRoutine");
    }

    IEnumerator GameOverRoutine()
    {
        //활성화 된 모든 동글 가져오기
        Dongle[] dongles = FindObjectsOfType<Dongle>();

        //모든 동글이의 물리효과 제거
        for (int index = 0; index < dongles.Length; ++index)
        {
            dongles[index].rb2d.simulated = false;
        }

        //모든 동글에 접근해서 하나씩 지우기
        for (int index = 0; index < dongles.Length; ++index)
        {
            //절대 나올 수 없는 값을 넘긴다.
            dongles[index].Hide(Vector3.up * 100);
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(1f);

        // 최고 점수 갱신
        int maxScore = Mathf.Max(score, PlayerPrefs.GetInt("MaxScore"));
        PlayerPrefs.SetInt("MaxScore", maxScore);

        //게임오버 UI 표시
        subScoreText.text = "점수 : " + scoreText.text; 
        endGroup.SetActive(true);

        audioSource.Stop();
        SfxPlay(Sfx.Over);
    }

    public void Restart()
    {
        SfxPlay(Sfx.Button);
        StartCoroutine("ResetCorutine");
    }

    IEnumerator ResetCorutine()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(0);
    }

    public void SfxPlay(Sfx type)
    {
        switch (type)
        {
            case Sfx.LevelUp:
                sfxPlayer[sfxCursor].clip = sfxClip[Random.Range(0, 3)];
                break;
            case Sfx.Next:
                sfxPlayer[sfxCursor].clip = sfxClip[3];
                break;
            case Sfx.Attach:
                sfxPlayer[sfxCursor].clip = sfxClip[4];
                break;
            case Sfx.Button:
                sfxPlayer[sfxCursor].clip = sfxClip[5];
                break;
            case Sfx.Over:
                sfxPlayer[sfxCursor].clip = sfxClip[6];
                break;
        }

        sfxPlayer[sfxCursor].Play();
        sfxCursor = (sfxCursor + 1) % sfxPlayer.Length;
    }

    void Update()
    {
        if(Input.GetButtonDown("Cancel"))
        {
            Application.Quit();
        }
    }

    void LateUpdate()
    {
        scoreText.text = score.ToString();    
    }
}
