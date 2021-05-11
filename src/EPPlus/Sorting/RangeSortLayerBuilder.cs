﻿/*************************************************************************************************
  Required Notice: Copyright (C) EPPlus Software AB. 
  This software is licensed under PolyForm Noncommercial License 1.0.0 
  and may only be used for noncommercial purposes 
  https://polyformproject.org/licenses/noncommercial/1.0.0/

  A commercial license to use this software can be purchased at https://epplussoftware.com
 *************************************************************************************************
  Date               Author                       Change
 *************************************************************************************************
  05/7/2021         EPPlus Software AB       EPPlus 5.6
 *************************************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OfficeOpenXml.Sorting
{
    public class RangeSortLayerBuilder
    {
        internal RangeSortLayerBuilder(RangeSortOptions options, RangeSortLayer sortLayer)
        {
            _options = options;
            _sortLayer = sortLayer;
        }

        private readonly RangeSortOptions _options;
        private readonly RangeSortLayer _sortLayer;

        public virtual RangeSortLayer ThenSortBy
        {
            get
            {
                return new RangeSortLayer(_options);
            }
        }

        /// <summary>
        /// Use a custom list for sorting on the current Sort layer.
        /// </summary>
        /// <param name="values">An array of strings defining the sort order</param>
        /// <returns>A <see cref="RangeSortLayerBuilder"/></returns>
        public RangeSortLayerBuilder UsingCustomList(params string[] values)
        {
            _sortLayer.SetCustomList(values);
            return this;
        }
    }
}
