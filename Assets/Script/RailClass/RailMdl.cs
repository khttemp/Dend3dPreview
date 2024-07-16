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
        public List<List<int[]>> prevRail;
        public List<List<int[]>> nextRail;
    }
}