using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text.Json;
using System.Threading.Tasks;
using CoreHambusCommonLibrary.DataLib;
using CoreHambusCommonLibrary.Model;
using CoreHambusCommonLibrary.Networking;
using HamBusCommonCore.Model;
using HambusCommonLibrary;
using Microsoft.AspNetCore.SignalR.Client;

namespace RigBus
{
  public class RigBusMain: Bus
  {
    Kenwood? kenwood { get; set; } 

    public async Task Run()
    {
      RigConf rigConf = RigConf.Instance;
      sigConnect = new SigRConnection();
      kenwood = new Kenwood(sigConnect);
      connection = await sigConnect.StartConnection($"http://{MasterHost}:{MasterPort}/masterbus");

      List<string>? groupList = new List<string>();
      groupList.Add("radio");
      groupList.Add("logging");
      groupList.Add("virtual");
      var ports = getAvailableSerialPort();
      Login(Name, groupList, ports);


      connection.On<BusConfigurationDB>("ReceiveConfiguration", (busConf) =>
      {
        var conf = JsonSerializer.Deserialize<RigConf>(busConf.Configuration);
        Console.WriteLine($"Got configuration ");

      });

      Console.WriteLine("after instantiating kenwood");
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
