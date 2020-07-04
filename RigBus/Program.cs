using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading.Tasks;
using CommandLine;
using HambusCommonLibrary;
using Microsoft.AspNetCore.SignalR.Client;

namespace RigBus
{
  public class Options
  {
    [Option('p', "port", Required = false, HelpText = "HTTP Port that Master Bus will listen")]
    public int? Port { get; set; } = 7300;

    [Option('h', "host", Required = false, HelpText = "Name of HTTP host that Master Bus will listen")]
    public string? Host { get; set; } = "localhost";

    [Option('n', "name", Required = true, HelpText = "Name of the instance.")]
    public string? Name { get; set; }

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
      confOptions = opts;
    }
    static void HandleParseError(IEnumerable<Error> errs)
    {
      throw new Exception("Invalid Args");
    }
  }
}
