using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using UnityEngine;
using UnityEngine.UI;

using MainClass;
using MdlMgrClass;
using JointMdlClass;
using RailListClass;
using RailMdlClass;
using RailDataClass;

namespace RailMgrClass
{
    public class RailMgr : MonoBehaviour
    {
        public List<GameObject> railObjList = new List<GameObject>();
        public List<GameObject> kasenchuObjList = new List<GameObject>();
        MdlMgr mMdlMgr = new MdlMgr();
        public List<int[]> prevFailList = new List<int[]>();
        public bool isError;
        SphereCollider sCollider;
        RaycastHit hitpos;
        Transform CanvasTr;
        Toggle railCheckToggle;
        public int search_rail_index;
        public bool isCheckError;

        void Start()
        {
            isError = false;
            isCheckError = false;
            search_rail_index = -1;
            sCollider = gameObject.AddComponent<SphereCollider>();
            sCollider.radius = 0.025f;

            CanvasTr = GameObject.Find("Canvas").transform;
            railCheckToggle = CanvasTr.Find("railCheckToggle").GetComponent<Toggle>();
        }

        public void CreateRail(int rail_index, string mdl_name, string kasenchu_mdl_name, Main mMain)
        {
            GameObject railObj = mMdlMgr.MdlCreate(mdl_name, "rail", mMain);
            if (railObj == null) {
                isError = true;
                return;
            }
            railObj.AddComponent<JointMdl>();
            JointMdl railObjJointMdl = railObj.GetComponent<JointMdl>();
            railObjJointMdl.Name = railObj.name;

            railObj.AddComponent<RailMdl>();
            RailMdl railMdl = railObj.GetComponent<RailMdl>();
            railMdl.railNum = rail_index;
            railMdl.railCheck = false;
            GameObject kasenchuObject = null;
            if (kasenchu_mdl_name != null)
            {
                kasenchuObject = mMdlMgr.MdlCreate(kasenchu_mdl_name, "kasenchu", mMain);
                if (kasenchuObject != null)
                {
                    railMdl.kasenchuObject = kasenchuObject;
                    kasenchuObjList.Add(kasenchuObject);
                }
                else
                {
                    isError = true;
                }
            }

            railObj.name = railObj.name + "_(" + rail_index + ")";
            railObjList.Add(railObj);
        }

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

            for (int i = 0; i < kasenchuObjList.Count; i++)
            {
                if (!(kasenchuObjList[i] == null))
                {
                    UnityEngine.Object.DestroyImmediate(kasenchuObjList[i]);
                }
            }
            kasenchuObjList.Clear();
        }

        public void GetModelInfo(int rail_index)
        {
            GameObject railObj = railObjList[rail_index];
            JointMdl railObjJointMdl = railObj.GetComponent<JointMdl>();
            railObjJointMdl.GetJointTransForm(railObj);
        }

