using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ATA_Tools
{
    public class ATARegisterError
    {
        public bool AMNF;
        public bool TONF;
        public bool ABRT;
        public bool MCR;
        public bool IDNF;
        public bool MC;
        public bool UNC;
        public bool BBK;
        public ATARegisterError()
        {
            setFalse();
        }
        private void setFalse()
        {
            this.AMNF = false;
            this.TONF = false;
            this.ABRT = false;
            this.MCR = false;
            this.IDNF = false;
            this.MC = false;
            this.UNC = false;
            this.BBK = false;
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
        public void setErrorRegister(UInt32 errorVal)
        {
            setFalse();
            string strError = getBinaryResult((byte)errorVal);
            for (int k = 0; k < strError.Length; k++)
            {
                if (k == 0 && strError[k] == '1') this.BBK = true;
                if (k == 1 && strError[k] == '1') this.UNC = true;
                if (k == 2 && strError[k] == '1') this.MC = true;
                if (k == 3 && strError[k] == '1') this.IDNF = true;
                if (k == 4 && strError[k] == '1') this.MCR = true;
                if (k == 5 && strError[k] == '1') this.ABRT = true;
                if (k == 6 && strError[k] == '1') this.TONF = true;
                if (k == 7 && strError[k] == '1') this.AMNF = true;
            }
        }
    }
}