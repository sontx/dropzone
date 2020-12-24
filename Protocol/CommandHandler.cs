using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DropZone.Protocol
{
    public class CommandHandler
    {
        public Func<Resolver, IEnumerable<string>, Task> HandleSendFilesAsync { get; set; }

        public Func<Resolver, string, Task> HandleChatMessageAsync { get; set; }

        public Func<Resolver, string, Task> HandleUnknownAsync { get; set; }
    }
}