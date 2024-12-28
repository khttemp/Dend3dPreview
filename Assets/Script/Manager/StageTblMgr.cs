using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MainClass;
using StListClass;
using EventListClass;
using MdlListClass;
using RailListClass;
using RailDataListClass;
using RailDataClass;
using AmbListClass;
using AmbDataClass;

namespace StageTblMgrClass
{
    public class StageTblMgr
    {
        public st_list[] StList;
        public event_list[] EventList;
        public mdl_list[] MdlList;
        public rail_list[] RailList;
        public amb_list[] AmbList;
        public int railStartIndex;
        public int ambStartIndex;

        public void Remove()
        {
            this.StList = null;
            this.EventList = null;
            this.MdlList = null;
            this.RailList = null;
            this.AmbList = null;
        }

        public bool Open(string t, Main mMain)
        {
            this.Remove();
            System.GC.Collect();
            string[] array = ReadTblLine(t, mMain);
            int num10 = -1;
            int num12 = -1;
            int num15 = -1;
            int num16 = -1;
            int num20 = -1;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].Contains("STCnt:"))
                {
                    num10 = i;
                }
                else if (array[i].Contains("ComicScript:"))
                {
                    num12 = i;
                }
                else if (array[i].Contains("MdlCnt:"))
                {
                    num15 = i;
                }
                else if (array[i].Contains("RailCnt:"))
                {
                    num16 = i;
                }
                else if (array[i].Contains("AmbCnt:"))
                {
                    num20 = i;
                }
                else if (num15 >= 0 && num16 >= 0 && num20 >= 0)
                {
                    break;
                }
            }

            // STCnt
            string[] array31 = null;
            try
            {
                string[] array14 = ReadTbl(array[num10]);
                int stationCnt = int.Parse(array14[1]);
                this.StList = new st_list[stationCnt];
                for (int num39 = 0; num39 < stationCnt; num39++)
                {
                    int num40 = 1;
                    this.StList[num39] = new st_list();
                    array31 = ReadTbl(array[num10 + 1 + num39]);
                    this.StList[num39].Index = int.Parse(array31[num40++]);
                    this.StList[num39].Rail = int.Parse(array31[num40++]);
                    this.StList[num39].Offset = float.Parse(array31[num40++]);
                    if (array31.Length > num40)
                    {
                        this.StList[num39].StationName = array31[num40++];
                    }
                    if (array31.Length > num40)
                    {
                        this.StList[num39].HuriganaJP = array31[num40++];
                    }
                    if (array31.Length > num40)
                    {
                        this.StList[num39].HuriganaEN = array31[num40++];
                    }
                    else
                    {
                        this.StList[num39].HuriganaEN = this.StList[num39].HuriganaJP;
                    }
                }
            }
            catch (System.Exception ex2)
            {
                mMain.DebugError("STCnt読込エラー");
                if (array31 != null)
                {
                    mMain.DebugError("エラーが起きたデータ：[" + string.Join(", ", array31) + "]");
                }
                mMain.DebugError(ex2.ToString());
                return false;
            }

            // ComicScript
            string[] array37 = null;
            try
            {
                string[] array16 = ReadTbl(array[num12]);
                int num35 = int.Parse(array16[1]);
                this.EventList = new event_list[num35];
                for (int num56 = 0; num56 < num35; num56++)
                {
                    int num57 = 1;
                    this.EventList[num56] = new event_list();
                    array37 = ReadTbl(array[num12 + 1 + num56]);
                    this.EventList[num56].event_no = int.Parse(array37[num57++]);
                    this.EventList[num56].event_type = int.Parse(array37[num57++]);
                    this.EventList[num56].rail_no = int.Parse(array37[num57++]);
                    this.EventList[num56].offset = float.Parse(array37[num57++]);
                }
            }
            catch (System.Exception ex2)
            {
                mMain.DebugError("ComicScript読込エラー");
                if (array37 != null)
                {
                    mMain.DebugError("エラーが起きたデータ：[" + string.Join(", ", array37) + "]");
                }
                mMain.DebugError(ex2.ToString());
                return false;
            }

            // MdlList
            string[] array38 = null;
            try
            {
                string[] array19 = ReadTbl(array[num15]);
                int num27 = int.Parse(array19[1]);
                this.MdlList = new mdl_list[num27];
                for (int num58 = 0; num58 < num27; num58++)
                {
                    int num59 = 1;
                    this.MdlList[num58] = new mdl_list();
                    array38 = ReadTbl(array[num15 + 1 + num58]);
                    if (array38.Length == 0)
                    {
                        mMain.DebugError("空のデータ！レール設定番号(" + num27 + ")　実データ(" + num58 + ")");
                        return false;
                    }
                    this.MdlList[num58].mdl_name = array38[num59++];
                    int num60 = int.Parse(array38[num59++]);
                    int num61 = int.Parse(array38[num59++]);
                    this.MdlList[num58].flg = (uint)(num60 + (num61 << 8));
                    this.MdlList[num58].kasenchu_mdl = int.Parse(array38[num59++]);
                }
            }
            catch (System.Exception ex2)
            {
                mMain.DebugError("MdlCnt読込エラー");
                if (array38 != null)
                {
                    mMain.DebugError("エラーが起きたデータ：[" + string.Join(", ", array38) + "]");
                }
                mMain.DebugError(ex2.ToString());
                return false;
            }

            // RailList
            string[] array39 = null;
            try
            {
                string[] array20 = ReadTbl(array[num16]);
                int num28 = int.Parse(array20[1]);
                this.RailList = new rail_list[num28];
                for (int num62 = 0; num62 < num28; num62++)
                {
                    int num63 = 1;
                    this.RailList[num62] = new rail_list();
                    array39 = ReadTbl(array[num16 + 1 + num62]);
                    if (array39.Length == 0)
                    {
                        mMain.DebugError("空のデータ！レール設定番号(" + num28 + ")　実データ(" + num62 + ")");
                        return false;
                    }
                    this.RailList[num62].prev_rail = int.Parse(array39[num63++]);
                    this.RailList[num62].block = int.Parse(array39[num63++]);
                    if (this.RailList[num62].block < 0)
                    {
                        mMain.DebugWarning("レールNo." + num62 + "のBlockの値が不正");
                    }
                    float x = float.Parse(array39[num63++]);
                    float y = float.Parse(array39[num63++]);
                    float z = float.Parse(array39[num63++]);
                    this.RailList[num62].offsetpos = new Vector3(x, y, z);
                    float x2 = float.Parse(array39[num63++]);
                    float y2 = float.Parse(array39[num63++]);
                    float z2 = float.Parse(array39[num63++]);
                    this.RailList[num62].dir = new Vector3(x2, y2, z2);
                    int mdl_no = int.Parse(array39[num63++]);
                    if (mdl_no < 0 || mdl_no >= this.MdlList.Length)
                    {
                        mMain.DebugError("レールNo." + num62 + "のモデル番号不正(" + mdl_no +")");
                        return false;
                    }
                    this.RailList[num62].mdl_no = mdl_no;
                    this.RailList[num62].kasenchu_mdl_no = int.Parse(array39[num63++]);
                    this.RailList[num62].per = float.Parse(array39[num63++]);
                    byte flg = byte.Parse(array39[num63++]);
                    byte flg2 = byte.Parse(array39[num63++]);
                    byte flg3 = byte.Parse(array39[num63++]);
                    byte flg4 = byte.Parse(array39[num63++]);
                    this.RailList[num62].flg = Flg(flg, flg2, flg3, flg4);

                    int num64 = int.Parse(array39[num63++]);
                    if (num64 <= 0)
                    {
                        mMain.DebugWarning("レールNo." + num62 + "のrail_dataが0以下：" + num64);
                    }
                    this.RailList[num62].r = new rail_data_list[num64];
                    try
                    {
                        for (int num65 = 0; num65 < num64; num65++)
                        {
                            this.RailList[num62].r[num65] = new rail_data_list();
                            this.RailList[num62].r[num65].prev = new rail_data();
                            this.RailList[num62].r[num65].next = new rail_data();
                            this.RailList[num62].r[num65].next.rail = int.Parse(array39[num63++]);
                            this.RailList[num62].r[num65].next.no = int.Parse(array39[num63++]);
                            this.RailList[num62].r[num65].prev.rail = int.Parse(array39[num63++]);
                            this.RailList[num62].r[num65].prev.no = int.Parse(array39[num63++]);
                        }
                    }
                    catch (System.Exception)
                    {
                        mMain.DebugWarning("レールNo." + num62 + "のPER以後のデータが不正");
                    }
                    num63++;
                }
            }
            catch (System.Exception ex2)
            {
                mMain.DebugError("RailCnt読込エラー");
                if (array39 != null)
                {
                    mMain.DebugError("エラーが起きたデータ：[" + string.Join(", ", array39) + "]");
                }
                mMain.DebugError(ex2.ToString());
                return false;
            }

            // AmbList
            string[] array43 = null;
            try
            {
                string[] array24 = ReadTbl(array[num20]);
                int num32 = int.Parse(array24[1]);
                int num33 = 0;
                if (array24.Length >= 3)
                {
                    num33 = int.Parse(array24[2]);
                }
                this.AmbList = new amb_list[num32];
                for (int num74 = 0; num74 < num32; num74++)
                {
                    int num75 = 1;
                    try
                    {
                        if (num20 + 1 + num74 >= array.Length)
                        {
                            mMain.DebugError("最後まで読込。AMB設定番号(" + num32 + ")　実データ(" + num74 + ")");
                            return false;
                        }
                        array43 = ReadTbl(array[num20 + 1 + num74]);
                        if (array43.Length == 0)
                        {
                            mMain.DebugError("空のデータ！AMB設定番号(" + num32 + ")　実データ(" + num74 + ")");
                            return false;
                        }
                    }
                    catch (System.Exception ex3)
                    {
                        mMain.DebugError("例外発生");
                        mMain.DebugError(ex3.ToString() + "AMB:" + num74);
                        return false;
                    }
                    this.AmbList[num74] = new amb_list();
                    this.AmbList[num74].rail_no = int.Parse(array43[num75++]);
                    this.AmbList[num74].length = float.Parse(array43[num75++]);
                    int num76 = int.Parse(array43[num75++]);
                    for (int num77 = 0; num77 < num76; num77++)
                    {
                        amb_data amb_data = new amb_data();
                        int mdl_no = int.Parse(array43[num75++]);
                        if (mdl_no < 0 || mdl_no >= this.MdlList.Length)
                        {
                            mMain.DebugError("AMB No." + num74 + "-" + (num77 + 1) + "番目のモデル番号不正(" + mdl_no +")");
                            return false;
                        }
                        amb_data.mdl_no = mdl_no;
                        int parentindex = int.Parse(array43[num75++]);
                        amb_data.parentindex = parentindex;
                        float x3 = float.Parse(array43[num75++]);
                        float y3 = float.Parse(array43[num75++]);
                        float z3 = float.Parse(array43[num75++]);
                        amb_data.offsetpos = new Vector3(x3, y3, z3);
                        float x4 = float.Parse(array43[num75++]);
                        float y4 = float.Parse(array43[num75++]);
                        float z4 = float.Parse(array43[num75++]);
                        amb_data.dir = new Vector3(x4, y4, z4);
                        float x5 = float.Parse(array43[num75++]);
                        float y5 = float.Parse(array43[num75++]);
                        float z5 = float.Parse(array43[num75++]);
                        amb_data.joint_dir = new Vector3(x5, y5, z5);
                        amb_data.per = float.Parse(array43[num75++]);
                        amb_data.size_per = 1f;
                        if (num33 >= 1 && array43.Length > num75)
                        {
                            amb_data.size_per = float.Parse(array43[num75++]);
                        }
                        this.AmbList[num74].datalist.Add(amb_data);
                    }
                    if (num76 > 0)
                    {
                        num75++;
                    }
                }
            }
            catch (System.Exception ex2)
            {
                mMain.DebugError("AmbCnt読込エラー");
                if (array43 != null)
                {
                    mMain.DebugError("エラーが起きたデータ：[" + string.Join(", ", array43) + "]");
                }
                mMain.DebugError(ex2.ToString());
                return false;
            }
            return true;
        }

        string[] ReadTbl(string t)
        {
            if (t == null)
            {
                return null;
            }
            string[] array = t.Split(new char[]
            {
                '\n'
            });
            List<string> list = new List<string>();
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].Length > 1 && !array[i].StartsWith("//"))
                {
                    string[] separator = new string[]
                    {
                        "\r",
                        "\t"
                    };
                    string[] collection = array[i].Split(separator, System.StringSplitOptions.RemoveEmptyEntries);
                    list.AddRange(collection);
                }
            }
            return list.ToArray();
        }

        string[] ReadTblLine(string t, Main mMain)
        {
            if (t == null)
            {
                mMain.DebugError("ReadTblLine Err!!");
                return null;
            }
            string[] separator = new string[]
            {
                "\r",
                "\n"
            };
            string[] array = t.Split(separator, System.StringSplitOptions.RemoveEmptyEntries);
            List<string> list = new List<string>();
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].Length > 1 && !array[i].StartsWith("//"))
                {
                    list.Add(array[i]);
                }
            }
            return list.ToArray();
        }

        uint Flg(byte flg1, byte flg2, byte flg3, byte flg4)
        {
            return (uint)((int)flg1 + ((int)flg2 << 8) + ((int)flg3 << 16) + ((int)flg4 << 24));
        }

        public int getRailDataTxtIndex(int railIndex, string t, string[] originArray, Main mMain)
        {
            int originRailDataIndex = -1;
            try
            {
                string[] separator = new string[]
                {
                    "\r",
                    "\n"
                };
                string[] array = t.Split(separator, System.StringSplitOptions.RemoveEmptyEntries);
                int railCntIndex = -1;
                string railData = null;
                for (int i = 0; i < array.Length; i++)
                {
                    if (array[i].Contains("RailCnt:"))
                    {
                        railCntIndex = i;
                        break;
                    }
                }
                for (int i = railCntIndex + 1; i < array.Length; i++)
                {
                    if (array[i].StartsWith("//"))
                    {
                        continue;
                    }
                    railData = array[i + railIndex];
                    break;
                }

                int originRailCntIndex = -1;
                int originAmbCntIndex = -1;
                for (int i = 0; i < originArray.Length; i++)
                {
                    if (originArray[i].Contains("RailCnt:"))
                    {
                        originRailCntIndex = i;
                    }
                    else if (originArray[i].Contains("AmbCnt:"))
                    {
                        originAmbCntIndex = i;
                    }
                    if (originRailCntIndex >= 0 && originAmbCntIndex >= 0)
                    {
                        break;
                    }
                }

                if (railData == null)
                {
                    mMain.DebugError("レールNo." + railIndex + "のデータを探せません");
                    return originRailDataIndex;
                }
                for (int i = originRailCntIndex + 1; i < originAmbCntIndex; i++)
                {
                    if (railData.Equals(originArray[i]))
                    {
                        originRailDataIndex = i;
                        break;
                    }
                }

                if (originRailDataIndex == -1)
                {
                    mMain.DebugError("レールNo." + railIndex + "のデータのindexを探せません");
                }
                return originRailDataIndex;
            }
            catch (System.Exception ex)
            {
                mMain.DebugError("予想外のエラー");
                mMain.DebugError(ex.ToString());
                return -1;
            }
        }

        public int getAmbDataTxtIndex(int ambNo, string t, string[] originArray, Main mMain)
        {
            int originAmbDataIndex = -1;
            try
            {
                string[] separator = new string[]
                {
                    "\r",
                    "\n"
                };
                string[] array = t.Split(separator, System.StringSplitOptions.RemoveEmptyEntries);
                int ambCntIndex = -1;
                string ambData = null;
                for (int i = 0; i < array.Length; i++)
                {
                    if (array[i].Contains("AmbCnt:"))
                    {
                        ambCntIndex = i;
                        break;
                    }
                }
                for (int i = ambCntIndex + 1; i < array.Length; i++)
                {
                    if (array[i].StartsWith("//"))
                    {
                        continue;
                    }
                    ambData = array[i + ambNo];
                    break;
                }

                int originAmbCntIndex = -1;
                for (int i = 0; i < originArray.Length; i++)
                {
                    if (originArray[i].Contains("AmbCnt:"))
                    {
                        originAmbCntIndex = i;
                        break;
                    }
                }

                if (ambData == null)
                {
                    mMain.DebugError("AMB No." + ambNo + "のデータを探せません");
                    return originAmbDataIndex;
                }

                for (int i = originAmbCntIndex + 1; i < originArray.Length; i++)
                {
                    if (ambData.Equals(originArray[i]))
                    {
                        originAmbDataIndex = i;
                        break;
                    }
                }

                if (originAmbDataIndex == -1)
                {
                    mMain.DebugError("AMB No." + ambNo + "のデータのindexを探せません");
                }
                return originAmbDataIndex;
            }
            catch (System.Exception ex)
            {
                mMain.DebugError("予想外のエラー");
                mMain.DebugError(ex.ToString());
                return -1;
            }
        }
    }
}