using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AmbMdlClass
{
    public class AmbMdl : MonoBehaviour
    {
        public int ParentRailNo;
        public int ParentAmbNo;
        public int ParentAmbIndex;
        public int ChildIndex;
        public float KasenChuScale;
        public Transform[] AllTransformChildren;
        public bool isExistKasen;
        public int kasenCnt;
        public Transform[,] railTransformList;
        public Transform[] railKasenStartPosList;
        public Transform[] railKasenEndPosList;
        public Transform[] railKasenMdlStartPosList;
        public Transform[] railKasenMdlEndPosList;
    }
}