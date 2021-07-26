using System.IO;
using UnityEditor;
using UnityEngine;

namespace UnityAvatar.Editor
{
    public class FbxUtility
    {
        private static readonly string _fbxPath = "Assets/Arts/Fbx";
        private static readonly string _meshPath = "Assets/Resources/Mesh";
        private static readonly string _texturePath = "Assets/Resources/Texture";
        private static readonly string _avatarPath = "Assets/Resources/Avatar";

        [MenuItem("UnityAvatarProject/Helper/Process Fbx", false, 1)]
        public static void Process()
        {
            EnsureCreateDirectory(_avatarPath);

            foreach (var file in Directory.GetFiles($"{_fbxPath}/Role"))
            {
                var extention = Path.GetExtension(file);
                if (extention != ".FBX" && extention != ".fbx")
                {
                    continue;
                }
                var fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(file);
                var fbxObject = Object.Instantiate(fbxAsset);
                var smrs = fbxObject.GetComponentsInChildren<SkinnedMeshRenderer>();

                var prefabDirectory = $"{_meshPath}/Role/{fbxAsset.name}";
                var texDirectory = $"{_texturePath}/Role/{fbxAsset.name}";

                if (smrs.Length > 0)
                {
                    EnsureCreateDirectory(prefabDirectory);
                    EnsureCreateDirectory(texDirectory);
                }

                foreach (var smr in smrs)
                {
                    var mesh = Object.Instantiate(smr.sharedMesh) as Mesh;
                    mesh.name = smr.sharedMesh.name;
                    mesh.UploadMeshData(false);

                    var tex = smr.sharedMaterial.mainTexture as Texture2D;
                    SaveMesh(prefabDirectory, texDirectory, mesh, tex);
                }

                var avatar = AvatarBuilder.BuildGenericAvatar(fbxObject, fbxObject.name);
                if (avatar)
                {
                    var avatarPath = $"{_avatarPath}/{fbxAsset.name}.asset";
                    if (File.Exists(avatarPath))
                    {
                        AssetDatabase.DeleteAsset(avatarPath);
                        AssetDatabase.Refresh();
                    }
                    AssetDatabase.CreateAsset(avatar, avatarPath);
                    AssetDatabase.SaveAssets();
                }

                Object.DestroyImmediate(fbxObject);

                AssetDatabase.Refresh();
            }

            foreach (var file in Directory.GetFiles($"{_fbxPath}/Weapon"))
            {
                var extention = Path.GetExtension(file);
                if (extention != ".FBX" && extention != ".fbx")
                {
                    continue;
                }
                var fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(file);
                var fbxObject = Object.Instantiate(fbxAsset);
                var meshFilter = fbxObject.GetComponent<MeshFilter>();
                var meshRenderer = fbxObject.GetComponent<MeshRenderer>();

                var prefabDirectory = $"{_meshPath}/Weapon/{fbxAsset.name}";
                var texDirectory = $"{_texturePath}/Weapon/{fbxAsset.name}";
                EnsureCreateDirectory(prefabDirectory);
                EnsureCreateDirectory(texDirectory);

                var mesh = Object.Instantiate(meshFilter.sharedMesh) as Mesh;
                mesh.name = meshFilter.sharedMesh.name;
                mesh.UploadMeshData(false);

                var tex = meshRenderer.sharedMaterial.mainTexture as Texture2D;
                SaveMesh(prefabDirectory, texDirectory, mesh, tex);

                Object.DestroyImmediate(fbxObject);

                AssetDatabase.Refresh();
            }
        }

        private static void SaveMesh(string meshDir, string texDir, Mesh mesh, Texture2D tex)
        {
            CleanMesh(mesh);

            var meshName = mesh.name;
            var meshPath = $"{meshDir}/{meshName}.asset";
            AssetDatabase.CreateAsset(mesh, meshPath);
            AssetDatabase.SaveAssets();

            if (tex != null)
            {
                var srcTexPath = AssetDatabase.GetAssetPath(tex);
                var destTexPath = $"{texDir}/{meshName}.tga";
                AssetDatabase.CopyAsset(srcTexPath, destTexPath);
                AssetDatabase.SaveAssets();

                var texImporter = (TextureImporter)AssetImporter.GetAtPath(destTexPath);
                if (texImporter)
                {
                    texImporter.maxTextureSize = 128;
                    texImporter.wrapMode = TextureWrapMode.Clamp;
                    texImporter.mipmapEnabled = false;
                    AssetDatabase.ImportAsset(destTexPath, ImportAssetOptions.ForceUpdate);
                }
            }
        }

        private static void EnsureCreateDirectory(string dir)
        {
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, true);
            }
            Directory.CreateDirectory(dir);
        }

        private static void CleanMesh(Mesh mesh)
        {
            mesh.uv2 = null;
            mesh.uv3 = null;
            mesh.uv4 = null;
            mesh.colors = null;
            mesh.colors32 = null;
            mesh.tangents = null;
        }
    }
}