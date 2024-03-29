﻿/*
 *  EpgTimerWeb2
 *  Copyright (C) 2015  YukiBoard 0X7hT.k8kU <yuki@yukiboard.tk>
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpgTimer
{
    public class JsonResult
    {
        public bool Result { set; get; }
        public string Error { set; get; }
        public object Data { set; get; }
        public JsonResult(object Data, bool Result = true, ErrCode Error = ErrCode.CMD_SUCCESS)
        {
            this.Data = Data;
            this.Result = Result;
            this.Error = Error.ToString();
        }
        public JsonResult(object Data, ErrCode Error = ErrCode.CMD_SUCCESS)
        {
            this.Data = Data;
            this.Result = Error == ErrCode.CMD_SUCCESS;
            this.Error = Error.ToString();
        }
        public JsonResult(object Data)
        {
            this.Data = Data;
            this.Result = true;
            this.Error = ErrCode.CMD_SUCCESS.ToString();
        }
    }
}
