﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsterNET.ARI.Models;
using AsterNET.ARI;
using ProtocolMessages;

namespace TestRouter
{
    public class CallHandler
    {
        enum CallState { NEW, ANSWERED, CONNECTIING, CONNECT_FAILDED, CONNECTED, AGENT_ANSWERED, TRANSFERRED, TERMINATED };

        string id;
        string appName;
        string currentQueue;
        AriClient pbx;
        Bridge bridge;
        Channel caller;
        Channel agent;
        Channel transferTarget;

        CallState callState;

        public string Id
        {
            get
            {
                return id;
            }
        }
        public Bridge Bridge
        {
            get
            {
                return bridge;
            }

            set
            {
                bridge = value;
            }
        }
        public Channel Caller
        {
            get
            {
                return caller;
            }

            set
            {
                caller = value;
            }
        }
        public Channel Agent
        {
            get
            {
                return agent;
            }

            set
            {
                agent = value;
            }
        }
        public string CurrentQueue
        {
            get
            {
                return currentQueue;
            }
        }


        //Constructor
        public CallHandler(string appName, AriClient pbx, Bridge bridge, Channel caller)
        {
            callState = CallState.NEW;
            this.id = Guid.NewGuid().ToString();
            this.appName = appName;
            this.pbx = pbx;
            this.bridge = bridge;
            bridge.Channels.Add(caller.Id);
            this.caller = caller;
            this.agent = null;

        }

        //significa EnterQueue
        public ProtocolMessages.Message SetCurrentQueue(string queueId) {
            currentQueue = queueId;
            return new MessageNewCall() { CallHandlerId = id, QueueId = queueId, ChannelId = caller.Id };

        }

        public Channel CallTo(string dst)
        {

            try
            {
                agent = pbx.Channels.Originate(dst, null, null, null, null, appName, "", "1111", 20, null, null, null, null);
                callState = CallState.CONNECTIING;
                bridge.Channels.Add(agent.Id);
            }
            catch (Exception ex)
            {
                throw new Exception("Error llamando a:  " + dst, ex);
            }

            return agent;
        }

        public void CallToSuccess(string channelId)
        {
            if (this.agent.Id != channelId)
            {
                throw new Exception("Callhandler: CallToSucces: " + channelId + " no es un canal de agente: ");
            }

            try
            {
                pbx.Bridges.StopMoh(this.Bridge.Id);
                //agrego el canal al bridge, controlar que pasa si falla el originate
                pbx.Bridges.AddChannel(this.Bridge.Id, channelId, null);
                callState = CallState.CONNECTED;
            }
            catch (Exception ex)
            {
                throw new Exception("Callhandler: Error al agregar el agent: " + channelId + " al bridge: " + bridge.Id, ex);
            }

        }

        public void AnswerCaller(string mediaType, string media)
        {
            try
            {
                //atiendo el caller
                pbx.Channels.Answer(caller.Id);
                //agrego el canal al bridge
                pbx.Bridges.AddChannel(bridge.Id, caller.Id, null);
                //inicio musica en espera si playMOH es true
                if (!String.IsNullOrEmpty(mediaType)) pbx.Bridges.StartMoh(bridge.Id, media);
                callState = CallState.ANSWERED;

            }
            catch (Exception ex)
            {
                throw new Exception("Callhandler: Error al agregar el caller: " + caller.Id + " al bridge: " + bridge.Id, ex);
            }
        }

        public ProtocolMessages.Message ChannelStateChangedEvent(string channelId, string newState)
        {
            ProtocolMessages.Message msg = null;

            if (channelId == caller.Id)
            {
                caller.State = newState;
            }
            else if (channelId == agent.Id)
            {
                agent.State = newState;
                if (newState == "Up") //Esto indica que el canal es de un agente y atendió la llamada
                {
                    msg = new MessageCallToSuccess() { CallHandlerId = this.id, QueueId = currentQueue };
                    callState = CallState.AGENT_ANSWERED;
                }
            }
            else
            {
                Console.WriteLine("Callhandler: El canal " + caller.Id + " no está en la llamada: " + this.id);
            }

            return msg;
        }

