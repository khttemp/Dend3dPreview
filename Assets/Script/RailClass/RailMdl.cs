using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using RailDataClass;

namespace RailMdlClass
{
    public class RailMdl : MonoBehaviour
    {
        public GameObject kasenchuObject;
        public int railCnt;
        public Transform[,] railTransformList;
        public SphereCollider[] sColliderList;
        public int[] prevRail;
        public int[] nextRail;
    }
}