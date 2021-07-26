using System.Collections.Generic;
using UnityEngine;

namespace UnityAvatar
{
    public class Main : MonoBehaviour
    {
        private enum ModelPart
        {
            Foot,
            Body,
            Hand,
            Head,
            Weapon
        }

        private static readonly Dictionary<ModelPart, List<string>> _prefabDict = new Dictionary<ModelPart, List<string>>()
        {
            [ModelPart.Foot] = new List<string>() {
                "ch_pc_hou_004_jiao",
                "ch_pc_hou_006_jiao",
                "ch_pc_hou_008_jiao",
            },
            [ModelPart.Body] = new List<string>() {
                "ch_pc_hou_004_shen",
                "ch_pc_hou_006_shen",
                "ch_pc_hou_008_shen",
            },
            [ModelPart.Hand] = new List<string>() {
                "ch_pc_hou_004_shou",
                "ch_pc_hou_006_shou",
                "ch_pc_hou_008_shou",
            },
            [ModelPart.Head] = new List<string>() {
                "ch_pc_hou_004_tou",
                "ch_pc_hou_006_tou",
                "ch_pc_hou_008_tou",
            },
            [ModelPart.Weapon] = new List<string>() {
                "ch_we_one_hou_004",
                "ch_we_one_hou_006",
                "ch_we_one_hou_008",
            },
        };

        private readonly Dictionary<ModelPart, int> _curIndexDict = new Dictionary<ModelPart, int>()
        {
            [ModelPart.Foot] = 0,
            [ModelPart.Body] = 0,
            [ModelPart.Hand] = 0,
            [ModelPart.Head] = 0,
            [ModelPart.Weapon] = 0,
        };

        private bool _rotation = true;
        private List<string> _animationList = new List<string>()
        {
            "breath",
            "run",
            "attack1",
            "attack2",
            "attack3",
            "attack4",
        };
        private int _animationIndex = 0;

        private Animation _animationController = null;
        private readonly List<Texture> _texList = new List<Texture>();
        private GameObject _skeletonObject;
        private GameObject _weaponObject;
        private Material _mat;

        void Start()
        {
            Combine();
        }

        void Update()
        {
            if (_skeletonObject && _rotation)
            {
                var trans = _skeletonObject.transform;
                trans.localEulerAngles = new Vector3(0, trans.localEulerAngles.y + 1, 0);
            }
        }

