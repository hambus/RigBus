using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using CoreHambusCommonLibrary.Networking;
using HamBusCommmonCore;
using HamBusCommonCore.Model;
using KellermanSoftware.CompareNetObjects;

namespace RigBus
{
  public class KenwoodRig : RigControlBase
  {
    private CompareLogic compareLogic = new CompareLogic();

    public bool IsStateLocked { get; set; } = false;
    public KenwoodRig() : base()
    {
      initStartupState();
    }
    public enum ModeValues
    {
      /// <summary> Defines the LSB
      /// </summary>
      LSB = 1,
      /// <summary> Defines the USB
      /// </summary>
      USB = 2,
      /// <summary> Defines the CW
      /// </summary>
      CW = 3,
      /// <summary> Defines the FM
      /// </summary>
      FM = 4,
      /// <summary> Defines the AM
      /// </summary>
      AM = 5,
      /// <summary> Defines the FSK
      /// </summary>
      FSK = 6,
      /// <summary>// Defines the CWR
      /// </summary>
      CWR = 7,
      /// <summary> Defines the Tune
      /// </summary>
      Tune = 8,
      /// <summary> Defines the FSR
      /// </summary>
      FSR = 9,
      /// <summary> Defines the ERROR
      /// </summary>
      ERROR = 10
    }



    protected override void initStartupState()
    {
      OpenPort();
    }

    public void ParseDataFromRadio(string cmd)
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
          AutoInfoCommand(cmd);
          break;
        case "FA":
          FreqCommand(cmd);
          break;
        case "FR":
        case "FT":
          VfoCommand(cmd);
          break;
        case "MD":
          ModeCommand(cmd);
          break;
        case "IF":
          ReadTransCeiverStatusCommand(cmd);
          break;
        case "TX":
        case "RX":
          SetTransmitModeCommand(cmd);
          break;
        case "KS":
          ReadKeyboardSpeedCommand(cmd);
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
    #region private methods
    #region parse commands
    private void SMCommand(string cmd)
    {
      SendSerial("SM00000;");
    }

    private void EXCommand(string cmd)
    {
      SendSerial("?;");
    }

    private void ReadKeyboardSpeedCommand(string cmd)
    {
      if (cmd.Length == 3)
      {
        SendSerial("KS010;");
      }
    }


    private void IDCommand(string cmd)
    {
      if (cmd.Length == 3)
      {
        SendSerial("ID020;");
      }
    }

    private void AutoInfoCommand(string cmd)
    {
      if (cmd.Length == 3)
      {
        SendSerial("AI0;");
      }
    }

    private void ReadTransCeiverStatusCommand(string cmd)
    {
      string sendStr;
      string extStr = "1";
      if (cmd.Length != 3)
        return;
      int iTx = 0;
      //if (state.Tx)
      //  iTx = 1;
      //extStr = string.Format("{0}000000 ",
      //    Convert.ToInt32(ModeStandardToKenwoodEnum()));  // p15 6
      sendStr = string.Format("IF{0}{1}{2}{3}{4}{5}{6}{7}{8};",
          State.Freq.ToString("D11"), //p1
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

    private void SetTransmitModeCommand(string cmd)
    {
      if (cmd == "TX;")
      {
        State.Tx = true;
      }
      else
      {
        State.Tx = false;
      }
    }

    private void ModeCommand(string cmd)
    {
      try
      {
        if (cmd.Length == 3)
          GetMode();
        else
        {
          var semiLoc = cmd.IndexOf(';');
          var modeEnumStr = cmd.Substring(2, semiLoc - 2);
          var modeInt = Convert.ToInt32(modeEnumStr);
          State.Mode = ((ModeValues)modeInt).ToString();
        }
      }
      catch (FormatException)
      { }
    }

    private void GetMode()
    {

      var modeFmt = string.Format("MD;");
      SendSerial(modeFmt);

      return;
    }
    private void VfoCommand(string cmd)
    {

      if (cmd.Length == 4)
      {
        if (cmd[2] == '1')
          State.Vfo = "b";
        else
          State.Vfo = "a";
        return;
      }

    }
    private void FreqCommand(string cmd)
    {
      if (cmd.Length <= 4)
      {
        RequestFrequency(cmd);
        return;
      }
      ParseFrequency(cmd);
    }

    private void ParseFrequency(string cmd)
    {
      var semiLoc = cmd.IndexOf(';');
      var freqStr = cmd.Substring(2, semiLoc - 2);

      try
      {
        var freqInt = Convert.ToInt64(freqStr);
        State.Freq = freqInt;
        State.FreqA = freqInt;
        State.Name = Name;
        SendState();
      }
      catch (Exception e)
      {
        Console.WriteLine(e.Message);
      }
    }

    private void RequestFrequency(string cmd)
    {
      if (cmd[1].ToString().ToLower() == "a")
        SendSerial("FA" + State.FreqA.ToString("D11") + ";");
      else
        SendSerial("FB" + State.FreqB.ToString("D11") + ";");
      return;
    }

    private void SendState()
    {

      if (!compareLogic.Compare(prevState, State).AreEqual)
      {
        prevState = (RigState)State.Clone();
        if (sigConnect == null)
          sigConnect = SigRConnection.Instance;
        sigConnect.SendRigState(State);

      }
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
        Thread.Sleep(PollTimer);
        if (!PausePolling)
        {
          // SendSerial($"FT;"); // read vfo
          SendSerial($"FA;"); // read vfo a
          SendSerial($"MD;");
          //SendSerial($"FR;");
        }
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
              Console.WriteLine("Serial port {0} read error", portConf.commPortName);
              return;
            }
            char ch = Convert.ToChar(c);
            if (ch == ';')
            {
              sb.Append(ch);
              ParseDataFromRadio(sb.ToString());
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
            Console.WriteLine("Timeout Exception:  Maybe {0} isn't running.", portConf.name);
          }
          catch (Exception e)
          {
            if (portConf == null)
              throw new NullReferenceException("port confi is null!");
            Console.WriteLine("Serial Read Error: {0} port {1} Display Name: {2}",
                e.ToString(), portConf.commPortName, portConf.name);
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

    private ModeValues ModeStandardToKenwoodEnum(string mode)
    {
      if (mode == null) return ModeValues.ERROR;

      switch (mode.ToUpper())
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
    #region Commands
    public override void SetFrequency(long freq)
    {
      var f = freq.ToString("00000000000");
      Console.WriteLine($"freq: {f}");

    }
    public override void SetFrequencyA(long freq)
    {
      if (IsStateLocked) return;
      var f = freq.ToString("00000000000");
      var cmd = $"FA{f};";
      SendSerial(cmd, IsStateLocked);
    }
    public override void SetFrequencyB(long freq)
    {
      if (IsStateLocked) return;
      var f = freq.ToString("00000000000");
      var cmd = $"FB{f};";
      SendSerial(cmd);
    }
    public override void SetMode(string? mode) 
    {

      if (string.IsNullOrWhiteSpace(mode) || IsStateLocked)
        return;
      var kMode = (int) ModeStandardToKenwoodEnum(mode);
      var cmd = $"MD{kMode};";
      Console.WriteLine(cmd);
      SendSerial(cmd);

    }
    public override void SetState(RigState state)
    {
      if (state.Name == Name || IsStateLocked) return;
      Console.WriteLine($"{state.Name}: {state.Freq}  {state.Mode}");
      SetFrequencyA(state.FreqA);
      SetFrequencyB(state.FreqB);
      SetMode(state.Mode);
    }
    #endregion
    #endregion
  }
}
