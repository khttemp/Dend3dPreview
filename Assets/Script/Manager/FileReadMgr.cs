﻿using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using UnityEngine;
using System.IO;
using System.Data;
using System.Text;
using Aspose.Cells;

using MainClass;
using JointMdlClass;

namespace FileReadMgrClass
{
    public class FileReadMgr
    {
        Dictionary<string, string> mdlMap;

        public void Read(Main mMain, bool railFlag, bool ambFlag)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = mMain.defaultPath;
            ofd.Filter =  "stagedata files (*.txt, *.xlsx)|*.txt;*.xlsx";
            
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string filePath = ofd.FileName;
                string fileExt = Path.GetExtension(filePath);
                mMain.defaultPath = Path.GetDirectoryName(filePath);
                string fileContent = string.Empty;
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
            Workbook workbook = new Workbook(filePath);
            mdlMap = new Dictionary<string, string>();
            string fileContent = string.Empty;
            fileContent += GetSheetInfo(mMain, workbook, "駅名", "STCnt:", false);
            fileContent += GetSheetInfo(mMain, workbook, "コミックスクリプト", "ComicScript:", false);
            fileContent += GetSheetInfo(mMain, workbook, "モデル情報", "MdlCnt:", false);
            fileContent += GetSheetInfo(mMain, workbook, "レール情報", "RailCnt:", true);
            fileContent += GetSheetInfo(mMain, workbook, "AMB情報", "AmbCnt:", true);
            mMain.SetPanelText("");
            yield return null;
            StagedataRead(mMain, fileContent, railFlag, ambFlag);
        }

        public string GetSheetInfo(Main mMain, Workbook workbook, string sheetName, string searchString, bool indexFlag)
        {
            Worksheet worksheet = workbook.Worksheets[sheetName];
            if (worksheet == null)
            {
                mMain.DebugError(sheetName + "シートなし！");
                return null;
            }
            StringBuilder sb = new StringBuilder();
            DataTable dt = worksheet.Cells.ExportDataTableAsString(0, 0, worksheet.Cells.MaxDataRow + 1, worksheet.Cells.MaxDataColumn + 1, false);
            int index = -1;
            int index2 = -1;
            int cnt = -1;
            int rowNum = dt.Rows.Count;
            bool isNewLineFlag = false;
            for (int i = 0; i < rowNum; i++)
            {
                if (index == -1 && searchString.Equals(dt.Rows[i][0]))
                {
                    index = i;
                    if (indexFlag)
                    {
                        continue;
                    }
                    break;
                }
                if (indexFlag)
                {
                    if ("index".Equals(dt.Rows[i][0]))
                    {
                        index2 = i;
                        break;
                    }
                    else if ("0".Equals(dt.Rows[i][0]))
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
                if (!int.TryParse((string) dt.Rows[index][1], out cnt))
                {
                    mMain.DebugError(sheetName + "シートの数が不正(" + dt.Rows[index][1] + ")");
                    return null;
                }
                sb.Append(dt.Rows[index][1]);
            }

            if (indexFlag)
            {
                if (index2 == -1)
                {
                    mMain.DebugError(sheetName + "シートの、データ位置を探せません");
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

            if (searchString.Equals("AmbCnt:"))
            {
                if (System.DBNull.Value.Equals(dt.Rows[index2 + cnt][0]))
                {
                    isNewLineFlag = true;
                }
                else
                {
                    int val = -1;
                    if (int.TryParse((string)dt.Rows[index2 + cnt][0], out val))
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
                            string s = (string) dt.Rows[index2 + 1 + i][j];
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
                            string s = (string) dt.Rows[index2 + 1 + i][j];
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
                        if (System.DBNull.Value.Equals(dt.Rows[idx][j]))
                        {
                            sb.Append("\n");
                            break;
                        }
                        string s = (string) dt.Rows[idx][j];

                        if (searchString.Equals("MdlCnt:"))
                        {
                            if (j == 1)
                            {
                                if (!mdlMap.ContainsKey(s))
                                {
                                    mdlMap.Add(s, i.ToString());
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
    }
}