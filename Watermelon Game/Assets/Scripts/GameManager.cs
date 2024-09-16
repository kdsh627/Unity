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

    //������Ʈ Ǯ�� ����
    Dongle MakeDongle()
    {
        //����Ʈ ����
        GameObject instantEffectObj = Instantiate(effectPrefab, effectGroup);
        instantEffectObj.name = "Effect" + effectPool.Count;
        ParticleSystem instantEffect = instantEffectObj.GetComponent<ParticleSystem>();
        effectPool.Add(instantEffect);

        //���ο� ������ ���� �׷쿡 ����Ͽ� ����
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

        //�� Ȱ��ȭ �Ǿ��־ �Ѱ��� ���� ������?
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

        //������Ʈ Ȱ��ȭ
        lastDongle.gameObject.SetActive(true);

        SfxPlay(Sfx.Next);
        StartCoroutine("WaitNext");
    }
    //�ڷ�ƾ ����
    IEnumerator WaitNext()
    {
        while(lastDongle != null)
        {
            yield return null;
        }
        //2.5�� �޽�
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
        //Ȱ��ȭ �� ��� ���� ��������
        Dongle[] dongles = FindObjectsOfType<Dongle>();

        //��� �������� ����ȿ�� ����
        for (int index = 0; index < dongles.Length; ++index)
        {
            dongles[index].rb2d.simulated = false;
        }

        //��� ���ۿ� �����ؼ� �ϳ��� �����
        for (int index = 0; index < dongles.Length; ++index)
        {
            //���� ���� �� ���� ���� �ѱ��.
            dongles[index].Hide(Vector3.up * 100);
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(1f);

        // �ְ� ���� ����
        int maxScore = Mathf.Max(score, PlayerPrefs.GetInt("MaxScore"));
        PlayerPrefs.SetInt("MaxScore", maxScore);

        //���ӿ��� UI ǥ��
        subScoreText.text = "���� : " + scoreText.text; 
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
