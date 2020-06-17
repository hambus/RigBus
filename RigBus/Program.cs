using CommandLine;
using HambusCommonLibrary;
using HamBusSig;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading.Tasks;

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

        [Option('c', "commport", Required = false, HelpText = "Comm Port to connect to.")]
        public string? CommPort { get; set; }

        [Option('P', "parity", Required = false, HelpText = "Comm Port parity: (odd, even, none, mark).")]
        public string Parity { get; set; } = "none";

        [Option('s', "speed", Required = false, HelpText = "Comm Port speed: (4800, 9600, 19,200, etc).")]
        public int? Speed { get; set; } = 9600;

        //[Option('P', "parity", Required = false, HelpText = "Comm Port parity: (odd, even, none, mark).")]
        //public string Parity { get; set; }
    }
    class Program
    {
        static Int64 port = 7300;
        static string host = "localhost";
        static async Task Main(string[] args)
        {
            CommandLine.Parser.Default.ParseArguments<Options>(args)
        .WithParsed(RunOptions)
        .WithNotParsed(HandleParseError);

            RigConf portConf = RigConf.Instance;
            SigRConnection sigConnect = new SigRConnection();

            await sigConnect.StartConnection($"http://{host}:{port}/masterbus");

            portConf.BaudRate = 57600;
            portConf.BaudRate = 8;
            portConf.StopBits = "one";
            portConf.Parity = "none";
            portConf.PortName = "com21";
            portConf.Name = "PowerSDR";
            portConf.Handshake = Handshake.None;

            var kenwood = new Kenwood(sigConnect);
            kenwood.OpenPort(portConf);
            Console.ReadKey();
        }
        static void RunOptions(Options opts)
        {
            var conf = RigConf.Instance;
#pragma warning disable CS8601 // Possible null reference assignment.
            conf.Name = opts.Name;
#pragma warning restore CS8601 // Possible null reference assignment.
            if (opts.Speed != null) conf.BaudRate = opts.Speed;

        }
        static void HandleParseError(IEnumerable<Error> errs)
        {
            throw new Exception("Invalid Args");
        }
    }
}
