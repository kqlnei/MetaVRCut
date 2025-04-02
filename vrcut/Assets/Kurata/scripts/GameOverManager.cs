using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameOverManager : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private int maxHits = 3;
    [SerializeField] private List<string> damageTags = new List<string> { "Damage" };
    [SerializeField] private List<string> damageLayers = new List<string> { "Bullet" };

    [Header("Instant Death Settings")]
    [SerializeField] private string instantDeathTag = "LastBoss";
    [SerializeField] private AudioClip instantDeathSound;

    [Header("Visual Feedback")]
    [SerializeField] private Image damageOverlay;
    [SerializeField] private float fadeDuration = 2.0f; // �I�[�o�[���C�̃t�F�[�h�A�E�g�ɂ����鎞��

    [Header("Audio Settings")]
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private AudioSource audioSource;

    [Header("UI Settings")]
    [SerializeField] private Text hitsText;

    private int currentHits = 0;
    private List<int> damageLayerIDs = new List<int>();
    private Coroutine fadeCoroutine;

    private void Start()
    {
        InitializeDamageLayers();
        InitializeDamageOverlay();
        UpdateHitsUI();
    }

    /// <summary>
    /// damageLayers���X�g�Ɋ�Â��A�e���C���[��ID���擾���ď�����
    /// </summary>
    private void InitializeDamageLayers()
    {
        damageLayerIDs.Clear();
        foreach (string layerName in damageLayers)
        {
            int layerID = LayerMask.NameToLayer(layerName);
            if (layerID == -1)
            {
                Debug.LogError($"���C���[ '{layerName}' ��������܂���I");
            }
            else
            {
                damageLayerIDs.Add(layerID);
            }
        }
    }

    /// <summary>
    /// �_���[�W�I�[�o�[���C�̏�����ԁi���S�����j��ݒ�
    /// </summary>
    private void InitializeDamageOverlay()
    {
        if (damageOverlay != null)
        {
            Color color = damageOverlay.color;
            color.a = 0f;
            damageOverlay.color = color;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == null) return;

        // �����_���[�W����
        if (other.CompareTag(instantDeathTag))
        {
            Debug.Log("[GameOverManager] �����_���[�W�����I");
            currentHits = maxHits;
            PlayInstantDeathSound();
            UpdateHitsUI();
            UpdateDamageOverlay();
            GameOver();
            return;
        }

        // �_���[�W�^�O�܂��̓_���[�W���C���[�ɊY������ꍇ�̏���
        bool isDamage = damageTags.Contains(other.tag) || damageLayerIDs.Contains(other.gameObject.layer);
        if (isDamage)
        {
            currentHits++;
            Debug.Log($"[GameOverManager] ���݂̃q�b�g��: {currentHits}");
            PlayDamageSound();
            UpdateHitsUI();
            UpdateDamageOverlay();

            if (currentHits >= maxHits)
            {
                GameOver();
            }
        }
    }

    /// <summary>
    /// �_���[�W�I�[�o�[���C�̃A���t�@�l�𑦎�50%�ɐݒ肵�A�t�F�[�h�A�E�g���J�n
    /// </summary>
    private void UpdateDamageOverlay()
    {
        if (damageOverlay == null)
        {
            Debug.LogWarning("Damage Overlay���ݒ肳��Ă��܂���B");
            return;
        }

        // �����̃t�F�[�h�R���[�`��������Β�~
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        // �I�[�o�[���C�̃A���t�@��50%�ɐݒ�
        Color color = damageOverlay.color;
        color.a = 0.5f;
        damageOverlay.color = color;

        // �t�F�[�h�A�E�g�R���[�`���J�n
        fadeCoroutine = StartCoroutine(FadeOverlay());
    }

    /// <summary>
    /// �_���[�W�I�[�o�[���C���w�莞�Ԃ����ăt�F�[�h�A�E�g
    /// </summary>
    private IEnumerator FadeOverlay()
    {
        float timer = 0f;
        Color initialColor = damageOverlay.color;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(0.5f, 0f, timer / fadeDuration);
            Color newColor = initialColor;
            newColor.a = alpha;
            damageOverlay.color = newColor;
            yield return null;
        }

        // �ŏI�I�Ɋ��S�ɓ����ɐݒ�
        Color finalColor = damageOverlay.color;
        finalColor.a = 0f;
        damageOverlay.color = finalColor;
    }

    /// <summary>
    /// �_���[�W���󂯂��ۂ̃T�E���h
    /// </summary>
    private void PlayDamageSound()
    {
        if (audioSource != null && damageSound != null)
        {
            audioSource.PlayOneShot(damageSound);
        }
        else
        {
            Debug.LogWarning("AudioSource�܂��̓_���[�W�����ݒ肳��Ă��܂���B");
        }
    }

    /// <summary>
    /// �����_���[�W���̃T�E���h
    /// </summary>
    private void PlayInstantDeathSound()
    {
        if (audioSource != null && instantDeathSound != null)
        {
            audioSource.PlayOneShot(instantDeathSound);
        }
        else
        {
            Debug.LogWarning("AudioSource�܂��͑��������ݒ肳��Ă��܂���B");
        }
    }

    /// <summary>
    /// UI��Ƀq�b�g���̍X�V�𔽉f
    /// </summary>
    private void UpdateHitsUI()
    {
        if (hitsText != null)
        {
            hitsText.text = $"Hits: {currentHits} / {maxHits}";
        }
        else
        {
            Debug.LogWarning("�q�b�g���\���pUI Text���ݒ肳��Ă��܂���B");
        }
    }

    /// <summary>
    /// �I�[�o�[�V�[���֑J��
    /// </summary>
    private void GameOver()
    {
        Debug.Log("[GameOverManager] �Q�[���I�[�o�[����");
        SceneManager.LoadScene("Over");
    }
}