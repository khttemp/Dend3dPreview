using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using UnityEngine;

using MainClass;
using RailMdlClass;
using RSMdlMgrClass;
using RSDecryptClass;
using RSRailListClass;

using JointMdlClass;

namespace RSRailMgrClass
{
    public class RSRailMgr : MonoBehaviour
    {
        public RSDecryptMgr mRSDecryptMgr = new RSDecryptMgr();
        public RSMdlMgr mRSMdlMgr = new RSMdlMgr();
        public List<GameObject> railObjList = new List<GameObject>();
        public int search_rail_index;

        public void RemoveRail()
        {
            for (int i = 0; i < railObjList.Count; i++)
            {
                if (!(railObjList[i] == null))
                {
                    UnityEngine.Object.DestroyImmediate(railObjList[i]);
                }
            }
            railObjList.Clear();
        }

        public bool CreateRSRail(int rail_index, string mdl_name, Main mMain)
        {
            GameObject railObj = mRSMdlMgr.MdlCreate(mdl_name, "rail", mMain);
            if (railObj == null) {
                return false;
            }
            else if (railObj.name.Equals("EmptyObj"))
            {
                railObj.name = railObj.name + "_(" + rail_index + ")";
                railObjList.Add(railObj);
                return true;
            }
            railObj.AddComponent<JointMdl>();
            JointMdl railObjJointMdl = railObj.GetComponent<JointMdl>();
            railObjJointMdl.Name = railObj.name;

            railObj.AddComponent<RailMdl>();
            RailMdl railMdl = railObj.GetComponent<RailMdl>();
            railMdl.railNum = rail_index;
            railMdl.railCheck = false;

            railObj.name = mdl_name + "_(" + rail_index + ")";
            railObjList.Add(railObj);
            return true;
        }

        public void GetModelInfo(int rail_index)
        {
            GameObject railObj = railObjList[rail_index];
            if (railObj.name.Contains("EmptyObj"))
            {
                return;
            }
            JointMdl railObjJointMdl = railObj.GetComponent<JointMdl>();
            railObjJointMdl.GetJointTransForm(railObj);
        }

        public void Init(int rail_index, rs_rail_list[] rail_list_array, Main mMain)
        {
            AutoSet(rail_index, mRSDecryptMgr.RSRailList[rail_index], mMain);
            InitPos(rail_index, mRSDecryptMgr.RSRailList, mMain);
        }

        public void AutoSet(int rail_index, rs_rail_list r, Main mMain)
        {
            GameObject railObj = railObjList[rail_index];
            if (railObj.name.Contains("EmptyObj"))
            {
                return;
            }
            JointMdl railObjJointMdl = railObj.GetComponent<JointMdl>();
            railObjJointMdl.BasePos = r.offsetpos;
            railObjJointMdl.JointDir = r.dir;
            railObjJointMdl.old_joint_dir = Vector3.zero;
            railObjJointMdl.UpdateJointDir();
            railObjJointMdl.UpdateOffsetPos();
            railObjJointMdl.UpdateBaseRot();
            railObjJointMdl.BaseJoint.localScale = new Vector3(1f, 1f, r.per);
            if (rail_index == 0)
            {
                railObjJointMdl.InitPosFlg = true;
            }
            RailMdl railMdl = railObj.GetComponent<RailMdl>();
            GetRailCnt(railMdl, railObjJointMdl, rail_index);
            GetKasen(railMdl, railObjJointMdl, rail_index);
        }

