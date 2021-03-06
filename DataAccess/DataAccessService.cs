﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models;
using DataProviderXML;


namespace DataAccess
{
    //Esta clase levanta del medio de persistnecia que sea la siguiente info
    //Las propiedades de las colas y dentro de cada una sus queuemember con sus propiedades
    //una lista de members con sus propiedades
    //De aca se cargan las queue sus queuemember y la lista de members para el queue system
    public class DataAccessService
    {
        //esto es para desarrollo, pero tal vez podría mantener un cache intermendio con un timepo de expiracion para no 
        //consultar continuamente al storage (tal vez Hazelcast)
        Dictionary<string, DTOMember> members;
        Dictionary<string, DTOQueue> queues;
        Dictionary<string, DTOQueue> memberQueues;
        IDataProvider dataProvider = new XMLDataProvider();

        public DataAccessService()
        {
            members = new Dictionary<string, DTOMember>();
            queues = new Dictionary<string, DTOQueue>();
            memberQueues = new Dictionary<string, DTOQueue>();
            dataProvider.Connect("datos-");
            InitTestData();
        }

        private void InitTestData()
        {
            //creo members
            //DTOMember dtom1 = new DTOMember() { Id = "3333", Password = "1234", Name = "Juana" };
            //members.Add(dtom1.Id, dtom1);

            List<DTOMember> memberList = dataProvider.GetMembers();
            foreach(var m in memberList)
            {
                members.Add(m.Id, m);
            }

            //dataProvider.SaveMembers(members.Values.ToList());

            //creo colas
            //DTOQueue dtoq1 = new DTOQueue() { Id = "5000", Media = "default", MediaType = "MoH", QueueMembers = new List<DTOQueueMember>(), Weight = 1, WrapupTime = 3, MemberStrategy = "rrmemory", CallOrderStrategy = "default" };
            //DTOQueue dtoq2 = new DTOQueue() { Id = "6000", Media = "default", MediaType = "MoH", QueueMembers = new List<DTOQueueMember>(), Weight = 2, WrapupTime = 3, MemberStrategy = "rrmemory", CallOrderStrategy = "default" };
            //queues.Add(dtoq1.Id, dtoq1);
            //queues.Add(dtoq2.Id, dtoq2);

            List<DTOQueue> queuesList = dataProvider.GetQueues();
            foreach (var q in queuesList)
            {
                queues.Add(q.Id, q);
            }

            //creo queue members
            //DTOQueueMember dtoqm1 = new DTOQueueMember() { MemberId = dtom1.Id, Priority = 0 };
            //DTOQueueMember dtoqm2 = new DTOQueueMember() { MemberId = dtom2.Id, Priority = 1 };
            //agrego los queue members a las colas

            //dtoq1.QueueMembers.Add(dtoqm1);
            //dtoq2.QueueMembers.Add(dtoqm2);

        }

        #region Métodos públicos
        public List<DTOQueue> GetQueues()
        {
            return queues.Values.ToList<DTOQueue>();
        }

        public List<DTOMember> GetMembers()
        {
            return null;
        }

        public DTOMember GetMemberById(string memberId)
        {
            if (members != null && members.ContainsKey(memberId))
            {
                return members[memberId];
            }
            else
            {
                return null;
            }
        }

        //TODO: mejorar esto, está medio chancho
        public List<string> GetMemberQueues(string memberId)
        {
            //if (memberQueues != null && memberQueues.ContainsKey(memberId))
            //{
                List<string> colas = new List<string>();
                foreach (DTOQueue q in queues.Values) {
                    foreach(DTOQueueMember m in q.QueueMembers){
                        if (memberId == m.MemberId) {
                            colas.Add(q.Id);
                        }
                    }
                }
                return colas;
            //}
            //else
            //{
            //    return null;
            //}
        }
        #endregion
    }
}
