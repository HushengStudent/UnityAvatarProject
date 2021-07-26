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
        private static readonly string _prefabPath = "Assets/Resources/Prefab";

        [MenuItem("UnityAvatarProject/Helper/Process Fbx", false, 1)]
        public static void Process()
        {
            //EnsureCreateDirectory(_avatarPath);
            {
                var meshDirectory = $"{_meshPath}/Role";
                var texDirectory = $"{_texturePath}/Role";
                var prefabDirectory = $"{_prefabPath}/Role";

                EnsureCreateDirectory(meshDirectory);
                EnsureCreateDirectory(texDirectory);
                EnsureCreateDirectory(prefabDirectory);

                foreach (var file in Directory.GetFiles($"{_fbxPath}/Role"))
                {
                    var extention = Path.GetExtension(file);
                    if (extention != ".FBX" && extention != ".fbx")
                    {
                        continue;
                    }
                    var fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(file);
                    var fbxObject = Object.Instantiate(fbxAsset);
                    fbxObject.name = fbxAsset.name;

                    var smr = fbxObject.GetComponentsInChildren<SkinnedMeshRenderer>()[0];

                    var mesh = Object.Instantiate(smr.sharedMesh) as Mesh;
                    mesh.name = smr.sharedMesh.name;
                    mesh.UploadMeshData(false);

                    var tex = smr.sharedMaterial.mainTexture as Texture2D;
                    SaveMesh(meshDirectory, texDirectory, mesh, tex);

                    smr.sharedMesh = mesh;
                    smr.sharedMaterial = null;

                    PrefabUtility.SaveAsPrefabAsset(fbxObject, $"{prefabDirectory}/{fbxObject.name}.prefab");

                    /*
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
                    */
                    Object.DestroyImmediate(fbxObject);

                    AssetDatabase.Refresh();

                }
            }

            {
                var meshDirectory = $"{_meshPath}/Weapon";
                var texDirectory = $"{_texturePath}/Weapon";
                var prefabDirectory = $"{_prefabPath}/Weapon";
                EnsureCreateDirectory(meshDirectory);
                EnsureCreateDirectory(texDirectory);
                EnsureCreateDirectory(prefabDirectory);

                foreach (var file in Directory.GetFiles($"{_fbxPath}/Weapon"))
                {
                    var extention = Path.GetExtension(file);
                    if (extention != ".FBX" && extention != ".fbx")
                    {
                        continue;
                    }
                    var fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(file);
                    var fbxObject = Object.Instantiate(fbxAsset);
                    fbxObject.name = fbxAsset.name;
                    var meshFilter = fbxObject.GetComponent<MeshFilter>();
                    var meshRenderer = fbxObject.GetComponent<MeshRenderer>();

                    var mesh = Object.Instantiate(meshFilter.sharedMesh) as Mesh;
                    mesh.name = meshFilter.sharedMesh.name;
                    mesh.UploadMeshData(false);

                    var tex = meshRenderer.sharedMaterial.mainTexture as Texture2D;
                    SaveMesh(meshDirectory, texDirectory, mesh, tex);

                    meshFilter.sharedMesh = mesh;
                    meshRenderer.sharedMaterial = null;
                    PrefabUtility.SaveAsPrefabAsset(fbxObject, $"{prefabDirectory}/{fbxObject.name}.prefab");

                    Object.DestroyImmediate(fbxObject);

                    AssetDatabase.Refresh();
                }
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