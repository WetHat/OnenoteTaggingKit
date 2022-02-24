﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WetHatLab.OneNote.TaggingKit.PageBuilder
{
    /// <summary>
    /// Proxy class for OneNote table rows.
    /// </summary>
    public class Row : PageObjectBase
    {
        CellCollection _cells;
        /// <summary>
        /// Initialize a table row proxy instance from an existing `one:Row`element
        /// in a OneNote table.
        /// </summary>
        /// <param name="element">OneNote table row XML element.</param>
        public Row(XElement element) : base(element) {
            _cells = new CellCollection(this);
        }
    }
}
