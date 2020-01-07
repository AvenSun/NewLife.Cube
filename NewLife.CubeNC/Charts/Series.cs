﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NewLife.Cube.Charts
{
    /// <summary>系列。一组数值以及他们映射成的图</summary>
    public class Series
    {
        #region 属性
        /// <summary>图表类型</summary>
        public String Type { get; set; }
        //public SeriesTypes Type { get; set; }

        /// <summary>名称</summary>
        public String Name { get; set; }

        /// <summary>数据</summary>
        public Object Data { get; set; }

        public Boolean Smooth { get; set; }
        #endregion
    }
}