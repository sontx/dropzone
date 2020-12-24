using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DropZone.Protocol
{
    internal static class Extensions
    {
        public static void ConfigSocket(this TcpClient client)
        {
            client.ReceiveBufferSize = Constants.BUFFER_SIZE_SOCKET;
            client.SendBufferSize = Constants.BUFFER_SIZE_SOCKET;
        }

        public static async Task<int> RequestSendFilesAsync(this Requester requester, IEnumerable<string> files)
        {
            var builder = new StringBuilder();
            foreach (var file in files)
            {
                builder.Append(file + "|");
            }

            await requester.SendCommand(Constants.COMMAND_SEND_FILES, builder.ToString());

            var response = await requester.WaitForResponseAsync();
            if (int.TryParse(response, out var port))
                return port;

            return -1;
        }

        public static async Task SendChatAsync(this Requester requester, string message)
        {
            await requester.SendCommand(Constants.COMMAND_CHAT_MESSAGE, message);
        }

        public static async Task HandleNextRequestAsync(this Resolver resolver, CommandHandler commandHandler)
        {
            var requestData = await resolver.WaitForRequestAsync();

            if (requestData.Command == Constants.COMMAND_SEND_FILES)
            {
                if (commandHandler.HandleSendFilesAsync != null)
                {
                    var files = requestData.Data.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    await commandHandler.HandleSendFilesAsync(resolver, files);
                }
            }
            else if (requestData.Command == Constants.COMMAND_CHAT_MESSAGE)
            {
                if (commandHandler.HandleChatMessageAsync != null)
                    await commandHandler.HandleChatMessageAsync(resolver, requestData.Data);
            }
            else
            {
                if (commandHandler.HandleUnknownAsync != null)
                    await commandHandler.HandleUnknownAsync(resolver, requestData.Data);
            }
        }
    }
}