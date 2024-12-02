using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;
using static MUImporter.Enums;

namespace MUImporter
{
    [UnityEditor.AssetImporters.ScriptedImporter(1, "bmd")]
    public class BMDCustomImport : UnityEditor.AssetImporters.ScriptedImporter
    {
        public override void OnImportAsset(UnityEditor.AssetImporters.AssetImportContext ctx)
        {
            string rootName = ctx.assetPath;
            var obj = LoadBMD(rootName);
            Type objType = obj.GetType();
            if (objType.IsSubclassOf(typeof(UnityEngine.Object)))
            {
                UnityEngine.Object uObject = obj as UnityEngine.Object;
                ctx.AddObjectToAsset(uObject.name, uObject);
            }
            else if (objType == typeof(UnityEngine.Object[]))
            {
                UnityEngine.Object[] objArray = obj as UnityEngine.Object[];
                foreach (UnityEngine.Object arrayObject in objArray)
                {
                    ctx.AddObjectToAsset(arrayObject.name, arrayObject);
                }
            }
        }

        public static T LoadBMD<T>(string assetPath)
        {
            return (T)LoadBMD(assetPath);
        }

        public static System.Object LoadBMD(string assetPath)
        {
            string rootName = assetPath;

            var split = rootName.Split('/');
            var objName = split[split.Length - 1];
            objName = Path.GetFileName(assetPath);
            rootName = objName.Split('.')[0];

            FileStream file = File.OpenRead(assetPath);
            long fileSize = file.Length;
            GameObject obj = new GameObject();


            byte[] buffer = new byte[fileSize];
            file.Read(buffer, 0, (int)fileSize);
            file.Close();
            switch (objName.ToLower())
            {
                case "item.bmd":
                case "item2.bmd":
                case "item380.bmd":
                {
                    int itemLength = Marshal.SizeOf<BMDItems.ITEM_ATTRIBUTE>();

                    var itemBuffer = new byte[itemLength * MAX_ITEM];
                    Array.Copy(buffer, 0, itemBuffer, 0, itemBuffer.Length);

                    int checkSum = BitConverter.ToInt32(buffer, itemBuffer.Length);
                    if (checkSum != GenerateCheckSum2(itemBuffer, itemBuffer.Length, 0xE2F1))
                    {
                        Debug.LogError($"File {rootName} is corrupted");
                        // return;
                    }

                    var buxItems = Decryptor.BuxConvert(itemBuffer, itemBuffer.Length);
                    GCHandle handle = GCHandle.Alloc(buxItems, GCHandleType.Pinned);
                    IntPtr ptr = handle.AddrOfPinnedObject();
                    // FileStream writer = File.Create("./items.bmd");

                    // writer.Write(buxItems, 0, buxItems.Length);
                    // writer.Close();
                    BMDItems.ITEM_ATTRIBUTE[] items = new BMDItems.ITEM_ATTRIBUTE[MAX_ITEM];

                    for (int i = 0; i < MAX_ITEM; i++)
                    {
                        items[i] = Marshal.PtrToStructure<BMDItems.ITEM_ATTRIBUTE>(ptr);
                        ptr += itemLength;
                    }

                    obj.AddComponent<BMDItems>().Items = items;
                    obj.name = objName;
                    return obj;
                }
                    break;
                case "dialog.bmd":
                {
                    var dialogs = BuxReader<BMDDialog.DIALOG_SCRIPT>.Read(buffer, 200);
                    obj.AddComponent<BMDDialog>().dialogs = dialogs;
                    obj.name = objName;
                    return obj;
                }
                    break;
                case "serverlist.bmd":
                {
                    var szData = Marshal.SizeOf<BMDServerGroupInfo>();
                    int readBytes = 0;
                    var groups = new Dictionary<short, ServerGroupInfo>();
                    while (readBytes < buffer.Length)
                    {
                        var buxdata = new byte[szData];

                        Array.Copy(buffer, readBytes, buxdata, 0, buxdata.Length);

                        buxdata = Decryptor.BuxConvert(buxdata, buxdata.Length);
                        var pinnedArray = GCHandle.Alloc(buxdata, GCHandleType.Pinned);
                        var groupInfo = Marshal.PtrToStructure<BMDServerGroupInfo>(pinnedArray.AddrOfPinnedObject());
                        pinnedArray.Free();
                        buxdata = new byte[groupInfo.m_nDescriptLen];
                        readBytes += szData;
                        Array.Copy(buffer, readBytes, buxdata, 0, groupInfo.m_nDescriptLen);
                        readBytes += groupInfo.m_nDescriptLen;
                        var arr = Decryptor.BuxConvert(buxdata, buxdata.Length);
                        var groupInfoDesc = System.Text.Encoding.UTF8.GetString(arr);
                        groups.Add(groupInfo.m_wIndex, new ServerGroupInfo(groupInfo, groupInfoDesc));
                    }

                    return groups;
                }
                    break;
                case "movereq.bmd":
                {
                    var buxdata = new byte[buffer.Length - 4];
                    Array.Copy(buffer, 4, buxdata, 0, buxdata.Length);
                    buxdata = Decryptor.BuxConvert(buxdata, buxdata.Length);
                    // FileStream writer = File.Create("./movereq.bmd");

                    // writer.Write(buxdata, 0, buxdata.Length);
                    // writer.Close();

                    int moveCount = BitConverter.ToInt32(buffer, 0);
                    var moveInfoData = new BMDMoveReq.MOVEINFODATA[moveCount];

                    int moveLength = Marshal.SizeOf<BMDMoveReq.MOVEINFODATA>();
                    GCHandle handle = GCHandle.Alloc(buxdata, GCHandleType.Pinned);
                    IntPtr ptr = handle.AddrOfPinnedObject();

                    for (int i = 0; i < moveCount; i++)
                    {
                        moveInfoData[i]._ReqInfo = Marshal.PtrToStructure<BMDMoveReq.MOVEREQINFO>(ptr);
                        ptr += moveLength;
                    }

                    obj.AddComponent<BMDMoveReq>().InfoData = moveInfoData;
                    obj.name = objName;
                    return obj;
                }
                    break;
                case "quest.bmd": //FIXME: find the proper structure for 712 bytes
                {
                    var quests = BuxReader<BMDQuests.QUEST_ATTRIBUTE>.Read(buffer, MAX_QUESTS, 712);
                    obj.AddComponent<BMDQuests>().Quests = quests;
                    obj.name = objName;
                    return obj;


                    // BMDQuests.QUEST_ATTRIBUTE[] quests = new BMDQuests.QUEST_ATTRIBUTE[BMDQuests.MAX_QUESTS];
                    // var questSize = Marshal.SizeOf<BMDQuests.QUEST_ATTRIBUTE>();
                    // //FileStream writer = File.Create("./quests.bmd");

                    // for (int i = 0; i < BMDQuests.MAX_QUESTS; i++)
                    // {
                    //     var quest = new byte[questSize];
                    //     Array.Copy(buffer, i * questSize, quest, 0, questSize);
                    //     quest = Decryptor.BuxConvert(quest, questSize);
                    //     GCHandle handle = GCHandle.Alloc(quest, GCHandleType.Pinned);
                    //     IntPtr ptr = handle.AddrOfPinnedObject();
                    //     quests[i] = Marshal.PtrToStructure<BMDQuests.QUEST_ATTRIBUTE>(ptr);
                    //     //writer.Write(quest, 0, 712);

                    // }

                    // obj.AddComponent<BMDQuests>().Quests = quests;
                    // ctx.AddObjectToAsset("quests.bmd", obj);
                    // var buxdata = Decryptor.BuxConvert(buffer, buffer.Length);


                    //writer.Close();
                }
                    break;
                case "skill.bmd":
                {
                    var skills = new BMDSkill.SKILL_ATTRIBUTE[MAX_SKILLS];
                    var skillSize = Marshal.SizeOf<BMDSkill.SKILL_ATTRIBUTE>();
                    skillSize = 80; //FIXME:proper struct size
                    for (int i = 0; i < MAX_SKILLS; i++)
                    {
                        byte[] buxdata = new byte[skillSize];
                        Array.Copy(buffer, i * skillSize, buxdata, 0, skillSize);

                        buxdata = Decryptor.BuxConvert(buxdata, skillSize);

                        GCHandle handle = GCHandle.Alloc(buxdata, GCHandleType.Pinned);
                        IntPtr ptr = handle.AddrOfPinnedObject();
                        skills[i] = Marshal.PtrToStructure<BMDSkill.SKILL_ATTRIBUTE>(ptr);
                    }

                    obj.AddComponent<BMDSkill>().Skills = skills;
                    obj.name = objName;
                    return obj;
                }
                    break;
                case "socketitem.bmd":
                {
                    int itemSize = Marshal.SizeOf<BMDSocketItem.SOCKET_OPTION_INFO>(); //FIXME:proper struct size == 104
                    itemSize = 104;
                    var items = new BMDSocketItem.SOCKET_OPTION_INFO[MAX_SKILLS];
                    for (int i = 0; i < MAX_SOCKET_OPTION * MAX_SOCKET_OPTION_TYPES; i++)
                    {
                        var buxdata = new byte[itemSize];
                        Array.Copy(buffer, i * itemSize, buxdata, 0, itemSize);
                        buxdata = Decryptor.BuxConvert(buxdata, itemSize);
                        GCHandle handle = GCHandle.Alloc(buxdata, GCHandleType.Pinned);
                        IntPtr ptr = handle.AddrOfPinnedObject();
                        items[i] = Marshal.PtrToStructure<BMDSocketItem.SOCKET_OPTION_INFO>(ptr);
                    }

                    obj.AddComponent<BMDSocketItem>().SocketItem = items;
                    obj.name = objName;
                    return obj;
                }
                    break;
                case "text.bmd":
                {
                    throw new Exception("Use static GlobalText instead");
                    int bSize = Marshal.SizeOf<BMDText.GLOBALTEXT_HEADER>();
                    GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                    IntPtr ptr = handle.AddrOfPinnedObject();
                    var gtHeader = Marshal.PtrToStructure<BMDText.GLOBALTEXT_HEADER>(ptr);
                    ptr += bSize;
                    int sSize = Marshal.SizeOf<BMDText.GLOBALTEXT_STRING_HEADER>();
                    string[] texts = new string[gtHeader.dwNumberOfText];
                    if (gtHeader.wSignature != 21575)
                        return null;
                    for (int i = 0; i < gtHeader.dwNumberOfText; i++)
                    {
                        var gtStringHeader = Marshal.PtrToStructure<BMDText.GLOBALTEXT_STRING_HEADER>(ptr);
                        ptr += sSize;
                        bSize += sSize;

                        byte[] stringBufffer = new byte[gtStringHeader.dwSizeOfString];
                        Marshal.Copy(ptr, stringBufffer, 0, gtStringHeader.dwSizeOfString);
                        ptr += gtStringHeader.dwSizeOfString;
                        bSize += gtStringHeader.dwSizeOfString;

                        if ( /*CheckLoadDisposition(GTStringHeader.dwKey, dwLoadDisposition) ||*/
                            gtStringHeader.dwKey < (int)GlobalTexts.MAX_NUMBER_OF_TEXTS && gtStringHeader.dwSizeOfString > 0)
                        {
                            stringBufffer = Decryptor.BuxConvert(stringBufffer, gtStringHeader.dwSizeOfString, 0);
                            texts[i] = System.Text.Encoding.Default.GetString(stringBufffer);
                        }
                    }

                    obj.AddComponent<BMDText>().stringList = texts;
                    return obj;
                }
                    break;
                case "itemsettype.bmd":
                {
                    var buffer2 = new byte[buffer.Length - 4];
                    Array.Copy(buffer, 4, buffer2, 0, buffer2.Length);

                    var items = BuxReader<BMDItemSetType.ITEM_SET_TYPE>.Read(buffer2, MAX_QUESTS);
                    obj.AddComponent<BMDItemSetType>().ItemSetType = items;
                    obj.name = objName;
                    return obj;
                }
                    break;
                case "itemsetoption.bmd":
                {
                    var buffer2 = new byte[buffer.Length - 4];
                    Array.Copy(buffer, 4, buffer2, 0, buffer2.Length);

                    var items = BuxReader<BMDItemSetOption.ITEM_SET_OPTION>.Read(buffer2, MAX_SET_OPTION);
                    obj.AddComponent<BMDItemSetOption>().ItemSetOption = items;
                    obj.name = objName;
                    return obj;
                }
                    break;
                case "questprogress.bmd":
                {
                    throw new Exception(objName + " is not described");


                    var items = BuxReader<BMDQuestProgress.SQuestProgress>.Read(buffer, MAX_SET_OPTION);
                    obj.AddComponent<BMDQuestProgress>().QuestProgress = items;
                    obj.name = objName;
                    return obj;
                }
                    break;
                case "questwords.bmd":
                {
                    throw new Exception(objName + " is not described");


                    var items = BuxReader<BMDQuestProgress.SQuestProgress>.Read(buffer, MAX_SET_OPTION);
                    obj.AddComponent<BMDQuestProgress>().QuestProgress = items;
                    obj.name = objName;
                    return obj;
                }
                    break;
                case "gate.bmd":
                {
                    var items = BuxReader<BMDGate.GATE_ATTRIBUTE>.Read(buffer, MAX_GATES, 14); //FIXME: proper struct size
                    obj.AddComponent<BMDGate>().Gates = items;
                    obj.name = objName;
                    return obj;
                }
                    break;
                case "filter.bmd": //FIXME: empty result
                {
                    // var buxdata = Decryptor.BuxConvert(buffer, buffer.Length - 4);

                    // FileStream writer = File.Create(objName);
                    // writer.Write(buxdata, 0, buxdata.Length);
                    // writer.Close();

                    var items = BuxReader<BMDFilter.FILTER>.Read(buffer, MAX_FILTERS); //FIXME: proper struct size
                    obj.AddComponent<BMDFilter>().Filters = items;
                    obj.name = objName;
                    return obj;
                }
                    break;
                case "filtername.bmd":
                {
                    var items = BuxReader<BMDFilter.FILTER>.Read(buffer, MAX_NAMEFILTERS); //FIXME: proper struct size
                    obj.AddComponent<BMDFilter>().Filters = items;
                    obj.name = objName;
                    return obj;
                }
                    break;
                case "monsterskill.bmd": //todo : watch sources
                {
                    var fileCount = BitConverter.ToInt32(buffer, 0);
                    var buxdata = new byte[buffer.Length - 4];
                    Array.Copy(buffer, 4, buxdata, 0, buxdata.Length);
                    var items = BuxReader<BMDMonsterSkill.Script_Skill>.Read(buxdata,
                        fileCount); //FIXME: proper struct size
                    obj.AddComponent<BMDMonsterSkill>().Skills = items;
                    obj.name = objName;
                    return obj;
                }
                    break;
                case "masterskilltree.bmd":
                {
                    var itemSize = 5;

                    // var items = new byte[9][];

                    // for (int i = 0; i < 9; i++)
                    // {
                    //     var buxdata = new byte[itemSize];
                    //     buxdata = Decryptor.BuxConvert(buffer, itemSize, i * itemSize);
                    //     items[i] = buxdata;
                    // }

                    var items = BuxReader<BMDMasterSkillTree.MASTER_SKILL_DATA>.Read(buffer, 9);

                    var items2 = BuxReader<BMDMasterSkillTree.MASTER_LEVEL_DATA>.Read(buffer, MAX_MASTER, 45);


                    var st = obj.AddComponent<BMDMasterSkillTree>();
                    st.Data = items2;
                    st.MasterSkillData = items;

                    obj.name = objName;
                    return obj;
                }
                    break;
                default:
                {
                    var objs = new List<UnityEngine.Object>();


                    BMD bmdObject = obj.AddComponent<BMD>();

                    bmdObject.FromBytes(buffer);
                    obj.name = rootName;
                    objs.Add(obj);

                    List<Transform> bones = new List<Transform>();
                    List<Matrix4x4> bindPoses = new List<Matrix4x4>();

                    int iterator = 0;
                    Avatar _avatar = null;
                    List<string> boneNames = new List<string>();
                    // StreamWriter writer = new StreamWriter("./bones.txt");

                    var bmdBones = bmdObject.Bones;
                    if (bmdBones != null)
                    {
                        for (int l = 0; l < bmdBones.Length; l++)
                        {
                            // writer.WriteLine($"BoneMesh {l}");
                            if (bmdBones[l].BoneInfo.IsNull == 0)
                            {
                                var bName = new string(bmdBones[l].BoneInfo.sName).Split('\0')[0];
                                // writer.Write(bmdBones[l].BoneInfo.Parent + " ");
                                // writer.WriteLine(bName);
                                for (int i = 0; i < bmdBones[l].lpVertPos.Length; i++)
                                {
                                    // writer.WriteLine($"Anim {i}");
                                    // for (int j = 0; j < bmdBones[l].lpVertPos[i].vector.Length; j++)
                                    // {
                                    //     writer.WriteLine($"position: {bmdBones[l].lpVertPos[i].vector[j]}");
                                    //     writer.WriteLine($"rotation: {bmdBones[l].lpVertRot[i].vector[j]}");
                                    //     writer.WriteLine();
                                    // }
                                }

                                // if (l != 0)
                                // {
                                //     bool canContinue = true;
                                //     foreach (var item in bones)
                                //     {
                                //         if (item)
                                //             if (item.name.Equals(bName))
                                //             {
                                //                 canContinue = false;
                                //                 break;
                                //             }
                                //     }
                                //     if (!canContinue)
                                //         continue;
                                // }
                                if (boneNames.Contains(bName))
                                {
                                    bName += l;
                                }

                                bones.Add(new GameObject(bName).transform);
                                if (bmdBones[l].BoneInfo.Parent != -1)
                                {
                                    //                    Debug.Log(l);
                                    bones[l].parent = bones[bmdBones[l].BoneInfo.Parent];
                                    boneNames.Add(boneNames[bmdBones[l].BoneInfo.Parent] + "/" + bName);

                                    // bindPoses.Add(bones[l].worldToLocalMatrix * obj.transform.localToWorldMatrix);
                                }
                                else
                                {
                                    bones[l].parent = obj.transform;
                                    boneNames.Add(bName);

                                    // bindPoses.Add(bones[l].worldToLocalMatrix * obj.transform.localToWorldMatrix);
                                }

                                bones[l].localPosition = Vector3.zero; // bmdBones[l].lpVertPos[0].vector[0];
                                bones[l].localRotation =
                                    Quaternion.identity; //Quaternion.Euler(bmdBones[l].lpVertRot[0].vector[0]);

                                bindPoses.Add(bones[l].worldToLocalMatrix);
                            }
                            else
                            {
                                bones.Add(null);
                                boneNames.Add(null);
                            }
                        }
                        // writer.Close();

                        _avatar = AvatarBuilder.BuildGenericAvatar(bones[0].gameObject, "");
                        _avatar.name = rootName + iterator + ".avatar";
                        objs.Add(_avatar);
                    }

                    if (bmdObject.Meshes != null)
                    {
                        foreach (var bmdMesh in bmdObject.Meshes)
                        {
                            GameObject newMesh = new GameObject($"mesh{iterator}");
                            newMesh.transform.parent = bmdObject.transform;

                            Mesh mesh = new Mesh();
                            mesh.Clear();

                            mesh.name = $"mesh{iterator}";
                            var vertices = bmdMesh.GetVertices();
                            BoneWeight[] weights = new BoneWeight[vertices.Vertices.Length];
                            for (int l = 0; l < vertices.Vertices.Length; l++)
                            {
                                if (vertices.parents[l] < 0)
                                    vertices.parents[l] = 0;
                                weights[l].boneIndex0 = vertices.parents[l];
                                weights[l].weight0 = 1;
                            }


                            mesh.vertices = vertices.Vertices;
                            mesh.triangles = bmdMesh.GetTriangles();
                            mesh.uv = bmdMesh.GetUV();
                            mesh.normals = bmdMesh.GetNomals();


                            mesh.bindposes = bindPoses.ToArray();
                            mesh.boneWeights = weights;


                            mesh.RecalculateBounds();


                            var mr = newMesh.AddComponent<SkinnedMeshRenderer>();
                            System.Collections.Generic.List<char> textChars = new System.Collections.Generic.List<char>();
                            for (int j = 0; j < bmdMesh.szTextureName.Length; j++)
                            {
                                if (bmdMesh.szTextureName[j] == 0)
                                {
                                    break;
                                }

                                textChars.Add((char)bmdMesh.szTextureName[j]);
                            }
                            //---
                            // string texture = new string(textChars.ToArray());
                            // texture = texture.Split('.')[0];
                            // string[] assetPath1 = AssetDatabase.FindAssets(texture+" t:Material");

                            // if (assetPath1.Length > 0)
                            // {
                            //     mr.sharedMaterials = new Material[] { Resources.Load<Material>(assetPath1[0]) };
                            // }
                            //---
                            string texture = new string(textChars.ToArray());
                            //texture = texture.Split('.')[0];
                            // Material mat = AssetDatabase.LoadAssetAtPath<Material>($"Assets/{texture}.ozj");
                            // AssetDatabase.GUIDToAssetPath
                            // Material material = Resources.Load<Material>(texture + ".ozj");
                            string thisFolder = assetPath.Substring(0, assetPath.LastIndexOf('/'));
                            string[] assetPath1 = AssetDatabase.FindAssets($"{texture}", new string[] { thisFolder });


                            Material material = null;


                            foreach (var ap in assetPath1)
                            {
                                string path = AssetDatabase.GUIDToAssetPath(ap);
                                material = AssetDatabase.LoadAssetAtPath<Material>(path);
                                // material.mainTexture = texture1;
                                //material = Resources.Load<Material>(path);
                                if (material?.mainTexture)
                                {
                                    break;
                                }
                            }

                            mr.sharedMaterial = material;
                            mr.sharedMesh = mesh;
                            mr.bones = bones.ToArray();
                            // ---
                            // ctx.AddObjectToAsset(rootName, material);
                            mesh.name = rootName + iterator;
                            newMesh.name = rootName + iterator;
                            objs.Add(mesh);
                            objs.Add(newMesh);
                            iterator++;
                        }
                    }

                    // Animator _animator = obj.AddComponent<Animator>();
                    if (bmdObject.Anims != null)
                    {
                        Animation _anim = obj.AddComponent<Animation>();
                        for (int i = 0; i < bmdObject.Anims.Length; i++)
                        {
                            var clip = new AnimationClip();


                            clip.name = rootName + i + ".clip";

                            //clip.legacy = true;

                            int frameCount = bmdObject.Anims[i].AnimInfo.nFrameCnt;
                            ///---x

                            for (int l = 0; l < bmdObject.Bones.Length; l++)
                            {
                                if (bmdObject.Bones[l].BoneInfo.IsNull == 0)
                                {
                                    Keyframe[] keysX = new Keyframe[frameCount];
                                    Keyframe[] keysY = new Keyframe[frameCount];
                                    Keyframe[] keysZ = new Keyframe[frameCount];
                                    Keyframe[] keysRX = new Keyframe[frameCount];
                                    Keyframe[] keysRY = new Keyframe[frameCount];
                                    Keyframe[] keysRZ = new Keyframe[frameCount];
                                    Keyframe[] keysRW = new Keyframe[frameCount];

                                    for (int j = 0; j < frameCount; j++)
                                    {
                                        keysX[j] = new Keyframe(((float)j) / 2, bmdObject.Bones[l].lpVertPos[i].vector[j].x,
                                            0, 0, 0, 0);
                                        keysY[j] = new Keyframe(((float)j) / 2, bmdObject.Bones[l].lpVertPos[i].vector[j].y,
                                            0, 0, 0, 0);
                                        keysZ[j] = new Keyframe(((float)j) / 2, bmdObject.Bones[l].lpVertPos[i].vector[j].z,
                                            0, 0, 0, 0);
                                        keysRX[j] = new Keyframe(((float)j) / 2,
                                            bmdObject.Bones[l].lpVertRot[i].vector3s[j].x, 0, 0, 0, 0);
                                        keysRY[j] = new Keyframe(((float)j) / 2,
                                            bmdObject.Bones[l].lpVertRot[i].vector3s[j].y, 0, 0, 0, 0);
                                        keysRZ[j] = new Keyframe(((float)j) / 2,
                                            bmdObject.Bones[l].lpVertRot[i].vector3s[j].z, 0, 0, 0, 0);


                                        // keysRX[j] = new Keyframe(((float)j) / 2, bmdObject.Bones[l].lpVertRot[i].vector[j].x, 0, 0, 0, 0);
                                        // keysRY[j] = new Keyframe(((float)j) / 2, bmdObject.Bones[l].lpVertRot[i].vector[j].y, 0, 0, 0, 0);
                                        // keysRZ[j] = new Keyframe(((float)j) / 2, bmdObject.Bones[l].lpVertRot[i].vector[j].z, 0, 0, 0, 0);
                                        // keysRW[j] = new Keyframe(((float)j) / 2, bmdObject.Bones[l].lpVertRot[i].vector[j].w, 0, 0, 0, 0);

                                        // keysRX[j] = new Keyframe(((float)j) / 2, bmdObject.Bones[l].lpVertRot[i].vector[j].eulerAngles.x, 0, 0, 0, 0);
                                        // keysRY[j] = new Keyframe(((float)j) / 2, bmdObject.Bones[l].lpVertRot[i].vector[j].eulerAngles.y, 0, 0, 0, 0);
                                        // keysRZ[j] = new Keyframe(((float)j) / 2, bmdObject.Bones[l].lpVertRot[i].vector[j].eulerAngles.z, 0, 0, 0, 0);
                                    }

                                    AnimationCurve curveX = new AnimationCurve(keysX);
                                    AnimationCurve curveY = new AnimationCurve(keysY);
                                    AnimationCurve curveZ = new AnimationCurve(keysZ);
                                    AnimationCurve curveRX = new AnimationCurve(keysRX);
                                    AnimationCurve curveRY = new AnimationCurve(keysRY);
                                    AnimationCurve curveRZ = new AnimationCurve(keysRZ);
                                    AnimationCurve curveRW = new AnimationCurve(keysRW);

                                    clip.SetCurve(boneNames[l], typeof(Transform), "localPosition.x", curveX);
                                    clip.SetCurve(boneNames[l], typeof(Transform), "localPosition.y", curveY);
                                    clip.SetCurve(boneNames[l], typeof(Transform), "localPosition.z", curveZ);
                                    clip.SetCurve(boneNames[l], typeof(Transform), "localEulerAnglesRaw.x", curveRX);
                                    clip.SetCurve(boneNames[l], typeof(Transform), "localEulerAnglesRaw.y", curveRY);
                                    clip.SetCurve(boneNames[l], typeof(Transform), "localEulerAnglesRaw.z", curveRZ);
                                    // clip.SetCurve(boneNames[l], typeof(Transform), "localRotation.x", curveRX);
                                    // clip.SetCurve(boneNames[l], typeof(Transform), "localRotation.y", curveRY);
                                    // clip.SetCurve(boneNames[l], typeof(Transform), "localRotation.z", curveRZ);
                                    // clip.SetCurve(boneNames[l], typeof(Transform), "localRotation.w", curveRW);
                                }
                            }

                            clip.legacy = true;
                            clip.wrapMode = UnityEngine.WrapMode.Loop;


                            if (i == 0)
                                _anim.clip = clip;
                            _anim.AddClip(clip, rootName + i + ".clip");
                            objs.Add(clip);
                        }

                        _anim.name = rootName + ".animator";
                        objs.Add(_anim);
                    }

                    return objs.ToArray();
                }
                    break;
                    return null;
            }
        }