        public void GetRailCnt(RailMdl railMdl, JointMdl railObjJointMdl, int rail_index)
        {
            railMdl.railCnt = 0;
            for (int i = 0; i < 4; i++)
            {
                string rail_name = "R" + (i * 100).ToString("D3");
                Transform findTransform = railObjJointMdl.AllTransformChildren.Find(x => x.name.ToUpper().Equals(rail_name));
                if (findTransform != null)
                {
                    railMdl.railCnt++;
                }
                else
                {
                    break;
                }
            }
            railMdl.railTransformList = new Transform[railMdl.railCnt, railObjJointMdl.JointList.Length];
            railMdl.prevRail = new List<List<int[]>>();
            railMdl.nextRail = new List<List<int[]>>();
            for (int i = 0; i < railMdl.railCnt; i++)
            {
                railMdl.prevRail.Add(new List<int[]>());
                railMdl.nextRail.Add(new List<int[]>());
            }
            for (int i = 0; i < railMdl.railCnt; i++)
		    {
                for (int j = 0; j < railObjJointMdl.JointList.Length; j++)
                {
                    string rail_name = "R" + (i * 100 + j).ToString("D3");
                    for (int k = 0; k < railObjJointMdl.JointList[j].childCount; k++)
                    {
                        Transform findTransform = railObjJointMdl.JointList[j].GetChild(k);
                        if (findTransform.name.Contains(rail_name))
                        {
                            railMdl.railTransformList[i, j] = findTransform;
                            break;
                        }
                    }
                }
            }
        }

