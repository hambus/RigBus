using System;
using System.Collections.Generic;

using System.IO.Ports;
using System.Threading.Tasks;
using CoreHambusCommonLibrary.Model;
using CoreHambusCommonLibrary.Networking;
using HamBusCommonStd;
using Microsoft.AspNetCore.SignalR.Client;



namespace RigBus
{
  public class RigBusMain: Bus
  {
    public KenwoodRig? rig { get; set; }
    private SigRConnection? sigRConn;
    //private HubConnection? connection;


    public async Task Run()
    {
      sigRConn = SigRConnection.Instance;

      sigRConn.RigState__.Subscribe<RigState>(state => OnStateChange(state));

      rig = new KenwoodRig();

      connection = await sigRConn.StartConnection($"http://{MasterHost}:{MasterPort}/masterbus");

      List<string>? groupList = new List<string>();
      groupList.Add("radio");
      groupList.Add("logging");
      groupList.Add("virtual");
      var ports = GetAvailableSerialPort();
      
      Login(Name, groupList, ports);
    }
    public async void Login(string name, List<string>? group, List<string>? ports)
    {
      rig!.Name = name;
      try
      {
        await connection.InvokeAsync("Login", name, group);
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error: {ex.Message}");
      }
    }


    private void OnStateChange(RigState state)
    {
      rig!.PausePolling = true;
      Console.WriteLine($"test of serial #{ state.SerialNum}");
      rig!.SetStateFromBus(state);
      rig!.PausePolling = false;

    }

    private List<string> GetAvailableSerialPort()
    {
      List<string> list = new List<string>(SerialPort.GetPortNames());
      return list;

    }
  }
}
