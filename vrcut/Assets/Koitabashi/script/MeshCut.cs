/*************************************************************
 * 
 * ���̃X�N���v�g�͍ŏI�I�Ɏg�p����Ă��܂���B
 * 
 *************************************************************/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BLINDED_AM_ME
{
    public class MeshCut
    {
        public class MeshCutSide
        {
            public List<Vector3> vertices = new List<Vector3>();
            public List<Vector3> normals = new List<Vector3>();
            public List<Vector2> uvs = new List<Vector2>();
            public List<int> triangles = new List<int>();
            public List<List<int>> subIndices = new List<List<int>>();

            public void ClearAll()
            {
                vertices.Clear();
                normals.Clear();
                uvs.Clear();
                triangles.Clear();
                subIndices.Clear();
            }

            /// <summary>
            /// �g���C�A���O���Ƃ���3���_��ǉ�
            /// �� ���_���͌��̃��b�V������R�s�[����
            /// </summary>
            /// <param name="p1">���_1</param>
            /// <param name="p2">���_2</param>
            /// <param name="p3">���_3</param>
            /// <param name="submesh">�Ώۂ̃T�u���V��</param>
            public void AddTriangle(int p1, int p2, int p3, int submesh)
            {
                // triangle index order goes 1,2,3,4....

                // ���_�z��̃J�E���g�B�����ǉ�����Ă������߁A�x�[�X�ƂȂ�index���`����B
                // �� AddTriangle���Ă΂�邽�тɒ��_���͑����Ă����B
                int base_index = vertices.Count;

                // �ΏۃT�u���b�V���̃C���f�b�N�X�ɒǉ����Ă���
                subIndices[submesh].Add(base_index + 0);
                subIndices[submesh].Add(base_index + 1);
                subIndices[submesh].Add(base_index + 2);

                // �O�p�`�S�̒��_��ݒ�
                triangles.Add(base_index + 0);
                triangles.Add(base_index + 1);
                triangles.Add(base_index + 2);

                // �ΏۃI�u�W�F�N�g�̒��_�z�񂩂璸�_�����擾���ݒ肷��
                // �ivictim_mesh��static�����o�Ȃ񂾂��ǂ����񂾂낤���E�E�j
                vertices.Add(victim_mesh.vertices[p1]);
                vertices.Add(victim_mesh.vertices[p2]);
                vertices.Add(victim_mesh.vertices[p3]);

                // ���l�ɁA�ΏۃI�u�W�F�N�g�̖@���z�񂩂�@�����擾���ݒ肷��
                normals.Add(victim_mesh.normals[p1]);
                normals.Add(victim_mesh.normals[p2]);
                normals.Add(victim_mesh.normals[p3]);

                // ���l�ɁAUV���B
                uvs.Add(victim_mesh.uv[p1]);
                uvs.Add(victim_mesh.uv[p2]);
                uvs.Add(victim_mesh.uv[p3]);
            }

            /// <summary>
            /// �g���C�A���O����ǉ�����
            /// �� �I�[�o�[���[�h���Ă��鑼���\�b�h�Ƃ͈قȂ�A�����̒l�Œ��_�i�|���S���j��ǉ�����
            /// </summary>
            /// <param name="points3">�g���C�A���O�����`������3���_</param>
            /// <param name="normals3">3���_�̖@��</param>
            /// <param name="uvs3">3���_��UV</param>
            /// <param name="faceNormal">�|���S���̖@��</param>
            /// <param name="submesh">�T�u���b�V��ID</param>
            public void AddTriangle(Vector3[] points3, Vector3[] normals3, Vector2[] uvs3, Vector3 faceNormal, int submesh)
            {
                // ������3���_����@�����v�Z
                Vector3 calculated_normal = Vector3.Cross((points3[1] - points3[0]).normalized, (points3[2] - points3[0]).normalized);

                int p1 = 0;
                int p2 = 1;
                int p3 = 2;

                // �����Ŏw�肳�ꂽ�@���Ƌt�������ꍇ�̓C���f�b�N�X�̏��Ԃ��t���ɂ���i�܂�ʂ𗠕Ԃ��j
                if (Vector3.Dot(calculated_normal, faceNormal) < 0)
                {
                    p1 = 2;
                    p2 = 1;
                    p3 = 0;
                }

                int base_index = vertices.Count;

                subIndices[submesh].Add(base_index + 0);
                subIndices[submesh].Add(base_index + 1);
                subIndices[submesh].Add(base_index + 2);

                triangles.Add(base_index + 0);
                triangles.Add(base_index + 1);
                triangles.Add(base_index + 2);

                vertices.Add(points3[p1]);
                vertices.Add(points3[p2]);
                vertices.Add(points3[p3]);

                normals.Add(normals3[p1]);
                normals.Add(normals3[p2]);
                normals.Add(normals3[p3]);

                uvs.Add(uvs3[p1]);
                uvs.Add(uvs3[p2]);
                uvs.Add(uvs3[p3]);
            }

        }

        private static MeshCutSide left_side = new MeshCutSide();
        private static MeshCutSide right_side = new MeshCutSide();

        private static Plane blade;
        private static Mesh victim_mesh;

        // capping stuff
        private static List<Vector3> new_vertices = new List<Vector3>();

        /// <summary>
        /// Cut the specified victim, blade_plane and capMaterial.
        /// �i�w�肳�ꂽ�uvictim�v���J�b�g����B�u���[�h�i���ʁj�ƃ}�e���A������ؒf�����s����j
        /// </summary>
        /// <param name="victim">Victim.</param>
        /// <param name="blade_plane">Blade plane.</param>
        /// <param name="capMaterial">Cap material.</param>
        public static GameObject[] Cut(GameObject victim, Vector3 anchorPoint, Vector3 normalDirection, Material capMaterial)
        {
            // set the blade relative to victim
            // victim���瑊�ΓI�ȕ��ʁi�u���[�h�j���Z�b�g
            // ��̓I�ɂ́A�ΏۃI�u�W�F�N�g�̃��[�J�����W�ł̕��ʂ̖@���ƈʒu���畽�ʂ𐶐�����
            blade = new Plane(
                victim.transform.InverseTransformDirection(-normalDirection),
                victim.transform.InverseTransformPoint(anchorPoint)
            );

            // get the victims mesh
            // �Ώۂ̃��b�V�����擾
            victim_mesh = victim.GetComponent<MeshFilter>().mesh;

            // reset values
            // �V�������_�S
            new_vertices.Clear();

            // ���ʂ�荶�̒��_�S�iMeshCutSide�j
            left_side.ClearAll();

            //���ʂ��E�̒��_�S�iMeshCutSide�j
            right_side.ClearAll();

            // �����ł́u3�v�̓g���C�A���O���H
            bool[] sides = new bool[3];
            int[] indices;
            int p1, p2, p3;

            // go throught the submeshes
            // �T�u���b�V���̐��������[�v
            for (int sub = 0; sub < victim_mesh.subMeshCount; sub++)
            {
                // �T�u���b�V���̃C���f�b�N�X�����擾
                indices = victim_mesh.GetIndices(sub);

                // List<List<int>>�^�̃��X�g�B�T�u���b�V������̃C���f�b�N�X���X�g
                left_side.subIndices.Add(new List<int>());  // ��
                right_side.subIndices.Add(new List<int>()); // �E

                // �T�u���b�V���̃C���f�b�N�X�������[�v
                for (int i = 0; i < indices.Length; i += 3)
                {
                    // p1 - p3�̃C���f�b�N�X���擾�B�܂�g���C�A���O��
                    p1 = indices[i + 0];
                    p2 = indices[i + 1];
                    p3 = indices[i + 2];

                    // ���ꂼ��]�����̃��b�V���̒��_���A�`���Œ�`���ꂽ���ʂ̍��E�ǂ���ɂ��邩��]���B
                    // `GetSide` ���\�b�h�ɂ��bool�𓾂�B
                    sides[0] = blade.GetSide(victim_mesh.vertices[p1]);
                    sides[1] = blade.GetSide(victim_mesh.vertices[p2]);
                    sides[2] = blade.GetSide(victim_mesh.vertices[p3]);

                    // whole triangle
                    // ���_�O�ƒ��_�P����ђ��_�Q���ǂ�����������ɂ���ꍇ�̓J�b�g���Ȃ�
                    if (sides[0] == sides[1] && sides[0] == sides[2])
                    {
                        if (sides[0])
                        { // left side
                          // GetSide���\�b�h�Ń|�W�e�B�u�itrue�j�̏ꍇ�͍����ɂ���
                            left_side.AddTriangle(p1, p2, p3, sub);
                        }
                        else
                        {
                            right_side.AddTriangle(p1, p2, p3, sub);
                        }
                    }
                    else
                    { // cut the triangle
                      // �����ł͂Ȃ��A�ǂ��炩�̓_�����ʂ̔��Α��ɂ���ꍇ�̓J�b�g�����s����
                        Cut_this_Face(sub, sides, p1, p2, p3);
                    }
                }
            }

            // �ݒ肳��Ă���}�e���A���z����擾
            Material[] mats = victim.GetComponent<MeshRenderer>().sharedMaterials;

            // �擾�����}�e���A���z��̍Ō�̃}�e���A�����A�J�b�g�ʂ̃}�e���A���łȂ��ꍇ
            if (mats[mats.Length - 1].name != capMaterial.name)
            { // add cap indices
                // �J�b�g�ʗp�̃C���f�b�N�X�z���ǉ��H
                left_side.subIndices.Add(new List<int>());
                right_side.subIndices.Add(new List<int>());

                // �J�b�g�ʕ����₵���}�e���A���z�������
                Material[] newMats = new Material[mats.Length + 1];

                // �����̂��̂�V�����z��ɃR�s�[
                mats.CopyTo(newMats, 0);

                // �V�����}�e���A���z��̍Ō�ɁA�J�b�g�ʗp�}�e���A����ǉ�
                newMats[mats.Length] = capMaterial;

                // ���������}�e���A�����X�g���Đݒ�
                mats = newMats;
            }

            // cap the opennings
            // �J�b�g�J�n
            Capping();


            // Left Mesh
            // �����̃��b�V���𐶐�
            // MeshCutSide�N���X�̃����o����e�l���R�s�[
            Mesh left_HalfMesh = new Mesh();
            left_HalfMesh.name = "Split Mesh Left";
            left_HalfMesh.vertices = left_side.vertices.ToArray();
            left_HalfMesh.triangles = left_side.triangles.ToArray();
            left_HalfMesh.normals = left_side.normals.ToArray();
            left_HalfMesh.uv = left_side.uvs.ToArray();

            left_HalfMesh.subMeshCount = left_side.subIndices.Count;
            for (int i = 0; i < left_side.subIndices.Count; i++)
            {
                left_HalfMesh.SetIndices(left_side.subIndices[i].ToArray(), MeshTopology.Triangles, i);
            }


            // Right Mesh
            // �E���̃��b�V�������l�ɐ���
            Mesh right_HalfMesh = new Mesh();
            right_HalfMesh.name = "Split Mesh Right";
            right_HalfMesh.vertices = right_side.vertices.ToArray();
            right_HalfMesh.triangles = right_side.triangles.ToArray();
            right_HalfMesh.normals = right_side.normals.ToArray();
            right_HalfMesh.uv = right_side.uvs.ToArray();

            right_HalfMesh.subMeshCount = right_side.subIndices.Count;
            for (int i = 0; i < right_side.subIndices.Count; i++)
            {
                right_HalfMesh.SetIndices(right_side.subIndices[i].ToArray(), MeshTopology.Triangles, i);
            }


            // assign the game objects

            // ���̃I�u�W�F�N�g�������̃I�u�W�F�N�g��
            victim.name = "left side";
            victim.GetComponent<MeshFilter>().mesh = left_HalfMesh;


            // �E���̃I�u�W�F�N�g�͐V�K�쐬
            GameObject leftSideObj = victim;

            GameObject rightSideObj = new GameObject("right side", typeof(MeshFilter), typeof(MeshRenderer));
            rightSideObj.transform.position = victim.transform.position;
            rightSideObj.transform.rotation = victim.transform.rotation;
            rightSideObj.GetComponent<MeshFilter>().mesh = right_HalfMesh;

            // assign mats
            // �V�K���������}�e���A�����X�g�����ꂼ��̃I�u�W�F�N�g�ɓK�p����
            leftSideObj.GetComponent<MeshRenderer>().materials = mats;
            rightSideObj.GetComponent<MeshRenderer>().materials = mats;

            // ���E��GameObject�̔z���Ԃ�
            return new GameObject[] { leftSideObj, rightSideObj };
        }

        /// <summary>
        /// �J�b�g�����s����B�������A���ۂ̃��b�V���̑���ł͂Ȃ��A�����܂Œ��_�̐U�蕪���A���O�����Ƃ��Ă̎��s
        /// </summary>
        /// <param name="submesh">�T�u���b�V���̃C���f�b�N�X</param>
        /// <param name="sides">�]������3���_�̍��E���</param>
        /// <param name="index1">���_1</param>
        /// <param name="index2">���_2</param>
        /// <param name="index3">���_3</param>
        static void Cut_this_Face(int submesh, bool[] sides, int index1, int index2, int index3)
        {
            // ���E���ꂼ��̏���ێ����邽�߂̔z��S
            Vector3[] leftPoints = new Vector3[2];
            Vector3[] leftNormals = new Vector3[2];
            Vector2[] leftUvs = new Vector2[2];
            Vector3[] rightPoints = new Vector3[2];
            Vector3[] rightNormals = new Vector3[2];
            Vector2[] rightUvs = new Vector2[2];

            bool didset_left = false;
            bool didset_right = false;

            // 3���_���J��Ԃ�
            // �������e�Ƃ��ẮA���E�𔻒肵�āA���E�̔z���3���_��U�蕪���鏈�����s���Ă���
            int p = index1;
            for (int side = 0; side < 3; side++)
            {
                switch (side)
                {
                    case 0:
                        p = index1;
                        break;
                    case 1:
                        p = index2;
                        break;
                    case 2:
                        p = index3;
                        break;
                }

                // sides[side]��true�A�܂荶���̏ꍇ
                if (sides[side])
                {
                    // ���łɍ����̒��_���ݒ肳��Ă��邩�i3���_�����E�ɐU�蕪�����邽�߁A�K�����E�ǂ��炩��2�̒��_�������ƂɂȂ�j
                    if (!didset_left)
                    {
                        didset_left = true;

                        // ������0,1�Ƃ��ɓ����l�ɂ��Ă���̂́A����������
                        // leftPoints[0,1]�̒l���g���ĕ����_�����߂鏈�������Ă��邽�߁B
                        // �܂�A�A�N�Z�X�����\��������

                        // ���_�̐ݒ�
                        leftPoints[0] = victim_mesh.vertices[p];
                        leftPoints[1] = leftPoints[0];

                        // UV�̐ݒ�
                        leftUvs[0] = victim_mesh.uv[p];
                        leftUvs[1] = leftUvs[0];

                        // �@���̐ݒ�
                        leftNormals[0] = victim_mesh.normals[p];
                        leftNormals[1] = leftNormals[0];
                    }
                    else
                    {
                        // 2���_�ڂ̏ꍇ��2�Ԗڂɒ��ڒ��_����ݒ肷��
                        leftPoints[1] = victim_mesh.vertices[p];
                        leftUvs[1] = victim_mesh.uv[p];
                        leftNormals[1] = victim_mesh.normals[p];
                    }
                }
                else
                {
                    // ���Ɠ��l�̑�����E�ɂ��s��
                    if (!didset_right)
                    {
                        didset_right = true;

                        rightPoints[0] = victim_mesh.vertices[p];
                        rightPoints[1] = rightPoints[0];
                        rightUvs[0] = victim_mesh.uv[p];
                        rightUvs[1] = rightUvs[0];
                        rightNormals[0] = victim_mesh.normals[p];
                        rightNormals[1] = rightNormals[0];
                    }
                    else
                    {
                        rightPoints[1] = victim_mesh.vertices[p];
                        rightUvs[1] = victim_mesh.uv[p];
                        rightNormals[1] = victim_mesh.normals[p];
                    }
                }
            }

            // �������ꂽ�_�̔䗦�v�Z�̂��߂̋���
            float normalizedDistance = 0f;

            // ����
            float distance = 0f;


            // ---------------------------
            // �����̏���

            // ��`�����ʂƌ�������_��T���B
            // �܂�A���ʂɂ���ĕ��������_��T���B
            // ���̓_���N�_�ɁA�E�̓_�Ɍ��������C���΂��A���̕����_��T��B
            blade.Raycast(new Ray(leftPoints[0], (rightPoints[0] - leftPoints[0]).normalized), out distance);

            // �������������_���A���_�Ԃ̋����Ŋ��邱�ƂŁA�����_�̍��E�̊������Z�o����
            normalizedDistance = distance / (rightPoints[0] - leftPoints[0]).magnitude;

            // �J�b�g��̐V���_�ɑ΂��鏈���B�t���O�����g�V�F�[�_�ł̕⊮�Ɠ������A���������ʒu�ɉ����ēK�؂ɕ⊮�����l��ݒ肷��
            Vector3 newVertex1 = Vector3.Lerp(leftPoints[0], rightPoints[0], normalizedDistance);
            Vector2 newUv1 = Vector2.Lerp(leftUvs[0], rightUvs[0], normalizedDistance);
            Vector3 newNormal1 = Vector3.Lerp(leftNormals[0], rightNormals[0], normalizedDistance);

            // �V���_�S�ɐV�������_��ǉ�
            new_vertices.Add(newVertex1);


            // ---------------------------
            // �E���̏���

            blade.Raycast(new Ray(leftPoints[1], (rightPoints[1] - leftPoints[1]).normalized), out distance);

            normalizedDistance = distance / (rightPoints[1] - leftPoints[1]).magnitude;
            Vector3 newVertex2 = Vector3.Lerp(leftPoints[1], rightPoints[1], normalizedDistance);
            Vector2 newUv2 = Vector2.Lerp(leftUvs[1], rightUvs[1], normalizedDistance);
            Vector3 newNormal2 = Vector3.Lerp(leftNormals[1], rightNormals[1], normalizedDistance);

            // �V���_�S�ɐV�������_��ǉ�
            new_vertices.Add(newVertex2);


            // �v�Z���ꂽ�V�������_���g���āA�V�g���C�A���O�������E�Ƃ��ɒǉ�����
            // memo: �ǂ���������Ă��A���E�ǂ��炩��1�̎O�p�`�ɂȂ�C�����邯�ǁA�k�ގO�p�`�I�Ȋ����łƂɂ���2���ǉ����Ă��銴�����낤���H
            left_side.AddTriangle(
                new Vector3[] { leftPoints[0], newVertex1, newVertex2 },
                new Vector3[] { leftNormals[0], newNormal1, newNormal2 },
                new Vector2[] { leftUvs[0], newUv1, newUv2 },
                newNormal1,
                submesh
            );

            left_side.AddTriangle(
                new Vector3[] { leftPoints[0], leftPoints[1], newVertex2 },
                new Vector3[] { leftNormals[0], leftNormals[1], newNormal2 },
                new Vector2[] { leftUvs[0], leftUvs[1], newUv2 },
                newNormal2,
                submesh
            );

            right_side.AddTriangle(
                new Vector3[] { rightPoints[0], newVertex1, newVertex2 },
                new Vector3[] { rightNormals[0], newNormal1, newNormal2 },
                new Vector2[] { rightUvs[0], newUv1, newUv2 },
                newNormal1,
                submesh
            );

            right_side.AddTriangle(
                new Vector3[] { rightPoints[0], rightPoints[1], newVertex2 },
                new Vector3[] { rightNormals[0], rightNormals[1], newNormal2 },
                new Vector2[] { rightUvs[0], rightUvs[1], newUv2 },
                newNormal2,
                submesh
            );
        }

        private static List<Vector3> capVertTracker = new List<Vector3>();
        private static List<Vector3> capVertpolygon = new List<Vector3>();

        /// <summary>
        /// �J�b�g�����s
        /// </summary>
        static void Capping()
        {
            // �J�b�g�p���_�ǐՃ��X�g
            // ��̓I�ɂ͐V���_�S���ɑ΂��钲�����s���B���̉ߒ��Œ����ς݂��}�[�N����ړI�ŗ��p����B
            capVertTracker.Clear();

            // �V���������������_���������[�v���遁�S�V���_�ɑ΂��ă|���S�����`�����邽�ߒ������s��
            // ��̓I�ɂ́A�J�b�g�ʂ��\������|���S�����`�����邽�߁A�J�b�g���ɏd���������_��ԗ����āu�ʁv���`�����钸�_�𒲍�����
            for (int i = 0; i < new_vertices.Count; i++)
            {
                // �Ώے��_�����łɒ����ς݂̃}�[�N����āi�ǐՔz��Ɋ܂܂�āj������X�L�b�v
                if (capVertTracker.Contains(new_vertices[i]))
                {
                    continue;
                }

                // �J�b�g�p�|���S���z����N���A
                capVertpolygon.Clear();

                // �������_�Ǝ��̒��_���|���S���z��ɕێ�����
                capVertpolygon.Add(new_vertices[i + 0]);
                capVertpolygon.Add(new_vertices[i + 1]);

                // �ǐՔz��Ɏ��g�Ǝ��̒��_��ǉ�����i�����ς݂̃}�[�N������j
                capVertTracker.Add(new_vertices[i + 0]);
                capVertTracker.Add(new_vertices[i + 1]);

                // �d�����_���Ȃ��Ȃ�܂Ń��[�v����������
                bool isDone = false;
                while (!isDone)
                {
                    isDone = true;

                    // �V���_�S�����[�v���A�u�ʁv���\������v���ƂȂ钸�_�����ׂĒ��o����B���o���I���܂Ń��[�v���J��Ԃ�
                    // 2���_���Ƃɒ������s�����߁A���[�v��2�P�ʂł����߂�
                    for (int k = 0; k < new_vertices.Count; k += 2)
                    { // go through the pairs
                        // �y�A�ƂȂ钸�_��T��
                        // �����ł̃y�A�Ƃ́A�����g���C�A���O�����琶�������V���_�̃y�A�B
                        // �g���C�A���O������͕K��2���_����������邽�߁A�����T���B
                        // �܂��A�S�|���S���ɑ΂��ĕ����_�𐶐����Ă��邽�߁A�قڕK���A�܂����������ʒu�ɑ��݂���A�ʃg���C�A���O���̕������_�����݂���͂��ł���B
                        if (new_vertices[k] == capVertpolygon[capVertpolygon.Count - 1] && !capVertTracker.Contains(new_vertices[k + 1]))
                        {   // if so add the other
                            // �y�A�̒��_�����������炻����|���S���z��ɒǉ����A
                            // �����σ}�[�N�����āA���̃��[�v�����ɉ�
                            isDone = false;
                            capVertpolygon.Add(new_vertices[k + 1]);
                            capVertTracker.Add(new_vertices[k + 1]);
                        }
                        else if (new_vertices[k + 1] == capVertpolygon[capVertpolygon.Count - 1] && !capVertTracker.Contains(new_vertices[k]))
                        {   // if so add the other
                            isDone = false;
                            capVertpolygon.Add(new_vertices[k]);
                            capVertTracker.Add(new_vertices[k]);
                        }
                    }
                }

                // �����������_�S�����ɁA�|���S�����`������
                FillCap(capVertpolygon);
            }
        }

        /// <summary>
        /// �J�b�g�ʂ𖄂߂�H
        /// </summary>
        /// <param name="vertices">�|���S�����`�����钸�_���X�g</param>
        static void FillCap(List<Vector3> vertices)
        {
            // center of the cap
            // �J�b�g���ʂ̒��S�_���v�Z����
            Vector3 center = Vector3.zero;

            // �����œn���ꂽ���_�ʒu�����ׂč��v����
            foreach (Vector3 point in vertices)
            {
                center += point;
            }

            // ����𒸓_���̍��v�Ŋ���A���S�Ƃ���
            center = center / vertices.Count;

            // you need an axis based on the cap
            // �J�b�g���ʂ��x�[�X�ɂ���upward
            Vector3 upward = Vector3.zero;

            // 90 degree turn
            // �J�b�g���ʂ̖@���𗘗p���āA�u��v���������߂�
            // ��̓I�ɂ́A���ʂ̍�������Ƃ��ė��p����
            upward.x = blade.normal.y;
            upward.y = -blade.normal.x;
            upward.z = blade.normal.z;

            // �@���Ɓu������v����A�������Z�o
            Vector3 left = Vector3.Cross(blade.normal, upward);

            Vector3 displacement = Vector3.zero;
            Vector3 newUV1 = Vector3.zero;
            Vector3 newUV2 = Vector3.zero;

            // �����ŗ^����ꂽ���_�����[�v����
            for (int i = 0; i < vertices.Count; i++)
            {
                // �v�Z�ŋ��߂����S�_����A�e���_�ւ̕����x�N�g��
                displacement = vertices[i] - center;

                // �V�K��������|���S����UV���W�����߂�B
                // displacement�����S����̃x�N�g���̂��߁AUV�I�Ȓ��S�ł���0.5���x�[�X�ɁA���ς��g����UV�̍ŏI�I�Ȉʒu�𓾂�
                newUV1 = Vector3.zero;
                newUV1.x = 0.5f + Vector3.Dot(displacement, left);
                newUV1.y = 0.5f + Vector3.Dot(displacement, upward);
                newUV1.z = 0.5f + Vector3.Dot(displacement, blade.normal);

                // ���̒��_�B�������A�Ō�̒��_�̎��͍ŏ��̒��_�𗘗p���邽�߁A�኱�g���b�L�[�Ȏw����@�����Ă���i% vertices.Count�j
                displacement = vertices[(i + 1) % vertices.Count] - center;

                newUV2 = Vector3.zero;
                newUV2.x = 0.5f + Vector3.Dot(displacement, left);
                newUV2.y = 0.5f + Vector3.Dot(displacement, upward);
                newUV2.z = 0.5f + Vector3.Dot(displacement, blade.normal);

                // uvs.Add(new Vector2(relativePosition.x, relativePosition.y));
                // normals.Add(blade.normal);

                // �����̃|���S���Ƃ��āA���߂�UV�𗘗p���ăg���C�A���O����ǉ�
                left_side.AddTriangle(
                    new Vector3[]{
                        vertices[i],
                        vertices[(i + 1) % vertices.Count],
                        center
                    },
                    new Vector3[]{
                        -blade.normal,
                        -blade.normal,
                        -blade.normal
                    },
                    new Vector2[]{
                        newUV1,
                        newUV2,
                        new Vector2(0.5f, 0.5f)
                    },
                    -blade.normal,
                    left_side.subIndices.Count - 1 // �J�b�g�ʁB�Ō�̃T�u���b�V���Ƃ��ăg���C�A���O����ǉ�
                );

                // �E���̃g���C�A���O���B��{�͍����Ɠ��������A�@�������t�����B
                right_side.AddTriangle(
                    new Vector3[]{
                        vertices[i],
                        vertices[(i + 1) % vertices.Count],
                        center
                    },
                    new Vector3[]{
                        blade.normal,
                        blade.normal,
                        blade.normal
                    },
                    new Vector2[]{
                        newUV1,
                        newUV2,
                        new Vector2(0.5f, 0.5f)
                    },
                    blade.normal,
                    right_side.subIndices.Count - 1 // �J�b�g�ʁB�Ō�̃T�u���b�V���Ƃ��ăg���C�A���O����ǉ�
                );
            }
        }
    }
}
