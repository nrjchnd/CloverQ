﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProtocolMessages
{
    public class MessageMemberLogoff : Message
    {
        public string MemberId { get; set; }
        public string Password { get; set; }
    }
}
