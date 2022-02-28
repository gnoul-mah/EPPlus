﻿/*************************************************************************************************
  Required Notice: Copyright (C) EPPlus Software AB. 
  This software is licensed under PolyForm Noncommercial License 1.0.0 
  and may only be used for noncommercial purposes 
  https://polyformproject.org/licenses/noncommercial/1.0.0/

  A commercial license to use this software can be purchased at https://epplussoftware.com
 *************************************************************************************************
  Date               Author                       Change
 *************************************************************************************************
  01/17/2022         EPPlus Software AB           ExcelTable Html Export
 *************************************************************************************************/
using OfficeOpenXml.Drawing;
using OfficeOpenXml.Drawing.Interfaces;
using System;
using System.Collections.Generic;

namespace OfficeOpenXml.Export.HtmlExport
{
    internal class HtmlImage
    {
        public ExcelPicture Picture { get; set; }
        public int FromRow { get; set; }
        public int FromRowOff { get; set; }
        public int ToRow { get; set; }
        public int ToRowOff { get; set; }
        public int FromColumn { get; set; }
        public int FromColumnOff { get; set; }
        public int ToColumn { get; set; }
        public int ToColumnOff { get; set; }
    }
    /// <summary>
    /// Baseclass for html exporters
    /// </summary>
    public abstract partial class HtmlExporterBase
    {
        internal const string TableClass = "epplus-table";
        internal List<string> _datatypes = new List<string>();
        internal List<int> _columns = new List<int>();
        internal List<HtmlImage> _rangePictures = null;
        internal void LoadRangeImages(ExcelRangeBase range)
        {
            if (_rangePictures != null)
            {
                return;
            }
            _rangePictures= new List<HtmlImage>(); 
            //Render in-cell images.
            foreach (var d in range.Worksheet.Drawings)
            {
                if (d is ExcelPicture p)
                {
                    p.GetFromBounds(out int fromRow, out int fromRowOff, out int fromCol, out int fromColOff);
                    p.GetToBounds(out int toRow, out int toRowOff, out int toCol, out int toColOff);

                    _rangePictures.Add(new HtmlImage() 
                    { 
                        Picture = p,
                        FromRow=fromRow,
                        FromRowOff = fromRowOff,
                        FromColumn = fromCol,
                        FromColumnOff = fromColOff,
                        ToRow = toRow,
                        ToRowOff = toRowOff,
                        ToColumn = toCol,
                        ToColumnOff = toColOff
                    });
                }
            }
        }
        internal HtmlImage GetImage(int row, int col)
        {
            if (_rangePictures == null) return null;
            foreach (var p in _rangePictures)
            {
                if (p.FromRow == row - 1 && p.FromColumn == col - 1)
                {
                    return p;
                }
            }
            return null;
        }
        internal static void AddImage(EpplusHtmlWriter writer, HtmlExportSettings settings, HtmlImage image, object value)
        {
            if (image != null)
            {
                var name = HtmlWriterBase.GetPictureName(image);
                string imageName = HtmlWriterBase.GetCssClassName(image.Picture.Name, ((IPictureContainer)image.Picture).ImageHash);
                writer.AddAttribute("class", $"{settings.StyleClassPrefix}image-{name} {settings.StyleClassPrefix}image-prop-{imageName}");
                writer.RenderBeginTag("img", true);
                if (value == null || value.ToString() == "")
                {
                    writer.RenderBeginTag("p");
                    writer.Write("&nbsp;");
                    writer.RenderEndTag();
                }
            }
        }
        internal void AddRowHeightStyle(EpplusHtmlWriter writer, ExcelRangeBase range, int row, string styleClassPrefix)
        {
            var r = range.Worksheet._values.GetValue(row, 0);
            if (r._value is RowInternal rowInternal)
            {
                if (rowInternal.Height != range.Worksheet.DefaultRowHeight)
                {
                    var ht = range.Worksheet.GetRowHeight(row);
                    writer.AddAttribute("style", $"height:{ht}px");
                }
            }
            writer.AddAttribute("class", $"{styleClassPrefix}drh"); //Default row height
        }
        internal void SetColumnGroup(EpplusHtmlWriter writer, ExcelRangeBase _range, HtmlExportSettings settings)
        {
            var ws = _range.Worksheet;
            writer.RenderBeginTag("colgroup");
            writer.ApplyFormatIncreaseIndent(settings.Minify);
            var mdw = _range.Worksheet.Workbook.MaxFontWidth;
            var defColWidth = ExcelColumn.ColumnWidthToPixels(Convert.ToDecimal(ws.DefaultColWidth), mdw);
            foreach (var c in _columns)
            {
                if (settings.SetColumnWidth)
                {
                    double width = ws.GetColumnWidthPixels(c - 1, mdw);
                    if (width == defColWidth)
                    {
                        writer.AddAttribute("class", $"{settings.StyleClassPrefix}dcw");
                    }
                    else
                    {
                        writer.AddAttribute("style", $"width:{width}px");
                    }
                }
                if (settings.HorizontalAlignmentWhenGeneral == eHtmlGeneralAlignmentHandling.ColumnDataType)
                {
                    writer.AddAttribute("class", $"{TableClass}-ar");
                }
                writer.AddAttribute("span", "1");
                writer.RenderBeginTag("col", true);
                writer.ApplyFormat(settings.Minify);
            }
            writer.Indent--;
            writer.RenderEndTag();
            writer.ApplyFormat(settings.Minify);
        }

        internal bool HandleHiddenRow(EpplusHtmlWriter writer, ExcelWorksheet ws, HtmlExportSettings Settings, ref int row)
        {
            if (Settings.HiddenRows != eHiddenState.Include)
            {
                var r = ws.Row(row);
                if (r.Hidden || r.Height == 0)
                {
                    if (Settings.HiddenRows == eHiddenState.IncludeButHide)
                    {
                        writer.AddAttribute("class", $"{Settings.StyleClassPrefix}hidden");
                    }
                    else
                    {
                        row++;
                        return true;
                    }
                }
            }

            return false;
        }

        internal static string GetClassName(string className)
        {
            className = className.Trim().Replace(" ", "-");
            var newClassName = "";
            for (int i = 0; i < className.Length; i++)
            {
                var c = className[i];
                if (i == 0)
                {
                    if (c == '-' || (c >= '0' && c <= '9'))
                    {
                        newClassName = "_";
                    }
                }

                if ((c >= '0' && c <= '9') ||
                   (c >= 'a' && c <= 'z') ||
                   (c >= 'A' && c <= 'Z') ||
                    c >= 0x00A0)
                {
                    newClassName += c;
                }
            }
            return string.IsNullOrEmpty(newClassName) ? "EmptyClass" : newClassName;
        }
    }
}