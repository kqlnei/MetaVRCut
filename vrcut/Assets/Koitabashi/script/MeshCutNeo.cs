using System;
using System.Collections.Generic;
using UnityEngine;


public class MeshCutNeo : MonoBehaviour
{
    static Mesh _targetMesh;
    static Vector3[] _targetVertices;
    static Vector3[] _targetNormals;
    static Vector2[] _targetUVs;   //����3�͂߂�����厖�ł��ꏑ���Ȃ���10�{���炢�d���Ȃ�(for�����Ŏg������Q�Ɠn�����Ƃ�΂�)

    //���ʂ̕�������n�Er=h(n�͖@��,r�͈ʒu�x�N�g��,h��const(=_planeValue))
    static Vector3 _planeNormal;
    static float _planeValue;

    static UnsafeList<bool> _isFront_List = new UnsafeList<bool>(SIZE);
    static UnsafeList<int> _trackedArray_List = new UnsafeList<int>(SIZE);

    static bool[] _isFront;//���_���ؒf�ʂɑ΂��ĕ\�ɂ��邩���ɂ��邩
    static int[] _trackedArray;//�ؒf���Mesh�ł̐ؒf�O�̒��_�̔ԍ�

    static bool _makeCutSurface;

    static Dictionary<int, (int, int)> newVertexDic = new Dictionary<int, (int, int)>(101);


    static FragmentList fragmentList = new FragmentList();
    static RoopFragmentCollection roopCollection = new RoopFragmentCollection();


    //UnsafeList��List�̒��g�̔z�����������o���Ē��ڏ��������邽�߂Ɏ��삵���N���X. ���������ǈ��S�����Ⴂ
    const int SIZE = 200;
    static UnsafeList<Vector3> _frontVertices = new UnsafeList<Vector3>(SIZE);//�z�肳��郂�f���̒��_�����̗̈��\�ߋ󂯂Ă���
    static UnsafeList<Vector3> _backVertices = new UnsafeList<Vector3>(SIZE);
    static UnsafeList<Vector3> _frontNormals = new UnsafeList<Vector3>(SIZE);
    static UnsafeList<Vector3> _backNormals = new UnsafeList<Vector3>(SIZE);
    static UnsafeList<Vector2> _frontUVs = new UnsafeList<Vector2>(SIZE);
    static UnsafeList<Vector2> _backUVs = new UnsafeList<Vector2>(SIZE);

    static UnsafeList<UnsafeList<int>> _frontSubmeshIndices = new UnsafeList<UnsafeList<int>>(SIZE * 3);
    static UnsafeList<UnsafeList<int>> _backSubmeshIndices = new UnsafeList<UnsafeList<int>>(SIZE * 3);

