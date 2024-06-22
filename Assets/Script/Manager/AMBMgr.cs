using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using UnityEngine;

using MainClass;
using MdlMgrClass;
using JointMdlClass;
using AmbListClass;
using AmbDataClass;
using AmbMdlClass;

namespace AMBMgrClass
{
    public class AMBMgr
    {
        public List<GameObject>[] ambObjList;
        MdlMgr mMdlMgr = new MdlMgr();
        public Dictionary<string, string[]> origin_size_per_dict;
        public Dictionary<string, string[]> size_per_dict;
        public List<string> keyList;

        public AMBMgr()
        {
            origin_size_per_dict = new Dictionary<string, string[]>(){
                {"AMB_BLACK_CHU", new string[]{"AMB_BLACK_CHU", "", "", ""}},
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
                {"AMB_TQ_Kaidan3", new string[]{"st_saku_obj", "", "", ""}},
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
        }

        public bool CreateAMB(int amb_index, int child_index, int rail_no, string mdl_name, amb_data ambData, Main mMain)
        {
            GameObject ambObj = mMdlMgr.MdlCreate(mdl_name, "amb", mMain);
            if (ambObj == null) {
                return false;
            }
            if (rail_no >= mMain.mRailMgr.railObjList.Count)
            {
                mMain.DebugError("AMB No." + amb_index + "は、存在しないレール番号(" + rail_no +")を指しています");
                ambObj.SetActive(false);
                return true;
            }
            if (!mMain.mRailMgr.railObjList[rail_no].activeSelf)
            {
                mMain.DebugError("AMB No." + amb_index + "は、Disabledレール番号(" + rail_no +")を指しています");
                ambObj.SetActive(false);
                return true;
            }
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
            return true;
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
                    ambObj.SetActive(false);
                }
                else
                {
                    GameObject railObj = mMain.mRailMgr.railObjList[ambMdl.ParentRailNo];
                    JointMdl railObjJointMdl = railObj.GetComponent<JointMdl>();
                    ambObjJointMdl.transform.parent = railObjJointMdl.transform;
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
                        mMain.DebugError("AMB No." + amb_index + "-" + (child_index + 1) + "ROOTが見つりません。\n入力値：AMB No." + amb_index + "-" + parent_index);
                        ambObj.SetActive(false);
                    }
                    JointMdl searchAmbObjJointMdl = searchAmbObj.GetComponent<JointMdl>();
                    ambObjJointMdl.transform.position = searchAmbObjJointMdl.LastTrans.position;
                    ambObjJointMdl.transform.rotation = searchAmbObjJointMdl.LastTrans.rotation;
                }
                else
                {
                    mMain.DebugError("AMB No." + amb_index + "-" + (child_index + 1) + "番目の親が見つりません。\n入力値：AMB No." + amb_index + "-" + parent_index);
                    ambObj.SetActive(false);
                }
            }
            ambObjJointMdl.GetJointTransForm(ambObj);
            ambObjJointMdl.UpdateOffsetPos();
            ambObjJointMdl.UpdateBaseRot(true);
            ambObjJointMdl.UpdateJointDir();
            ambObjJointMdl.old_joint_dir = ambObjJointMdl.JointDir;
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
                RemoveAMB();
                ambObjList = new List<GameObject>[mMain.mStageTblMgr.AmbList.Length];
                for (int i = 0; i < mMain.mStageTblMgr.AmbList.Length; i++)
                {
                    if (mMain.mStageTblMgr.AmbList[i] == null)
                    {
                        continue;
                    }
                    int rail_no = mMain.mStageTblMgr.AmbList[i].rail_no;
                    if (rail_no < 0)
                    {
                        mMain.DebugWarning("レール番号不正(" + rail_no +")　No." + i + "のAMBは作成しない");
                        continue;
                    }

                    for (int j = 0; j < mMain.mStageTblMgr.AmbList[i].datalist.Count; j++)
                    {
                        amb_data ambData = mMain.mStageTblMgr.AmbList[i].datalist[j];
                        int mdl_no = ambData.mdl_no;
                        string mdl_name = mMain.mStageTblMgr.MdlList[mdl_no].mdl_name;
                        bool ambResult = mMain.mAMBMgr.CreateAMB(i, j, rail_no, mdl_name, ambData, mMain);
                        if (!ambResult) {
                            MessageBox.Show("No." + i + "：探せないAMBモデル\n" + mdl_name, "失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                }

                mMain.mAMBMgr.UpdateAll(mMain);
            }
            catch (System.Exception e)
            {
                MessageBox.Show("エラー！", "失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Debug.LogError(e.ToString());
                mMain.DebugError(e.ToString());
            }
        }
    }
}