        static int GenerateCheckSum2(byte[] pbyBuffer, int dwSize, int wKey)
        {
            int dwKey = (int)wKey;
            int dwResult = dwKey << 9;
            for (int dwChecked = 0; dwChecked <= dwSize - 4; dwChecked += 4)
            {
                int dwTemp;
                dwTemp = pbyBuffer[dwChecked];
                // memcpy( &dwTemp, pbyBuffer + dwChecked, sizeof ( int));


                switch (((dwChecked / 4) + wKey) % 2)
                {
                    case 0:
                        dwResult ^= dwTemp;
                        break;
                    case 1:
                        dwResult += dwTemp;
                        break;
                }

                if (0 == (dwChecked % 16))
                {
                    dwResult ^= ((dwKey + dwResult) >> ((dwChecked / 4) % 8 + 1));
                }
            }

            return (dwResult);
        }

        static class BuxReader<T>
        {
            public static T[] Read(byte[] buffer, int count, int properStructureSize = 0)
            {
                T[] items = new T[count];
                var itemSize = Marshal.SizeOf<T>();
                itemSize = properStructureSize != 0 ? properStructureSize : itemSize; //TODO: Remove after fixing

                for (int i = 0; i < count; i++)
                {
                    var item = new byte[itemSize];
                    Array.Copy(buffer, i * itemSize, item, 0, itemSize);
                    item = Decryptor.BuxConvert(item, itemSize);
                    GCHandle handle = GCHandle.Alloc(item, GCHandleType.Pinned);
                    IntPtr ptr = handle.AddrOfPinnedObject();
                    items[i] = Marshal.PtrToStructure<T>(ptr);
                    //writer.Write(quest, 0, 712);
                }

                return items;
            }
        }
    }
}