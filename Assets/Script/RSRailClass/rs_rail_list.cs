using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using RSRailDataListClass;

namespace RSRailListClass
{
    public class rs_rail_list
    {
        public int prev_rail;
        public int block;
        public Vector3 offsetpos;
        public Vector3 offsetrot;
        public Vector3 dir;
        public int mdl_no;
        public int kasenchu_mdl_no;
        public float per;
        public uint flg;
        public rs_rail_data_list[] r;
        public rs_rail_data_list[] rev_r;
    }
}