    /// <summary>
    /// <para>gameObject��ؒf����2��Mesh�ɂ��ĕԂ��܂�.1�ڂ�Mesh���ؒf�ʂ̖@���ɑ΂��ĕ\��, 2�ڂ������ł�.</para>
    /// <para>���x���؂�悤�ȃI�u�W�F�N�g�ł����_���������Ȃ��悤�ɏ��������Ă���ق�, �ȒP�ȕ��̂Ȃ�ؒf�ʂ�D�����킹�邱�Ƃ��ł��܂�</para>
    /// </summary>
    /// <param name="targetMesh">�ؒf����Mesh</param>
    /// <param name="targetTransform">�ؒf����Mesh��Transform</param>
    /// <param name="planeAnchorPoint">�ؒf�ʏ�̃��[���h��ԏ�ł�1�_</param>
    /// <param name="planeNormalDirection">�ؒf�ʂ̃��[���h��ԏ�ł̖@��</param>
    /// <param name="makeCutSurface">�ؒf���Mesh��D�����킹�邩�ۂ�</param>
    /// <param name="addNewMeshIndices">�V����subMesh����邩(�ؒf�ʂɐV�����}�e���A�������蓖�Ă�ꍇ�ɂ�true, ���łɐؒf�ʂ̃}�e���A����Renderer�ɂ��Ă�ꍇ��false)</param>
    /// <returns></returns>
    public static (Mesh frontside, Mesh backside) CutMesh(Mesh targetMesh, Transform targetTransform, Vector3 planeAnchorPoint, Vector3 planeNormalDirection, bool makeCutSurface = true, bool addNewMeshIndices = false)
    {
        if (planeNormalDirection == Vector3.zero)
        {
            Debug.LogError("the normal vector magnitude is zero!");

            Mesh empty = new Mesh();
            empty.vertices = new Vector3[] { };
            return (null, null);
        }

        //������
        {
            _targetMesh = targetMesh; //Mesh���擾
            //for����_targetMesh����ĂԂ͔̂��ɏd���Ȃ�̂ł����Ŕz��Ɋi�[����for���ł͂�������n��(Mesh.vertices�Ȃǂ͎Q�Ƃł͂Ȃ��Ė���R�s�[�������̂�Ԃ��Ă���ۂ�)
            _targetVertices = _targetMesh.vertices;
            _targetNormals = _targetMesh.normals;
            _targetUVs = _targetMesh.uv;


            int verticesLength = _targetVertices.Length;
            _makeCutSurface = makeCutSurface;

            _trackedArray_List.Clear(verticesLength);//List�̃T�C�Y���m��_trackedArray_List�͂����Ŕz��̃T�C�Y�𐮂��邽�߂����Ɏg�p
            _trackedArray = _trackedArray_List.unsafe_array;//���g�̔z������蓖��
            _isFront_List.Clear(verticesLength);
            _isFront = _isFront_List.unsafe_array;
            newVertexDic.Clear();
            roopCollection.Clear();
            fragmentList.Clear();

            _frontVertices.Clear(verticesLength); //List.Clear()�Ƃقړ�������
            _frontNormals.Clear(verticesLength);
            _frontUVs.Clear(verticesLength);
            _frontSubmeshIndices.Clear(2);

            _backVertices.Clear(verticesLength);
            _backNormals.Clear(verticesLength);
            _backUVs.Clear(verticesLength);
            _backSubmeshIndices.Clear(2);

            Vector3 scale = targetTransform.localScale;//localscale�ɍ��킹��Plane�ɓ����normal�ɕ␳��������
            _planeNormal = Vector3.Scale(scale, targetTransform.transform.InverseTransformDirection(planeNormalDirection)).normalized;
        }



        //�ŏ��ɒ��_�̏�񂾂�����͂��Ă���

        Vector3 anchor = targetTransform.transform.InverseTransformPoint(planeAnchorPoint);
        _planeValue = Vector3.Dot(_planeNormal, anchor);
        {
            //UnsafeList���璆�g�̔z������o��(�z��̗v�f����verticesLength�Ȃ̂ŗv�f���𒴂����A�N�Z�X�͔������Ȃ�)
            //List.Add����������Ƒ���
            Vector3[] frontVertices_array = _frontVertices.unsafe_array;
            Vector3[] backVertices_array = _backVertices.unsafe_array;
            Vector3[] frontNormals_array = _frontNormals.unsafe_array;
            Vector3[] backNormals_array = _backNormals.unsafe_array;
            Vector2[] frontUVs_array = _frontUVs.unsafe_array;
            Vector2[] backUVs_array = _backUVs.unsafe_array;

            float pnx = _planeNormal.x;
            float pny = _planeNormal.y;
            float pnz = _planeNormal.z;

            float ancx = anchor.x;
            float ancy = anchor.y;
            float ancz = anchor.z;

            int frontCount = 0;
            int backCount = 0;
            for (int i = 0; i < _targetVertices.Length; i++)
            {
                Vector3 pos = _targetVertices[i];
                //plane�̕\���ɂ��邩�����ɂ��邩�𔻒�.(���Ԃ�\��������true)
                if (_isFront[i] = (pnx * (pos.x - ancx) + pny * (pos.y - ancy) + pnz * (pos.z - ancz)) > 0)
                {
                    //���_�������
                    frontVertices_array[frontCount] = pos;
                    frontNormals_array[frontCount] = _targetNormals[i];
                    frontUVs_array[frontCount] = _targetUVs[i];
                    //���Ƃ�Mesh��n�Ԗڂ̒��_���V����Mesh�ŉ��ԖڂɂȂ�̂����L�^
                    _trackedArray[i] = frontCount++;
                }
                else
                {
                    backVertices_array[backCount] = pos;
                    backNormals_array[backCount] = _targetNormals[i];
                    backUVs_array[backCount] = _targetUVs[i];

                    _trackedArray[i] = backCount++;
                }
            }

            //�z��ɓ��ꂽ�v�f���Ɠ�������count�������߂�
            _frontVertices.unsafe_count = frontCount;
            _frontNormals.unsafe_count = frontCount;
            _frontUVs.unsafe_count = frontCount;
            _backVertices.unsafe_count = backCount;
            _backNormals.unsafe_count = backCount;
            _backUVs.unsafe_count = backCount;



            if (frontCount == 0 || backCount == 0)//�Б��ɑS��������ꍇ�͂����ŏI��
            {
                return (null, null);
            }
        }







        //����, �O�p�|���S���̏���ǉ����Ă���
        int submeshCount = _targetMesh.subMeshCount;

        for (int sub = 0; sub < submeshCount; sub++)
        {

            int[] indices = _targetMesh.GetIndices(sub);


            //�|���S�����`�����钸�_�̔ԍ�������int�̔z�������Ă���.(submesh���Ƃɒǉ�)
            int indicesLength = indices.Length;
            _frontSubmeshIndices.AddOnlyCount();
            _frontSubmeshIndices.Top = _frontSubmeshIndices.Top?.Clear(indicesLength) ?? new UnsafeList<int>(indicesLength);
            _backSubmeshIndices.AddOnlyCount();
            _backSubmeshIndices.Top = _backSubmeshIndices.Top?.Clear(indicesLength) ?? new UnsafeList<int>(indicesLength);


            //���X�g����z��������o��
            UnsafeList<int> frontIndices = _frontSubmeshIndices[sub];
            int[] frontIndices_array = frontIndices.unsafe_array;
            int frontIndicesCount = 0;
            UnsafeList<int> backIndices = _backSubmeshIndices[sub];
            int[] backIndices_array = backIndices.unsafe_array;
            int backIndicesCount = 0;

            //�|���S���̏��͒��_3��1�Z�b�g�Ȃ̂�3��΂��Ń��[�v
            for (int i = 0; i < indices.Length; i += 3)
            {
                int p1, p2, p3;
                p1 = indices[i];
                p2 = indices[i + 1];
                p3 = indices[i + 2];


                //�\�ߌv�Z���Ă��������ʂ������Ă���(�����Ōv�Z����Ɠ������_�ɂ������ĉ���������v�Z�����邱�ƂɂȂ邩��ŏ��ɂ܂Ƃ߂Ă���Ă���(���̂ق����������Ԃ���������))
                bool side1 = _isFront[p1];
                bool side2 = _isFront[p2];
                bool side3 = _isFront[p3];



                if (side1 && side2 && side3)//3�Ƃ��\��, 3�Ƃ������̂Ƃ��͂��̂܂܏o��
                {
                    //indices�͐ؒf�O��Mesh�̒��_�ԍ��������Ă���̂�_trackedArray��ʂ����ƂŐV����Mesh�ł̔ԍ��ɕς��Ă���
                    frontIndices_array[frontIndicesCount++] = _trackedArray[p1];
                    frontIndices_array[frontIndicesCount++] = _trackedArray[p2];
                    frontIndices_array[frontIndicesCount++] = _trackedArray[p3];
                }
                else if (!side1 && !side2 && !side3)
                {
                    backIndices_array[backIndicesCount++] = _trackedArray[p1];
                    backIndices_array[backIndicesCount++] = _trackedArray[p2];
                    backIndices_array[backIndicesCount++] = _trackedArray[p3];
                }
                else  //�O�p�|���S�����`������e�_�Ŗʂɑ΂���\�����قȂ�ꍇ, �܂�ؒf�ʂƏd�Ȃ��Ă��镽�ʂ͕�������.
                {
                    Sepalate(new bool[3] { side1, side2, side3 }, new int[3] { p1, p2, p3 }, sub);
                }

            }
            //�Ō��UnsafeList�̃J�E���g��i�߂Ă���
            frontIndices.unsafe_count = frontIndicesCount;
            backIndices.unsafe_count = backIndicesCount;
        }





        fragmentList.MakeTriangle();//�ؒf���ꂽ�|���S���͂����ł��ꂼ���Mesh�ɒǉ������

        if (makeCutSurface)
        {
            if (addNewMeshIndices)
            {
                _frontSubmeshIndices.Add(new UnsafeList<int>(20));//submesh��������̂Ń��X�g�ǉ�
                _backSubmeshIndices.Add(new UnsafeList<int>(20));
            }
            roopCollection.MakeCutSurface(_frontSubmeshIndices.Count - 1, targetTransform);
        }

        //2��Mesh��V�K�ɍ���Ă��ꂼ��ɏ���ǉ����ďo��
        Mesh frontMesh = new Mesh();
        frontMesh.name = "Split Mesh front";

        //unity2019.4�ȍ~�Ȃ炱�������g��������2�����x�����Ȃ�(unity2019.2�ȑO�͑Ή����Ă��Ȃ�.2019.3�͒m��Ȃ�)
        //int fcount = _frontVertices.unsafe_count;//unity2019.4�ȍ~
        //frontMesh.SetVertices(_frontVertices.unsafe_array, 0, fcount);//unity2019.4�ȍ~
        //frontMesh.SetNormals(_frontNormals.unsafe_array, 0, fcount);//unity2019.4�ȍ~
        //frontMesh.SetUVs(0, _frontUVs.unsafe_array, 0, fcount);//unity2019.4�ȍ~
        frontMesh.vertices = _frontVertices.ToArray();//unity2019.2�ȑO
        frontMesh.normals = _frontNormals.ToArray();//unity2019.2�ȑO
        frontMesh.uv = _frontUVs.ToArray();//unity2019.2�ȑO



        frontMesh.subMeshCount = _frontSubmeshIndices.Count;
        for (int i = 0; i < _frontSubmeshIndices.Count; i++)
        {
            frontMesh.SetIndices(_frontSubmeshIndices[i].ToArray(), MeshTopology.Triangles, i, false);//unity2019.2�ȑO
            //frontMesh.SetIndices(_frontSubmeshIndices[i].unsafe_array, 0, _frontSubmeshIndices[i].unsafe_count, MeshTopology.Triangles, i, false);//unity2019.4�ȍ~
        }


        Mesh backMesh = new Mesh();
        backMesh.name = "Split Mesh back";
        //int bcount = _backVertices.unsafe_count;//unity2019.4�ȍ~
        //backMesh.SetVertices(_backVertices.unsafe_array, 0, bcount);//unity2019.4�ȍ~
        //backMesh.SetNormals(_backNormals.unsafe_array, 0, bcount);//unity2019.4�ȍ~
        //backMesh.SetUVs(0, _backUVs.unsafe_array, 0, bcount);//unity2019.4�ȍ~
        backMesh.vertices = _backVertices.ToArray();//unity2019.2�ȑO
        backMesh.normals = _backNormals.ToArray();//unity2019.2�ȑO
        backMesh.uv = _backUVs.ToArray();//unity2019.2�ȑO

        backMesh.subMeshCount = _backSubmeshIndices.Count;
        for (int i = 0; i < _backSubmeshIndices.Count; i++)
        {
            backMesh.SetIndices(_backSubmeshIndices[i].ToArray(), MeshTopology.Triangles, i, false);//unity2019.2�ȑO
            //backMesh.SetIndices(_backSubmeshIndices[i].unsafe_array, 0, _backSubmeshIndices[i].unsafe_count, MeshTopology.Triangles, i, false);//unity2019.4�ȍ~
        }



        return (frontMesh, backMesh);
    }

