using System;
using System.Collections.Generic;
using System.Text;
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
    public RigBusMain() { }

    public async Task Run(Options opts)
    {
      RigConf rigConf = RigConf.Instance;
      SigRConnection sigConnect = new SigRConnection();

      HubConnection connection = await sigConnect.StartConnection($"http://{opts.Host}:{opts.Port}/masterbus");
      connection.On<BusConfiguration>("ReceiveConfigation", (busConf) =>
      {

        //var newMessage = $"{user}: {message}";
        //messagesList.Items.Add(newMessage);

      });
      var kenwood = new Kenwood(sigConnect);
      kenwood.OpenPort(rigConf);
    }
  }
}
