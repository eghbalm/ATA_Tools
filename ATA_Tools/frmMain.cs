/*
ATA_Tools
Created by eghbal mohammadi
Mail : eghbalm1362@gmail.com
Mobile : 09187843531
this app correctly run in 32bit operation system
 */
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace ATA_Tools
{
    public unsafe partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }
        string TempfileName = Path.GetTempPath() + @"ATACommand-Temp";
        FileStream fs;
        public UInt16 baseAddress = 0;
        public class IDEREGS
        {
            public byte Features;
            public byte SectorCount;
            public byte SectorNumber;
            public byte CylinderLow;
            public byte CylinderHigh;
            public byte DriveHead;
            public byte Command;
            public byte Reserved;
        }
        IDEREGS irs = new IDEREGS();
        IDEREGS[] loopir = new IDEREGS[2];
        List<byte> IdentityBuffers = new List<byte>();
        IOPortAccess p;
        bool Monitoring = false;
        Thread trLoop;
        bool trlp = false;
        frmPortIoNumber fpin;

        string loopResult = "";
        private void Form1_Load(object sender, EventArgs e)
        {
            fpin = new frmPortIoNumber(this);
            if (p == null)
            {
                p = new IOPortAccess();
            }
            btnClearBuf_Click(null, null);
            btnMonitor_Click(null, null);
        }

        private void delTempFile()
        {
            try
            {
                File.SetAttributes(TempfileName, FileAttributes.Normal);
            }
            catch { }
            if (File.Exists(TempfileName))
            {
                try
                {
                    File.Delete(TempfileName);
                }
                catch { }
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (p != null)
            {
                p.Close();
                p = null;
            }
            delTempFile();
        }

        private void btnSet_Click(object sender, EventArgs e)
        {
            irs.Features = Convert.ToByte(int.Parse(txtFet.Text, System.Globalization.NumberStyles.HexNumber));
            irs.SectorCount = Convert.ToByte(int.Parse(txtSec.Text, System.Globalization.NumberStyles.HexNumber));
            irs.SectorNumber = Convert.ToByte(int.Parse(txtLow.Text, System.Globalization.NumberStyles.HexNumber));
            irs.CylinderLow = Convert.ToByte(int.Parse(txtMid.Text, System.Globalization.NumberStyles.HexNumber));
            irs.CylinderHigh = Convert.ToByte(int.Parse(txtHight.Text, System.Globalization.NumberStyles.HexNumber));
            irs.DriveHead = Convert.ToByte(int.Parse(txtDev.Text, System.Globalization.NumberStyles.HexNumber));
            irs.Command = Convert.ToByte(int.Parse(txtCmd.Text, System.Globalization.NumberStyles.HexNumber));
            SendATACommand(irs);
        }

        private void clearBuffer()
        {
            if (File.Exists(TempfileName))
            {
                try
                {
                    File.Delete(TempfileName);
                }
                catch (Exception)
                {

                    try
                    {
                        fs.Close();
                        File.Delete(TempfileName);
                    }
                    catch (Exception)
                    {
                        TempfileName += "0";
                    }
                }

            }
        }

        private void btnGet_Click(object sender, EventArgs e)
        {
            UInt16 addr = 0;
            if (txtPort.Text != "")
            {
                addr = (UInt16)int.Parse(txtPort.Text, System.Globalization.NumberStyles.HexNumber);
            }
            bool yess = GetBuffer(addr,TempfileName);
            if (yess)
            {
                FileInfo finf = new FileInfo(TempfileName);
                lblbuf.Text = finf.Length.ToString();
                finf = null;
            }
            else
            {
                clearBuffer();
            }

        }

        char[] HardDriveSerialNumber = new char[1024];

        private bool SendATACommand(IDEREGS irs)
        {
            bool done = false;
            //  Wait for controller not busy 
            WaitNBSY();
            //  Get drive info data
            p.SetPortValue((UInt16)(baseAddress + 1), irs.Features, 1);
            p.SetPortValue((UInt16)(baseAddress + 2), irs.SectorCount, 1);
            p.SetPortValue((UInt16)(baseAddress + 3), irs.SectorNumber, 1);
            p.SetPortValue((UInt16)(baseAddress + 4), irs.CylinderLow, 1);
            p.SetPortValue((UInt16)(baseAddress + 5), irs.CylinderHigh, 1);
            p.SetPortValue((UInt16)(baseAddress + 6), irs.DriveHead, 1);
            p.SetPortValue((UInt16)(baseAddress + 7), irs.Command, 1);
            WaitNBSY();
            done = true;
            return done;
        }

        private bool GetBuffer(UInt16 baseAddress,string pathfile)
        {
            bool can = false;
        lpp:
            if (fs != null)
            {
                fs.Close();
            }
            try
            {
                fs = File.Open(pathfile, FileMode.OpenOrCreate);
                fs.Seek(fs.Length, SeekOrigin.Begin);
            }
            catch (Exception)
            {
                if (pathfile == TempfileName)
                {
                    TempfileName = TempfileName + "0";
                    pathfile = TempfileName;
                }
                else {
                    pathfile = pathfile + "0";
                }                
                goto lpp;
            }
            uint portValue = 0;
            while (true)
            {
                portValue = 0;
                p.GetPortValue((UInt16)(baseAddress + 7), &portValue, 1);
                if (portValue == 88)
                {
                    UInt32 its = 0;
                    p.GetPortValue(baseAddress, &its, 4);
                    byte[] bts = BitConverter.GetBytes(its);
                    fs.Write(bts, 0, bts.Length);
                    can = true;
                }
                else
                {
                    if (portValue < 256)
                    {
                        break;
                    }
                }
            }
            fs.Close();
            return can;
        }

        private void lblClear()
        {
            lbBsy.BackColor = Color.DarkGray;
            lbCorr.BackColor = Color.DarkGray;
            lbDf.BackColor = Color.DarkGray;
            lbDrdy.BackColor = Color.DarkGray;
            lbDrq.BackColor = Color.DarkGray;
            lbDsc.BackColor = Color.DarkGray;
            lbEror.BackColor = Color.DarkGray;
            lbIdx.BackColor = Color.DarkGray;
            /////////////////////////////////////////////
            lbAMNF.BackColor = Color.DarkGray;
            lbTONF.BackColor = Color.DarkGray;
            lbABRT.BackColor = Color.DarkGray;
            lbIDNF.BackColor = Color.DarkGray;
            lbUNC.BackColor = Color.DarkGray;
            lbBBK.BackColor = Color.DarkGray;
        }

        private bool WaitNBSY()
        {
            lblClear();
            bool result = true;
            UInt32 portValue = 0;
            UInt32 errValue = 0;
            int waitLoop = 10000;
            while (--waitLoop > 0)
            {
                portValue = 0;
                p.GetPortValue((UInt16)(baseAddress + 7), &portValue, 1);
                //  drive is ready
                if (portValue == 0x50) { lbDrdy.BackColor = Color.Green; lbDsc.BackColor = Color.Green; break; }
                //  previous drive command ended in error
                if (portValue == 0x51)
                {
                    lbDrdy.BackColor = Color.Green; lbDsc.BackColor = Color.Green; lbEror.BackColor = Color.Red;
                    break;
                }
                //  drive is ready and drq
                if (portValue == 0x58) { lbDrdy.BackColor = Color.Green; lbDsc.BackColor = Color.Green; lbDrq.BackColor = Color.Green; break; }
                //  drive is busy
                if (portValue == 0xD0) { lbDrdy.BackColor = Color.Green; lbDsc.BackColor = Color.Green; lbBsy.BackColor = Color.Green; break; }
                //  previous drive command ended in error
            }
            if (waitLoop < 1) { lbBsy.BackColor = Color.Green; result = false; };
            errValue = 0;
            p.GetPortValue((UInt16)(baseAddress + 1), &errValue, 1);
            switch (errValue)
            {
                case 0: { break; }//no error
                case 1: { lbAMNF.BackColor = Color.Red; break; }
                case 2: { break; }
                case 3: { break; }
                case 4: { lbABRT.BackColor = Color.Red; break; }
                case 64: { lbUNC.BackColor = Color.Red; break; }
                case 16: { lbIDNF.BackColor = Color.Red; break; }
            }
            return result;
        }

        private string getNormalCount(string str)
        {
            string str1 = "";

            if (str.Length > 2)
            {
                str1 = str.Substring(str.Length - 2, 2);
            }
            else
            {
                str1 = str;
            }
            if (str1.Length == 1)
            {
                str1 = "0" + str1;
            }
            return str1;
        }

        private void Monitorings()
        {
            lblClear();
            UInt32 Features;
            UInt32 SectorCount;
            UInt32 SectorNumber;
            UInt32 CylinderLow;
            UInt32 CylinderHigh;
            UInt32 DriveHead;
            UInt32 Command;
            Features = 0;
            p.GetPortValue((UInt16)(baseAddress + 1), &Features, 1);
            mon1.Text = getNormalCount(Features.ToString("x"));
            SectorCount = 0;
            p.GetPortValue((UInt16)(baseAddress + 2), &SectorCount, 1);
            mon2.Text = getNormalCount(SectorCount.ToString("x"));
            SectorNumber = 0;
            p.GetPortValue((UInt16)(baseAddress + 3), &SectorNumber, 1);
            mon3.Text = getNormalCount(SectorNumber.ToString("x"));
            CylinderLow = 0;
            p.GetPortValue((UInt16)(baseAddress + 4), &CylinderLow, 1);
            mon4.Text = getNormalCount(CylinderLow.ToString("x"));
            CylinderHigh = 0;
            p.GetPortValue((UInt16)(baseAddress + 5), &CylinderHigh, 1);
            mon5.Text = getNormalCount(CylinderHigh.ToString("x"));
            DriveHead = 0;
            p.GetPortValue((UInt16)(baseAddress + 6), &DriveHead, 1);
            mon6.Text = getNormalCount(DriveHead.ToString("x"));
            Command = 0;
            p.GetPortValue((UInt16)(baseAddress + 7), &Command, 1);
            mon7.Text = getNormalCount(Command.ToString("x"));
            if (Command != 0)
            {
                switch (Command)
                {
                    //  drive is ready
                    case 0x50: { lbDrdy.BackColor = Color.Green; lbDsc.BackColor = Color.Green; break; }//no error
                    //  previous drive command ended in error
                    case 0x51: { lbDrdy.BackColor = Color.Green; lbDsc.BackColor = Color.Green; lbEror.BackColor = Color.Red; break; }
                    //  drive is ready and drq
                    case 0x58: { lbDrdy.BackColor = Color.Green; lbDsc.BackColor = Color.Green; lbDrq.BackColor = Color.Green; break; }
                    //  drive is busy
                    case 0xD0: { lbDrdy.BackColor = Color.Green; lbDsc.BackColor = Color.Green; lbBsy.BackColor = Color.Green; break; }
                    default: { lbBsy.BackColor = Color.Green; break; }
                }
                switch (Features)
                {
                    case 0: { break; }//no error
                    case 1: { lbAMNF.BackColor = Color.Red; break; }
                    case 2: { break; }
                    case 3: { break; }
                    case 4: { lbABRT.BackColor = Color.Red; break; }
                    case 64: { lbUNC.BackColor = Color.Red; break; }
                    case 16: { lbIDNF.BackColor = Color.Red; break; }
                }
            }
        }

        private void btnMonitor_Click(object sender, EventArgs e)
        {
            if (Monitoring)
            {
                Monitoring = false;
                btnMonitor.Text = "Start";
                timer1.Enabled = false;
            }
            else
            {
                Monitoring = true;
                btnMonitor.Text = "Stop";
                timer1.Enabled = true;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Monitorings();
        }

        private void txtbox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((sender as TextBox).SelectedText != "")
            {
                int a, ss;
                ss = (sender as TextBox).SelectionStart;
                a = (sender as TextBox).SelectionLength;
                (sender as TextBox).Text = (sender as TextBox).Text.Remove(ss, a);
                if (ss > 0)
                {
                    (sender as TextBox).SelectionStart = (sender as TextBox).Text.Length;
                    (sender as TextBox).SelectionLength = 0;
                }
            }

            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
            !((e.KeyChar > 64) && (e.KeyChar < 71)) && !((e.KeyChar > 96) && (e.KeyChar < 103)))
            {
                e.Handled = true;
            }
            if ((sender as TextBox).Text != "")
            {
                if ((e.KeyChar != (char)Keys.Back) && (sender as TextBox).Text.Length > 1)
                {
                    e.Handled = true;
                }

            }
        }

        private void txtbox_Leave(object sender, EventArgs e)
        {
            if ((sender as TextBox).Text == "")
            {
                (sender as TextBox).Text = "00";
            }
            try
            {
                int.Parse((sender as TextBox).Text, System.Globalization.NumberStyles.HexNumber);
            }
            catch (Exception)
            {
                (sender as TextBox).Text = "00";
            }
            if (int.Parse(tb1.Text, System.Globalization.NumberStyles.HexNumber) > int.Parse(te1.Text, System.Globalization.NumberStyles.HexNumber))
            {
                tb1.Text = getNormalCount(te1.Text);
            }
            if (int.Parse(tb2.Text, System.Globalization.NumberStyles.HexNumber) > int.Parse(te2.Text, System.Globalization.NumberStyles.HexNumber))
            {
                tb2.Text = getNormalCount(te2.Text);
            }
            if (int.Parse(tb3.Text, System.Globalization.NumberStyles.HexNumber) > int.Parse(te3.Text, System.Globalization.NumberStyles.HexNumber))
            {
                tb3.Text = getNormalCount(te3.Text);
            }
            if (int.Parse(tb4.Text, System.Globalization.NumberStyles.HexNumber) > int.Parse(te4.Text, System.Globalization.NumberStyles.HexNumber))
            {
                tb4.Text = getNormalCount(te4.Text);
            }
            if (int.Parse(tb5.Text, System.Globalization.NumberStyles.HexNumber) > int.Parse(te5.Text, System.Globalization.NumberStyles.HexNumber))
            {
                tb5.Text = getNormalCount(te5.Text);
            }
            if (int.Parse(tb6.Text, System.Globalization.NumberStyles.HexNumber) > int.Parse(te6.Text, System.Globalization.NumberStyles.HexNumber))
            {
                tb6.Text = getNormalCount(te6.Text);
            }
            if (int.Parse(tb7.Text, System.Globalization.NumberStyles.HexNumber) > int.Parse(te7.Text, System.Globalization.NumberStyles.HexNumber))
            {
                tb7.Text = getNormalCount(te7.Text);
            }

            if ((sender as TextBox).Text.Length <2)
            {
                (sender as TextBox).Text = "0" + (sender as TextBox).Text;
            }
        }

        private void runLoop()
        {
            for (int i7 = loopir[0].Command; i7 < loopir[1].Command + 1; i7++)
            {
                for (int i6 = loopir[0].DriveHead; i6 < loopir[1].DriveHead + 1; i6++)
                {
                    for (int i5 = loopir[0].CylinderHigh; i5 < loopir[1].CylinderHigh + 1; i5++)
                    {
                        for (int i4 = loopir[0].CylinderLow; i4 < loopir[1].CylinderLow + 1; i4++)
                        {
                            for (int i3 = loopir[0].SectorNumber; i3 < loopir[1].SectorNumber + 1; i3++)
                            {
                                for (int i2 = loopir[0].SectorCount; i2 < loopir[1].SectorCount + 1; i2++)
                                {
                                    for (int i1 = loopir[0].Features; i1 < loopir[1].Features + 1; i1++)
                                    {                                        
                                        groupBox1.Invoke(new MethodInvoker(delegate
                                        {
                                            tb1.Text = getNormalCount(i1.ToString("x"));
                                            tb2.Text = getNormalCount(i2.ToString("x"));
                                            tb3.Text = getNormalCount(i3.ToString("x"));
                                            tb4.Text = getNormalCount(i4.ToString("x"));
                                            tb5.Text = getNormalCount(i5.ToString("x"));
                                            tb6.Text = getNormalCount(i6.ToString("x"));
                                            tb7.Text = getNormalCount(i7.ToString("x"));
                                        }));
                                        p.SetPortValue((UInt16)(baseAddress + 1), (UInt32)i1, 1);
                                        p.SetPortValue((UInt16)(baseAddress + 2), (UInt32)i2, 1);
                                        p.SetPortValue((UInt16)(baseAddress + 3), (UInt32)i3, 1);
                                        p.SetPortValue((UInt16)(baseAddress + 4), (UInt32)i4, 1);
                                        p.SetPortValue((UInt16)(baseAddress + 5), (UInt32)i5, 1);
                                        p.SetPortValue((UInt16)(baseAddress + 6), (UInt32)i6, 1);
                                        p.SetPortValue((UInt16)(baseAddress + 7), (UInt32)i7, 1);
                                        UInt32 portValue = 0;
                                        int waitLoop = 1000000;
                                        while (--waitLoop > 0)
                                        {
                                            portValue = 0;
                                            p.GetPortValue((UInt16)(baseAddress + 7), &portValue, 1);
                                            //  drive is ready
                                            if (portValue == 0x50) { loopResult += "ready= " + i1.ToString("x") + " - " + i2.ToString("x") + " - " + i3.ToString("x") + " - " + i4.ToString("x") + " - " + i5.ToString("x") + " - " + i6.ToString("x") + " - " + i7.ToString("x") + Environment.NewLine; lbDrdy.BackColor = Color.Green; lbDsc.BackColor = Color.Green; break; }
                                            //  previous drive command ended in error
                                            if (portValue == 0x51) { /*textBox1.Text = textBox1.Text + "error=" + i.ToString("x") + Environment.NewLine;*/ lbDrdy.BackColor = Color.Green; lbDsc.BackColor = Color.Green; lbEror.BackColor = Color.Red; break; }
                                            //  drive is ready and drq
                                            if (portValue == 0x58)
                                            {
                                                if (ifDrq.Checked)
                                                {
                                                    trlp = false;
                                                }
                                                if (SDRQ.Checked)
                                                {
                                                    string strpath = Application.StartupPath + "\\OutDir\\";
                                                    if (!Directory.Exists(strpath))
                                                    {
                                                        Directory.CreateDirectory(strpath);
                                                    }
                                                    strpath += i1.ToString("x") + "-";
                                                    strpath += i2.ToString("x") + "-";
                                                    strpath += i3.ToString("x") + "-";
                                                    strpath += i4.ToString("x") + "-";
                                                    strpath += i5.ToString("x") + "-";
                                                    strpath += i6.ToString("x") + "-";
                                                    strpath += i7.ToString("x") + ".bin";
                                                    GetBuffer(baseAddress, strpath);
                                                }
                                                loopResult += "drq= " + i1.ToString("x") + " - " + i2.ToString("x") + " - " + i3.ToString("x") + " - " + i4.ToString("x") + " - " + i5.ToString("x") + " - " + i6.ToString("x") + " - " + i7.ToString("x") + Environment.NewLine; lbDrdy.BackColor = Color.Green; lbDsc.BackColor = Color.Green; lbDrq.BackColor = Color.Green; break;
                                            }
                                            //  drive is busy
                                            if (portValue == 0xD0) { lbDrdy.BackColor = Color.Green; lbDsc.BackColor = Color.Green; lbBsy.BackColor = Color.Green; break; }
                                            //  previous drive command ended in error
                                        }
                                        //if (waitLoop < 1) { trlp = false; lbBsy.BackColor = Color.Green; };

                                        if (trlp == false)
                                        {
                                            btnLoop.Invoke(new MethodInvoker(delegate
                                            {
                                                btnLoop.Text = "Start";
                                            }));
                                            groupBox1.Invoke(new MethodInvoker(delegate
                                            {
                                                btnLoop.Text = "Start";
                                                tb1.Enabled = true; tb2.Enabled = true; tb3.Enabled = true; tb4.Enabled = true; tb5.Enabled = true; tb6.Enabled = true; tb7.Enabled = true;
                                                te1.Enabled = true; te2.Enabled = true; te3.Enabled = true; te4.Enabled = true; te5.Enabled = true; te6.Enabled = true; te7.Enabled = true;
                                            }));
                                            trLoop.Abort();
                                            if (trLoop != null)
                                            {
                                                trLoop = null;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            groupBox1.Invoke(new MethodInvoker(delegate
            {
                btnLoop.Text = "Start";
                tb1.Enabled = true; tb2.Enabled = true; tb3.Enabled = true; tb4.Enabled = true; tb5.Enabled = true; tb6.Enabled = true; tb7.Enabled = true;
                te1.Enabled = true; te2.Enabled = true; te3.Enabled = true; te4.Enabled = true; te5.Enabled = true; te6.Enabled = true; te7.Enabled = true;
            }));
            if (trLoop != null)
            {
                trLoop = null;
            }
        }

        private void btnLoop_Click(object sender, EventArgs e)
        {
            if (trLoop == null)
            {
                loopResult = "";
                loopir[0] = new IDEREGS();
                loopir[1] = new IDEREGS();
                loopir[0].Features = (byte)int.Parse(tb1.Text, System.Globalization.NumberStyles.HexNumber);
                loopir[0].SectorCount = (byte)int.Parse(tb2.Text, System.Globalization.NumberStyles.HexNumber);
                loopir[0].SectorNumber = (byte)int.Parse(tb3.Text, System.Globalization.NumberStyles.HexNumber);
                loopir[0].CylinderLow = (byte)int.Parse(tb4.Text, System.Globalization.NumberStyles.HexNumber);
                loopir[0].CylinderHigh = (byte)int.Parse(tb5.Text, System.Globalization.NumberStyles.HexNumber);
                loopir[0].DriveHead = (byte)int.Parse(tb6.Text, System.Globalization.NumberStyles.HexNumber);
                loopir[0].Command = (byte)int.Parse(tb7.Text, System.Globalization.NumberStyles.HexNumber);
                loopir[1].Features = (byte)int.Parse(te1.Text, System.Globalization.NumberStyles.HexNumber);
                loopir[1].SectorCount = (byte)int.Parse(te2.Text, System.Globalization.NumberStyles.HexNumber);
                loopir[1].SectorNumber = (byte)int.Parse(te3.Text, System.Globalization.NumberStyles.HexNumber);
                loopir[1].CylinderLow = (byte)int.Parse(te4.Text, System.Globalization.NumberStyles.HexNumber);
                loopir[1].CylinderHigh = (byte)int.Parse(te5.Text, System.Globalization.NumberStyles.HexNumber);
                loopir[1].DriveHead = (byte)int.Parse(te6.Text, System.Globalization.NumberStyles.HexNumber);
                loopir[1].Command = (byte)int.Parse(te7.Text, System.Globalization.NumberStyles.HexNumber);
                tb1.Enabled = false; tb2.Enabled = false; tb3.Enabled = false; tb4.Enabled = false; tb5.Enabled = false; tb6.Enabled = false; tb7.Enabled = false;
                te1.Enabled = false; te2.Enabled = false; te3.Enabled = false; te4.Enabled = false; te5.Enabled = false; te6.Enabled = false; te7.Enabled = false;
                btnLoop.Text = "Stop";
                trlp = true;
                trLoop = new Thread(new ThreadStart(runLoop));
                trLoop.Start();
            }
            else
            {
                trlp = false;
                btnLoop.Text = "Start";
                if (trLoop != null)
                {
                    trLoop.Abort();
                    trLoop = null;
                }
                tb1.Enabled = true; tb2.Enabled = true; tb3.Enabled = true; tb4.Enabled = true; tb5.Enabled = true; tb6.Enabled = true; tb7.Enabled = true;
                te1.Enabled = true; te2.Enabled = true; te3.Enabled = true; te4.Enabled = true; te5.Enabled = true; te6.Enabled = true; te7.Enabled = true;
            }
        }

        private string SwapChars(string chars)
        {
            string strs = "";
            for (int i = 0; i <= chars.Length - 2; i += 2)
            {
                strs += chars[i + 1].ToString() + chars[i].ToString();
            }
            strs = strs.Replace(" ", "");
            return strs;
        }

        private bool GetIdentity()
        {
            UInt32 portValue = 0;
            while (true)
            {
                portValue = 0;
                p.GetPortValue((UInt16)(baseAddress + 7), &portValue, 1);
                if (portValue == 88)
                {
                    UInt32 its = 0;
                    p.GetPortValue(baseAddress, &its, 4);
                    IdentityBuffers.Add(BitConverter.GetBytes(its)[0]);
                    IdentityBuffers.Add(BitConverter.GetBytes(its)[1]);
                    IdentityBuffers.Add(BitConverter.GetBytes(its)[2]);
                    IdentityBuffers.Add(BitConverter.GetBytes(its)[3]);
                }
                else
                {
                    break;
                }
            }
            if (IdentityBuffers.Count > 0)
            {
                byte[] FW = IdentityBuffers.GetRange(46, 8).ToArray();
                byte[] SerialHdd = IdentityBuffers.GetRange(20, 28).ToArray();
                byte[] Model = IdentityBuffers.GetRange(54, 20).ToArray();
                byte[] LBA = IdentityBuffers.GetRange(200, 4).ToArray();
                //char[] serialno;
                txtFW.Text = SwapChars(ASCIIEncoding.ASCII.GetString(FW));
                txtSN.Text = SwapChars(ASCIIEncoding.ASCII.GetString(SerialHdd));
                txtModel.Text = SwapChars(ASCIIEncoding.ASCII.GetString(Model));
                for (int l = 0; l < LBA.Length; l++)
                {
                    txtLBA.Text += LBA[l].ToString("x");
                }
                string lbastr = SwapChars(txtLBA.Text);
                string resultlba = "";
                for (int o = lbastr.Length - 1; o >= 0; o--)
                {
                    resultlba += lbastr[o].ToString();
                }
                txtLBA.Text = int.Parse(resultlba, System.Globalization.NumberStyles.HexNumber).ToString();
            }
            return true;
        }

        private void btnIdentity_Click(object sender, EventArgs e)
        {
            IdentityBuffers.RemoveRange(0, IdentityBuffers.Count());
            txtModel.Text = "";
            txtSN.Text = "";
            txtLBA.Text = "";
            txtFW.Text = "";
            IDEREGS irIdentity = new IDEREGS();
            irIdentity.Command = 0xec;
            if (Slaver.Checked)
            {
                irIdentity.DriveHead = 0xb0;
            }
            else
            {
                irIdentity.DriveHead = 0xa0;
            }
            SendATACommand(irIdentity);
            GetIdentity();
        }

        private void btnSetPort_Click(object sender, EventArgs e)
        {
            if (txtPort.Text != "")
            {
                baseAddress = (UInt16)int.Parse(txtPort.Text, System.Globalization.NumberStyles.HexNumber);
            }
            if (Slaver.Checked)
            {
                p.SetPortValue((UInt16)(baseAddress + 6), 0xb0, 1);
            }
            else
            {
                p.SetPortValue((UInt16)(baseAddress + 6), 0xa0, 1);
            }
        }

        private void txtPort_Leave(object sender, EventArgs e)
        {
            if ((sender as TextBox).Text == "")
            {
                (sender as TextBox).Text = "0";
            }
        }

        private void txtPort_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((sender as TextBox).SelectedText != "")
            {
                int a, ss;
                ss = (sender as TextBox).SelectionStart;
                a = (sender as TextBox).SelectionLength;
                (sender as TextBox).Text = (sender as TextBox).Text.Remove(ss, a);
                if (ss > 0)
                {
                    (sender as TextBox).SelectionStart = ss;
                    (sender as TextBox).SelectionLength = 0;
                }
            }

            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
            !((e.KeyChar > 64) && (e.KeyChar < 71)) && !((e.KeyChar > 96) && (e.KeyChar < 103)))
            {
                e.Handled = true;
            }
            if ((sender as TextBox).Text != "")
            {
                if ((e.KeyChar != (char)Keys.Back) && (sender as TextBox).Text.Length > 3)
                {
                    e.Handled = true;
                }

            }
        }

        private void Slaver_CheckedChanged(object sender, EventArgs e)
        {
            btnSetPort_Click(null, null);
        }

        private void btnSaveBuf_Click(object sender, EventArgs e)
        {
            if (File.Exists(TempfileName))
            {
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    string namef = saveFileDialog1.FileName;
                    if (File.Exists(namef))
                    {
                        DialogResult drs = MessageBox.Show("this file already exists . owerrite this ?", "Danger");
                        if (drs == DialogResult.OK)
                        {
                            try
                            {
                                File.Delete(namef);
                            }
                            catch (Exception)
                            {
                                MessageBox.Show("Access Denied. try enother path", "Error");
                            }
                        }
                    }
                    try
                    {
                        File.Copy(TempfileName, namef);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Access Denied. try enother path", "Error");
                    }
                }
            }
        }

        private bool WriteBuffer(UInt16 baseAddress, string pathfile)
        {
            bool can = false;
            if (fs != null)
            {
                fs.Close();
            }
            try
            {
                fs = File.Open(pathfile, FileMode.Open);
            }
            catch (Exception)
            {
                MessageBox.Show("Buffer Damaged Try Egain", "Error");
            }
            int idx = 0;
            uint portValue = 0;
            while (true)
            {
                portValue = 0;
                p.GetPortValue((UInt16)(baseAddress + 7), &portValue, 1);
                if (portValue == 88)
                {
                    byte[] bts = new byte[4];
                    // UInt32 its=0;
                    if (idx < fs.Length + 2)
                    {
                        fs.Read(bts, 0, 2);
                    }
                    p.SetPortValue(baseAddress, (UInt32)BitConverter.ToInt32(bts, 0), 2);
                    can = true;
                    idx++;
                }
                else
                {
                    if (portValue < 256)
                    {
                        break;
                    }
                }
            }
            fs.Close();
            return can;
        }

        private void btnWB_Click(object sender, EventArgs e)
        {
            bool canwrt = WriteBuffer(baseAddress,TempfileName);
            if (!canwrt)
            {
                MessageBox.Show("Can Not Send To HDD","Error");
            }
        }

        private void btnClearBuf_Click(object sender, EventArgs e)
        {
            clearBuffer();
            lblbuf.Text = "0";
        }

        private void btnShowBuffer_Click(object sender, EventArgs e)
        {
            if (File.Exists(TempfileName))
            {
                frmDump frd = new frmDump(TempfileName);
                frd.ShowDialog();
                FileInfo finf = new FileInfo(TempfileName);
                lblbuf.Text = finf.Length.ToString();
                finf = null;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            tbResult.Text = loopResult;
        }

        private void btnHTF_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog()==DialogResult.OK)
            {
                bool yess = GetBuffer(baseAddress, saveFileDialog1.FileName);
                if (yess)
                {
                    MessageBox.Show("Get From HDD Successfull.","Success");
                } else
                {
                    MessageBox.Show("Get From HDD Error.", "Errror");
                }
            }
        }

        private void btnFTH_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog()==DialogResult.OK)
            {
            bool canwrt = WriteBuffer(baseAddress, ofd.FileName);
            if (!canwrt)
            {
                MessageBox.Show("Can Not Send To HDD", "Error");
            }
            }
            ofd = null;
           
        }

        private void btnFTB_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (File.GetAttributes(openFileDialog1.FileName) != FileAttributes.Hidden)
                {
                    if (File.Exists(TempfileName))
                    {
                        try
                        {
                            File.Delete(TempfileName);
                        }
                        catch (Exception)
                        {
                            File.SetAttributes(TempfileName, FileAttributes.Normal);
                        }

                    }
                    File.Copy(openFileDialog1.FileName, TempfileName, true);
                    FileInfo finf = new FileInfo(TempfileName);
                    lblbuf.Text = finf.Length.ToString();
                    finf = null;
                }
            }
        }

        private void btnScan_Click(object sender, EventArgs e)
        {            
            fpin.ShowDialog();
            btnSetPort_Click(null, null);
        }
    }
}
