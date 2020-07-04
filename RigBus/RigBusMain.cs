using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text.Json;
using System.Threading.Tasks;
using CoreHambusCommonLibrary.DataLib;
using CoreHambusCommonLibrary.Model;
using CoreHambusCommonLibrary.Networking;
using HambusCommonLibrary;

using Microsoft.AspNetCore.SignalR.Client;

namespace RigBus
{
  public class RigBusMain: Bus
  {
    //private SigRConnection? sigConnect;
    //private HubConnection connection;
    public RigBusMain() { }

    public async Task Run(Options opts)
    {
      RigConf rigConf = RigConf.Instance;
      sigConnect = new SigRConnection();

      connection = await sigConnect.StartConnection($"http://{opts.Host}:{opts.Port}/masterbus");

      List<string>? groupList = new List<string>();
      groupList.Add("radio");
      groupList.Add("logging");
      groupList.Add("virtual");
      var ports = getAvailableSerialPort();
      Login("Flex300", groupList, ports);


      connection.On<BusConfigurationDB>("ReceiveConfiguration", (busConf) =>
      {
        var conf = JsonSerializer.Deserialize<RigConf>(busConf.Configuration);
        Console.WriteLine($"Got configuration ");

      });
      var kenwood = new Kenwood(sigConnect);
      Console.WriteLine("after instantanting kenwood");
      kenwood.OpenPort(rigConf);
    }
    public async void Login(string name, List<string>? group, List<string>? ports)
    {
      try
      {
        connection.On<string>("loginResponse", loginResponse);
        await connection.InvokeAsync("Login", name, group, ports);
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error: {ex.Message}");
      }
    }
    private void loginResponse(string message)
    {
      Console.WriteLine(message);
    }
    private List<string> getAvailableSerialPort()
    {
      List<string> list = new List<string>(SerialPort.GetPortNames());
      return list;

    }
  }
}
