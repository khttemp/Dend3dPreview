using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Data;
using UnityEngine;

using MainClass;
using RSMdlListClass;
using RSStListClass;
using RSEventListClass;
using RSRailListClass;
using RSRailDataListClass;
using RSRailDataClass;

namespace RSDecryptClass
{
    public class RSDecryptMgr
    {
        public bool ouhukuFlag;
        public string header;
        public rs_mdl_list[] RSMdlList;
        public rs_st_list[] RSStList;
        public rs_event_list[] RSEventList;
        public rs_rail_list[] RSRailList;
        StringBuilder sb = new StringBuilder();
        public Dictionary<string, string> rsMdlMap;

        public void Remove()
        {
            this.RSMdlList = null;
            this.RSStList = null;
            this.RSEventList = null;
            this.RSRailList = null;
        }

        public bool Decrypt(string filePath, Main mMain)
        {
            this.Remove();
            System.GC.Collect();
            using (BinaryReader reader = new BinaryReader(File.OpenRead(filePath)))
            {
                header = "";
                try
                {
                    header = new string(reader.ReadChars(16));
                    if (!"DEND_MAP_VER0300".Equals(header) && !"DEND_MAP_VER0400".Equals(header))
                    {
                        mMain.DebugError("RSレールデータのファイルではありません");
                        return false;
                    }
                }
                catch (System.Exception)
                {
                    mMain.DebugError("RSレールデータのファイルではありません");
                    return false;
                }
                ouhukuFlag = false;
                if ("DEND_MAP_VER0400".Equals(header))
                {
                    ouhukuFlag = true;
                }

                try
                {
                    try
                    {
                        int musicCnt = reader.ReadByte();
                        for (int i = 0; i < musicCnt; i++)
                        {
                            reader.ReadByte();
                        }

                        int trainCnt = reader.ReadByte();
                        for (int i = 0; i < trainCnt; i++)
                        {
                            reader.ReadChars(9);
                        }
                        reader.ReadChars(9);
                        for (int i = 0; i < 2; i++)
                        {
                            reader.ReadChars(9);
                        }

                        reader.ReadByte();
                    }
                    catch (System.Exception)
                    {
                        mMain.DebugError("BGM、配置情報の読込エラー");
                        return false;
                    }

                    try
                    {
                        reader.ReadChars(4);
                        int cnt = reader.ReadByte();
                        for (int i = 0; i < cnt; i++)
                        {
                            reader.ReadChars(11);
                        }

                        int lightCnt = reader.ReadByte();
                        for (int i = 0; i < lightCnt; i++)
                        {
                            int b = reader.ReadByte();
                            reader.ReadChars(b);
                        }

                        int pngCnt = reader.ReadUInt16();
                        for (int i = 0; i < pngCnt; i++)
                        {
                            int b = reader.ReadByte();
                            reader.ReadChars(b);
                        }

                        int stationAmbCnt = reader.ReadUInt16();
                        for (int i = 0; i < stationAmbCnt; i++)
                        {
                            reader.ReadChars(9);
                        }

                        int binCnt = reader.ReadByte();
                        for (int i = 0; i < binCnt; i++)
                        {
                            int b = reader.ReadByte();
                            reader.ReadChars(b);
                        }

                        int binAnimeCnt = reader.ReadByte();
                        for (int i = 0; i < binAnimeCnt; i++)
                        {
                            reader.ReadChars(5);
                        }
                    }
                    catch (System.Exception)
                    {
                        mMain.DebugError("要素1の読込エラー");
                        return false;
                    }

                    try
                    {
                        int smfCnt = reader.ReadByte();
                        this.RSMdlList = new rs_mdl_list[smfCnt];
                        for (int i = 0; i < smfCnt; i++)
                        {
                            this.RSMdlList[i] = new rs_mdl_list();
                            int b = reader.ReadByte();
                            sb.Clear();
                            sb.Append(reader.ReadChars(b));
                            this.RSMdlList[i].mdl_name = sb.ToString();
                            byte flg1 = reader.ReadByte();
                            byte flg2 = reader.ReadByte();
                            this.RSMdlList[i].flg = (uint)(flg1 + (flg2 << 8));
                            this.RSMdlList[i].rail_length = (int)reader.ReadByte();
                            reader.ReadByte();
                            reader.ReadByte();
                            this.RSMdlList[i].kasenchu_mdl = (int)reader.ReadByte();
                            reader.ReadUInt16();
                        }
                    }
                    catch (System.Exception)
                    {
                        mMain.DebugError("smf情報の読込エラー");
                        return false;
                    }

                    try
                    {
                        int stCnt = reader.ReadByte();
                        this.RSStList = new rs_st_list[stCnt];
                        for (int i = 0; i < stCnt; i++)
                        {
                            this.RSStList[i] = new rs_st_list();
                            this.RSStList[i].Index = i;
                            int b = reader.ReadByte();
                            this.RSStList[i].StationName = Encoding.GetEncoding("Shift_JIS").GetString(reader.ReadBytes(b));
                            this.RSStList[i].Type = (int)reader.ReadByte();
                            this.RSStList[i].Rail = reader.ReadInt16();
                            reader.ReadChars(26);
                        }
                    }
                    catch (System.Exception)
                    {
                        mMain.DebugError("駅名位置情報の読込エラー");
                        return false;
                    }

                    try
                    {
                        int cnt = reader.ReadByte();
                        for (int i = 0; i < cnt; i++)
                        {
                            reader.ReadChars(17);
                        }
                    }
                    catch (System.Exception)
                    {
                        mMain.DebugError("要素2の読込エラー");
                        return false;
                    }

                    try
                        {
                        int cpuCnt = reader.ReadByte();
                        for (int i = 0; i < cpuCnt; i++)
                        {
                            reader.ReadChars(20);
                        }
                    }
                    catch (System.Exception)
                    {
                        mMain.DebugError("CPU情報の読込エラー");
                        return false;
                    }

                    try
                    {
                        int comicScriptCnt = reader.ReadByte();
                        this.RSEventList = new rs_event_list[comicScriptCnt];
                        for (int i = 0; i < comicScriptCnt; i++)
                        {
                            this.RSEventList[i] = new rs_event_list();
                            this.RSEventList[i].event_no = reader.ReadInt16();
                            this.RSEventList[i].event_type = (int) reader.ReadByte();
                            this.RSEventList[i].rail_no = reader.ReadInt16();
                        }

                        int dosanCnt = reader.ReadByte();
                        for (int i = 0; i < dosanCnt; i++)
                        {
                            reader.ReadChars(36);
                        }
                    }
                    catch (System.Exception)
                    {
                        mMain.DebugError("Comic Script、土讃線の読込エラー");
                        return false;
                    }

                    try
                    {
                        int railCnt = reader.ReadUInt16();
                        this.RSRailList = new rs_rail_list[railCnt];
                        for (int i = 0; i < railCnt; i++)
                        {
                            this.RSRailList[i] = new rs_rail_list();
                            this.RSRailList[i].prev_rail = reader.ReadInt16();
                            this.RSRailList[i].block = (int) reader.ReadByte();
                            this.RSRailList[i].offsetpos = Vector3.zero;
                            this.RSRailList[i].offsetrot = Vector3.zero;
                            float x = reader.ReadSingle();
                            float y = reader.ReadSingle();
                            float z = reader.ReadSingle();
                            this.RSRailList[i].dir = new Vector3(x, y, z);
                            int mdl_no = (int) reader.ReadByte();
                            if (mdl_no < 0 || mdl_no >= this.RSMdlList.Length)
                            {
                                mMain.DebugError("レールNo." + i + "のモデル番号不正(" + mdl_no +")");
                                return false;
                            }
                            this.RSRailList[i].mdl_no = mdl_no;
                            reader.ReadByte();
                            this.RSRailList[i].kasenchu_mdl_no = (int) reader.ReadByte();
                            this.RSRailList[i].per = reader.ReadSingle();
                            byte flg = reader.ReadByte();
                            byte flg2 = reader.ReadByte();
                            byte flg3 = reader.ReadByte();
                            byte flg4 = reader.ReadByte();
                            this.RSRailList[i].flg = Flg(flg, flg2, flg3, flg4);

                            int num64 = (int) reader.ReadByte();
                            if (num64 <= 0)
                            {
                                mMain.DebugWarning("レールNo." + i + "のrail_dataが0以下：" + num64);
                                break;
                            }
                            this.RSRailList[i].r = new rs_rail_data_list[num64];
                            try
                            {
                                for (int num65 = 0; num65 < num64; num65++)
                                {
                                    this.RSRailList[i].r[num65] = new rs_rail_data_list();
                                    this.RSRailList[i].r[num65].prev = new rs_rail_data();
                                    this.RSRailList[i].r[num65].next = new rs_rail_data();
                                    this.RSRailList[i].r[num65].next.rail = reader.ReadInt16();
                                    this.RSRailList[i].r[num65].next.no = reader.ReadInt16();
                                    this.RSRailList[i].r[num65].prev.rail = reader.ReadInt16();
                                    this.RSRailList[i].r[num65].prev.no = reader.ReadInt16();
                                }
                            }
                            catch (System.Exception)
                            {
                                mMain.DebugWarning("レールNo." + i + "のPER以後のデータが不正");
                            }

                            if (ouhukuFlag)
                            {
                                this.RSRailList[i].rev_r = new rs_rail_data_list[num64];
                                try
                                {
                                    for (int num65 = 0; num65 < num64; num65++)
                                    {
                                        this.RSRailList[i].rev_r[num65] = new rs_rail_data_list();
                                        this.RSRailList[i].rev_r[num65].prev = new rs_rail_data();
                                        this.RSRailList[i].rev_r[num65].next = new rs_rail_data();
                                        this.RSRailList[i].rev_r[num65].next.rail = reader.ReadInt16();
                                        this.RSRailList[i].rev_r[num65].next.no = reader.ReadInt16();
                                        this.RSRailList[i].rev_r[num65].prev.rail = reader.ReadInt16();
                                        this.RSRailList[i].rev_r[num65].prev.no = reader.ReadInt16();
                                    }
                                }
                                catch (System.Exception)
                                {
                                    mMain.DebugWarning("レールNo." + i + "のPER以後のデータが不正");
                                }
                            }
                        }
                    }
                    catch (System.Exception)
                    {
                        mMain.DebugError("レール情報の読込エラー");
                        return false;
                    }

                    try
                    {
                        int cnt = reader.ReadUInt16();
                        for (int i = 0; i < cnt; i++)
                        {
                            reader.ReadUInt16();
                            int endcnt = (int) reader.ReadByte();
                            for (int j = 0; j < endcnt; j++)
                            {
                                reader.ReadChars(8);
                            }
                        }
                    }
                    catch (System.Exception)
                    {
                        mMain.DebugError("要素3の読込エラー");
                        return false;
                    }

                    try
                    {
                        int else4Cnt = reader.ReadUInt16();
                        for (int i = 0; i < else4Cnt; i++)
                        {
                            int railNo = reader.ReadInt16();
                            rs_rail_list mRSRailList = this.RSRailList[railNo];
                            if (mRSRailList.prev_rail != -1)
                            {
                                mMain.DebugError("指定したレール(" + railNo + ")は、prev_railが-1ではありません：" + mRSRailList.prev_rail);
                                return false;
                            }
                            mRSRailList.prev_rail = reader.ReadInt16();
                            float x = reader.ReadSingle();
                            float y = reader.ReadSingle();
                            float z = reader.ReadSingle();
                            mRSRailList.offsetpos = new Vector3(x, y, z);
                            float x2 = reader.ReadSingle();
                            float y2 = reader.ReadSingle();
                            float z2 = reader.ReadSingle();
                            mRSRailList.offsetrot = new Vector3(x2, y2, z2);
                        }
                    }
                    catch (System.Exception)
                    {
                        mMain.DebugError("要素4の読込エラー");
                        return false;
                    }
                }
                catch (System.Exception ex)
                {
                    mMain.DebugError("RSレールデータ読込失敗！（予想外のエラー）");
                    mMain.DebugError(ex.ToString());
                }
                return true;
            }
        }

