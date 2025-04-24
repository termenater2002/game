using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.Muse.Animate
{
    static class ColliderUtils
    {
        static SkinnedMeshRenderer[] s_SingleSkinnedMeshRenderer = new SkinnedMeshRenderer[1];

        public static void SetupColliders(MeshRenderer[] meshRenderers, int numPointsThreshold = 5)
        {
            for (var i = 0; i < meshRenderers.Length; i++)
            {
                SetupColliders(meshRenderers[i], numPointsThreshold);
            }
        }

        public static void SetupColliders(MeshRenderer meshRenderer, int numPointsThreshold = 5)
        {
            if (meshRenderer == null)
                return;

            var meshFilter = meshRenderer.GetComponent<MeshFilter>();
            if (meshFilter == null || meshFilter.sharedMesh == null)
                return;

            var mesh = meshFilter.sharedMesh;
            var vertices = mesh.vertices;

            var builder = new BoundingBoxBuilder();
            for (var vertIndex = 0; vertIndex < vertices.Length; vertIndex++)
            {
                var position = vertices[vertIndex];
                builder.AddPoint(position);
            }

            if (builder.NumPoints < numPointsThreshold)
                return;

            var collider = meshRenderer.gameObject.AddComponent<BoxCollider>();
            collider.center = builder.Center;
            collider.size = builder.Size;
        }

        public static void SetupColliders(SkinnedMeshRenderer[] skinnedMeshRenderers, float weightThreshold = 0.0f, int numPointsThreshold = 5)
        {
            if (skinnedMeshRenderers.Length == 0)
                return;

            // Check that all renderers have same number of bones
            var numBones = skinnedMeshRenderers[0].bones.Length;
            for (var i = 1; i < skinnedMeshRenderers.Length; i++)
            {
                Assert.AreEqual(numBones, skinnedMeshRenderers[i].bones.Length);
            }

            // Check that all renderers have same bone transforms
            for (var i = 0; i < numBones; i++)
            {
                var boneTransform = skinnedMeshRenderers[0].bones[i];
                for (var j = 1; j < skinnedMeshRenderers.Length; j++)
                {
                    Assert.AreEqual(boneTransform, skinnedMeshRenderers[j].bones[i]);
                }
            }

            // Common builder list
            using var tmpList = TempList<BoundingBoxBuilder>.Allocate();
            for (var i = 0; i < numBones; i++)
            {
                var builder = new BoundingBoxBuilder();
                tmpList.Add(builder);
            }

            // Build colliders
            for (var i = 0; i < skinnedMeshRenderers.Length; i++)
            {
                var skinnedMeshRenderer = skinnedMeshRenderers[i];
                BuildColliders(skinnedMeshRenderer, tmpList.List, weightThreshold);
            }

            // Create colliders
            for (var i = 0; i < numBones; i++)
            {
                var builder = tmpList.List[i];
                if (builder.NumPoints < numPointsThreshold)
                    continue;

                var boneTransform = skinnedMeshRenderers[0].bones[i];
                var collider = boneTransform.gameObject.AddComponent<BoxCollider>();
                collider.center = builder.Center;
                collider.size = builder.Size;
            }
        }

        public static void SetupColliders(this SkinnedMeshRenderer skinnedMeshRenderer, float weightThreshold = 0.0f, int numPointsThreshold = 5)
        {
            s_SingleSkinnedMeshRenderer[0] = skinnedMeshRenderer;
            SetupColliders(s_SingleSkinnedMeshRenderer, weightThreshold);
        }

        static void BuildColliders(SkinnedMeshRenderer skinnedMeshRenderer, List<BoundingBoxBuilder> builders, float weightThreshold)
        {
            using var tmpBindPoses = TempList<Matrix4x4>.Allocate();

            var mesh = skinnedMeshRenderer.sharedMesh;

            var bonesPerVertex = mesh.GetBonesPerVertex();
            if (bonesPerVertex.Length == 0)
                return;

            var boneWeights = mesh.GetAllBoneWeights();
            mesh.GetBindposes(tmpBindPoses.List);

            var vertices = mesh.vertices;

            var boneWeightIndex = 0;
            for (var vertIndex = 0; vertIndex < vertices.Length; vertIndex++)
            {
                var numberOfBonesForThisVertex = bonesPerVertex[vertIndex];
                var vertexPosition = vertices[vertIndex];

                for (var i = 0; i < numberOfBonesForThisVertex; i++)
                {
                    var currentBoneWeight = boneWeights[boneWeightIndex];

                    if (currentBoneWeight.weight < weightThreshold)
                        continue;

                    var bindPose = tmpBindPoses.List[currentBoneWeight.boneIndex];
                    var localVertexPosition = bindPose.MultiplyPoint(vertexPosition);

                    var boneBuilder = builders[currentBoneWeight.boneIndex];
                    boneBuilder.AddPoint(localVertexPosition);

                    boneWeightIndex++;
                }
            }
        }
    }
}
