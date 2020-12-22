using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DropZone.Protocol
{
    internal static class Extensions
    {
        public static async Task<int> RequestSendFiles(this Requester requester, IEnumerable<string> files)
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

        public static async Task<IEnumerable<string>> WaitForFilesAsync(this Resolver resolver)
        {
            var requestData = await resolver.WaitForRequestAsync();

            if (requestData.Command != Constants.COMMAND_SEND_FILES)
                return new List<string>(0);

            return requestData.Data.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}