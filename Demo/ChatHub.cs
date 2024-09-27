using Microsoft.AspNetCore.SignalR;
using System;


namespace Demo;

public class ChatHub : Hub
{
    private static int count = 0;

    public async Task SendText(string photo, string name, string filteredMessage)
    {
        await Clients.Caller.SendAsync("ReceiveText", photo, name, filteredMessage, "caller");
        await Clients.Others.SendAsync("ReceiveText", photo, name, filteredMessage, "others");
    }

    public override async Task OnConnectedAsync()
    {
        count++;
        string photo = Context.GetHttpContext()!.Request.Query["photo"].ToString();
        string name = Context.GetHttpContext()!.Request.Query["name"].ToString();
        await Clients.All.SendAsync("UpdateStatus", count, $"<b>{name}</b> joined");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        count--;
        string name = Context.GetHttpContext()!.Request.Query["name"].ToString();
        await Clients.All.SendAsync("UpdateStatus", count, $"<b>{name}</b> left");
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendImage(string photo, string name, string url)
    {
        await Clients.Caller.SendAsync("ReceiveImage", photo, name, url, "caller");
        await Clients.Others.SendAsync("ReceiveImage", photo, name, url, "others");
    }

    public async Task SendYouTube(string photo, string name, string id)
    {
        await Clients.Caller.SendAsync("ReceiveYouTube", photo, name, id, "caller");
        await Clients.Others.SendAsync("ReceiveYouTube", photo, name, id, "others");
    }


}

public class ScreenShareHub : Hub
{
    public async Task NotifyScreenShareStarted()
    {
        // Notify other clients that screen sharing has started
        await Clients.Others.SendAsync("ScreenShareStarted");
    }

    public async Task NotifyScreenShareStopped()
    {
        // Notify other clients that screen sharing has stopped
        await Clients.Others.SendAsync("ScreenShareStopped");
    }
}