        //por lo que pude relevar el evento channelDestry solo llega si el canal no fué atendido
        //hay que ver bien cause que valores toma en los casos de una llamada terminada por timeout, o por falla
        //basado en lo que dije en las lineas anteriores, no debería llegar aca por un canal de caller, ya que al entrar en stasis
        //debería haber sido atendido. Si pasa con las llamadas que inicio hacia los agentes
        public ProtocolMessages.Message ChannelDestroyEvent(string channelId, int cause, string causeText)
        {
            ProtocolMessages.Message msg = null;
            //Hay que pulir la lógica del hangup, también hay que tener en cuenta los transfer
            if (channelId == caller.Id && callState != CallState.TERMINATED)
            {
                msg = new MessageCallerHangup() { CallHandlerId = this.id, QueueId = currentQueue, HangUpCode = cause.ToString(), HangUpReason = causeText };
                callState = CallState.TERMINATED;
            }
            else if (channelId == agent.Id)
            {
                msg = new MessageCallToFailed() { CallHandlerId = this.id, QueueId = currentQueue, Code = cause, Reason = causeText };
                callState = CallState.CONNECT_FAILDED;
            }
            else
                Console.WriteLine("Callhandler: El canal " + caller.Id + " no está en la llamada: " + this.id);

            return msg;
        }

        //por lo que pude relevar el evento channelHangup solo llega si el canal no atendido
        //hay que ver bien cause que valores toma en los casos de una llamada terminada por falla
        //basado en lo que dije en las lineas anteriores, aca debería llegar por un canal de caller que corta, ya que al entrar en stasis
        //debería haber sido atendido. o bien por un agente que corta
        public ProtocolMessages.Message ChannelHangupEvent(string channelId, int cause, string causeText)
        {
            ProtocolMessages.Message msg = null;
            //Hay que pulir la lógica del hangup, también hay que tener en cuenta los transfer
            if (channelId == caller.Id && callState != CallState.TERMINATED && callState != CallState.TRANSFERRED)
            {
                msg = new MessageCallerHangup() { CallHandlerId = this.id, QueueId = currentQueue, HangUpCode = cause.ToString(), HangUpReason = causeText };
                TerminateAgent();
                callState = CallState.TERMINATED;
            }
            else if (channelId == agent.Id)
            {

                //prevengo que si la llamada fue transferida le corte al que llamó
                if (callState != CallState.TRANSFERRED && callState != CallState.TERMINATED)
                {
                    msg = new MessageAgentHangup() { CallHandlerId = this.id, QueueId = currentQueue, HangUpCode = cause.ToString(), HangUpReason = causeText };
                    TerminateCaller();
                    callState = CallState.TERMINATED;
                }
                //else
                //{
                //    msg = new MessageCallTransfer() { CallHandlerId = this.id, TargetId = transferTarget.Id, TargetName = transferTarget.Name };
                //}
            }
            else
                Console.WriteLine("Callhandler: El canal " + caller.Id + " no está en la llamada: " + this.id);

            return msg;
        }

        private void TerminateCaller()
        {
            TerminateLeg(this.caller.Id);
        }
        private void TerminateAgent()
        {
            if(agent != null)
                TerminateLeg(this.agent.Id);
        }

        private void TerminateLeg(string channelId)
        {
            try
            {
                pbx.Channels.Hangup(channelId);
            }
            catch (Exception ex)
            {
                Console.WriteLine("CallHandler: " + this.id + " request hangup failed on channel: " + channelId + "Error: " + ex.Message);
            }

        }
        //TODO: esto es una versión muy simplificada, el evento de transferencia atendida requiere mayo estudio
        public ProtocolMessages.Message AttendedTransferEvent(Channel ch1, Channel ch2)
        {
            ProtocolMessages.Message msg = null;
            if (ch1.Id == caller.Id)
            {
               msg =  TransferTo(ch2);

            }

            if (ch2.Id == caller.Id)
            {
                msg = TransferTo(ch1);
            }

            return msg;

        }

        public ProtocolMessages.Message UnattendedTransferEvent(Channel ch1, Channel ch2)
        {
            return TransferTo(ch1);
        }

        private ProtocolMessages.Message TransferTo(Channel target)
        {
            ProtocolMessages.Message msg;
            this.transferTarget = target;
            callState = CallState.TRANSFERRED;
            msg = new MessageCallTransfer() { CallHandlerId = this.id, QueueId = currentQueue, TargetId = transferTarget.Id, TargetName = transferTarget.Name };
            return msg;

        }

        public void ChannelReplace(Channel replaceChannel, Channel newChannel)
        {
            if (replaceChannel.Id == caller.Id)
            {
                caller = newChannel;
            }
            else if (replaceChannel.Id == agent.Id)
            {
                agent = newChannel;
            }
            else
                Console.WriteLine("Callhandler: ChannelReplace: El canal " + caller.Id + " no está en la llamada: " + this.id);

        }
    }
}