    /// <summary>
    /// Mesh��ؒf���܂�. 
    /// 1�ڂ�GameObject���@���̌����Ă�������ŐV����Instantiate��������, 1�ڂ�GameObject���@���Ɣ��Ε����œ��͂������̂�Ԃ��܂�
    /// </summary>
    /// <param name="targetGameObject">�ؒf�����GameObject</param>
    /// <param name="planeAnchorPoint">�ؒf���ʏ�̂ǂ���1�_(���[���h���W)</param>
    /// <param name="planeNormalDirection">�ؒf���ʂ̖@��(���[���h���W)</param>
    /// <param name="makeCutSurface">�ؒf�ʂ���邩�ǂ���</param>
    /// <param name="cutSurfaceMaterial">�ؒf�ʂɊ��蓖�Ă�}�e���A��(null�̏ꍇ�͓K���ȃ}�e���A�������蓖�Ă�)</param>
    /// <returns></returns>
    public static (GameObject copy_normalside, GameObject original_anitiNormalside) CutMesh(GameObject targetGameObject, Vector3 planeAnchorPoint, Vector3 planeNormalDirection, bool makeCutSurface = true, Material cutSurfaceMaterial = null)
    {
        if (!targetGameObject.GetComponent<MeshFilter>())
        {
            Debug.LogError("�����̃I�u�W�F�N�g�ɂ�MeshFilter���A�^�b�`����!");
            return (null, null);
        }
        else if (!targetGameObject.GetComponent<MeshRenderer>())
        {
            Debug.LogError("�����̃I�u�W�F�N�g�ɂ�Meshrenderer���A�^�b�`����!");
            return (null, null);
        }

        Mesh mesh = targetGameObject.GetComponent<MeshFilter>().mesh;
        Transform transform = targetGameObject.transform;
        bool addNewMaterial;

        MeshRenderer renderer = targetGameObject.GetComponent<MeshRenderer>();
        //material�ɃA�N�Z�X����Ƃ��̏u�Ԃ�material�̌ʂ̃C���X�^���X������ă}�e���A������(instance)�����Ă��܂��̂œǂݍ��݂�sharedMaterial�ōs��
        Material[] mats = renderer.sharedMaterials;
        if (makeCutSurface && cutSurfaceMaterial != null)
        {
            if (mats[mats.Length - 1]?.name == cutSurfaceMaterial.name)//���łɐؒf�}�e���A�����ǉ�����Ă���Ƃ��͂�����g���̂Œǉ����Ȃ�
            {
                addNewMaterial = false;
            }
            else
            {
                addNewMaterial = true;
            }
        }
        else
        {
            addNewMaterial = false;
        }

        (Mesh fragMesh, Mesh originMesh) = CutMesh(mesh, transform, planeAnchorPoint, planeNormalDirection, makeCutSurface, addNewMaterial);


        if (originMesh == null || fragMesh == null)
        {
            return (null, null);

        }
        if (addNewMaterial)
        {
            int matLength = mats.Length;
            Material[] newMats = new Material[matLength + 1];
            mats.CopyTo(newMats, 0);
            newMats[matLength] = cutSurfaceMaterial;


            renderer.sharedMaterials = newMats;
        }


        targetGameObject.GetComponent<MeshFilter>().mesh = originMesh;

        //GameObject fragment = new GameObject("Fragment", typeof(MeshFilter), typeof(MeshRenderer));
        Transform originTransform = targetGameObject.transform;
        GameObject fragment = Instantiate(targetGameObject, originTransform.position, originTransform.rotation, originTransform.parent);
        fragment.transform.parent = null;
        fragment.GetComponent<MeshFilter>().mesh = fragMesh;
        fragment.GetComponent<MeshRenderer>().sharedMaterials = targetGameObject.GetComponent<MeshRenderer>().sharedMaterials;

        foreach (Transform child in fragment.transform)
        {
            GameObject.Destroy(child.gameObject);
        }


        if (targetGameObject.GetComponent<MeshCollider>())
        {
            //���_��1�_�ɏd�Ȃ��Ă���ꍇ�ɂ̓G���[���o��̂�, ���������ꍇ��mesh.RecalculateBounds�̂��Ƃ�mesh.bounds.size.magnitude<0.00001�Ȃǂŏ����������đΏ����Ă�������
            targetGameObject.GetComponent<MeshCollider>().sharedMesh = originMesh;
            fragment.GetComponent<MeshCollider>().sharedMesh = fragMesh;
        }



        return (fragment, targetGameObject);

    }



