using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using UnityEngine;

using MainClass;
using MdlMgrClass;
using JointMdlClass;
using RailListClass;
using RailMdlClass;
using RailDataClass;

namespace RailMgrClass
{
    public class RailMgr
    {
        public List<GameObject> railObjList = new List<GameObject>();
        public List<GameObject> kasenchuObjList = new List<GameObject>();
        MdlMgr mMdlMgr = new MdlMgr();
        public List<int[]> prevFailList = new List<int[]>();
        public bool isError = false;

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
                    kasenchuObject.SetActive(false);
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
            GetRailCnt(railMdl, railObjJointMdl);
        }

        public void GetRailCnt(RailMdl railMdl, JointMdl railObjJointMdl)
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
            railMdl.prevRail = new int[railMdl.railCnt * 2];
            railMdl.nextRail = new int[railMdl.railCnt * 2];
            railMdl.sColliderList = new SphereCollider[railMdl.railCnt];
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
                            if (j == 0)
                            {
                                findTransform.gameObject.AddComponent<SphereCollider>();
                                SphereCollider sCollider = findTransform.gameObject.GetComponent<SphereCollider>();
                                sCollider.radius = 0.025f;
                                railMdl.sColliderList[i] = sCollider;
                            }
                            break;
                        }
                    }
                }
                for (int j = 0; j < 2; j++)
                {
                    railMdl.prevRail[2*i] = -999;
                    railMdl.prevRail[2*i + 1] = -999;
                    railMdl.nextRail[2*i] = -999;
                    railMdl.nextRail[2*i + 1] = -999;
                }
            }
        }

        public void RailLineChk(int now, int rail_data_index, int bone_index, int prev, RailMdl railMdl, Main mMain, bool flag=false)
        {
            //bool findFlag = false;
            GameObject railObj = railObjList[now];
            for (int j = 0; j < railMdl.railCnt; j++)
            {
                Transform nowTrans = railMdl.railTransformList[j, bone_index];
                if (nowTrans == null)
                {
                    mMain.DebugError("レールNo." + now + "のボーン(" + j * 100 + ", " + bone_index + ")エラー！");
                    return;
                }
                SphereCollider sCollider = railMdl.sColliderList[j];
                sCollider.enabled = true;
                sCollider.transform.position = nowTrans.position;
                
                GameObject prevRailObj = railObjList[prev];
                JointMdl prevJointMdl = prevRailObj.GetComponent<JointMdl>();
                RailMdl prevRailMdl = prevRailObj.GetComponent<RailMdl>();
                for (int k = 0; k < prevJointMdl.JointList.Length - 1; k++)
                {
                    for (int l = 0; l < prevRailMdl.railCnt; l++)
                    {
                        Transform r = prevRailMdl.railTransformList[l, k];
                        if (r == null)
                        {
                            mMain.DebugError("レールNo." + prev + "のボーン(" + l + ", " + k + ")エラー！");
                            return;
                        }
                        Transform rNext = prevRailMdl.railTransformList[l, k+1];
                        if (rNext == null)
                        {
                            mMain.DebugError("レールNo." + prev + "のボーン(" + l + ", " + k+1 + ")エラー！");
                            return;
                        }
                        // Debug.Log("Check...レールNo." + now + "：" + sCollider.transform.name + " [" + sCollider.transform.position.x + ", " + sCollider.transform.position.y + ", " + sCollider.transform.position.z + "] " + r.name + ", " + rNext.name);
                        if (Physics.Linecast(r.position, rNext.position))
                        {
                            // Debug.Log("レールNo." + now + "：" + sCollider.transform.name + ", " + r.name + ", " + rNext.name);
                            //findFlag = true;
                            railMdl.prevRail[2*j] = prev;
                            railMdl.prevRail[2*j + 1] = (l * 100 + k);
                            prevRailMdl.nextRail[2*l] = now;
                            prevRailMdl.nextRail[2*l + 1] = l*100;
                        }
                    }
                }
                sCollider.enabled = false;
            }
            // if (!findFlag)
            // {
            //     if (flag)
            //     {
            //         Debug.LogWarning("Next繋ぎ：レールNo." + prev + "の" + rail_data_index * 100 + "側は、next No." + now + "と繋がっていません！");
            //     }
            //     else
            //     {
            //         Debug.LogWarning("Prev繋ぎ：レールNo." + now + "の" + rail_data_index * 100 + "側は、prev No." + prev + "と繋がっていません！");
            //     }
            // }
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
                        mMain.DebugWarning("レールNo." + rail_index + "のrail_data(" + r.r.Length + ")が、モデル(" + railMdl.railCnt +"個)と違います");
                    }
                    for (int j = 0; j < r.r.Length; j++)
                    {
                        if (r.r[j].prev.rail >= 0)
                        {
                            if (r.r[j].prev.rail != rail_index)
                            {
                                RailLineChk(rail_index, j, 0, r.r[j].prev.rail, railMdl, mMain);
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

        public void SetRailData(Main mMain)
        {
            try
            {
                isError = false;
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
                            kasenchu_name = mMain.mStageTblMgr.MdlList[base_mdl_no].mdl_name;
                        }
                    }
                    else if (kasenchu_mdl_no != 254 && kasenchu_mdl_no != -2)
                    {
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
                // for (int i = 0; i < mMain.mStageTblMgr.RailList.Length; i++)
                // {
                //     if (railObjList[i].activeSelf)
                //     {
                //         RailMdl railMdl = railObjList[i].GetComponent<RailMdl>();
                //         for (int j = 0; j < railMdl.railCnt; j++)
                //         {
                //             if (railMdl.prevRail[2*j] == -999)
                //             {
                //                 rail_list r = mMain.mStageTblMgr.RailList[i];
                //                 for (int k = 0; k < r.r.Length; k++)
                //                 {
                //                     if (r.r[k].next.rail < 0 || r.r[k].next.rail >= mMain.mStageTblMgr.RailList.Length)
                //                     {
                //                         continue;
                //                     }
                //                     Debug.LogWarning("Prev繋ぎ：レールNo." + i + "の" + k * 100 + "側は、prev No." + r.r[k].prev.rail + "と繋がっていません！");
                //                     RailLineChk(r.r[k].next.rail, k, r.r[k].next.no % 100, i, railMdl, mMain, true);
                //                 }
                //             }
                //         }
                //     }
                // }
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