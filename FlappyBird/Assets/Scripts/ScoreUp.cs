using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScoreUp : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Score.score += 1;
    }
}