    //�|���S����ؒf
    //�|���S���͐ؒf�ʂ̕\���Ɨ����ɕ��������.
    //���̂Ƃ��O�p�|���S����\�ʂ��猩��, �Ȃ����ؒf�ʂ̕\���ɂ��钸�_�����ɗ���悤�Ɍ���,
    //�O�p�`�̍����̕ӂ��`������_��f0,b0, �E���ɂ���ӂ����_��f1,b1�Ƃ���.(f�͕\���ɂ���_��b�͗���)(���_��3�Ȃ̂Ŕ�肪���݂���)
    //�����Ń|���S���̌��������߂Ă����ƌ�X�ƂĂ��֗�
    //�ȍ~�����ɂ�����̂�0,�E���ɂ�����̂�1�����Ĉ���(��O�͂��邩��)
    //(�Ђ���Ƃ���Ǝ��ۂ̌����͋t��������Ȃ�����vertexIndices�Ɠ����܂����ŏo�͂��Ă�̂ŋt�ł����͂Ȃ�.�����ł�3�_�����v���ŕ���ł���Ɖ��肵�đS����)
    private static void Sepalate(bool[] sides, int[] vertexIndices, int submesh)
    {
        int f0 = 0, f1 = 0, b0 = 0, b1 = 0; //���_��index�ԍ����i�[����̂Ɏg�p
        bool twoPointsInFrontSide;//�ǂ��炪�ɒ��_��2���邩

        //�|���S���̌����𑵂���
        if (sides[0])
        {
            if (sides[1])
            {
                f0 = vertexIndices[1];
                f1 = vertexIndices[0];
                b0 = b1 = vertexIndices[2];
                twoPointsInFrontSide = true;
            }
            else
            {
                if (sides[2])
                {
                    f0 = vertexIndices[0];
                    f1 = vertexIndices[2];
                    b0 = b1 = vertexIndices[1];
                    twoPointsInFrontSide = true;
                }
                else
                {
                    f0 = f1 = vertexIndices[0];
                    b0 = vertexIndices[1];
                    b1 = vertexIndices[2];
                    twoPointsInFrontSide = false;
                }
            }
        }
        else
        {
            if (sides[1])
            {
                if (sides[2])
                {
                    f0 = vertexIndices[2];
                    f1 = vertexIndices[1];
                    b0 = b1 = vertexIndices[0];
                    twoPointsInFrontSide = true;
                }
                else
                {
                    f0 = f1 = vertexIndices[1];
                    b0 = vertexIndices[2];
                    b1 = vertexIndices[0];
                    twoPointsInFrontSide = false;
                }
            }
            else
            {
                f0 = f1 = vertexIndices[2];
                b0 = vertexIndices[0];
                b1 = vertexIndices[1];
                twoPointsInFrontSide = false;
            }
        }

        //�ؒf�O�̃|���S���̒��_�̍��W���擾(���̂���2�͂��Ԃ��Ă�)
        Vector3 frontPoint0, frontPoint1, backPoint0, backPoint1;
        if (twoPointsInFrontSide)
        {
            frontPoint0 = _targetVertices[f0];
            frontPoint1 = _targetVertices[f1];
            backPoint0 = backPoint1 = _targetVertices[b0];
        }
        else
        {
            frontPoint0 = frontPoint1 = _targetVertices[f0];
            backPoint0 = _targetVertices[b0];
            backPoint1 = _targetVertices[b1];
        }

        //�x�N�g��[backPoint0 - frontPoint0]�����{������ؒf���ʂɓ��B���邩�͈ȉ��̎��ŕ\�����
        //���ʂ̎�: dot(r,n)=A ,A�͒萔,n�͖@��, 
        //����    r =frontPoint0+k*(backPoint0 - frontPoint0), (0 �� k �� 1)
        //�����, �V�����ł��钸�_��2�̒��_�����Ή��ɓ������Ăł���̂����Ӗ����Ă���
        float dividingParameter0 = (_planeValue - Vector3.Dot(_planeNormal, frontPoint0)) / (Vector3.Dot(_planeNormal, backPoint0 - frontPoint0));
        //Lerp�Őؒf�ɂ���Ă��܂��V�������_�̍��W�𐶐�
        Vector3 newVertexPos0 = Vector3.Lerp(frontPoint0, backPoint0, dividingParameter0);


        float dividingParameter1 = (_planeValue - Vector3.Dot(_planeNormal, frontPoint1)) / (Vector3.Dot(_planeNormal, backPoint1 - frontPoint1));
        Vector3 newVertexPos1 = Vector3.Lerp(frontPoint1, backPoint1, dividingParameter1);

        //�V�������_�̐���, �����ł�Normal��UV�͌v�Z�����ォ��v�Z�ł���悤�ɒ��_��index(_trackedArray[f0], _trackedArray[b0],)�Ɠ����_�̏��(dividingParameter0)�������Ă���
        NewVertex vertex0 = fragmentList.MakeVertex(_trackedArray[f0], _trackedArray[b0], dividingParameter0, newVertexPos0);
        NewVertex vertex1 = fragmentList.MakeVertex(_trackedArray[f1], _trackedArray[b1], dividingParameter1, newVertexPos1);


        //�ؒf�łł����(���ꂪ�����|���S���͌������邱�ƂŒ��_���̑�����}������)
        Vector3 cutLine = (newVertexPos1 - newVertexPos0).normalized;
        int KEY_CUTLINE = MakeIntFromVector3_ErrorCut(cutLine);//Vector3���Ə������d�����Ȃ̂�int�ɂ��Ă���, ���łɊۂߌ덷��؂藎�Ƃ�

        //�ؒf�����܂�Fragment�N���X
        Fragment fragment = fragmentList.MakeFragment(vertex0, vertex1, twoPointsInFrontSide, KEY_CUTLINE, submesh);
        //List�ɒǉ�����List�̒��œ��ꕽ�ʂ�Fragment�͌����Ƃ�����
        fragmentList.Add(fragment, KEY_CUTLINE, submesh);

    }

