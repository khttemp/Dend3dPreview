using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using UnityEngine;
using UnityEngine.Rendering;
using System.IO;

using MainClass;
using MdlMgrClass;
using JointMdlClass;
using AmbListClass;
using AmbDataClass;
using AmbMdlClass;
using EkihyoClass;

namespace AMBMgrClass
{
    public class AMBMgr
    {
        public List<GameObject>[] ambObjList;
        MdlMgr mMdlMgr = new MdlMgr();
        public Dictionary<string, string[]> origin_size_per_dict;
        public Dictionary<string, string[]> size_per_dict;
        public List<string> keyList;
        public Dictionary<string, int> origin_kasen_model_dict;
        public Dictionary<string, int> kasen_model_dict;
        public List<string> kasenModelList;
        public Dictionary<string, List<string[]>> origin_ekihyo_dict;
        public Dictionary<string, List<string[]>> ekihyo_dict;
        public List<string> EkihyoKeyList;
        public List<string[]> EkihyoSeriesList;
        public Dictionary<string, AssetBundle> stage_res_ab_dict;
        public List<Texture2D> TexList;
        public bool isError = false;
        public int drawAmbCount = 0;
        public int search_amb_no = -1;
        public int search_amb_index = -1;

        public AMBMgr()
        {
            origin_size_per_dict = new Dictionary<string, string[]>(){
                {"AMB_BLACK_CHU", new string[]{"AMB_BLACK_CHU 1", "", "", ""}},
                {"AMB_BlackCube", new string[]{"Scale", "", "", ""}},
                {"AMB_HQ_KasenLong0", new string[]{"center_bar", "LeftPos", "RightPos", ""}},
                {"AMB_Kaidan2", new string[]{"Center", "LeftPos", "RightPos", ""}},
                {"AMB_Kasenchu_Center", new string[]{"CenterLong", "", "", ""}},
                {"AMB_Kasenchu_Left", new string[]{"Center_S", "LeftPos", "", ""}},
                {"AMB_Kasenchu_Long0", new string[]{"CenterLong", "LeftPos", "RightPos", ""}},
                {"AMB_Kasenchu_Long1", new string[]{"Center_S", "LeftPos", "RightPos", ""}},
                {"AMB_Kasenchu_Long2", new string[]{"CenterLong", "LeftPos", "RightPos", ""}},
                {"AMB_Kasenchu_Short", new string[]{"Center_S", "LeftPos", "RightPos", ""}},
                {"AMB_Mina_Wall2", new string[]{"obj8", "", "", ""}},
                {"AMB_SAKU", new string[]{"st_saku_obj", "", "", ""}},
                {"AMB_Toyo_Kasenchu", new string[]{"obj71", "LeftPos", "", ""}},
                {"AMB_TQ_Kaidan3", new string[]{"obj40", "", "", ""}},
                {"TQ_Eda_Iron", new string[]{"Mesh0", "Left", "Right", ""}},
                {"TQ_Eda_Yane", new string[]{"J00", "", "", "Hashira"}}
            };
            keyList = new List<string>(origin_size_per_dict.Keys);
            for (int i = 0; i < keyList.Count; i++)
            {
                keyList[i] = keyList[i].ToUpper();
            }
            List<string[]> valueList = new List<string[]>(origin_size_per_dict.Values);
            size_per_dict = new Dictionary<string, string[]>();
            for (int i = 0; i < valueList.Count; i++)
            {
                size_per_dict.Add(keyList[i], valueList[i]);
            }

            origin_kasen_model_dict = new Dictionary<string, int>(){
                {"1Rail_HQ_Konkuri100_AMB", 1},
                {"1Rail_HQ_Konkuri25_AMB", 1},
                {"1Rail_HQ_Konkuri50_AMB", 1},
                {"1Rail_Kin_100_AMB", 1},
                {"1Rail_Kin_25_AMB", 1},
                {"1Rail_Kin_50_AMB", 1},
                {"2Rail_Den_100_AMB", 2},
                {"2Rail_Den_25_AMB", 2},
                {"2Rail_Den_50_AMB", 2},
                {"2Rail_Kin_100_AMB", 2},
                {"AMB_1Rail_Den_100", 1},
                {"AMB_1Rail_Den_100_Under", 1},
                {"AMB_1Rail_Den_25", 1},
                {"AMB_1Rail_Den_25_Under", 1},
                {"AMB_1Rail_Den_50", 1},
                {"AMB_1Rail_Den_50_Konkuri", 1},
                {"AMB_1Rail_Den_50_Under", 1},
                {"AMB_1Rail_Den_50W", 1},
                {"AMB_1Rail_Kin_100", 1},
                {"AMB_1Rail_Kin_25", 1},
                {"AMB_1Rail_Kin_50", 1},
                {"AMB_2Rail100Only", 2},
                {"rail_only_50", 1}
            };
            kasenModelList = new List<string>(origin_kasen_model_dict.Keys);
            for (int i = 0; i < kasenModelList.Count; i++)
            {
                kasenModelList[i] = kasenModelList[i].ToUpper();
            }
            List<int> kasenValueList = new List<int>(origin_kasen_model_dict.Values);
            kasen_model_dict = new Dictionary<string, int>();
            for (int i = 0; i < kasenValueList.Count; i++)
            {
                kasen_model_dict.Add(kasenModelList[i], kasenValueList[i]);
            }

            origin_ekihyo_dict = new Dictionary<string, List<string[]>>(){
                {
                    "AMB_DenWall", new List<string[]>(){
                        new string[]{"TQ_Ekihyo_Only", "0"}
                    }
                },
                {
                    "AMB_Ekihyo", new List<string[]>(){
                        new string[]{"BASE", "0"}
                    }
                },
                {
                    "AMB_Ekihyo_Reg", new List<string[]>(){
                        new string[]{"BASE", "1"}
                    }
                },
                {
                    "AMB_Ekihyo_Tate", new List<string[]>(){
                        new string[]{null}
                    }
                },
                {
                    "AMB_HQ_YANE0", new List<string[]>(){
                        new string[]{"HQ_Ekihyo", "2"},
                        new string[]{"HQ_Ekihyo", "3"}
                    }
                },
                {
                    "AMB_HQ_YANE1", new List<string[]>(){
                        new string[]{"HQ_Ekihyo", "2"},
                        new string[]{"HQ_Ekihyo", "3"}
                    }
                },
                {
                    "AMB_HQ_YANE2", new List<string[]>(){
                        new string[]{"HQ_Ekihyo", "2"},
                        new string[]{"HQ_Ekihyo", "3"}
                    }
                },
                {
                    "AMB_HQ_YANE3", new List<string[]>(){
                        new string[]{"HQ_Ekihyo", "2"},
                        new string[]{"HQ_Ekihyo", "3"}
                    }
                },
                {
                    "AMB_MINA_ST_WALL", new List<string[]>(){
                        new string[]{"MM_Ekihyo_Only", "4"}
                    }
                },
                {
                    "AMB_Mina_Wall", new List<string[]>(){
                        new string[]{"MM_Ekihyo_Only", "4"}
                    }
                },
                {
                    "AMB_MM_Ekihyo", new List<string[]>(){
                        new string[]{"MM_Ekihyo_Only", "4"}
                    }
                },
                {
                    "AMB_ShibuyaWall", new List<string[]>(){
                        new string[]{"TQ_Ekihyo_Only", "0"}
                    }
                },
                {
                    "AMB_ST_WALL", new List<string[]>(){
                        new string[]{"MM_Ekihyo_Only", "4"}
                    }
                },
                {
                    "AMB_TQLight", new List<string[]>(){
                        new string[]{"TQ_Ekihyo2 1", "6"},
                        new string[]{"TQ_Ekihyo2 (1) 2", "7"},
                        new string[]{"TQ_Ekihyo2 2", "6"},
                        new string[]{"TQ_Ekihyo2 (1) 1", "7"},
                        new string[]{"TQ_Ekihyo2", "6"},
                        new string[]{"TQ_Ekihyo2 (1)", "7"}
                    }
                },
                {
                    "Aobadai", new List<string[]>(){
                        new string[]{"TQ_Ekihyo1 1", "0"},
                        new string[]{"TQ_Ekihyo1", "0"}
                    }
                },
                {
                    "basha_wall", new List<string[]>(){
                        new string[]{"MM_Ekihyo_Only", "4"}
                    }
                },
                {
                    "HQ_Ekihyo", new List<string[]>(){
                        new string[]{"Base", "2"},
                        new string[]{"Base", "3"}
                    }
                },
                {
                    "ST_IronHashira", new List<string[]>(){
                        new string[]{"TQ_Ekihyo1", "0"},
                        new string[]{"TQ_Ekihyo1 (1)", "0"}
                    }
                },
                {
                    "ST_IronHashira2", new List<string[]>(){
                        new string[]{"TQ_Ekihyo1", "0"},
                        new string[]{"TQ_Ekihyo1 (1)", "0"}
                    }
                },
                {
                    "takatu_st", new List<string[]>(){
                        new string[]{"Ekihyo", "5"}
                    }
                },
                {
                    "takatu_st_none", new List<string[]>(){
                        new string[]{"Ekihyo", "5"}
                    }
                },
                {
                    "tq_hutako", new List<string[]>(){
                        new string[]{"Ekihyo", "5"},
                        new string[]{"Ekihyo (1)", "5"}
                    }
                },
                {
                    "tq_hutako2", new List<string[]>(){
                        new string[]{"Ekihyo", "5"},
                        new string[]{"Ekihyo (1)", "5"}
                    }
                },
                {
                    "TQ_Obj1", new List<string[]>(){
                        new string[]{"TQ_Ekihyo1", "0"},
                        new string[]{"TQ_Ekihyo1 (1)", "0"}
                    }
                },
                {
                    "tq_st_wall2", new List<string[]>(){
                        new string[]{"TQ_Ekihyo1", "0"}
                    }
                },
                {
                    "TQ_Yane_Big", new List<string[]>(){
                        new string[]{"Ekihyo", "5"}
                    }
                },
                {
                    "TQ_Yane00", new List<string[]>(){
                        new string[]{"Ekihyo", "5"}
                    }
                },
                {
                    "Yane_W", new List<string[]>(){
                        new string[]{"Ekihyo", "5"},
                        new string[]{"Ekihyo (1)", "5"}
                    }
                },
                {
                    "Yane_W2", new List<string[]>(){
                        new string[]{"Ekihyo", "5"},
                        new string[]{"Ekihyo (1)", "5"}
                    }
                }
            };
            EkihyoKeyList = new List<string>(origin_ekihyo_dict.Keys);
            for (int i = 0; i < EkihyoKeyList.Count; i++)
            {
                EkihyoKeyList[i] = EkihyoKeyList[i].ToUpper();
            }
            List<List<string[]>> EkihyoValueList = new List<List<string[]>>(origin_ekihyo_dict.Values);
            ekihyo_dict = new Dictionary<string, List<string[]>>();
            for (int i = 0; i < EkihyoValueList.Count; i++)
            {
                ekihyo_dict.Add(EkihyoKeyList[i], EkihyoValueList[i]);
            }
            EkihyoSeriesList = new List<string[]>(){
                new string[]{"TQ_Ekihyo", "to_shibuya", "to_chuorinkan"},
                new string[]{null, "to_shibuya", "to_chuorinkan"},
                new string[]{"obj64"},
                new string[]{"obj65"},
                new string[]{null, "obj114", "obj115"},
                new string[]{null, "TQ_Den_Ekihyo0", "TQ_Den_Ekihyo1"},
                new string[]{"TQ_Ekihyo2"},
                new string[]{"TQ_Ekihyo2 (1)"}
            };
        }

