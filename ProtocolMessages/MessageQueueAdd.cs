﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProtocolMessages
{
    public class MessageQueueAdd : Message
    {
        public string QueueId { get; set; }
    }
}
