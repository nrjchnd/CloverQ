﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Akka.Actor;
using ProtocolMessages;
using QueueSystem;



namespace AkkaActorSystem
{
    public class ActorCallDistributor : ReceiveActor
    {
        QueueSystemManager queueSystem;

        /// <summary>
        /// Esta clase va a manejar la relación colas <-> miembros
        /// Mantiene una lista de todos los miembros logeados (y los que no) y el estado de su dispositivo
        /// Interactua con el ActorQueueRouter
        /// </summary>
        public ActorCallDistributor()
        {

            queueSystem = new QueueSystemManager();

            Receive<MessageMemberLogin>(mlin =>
            {
                //Mensaje que proviene del ActorMemberLoginService, aca creo un nuevo member, cuando me llegan los QMemberAdd creo los
                //QueueMember en base a este objeto. El member quue creo aca tambien recibe mensajes del stateprovider
                queueSystem.MemberCache.Add(new Member() { Id = mlin.MemberId, Name = mlin.Name, Contact = mlin.Contact, Password = mlin.Password, DeviceId = mlin.DeviceId });
            });
            // Ejemplo de filtro de mensaje: Receive<String>(s => s.Equals("Start"), (s) => { proxyClient.Connect(); }); //ejemplito
            //Es estado del del sipositivo de
            Receive<MessageDeviceStateChanged>(dsc =>
            {
                Member member = queueSystem.MemberCache.GetMemberById(dsc.MemberId);
                //verifico que sea del mismo dispositivo
                if(dsc.DeviceId == member.DeviceId)
                {
                    member.Contact = dsc.Contact;
                    member.DeviceIsInUse = dsc.IsInUse;
                    member.EndpointIsOfline = dsc.IsOffline;
                    Console.WriteLine("CALL DIST: member STATE changed, Contact: " + member.Contact + ", IsInUse: " + member.DeviceIsInUse);
                }
            });
            Receive<MessageNewCall>(nc =>
            {
                Queue queue = queueSystem.QueueCache.GetQueue(nc.QueueId);
                Call call = new Call() { CallHandlerId = nc.CallHandlerId };
                QueueMember queueMember = null;
                if (queue != null)
                    queueMember = queue.AddCall(call); //agrega la llamada y si hay un qm para atenderla lo devuelve
                Sender.Tell(new MessageAnswerCall() { CallHandlerId = nc.CallHandlerId, MediaType = "MoH", Media = "default" });
                if (queueMember == null)
                {
                    Sender.Tell(new MessageCallQueued() { CallHandlerId = nc.CallHandlerId });
                    //TODO: remover MessageCallTo de aca, es solo para prueba
                    Sender.Tell(new MessageCallTo() { CallHandlerId = nc.CallHandlerId, Destination = queueSystem.MemberCache.GetMemberById("3333").Contact });
                }
                else
                {
                    call.IsDispatching = true;
                    Sender.Tell(new MessageCallTo() { CallHandlerId = nc.CallHandlerId, Destination = queueMember.Member.Contact });
                }
            });
            Receive<MessageCallToFailed>(ctf =>
            {
                //Busco otro member
                Console.WriteLine("CALL DIST: callto failed with code: " + ctf.Code.ToString() + " Reason: " + ctf.Reason );
            });
            Receive<MessageCallToSuccess>(cts =>
            {
                //Busco otro member
                Console.WriteLine("CALL DIST: callto success");
            });
            Receive<MessageCallerHangup>(chup =>
            {
                //si caller hangup termino toda la llamad?
                Console.WriteLine("CALL DIST: Caller Hangup");
            });
            Receive<MessageAgentHangup>(ahup =>
            {
                //Si agent hangup hago que la llamada del caller siga en el dialplan?
                Console.WriteLine("CALL DIST: Agent Hangup");
            });
            Receive<MessageCallTransfer>(ctrans =>
            {
                //Si agent hangup hago que la llamada del caller siga en el dialplan?
                Console.WriteLine("CALL DIST: Call Trasnfer: dst: "  + ctrans.TargetName);
            });
        }
        protected override void Unhandled(object message)
        {
            base.Unhandled(message);
            Console.WriteLine("CallDistributor mensaje no manejado", message.ToString());
        }
    }
}
