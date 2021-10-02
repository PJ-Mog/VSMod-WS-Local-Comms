using System;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;

namespace WSLocalComms {
  public class WSLocalCommsMod : ModSystem {
    public override bool ShouldLoad(EnumAppSide forSide) {
      return forSide == EnumAppSide.Server;
    }
    public override void StartServerSide(ICoreServerAPI sapi) {
      base.StartServerSide(sapi);
      PlayerChatDelegate chatDelegate = (IServerPlayer speakingPlayer, int channelId, ref string message, ref string data, BoolRef consumed) => {
        var speakingPlayerPos = speakingPlayer.Entity.Pos;

        var reachablePlayers = from player in sapi.World.AllOnlinePlayers
                               where Math.Abs(player.Entity.Pos.X - speakingPlayerPos.X) <= 1000 && Math.Abs(player.Entity.Pos.Z - speakingPlayerPos.Z) <= 1000 && player.ClientId != speakingPlayer.ClientId
                               select player;

        foreach (var listeningPlayer in reachablePlayers) {
          if ((channelId == GlobalConstants.AllChatGroups || channelId == GlobalConstants.GeneralChatGroup || channelId == GlobalConstants.CurrentChatGroup || channelId == GlobalConstants.ServerInfoChatGroup || listeningPlayer.GetGroup(channelId) != null)) {
            sapi.SendMessage(listeningPlayer, channelId, message, EnumChatType.OthersMessage, data);
          }
        }

        if (reachablePlayers.Count() > 0) {
          sapi.SendMessage(speakingPlayer, channelId, message, EnumChatType.OwnMessage, data);
        }
        else {
          sapi.SendMessage(speakingPlayer, channelId, "It doesn't seem like anyone heard you.", EnumChatType.Notification, data);
        }
        consumed.value = true;
      };
      sapi.Event.PlayerChat += chatDelegate;
    }
  }
}
