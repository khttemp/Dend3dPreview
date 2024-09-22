using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using UnityEngine;
using System.IO;
using System.Data;
using System.Text;
using ExcelDataReader;

using MainClass;
using RSDecryptClass;

namespace FileReadMgrClass
{
    public class FileReadMgr
    {
        Dictionary<string, string> mdlMap;
        Dictionary<string, string> sheetMap = new Dictionary<string, string>()
        {
            {"駅名", "STCnt:"},
            {"コミックスクリプト", "ComicScript:"},
            {"モデル情報", "MdlCnt:"},
            {"レール情報", "RailCnt:"},
            {"AMB情報", "AmbCnt:"}
        };

        Dictionary<string, string> rsSheetMap = new Dictionary<string, string>()
        {
            {"BGM、配置情報", ""},
            {"駅名位置情報", "STCnt"},
            {"Comic Script、土讃線", "ComicScript"},
            {"smf情報", "MdlCnt"},
            {"レール情報", "RailCnt"},
            {"要素4", "else4"}
        };

        public void Read(Main mMain, bool railFlag, bool ambFlag)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = mMain.defaultPath;
            ofd.FilterIndex = mMain.defaultOfdIndex;
            ofd.Filter =  "stagedata files (*.txt, *.xlsx)|*.txt;*.xlsx";
            //ofd.Filter =  "stagedata files (*.txt, *.xlsx)|*.txt;*.xlsx|RS stagedata (RAIL*.BIN, *.xlsx)|*.BIN;*.xlsx";
            
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string filePath = ofd.FileName;
                string fileExt = Path.GetExtension(filePath).ToLower();
                mMain.defaultPath = Path.GetDirectoryName(filePath);
                mMain.defaultOfdIndex = ofd.FilterIndex;
                string fileContent = string.Empty;
                if (ofd.FilterIndex == 1)
                {
                    if (".txt".Equals(fileExt))
                    {
                        var fileStream = ofd.OpenFile();
                        using (StreamReader reader = new StreamReader(fileStream))
                        {
                            fileContent = reader.ReadToEnd();
                        }
                        StagedataRead(mMain, fileContent, railFlag, ambFlag);
                    }
                    else if (".xlsx".Equals(fileExt))
                    {
                        try
                        {
                            xlsxRead(mMain, filePath, railFlag, ambFlag);
                        }
                        catch (System.Exception ex)
                        {
                            MessageBox.Show("予想外のエラー", "失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            mMain.DebugError(ex.ToString());
                            mMain.SetPanelText("");
                        }
                    }
                }
                else if (ofd.FilterIndex == 2)
                {
                    if (".bin".Equals(fileExt))
                    {
                        if (!mMain.mRSRailMgr.mRSDecryptMgr.Decrypt(filePath, mMain))
                        {
                            MessageBox.Show("レールデータ読込失敗！\nエラーを確認してください", "失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        RSStagedataRead(mMain);
                    }
                    else if (".xlsx".Equals(fileExt))
                    {
                        try
                        {
                            rsXlsxRead(mMain, filePath, railFlag, ambFlag);
                        }
                        catch (System.Exception ex)
                        {
                            MessageBox.Show("予想外のエラー", "失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            mMain.DebugError(ex.ToString());
                            mMain.SetPanelText("");
                        }
                    }
                }
            }
        }

        public void StagedataRead(Main mMain, string fileContent, bool railFlag, bool ambFlag)
        {
            bool result = mMain.mStageTblMgr.Open(fileContent, mMain);
            if (!result)
            {
                MessageBox.Show("読込失敗！\nエラーを確認してください", "失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
                mMain.SetDeactiveButton();
            }
            else
            {
                mMain.SetActiveButton();
                mMain.SetDrawModel(railFlag, ambFlag);
            }
        }

        public void xlsxRead(Main mMain, string filePath, bool railFlag, bool ambFlag)
        {
            mMain.StartCoroutine(_xlsxRead(mMain, filePath, railFlag, ambFlag));
        }

        public IEnumerator _xlsxRead(Main mMain, string filePath, bool railFlag, bool ambFlag)
        {
            mMain.SetPanelText("エクセルファイル\n読み込み中...");
            yield return null;
            List<string> sheetKeyList = new List<string>(sheetMap.Keys);
            bool defaultXlsx = true;
            using (var fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var reader = ExcelReaderFactory.CreateReader(fileStream))
                {
                    DataSet dataSetResult = reader.AsDataSet();

                    mdlMap = new Dictionary<string, string>();
                    List<string> sheetNameList = new List<string>();
                    for (int i = 0; i < dataSetResult.Tables.Count; i++)
                    {
                        sheetNameList.Add(dataSetResult.Tables[i].TableName);
                    }
                    for (int i = 0; i < sheetKeyList.Count; i++)
                    {
                        if (!sheetNameList.Exists(x => x.Equals(sheetKeyList[i])))
                        {
                            defaultXlsx = false;
                            break;
                        }
                    }

                    string fileContent = string.Empty;
                    if (defaultXlsx)
                    {
                        fileContent += GetSheetInfo(mMain, dataSetResult, "駅名", "STCnt:", false);
                        fileContent += GetSheetInfo(mMain, dataSetResult, "コミックスクリプト", "ComicScript:", false);
                        fileContent += GetSheetInfo(mMain, dataSetResult, "モデル情報", "MdlCnt:", false);
                        fileContent += GetSheetInfo(mMain, dataSetResult, "レール情報", "RailCnt:", true);
                        fileContent += GetSheetInfo(mMain, dataSetResult, "AMB情報", "AmbCnt:", true);
                    }
                    else
                    {
                        fileContent += GetSheetInfo(mMain, dataSetResult, "0番目", "STCnt:", false);
                        fileContent += GetSheetInfo(mMain, dataSetResult, "0番目", "ComicScript:", false);
                        fileContent += GetSheetInfo(mMain, dataSetResult, "0番目", "MdlCnt:", false);
                        fileContent += GetSheetInfo(mMain, dataSetResult, "0番目", "RailCnt:", true);
                        fileContent += GetSheetInfo(mMain, dataSetResult, "0番目", "AmbCnt:", true);
                    }

                    mMain.SetPanelText("");
                    yield return null;
                    StagedataRead(mMain, fileContent, railFlag, ambFlag);
                }
            }
        }

        public string GetSheetInfo(Main mMain, DataSet ds, string sheetName, string searchString, bool indexFlag)
        {
            DataTable dt;
            if (sheetName.Equals("0番目"))
            {
                dt = ds.Tables[0];
            }
            else
            {
                dt = ds.Tables[sheetName];
            }
            if (dt == null)
            {
                mMain.DebugError(sheetName + "シートなし！");
                return null;
            }
            StringBuilder sb = new StringBuilder();
            int index = -1;
            int index2 = -1;
            int cnt = -1;
            int rowNum = dt.Rows.Count;
            bool isNewLineFlag = false;
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
                return null;
            }
            else
            {
                sb.Append(dt.Rows[index][0]);
                sb.Append("\t");
                if (!int.TryParse(dt.Rows[index][1].ToString(), out cnt))
                {
                    mMain.DebugError(sheetName + "シートの数が不正(" + dt.Rows[index][1] + ")");
                    return null;
                }
                sb.Append(dt.Rows[index][1]);
            }

            if (indexFlag)
            {
                if (index2 == -1 && cnt != 0)
                {
                    mMain.DebugError(sheetName + "シートの" + searchString + "データ位置を探せません");
                    return null;
                }
                else
                {
                    if (searchString.Equals("AmbCnt:"))
                    {
                        sb.Append("\t");
                        sb.Append(dt.Rows[index][2]);
                    }
                }
            }
            sb.Append("\n");

            if (searchString.Equals("AmbCnt:") && cnt != 0)
            {
                if (System.DBNull.Value.Equals(dt.Rows[index2 + cnt][0]))
                {
                    isNewLineFlag = true;
                }
                else
                {
                    int val = -1;
                    if (int.TryParse(dt.Rows[index2 + cnt][0].ToString(), out val))
                    {
                        if (cnt != val + 1)
                        {
                            isNewLineFlag = true;
                        }
                        else
                        {
                            isNewLineFlag = false;
                        }
                    }
                    else
                    {
                        isNewLineFlag = true;
                    }
                }
            }

            if (isNewLineFlag)
            {
                cnt = rowNum - index2 - 1;
            }

            int colNum = dt.Columns.Count;
            int ambData = -1;
            int curData = -1;
            bool isChildFlag = false;
            for (int i = 0; i < cnt; i++)
            {
                if (isNewLineFlag)
                {
                    if (!isChildFlag)
                    {
                        for (int j = 0; j < colNum; j++)
                        {
                            if (System.DBNull.Value.Equals(dt.Rows[index2 + 1 + i][j]))
                            {
                                sb.Append("\n");
                                break;
                            }
                            string s = dt.Rows[index2 + 1 + i][j].ToString();
                            // ambData
                            if (j == 3)
                            {
                                if (!int.TryParse(s, out ambData))
                                {
                                    ambData = 0;
                                }
                                curData = 1;
                            }
                            // mdl_no
                            if (j == 4)
                            {
                                if (mdlMap.ContainsKey(s))
                                {
                                    s = mdlMap[s];
                                }
                            }

                            sb.Append(s);
                            if (j < colNum - 1)
                            {
                                sb.Append("\t");
                            }
                        }
                    }
                    else
                    {
                        for (int j = 0; j < colNum; j++)
                        {
                            if (j < 4)
                            {
                                continue;
                            }
                            if (System.DBNull.Value.Equals(dt.Rows[index2 + 1 + i][j]))
                            {
                                sb.Append("\n");
                                break;
                            }
                            string s = dt.Rows[index2 + 1 + i][j].ToString();
                            // mdl_no
                            if (j == 4)
                            {
                                if (mdlMap.ContainsKey(s))
                                {
                                    s = mdlMap[s];
                                }
                            }

                            sb.Append(s);
                            if (j < colNum - 1)
                            {
                                sb.Append("\t");
                            }
                        }
                    }

                    if (curData < ambData)
                    {
                        sb.Append("\t");
                        isChildFlag = true;
                        curData++;
                    }
                    else
                    {
                        sb.Append("\n");
                        isChildFlag = false;
                        ambData = -1;
                        curData = -1;
                    }
                }
                else
                {
                    for (int j = 0; j < colNum; j++)
                    {
                        int idx = index + 1 + i;
                        if ("RailCnt:".Equals(searchString) || "AmbCnt:".Equals(searchString))
                        {
                            idx = index2 + 1 + i;
                        }
                        if (idx >= dt.Rows.Count)
                        {
                            mMain.DebugError("入力した" + searchString + "の数字は、データの行範囲(" + dt.Rows.Count + ")を超えます");
                            return null;
                        }
                        if (System.DBNull.Value.Equals(dt.Rows[idx][j]))
                        {
                            sb.Append("\n");
                            break;
                        }
                        string s = dt.Rows[idx][j].ToString();

                        if (searchString.Equals("MdlCnt:"))
                        {
                            if (j == 1)
                            {
                                if (!mdlMap.ContainsKey(s))
                                {
                                    mdlMap.Add(s, i.ToString());
                                }
                            }
                            else if (j == 2 || j == 3)
                            {
                                if (s.Contains("0x"))
                                {
                                    s = s.Substring(2);
                                }
                            }
                        }
                        else if (searchString.Equals("RailCnt:"))
                        {
                            // mdl_no, kasenchu_no
                            if (j == 9 || j == 10)
                            {
                                if (mdlMap.ContainsKey(s))
                                {
                                    s = mdlMap[s];
                                }
                            }
                            else if (j >= 12 && j <= 15)
                            {
                                if (s.Contains("0x"))
                                {
                                    s = s.Substring(2);
                                }
                            }
                        }
                        else if (searchString.Equals("AmbCnt:"))
                        {
                            // mdl_no
                            if (j % 13 == 4)
                            {
                                if (mdlMap.ContainsKey(s))
                                {
                                    s = mdlMap[s];
                                }
                            }
                        }

                        sb.Append(s);
                        if (j < colNum - 1)
                        {
                            sb.Append("\t");
                        }
                        else
                        {
                            sb.Append("\n");
                        }
                    }
                }
            }
            sb.Append("\n");
            return sb.ToString();
        }

        public void rsXlsxRead(Main mMain, string filePath, bool railFlag, bool ambFlag)
        {
            mMain.StartCoroutine(_rsXlsxRead(mMain, filePath, railFlag, ambFlag));
        }

        public IEnumerator _rsXlsxRead(Main mMain, string filePath, bool railFlag, bool ambFlag)
        {
            mMain.SetPanelText("RSのエクセルファイル\n読み込み中...");
            yield return null;
            List<string> sheetKeyList = new List<string>(rsSheetMap.Keys);
            bool defaultXlsx = true;
            using (var fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var reader = ExcelReaderFactory.CreateReader(fileStream))
                {
                    DataSet dataSetResult = reader.AsDataSet();

                    List<string> sheetNameList = new List<string>();
                    for (int i = 0; i < dataSetResult.Tables.Count; i++)
                    {
                        sheetNameList.Add(dataSetResult.Tables[i].TableName);
                    }
                    for (int i = 0; i < sheetKeyList.Count; i++)
                    {
                        if (!sheetNameList.Exists(x => x.Equals(sheetKeyList[i])))
                        {
                            MessageBox.Show(sheetKeyList[i] + "シートなし", "失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            defaultXlsx = false;
                            break;
                        }
                    }

                    if (!defaultXlsx)
                    {
                        mMain.SetPanelText("");
                        yield break;
                    }

                    DataTable dt = dataSetResult.Tables["BGM、配置情報"];
                    string header = dt.Rows[0][0].ToString();
                    if ("DEND_MAP_VER0400".Equals(header))
                    {
                        mMain.mRSRailMgr.mRSDecryptMgr.ouhukuFlag = true;
                    }
                    else if ("DEND_MAP_VER0300".Equals(header))
                    {
                        mMain.mRSRailMgr.mRSDecryptMgr.ouhukuFlag = false;
                    }
                    else
                    {
                        MessageBox.Show("シートのヘッダー情報不正", "失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        mMain.SetPanelText("");
                        yield break;
                    }

                    mMain.mRSRailMgr.mRSDecryptMgr.rsMdlMap = new Dictionary<string, string>();
                    mMain.mRSRailMgr.mRSDecryptMgr.Remove();
                    if (!mMain.mRSRailMgr.mRSDecryptMgr.XlsxRead(mMain, dataSetResult, "駅名位置情報", "STCnt", false))
                    {
                        MessageBox.Show("エクセル読込失敗！\nエラーを確認してください", "失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        mMain.SetPanelText("");
                        yield break;
                    }
                    if (!mMain.mRSRailMgr.mRSDecryptMgr.XlsxRead(mMain, dataSetResult, "Comic Script、土讃線", "ComicScript", false))
                    {
                        MessageBox.Show("エクセル読込失敗！\nエラーを確認してください", "失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        mMain.SetPanelText("");
                        yield break;
                    }
                    if (!mMain.mRSRailMgr.mRSDecryptMgr.XlsxRead(mMain, dataSetResult, "smf情報", "MdlCnt", false))
                    {
                        MessageBox.Show("エクセル読込失敗！\nエラーを確認してください", "失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        mMain.SetPanelText("");
                        yield break;
                    }
                    if (!mMain.mRSRailMgr.mRSDecryptMgr.XlsxRead(mMain, dataSetResult, "レール情報", "RailCnt", true))
                    {
                        MessageBox.Show("エクセル読込失敗！\nエラーを確認してください", "失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        mMain.SetPanelText("");
                        yield break;
                    }
                    if (!mMain.mRSRailMgr.mRSDecryptMgr.XlsxRead(mMain, dataSetResult, "要素4", "else4", false))
                    {
                        MessageBox.Show("エクセル読込失敗！\nエラーを確認してください", "失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        mMain.SetPanelText("");
                        yield break;
                    }

                    mMain.SetPanelText("");
                    yield return null;
                    RSStagedataRead(mMain);
                }
            }
        }

        public void RSStagedataRead(Main mMain)
        {
            mMain.SetRSDrawModel();
        }
    }
}