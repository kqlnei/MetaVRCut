using UnityEngine;
using System.Collections.Generic;

public class SurroundingGenerator : MonoBehaviour
{
    [System.Serializable]
    public class SpawnPattern
    {
        public RotationType rotationType;
        public GameObject[] prefabs;
        public Vector3 basePosition;
        public Vector3 interval;
    }

    public enum RotationType
    {
        RightWall,
        LeftWall,
        Ceiling,
        Floor
    }

    [Header("�����ݒ�")]
    [SerializeField] private SpawnPattern[] spawnPatterns;
    [SerializeField] private int visibleSectionCount = 5;

    private Transform player;
    private Queue<GameObject>[] generatedSections;
    private float nextSpawnZ = 0f;
    private Transform sectionsParent;

    [Header("�폜�ݒ�")]
    [SerializeField] private float removalDelayMultiplier = 1.2f;
    [SerializeField] private int bufferSections = 2;

    private float sectionIntervalZ = 0f;

    private void Start()
    {
        if (spawnPatterns == null || spawnPatterns.Length == 0)
        {
            Debug.LogError("SpawnPatterns���ݒ肳��Ă��܂���B");
            enabled = false;
            return;
        }
        // patterns[0]��Z�����Ԋu���L���b�V��
        sectionIntervalZ = spawnPatterns[0].interval.z;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null)
        {
            Debug.LogError("Player�^�O���t�����I�u�W�F�N�g��������܂���B");
            enabled = false;
            return;
        }
        player = playerObj.transform;

        CreateParentObject();
        InitializeSections();
        PrewarmSections();
    }

    /// <summary>
    /// ���������Z�N�V�������i�[����e�I�u�W�F�N�g�𐶐�
    /// </summary>
    void CreateParentObject()
    {
        sectionsParent = new GameObject("GeneratedSections").transform;
    }

    /// <summary>
    /// �eSpawnPattern���ƂɃZ�N�V�������Ǘ�����Queue��������
    /// </summary>
    void InitializeSections()
    {
        generatedSections = new Queue<GameObject>[spawnPatterns.Length];
        for (int i = 0; i < spawnPatterns.Length; i++)
        {
            generatedSections[i] = new Queue<GameObject>();
        }
    }

    /// <summary>
    /// ������ԂŌ�����͈͂̃Z�N�V�������ɐ���
    /// </summary>
    void PrewarmSections()
    {
        for (int i = 0; i < visibleSectionCount; i++)
        {
            GenerateSections(nextSpawnZ);
            nextSpawnZ += sectionIntervalZ;
        }
    }

    private void Update()
    {
        float removalThreshold = visibleSectionCount * sectionIntervalZ * removalDelayMultiplier;
        if (player.position.z > nextSpawnZ - removalThreshold)
        {
            GenerateSections(nextSpawnZ);
            RemoveOldSections();
            nextSpawnZ += sectionIntervalZ;
        }
    }

    /// <summary>
    /// �w��ʒu�Ɋe�p�^�[���̃Z�N�V�����𐶐�
    /// </summary>
    /// <param name="zPosition">�����ʒu��Z���W</param>
    void GenerateSections(float zPosition)
    {
        for (int i = 0; i < spawnPatterns.Length; i++)
        {
            SpawnPattern pattern = spawnPatterns[i];
            if (pattern.prefabs == null || pattern.prefabs.Length == 0)
                continue;

            GameObject selectedPrefab = pattern.prefabs[Random.Range(0, pattern.prefabs.Length)];
            Vector3 spawnPos = CalculatePosition(pattern, zPosition);
            Quaternion rotation = GetPatternRotation(pattern.rotationType);

            GameObject section = Instantiate(selectedPrefab, spawnPos, rotation, sectionsParent);
            generatedSections[i].Enqueue(section);
        }
    }

    /// <summary>
    /// �w��̃p�^�[���Ɋ�Â��A�����ʒu���v�Z
    /// </summary>
    Vector3 CalculatePosition(SpawnPattern pattern, float z)
    {
        return new Vector3(
            pattern.basePosition.x,
            pattern.basePosition.y,
            z + pattern.basePosition.z
        );
    }

    /// <summary>
    /// �w�肳�ꂽRotationType�ɍ��킹����]��Ԃ�
    /// </summary>
    Quaternion GetPatternRotation(RotationType type)
    {
        // �����_���ȉ�]�̃I�t�Z�b�g��t�^�i0�܂���180�x�j
        int random180 = Random.Range(0, 2) * 180;
        return type switch
        {
            RotationType.RightWall => Quaternion.Euler(random180, 0, 0),
            RotationType.LeftWall => Quaternion.Euler(random180, 180, 0),
            RotationType.Ceiling => Quaternion.Euler(0, random180, 90),
            RotationType.Floor => Quaternion.Euler(0, random180, -90),
            _ => Quaternion.identity,
        };
    }

    /// <summary>
    /// �v���C���[���痣�ꂽ�Â��Z�N�V�������폜
    /// </summary>
    void RemoveOldSections()
    {
        for (int i = 0; i < spawnPatterns.Length; i++)
        {
            int keepCount = visibleSectionCount + bufferSections;
            while (generatedSections[i].Count > keepCount)
            {
                GameObject oldSection = generatedSections[i].Dequeue();
                if (oldSection != null)
                {
                    Destroy(oldSection);
                }
            }
        }
    }
}