        public void CreateAMB(int amb_index, int child_index, int rail_no, string mdl_name, amb_data ambData, Main mMain)
        {
            GameObject ambObj = mMdlMgr.MdlCreate(mdl_name, "amb", mMain);
            if (ambObj == null) {
                isError = true;
                return;
            }
            drawAmbCount++;
            ambObj.AddComponent<JointMdl>();
            JointMdl ambJointModel = ambObj.GetComponent<JointMdl>();
            ambJointModel.Name = ambObj.name;
            ambJointModel.BasePos = ambData.offsetpos;
            ambJointModel.BaseRot = ambData.dir;
            ambJointModel.JointDir = ambData.joint_dir;
            ambJointModel.LengthPer = ambData.per;

            ambObj.name = ambObj.name + "_(AMB)(" + amb_index + "-" + child_index + ")";
            ambObj.AddComponent<AmbMdl>();
            AmbMdl ambMdl = ambObj.GetComponent<AmbMdl>();
            if (child_index == 0)
            {
                ambMdl.ParentRailNo = rail_no;
            }
            else
            {
                ambMdl.ParentRailNo = -1;
            }
            ambMdl.ParentAmbNo = amb_index;
            ambMdl.ParentAmbIndex = ambData.parentindex;
            ambMdl.ChildIndex = child_index;
            ambMdl.KasenChuScale = ambData.size_per;
            ambMdl.isExistKasen = false;
            ambMdl.kasenCnt = 0;

            if (keyList.Contains(ambJointModel.Name.ToUpper()))
            {
                SetScale(ambObj, ambMdl.KasenChuScale, ambJointModel.Name.ToUpper(), size_per_dict[ambJointModel.Name.ToUpper()]);
            }

            List<GameObject> ambMdlList;
            if (ambObjList[amb_index] == null)
            {
                ambMdlList = new List<GameObject>();
                ambObjList[amb_index] = ambMdlList;
            }
            else
            {
                ambMdlList = ambObjList[amb_index];
            }
            ambMdlList.Add(ambObj);

            if (kasenModelList.Contains(ambJointModel.Name.ToUpper()))
            {
                ambMdl.isExistKasen = true;
                ambMdl.kasenCnt = kasen_model_dict[ambJointModel.Name.ToUpper()];
            }

            MeshRenderer[] ambMeshList = ambObj.transform.GetComponentsInChildren<MeshRenderer>(true);
            if (ambMeshList != null)
            {
                for (int i = 0; i < ambMeshList.Length; i++)
                {
                    ambMeshList[i].lightProbeUsage = LightProbeUsage.Off;
                    ambMeshList[i].reflectionProbeUsage = ReflectionProbeUsage.Off;
                    ambMeshList[i].shadowCastingMode = ShadowCastingMode.Off;
                    ambMeshList[i].receiveShadows = false;
                }
            }
            SkinnedMeshRenderer[] ambSkinnedMeshList = ambObj.transform.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            if (ambSkinnedMeshList != null)
            {
                for (int i = 0; i < ambSkinnedMeshList.Length; i++)
                {
                    ambSkinnedMeshList[i].lightProbeUsage = LightProbeUsage.Off;
                    ambSkinnedMeshList[i].reflectionProbeUsage = ReflectionProbeUsage.Off;
                    ambSkinnedMeshList[i].shadowCastingMode = ShadowCastingMode.Off;
                    ambSkinnedMeshList[i].receiveShadows = false;
                    if (ambSkinnedMeshList[i].sharedMesh != null)
                    {
                        ambSkinnedMeshList[i].sharedMesh.RecalculateBounds();
                    }
                }
            }
        }

