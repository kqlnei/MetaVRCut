using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement; // �V�[���J�ڂɕK�v

public class SceneTransitionOnCollision : MonoBehaviour
{
    public float delayTime = 1f; // ���b��ɃV�[���J��
    public string targetSceneName = "EnemyWave"; // �J�ڐ�̃V�[����

    private void OnTriggerEnter(Collider other)
    {
        // "katana"�^�O���t�����I�u�W�F�N�g���ڐG�����ꍇ
        if (other.CompareTag("katana"))
        {
            // �x�����ăV�[���J�ڂ��s��
            StartCoroutine(DelayedSceneTransition());
            Debug.Log("ggggggggggggggg");
        }
    }

    // �x�����ăV�[���J�ڂ���R���[�`��
    private IEnumerator DelayedSceneTransition()
    {
        yield return new WaitForSeconds(delayTime); // delayTime�b�ҋ@
        SceneManager.LoadScene(targetSceneName); // �V�[���J��
    }
}