    class RoopFragment
    {
        public RoopFragment next; //�E�ׂ̂��
        public Vector3 rightPosition;//�E���̍��W(�����̍��W�͍��ׂ̂�������Ă�)
        public RoopFragment(Vector3 _rightPosition)
        {
            next = null;
            rightPosition = _rightPosition;
        }
        public RoopFragment SetNew(Vector3 _rightPosition)
        {
            next = null;
            rightPosition = _rightPosition;
            return this;
        }
    }
    class RooP
    {
        public RoopFragment start, end; //start�����[, end���E�[
        //public int KEY_LEFT, KEY_RIGHT;
        public Vector3 startPos, endPos;
        public int count;
        public Vector3 center;
        public RooP(RoopFragment _left, RoopFragment _right, Vector3 _startPos, Vector3 _endPos, Vector3 rightPos)
        {
            start = _left;
            end = _right;
            startPos = _startPos;
            endPos = _endPos;
            count = 1;
            center = rightPos;
        }
    }

    public class RoopFragmentCollection
    {
        const int listSize = 31;
        List<RooP>[] leftLists = new List<RooP>[listSize];//���胊�X�g�z��(����vector3�Ȃ瓯��List�ɓ���)
        List<RooP>[] rightLists = new List<RooP>[listSize];//�E�胊�X�g�z��
        UnsafeList<RoopFragment> roopFragments = new UnsafeList<RoopFragment>(100);

        public RoopFragmentCollection()
        {
            for (int i = 0; i < listSize; i++)
            {
                leftLists[i] = new List<RooP>(5);
                rightLists[i] = new List<RooP>(5);
            }
        }

        public void Add(Vector3 left, Vector3 right)
        {
            int KEY_LEFT = MakeIntFromVector3(left); //Vector3����int��
            int KEY_RIGHT = MakeIntFromVector3(right);


            RoopFragment target;
            roopFragments.AddOnlyCount();
            roopFragments.Top = roopFragments.Top?.SetNew(right) ?? new RoopFragment(right);
            target = roopFragments.Top;

            //Dictionary�Ƃɂ�����
            int leftIndex = KEY_LEFT % listSize;//�����̍���̍��W���i�[����Ă���index 
            int rightIndex = KEY_RIGHT % listSize;//�E��

            //�����̍���Ƃ������̂͑���̉E��Ȃ̂ŉE��List�̒����玩���̍���index�̏ꏊ��T��
            var rList = rightLists[leftIndex];
            RooP roop1 = null;
            bool find1 = false;
            int rcount = rList.Count;
            for (int i = 0; i < rcount; i++)
            {
                RooP temp = rList[i];
                if (temp.endPos == left)
                {
                    //roop�̉E���target�̉E��ɕς���(roop�͍��[�ƉE�[�̏�񂾂��������Ă���)
                    temp.end.next = target;
                    temp.end = target;
                    temp.endPos = right;
                    roop1 = temp;
                    //roop�����X�g����O��(���ƂŉE��List�̎����̉E��index�̏ꏊ�Ɉڂ�����)
                    rList.RemoveAt(i);
                    find1 = true;
                    break;
                }
            }
            var lList = leftLists[rightIndex];
            RooP roop2 = null;
            bool find2 = false;
            int lcount = lList.Count;
            for (int j = 0; j < lcount; j++)
            {
                roop2 = lList[j];
                if (right == roop2.startPos)
                {
                    if (roop1 == roop2)
                    {
                        //print("make roop");
                        roop1.count++;
                        roop1.center += right;
                        return;
                    }//roop1==roop2�̂Ƃ�, roop�����������̂�return

                    target.next = roop2.start;
                    roop2.start = target;
                    roop2.startPos = left;
                    lList.RemoveAt(j);
                    find2 = true;
                    break;
                }
            }

            if (find1)
            {
                if (find2)//2��roop�����������Ƃ�
                {
                    roop1.end = roop2.end;
                    roop1.endPos = roop2.endPos;
                    roop1.count += roop2.count + 1;
                    roop1.center += roop2.center + right;
                    int key = MakeIntFromVector3(roop2.endPos) % listSize;
                    for (int i = 0; i < rightLists[key].Count; i++)
                    {
                        if (roop2 == rightLists[key][i])
                        {
                            rightLists[key][i] = roop1;
                        }
                    }

                }
                else//�����̍����roop�̉E�肪���������Ƃ�, �E�胊�X�g�̎����̉E��index��roop������
                {
                    roop1.count++;
                    roop1.center += right;
                    rightLists[rightIndex].Add(roop1);
                }
            }
            else
            {
                if (find2)
                {
                    roop2.count++;
                    roop2.center += right;
                    leftLists[leftIndex].Add(roop2);
                }
                else//�ǂ��ɂ��������Ȃ������Ƃ�, roop���쐬, �ǉ�
                {
                    RooP newRoop = new RooP(target, target, left, right, right);
                    rightLists[rightIndex].Add(newRoop);
                    leftLists[leftIndex].Add(newRoop);
                }
            }
        }


