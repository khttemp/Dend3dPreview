using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

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
        Toggle stationBoundsToggle;
        Toggle comicBoundsToggle;
        public List<GameObject> stationBoundsList = new List<GameObject>();
        public List<GameObject> comicBoundsList = new List<GameObject>();

        void Start()
        {
            isError = false;
            isCheckError = false;
            search_rail_index = -1;
            sCollider = gameObject.AddComponent<SphereCollider>();
            sCollider.radius = 0.025f;

            CanvasTr = GameObject.Find("Canvas").transform;
            railCheckToggle = CanvasTr.Find("railCheckToggle").GetComponent<Toggle>();
            stationBoundsToggle = CanvasTr.Find("stationBoundsToggle").GetComponent<Toggle>();
            stationBoundsToggle.onValueChanged.AddListener(StationChange);
            comicBoundsToggle = CanvasTr.Find("comicBoundsToggle").GetComponent<Toggle>();
            comicBoundsToggle.onValueChanged.AddListener(ComicChange);
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

            MeshRenderer[] railMeshList = railObj.transform.GetComponentsInChildren<MeshRenderer>(true);
            if (railMeshList != null)
            {
                for (int i = 0; i < railMeshList.Length; i++)
                {
                    railMeshList[i].lightProbeUsage = LightProbeUsage.Off;
                    railMeshList[i].reflectionProbeUsage = ReflectionProbeUsage.Off;
                    railMeshList[i].shadowCastingMode = ShadowCastingMode.Off;
                    railMeshList[i].receiveShadows = false;
                }
            }
            SkinnedMeshRenderer[] railSkinnedMeshList = railObj.transform.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            if (railSkinnedMeshList != null)
            {
                for (int i = 0; i < railSkinnedMeshList.Length; i++)
                {
                    railSkinnedMeshList[i].lightProbeUsage = LightProbeUsage.Off;
                    railSkinnedMeshList[i].reflectionProbeUsage = ReflectionProbeUsage.Off;
                    railSkinnedMeshList[i].shadowCastingMode = ShadowCastingMode.Off;
                    railSkinnedMeshList[i].receiveShadows = false;
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
                                if (r.r[j].prev.rail >= mMain.mStageTblMgr.RailList.Length)
                                {
                                    mMain.DebugError("レールNo." + rail_index + "の、prevのレール指定が不正(" + r.r[j].prev.rail + ", " + r.r[j].prev.no + ")");
                                    isError = true;
                                    return;
                                }
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
                        if (r.r[j].next.rail < 0 || r.r[j].next.no < 0)
                        {
                            continue;
                        }
                        if (r.r[j].next.rail >= mMain.mStageTblMgr.RailList.Length)
                        {
                            mMain.DebugError("レールNo." + i + "の次のレール指定が不正(" + r.r[j].next.rail + ", " + r.r[j].next.no + ")");
                            isError = true;
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

        public void SetStationBoxCollider(Main mMain)
        {
            stationBoundsList.Clear();
            Vector3 initSize = new Vector3(1f, 1f, 1f);
            Vector3 initCenter = new Vector3(0f, 0f, 0.5f);
            for (int i = 0; i < mMain.mStageTblMgr.StList.Length; i++)
            {
                int railNo = mMain.mStageTblMgr.StList[i].Rail;
                if (railNo < 0 || railNo >= railObjList.Count)
                {
                    continue;
                }
                GameObject railObj = railObjList[railNo];
                GameObject eventObj = new GameObject("st_" + i);
                eventObj.transform.parent = railObj.transform;
                eventObj.transform.localScale = Vector3.one;
                Vector3 offset = Vector3.zero;
                offset.z = mMain.mStageTblMgr.StList[i].Offset / 100f;
                eventObj.transform.localPosition = offset;
                eventObj.transform.localRotation = Quaternion.identity;

                GameObject centerObj = new GameObject("st_center_" + i);
                centerObj.transform.parent = eventObj.transform;
                centerObj.transform.localScale = Vector3.one;
                centerObj.transform.localPosition = new Vector3(0f, 0f, 0.5f);
                centerObj.transform.localRotation = Quaternion.identity;
                BoxCollider stBox = eventObj.AddComponent<BoxCollider>();
                stBox.size = initSize;
                stBox.center = initCenter;

                GameObject boundsObj = new GameObject("st_bounds_" + i);
                boundsObj.transform.parent = centerObj.transform;
                boundsObj.transform.localScale = Vector3.one;
                boundsObj.transform.position = centerObj.transform.position;
                BoxCollider stBoxBounds = boundsObj.AddComponent<BoxCollider>();
                stBoxBounds.size = Vector3.one;
                stationBoundsList.Add(boundsObj);
                bool result = calcBounds(stBoxBounds, boundsObj.transform, i, "駅", mMain);
                if (!result)
                {
                    stBoxBounds.size = Vector3.one;
                }
                else
                {
                    string stName = "駅_" + i;
                    if (mMain.mStageTblMgr.StList[i].StationName != null)
                    {
                        stName += "\n" + mMain.mStageTblMgr.StList[i].StationName;
                    }
                    string textCubePath = "dialogPrefab/TextCube";
                    GameObject textCubeObj = (UnityEngine.Object.Instantiate(Resources.Load(textCubePath)) as GameObject);
                    textCubeObj.name = textCubeObj.name.Replace("(Clone)", "");
                    textCubeObj.transform.parent = centerObj.transform;
                    textCubeObj.transform.position = centerObj.transform.position;
                    textCubeObj.transform.localScale = stBoxBounds.size;
                    TextMesh textMesh = textCubeObj.transform.Find("InputText").GetComponent<TextMesh>();
                    textMesh.text = stName;
                    Renderer cubeRenderer = textCubeObj.GetComponent<Renderer>();
                    cubeRenderer.material.color = new Color(0f, 0.8f, 0f, 0.25f);
                    textCubeObj.SetActive(stationBoundsToggle.isOn);
                    stationBoundsList.Add(textCubeObj);
                }
            }
        }

        public void SetComicScriptBoxCollider(Main mMain)
        {
            comicBoundsList.Clear();
            Vector3 initSize = new Vector3(1f, 1f, 1f);
            Vector3 initCenter = new Vector3(0f, 0f, 0.5f);
            for (int i = 0; i < mMain.mStageTblMgr.EventList.Length; i++)
            {
                int railNo = mMain.mStageTblMgr.EventList[i].rail_no;
                if (railNo < 0 || railNo >= railObjList.Count)
                {
                    continue;
                }
                GameObject railObj = railObjList[railNo];
                GameObject eventObj = new GameObject("comic_" + i);
                eventObj.transform.parent = railObj.transform;
                eventObj.transform.localScale = Vector3.one;
                Vector3 offset = Vector3.zero;
                offset.z = mMain.mStageTblMgr.EventList[i].offset / 100f;
                eventObj.transform.localPosition = offset;
                eventObj.transform.localRotation = Quaternion.identity;

                GameObject centerObj = new GameObject("comic_center_" + i);
                centerObj.transform.parent = eventObj.transform;
                centerObj.transform.localScale = Vector3.one;
                centerObj.transform.localPosition = new Vector3(0f, 0f, 0.5f);
                centerObj.transform.localRotation = Quaternion.identity;
                BoxCollider stBox = eventObj.AddComponent<BoxCollider>();
                stBox.size = initSize;
                stBox.center = initCenter;

                GameObject boundsObj = new GameObject("comic_bounds_" + i);
                boundsObj.transform.parent = centerObj.transform;
                boundsObj.transform.localScale = Vector3.one;
                boundsObj.transform.position = centerObj.transform.position;
                BoxCollider stBoxBounds = boundsObj.AddComponent<BoxCollider>();
                stBoxBounds.size = Vector3.one;
                comicBoundsList.Add(boundsObj);
                bool result = calcBounds(stBoxBounds, boundsObj.transform, i, "スクリプト", mMain);
                if (!result)
                {
                    stBoxBounds.size = Vector3.one;
                }
                else
                {
                    string comicName = "スクリプト\n" + mMain.mStageTblMgr.EventList[i].event_no;
                    string textCubePath = "dialogPrefab/TextCube";
                    GameObject textCubeObj = (UnityEngine.Object.Instantiate(Resources.Load(textCubePath)) as GameObject);
                    textCubeObj.name = textCubeObj.name.Replace("(Clone)", "");
                    textCubeObj.transform.parent = centerObj.transform;
                    textCubeObj.transform.position = centerObj.transform.position;
                    textCubeObj.transform.localScale = stBoxBounds.size;
                    TextMesh textMesh = textCubeObj.transform.Find("InputText").GetComponent<TextMesh>();
                    textMesh.text = comicName;
                    Renderer cubeRenderer = textCubeObj.GetComponent<Renderer>();
                    cubeRenderer.material.color = new Color(0f, 0f, 0.6f, 0.25f);
                    textCubeObj.SetActive(comicBoundsToggle.isOn);
                    comicBoundsList.Add(textCubeObj);
                }
            }
        }

        public bool calcBounds(BoxCollider box, Transform transform, int i, string name, Main mMain)
        {
            float newX = box.size.x;
            float newY = box.size.y;
            float newZ = box.size.z;
            float rotX = transform.localRotation.eulerAngles.x;
            if (rotX > 180)
            {
                rotX = Mathf.Abs(rotX - 360);
            }
            float rotY = Mathf.Abs(transform.localRotation.eulerAngles.y);
            if (rotY > 180)
            {
                rotY = Mathf.Abs(rotY - 360);
            }
            float rotZ = Mathf.Abs(transform.localRotation.eulerAngles.z);
            if (rotZ > 180)
            {
                rotZ = Mathf.Abs(rotZ - 360);
            }
            // rotate Y
            if (rotY <= 90)
            {
                newX = box.size.x * Mathf.Cos(rotY * Mathf.Deg2Rad) + box.size.z * Mathf.Sin(rotY * Mathf.Deg2Rad);
                newZ = box.size.x * Mathf.Sin(rotY * Mathf.Deg2Rad) + box.size.z * Mathf.Cos(rotY * Mathf.Deg2Rad);
            }
            else
            {
                newX = box.size.z * Mathf.Sin(rotY * Mathf.Deg2Rad) - box.size.x * Mathf.Cos(rotY * Mathf.Deg2Rad);
                newZ = box.size.x * Mathf.Sin(rotY * Mathf.Deg2Rad) - box.size.z * Mathf.Cos(rotY * Mathf.Deg2Rad);
            }
            if (newX < 0f || newZ < 0f)
            {
                mMain.DebugWarning(i + "番目の" + name +"のBoxColliderのbounds計算失敗! rotate Y(" + rotY + ")");
                return false;
            }
            box.size = new Vector3(newX, box.size.y, newZ);
            // rotate X
            if (rotX <= 90)
            {
                newY = box.size.y * Mathf.Cos(rotX * Mathf.Deg2Rad) + box.size.z * Mathf.Sin(rotX * Mathf.Deg2Rad);
                newZ = box.size.y * Mathf.Sin(rotX * Mathf.Deg2Rad) + box.size.z * Mathf.Cos(rotX * Mathf.Deg2Rad);
            }
            else
            {
                newY = box.size.z * Mathf.Sin(rotX * Mathf.Deg2Rad) - box.size.y * Mathf.Cos(rotX * Mathf.Deg2Rad);
                newZ = box.size.y * Mathf.Sin(rotX * Mathf.Deg2Rad) - box.size.z * Mathf.Cos(rotX * Mathf.Deg2Rad);
            }
            if (newY < 0f || newZ < 0f)
            {
                mMain.DebugWarning(i + "番目の" + name +"のBoxColliderのbounds計算失敗! rotateX!(" + rotX + ")");
                return false;
            }
            box.size = new Vector3(box.size.x, newY, newZ);
            // rotate Z
            if (rotZ <= 90)
            {
                newX = box.size.x * Mathf.Cos(rotZ * Mathf.Deg2Rad) + box.size.y * Mathf.Sin(rotZ * Mathf.Deg2Rad);
                newY = box.size.x * Mathf.Sin(rotZ * Mathf.Deg2Rad) + box.size.y * Mathf.Cos(rotZ * Mathf.Deg2Rad);
            }
            else
            {
                newX = box.size.y * Mathf.Sin(rotZ * Mathf.Deg2Rad) - box.size.x * Mathf.Cos(rotZ * Mathf.Deg2Rad);
                newY = box.size.x * Mathf.Sin(rotZ * Mathf.Deg2Rad) - box.size.y * Mathf.Cos(rotZ * Mathf.Deg2Rad);
            }
            if (newX < 0f || newY < 0f)
            {
                mMain.DebugWarning(i + "番目の" + name +"のBoxColliderのbounds計算失敗! rotateZ!(" + rotZ + ")");
                return false;
            }
            box.size = new Vector3(newX, newY, box.size.z);
            return true;
        }

        public void StationChange(bool state)
        {
            for (int i = 0; i < stationBoundsList.Count; i++)
            {
                stationBoundsList[i].SetActive(state);
            }
        }

        public void ComicChange(bool state)
        {
            for (int i = 0; i < comicBoundsList.Count; i++)
            {
                comicBoundsList[i].SetActive(state);
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
                SetStationBoxCollider(mMain);
                SetComicScriptBoxCollider(mMain);
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