using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using CoreHambusCommonLibrary.Model;
using CoreHambusCommonLibrary.Networking;
using HamBusCommmonCore;
using HamBusCommonCore.Model;
using KellermanSoftware.CompareNetObjects;

namespace RigBus
{
  public class KenwoodRig : RigControlBase
  {
    private CompareLogic compareLogic = new CompareLogic();


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
      FSKR = 9,
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
        case "KS":
          ReadKeyingSpeedCommand(cmd);
          break;
        case "EX":
          EXCommand(cmd);
          break;
        default:
          Console.WriteLine("Unknown: {0}", cmd);
          break;
      }
    }
    #region private methods
    #region parse commands

    private void EXCommand(string cmd)
    {
      SendSerial("?;");
    }

    private void ReadKeyingSpeedCommand(string cmd)
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
      // IF000180907501000+0000000000030010000;

      //   123456789 123456789 123456789 123456789 
      //                                 111111111
      //     11111111111222223333345677890123445
      //   IF000180907501000+0000000000030010000;


      var freq = cmd.Substring(2, 11);         // p1 Freq
      var space = cmd.Substring(13, 5);        // p2 space
      var rit = cmd.Substring(18, 5);          // p3 rit/xit offset 
      var ritOn = cmd.Substring(23, 1);        // p4 rit on/off
      var xitOn = cmd.Substring(24, 1);        // p5 xit on/off
      var memChannel = cmd.Substring(25,1);    // p6 mem channel
      var memChannel2 = cmd.Substring(26, 2);  // p7 memchannel 2
      var rxTx = cmd.Substring(28, 1);         // p8
      var mode = cmd.Substring(29, 1);         // p9 mode
      var vfo = cmd.Substring(30, 1);          // p10 VFO
      
      // p7 mem channel 2
      //Console.WriteLine($"Freq: {freq} ");
      State.Freq = Convert.ToInt64(freq);
      if (rit == "1") State.Rit = true; else State.Rit = false;
      if (rit == "1") State.Xit = true; else State.Xit = false;
      if (rxTx == "1") State.Tx = true; else State.Tx = false;
      State.Mode = ModeKenwoodToStandard((ModeValues) Convert.ToInt32(mode));
      //Console.WriteLine($"Mode: {mode} ");
      State.Name = Bus.Name;
      if (State.IsDirty() == true)
        SendState();
      State.ClearDirty();
    }



    private void ModeCommand(string cmd)
    {
      try
      {
          var semiLoc = cmd.IndexOf(';');
          var modeEnumStr = cmd.Substring(2, semiLoc - 2);
          var modeInt = Convert.ToInt32(modeEnumStr);
          State.Mode = ((ModeValues)modeInt).ToString();
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
        State.Name = Bus.Name;
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
          SendSerial("IF;");
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
          return ModeValues.FSKR;
        case "CWR":
          return ModeValues.CWR;
        case "FSR":
          return ModeValues.FSKR;
        case "TUNE":
          return ModeValues.Tune;
      }
      return ModeValues.ERROR;
    }

    private string ModeKenwoodToStandard(ModeValues mode)
    {

      switch (mode)
      {
        case ModeValues.USB:
          return "USB";
        case ModeValues.LSB:
          return "LSB";
        case ModeValues.CW:
          return "CW";
        case ModeValues.AM:
          return "AM";
        case ModeValues.FM:
          return "FM";
        case ModeValues.FSK:
          return "FSK";

        case ModeValues.CWR:
          return "CWR";
        case ModeValues.FSKR:
          return "FSKR";
        case ModeValues.Tune:
          return "TUNE";
      }
      return "ERROR";
    }
    #region Commands
    public override void SetLocalFrequency(long freq)
    {
      var f = freq.ToString("D11");
      //Console.WriteLine($"393: freq: {f}");

    }
    public override void SetLocalFrequencyA(long freq)
    {
      if (IsStateLocked) return;
      var f = freq.ToString("D11");
      var cmd = $"FA{f};";
      //Console.WriteLine($"401: from bus vfo a: {f} orig: {freq}");
      SendSerial(cmd, IsStateLocked);
    }
    public override void SetLocalFrequencyB(long freq)
    {
      if (IsStateLocked) return;
      var f = freq.ToString("D11");
      //Console.WriteLine($"408: from bus vfo b: {f} orig: {freq}");
      var cmd = $"FB{f};";
      SendSerial(cmd);
    }
    public override void SetLocalMode(string? mode) 
    {

      if (string.IsNullOrWhiteSpace(mode) || IsStateLocked)
        return;
      var kMode = (int) ModeStandardToKenwoodEnum(mode);
      var cmd = $"MD{kMode};";
      Console.WriteLine(cmd);
      SendSerial(cmd);

    }
    public override void SetStateFromBus(RigState state)
    {
      //Console.WriteLine($"425: {state.Name}: {state.Freq}  A{state.FreqA} B{state.FreqB} {state.Mode}");
      if (state.Name == Bus.Name || IsStateLocked) return;
      //Console.WriteLine($"{state.Name}: {state.Freq}  A{state.FreqA} B{state.FreqB} {state.Mode}");
      SetLocalFrequencyA(state.Freq);
      SetLocalFrequencyB(state.FreqB);
      SetLocalMode(state.Mode);
    }
    #endregion
    #endregion
  }
}
