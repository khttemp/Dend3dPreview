using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using JointMdlClass;
using RailDataListClass;

namespace RailListClass
{
    public class rail_list
    {
        public int prev_rail;
        public int block;
        public Vector3 offsetpos;
        public Vector3 dir;
        public int mdl_no;
        public int kasenchu_mdl_no;
        public float per;
        public uint flg;
        public rail_data_list[] r;
    }
}