        public void MakeCutSurface(int submesh, Transform targetTransform)
        {
            Vector3 scale = targetTransform.localScale;
            Vector3 world_Up = Vector3.Scale(scale, targetTransform.InverseTransformDirection(Vector3.up)).normalized;//���[���h���W�̏�������I�u�W�F�N�g���W�ɕϊ�
            Vector3 world_Right = Vector3.Scale(scale, targetTransform.InverseTransformDirection(Vector3.right)).normalized;//���[���h���W�̉E�������I�u�W�F�N�g���W�ɕϊ�


            Vector3 uVector, vVector; //�I�u�W�F�N�g��ԏ�ł�UV��U��,V��
            uVector = Vector3.Cross(world_Up, _planeNormal); //U���͐ؒf�ʂ̖@����Y���Ƃ̊O��
            uVector = (uVector.sqrMagnitude != 0) ? uVector.normalized : world_Right; //�ؒf�ʂ̖@����Z�������̂Ƃ���uVector���[���x�N�g���ɂȂ�̂ŏꍇ����

            vVector = Vector3.Cross(_planeNormal, uVector).normalized; //V����U���Ɛؒf���ʂ̃m�[�}���Ƃ̊O��
            if (Vector3.Dot(vVector, world_Up) < 0) { vVector *= -1; } //v���̕��������[���h���W������ɑ�����.

            float u_min, u_max, u_range;
            float v_min, v_max, v_range;


            foreach (List<RooP> list in leftLists)
            {
                foreach (RooP roop in list)
                {


                    {

                        u_min = u_max = Vector3.Dot(uVector, roop.startPos);
                        v_min = v_max = Vector3.Dot(vVector, roop.startPos);
                        RoopFragment fragment = roop.start;


                        int count = 0;
                        do
                        {
                            float u_value = Vector3.Dot(uVector, fragment.rightPosition);
                            u_min = Mathf.Min(u_min, u_value);
                            u_max = Mathf.Max(u_max, u_value);

                            float v_value = Vector3.Dot(vVector, fragment.rightPosition);
                            v_min = Mathf.Min(v_min, v_value);
                            v_max = Mathf.Max(v_max, v_value);


                            if (count > 1000) //�����������Ƃ��̂��߂̈��S���u(while�����킢)
                            {
                                Debug.LogError("Something is wrong?");
                                break;
                            }
                            count++;

                        }
                        while ((fragment = fragment.next) != null);

                        u_range = u_max - u_min;
                        v_range = v_max - v_min;

                    }





                    //roopFragment��next�����ǂ��Ă������Ƃ�roop������ł���

                    MakeVertex(roop.center / roop.count, out int center_f, out int center_b);

                    RoopFragment nowFragment = roop.start;

                    MakeVertex(nowFragment.rightPosition, out int first_f, out int first_b);
                    int previous_f = first_f;
                    int previous_b = first_b;

                    while (nowFragment.next != null)
                    {
                        nowFragment = nowFragment.next;


                        MakeVertex(nowFragment.rightPosition, out int index_f, out int index_b);

                        _frontSubmeshIndices[submesh].Add(center_f);
                        _frontSubmeshIndices[submesh].Add(index_f);
                        _frontSubmeshIndices[submesh].Add(previous_f);

                        _backSubmeshIndices[submesh].Add(center_b);
                        _backSubmeshIndices[submesh].Add(previous_b);
                        _backSubmeshIndices[submesh].Add(index_b);

                        previous_f = index_f;
                        previous_b = index_b;


                    }
                    _frontSubmeshIndices[submesh].Add(center_f);
                    _frontSubmeshIndices[submesh].Add(first_f);
                    _frontSubmeshIndices[submesh].Add(previous_f);

                    _backSubmeshIndices[submesh].Add(center_b);
                    _backSubmeshIndices[submesh].Add(previous_b);
                    _backSubmeshIndices[submesh].Add(first_b);
                }
            }

            void MakeVertex(Vector3 vertexPos, out int findex, out int bindex)
            {
                findex = _frontVertices.Count;
                bindex = _backVertices.Count;
                Vector2 uv;
                { //position��UV�ɕϊ�
                    float uValue = Vector3.Dot(uVector, vertexPos);
                    float normalizedU = (uValue - u_min) / u_range;
                    float vValue = Vector3.Dot(vVector, vertexPos);
                    float normalizedV = (vValue - v_min) / v_range;

                    uv = new Vector2(normalizedU, normalizedV);

                }
                _frontVertices.Add(vertexPos);
                _frontNormals.Add(-_planeNormal);
                _frontUVs.Add(uv);

                _backVertices.Add(vertexPos);
                _backNormals.Add(_planeNormal);
                _backUVs.Add(new Vector2(1 - uv.x, uv.y));//UV�����E���]����

            }
        }

        public void Clear()
        {
            for (int i = 0; i < listSize; i++)
            {
                leftLists[i].Clear();
                rightLists[i].Clear();
            }
        }
    }

    public class Fragment
    {
        public NewVertex vertex0, vertex1;
        public int KEY_CUTLINE;
        public int submesh;//submesh�ԍ�(�ǂ̃}�e���A���𓖂Ă邩)
        public Point firstPoint_f, lastPoint_f, firstPoint_b, lastPoint_b;
        public int count_f, count_b;

        public Fragment(NewVertex _vertex0, NewVertex _vertex1, bool _twoPointsInFrontSide, int _KEY_CUTLINE, int _submesh)
        {
            SetNew(_vertex0, _vertex1, _twoPointsInFrontSide, _KEY_CUTLINE, _submesh);
        }

