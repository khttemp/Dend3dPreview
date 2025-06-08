using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using RailDataClass;

namespace RailMdlClass
{
    public class RailMdl : MonoBehaviour
    {
        public GameObject kasenchuObject;
        public int block;
        public int railNum;
        public bool railCheck;
        public int railCnt;
        public Transform[,] railTransformList;
        public Transform[] railKasenStartPosList;
        public Transform[] railKasenEndPosList;
        public Transform[] railKasenMdlStartPosList;
        public Transform[] railKasenMdlEndPosList;
        public List<List<int[]>> prevRail;
        public List<List<int[]>> nextRail;
    }
}