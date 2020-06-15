using HambusCommonLibrary;
using HamBusSig;
using System;
using System.Threading.Tasks;
using CommandLine;
using System.Collections.Generic;

namespace RigBus
{
  class Options
  {
    [Option('p', "port", Required = false, HelpText = "Port that Master Bus will listen")]
    public int? Port { get; set; }

    [Option('h', "host", Required = false, HelpText = "Name of host that Master Bus will listen")]
    public string? Host { get; set; } = null;

    [Option('n', "name", Required = true, HelpText = "Name of the instance.")]
    public string? Name { get; set; }

    //[Option('c', "commport", Required = false, HelpText = "Comm Port to connect to.")]
    //public string CommPort { get; set; }

    //[Option('P', "parity", Required = false, HelpText = "Comm Port parity: (odd, even, none, mark).")]
    //public string Parity { get; set; }

    //[Option('P', "parity", Required = false, HelpText = "Comm Port parity: (odd, even, none, mark).")]
    //public string Parity { get; set; }
  }
  class Program
  {
    static async Task Main(string[] args)
    {
      CommandLine.Parser.Default.ParseArguments<Options>(args)
  .WithParsed(RunOptions)
  .WithNotParsed(HandleParseError);

      CommPortConfig portConf = new CommPortConfig();
      SigRConnection sigConnect = new SigRConnection();
      await sigConnect.StartConnection("http://localhost:7300/masterbus");

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
    static void RunOptions(Options opts)
    {
      if (string.IsNullOrWhiteSpace(opts.Name))

        throw new Exception("Name is required!");

    }
    static void HandleParseError(IEnumerable<Error> errs)
    {
      throw new Exception("Invalid Args");
    }
  }
}
