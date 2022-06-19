using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ATA_Tools
{
    public class ATARegisterStatus
    {
        public bool ERR;
        public bool IDX;
        public bool CORR;
        public bool DRQ;
        public bool DSC;
        public bool DF;
        public bool DRDY;
        public bool BSY;
        public ATARegisterStatus()
        {
            setFalse();
        }
        private void setFalse()
        {
            this.ERR = false;
            this.IDX = false;
            this.CORR = false;
            this.DRQ = false;
            this.DSC = false;
            this.DF = false;
            this.DRDY = false;
            this.BSY = false;
        }
        private string getBinaryResult(int Val)
        {
            string res = Convert.ToString(Val, 2);
            for (int i = res.Length; i < 8; i++)
            {
                res = "0" + res;
            }
            return res;
        }
        public void setStatusRegister(UInt32 statusVal)
        {
            setFalse();
            string strStatus = getBinaryResult((byte)statusVal);
            for (int k = 0; k < strStatus.Length; k++)
            {
                if (k == 7 && strStatus[k] == '1') this.ERR = true;
                if (k == 6 && strStatus[k] == '1') this.IDX = true;
                if (k == 5 && strStatus[k] == '1') this.CORR = true;
                if (k == 4 && strStatus[k] == '1') this.DRQ = true;
                if (k == 3 && strStatus[k] == '1') this.DSC = true;
                if (k == 2 && strStatus[k] == '1') this.DF = true;
                if (k == 1 && strStatus[k] == '1') this.DRDY = true;
                if (k == 0 && strStatus[k] == '1') this.BSY = true;
            }
        }
    }
}
