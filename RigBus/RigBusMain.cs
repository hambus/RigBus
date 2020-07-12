using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text.Json;
using System.Threading.Tasks;
using CoreHambusCommonLibrary.DataLib;
using CoreHambusCommonLibrary.Model;
using CoreHambusCommonLibrary.Networking;
using HamBusCommmonCore;
using HamBusCommonCore.Model;
using HambusCommonLibrary;
using Microsoft.AspNetCore.SignalR.Client;

namespace RigBus
{
  public class RigBusMain: Bus
  {
    KenwoodRig? rig { get; set; } 

    public async Task Run()
    {
      //RigConf rigConf = RigConf.Instance;
      sigConnect = new SigRConnection();
      rig = new KenwoodRig(sigConnect);

      connection = await sigConnect.StartConnection($"http://{MasterHost}:{MasterPort}/masterbus");

      List<string>? groupList = new List<string>();
      groupList.Add("radio");
      groupList.Add("logging");
      groupList.Add("virtual");
      var ports = getAvailableSerialPort();
      Login(Name, groupList, ports);
    }
    public async void Login(string name, List<string>? group, List<string>? ports)
    {
      rig!.Name = name;
      try
      {
        SetupHandlerForResponses();
        await connection.InvokeAsync("Login", name, group, ports);
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error: {ex.Message}");
      }
    }

    private void SetupHandlerForResponses()
    {
      connection.On<string>("loginResponse", LoginResponse);
      connection.On<RigState>("state", OnStateChange);
      connection.On<BusConfigurationDB>("ReceiveConfiguration", (busConf) =>
      {
        var conf = JsonSerializer.Deserialize<RigConf>(busConf.Configuration);
        rig!.OpenPort(conf);
      });
    }

    private void OnStateChange(RigState state)
    {
      rig!.SetState(state);

    }

    private void LoginResponse(string message)
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
