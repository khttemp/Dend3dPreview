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

        public void InitPos(int rail_index, rs_rail_list[] rail_list_array, Main mMain)
        {
            GameObject railObj = railObjList[rail_index];
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