        public void AutoSet(int rail_index, rail_list r, Main mMain)
        {
            GameObject railObj = railObjList[rail_index];
            JointMdl railObjJointMdl = railObj.GetComponent<JointMdl>();
            railObjJointMdl.BasePos = r.offsetpos;
            railObjJointMdl.JointDir = r.dir;
            railObjJointMdl.old_joint_dir = Vector3.zero;
            railObjJointMdl.LengthPer = r.per;
            railObjJointMdl.UpdateJointDir();
            railObjJointMdl.UpdateOffsetPos();
            if (rail_index == 0)
            {
                railObjJointMdl.InitPosFlg = true;
            }
            RailMdl railMdl = railObj.GetComponent<RailMdl>();
            GameObject kasenchuObject = railMdl.kasenchuObject;
            if (kasenchuObject != null)
            {
                kasenchuObject.transform.parent = railObjJointMdl.JointList[0];
                kasenchuObject.transform.position = railObjJointMdl.JointList[0].position;
			    kasenchuObject.transform.rotation = Quaternion.Euler(0f, railObjJointMdl.JointList[0].rotation.eulerAngles.y, 0f);
            }
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

        public bool RailLineChk(int now, int rail_data_index, int bone_index, int prev, Main mMain, bool flag=false)
        {
            RailMdl railMdl = railObjList[now].GetComponent<RailMdl>();
            railMdl.railCheck = true;
            sCollider.enabled = true;
            for (int j = 0; j < railMdl.railCnt; j++)
            {
                Transform now_r = null;
                try
                {
                    now_r = railMdl.railTransformList[j, bone_index];
                }
                catch (System.Exception)
                {
                    
                }
                if (now_r == null)
                {
                    mMain.DebugError("レールNo." + now + "のボーン(" + j * 100 + ", " + bone_index + ")エラー！");
                    return false;
                }
                sCollider.center = now_r.position;

                GameObject prevRailObj = railObjList[prev];
                JointMdl prevJointMdl = prevRailObj.GetComponent<JointMdl>();
                RailMdl prevRailMdl = prevRailObj.GetComponent<RailMdl>();
                prevRailMdl.railCheck = true;
                for (int k = 0; k < prevJointMdl.JointList.Length - 1; k++)
                {
                    for (int l = 0; l < prevRailMdl.railCnt; l++)
                    {
                        Transform r = prevRailMdl.railTransformList[l, k];
                        if (r == null)
                        {
                            mMain.DebugError("レールNo." + prev + "のボーン(" + l + ", " + k + ")エラー！");
                            return false;
                        }
                        Transform rNext = prevRailMdl.railTransformList[l, k+1];
                        if (rNext == null)
                        {
                            mMain.DebugError("レールNo." + prev + "のボーン(" + l + ", " + k+1 + ")エラー！");
                            return false;
                        }
                        if (mMain.isAllDebug)
                        {
                            DebugCheckRail(now, prev, sCollider, r, rNext);
                        }
                        if (Physics.Linecast(r.position, rNext.position, out hitpos))
                        {
                            if (CheckLength(now, prev, now_r, r, mMain))
                            {
                                if (mMain.isAllDebug || mMain.isDebug)
                                {
                                    DebugHitRail(now, prev, now_r, r, rNext, flag, hitpos);
                                }
                                bool sameFlag = false;

                                foreach (int[] prevRail in railMdl.prevRail[j])
                                {
                                    if (prevRail[0] == prev && prevRail[1] == (l*100 + k))
                                    {
                                        sameFlag = true;
                                        break;
                                    }
                                }
                                if (!sameFlag)
                                {
                                    railMdl.prevRail[j].Add(new int[] {prev, l * 100 + k});
                                }

                                sameFlag = false;
                                foreach (int[] nextRail in prevRailMdl.nextRail[l])
                                {
                                    if (nextRail[0] == now && nextRail[1] == (j*100))
                                    {
                                        sameFlag = true;
                                        break;
                                    }
                                }
                                if (!sameFlag)
                                {
                                    prevRailMdl.nextRail[l].Add(new int[] {now, j * 100});
                                }
                            }
                        }
                    }
                }
            }
            sCollider.enabled = false;
            return true;
        }

        public void InitPos(int rail_index, rail_list[] rail_list_array, Main mMain)
        {
            GameObject railObj = railObjList[rail_index];
            JointMdl railObjJointMdl = railObj.GetComponent<JointMdl>();
            RailMdl railMdl = railObj.GetComponent<RailMdl>();
            rail_list r = rail_list_array[rail_index];
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
                isError = true;
                return;
            }
            GameObject prevRailObj = railObjList[r.prev_rail];
            JointMdl prevRailObjJointMdl = prevRailObj.GetComponent<JointMdl>();
            if (!prevRailObjJointMdl.InitPosFlg)
            {
                Init(r.prev_rail, rail_list_array, mMain);
            }
            if (prevRailObjJointMdl.LastTrans != null)
            {
                railObjJointMdl.transform.position = prevRailObjJointMdl.LastTrans.transform.position;
                railObjJointMdl.transform.rotation = prevRailObjJointMdl.LastTrans.transform.rotation;
                railObjJointMdl.InitPosFlg = true;

                if (r.r != null)
                {
                    if (r.r.Length != railMdl.railCnt)
                    {
                        mMain.DebugWarning("レールNo." + rail_index + "の、設定したrail_data(" + r.r.Length + ")が、モデル(" + railMdl.railCnt + "個)と違います");
                    }
                    if (railCheckToggle.isOn)
                    {
                        for (int j = 0; j < r.r.Length; j++)
                        {
                            if (r.r[j].prev.rail >= 0)
                            {
                                if (r.r[j].prev.rail != rail_index)
                                {
                                    RailLineChk(rail_index, j, 0, r.r[j].prev.rail, mMain);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void Init(int rail_index, rail_list[] rail_list_array, Main mMain)
        {
            AutoSet(rail_index, mMain.mStageTblMgr.RailList[rail_index], mMain);
            InitPos(rail_index, mMain.mStageTblMgr.RailList, mMain);
        }

        public bool CheckLength(int now, int prev, Transform basePos, Transform pos, Main mMain)
        {
            float dis = Vector3.Distance(basePos.position, pos.position);
            if (dis > 1f)
			{
                mMain.DebugWarning("レールNo." + now + "の" + basePos.name + "と レールNo." + prev + "の" + pos.name + "距離異常！(" + dis + ")");
                return false;
            }
            return true;
        }

        public void RailAllCheck(Main mMain)
        {
            for (int i = 0; i < railObjList.Count; i++)
            {
                rail_list r = mMain.mStageTblMgr.RailList[i];
                GameObject railObj = railObjList[i];
                if (railObj.activeSelf)
                {
                    RailMdl railMdl = railObj.GetComponent<RailMdl>();
                    for (int j = 0; j < r.r.Length; j++)
                    {
                        if (r.r[j].next.rail < 0 || r.r[j].next.no < 0 || r.r[j].next.rail >= mMain.mStageTblMgr.RailList.Length)
                        {
                            continue;
                        }
                        bool result = RailLineChk(r.r[j].next.rail, j, r.r[j].next.no % 100, i, mMain, true);
                        if (!result)
                        {
                            mMain.DebugError("レールNo." + i + "の次のレール指定が不正(" + r.r[j].next.rail + ", " + r.r[j].next.no + ")");
                            isError = true;
                        }
                    }
                }
            }

            for (int i = 0; i < railObjList.Count; i++)
            {
                rail_list r = mMain.mStageTblMgr.RailList[i];
                GameObject railObj = railObjList[i];
                if (railObj.activeSelf)
                {
                    RailMdl railMdl = railObj.GetComponent<RailMdl>();
                    for (int j = 0; j < r.r.Length; j++)
                    {
                        if (railMdl.nextRail.Count <= j)
                        {
                            isCheckError = true;
                            mMain.DebugWarning("レールNo." + i + "の" + j * 100 + "側は、Nextレールが繋がっていません");
                        }
                        else
                        {
                            if (r.r[j].next.rail >= 0 && r.r[j].next.rail < mMain.mStageTblMgr.RailList.Length && railMdl.nextRail.Count > j)
                            {
                                if (railMdl.nextRail[j].Count == 0)
                                {
                                    isCheckError = true;
                                    mMain.DebugWarning("レールNo." + i + "の" + j * 100 + "側は、Nextレールが繋がっていません");
                                }
                            }
                        }
                        if (railMdl.prevRail.Count <= j)
                        {
                            isCheckError = true;
                            mMain.DebugWarning("レールNo." + i + "の" + j * 100 + "側は、prevレールが繋がっていません");
                        }
                        else
                        {
                            if (r.r[j].prev.rail >= 0 && r.r[j].prev.rail < mMain.mStageTblMgr.RailList.Length - 1)
                            {
                                if (railMdl.prevRail[j].Count == 0)
                                {
                                    isCheckError = true;
                                    mMain.DebugWarning("レールNo." + i + "の" + j * 100 + "側は、prevレールが繋がっていません");
                                }
                            }
                        }
                    }
                }
            }
        }

        public void SetRailData(Main mMain)
        {
            try
            {
                isError = false;
                isCheckError = false;
                RemoveRail();
                for (int i = 0; i < mMain.mStageTblMgr.RailList.Length; i++)
                {
                    int mdl_no = mMain.mStageTblMgr.RailList[i].mdl_no;
                    string mdl_name = mMain.mStageTblMgr.MdlList[mdl_no].mdl_name;
                    int kasenchu_mdl_no = mMain.mStageTblMgr.RailList[i].kasenchu_mdl_no;
                    int base_mdl_no = mMain.mStageTblMgr.MdlList[mdl_no].kasenchu_mdl;
                    string kasenchu_name = null;
                    if (kasenchu_mdl_no == 255 || kasenchu_mdl_no == -1)
                    {
                        if (base_mdl_no < 254 && base_mdl_no >= 0)
                        {
                            if (base_mdl_no >= mMain.mStageTblMgr.MdlList.Length)
                            {
                                mMain.DebugError("レールNo." + i + "のデフォルト架線柱(" + kasenchu_mdl_no + ") モデル番号不正(" + base_mdl_no +")");
                                isError = true;
                                return;
                            }
                            kasenchu_name = mMain.mStageTblMgr.MdlList[base_mdl_no].mdl_name;
                        }
                    }
                    else if (kasenchu_mdl_no != 254 && kasenchu_mdl_no != -2)
                    {
                        if (kasenchu_mdl_no >= mMain.mStageTblMgr.MdlList.Length)
                        {
                            mMain.DebugError("レールNo." + i + "の指定した架線柱 モデル番号不正(" + kasenchu_mdl_no +")");
                            isError = true;
                            return;
                        }
                        kasenchu_name = mMain.mStageTblMgr.MdlList[kasenchu_mdl_no].mdl_name;
                    }

                    CreateRail(i, mdl_name, kasenchu_name, mMain);
                }
                for (int i = 0; i < mMain.mStageTblMgr.RailList.Length; i++)
                {
                    GetModelInfo(i);
                }
                for (int i = 0; i < mMain.mStageTblMgr.RailList.Length; i++)
                {
                    Init(i, mMain.mStageTblMgr.RailList, mMain);
                }
                if (railCheckToggle.isOn)
                {
                    RailAllCheck(mMain);
                }
            }
            catch (System.Exception e)
            {
                MessageBox.Show("予想外のエラー！", "失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Debug.LogError(e.ToString());
                mMain.DebugError(e.ToString());
            }
        }

        public void DebugCheckRail(int now, int prev, SphereCollider sCollider, Transform r, Transform rNext)
        {
            Debug.Log(
                "Check...レールNo." + now + "：sCollider ["
                    + sCollider.center.x + ", " + sCollider.center.y + ", " + sCollider.center.z
                    + "] "
                    + "レールNo." + prev + " "
                    + r.name + ", ["
                    + r.position.x + ", " + r.position.y + ", " + r.position.z
                    + "], "
                    + rNext.name + ", ["
                    + rNext.position.x + ", " + rNext.position.y + ", " + rNext.position.z
                    + "]"
            );
        }

        public void DebugHitRail(int now, int prev, Transform now_r, Transform r, Transform rNext, bool flag, RaycastHit hitpos)
        {
            Debug.Log("レールNo." + now + "の" + now_r.name + "は、レールNo." + prev + "の" + rNext.name + "と繋がりました");
        }
    }
}