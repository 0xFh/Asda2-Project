/*************************************************************************
 *
 *   file		: WDBRecordConverter.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-01-31 12:35:36 +0100 (to, 31 jan 2008) $
 *   last author	: $LastChangedBy: tobz $
 *   revision		: $Rev: 87 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System.IO;

namespace WCell.Core.WDB
{
    public abstract class WDBRecordConverter<TEntry>
    {
        public abstract TEntry Convert(BinaryReader binReader);
    }
}