        public void SetScale(GameObject ambObj, float scale, string name, string[] infoList)
        {
            Vector3 centerV = Vector3.one;
            Vector3 v = Vector3.one;
            centerV.x = scale;
            v.x = 1f / scale;
            List<Transform> childList = SearchChild<Transform>(ambObj.transform);
            if (name.Equals("AMB_TQ_Kaidan3".ToUpper()))
            {
                Transform baseChild = childList.Find(x => x.name == "tq_aobadai_kaidan");
                if (baseChild != null && baseChild.childCount != 0)
                {
                    for (int i = 0; i < baseChild.childCount; i++)
                    {
                        // Center
                        Transform child = baseChild.GetChild(i);
                        if (child.name.Equals(infoList[0]))
                        {
                            child.localScale = centerV;
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < childList.Count; i++)
                {
                    Transform child = childList[i];
                    // Center
                    if (!"".Equals(infoList[0]) && child.name.Equals(infoList[0]))
                    {
                        child.localScale = centerV;
                    }
                    // LeftDel
                    if (!"".Equals(infoList[1]) && child.name.Contains(infoList[1]))
                    {
                        child.localScale = v;
                    }
                    // RightDel
                    if (!"".Equals(infoList[2]) && child.name.Contains(infoList[2]))
                    {
                        child.localScale = v;
                    }
                    // DelObj
                    if (!"".Equals(infoList[3]) && child.name.Contains(infoList[3]))
                    {
                        child.localScale = v;
                    }
                }
            }
        }

        public List<T> SearchChild<T>(Transform t)
        {
            List<T> list = new List<T>();
            if (t != null)
            {
                T[] componentsInChildren = t.GetComponentsInChildren<T>(true);
                list.AddRange(componentsInChildren);
            }
            return list;
        }

        public void RemoveAMB()
        {
            if (ambObjList != null)
            {
                for (int i = 0; i < ambObjList.Length; i++)
                {
                    if (ambObjList[i] != null)
                    {
                        for (int j = 0; j < ambObjList[i].Count; j++)
                        {
                            if (!(ambObjList[i][j] == null))
                            {
                                UnityEngine.Object.DestroyImmediate(ambObjList[i][j]);
                            }
                        }
                    }
                }
            }
            ambObjList = null;
            stage_res_ab_dict = null;
            TexList = null;
            search_amb_no = -1;
            search_amb_index = -1;
            System.GC.Collect();
        }

        public void UpdateAll(Main mMain)
        {
            if (ambObjList != null)
            {
                for (int i = 0; i < ambObjList.Length; i++)
                {
                    if (ambObjList[i] != null)
                    {
                        for (int j = 0; j < ambObjList[i].Count; j++)
                        {
                            if (!(ambObjList[i][j] == null))
                            {
                                UpdateIndex(ambObjList[i][j], mMain);
                            }
                        }
                    }
                }
            }
        }

        public void UpdateIndex(GameObject ambObj, Main mMain)
        {
            AmbMdl ambMdl = ambObj.GetComponent<AmbMdl>();
            JointMdl ambObjJointMdl = ambObj.GetComponent<JointMdl>();
            int amb_index = ambMdl.ParentAmbNo;
            int parent_index = ambMdl.ParentAmbIndex;
            int child_index = ambMdl.ChildIndex;
            if (ambMdl.ParentRailNo >= 0)
            {
                if (ambMdl.ParentRailNo >= mMain.mRailMgr.railObjList.Count)
                {
                    mMain.DebugError("AMB No." + amb_index + "のレール番号(" + ambMdl.ParentRailNo + ")不正");
                    isError = true;
                    ambObj.SetActive(false);
                }
                else
                {
                    GameObject railObj = mMain.mRailMgr.railObjList[ambMdl.ParentRailNo];
                    JointMdl railObjJointMdl = railObj.GetComponent<JointMdl>();
                    ambObjJointMdl.transform.parent = railObjJointMdl.BaseJoint.transform;
                    ambMdl.ParentAmbIndex = -1;
                    ambObj.SetActive(true);
                }
                ambObjJointMdl.transform.localPosition = Vector3.zero;
                ambObjJointMdl.transform.localRotation = Quaternion.Euler(Vector3.zero);
            }
            else
            {
                GameObject searchAmbObj = Search(amb_index, parent_index - 1);
                if (searchAmbObj != null)
                {
                    GameObject rootAmbObj = Search(amb_index, -1);
                    if (rootAmbObj != null)
                    {
                        JointMdl rootAmbObjJointMdl = rootAmbObj.GetComponent<JointMdl>();
                        ambObjJointMdl.transform.parent = rootAmbObjJointMdl.transform.parent;
                    }
                    else
                    {
                        mMain.DebugWarning("AMB No." + amb_index + "-" + (child_index + 1) + "ROOTが見つりません。\n入力値：AMB No." + amb_index + "-" + parent_index);
                        ambObj.SetActive(false);
                    }
                    JointMdl searchAmbObjJointMdl = searchAmbObj.GetComponent<JointMdl>();
                    ambObjJointMdl.transform.position = searchAmbObjJointMdl.LastTrans.position;
                    ambObjJointMdl.transform.rotation = searchAmbObjJointMdl.LastTrans.rotation;
                }
                else
                {
                    mMain.DebugWarning("AMB No." + amb_index + "-" + (child_index + 1) + "番目の親が見つりません。\n入力値：AMB No." + amb_index + "-" + parent_index);
                    ambObj.SetActive(false);
                }
            }
            ambObjJointMdl.GetJointTransForm(ambObj);
            ambObjJointMdl.UpdateOffsetPos();
            ambObjJointMdl.UpdateBaseRot(true);
            ambObjJointMdl.UpdateJointDir();
            ambObjJointMdl.old_joint_dir = ambObjJointMdl.JointDir;
            if (ambMdl.isExistKasen)
            {
                GetAmbKasen(ambMdl, ambObjJointMdl);
            }
        }

        public void GetAmbKasen(AmbMdl ambMdl, JointMdl ambJointMdl)
        {
            List<Transform> findChildTransList = ambJointMdl.AllTransformChildren;
            ambMdl.railKasenStartPosList = new Transform[ambMdl.kasenCnt];
            ambMdl.railKasenMdlStartPosList = new Transform[ambMdl.kasenCnt];
            ambMdl.railKasenMdlEndPosList = new Transform[ambMdl.kasenCnt];
            for (int i = 0; i < ambMdl.kasenCnt; i++)
            {
                string rail_name = "R" + (i * 100).ToString("D3");
                List<Transform> railTransList = findChildTransList.FindAll(x => x.name.Contains(rail_name));
                if (railTransList != null)
                {
                    foreach (Transform railTrans in railTransList)
                    {
                        string kasen_name = "L" + (i * 100).ToString("D3");
                        string mdl_start_name = "N00";
                        string mdl_end_name = "N01";
                        Transform[] findKasenTransList = railTrans.GetComponentsInChildren<Transform>(true);
                        ambMdl.railKasenStartPosList[i] = null;
                        ambMdl.railKasenMdlStartPosList[i] = null;
                        ambMdl.railKasenMdlEndPosList[i] = null;
                        for (int j = 0; j < findKasenTransList.Length; j++)
                        {
                            if (findKasenTransList[j].name.Contains(kasen_name))
                            {
                                ambMdl.railKasenStartPosList[i] = findKasenTransList[j];
                            }
                            else if (findKasenTransList[j].name.Contains(mdl_start_name))
                            {
                                ambMdl.railKasenMdlStartPosList[i] = findKasenTransList[j];
                            }
                            else if (findKasenTransList[j].name.Contains(mdl_end_name))
                            {
                                ambMdl.railKasenMdlEndPosList[i] = findKasenTransList[j];
                            }
                        }
                        if (ambMdl.railKasenStartPosList[i] != null && ambMdl.railKasenMdlStartPosList[i] != null && ambMdl.railKasenMdlEndPosList[i] != null)
                        {
                            break;
                        }
                    }
                }
            }
            ambMdl.railKasenEndPosList = new Transform[ambMdl.kasenCnt];
            for (int i = 0; i < ambMdl.kasenCnt; i++)
            {
                string rail_name = "R" + (i * 100 + ambJointMdl.JointList.Length - 1).ToString("D3");
                Transform railTrans = findChildTransList.Find(x => x.name.Equals(rail_name));
                if (railTrans != null)
                {
                    string kasen_name = "L" + (i * 100 + 1).ToString("D3");
                    Transform[] findKasenTransList = railTrans.GetComponentsInChildren<Transform>(true);
                    for (int j = 0; j < findKasenTransList.Length; j++)
                    {
                        if (findKasenTransList[j].name.Contains(kasen_name))
                        {
                            ambMdl.railKasenEndPosList[i] = findKasenTransList[j];
                            break;
                        }
                    }
                }
            }
            AmbRailKasenUpdateDir(ambMdl);
        }

        public void AmbRailKasenUpdateDir(AmbMdl ambMdl)
        {
            for (int i = 0; i < ambMdl.kasenCnt; i++)
            {
                if (ambMdl.railKasenStartPosList[i] == null)
                {
                    continue;
                }
                if (ambMdl.railKasenEndPosList[i] == null)
                {
                    continue;
                }
                ambMdl.railKasenStartPosList[i].LookAt(ambMdl.railKasenEndPosList[i].position);
                if (ambMdl.railKasenMdlStartPosList[i] == null)
                {
                    continue;
                }
                if (ambMdl.railKasenMdlEndPosList[i] == null)
                {
                    continue;
                }
                float posLength = Vector3.Distance(ambMdl.railKasenStartPosList[i].position, ambMdl.railKasenEndPosList[i].position);
                float MdlLength = Vector3.Distance(ambMdl.railKasenMdlStartPosList[i].position, ambMdl.railKasenMdlEndPosList[i].position);
                ambMdl.railKasenStartPosList[i].localScale = new Vector3(1f, 1f, posLength / MdlLength);
            }
        }

        public GameObject Search(int amb_index, int parent_index)
        {
            if (ambObjList != null)
            {
                for (int i = 0; i < ambObjList.Length; i++)
                {
                    if (ambObjList[i] != null)
                    {
                        for (int j = 0; j < ambObjList[i].Count; j++)
                        {
                            if (!(ambObjList[i][j] == null))
                            {
                                GameObject obj = ambObjList[i][j];
                                AmbMdl ambMdl = obj.GetComponent<AmbMdl>();
                                if (ambMdl.ParentAmbNo == amb_index && ambMdl.ParentAmbIndex == parent_index)
                                {
                                    return obj;
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }

        public void SetAmbData(Main mMain)
        {
            try
            {
                isError = false;
                drawAmbCount = 0;
                RemoveAMB();
                ambObjList = new List<GameObject>[mMain.mStageTblMgr.AmbList.Length];
                for (int i = 0; i < mMain.mStageTblMgr.AmbList.Length; i++)
                {
                    if (mMain.mStageTblMgr.AmbList[i] == null)
                    {
                        continue;
                    }
                    int rail_no = mMain.mStageTblMgr.AmbList[i].rail_no;
                    if (rail_no < 0 || rail_no >= mMain.mRailMgr.railObjList.Count)
                    {
                        mMain.DebugWarning("AMB No." + i + "は、存在しないレール番号(" + rail_no +")を指しています");
                        continue;
                    }
                    if (!mMain.mRailMgr.railObjList[rail_no].activeSelf)
                    {
                        mMain.DebugWarning("AMB No." + i + "は、Disabledレール番号(" + rail_no +")を指しています");
                        continue;
                    }

                    for (int j = 0; j < mMain.mStageTblMgr.AmbList[i].datalist.Count; j++)
                    {
                        amb_data ambData = mMain.mStageTblMgr.AmbList[i].datalist[j];
                        int mdl_no = ambData.mdl_no;
                        string mdl_name = mMain.mStageTblMgr.MdlList[mdl_no].mdl_name;
                        mMain.mAMBMgr.CreateAMB(i, j, rail_no, mdl_name, ambData, mMain);
                    }
                }

                mMain.mAMBMgr.UpdateAll(mMain);
            }
            catch (System.Exception e)
            {
                MessageBox.Show("予想外のエラー！", "失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Debug.LogError(e.ToString());
                mMain.DebugError(e.ToString());
            }
        }

        public void SetTexData(Main mMain)
        {
            try
            {
                isError = false;
                if (mMain.mStageTblMgr.StageResDataList != null)
                {
                    stage_res_ab_dict = new Dictionary<string, AssetBundle>();
                    TexList = new List<Texture2D>();
                    List<TextAsset> tempTextAsset = new List<TextAsset>();
                    for (int i = 0; i < mMain.mStageTblMgr.StageResDataList.Length; i++)
                    {
                        string denFile = mMain.mStageTblMgr.StageResDataList[i].ab + mMain.denFileExt;
                        AssetBundle readAssetBundle;
                        if (!stage_res_ab_dict.ContainsKey(denFile))
                        {
                            readAssetBundle = GetAssetBundle(mMain, denFile);
                            stage_res_ab_dict.Add(denFile, readAssetBundle);
                        }
                        else
                        {
                            readAssetBundle = stage_res_ab_dict[denFile];
                        }
                        if (readAssetBundle == null)
                        {
                            mMain.DebugError("StageRes No." + i + "のdenファイル(" + denFile + ")を探せません");
                            isError = true;
                            break;
                        }
                        string filename = mMain.mStageTblMgr.StageResDataList[i].res_name;
                        TextAsset textAsset = readAssetBundle.LoadAsset(filename) as TextAsset;
                        if (textAsset == null)
                        {
                            mMain.DebugError("StageRes No." + i + "のdenファイル(" + denFile + ")のテクスチャ(" + filename + ")探せません");
                            isError = true;
                            break;
                        }
                        Texture2D texture2D = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                        texture2D.LoadImage(textAsset.bytes);
                        texture2D.filterMode = FilterMode.Bilinear;
                        texture2D.wrapMode = TextureWrapMode.Repeat;
                        texture2D.name = filename;
                        TexList.Add(texture2D);
                    }
                }
            }
            catch (System.Exception e)
            {
                MessageBox.Show("予想外のエラー！", "失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Debug.LogError(e.ToString());
                mMain.DebugError(e.ToString());
            }

            try
            {
                if (mMain.mStageTblMgr.TexInfoDataList != null)
                {
                    List<GameObject> searchAmbMdlList;
                    GameObject ambObj;
                    for (int i = 0; i < mMain.mStageTblMgr.TexInfoDataList.Length; i++)
                    {
                        searchAmbMdlList = null;
                        ambObj = null;
                        int ambNo = mMain.mStageTblMgr.TexInfoDataList[i].amb;
                        if (ambNo >= 0)
                        {
                            int amb_child = mMain.mStageTblMgr.TexInfoDataList[i].amb_child;
                            if (ambNo >= ambObjList.Length)
                            {
                                mMain.DebugWarning("SetTexInfo No." + i + "のAMB No." + ambNo + "は存在しません");
                                continue;
                            }
                            searchAmbMdlList = ambObjList[ambNo];
                            if (searchAmbMdlList == null)
                            {
                                mMain.DebugWarning("SetTexInfo No." + i + "のAMB No." + ambNo + "のモデルは存在しません");
                                continue;
                            }
                            if (amb_child >= searchAmbMdlList.Count)
                            {
                                mMain.DebugWarning("SetTexInfo No." + i + "のAMB No." + ambNo + "で、child番号(" + amb_child + ")は存在しません");
                                continue;
                            }
                            ambObj = searchAmbMdlList[amb_child];
                            if (ambObj != null)
                            {
                                JointMdl ambJointModel = ambObj.GetComponent<JointMdl>();
                                int tex_type = mMain.mStageTblMgr.TexInfoDataList[i].tex_type;
                                if (tex_type == 0 || tex_type == 1 || tex_type == 2)
                                {
                                    if (ambObj.GetComponent<Ekihyo>() == null)
                                    {
                                        ambObj.AddComponent<Ekihyo>();
                                    }
                                    Ekihyo ekihyo = ambObj.GetComponent<Ekihyo>();
                                    if (!ekihyo_dict.ContainsKey(ambJointModel.Name.ToUpper()))
                                    {
                                        mMain.DebugWarning("SetTexInfo No." + i + "で指定したAMB(" + ambJointModel.Name + ")は、駅表を設定できません");
                                        continue;
                                    }
                                    int change_index = mMain.mStageTblMgr.TexInfoDataList[i].change_index;
                                    List<string[]> ekihyo_value = ekihyo_dict[ambJointModel.Name.ToUpper()];
                                    if (change_index >= ekihyo_value.Count)
                                    {
                                        mMain.DebugWarning("SetTexInfo No." + i + "で指定したAMB(" + ambJointModel.Name + ")の、対象の駅表モデルindex範囲を超えています(" + change_index + ")");
                                        continue;
                                    }
                                    string[] change_info = ekihyo_value[change_index];
                                    if (change_info[0] == null)
                                    {
                                        continue;
                                    }

                                    List<Transform> childList = SearchChild<Transform>(ambObj.transform);
                                    Transform parentTrans = childList.Find(x => x.name.Equals(change_info[0]));
                                    if (parentTrans == null)
                                    {
                                        mMain.DebugWarning("SetTexInfo No." + i + "で指定したAMB(" + ambJointModel.Name + ")で、" + change_info[0] + "の要素を探せません");
                                        continue;
                                    }

                                    string[] searchNameList = EkihyoSeriesList[int.Parse(change_info[1])];
                                    Transform bodyObjTrans = null;
                                    Transform[] objListTrans = null;
                                    if (ambJointModel.Name.Equals("AMB_TQLight"))
                                    {
                                        bodyObjTrans = parentTrans;
                                        parentTrans = parentTrans.transform.parent;
                                    }
                                    else
                                    {
                                        if (searchNameList.Length > 1)
                                        {
                                            objListTrans = new Transform[2];
                                        }
                                        for (int j = 0; j < parentTrans.childCount; j++)
                                        {
                                            Transform childTrans = parentTrans.GetChild(j);
                                            if (searchNameList[0] != null && childTrans.name.Contains(searchNameList[0]))
                                            {
                                                bodyObjTrans = childTrans;
                                            }
                                            if (searchNameList.Length > 1)
                                            {
                                                if (childTrans.name.Contains(searchNameList[1]))
                                                {
                                                    objListTrans[0] = childTrans;
                                                }
                                                if (childTrans.name.Contains(searchNameList[2]))
                                                {
                                                    objListTrans[1] = childTrans;
                                                }
                                            }
                                        }
                                    }

                                    Texture2D tex = TexList[mMain.mStageTblMgr.TexInfoDataList[i].res_data_index];
                                    if (bodyObjTrans != null)
                                    {
                                        Renderer BodyObj = bodyObjTrans.GetComponent<Renderer>();
                                        Material[] materials = BodyObj.materials;
                                        tex.wrapMode = materials[0].mainTexture.wrapMode;
                                        materials[0].SetTexture("_MainTex", tex);
                                        materials[0].SetTexture("_EmissionMap", tex);
                                        BodyObj.materials = materials;
                                        if (tex_type == 2)
                                        {
                                            if (!ekihyo.is_rot)
                                            {
                                                ekihyo.is_rot = true;
                                                Vector3 local_rot = parentTrans.localRotation.eulerAngles;
                                                local_rot.y = local_rot.y + 180f;
                                                parentTrans.localRotation = Quaternion.Euler(local_rot);
                                            }
                                        }
                                    }
                                    if (objListTrans != null)
                                    {
                                        for (int j = 0; j < objListTrans.Length; j++)
                                        {
                                            Renderer renderObj = objListTrans[j].GetComponent<Renderer>();
                                            if (j == tex_type)
                                            {
                                                renderObj.gameObject.SetActive(true);
                                                int mat_index = mMain.mStageTblMgr.TexInfoDataList[i].mat_index;
                                                Material[] materials = renderObj.materials;
                                                if (mat_index >= materials.Length)
                                                {
                                                    mMain.DebugWarning("SetTexInfo No." + i + "で指定したAMB(" + ambJointModel.Name + ")の、対象のmaterial index範囲を超えています(" + mat_index + ")");
                                                    continue;
                                                }
                                                tex.wrapMode = materials[mat_index].mainTexture.wrapMode;
                                                materials[mat_index].SetTexture("_MainTex", tex);
                                                materials[mat_index].SetTexture("_EmissionMap", tex);
                                                renderObj.materials = materials;
                                            }
                                            else
                                            {
                                                renderObj.gameObject.SetActive(false);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                ClearAssetBundle();
            }
            catch (System.Exception e)
            {
                MessageBox.Show("予想外のエラー！", "失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Debug.LogError(e.ToString());
                mMain.DebugError(e.ToString());
            }
        }

        public AssetBundle GetAssetBundle(Main mMain, string denFile)
        {
            string path = Path.Combine(mMain.defaultPath, denFile);
            if (!File.Exists(path))
            {
                return null;
            }
            return AssetBundle.LoadFromFile(path);
        }

        public void ClearAssetBundle()
        {
            if (stage_res_ab_dict != null)
            {
                List<string> abKeyList = new List<string>(stage_res_ab_dict.Keys);
                for (int i = 0; i < abKeyList.Count; i++)
                {
                    stage_res_ab_dict[abKeyList[i]].Unload(true);
                }
            }
        }
    }
}