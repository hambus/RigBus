using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CoreHambusCommonLibrary;
using CoreHambusCommonLibrary.DataLib;
using HambusCommonLibrary;
using HamBusSig;
using Microsoft.AspNetCore.SignalR.Client;

namespace RigBus
{
  public class RigBusMain
  {
    private SigRConnection? sigConnect;
    public RigBusMain() { }

    public async Task Run(Options opts)
    {
      RigConf rigConf = RigConf.Instance;
      sigConnect = new SigRConnection();

      HubConnection connection = await sigConnect.StartConnection($"http://{opts.Host}:{opts.Port}/masterbus");
      connection.On<BusConfigurationDB>("ReceiveConfiguration", (busConf) =>
      {
        var conf = JsonSerializer.Deserialize<RigConf>(busConf.Configuration);
        Console.WriteLine($"Got configuration ");
        //var newMessage = $"{user}: {message}";
        //messagesList.Items.Add(newMessage);

      });
      var kenwood = new Kenwood(sigConnect);
      kenwood.OpenPort(rigConf);
    }
    public async void Login(string name, List<string>? group, Action<string>? cb = null)
    {
      try
      {
        sigConnect!.connection.On<string>("loginResponse", cb);
        await sigConnect.connection.InvokeAsync("Login", name, group);
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error: {ex.Message}");
      }
    }
  }
}