        public Fragment SetNew(NewVertex _vertex0, NewVertex _vertex1, bool _twoPointsInFrontSide, int _KEY_CUTLINE, int _submesh)
        {
            vertex0 = _vertex0;
            vertex1 = _vertex1;
            KEY_CUTLINE = _KEY_CUTLINE;
            submesh = _submesh;



            if (_twoPointsInFrontSide)
            {
                firstPoint_f = fragmentList.MakePoint(_vertex0.frontsideindex_of_frontMesh);
                lastPoint_f = fragmentList.MakePoint(_vertex1.frontsideindex_of_frontMesh);
                firstPoint_f.next = lastPoint_f;
                firstPoint_b = fragmentList.MakePoint(vertex0.backsideindex_of_backMash);
                lastPoint_b = firstPoint_b;
                count_f = 2;
                count_b = 1;
            }
            else
            {
                firstPoint_f = fragmentList.MakePoint(_vertex0.frontsideindex_of_frontMesh);
                lastPoint_f = firstPoint_f;
                firstPoint_b = fragmentList.MakePoint(vertex0.backsideindex_of_backMash);
                lastPoint_b = fragmentList.MakePoint(vertex1.backsideindex_of_backMash);
                firstPoint_b.next = lastPoint_b;
                count_f = 1;
                count_b = 2;
            }
            return this;
        }

        public void AddTriangle()
        {
            (int findex0, int bindex0) = vertex0.GetIndex(); //Vertex�̒��ŐV�����������ꂽ���_��o�^���Ă��̔ԍ�������Ԃ��Ă���
            (int findex1, int bindex1) = vertex1.GetIndex();

            Point point = firstPoint_f;
            int preIndex = point.index;

            int count = count_f;
            int halfcount = count_f / 2;
            for (int i = 0; i < halfcount; i++)
            {
                point = point.next;
                int index = point.index;
                _frontSubmeshIndices[submesh].Add(index);
                _frontSubmeshIndices[submesh].Add(preIndex);
                _frontSubmeshIndices[submesh].Add(findex0);
                preIndex = index;
            }
            _frontSubmeshIndices[submesh].Add(preIndex);
            _frontSubmeshIndices[submesh].Add(findex0);
            _frontSubmeshIndices[submesh].Add(findex1);
            int elseCount = count_f - halfcount - 1;
            for (int i = 0; i < elseCount; i++)
            {
                point = point.next;
                int index = point.index;
                _frontSubmeshIndices[submesh].Add(index);
                _frontSubmeshIndices[submesh].Add(preIndex);
                _frontSubmeshIndices[submesh].Add(findex1);
                preIndex = index;
            }


            point = firstPoint_b;
            preIndex = point.index;
            count = count_b;
            halfcount = count_b / 2;

            for (int i = 0; i < halfcount; i++)
            {
                point = point.next;
                int index = point.index;
                _backSubmeshIndices[submesh].Add(index);
                _backSubmeshIndices[submesh].Add(bindex0);
                _backSubmeshIndices[submesh].Add(preIndex);
                preIndex = index;
            }
            _backSubmeshIndices[submesh].Add(preIndex);
            _backSubmeshIndices[submesh].Add(bindex1);
            _backSubmeshIndices[submesh].Add(bindex0);
            elseCount = count_b - halfcount - 1;
            for (int i = 0; i < elseCount; i++)
            {
                point = point.next;
                int index = point.index;
                _backSubmeshIndices[submesh].Add(index);
                _backSubmeshIndices[submesh].Add(bindex1);
                _backSubmeshIndices[submesh].Add(preIndex);
                preIndex = index;
            }

            if (_makeCutSurface)
            {
                roopCollection.Add(vertex0.position, vertex1.position);//�ؒf���ʂ��`�����鏀��
            }

        }
    }

    //�V�������_��Normal��UV�͍Ō�ɐ�������̂�, ���Ƃ��Ƃ��钸�_���ǂ̔�ō����邩��dividingParameter�������Ă���
    public class NewVertex
    {
        public int frontsideindex_of_frontMesh; //frontVertices,frontNormals,frontUVs�ł̒��_�̔ԍ�(frontsideindex_of_frontMesh��backsideindex_of_backMash�łł���ӂ̊ԂɐV�������_���ł���)
        public int backsideindex_of_backMash;
        public float dividingParameter;//�V�������_��(frontsideindex_of_frontMesh��backsideindex_of_backMash�łł���ӂɑ΂���)�����_
        public int KEY_VERTEX;
        public Vector3 position;

        public NewVertex(int front, int back, float parameter, Vector3 vertexPosition)
        {
            frontsideindex_of_frontMesh = front;
            backsideindex_of_backMash = back;
            KEY_VERTEX = (front << 16) | back;
            dividingParameter = parameter;
            position = vertexPosition;
        }

        public NewVertex SetNew(int front, int back, float parameter, Vector3 vertexPosition)
        {
            frontsideindex_of_frontMesh = front;
            backsideindex_of_backMash = back;
            KEY_VERTEX = (front << 16) | back;
            dividingParameter = parameter;
            position = vertexPosition;
            return this;
        }

        public (int findex, int bindex) GetIndex()
        {
            //�@����UV�̏��͂����Ő�������
            Vector3 frontNormal, backNormal;
            Vector2 frontUV, backUV;

            frontNormal = _frontNormals[frontsideindex_of_frontMesh];
            frontUV = _frontUVs[frontsideindex_of_frontMesh];

            backNormal = _backNormals[backsideindex_of_backMash];
            backUV = _backUVs[backsideindex_of_backMash];



            Vector3 newNormal = Vector3.Lerp(frontNormal, backNormal, dividingParameter);
            Vector2 newUV = Vector2.Lerp(frontUV, backUV, dividingParameter);

            int findex, bindex;
            (int, int) trackNumPair;
            //����2�̓_�̊Ԃɐ�������钸�_��1�ɂ܂Ƃ߂����̂�Dictionary���g��
            if (newVertexDic.TryGetValue(KEY_VERTEX, out trackNumPair))
            {
                findex = trackNumPair.Item1;//�V�������_���\����Mesh�ŉ��Ԗڂ�
                bindex = trackNumPair.Item2;
            }
            else
            {

                findex = _frontVertices.Count;
                _frontVertices.Add(position);
                _frontNormals.Add(newNormal);
                _frontUVs.Add(newUV);

                bindex = _backVertices.Count;
                _backVertices.Add(position);
                _backNormals.Add(newNormal);
                _backUVs.Add(newUV);

                newVertexDic.Add(KEY_VERTEX, (findex, bindex));

            }

            return (findex, bindex);
        }
    }

    public class Point
    {
        public Point next;
        public int index;
        public Point(int _index)
        {
            index = _index;
            next = null;
        }
        public Point SetNew(int _index)
        {
            index = _index;
            next = null;
            return this;
        }
    }


