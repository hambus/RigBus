using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading.Tasks;
using CommandLine;
using HambusCommonLibrary;
using HamBusSig;

namespace RigBus
{
    class Options
    {
        [Option('p', "port", Required = false, HelpText = "HTTP Port that Master Bus will listen")]
        public int? Port { get; set; }

        [Option('h', "host", Required = false, HelpText = "Name of HTTP host that Master Bus will listen")]
        public string? Host { get; set; } = null;

        [Option('n', "name", Required = true, HelpText = "Name of the instance.")]
        public string? Name { get; set; }

        [Option('c', "commport", Required = false, HelpText = "Comm Port to connect to.")]
        public int? CommPort { get; set; }

        [Option('P', "parity", Required = false, HelpText = "Comm Port parity: (odd, even, none, mark).")]
        public string Parity { get; set; } = "none";

        [Option('s', "speed", Required = false, HelpText = "Comm Port speed: (4800, 9600, 19,200, etc).")]
        public int? Speed { get; set; } = 9600;

        [Option('d', "databits", Required = false, HelpText = "Data bits: (7 or 8).")]
        public int? DataBits { get; set; } = 9600;

        [Option('S', "stopbits", Required = false, HelpText = "Data bits: (1 or 2).")]
        public string? StopBits { get; set; } = "1";

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
            portConf.DataBits = 8;
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

            if (opts.Name != null) conf.Name = opts.Name;
            if (opts.Speed != null) conf.BaudRate = opts.Speed;
            if (opts.DataBits != null) conf.DataBits = opts.DataBits;
            if (opts.CommPort != null) conf.Port = (int)opts.CommPort;
            if (opts.Parity != null) conf.Parity = opts.Parity;
            if (opts.StopBits != null) conf.StopBits = opts.StopBits;
        }
        static void HandleParseError(IEnumerable<Error> errs)
        {
            throw new Exception("Invalid Args");
        }
    }
}
