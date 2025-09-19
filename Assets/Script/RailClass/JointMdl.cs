using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MainClass;

namespace JointMdlClass
{
    public class JointMdl : MonoBehaviour
    {
        public Transform BaseJoint;
        public Transform[] JointList;
        public Transform LastTrans;
        public List<Transform> BuffList;
        public List<Transform> AllTransformChildren;

        public string Name;
        public Vector3 BasePos;
        public Vector3 old_base_pos;
        public Vector3 BaseRot;
        public Vector3 old_base_rot;
        public Vector3 JointDir;
        public Vector3 old_joint_dir;
        public Vector3 One_Dir;
        public Quaternion One_Qua;
        public float LengthPer = 1f;
	    public float old_length_per;
        public bool InitPosFlg = false;

        public void GetJointTransForm(GameObject obj)
        {
            BuffList = new List<Transform>();
            this.FindChildToAddList(obj, obj.transform);
        }

        public void FindChildToAddList(GameObject obj, Transform t)
        {
            AllTransformChildren = SearchChild<Transform>(t);
            // BaseJoint
            for (int i = 0; i < AllTransformChildren.Count; i++)
            {
                string transformName = AllTransformChildren[i].name;
                if (this.Name.ToUpper().Equals("AMB_ST_KAIDAN_D4".ToUpper()) || this.Name.ToUpper().Equals("GroundFlash".ToUpper()))
                {
                    this.BaseJoint = null;
                }
                else if (this.Name.ToUpper().Equals("AMB_1Rail_Den_25_Under".ToUpper()))
                {
                    if (transformName.ToUpper().Equals("J00"))
                    {
                        this.BaseJoint = AllTransformChildren[i];
                    }
                }
                else
                {
                    if (transformName.ToUpper().Equals("BASE"))
                    {
                        this.BaseJoint = AllTransformChildren[i];
                    }
                }
            }
            if (this.BaseJoint == null)
            {
                this.BaseJoint = AllTransformChildren[1];
            }

            // BuffList
            int findCount = 0;
            while (true)
            {
                string searchName = "J" + findCount.ToString("D2");
                Transform findTransform = AllTransformChildren.Find(x => x.name.ToUpper().Equals(searchName));
                if (findTransform == null)
                {
                    break;
                }
                else
                {
                    BuffList.Add(findTransform);
                    findCount++;
                }
            }
            this.BuffList.Sort((Transform obj1, Transform obj2) => string.Compare(obj1.name, obj2.name));
            if (this.BuffList.Count == 0)
            {
                Debug.Log(obj.name + "のボーン情報見つからず");
                return;
            }
            
            this.JointList = new Transform[this.BuffList.Count];
            for (int i = 0; i < this.BuffList.Count; i++)
            {
                this.JointList[i] = this.BuffList[i];
            }
            this.LastTrans = this.JointList[this.BuffList.Count - 1];
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

        public void UpdateJointDir()
        {
            UpdateJointScale();
            this.One_Dir = this.JointDir / (float)this.JointList.Length;
            this.One_Qua = Quaternion.Euler(this.One_Dir);
            for (int i = 0; i < this.JointList.Length - 1; i++)
            {
                if (i == 0 && this.JointList[0] == this.BaseJoint)
                {
                    this.BaseJoint.localRotation = Quaternion.Euler(this.BaseRot + this.One_Dir);
                }
                else
                {
                    this.JointList[i].localRotation = this.One_Qua;
                }
            }
        }

        public void UpdateJointScale()
        {
            if (this.JointList == null || this.JointList.Length < 2)
            {
                return;
            }
            this.JointList[this.JointList.Length - 2].localScale = new Vector3(1f, 1f, this.LengthPer);
            this.old_length_per = this.LengthPer;
        }

        public void UpdateOffsetPos()
        {
            if (this.JointList != null && this.JointList.Length != 0 && (this.BasePos != this.old_base_pos))
            {
                if (this.BaseJoint != null)
                {
                    this.BaseJoint.localPosition = this.BasePos / 100f;
                }
                else
                {
                    this.JointList[0].localPosition = this.BasePos / 100f;
                }
                this.old_base_pos = this.BasePos;
            }
        }

        public void UpdateBaseRot(bool flg = false)
        {
            if (this.JointList != null && this.JointList.Length != 0 && (flg || this.BaseRot != this.old_base_rot))
            {
                if (this.BaseJoint != null)
                {
                    this.BaseJoint.localRotation = Quaternion.Euler(this.BaseRot);
                }
                else
                {
                    this.JointList[0].localRotation = Quaternion.Euler(this.BaseRot);
                }
                this.old_base_rot = this.BaseRot;
            }
        }
    }
}