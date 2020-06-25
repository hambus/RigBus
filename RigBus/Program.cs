using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading.Tasks;
using CommandLine;
using HambusCommonLibrary;
using HamBusSig;
using Microsoft.AspNetCore.SignalR.Client;

namespace RigBus
{
  public class Options
  {
    [Option('p', "port", Required = false, HelpText = "HTTP Port that Master Bus will listen")]
    public int? Port { get; set; }

    [Option('h', "host", Required = false, HelpText = "Name of HTTP host that Master Bus will listen")]
    public string? Host { get; set; } = null;

    [Option('n', "name", Required = true, HelpText = "Name of the instance.")]
    public string? Name { get; set; }

    [Option('c', "commport", Required = false, HelpText = "Comm Port to connect to.")]
    public string? CommPort { get; set; }

    [Option('P', "parity", Required = false, HelpText = "Comm Port parity: (odd, even, none, mark).")]
    public string Parity { get; set; } = "none";

    [Option('s', "speed", Required = false, HelpText = "Comm Port speed: (4800, 9600, 19,200, etc).")]
    public int? Speed { get; set; } = 9600;

    [Option('d', "databits", Required = false, HelpText = "Data bits: (7 or 8).")]
    public int? DataBits { get; set; } = 9600;

    [Option('S', "stopbits", Required = false, HelpText = "Data bits: (one or two).")]
    public string? StopBits { get; set; } = "1";
  }
  class Program
  {
    static int port = 7300;
    static string host = "localhost";
    static Options? confOptions;
    static async Task Main(string[] args)
    {
      CommandLine.Parser.Default.ParseArguments<Options>(args)
        .WithParsed(RunOptions)
        .WithNotParsed(HandleParseError);

      var rigMain = new RigBusMain();
      if (confOptions == null)
        Environment.Exit(1);
      await rigMain.Run(confOptions);

      Console.ReadKey();
    }
    static void RunOptions(Options opts)
    {
      var conf = RigConf.Instance;

      if (opts.Name != null) conf.Name = opts.Name;
      if (opts.Speed != null) conf.BaudRate = opts.Speed;
      if (opts.DataBits != null) conf.DataBits = opts.DataBits;
      if (opts.CommPort != null) conf.PortName = opts.CommPort;
      if (opts.Parity != null) conf.Parity = opts.Parity;
      if (opts.StopBits != null) conf.StopBits = opts.StopBits;
      confOptions = opts;
    }
    static void HandleParseError(IEnumerable<Error> errs)
    {
      throw new Exception("Invalid Args");
    }
  }
}
