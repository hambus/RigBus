using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using CoreHambusCommonLibrary.Model;
using HambusCommonLibrary;
using HamBusCommonStd.Model;
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
    RigBusMain? rigMain = null;
    static async Task Main(string[] args)
    {
      var prog = new Program();
      await prog.Run(args);
    }

    async Task Run(string[] args) 
    {
      rigMain = new RigBusMain();
      Parser.Default.ParseArguments<Options>(args)
        .WithParsed(RunOptions)
        .WithNotParsed(HandleParseError);

      await rigMain.Run();

      while (true) Thread.Sleep(100000);
    }
     void RunOptions(Options opts)
    {
      if (rigMain == null)
        throw new NullReferenceException("RigMain");
      if (opts.Name != null) 
        Bus.Name = opts.Name;
      if (opts.Host != null)
        rigMain!.MasterHost = opts.Host;
      if (opts.Port != null)
        rigMain!.MasterPort = Convert.ToInt32(opts.Port);
    }
    void HandleParseError(IEnumerable<Error> errs)
    {
      throw new Exception("Invalid Args");
    }
  }
}
