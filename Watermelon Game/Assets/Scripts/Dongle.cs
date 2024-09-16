using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Dongle : MonoBehaviour
{
    public GameManager gameManager;
    public ParticleSystem effect;
    public Rigidbody2D rb2d;
    public int level;
    public bool isMerge = false;
    public bool isAttach = false;

    [SerializeField] bool isDrag = false;

    Animator anim;
    CircleCollider2D circleCollider;
    SpriteRenderer spriteRenderer;

    float deadTime;
    void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        circleCollider = GetComponent<CircleCollider2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    //스크립트 활성화 시 자동실행
    void OnEnable()
    {
        anim.SetInteger("Level", level);
    }

    void OnDisable()
    {
        //속성 초기화
        level = 0;
        isDrag = false;
        isMerge = false;
        isAttach = false;

        //트랜스폼 초기화
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.zero;

        //물리 초기화
        rb2d.simulated = false;
        rb2d.velocity = Vector2.zero;
        rb2d.angularVelocity = 0;
        circleCollider.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (isDrag)
        {
            //스크린 좌표계를 월드 좌표계로 변환
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            float radius = transform.localScale.x / 2f;
            float leftBorder = -4.2f + radius;
            float rightBorder = 4.2f - radius;

            //왼쪽을 벗어나면
            if (mousePos.x < leftBorder)
            {
                //x 고정
                mousePos.x = leftBorder;
            }
            //오른 쪽을 벗어나면
            else if (mousePos.x > rightBorder)
            {
                mousePos.x = rightBorder;
            }

            mousePos.y = 8f;
            mousePos.z = 0f;
            transform.position = Vector3.Lerp(transform.position, mousePos, 0.5f);
        }
    }

    public void Drag()
    {
        isDrag = true;
    }
    public void Drop()
    {
        isDrag = false;
        rb2d.simulated = true;
    }

    void Crush(Collision2D collision)
    {
        if (collision.gameObject.tag == "Dongle")
        {
            Dongle other = collision.gameObject.GetComponent<Dongle>();

            if (level == other.level && !isMerge && !other.isMerge && level < 7)
            {
                //합치기 실행
                float X = transform.position.x;
                float Y = transform.position.y;
                float otherX = other.transform.position.x;
                float otherY = other.transform.position.y;

                if (Y < otherY || (Y == otherY && X > otherX))
                {
                    //Y가 같으면 내가 오른쪽에 있을때, 다르면 내가 아래에 있을 때
                    //상대방을 나한테 끌어오면서 숨긴다.
                    other.Hide(transform.position);

                    //레벨업
                    levelUp();

                }
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Crush(collision);

        StartCoroutine("AttachRoutine");
    }

    IEnumerator AttachRoutine()
    {
        if(isAttach)
        {
            //탈출
            yield break;
        }

        isAttach = true;
        gameManager.SfxPlay(GameManager.Sfx.Attach);
        yield return new WaitForSeconds(0.2f);

        isAttach = false;
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        Crush(collision);
    }

    //숨기는 함수
    public void Hide(Vector3 targetPos)
    {
        isMerge = true;

        rb2d.simulated = false;
        circleCollider.enabled = false;

        if(targetPos == Vector3.up* 100)
        {
            EffectPlay();
        }

        StartCoroutine(HideRoutine(targetPos));
    }

    //숨길 때 코루틴
    IEnumerator HideRoutine(Vector3 targetPos) 
    {
        int frameCount = 0;

        //20프레임 동안 실행
        while(frameCount < 20)
        {
            frameCount++;
            if(targetPos != Vector3.up * 100)
            {
                //상대방에게 실행되므로 여기서 포지션은 끌려오는 상대의 포지션임

                transform.position = Vector3.Lerp(transform.position, targetPos, 0.5f);
            }
            else if(targetPos == Vector3.up * 100)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, 0.2f);
            }
            yield return null;
        }

        gameManager.score += (int)Mathf.Pow(2, level);

        isMerge = false;
        gameObject.SetActive(false);
    }

    void levelUp()
    {
        isMerge = true;

        //2d라서 Vector2사용, 레벨업 중에는 물리속도 제거
        rb2d.velocity = Vector2.zero;
        rb2d.angularVelocity = 0;

        StartCoroutine(LevelUpRoutine());
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.tag == "Finish")
        {
            deadTime += Time.deltaTime;

            if(deadTime > 2)
            {
                spriteRenderer.color = new Color(0.9f, 0.2f, 0.2f);
            }

            if(deadTime > 5)
            {
                gameManager.GameOver();
            }
        }
    }

    IEnumerator LevelUpRoutine()
    {
        yield return new WaitForSeconds(0.2f);

        anim.SetInteger("Level", level + 1);
        EffectPlay();
        gameManager.SfxPlay(GameManager.Sfx.LevelUp);

        yield return new WaitForSeconds(0.3f);
        level++;

        gameManager.maxLevel = Mathf.Max(level, gameManager.maxLevel);
        //gameManager.maxLevel = Mathf.Min(gameManager.maxLevel, 8);

        isMerge = false;
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.tag == "Finish")
        {
            deadTime = 0;
            spriteRenderer.color = Color.white;
        }
    }

    void EffectPlay()
    {
        effect.transform.position = transform.position;
        effect.transform.localScale = transform.localScale;
        effect.Play();
    }
}
