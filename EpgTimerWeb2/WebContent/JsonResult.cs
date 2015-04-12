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
