using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using CoreHambusCommonLibrary.Model;
using CoreHambusCommonLibrary.Networking;
using HamBusCommonStd;
using HamBusCommonCore.Model;
using KellermanSoftware.CompareNetObjects;
using KenwoodLib;
using static KenwoodLib.KenwodModesConverter;

namespace RigBus
{
  public class KenwoodRig : RigControlBase
  {
    private CompareLogic compareLogic = new CompareLogic();

    public KenwoodRig() : base()
    {
      initStartupState();
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
          //FreqCommand(cmd);
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
      //SendSerial("?;");
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
        if (sigConnect == null)
          sigConnect = SigRConnection.Instance;
        sigConnect.SendRigState(State);
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
                throw new NullReferenceException("port config is null!");
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
    }
    public override void SetLocalFrequencyA(long freq)
    {
      var f = freq.ToString("D11");
      var cmd = $"FA{f};";
      SendSerial(cmd);
    }
    public override void SetLocalFrequencyB(long freq)
    {

      var f = freq.ToString("D11");
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
      if (IsStateLocked) return;

      if (state.Name == Bus.Name || IsStateLocked) return;

      SetLocalFrequencyA(state.Freq);
      SetLocalFrequencyB(state.FreqB);
      SetLocalMode(state.Mode);
    }
    #endregion
    #endregion
  }
}
