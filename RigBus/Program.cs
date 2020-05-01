using HambusCommonLibrary;
using HamBusSig;
using System;

namespace RigBus
{
  class Program
  {
    static void Main(string[] args)
    {
      CommPortConfig portConf = new CommPortConfig();
      SigRConnection sigConnect = new SigRConnection();
      sigConnect.StartConnection("http://localhost:7300/masterbus");
      portConf.BaudRate = 57600;
      portConf.BaudRate = 8;
      portConf.StopBits = "one";
      portConf.Parity = "none";
      portConf.PortName = "com21";
      portConf.DisplayName = "PowerSDR";
      portConf.Handshake = "none";

      var kenwood = new Kenwood(sigConnect);
      kenwood.OpenPort(portConf);
      Console.ReadKey();
    }
  }
}
