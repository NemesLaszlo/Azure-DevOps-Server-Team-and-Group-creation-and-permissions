﻿using System;
using System.Collections.Generic;
using System.Data;
using Team_and_Group_Assist.Domain;
using Excel = Microsoft.Office.Interop.Excel;

namespace Team_and_Group_Assist.Excel_Process
{
    public static  class ExcelDataSetHandler
    {
        /// <summary>
        /// /Reads an excel file and converts it into dataset with each sheet as each table of the dataset
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="headers">If set to true the first row will be considered as headers</param>
        /// <returns></returns>
        public static DataSet DatasetImportFromExcel(string filePath, bool headers = true)
        {
            var _xl = new Excel.Application();
            var wb = _xl.Workbooks.Open(filePath);
            var sheets = wb.Sheets;
            DataSet dataSet = null;
            if (sheets != null && sheets.Count != 0)
            {
                dataSet = new DataSet();
                foreach (var item in sheets)
                {
                    var sheet = (Excel.Worksheet)item;
                    DataTable dt = null;
                    if (sheet != null)
                    {
                        dt = new DataTable();
                        dt.TableName = sheet.Name;
                        int ColumnCount = ((Excel.Range)sheet.UsedRange.Rows[1, Type.Missing]).Columns.Count;
                        int rowCount = ((Excel.Range)sheet.UsedRange.Columns[1, Type.Missing]).Rows.Count;

                        for (int j = 0; j < ColumnCount; j++)
                        {
                            var cell = (Excel.Range)sheet.Cells[1, j + 1];
                            var column = new DataColumn(headers ? cell.Value : string.Empty);
                            dt.Columns.Add(column);
                        }

                        for (int i = 0; i < rowCount; i++)
                        {
                            DataRow r = dt.NewRow();
                            for (int j = 0; j < ColumnCount; j++)
                            {
                                var cell = (Excel.Range)sheet.Cells[i + 1 + (headers ? 1 : 0), j + 1];
                                r[j] = cell.Value;
                            }
                            dt.Rows.Add(r);
                        }

                    }
                    dataSet.Tables.Add(dt);
                }
            }
            _xl.Quit();
            return dataSet;
        }
    }
}