        public void GetKasen(RailMdl railMdl, JointMdl railObjJointMdl, int rail_index)
        {
            List<Transform> findChildTransList = railObjJointMdl.AllTransformChildren;
            railMdl.railKasenStartPosList = new Transform[railMdl.railCnt];
            railMdl.railKasenMdlStartPosList = new Transform[railMdl.railCnt];
            railMdl.railKasenMdlEndPosList = new Transform[railMdl.railCnt];
            for (int i = 0; i < railMdl.railCnt; i++)
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
                        railMdl.railKasenStartPosList[i] = null;
                        railMdl.railKasenMdlStartPosList[i] = null;
                        railMdl.railKasenMdlEndPosList[i] = null;
                        for (int j = 0; j < findKasenTransList.Length; j++)
                        {
                            if (findKasenTransList[j].name.Contains(kasen_name))
                            {
                                railMdl.railKasenStartPosList[i] = findKasenTransList[j];
                            }
                            else if (findKasenTransList[j].name.Contains(mdl_start_name))
                            {
                                railMdl.railKasenMdlStartPosList[i] = findKasenTransList[j];
                            }
                            else if (findKasenTransList[j].name.Contains(mdl_end_name))
                            {
                                railMdl.railKasenMdlEndPosList[i] = findKasenTransList[j];
                            }
                        }
                        if (railMdl.railKasenStartPosList[i] != null && railMdl.railKasenMdlStartPosList[i] != null && railMdl.railKasenMdlEndPosList[i] != null)
                        {
                            break;
                        }
                    }
                }
            }
            railMdl.railKasenEndPosList = new Transform[railMdl.railCnt];
            for (int i = 0; i < railMdl.railCnt; i++)
            {
                string kasen_name = "L" + (i * 100 + 1).ToString("D3");
                int end_index = railMdl.railTransformList.Length / railMdl.railCnt - 1;
                Transform[] findKasenTransList = railMdl.railTransformList[i, end_index].GetComponentsInChildren<Transform>(true);
                for (int j = 0; j < findKasenTransList.Length; j++)
                {
                    if (findKasenTransList[j].name.Contains(kasen_name))
                    {
                        railMdl.railKasenEndPosList[i] = findKasenTransList[j];
                        break;
                    }
                }
            }
            RailKasenUpdateDir(railMdl, rail_index);
        }

        public void RailKasenUpdateDir(RailMdl railMdl, int rail_index)
        {
            for (int i = 0; i < railMdl.railCnt; i++)
            {
                if (railMdl.railKasenStartPosList[i] == null)
                {
                    continue;
                }
                if (railMdl.railKasenEndPosList[i] == null)
                {
                    continue;
                }
                railMdl.railKasenStartPosList[i].LookAt(railMdl.railKasenEndPosList[i].position);
                if (railMdl.railKasenMdlStartPosList[i] == null)
                {
                    continue;
                }
                if (railMdl.railKasenMdlEndPosList[i] == null)
                {
                    continue;
                }
                float posLength = Vector3.Distance(railMdl.railKasenStartPosList[i].position, railMdl.railKasenEndPosList[i].position);
                float MdlLength = Vector3.Distance(railMdl.railKasenMdlStartPosList[i].position, railMdl.railKasenMdlEndPosList[i].position);
                railMdl.railKasenStartPosList[i].localScale = new Vector3(1f, 1f, posLength / MdlLength);
            }
        }

        public void InitPos(int rail_index, rs_rail_list[] rail_list_array, Main mMain)
        {
            GameObject railObj = railObjList[rail_index];
            if (railObj.name.Contains("EmptyObj"))
            {
                return;
            }
            JointMdl railObjJointMdl = railObj.GetComponent<JointMdl>();
            rs_rail_list r = rail_list_array[rail_index];
            if ((r.flg & (1U << 31)) > 0 )
            {
                railObj.SetActive(false);
                return;
            }
            if (r.prev_rail < 0)
            {
                return;
            }
            if (r.prev_rail >= railObjList.Count || rail_index == r.prev_rail)
            {
                mMain.DebugError("No." + rail_index + " prev_rail Err!");
                return;
            }
            GameObject prevRailObj = railObjList[r.prev_rail];
            if (prevRailObj.name.Contains("EmptyObj") || prevRailObj.activeSelf == false)
            {
                railObj.SetActive(false);
                return;
            }
            JointMdl prevRailObjJointMdl = prevRailObj.GetComponent<JointMdl>();
            if (!prevRailObjJointMdl.InitPosFlg)
            {
                Init(r.prev_rail, rail_list_array, mMain);
            }

            RailMdl railMdl = railObj.GetComponent<RailMdl>();
            RailMdl prevRailMdl = prevRailObj.GetComponent<RailMdl>();

            if (prevRailObjJointMdl.LastTrans != null)
            {
                if (railMdl.railCnt != prevRailMdl.railCnt)
                {
                    for (int i = 0; i < r.r.Length; i++)
                    {
                        if (r.r[i].prev.rail == r.prev_rail)
                        {
                            Vector3 offsetPos = r.offsetpos;
                            if (r.r[i].prev.no / 100 == 0)
                            {
                                if (railMdl.railCnt == 1)
                                {
                                    railObjJointMdl.BasePos = new Vector3(-6.5f, offsetPos.y, offsetPos.z);
                                }
                                else if (railMdl.railCnt == 2)
                                {
                                    railObjJointMdl.BasePos = new Vector3(6.5f, offsetPos.y, offsetPos.z);
                                }
                            }
                            else if (r.r[i].prev.no / 100 == 1)
                            {
                                if (railMdl.railCnt == 1)
                                {
                                    railObjJointMdl.BasePos = new Vector3(6.5f, offsetPos.y, offsetPos.z);
                                }
                                else if (railMdl.railCnt == 2)
                                {
                                    railObjJointMdl.BasePos = new Vector3(-6.5f, offsetPos.y, offsetPos.z);
                                }
                            }
                            railObjJointMdl.UpdateOffsetPos();
                            break;
                        }
                    }
                }
                railObjJointMdl.transform.position = prevRailObjJointMdl.LastTrans.transform.position;
                railObjJointMdl.transform.rotation = prevRailObjJointMdl.LastTrans.transform.rotation;
                railObjJointMdl.InitPosFlg = true;
            }
        }

        public void SetRailData(Main mMain)
        {
            try
            {
                RemoveRail();
                for (int i = 0; i < mRSDecryptMgr.RSRailList.Length; i++)
                {
                    int mdl_no = mRSDecryptMgr.RSRailList[i].mdl_no;
                    string mdl_name = mRSDecryptMgr.RSMdlList[mdl_no].mdl_name;
                    if (!CreateRSRail(i, mdl_name, mMain))
                    {
                        break;
                    }
                }
                for (int i = 0; i < railObjList.Count; i++)
                {
                    GetModelInfo(i);
                }
                for (int i = 0; i < railObjList.Count; i++)
                {
                    Init(i, mRSDecryptMgr.RSRailList, mMain);
                }
            }
            catch (System.Exception e)
            {
                MessageBox.Show("予想外のエラー！", "失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Debug.LogError(e.ToString());
                mMain.DebugError(e.ToString());
            }
        }
    }
}