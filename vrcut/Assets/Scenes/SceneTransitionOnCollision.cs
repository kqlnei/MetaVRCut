using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement; // シーン遷移に必要

public class SceneTransitionOnCollision : MonoBehaviour
{
    public float delayTime = 1f; // 数秒後にシーン遷移
    public string targetSceneName = "EnemyWave"; // 遷移先のシーン名

    private void OnTriggerEnter(Collider other)
    {
        // "katana"タグが付いたオブジェクトが接触した場合
        if (other.CompareTag("katana"))
        {
            // 遅延してシーン遷移を行う
            StartCoroutine(DelayedSceneTransition());
            Debug.Log("ggggggggggggggg");
        }
    }

    // 遅延してシーン遷移するコルーチン
    private IEnumerator DelayedSceneTransition()
    {
        yield return new WaitForSeconds(delayTime); // delayTime秒待機
        SceneManager.LoadScene(targetSceneName); // シーン遷移
    }
}
