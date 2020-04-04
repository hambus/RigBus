using HambusCommonLibrary;
using System;

namespace RigBus
{
  class Program
  {
    static void Main(string[] args)
    {
      CommPortConfig portConf = new CommPortConfig();
      portConf.BaudRate = 57600;
      portConf.BaudRate = 8;
      portConf.StopBits = "one";
      portConf.Parity = "none";
      portConf.PortName = "com21";
      portConf.DisplayName = "PowerSDR";
      portConf.Handshake = "none";

      var kenwood = new Kenwood();
      kenwood.OpenPort(portConf);
      Console.ReadKey();
    }
  }
}
