﻿using OfficeOpenXml.Core.CellStore;
using OfficeOpenXml.Export.ToCollection.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OfficeOpenXml.Export.ToCollection
{
    internal class ToCollectionRange
    {
        internal static List<string> GetRangeHeaders(ExcelRangeBase range, string[] headers, int? headerRow)
        {
            List<string> headersList;
            if (headers == null || headers.Length == 0)
            {
                headersList = new List<string>();
                if (headerRow.HasValue == false) return headersList;

                for (int c = range._fromCol; c <= range._toCol; c++)
                {
                    var h = range.Worksheet.Cells[range._fromRow + headerRow.Value, c].Text;
                    if (string.IsNullOrEmpty(h))
                    {
                        throw new InvalidOperationException("Header cells cannot be empty");
                    }
                    if (headersList.Contains(h))
                    {
                        throw new InvalidOperationException($"Header cells must be unique. Value : {h}");
                    }
                    headersList.Add(h);
                }
            }
            else
            {
                headersList = new List<string>(headers);
            }

            return headersList;
        }
        internal static List<T> ToCollection<T>(ExcelRangeBase range, Func<ToCollectionRow, T> setRow, ToCollectionRangeOptions options)
        {
            var ret = new List<T>();
            if (range._toRow < range._fromRow) return null;

            var headers = GetRangeHeaders(range, options.Headers, options.HeaderRow);

            var values = new List<ExcelValue>();
            var row = new ToCollectionRow(headers, range._workbook, options.ConversionFailureStrategy);
            var startRow = options.DataStartRow ?? ((options.HeaderRow ?? -1) + 1);
            for (int r = range._fromRow + startRow; r <= range._toRow; r++)
            {
                for (int c = range._fromCol; c <= range._toCol; c++)
                {
                    values.Add(range.Worksheet.GetCoreValueInner(r, c));
                }
                row._cellValues = values;
                var item = setRow(row);
                if (item != null)
                {
                    ret.Add(item);
                }

                values.Clear();
            }
            return ret;
        }
        #if(!NET35)        
        internal static List<T> ToCollection<T>(ExcelRangeBase range, ToCollectionRangeOptions options)
        {
            var t = typeof(T);
            var h = GetRangeHeaders(range, options.Headers, options.HeaderRow);
            if (h.Count <= 0) throw new InvalidOperationException("No headers specified. Please set a ToCollectionOptions.HeaderRow or ToCollectionOptions.Headers[].");
            var d = ToCollectionAutomap.GetAutomapList<T>(h);
            var l = new List<T>();
            var values = new List<ExcelValue>();
            var startRow = options.DataStartRow ?? ((options.HeaderRow ?? -1) + 1);
            for (int r = range._fromRow + startRow; r <= range._toRow; r++)
            {
                var item = (T)Activator.CreateInstance(t);
                foreach (var m in d)
                {
                    var v = range.Worksheet.GetValueInner(r, m.Item1 + range._fromCol);
                    try
                    {
                        m.Item2.SetValue(item, v);
                    }
                    catch (Exception ex)
                    {
                        if (options.ConversionFailureStrategy == ToCollectionConversionFailureStrategy.Exception)
                        {
                            throw new EPPlusDataTypeConvertionException($"Failure to convert value {v} for index {m.Item1}", ex);
                        }
                        else
                        {
                            m.Item2.SetValue(item, default(T));
                        }
                    }
                }

                l.Add(item);
            }

            return l;
        }
        #endif
    }

}