        private void Combine()
        {
            foreach (var tex in _texList)
            {
                if (tex)
                {
                    Resources.UnloadAsset(tex);
                }
            }

            if (!_skeletonObject)
            {
                var skeletonAsset = Resources.Load<GameObject>("Skeleton");
                _skeletonObject = Instantiate(skeletonAsset);
            }

            var skeleton = _skeletonObject.GetComponentsInChildren<Transform>();
            var weapon_l = _skeletonObject.transform.Find("Bone_root01/Bone_root02/weapon_hand_l");
            var weapon_r = _skeletonObject.transform.Find("Bone_root01/Bone_root02/weapon_hand_r");

            var combineInstancesList = new List<CombineInstance>();
            var bonesList = new List<Transform>();

            if (!_mat)
            {
                _mat = new Material(Shader.Find("Avatar/Diffuse"));
            }

            var matPropertyBlock = new MaterialPropertyBlock();

            foreach (var temp in _curIndexDict)
            {
                var name = _prefabDict[temp.Key][temp.Value];

                if (temp.Key == ModelPart.Weapon)
                {
                    if (!_weaponObject)
                    {
                        _weaponObject = Instantiate(Resources.Load<GameObject>($"Prefab/Weapon/{name}"));
                        _weaponObject.transform.SetParent(weapon_r);
                        _weaponObject.transform.localPosition = Vector3.zero;
                        _weaponObject.transform.localEulerAngles = Vector3.zero;
                        _weaponObject.transform.localScale = Vector3.one;
                    }
                    continue;
                }

                var prefabPath = $"Prefab/Role/{name}";
                var texPath = $"Texture/Role/{name}";

                //加载纹理;
                var tex = Resources.Load<Texture>(texPath);
                _texList.Add(tex);

                if (temp.Key == ModelPart.Foot)
                {
                    matPropertyBlock.SetTexture(Shader.PropertyToID("_Texture_jiao"), tex);
                }
                if (temp.Key == ModelPart.Body)
                {
                    matPropertyBlock.SetTexture(Shader.PropertyToID("_Texture_shen"), tex);
                }
                if (temp.Key == ModelPart.Hand)
                {
                    matPropertyBlock.SetTexture(Shader.PropertyToID("_Texture_shou"), tex);
                }
                if (temp.Key == ModelPart.Head)
                {
                    matPropertyBlock.SetTexture(Shader.PropertyToID("_Texture_tou"), tex);
                }

                var go = Instantiate(Resources.Load<GameObject>(prefabPath));
                var smr = go.GetComponentInChildren<SkinnedMeshRenderer>();

                for (var sub = 0; sub < smr.sharedMesh.subMeshCount; sub++)
                {
                    var combineInstance = new CombineInstance
                    {
                        mesh = smr.sharedMesh,
                        subMeshIndex = sub
                    };
                    combineInstancesList.Add(combineInstance);
                }
                foreach (var bone in smr.bones)
                {
                    var bonename = bone.name;
                    foreach (var transform in skeleton)
                    {
                        if (transform.name != bonename)
                        {
                            continue;
                        }
                        bonesList.Add(transform);
                        break;
                    }
                }
                DestroyImmediate(go);
            }

            //四个纹理的uv布局;
            var tempUVList = new List<Rect>() {
                new Rect(0f,0f,0.5f,0.5f),
                new Rect(0.5f,0f,0.5f,0.5f),
                new Rect(0f,0.5f,0.5f,0.5f),
                new Rect(0.5f,0.5f,0.5f,0.5f),
            };
            var texUV = tempUVList.ToArray();

            var uvList = new List<Vector2[]>();
            Vector2[] oldUV, newUV;
            for (var i = 0; i < combineInstancesList.Count; i++)
            {
                oldUV = combineInstancesList[i].mesh.uv;
                newUV = new Vector2[oldUV.Length];
                for (var j = 0; j < oldUV.Length; j++)
                {
                    newUV[j] = new Vector2((oldUV[j].x * texUV[i].width) + texUV[i].x, (oldUV[j].y * texUV[i].height) + texUV[i].y);
                }
                uvList.Add(combineInstancesList[i].mesh.uv);
                combineInstancesList[i].mesh.uv = newUV;
            }

            var skinnedMeshRenderer = _skeletonObject.GetComponent<SkinnedMeshRenderer>();
            if (skinnedMeshRenderer != null)
            {
                DestroyImmediate(skinnedMeshRenderer);
            }

            skinnedMeshRenderer = _skeletonObject.AddComponent<SkinnedMeshRenderer>();
            skinnedMeshRenderer.sharedMesh = new Mesh();
            skinnedMeshRenderer.sharedMesh.CombineMeshes(combineInstancesList.ToArray(), true, false);
            skinnedMeshRenderer.bones = bonesList.ToArray();
            //skinnedMeshRenderer.materials = materials.ToArray();
            skinnedMeshRenderer.material = _mat;
            skinnedMeshRenderer.SetPropertyBlock(matPropertyBlock);

            for (var i = 0; i < combineInstancesList.Count; i++)
            {
                combineInstancesList[i].mesh.uv = uvList[i];
            }
            if (_skeletonObject.transform.childCount > 0)
            {
                _skeletonObject.transform.GetChild(0).hideFlags = HideFlags.HideInHierarchy;
            }

            _animationController = _skeletonObject.GetComponent<Animation>();
            _animationController.wrapMode = WrapMode.Loop;
            _animationController.Play(_animationList[_animationIndex]);
        }

        void OnGUI()
        {
            if (GUI.Button(new Rect(50, 170, 150, 30), _rotation ? "不旋转" : "旋转"))
            {
                _rotation = !_rotation;
            }
            if (GUI.Button(new Rect(200, 170, 150, 30), "切换动画") && _animationController)
            {
                _animationIndex++;
                if (_animationIndex >= _animationList.Count)
                {
                    _animationIndex = 0;
                }
                _animationController.Play(_animationList[_animationIndex]);
            }
            if (GUI.Button(new Rect(50, 200, 150, 30), $"换脚,当前为脚{_curIndexDict[ModelPart.Foot] + 1}"))
            {
                Change(ModelPart.Foot);
            }
            if (GUI.Button(new Rect(200, 200, 150, 30), $"换身,当前为身{_curIndexDict[ModelPart.Body] + 1}"))
            {
                Change(ModelPart.Body);
            }
            if (GUI.Button(new Rect(350, 200, 150, 30), $"换手,当前为手{_curIndexDict[ModelPart.Hand] + 1}"))
            {
                Change(ModelPart.Hand);
            }
            if (GUI.Button(new Rect(500, 200, 150, 30), $"换头,当前为头{_curIndexDict[ModelPart.Head] + 1}"))
            {
                Change(ModelPart.Head);
            }
            if (GUI.Button(new Rect(650, 200, 150, 30), $"换武器,当前为武器{_curIndexDict[ModelPart.Weapon] + 1}"))
            {
                if (_weaponObject)
                {
                    DestroyImmediate(_weaponObject);
                }
                Change(ModelPart.Weapon);
            }
        }

        private void Change(ModelPart part)
        {
            _curIndexDict[part] = _curIndexDict[part] + 1;
            if (_curIndexDict[part] > 2)
            {
                _curIndexDict[part] = 0;
            }
            Combine();
        }
    }
}