    public class FragmentList
    {
        const int listSize = 71;
        List<Fragment>[] fragmentLists = new List<Fragment>[listSize];//������List�ɕ��U�����邱�ƂŌ������x���グ�Ă���(Dictionary���Q�l�ɂ���)
        public FragmentList()
        {
            for (int i = 0; i < listSize; i++)
            {
                fragmentLists[i] = new List<Fragment>(10);
            }
        }
        public void Add(Fragment fragment, int KEY_CUTLINE, int submesh)
        {

            //��{�I�Ȏd�g�݂�Dictionary�Ɠ���
            int listIndex = KEY_CUTLINE % listSize;
            List<Fragment> flist = fragmentLists[listIndex];//�����ؒf�ӂ�����Fragment�͓����ꏊ�Ɋi�[�����(�ʂ�Fragment�������Ă��Ȃ��킯�ł͂Ȃ�)
            bool connect = false;
            //�i�[����Ă���Fragment���炭����������T��
            for (int i = flist.Count - 1; i >= 0; i--)
            {
                Fragment compareFragment = flist[i];
                if (fragment.KEY_CUTLINE == compareFragment.KEY_CUTLINE)//�����ؒf�ӂ��������f
                {
                    Fragment left, right;
                    if (fragment.vertex0.KEY_VERTEX == compareFragment.vertex1.KEY_VERTEX)//fragment��compareFragment�ɉE�����炭�����ꍇ
                    {
                        right = fragment;
                        left = compareFragment;
                    }
                    else if (fragment.vertex1.KEY_VERTEX == compareFragment.vertex0.KEY_VERTEX)//�������炭�����ꍇ
                    {
                        left = fragment;
                        right = compareFragment;
                    }
                    else
                    {
                        continue;//�ǂ����ł��Ȃ��Ƃ��͎��̃��[�v��
                    }

                    //Point�N���X�̂Ȃ����킹. 
                    //firstPoint.next��null�Ƃ������Ƃ͒��_��1���������Ă��Ȃ�. 
                    //�܂����̒��_��left��lastPoint�Ƃ��Ԃ��Ă���̂Œ��_�������邱�Ƃ͂Ȃ�
                    //(left.lastPoint_f��right.lastPoint_f�͓����_���������ʂ̃C���X�^���X�Ȃ̂�next��null�̂Ƃ��ɓ���ւ���ƃ��[�v���r�؂�Ă��܂�)
                    if ((left.lastPoint_f.next = right.firstPoint_f.next) != null)
                    {
                        left.lastPoint_f = right.lastPoint_f;
                        left.count_f += right.count_f - 1;
                    }
                    if ((left.lastPoint_b.next = right.firstPoint_b.next) != null)
                    {
                        left.lastPoint_b = right.lastPoint_b;
                        left.count_b += right.count_b - 1;
                    }


                    //�������s��
                    //Fragment�����L���Ȃ�悤�ɒ��_����ς���
                    left.vertex1 = right.vertex1;
                    right.vertex0 = left.vertex0;

                    //connect��true�ɂȂ��Ă���Ƃ������Ƃ�2��Fragment�̂������ɐV��������͂܂���3��1�ɂȂ����Ƃ�������
                    //connect==true�̂Ƃ�, right��left��List�ɂ��łɓo�^����Ă��Ȃ̂łǂ������������Ă��
                    if (connect)
                    {
                        flist.Remove(right);

                        break;
                    }

                    flist[i] = left;
                    fragment = left;
                    connect = true;
                }
            }

            if (!connect)
            {
                flist.Add(fragment);
            }
        }


        public void MakeTriangle()
        {
            int sum = 0;
            foreach (List<Fragment> list in fragmentLists)
            {
                foreach (Fragment f in list)
                {
                    f.AddTriangle();
                    sum++;
                }
            }
        }

        public void Clear()
        {
            foreach (List<Fragment> f in fragmentLists)
            {
                f.Clear();

            }
            vertexRepository.Clear(200);
            fragmentRepository.Clear(100);
        }

        UnsafeList<NewVertex> vertexRepository = new UnsafeList<NewVertex>(200);
        UnsafeList<Fragment> fragmentRepository = new UnsafeList<Fragment>(100);
        UnsafeList<Point> pointRepository = new UnsafeList<Point>(400);
        public NewVertex MakeVertex(int front, int back, float parameter, Vector3 vertexPosition)
        {
            vertexRepository.AddOnlyCount();
            vertexRepository.Top = vertexRepository.Top?.SetNew(front, back, parameter, vertexPosition) ?? new NewVertex(front, back, parameter, vertexPosition);
            return vertexRepository.Top;
        }

        public Fragment MakeFragment(NewVertex _vertex0, NewVertex _vertex1, bool _twoPointsInFrontSide, int _KEY_CUTLINE, int _submesh)
        {
            fragmentRepository.AddOnlyCount();
            fragmentRepository.Top = fragmentRepository.Top?.SetNew(_vertex0, _vertex1, _twoPointsInFrontSide, _KEY_CUTLINE, _submesh) ?? new Fragment(_vertex0, _vertex1, _twoPointsInFrontSide, _KEY_CUTLINE, _submesh);
            return fragmentRepository.Top;
        }

        public Point MakePoint(int index)
        {
            pointRepository.AddOnlyCount();
            pointRepository.Top = pointRepository.Top?.SetNew(index) ?? new Point(index);
            return pointRepository.Top;
        }

    }




    const int filter = 0x000003FF;
    const int amp = 1 << 18;
    public static int MakeIntFromVector3(Vector3 vec)
    {

        int cutLineX = ((int)(vec.x * amp) & filter) << 20;
        int cutLineY = ((int)(vec.y * amp) & filter) << 10;
        int cutLineZ = ((int)(vec.z * amp) & filter);

        return cutLineX | cutLineY | cutLineZ;
    }

    const int amp2 = 1 << 10;//�ۂߌ덷�𗎂Ƃ����߂ɂ���߂̔{�����������Ă���
    public static int MakeIntFromVector3_ErrorCut(Vector3 vec)
    {
        int cutLineX = ((int)(vec.x * amp2) & filter) << 20;
        int cutLineY = ((int)(vec.y * amp2) & filter) << 10;
        int cutLineZ = ((int)(vec.z * amp2) & filter);

        return cutLineX | cutLineY | cutLineZ;
    }
}