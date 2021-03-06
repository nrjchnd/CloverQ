﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProtocolMessages
{
    //lo anvia el actor para que el call manager atienda la llamada
    public class MessageAnswerCall : Message
    {
        //deberia ser un enumerado, pero va como string pensando en la serialización, ej MOH
        public string MediaType { get; set; }
        //indica que va a escuchar el que llama mientras espera
        public string Media { get; set; }
        //indica el timeout de la cola
        public int TimeOut { get; set; }
    }
}
