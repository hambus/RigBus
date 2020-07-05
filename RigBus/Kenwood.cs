using System;
using System.IO.Ports;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using CoreHambusCommonLibrary.Networking;
using HamBusCommmonCore;
using HamBusCommonCore.Model;
using KellermanSoftware.CompareNetObjects;

namespace RigBus
{
  public class Kenwood : RigControlBase
  {
    private CompareLogic compareLogic = new CompareLogic();

    public override long Freq { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public override string Mode { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public override long FreqA { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public override long FreqB { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public override int Pitch { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public override string? RigType { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public override string? Rit { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public override int RitOffset { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public override string? Status { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public override string? StatusStr { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public override string? Split { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public override bool Tx { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public override string? Vfo { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public override string? Xit { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public Kenwood(SigRConnection sigRConnection) : base(sigRConnection)
    {
      initStartupState();
    }
    public enum ModeValues
    {
      /// <summary>
      /// Defines the LSB
      /// </summary>
      LSB = 1,
      /// <summary>
      /// Defines the USB
      /// </summary>
      USB = 2,
      /// <summary>
      /// Defines the CW
      /// </summary>
      CW = 3,
      /// <summary>
      /// Defines the FM
      /// </summary>
      FM = 4,
      /// <summary>
      /// Defines the AM
      /// </summary>
      AM = 5,
      /// <summary>
      /// Defines the FSK
      /// </summary>
      FSK = 6,
      /// <summary>
      /// Defines the CWR
      /// </summary>
      CWR = 7,
      /// <summary>
      /// Defines the Tune
      /// </summary>
      Tune = 8,
      /// <summary>
      /// Defines the FSR
      /// </summary>
      FSR = 9,
      /// <summary>
      /// Defines the ERROR
      /// </summary>
      ERROR = 10
    }

    protected override void initStartupState()
    {
      state.Freq = 14250000;
      state.FreqA = 14250000;
      state.Mode = "usb";
      state.Pitch = 0;
      state.Rit = "";
      state.RigType = "Kenwood";
      state.RitOffset = 0;
      state.Split = "off";
      state.StatusStr = "";
    }

    public void Command(string cmd)
    {
      cmd = cmd.ToUpper();
      cmd = Regex.Replace(cmd, @"\t|\n|\r", "");
      string subcmd = cmd.Substring(0, 2).ToUpper();

      switch (subcmd)
      {
        case "ID":
          IDCommand(cmd);
          break;
        case "AI":
          AICommand(cmd);
          break;
        case "FA":
          FreqCommand(cmd);
          break;
        case "FB":
          FreqCommand(cmd);
          break;
        case "FR":
          FreqCommand(cmd);
          break;
        case "FT":
          FTCommand(cmd);
          break;
        case "MD":
          ModeCommand(cmd);
          break;
        case "TX":
          TXRXCommand(cmd);
          break;
        case "IF":
          IFCommand(cmd);
          break;
        case "RX":
          TXRXCommand(cmd);
          break;
        case "KS":
          KSCommand(cmd);
          break;
        case "SM":
          SMCommand(cmd);
          break;
        case "EX":
          EXCommand(cmd);
          break;
        case "?;":
          Console.WriteLine("Error: {0}", cmd);
          break;
        default:
          Console.WriteLine("Unknown: {0}", cmd);
          break;
      }
    }

    #region parse commands
    private void SMCommand(string cmd)
    {
      SendSerial("SM00000;");
    }

    private void EXCommand(string cmd)
    {
      SendSerial("?;");
    }

    private void KSCommand(string cmd)
    {
      if (cmd.Length == 3)
      {
        SendSerial("KS010;");
      }
    }

    private void FTCommand(string cmd)
    {
      if (cmd.Length == 3)
      {
        SendSerial("FT0;");
      }
    }

    private void IDCommand(string cmd)
    {
      if (cmd.Length == 3)
      {
        SendSerial("ID020;");
      }
    }

    private void AICommand(string cmd)
    {
      if (cmd.Length == 3)
      {
        SendSerial("AI0;");
      }
    }

    private void IFCommand(string cmd)
    {
      string sendStr;
      string extStr;
      if (cmd.Length != 3)
        return;
      int iTx = 0;
      //if (state.Tx)
      //  iTx = 1;
      extStr = string.Format("{0}000000 ",
          Convert.ToInt32(ModeStandardToKenwoodEnum()));  // p15 6
      sendStr = string.Format("IF{0}{1}{2}{3}{4}{5}{6}{7}{8};",
          state.Freq.ToString("D11"), //p1
          "TS480",//p2
          "+0000",// p3
          "0", // p4
          "0", // p5
          "0", // p6
          "00", // p7
          iTx.ToString(), //p8
          extStr); // p9

      SendSerial(sendStr);
    }

    private void TXRXCommand(string cmd)
    {
      if (cmd == "TX;")
      {
        state.Tx = true;
      }
      else
      {
        state.Tx = false;
      }
      //udpServer.SendBroadcast(state, 7300);
    }

    private void ModeCommand(string cmd)
    {
      try
      {
        if (cmd.Length == 3)
        {

          int mode = Convert.ToInt32(ModeStandardToKenwoodEnum());
          var modeFmt = string.Format("MD{0};", mode.ToString());
          SendSerial(modeFmt);

          return;
        }
        var semiLoc = cmd.IndexOf(';');
        var modeEnumStr = cmd.Substring(2, semiLoc - 2);
        var modeInt = Convert.ToInt32(modeEnumStr);
        state.Mode = ((ModeValues)modeInt).ToString();
      }
      catch (FormatException)
      { }
    }

    private void FreqCommand(string cmd)
    {
      if (cmd.Length == 3)
      {
        if (cmd[1].ToString().ToLower() == "a")
          SendSerial("FA" + state.FreqA.ToString("D11") + ";");
        else
          SendSerial("FB" + state.FreqB.ToString("D11") + ";");
        return;
      }


      var semiLoc = cmd.IndexOf(';');
      var freqStr = cmd.Substring(2, semiLoc - 2);
      try
      {
        var freqInt = Convert.ToInt64(freqStr);
        state.Freq = freqInt;
        state.FreqA = freqInt;
        signal();
      }
      catch (Exception e)
      {
        Console.WriteLine(e.Message);
      }

    }

    private void signal()
    {

      if (!compareLogic.Compare(prevState, state).AreEqual)
      {
        prevState = (RigState)state.Clone();
        if (sigConnect == null) throw new NullReferenceException("sigConnect is null");
        sigConnect.sendRigState(state);
        printRigSettings();
      }

    }
    private void printRigSettings()
    {
      Console.WriteLine($"Freq: {state.Freq}");
      Console.WriteLine($"Mode: {state.Mode}");
    }

    private void VFOCommand(string cmd)
    {
      if (cmd.Length == 3)
      {
        SendSerial("FR0;");
        return;
      }
    }
    public override void PollRig()
    {
      while (true)
      {
        Thread.Sleep(pollTimer);
        SendSerial($"FT;");
        SendSerial($"FA;");
        SendSerial($"MD;");
      }

    }
    #endregion
    public override void ReadSerialPortThread()
    {
      StringBuilder sb = new StringBuilder();
      while (continueReadingSerialPort)
      {
        try
        {
          try
          {
            if (serialPort == null) throw new NullReferenceException("serial port is null!");
            int c = serialPort.ReadChar();
            if (c < 0)
            {
              if (portConf == null)
                throw new NullReferenceException("port confi is null!");
              Console.WriteLine("Serial port {0} read error", portConf.CommPortName);
              return;
            }
            char ch = Convert.ToChar(c);
            if (ch == ';')
            {
              sb.Append(ch);
              //Console.WriteLine($"response: {sb.ToString()}");
              Command(sb.ToString());
              sb.Clear();
            }
            else
            {
              sb.Append(ch);
            }
          }
          catch (TimeoutException)
          {
            if (portConf == null)
              throw new NullReferenceException("port confi is null!");
            Console.WriteLine("Timeout Exception:  Maybe {0} isn't running.", portConf.Name);
          }
          catch (Exception e)
          {
            if (portConf == null)
              throw new NullReferenceException("port confi is null!");
            Console.WriteLine("Serial Read Error: {0} port {1} Display Name: {2}",
                e.ToString(), portConf.CommPortName, portConf.Name);
          }
        }
        catch (TimeoutException)
        {
          Console.WriteLine("Time out exception");
        }
        catch (FormatException) { }
      }
      if (serialPort == null) throw new NullReferenceException("serial port is null!");
      serialPort.Close();
    }

    private ModeValues ModeStandardToKenwoodEnum()
    {
      if (state.Mode == null) return ModeValues.ERROR;

      switch (state.Mode.ToUpper())
      {
        case "USB":
          return ModeValues.USB;
        case "LSB":
          return ModeValues.LSB;
        case "CW":
          return ModeValues.CW;
        case "CWL":
          return ModeValues.CW;
        case "CWU":
          return ModeValues.CW;
        case "AM":
          return ModeValues.AM;
        case "FM":
          return ModeValues.FM;
        case "FSK":
          return ModeValues.FSK;
        case "DIGH":
          return ModeValues.FSK;
        case "DIGL":
          return ModeValues.FSR;
        case "CWR":
          return ModeValues.CWR;
        case "FSR":
          return ModeValues.FSR;
        case "TUNE":
          return ModeValues.Tune;
      }
      return ModeValues.ERROR;
    }
  }
}