        uint Flg(byte flg1, byte flg2, byte flg3, byte flg4)
        {
            return (uint)((int)flg1 + ((int)flg2 << 8) + ((int)flg3 << 16) + ((int)flg4 << 24));
        }

        public bool XlsxRead(Main mMain, DataSet ds, string sheetName, string searchString, bool indexFlag)
        {
            DataTable dt;
            dt = ds.Tables[sheetName];
            if (dt == null)
            {
                mMain.DebugError(sheetName + "シートなし！");
                return false;
            }
            int index = -1;
            int index2 = -1;
            int cnt = -1;
            int rowNum = dt.Rows.Count;
            for (int i = 0; i < rowNum; i++)
            {
                if (index == -1 && searchString.Equals(dt.Rows[i][0].ToString()))
                {
                    index = i;
                    if (indexFlag)
                    {
                        continue;
                    }
                    break;
                }
                if (index != -1 && indexFlag)
                {
                    if ("index".Equals(dt.Rows[i][0].ToString()))
                    {
                        index2 = i;
                        break;
                    }
                    else if ("0".Equals(dt.Rows[i][0].ToString()))
                    {
                        index2 = i - 1;
                        break;
                    }
                }
            }
            if (index == -1)
            {
                mMain.DebugError(sheetName + "シートの" + searchString + "なし！");
                return false;
            }
            else
            {
                if (!int.TryParse(dt.Rows[index][1].ToString(), out cnt))
                {
                    mMain.DebugError(sheetName + "シートの数が不正(" + dt.Rows[index][1] + ")");
                    return false;
                }
                else
                {
                    if ("STCnt".Equals(searchString))
                    {
                        this.RSStList = new rs_st_list[cnt];
                    }
                    else if ("ComicScript".Equals(searchString))
                    {
                        this.RSEventList = new rs_event_list[cnt];
                    }
                    else if ("MdlCnt".Equals(searchString))
                    {
                        this.RSMdlList = new rs_mdl_list[cnt];
                    }
                    else if ("RailCnt".Equals(searchString))
                    {
                        this.RSRailList = new rs_rail_list[cnt];
                    }
                }
            }

            if (indexFlag)
            {
                if (index2 == -1 && cnt != 0)
                {
                    mMain.DebugError(sheetName + "シートの" + searchString + "データ位置を探せません");
                    return false;
                }
            }

            int colNum = dt.Columns.Count;
            for (int i = 0; i < cnt; i++)
            {
                int idx = -1;
                try
                {
                    if ("STCnt".Equals(searchString))
                    {
                        this.RSStList[i] = new rs_st_list();
                    }
                    else if ("ComicScript".Equals(searchString))
                    {
                        this.RSEventList[i] = new rs_event_list();
                    }
                    else if ("MdlCnt".Equals(searchString))
                    {
                        this.RSMdlList[i] = new rs_mdl_list();
                    }
                    else if ("RailCnt".Equals(searchString))
                    {
                        this.RSRailList[i] = new rs_rail_list();
                        this.RSRailList[i].offsetpos = Vector3.zero;
                        this.RSRailList[i].offsetrot = Vector3.zero;
                    }

                    float dir_x = 0f;
                    float dir_y = 0f;
                    float dir_z = 0f;
                    int railData = 0;
                    int curRailData = 0;
                    int ouhukuNum = 0;

                    int else4Rail = 0;
                    float pos_x = 0f;
                    float pos_y = 0f;
                    float pos_z = 0f;
                    float rot_x = 0f;
                    float rot_y = 0f;
                    float rot_z = 0f;
                    for (int j = 0; j < colNum; j++)
                    {
                        idx = index + 1 + i;
                        if ("RailCnt".Equals(searchString))
                        {
                            idx = index2 + 1 + i;
                        }
                        if (idx >= dt.Rows.Count)
                        {
                            mMain.DebugError("入力した" + searchString + "の数字は、データの行範囲(" + dt.Rows.Count + ")を超えます");
                            return false;
                        }
                        string s = dt.Rows[idx][j].ToString();

                        if ("STCnt".Equals(searchString))
                        {
                            if (j == 1)
                            {
                                this.RSStList[i].StationName = s;
                            }
                            else
                            {
                                int val = -1;
                                if (!int.TryParse(s, out val))
                                {
                                    mMain.DebugError(sheetName + "シートの" + i + "番目 値が不正(" + s + ")");
                                    return false;
                                }

                                if (j == 0)
                                {
                                    this.RSStList[i].Index = val;
                                }
                                else if (j == 2)
                                {
                                    this.RSStList[i].Type = val;
                                }
                                else if (j == 3)
                                {
                                    this.RSStList[i].Rail = val;
                                }
                            }
                        }
                        else if ("ComicScript".Equals(searchString))
                        {
                            int val = -1;
                            if (!int.TryParse(s, out val))
                            {
                                mMain.DebugError(sheetName + "シートの" + i + "番目 値が不正(" + s + ")");
                                return false;
                            }

                            if (j == 1)
                            {
                                this.RSEventList[i].event_no = val;
                            }
                            else if (j == 2)
                            {
                                this.RSEventList[i].event_type = val;
                            }
                            else if (j == 3)
                            {
                                this.RSEventList[i].rail_no = val;
                            }
                        }
                        else if ("MdlCnt".Equals(searchString))
                        {
                            if (j == 1)
                            {
                                if (!rsMdlMap.ContainsKey(s))
                                {
                                    rsMdlMap.Add(s, i.ToString());
                                }
                                this.RSMdlList[i].mdl_name = s;
                            }
                            else
                            {
                                int val = -1;
                                if (s.Contains("0x") && (j == 2 || j == 3))
                                {
                                    s = s.Substring(2);
                                    val = System.Convert.ToInt32(s, 16);
                                }
                                else
                                {
                                    if (!int.TryParse(s, out val))
                                    {
                                        mMain.DebugError(sheetName + "シートの" + i + "番目 値が不正(" + s + ")");
                                        return false;
                                    }
                                }

                                if (j == 2)
                                {
                                    this.RSMdlList[i].flg = (uint)val;
                                }
                                else if (j == 3)
                                {
                                    this.RSMdlList[i].flg += (uint)(val << 8);
                                }
                                else if (j == 4)
                                {
                                    this.RSMdlList[i].rail_length = val;
                                }
                                else if (j == 7)
                                {
                                    this.RSMdlList[i].kasenchu_mdl = val;
                                }
                            }
                        }
                        else if ("RailCnt".Equals(searchString))
                        {
                            if (j == 6 || j == 8)
                            {
                                if (rsMdlMap.ContainsKey(s))
                                {
                                    s = rsMdlMap[s];
                                }
                                int val = -1;
                                if (!int.TryParse(s, out val))
                                {
                                    mMain.DebugError("レールNo." + i + " 存在しないモデル(" + s + ")");
                                    return false;
                                }

                                if (j == 6)
                                {
                                    if (val < 0 || val >= this.RSMdlList.Length)
                                    {
                                        mMain.DebugError("レールNo." + i + "のモデル番号不正(" + val +")");
                                        return false;
                                    }
                                    this.RSRailList[i].mdl_no = val;
                                }
                                else
                                {
                                    this.RSRailList[i].kasenchu_mdl_no = val;
                                }
                            }
                            else if ((j >= 3 && j <= 5) || j == 9)
                            {
                                float val = 1f;
                                if (!float.TryParse(s, out val))
                                {
                                    mMain.DebugError(sheetName + "シートの" + i + "番目 値が不正(" + s + ")");
                                    return false;
                                }

                                if (j == 3)
                                {
                                    dir_x = val;
                                }
                                else if (j == 4)
                                {
                                    dir_y = val;
                                }
                                else if (j == 5)
                                {
                                    dir_z = val;
                                    this.RSRailList[i].dir = new Vector3(dir_x, dir_y, dir_z);
                                }
                                else if (j == 9)
                                {
                                    this.RSRailList[i].per = val;
                                }                                
                            }
                            else
                            {
                                if (ouhukuFlag)
                                {
                                    if (j >= 15 + 8*railData)
                                    {
                                        continue;
                                    }
                                }
                                else
                                {
                                    if (j >= 15 + 4*railData)
                                    {
                                        continue;
                                    }
                                }

                                int val = -1;
                                if (s.Contains("0x") && (j >= 10 && j <= 13))
                                {
                                    s = s.Substring(2);
                                    val = System.Convert.ToInt32(s, 16);
                                }
                                else
                                {
                                    if (!int.TryParse(s, out val))
                                    {
                                        mMain.DebugError(sheetName + "シートの" + i + "番目 値が不正(" + s + ")");
                                        return false;
                                    }
                                }

                                if (j == 1)
                                {
                                    this.RSRailList[i].prev_rail = val;
                                }
                                else if (j == 2)
                                {
                                    this.RSRailList[i].block = val;
                                }
                                else if (j == 10)
                                {
                                    this.RSRailList[i].flg = (uint)val;
                                }
                                else if (j == 11)
                                {
                                    this.RSRailList[i].flg += (uint)(val << 8);
                                }
                                else if (j == 12)
                                {
                                    this.RSRailList[i].flg += (uint)(val << 16);
                                }
                                else if (j == 13)
                                {
                                    this.RSRailList[i].flg += (uint)(val << 24);
                                }
                                else if (j == 14)
                                {
                                    if (val <= 0)
                                    {
                                        mMain.DebugWarning("レールNo." + i + "のrail_dataが0以下：" + val);
                                        break;
                                    }
                                    railData = val;
                                    this.RSRailList[i].r = new rs_rail_data_list[railData];
                                    for (int num65 = 0; num65 < railData; num65++)
                                    {
                                        this.RSRailList[i].r[num65] = new rs_rail_data_list();
                                        this.RSRailList[i].r[num65].prev = new rs_rail_data();
                                        this.RSRailList[i].r[num65].next = new rs_rail_data();
                                    }
                                    if (ouhukuFlag)
                                    {
                                        this.RSRailList[i].rev_r = new rs_rail_data_list[railData];
                                        for (int num65 = 0; num65 < railData; num65++)
                                        {
                                            this.RSRailList[i].rev_r[num65] = new rs_rail_data_list();
                                            this.RSRailList[i].rev_r[num65].prev = new rs_rail_data();
                                            this.RSRailList[i].rev_r[num65].next = new rs_rail_data();
                                        }
                                        ouhukuNum = 15 + 4*railData;
                                    }
                                }
                                else if (j >= 15)
                                {
                                    if ((ouhukuFlag && j < ouhukuNum) || !ouhukuFlag)
                                    {
                                        if (j % 4 == 3)
                                        {
                                            this.RSRailList[i].r[curRailData].next.rail = val;
                                        }
                                        else if (j % 4 == 0)
                                        {
                                            this.RSRailList[i].r[curRailData].next.no = val;
                                        }
                                        else if (j % 4 == 1)
                                        {
                                            this.RSRailList[i].r[curRailData].prev.rail = val;
                                        }
                                        else if (j % 4 == 2)
                                        {
                                            this.RSRailList[i].r[curRailData].prev.no = val;
                                            curRailData++;
                                        }
                                    }
                                    else if (ouhukuFlag && j >= ouhukuNum)
                                    {
                                        if (j == ouhukuNum)
                                        {
                                            curRailData = 0;
                                        }

                                        if (j % 4 == ouhukuNum % 4)
                                        {
                                            this.RSRailList[i].rev_r[curRailData].next.rail = val;
                                        }
                                        else if (j % 4 == (ouhukuNum + 1) % 4)
                                        {
                                            this.RSRailList[i].rev_r[curRailData].next.no = val;
                                        }
                                        else if (j % 4 == (ouhukuNum + 2) % 4)
                                        {
                                            this.RSRailList[i].rev_r[curRailData].prev.rail = val;
                                        }
                                        else if (j % 4 == (ouhukuNum + 3) % 4)
                                        {
                                            this.RSRailList[i].rev_r[curRailData].prev.no = val;
                                            curRailData++;
                                        }
                                    }
                                }
                            }
                        }
                        else if ("else4".Equals(searchString))
                        {
                            if (j >= 3)
                            {
                                float val = 1f;
                                if (!float.TryParse(s, out val))
                                {
                                    mMain.DebugError(sheetName + "シートの" + i + "番目 値が不正(" + s + ")");
                                    return false;
                                }

                                if (j == 3)
                                {
                                    pos_x = val;
                                }
                                else if (j == 4)
                                {
                                    pos_y = val;
                                }
                                else if (j == 5)
                                {
                                    pos_z = val;
                                    this.RSRailList[else4Rail].offsetpos = new Vector3(pos_x, pos_y, pos_z);
                                }
                                else if (j == 6)
                                {
                                    rot_x = val;
                                }
                                else if (j == 7)
                                {
                                    rot_y = val;
                                }
                                else if (j == 8)
                                {
                                    rot_z = val;
                                    this.RSRailList[else4Rail].offsetrot = new Vector3(rot_x, rot_y, rot_z);
                                }                             
                            }
                            else
                            {
                                int val = -1;
                                if (!int.TryParse(s, out val))
                                {
                                    mMain.DebugError(sheetName + "シートの" + i + "番目 値が不正(" + s + ")");
                                    return false;
                                }

                                if (j == 1)
                                {
                                    else4Rail = val;
                                }
                                else if (j == 2)
                                {
                                    if (this.RSRailList[else4Rail].prev_rail != -1)
                                    {
                                        mMain.DebugError("指定したレール(" + else4Rail + ")は、prev_railが-1ではありません：" + this.RSRailList[else4Rail].prev_rail);
                                        return false;
                                    }
                                    this.RSRailList[else4Rail].prev_rail = val;
                                }
                            }
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    mMain.DebugError(sheetName + "シートの下記のデータ読込中、予想外のエラー");
                    sb.Clear();
                    for (int num = 0; num < dt.Rows[idx].ItemArray.Length; num++)
                    {
                        sb.Append(dt.Rows[idx].ItemArray[num]);
                        if (num < dt.Rows[idx].ItemArray.Length - 1)
                        {
                            sb.Append(", ");
                        }
                    }
                    mMain.DebugError(sb.ToString());
                    mMain.DebugError(ex.ToString());
                    return false;
                }
            }
            return true;
        }
    }
}