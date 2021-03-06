﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace QueueSystem
{
    public enum MemberStrategy { RRMEMORY }

    /// <summary>
    /// Representa la lista de miembros de una cola, posee una estrategia para seleccionar el próximo libre
    /// Es una lista de QueueMember no de Member
    /// </summary>
    public class QueueMemberList
    {
        /// <summary>
        /// Estrategia que contiene el algoritmo para selecciónar el próximo miembro disponible
        /// </summary>
        IMemberStrategy strategy = null;

        /// <summary>
        /// Mantiene una lista de los miembros que pertenecen a un CallDispatcher
        /// </summary>
        List<QueueMember> members = new List<QueueMember>();

        /// <summary>
        /// Cosntructor por defecto que crea una lista vacia y una estrategia rr memory por defecto
        /// </summary>
        public QueueMemberList()
        {
            members = new List<QueueMember>();
            strategy = new MemberStrategyRRM(members);
        }

        /// <summary>
        /// Cosntructor que recibe una lista de miembros
        /// </summary>
        /// <param name="members"></param>
        public QueueMemberList(List<QueueMember> members, IMemberStrategy strategy)
        {
            this.members = members;
            this.strategy = strategy;
            this.strategy.Members = this.members;

        }

        /// <summary>
        /// Método que me permite cambiar la estrategia de seleccion de miembro
        /// </summary>
        /// <param name="strategy"></param>
        public void SetMemberSrtategy(IMemberStrategy strategy)
        {
            this.strategy = strategy;
            this.strategy.Members = this.members;
        }

        /// <summary>
        /// Método que me permite cambiar la estrategia de seleccion de miembro
        /// </summary>
        /// <param name="strategy"></param>
        public void SetMemberSrtategy(MemberStrategy strategy)
        {
            switch (strategy)
            {
                case MemberStrategy.RRMEMORY:
                    SetMemberSrtategy(new MemberStrategyRRM());
                    break;
                default:
                    SetMemberSrtategy(new MemberStrategyRRM());
                    break;
            }
        }

        /// <summary>
        /// Método que me permite cambiar la estrategia de seleccion de miembro
        /// </summary>
        /// <param name="strategy"></param>
        public void SetMemberSrtategy(string strategy)
        {
            try
            {
                SetMemberSrtategy((MemberStrategy)Enum.Parse(typeof(MemberStrategy), strategy.ToUpper()));
            }
            catch (Exception ex)
            {
                Log.Logger.Debug("Error en SetMemberStrategy: " + ex.Message);
            }
        }

        /// <summary>
        /// Propiedad que permite asignar u obtener la lista subyacente de miembros
        /// </summary>
        public List<QueueMember> Members { get { return members; } set { members = value; } }

        /// <summary>
        /// Agrega un miembro a la lista
        /// </summary>
        /// <param name="member"></param>
        public void AddMember(QueueMember member)
        {
            this.members.Add(member);
        }

        /// <summary>
        /// Remueve un miembro de la lista
        /// </summary>
        /// <param name="member"></param>
        public void RemoveMember(QueueMember member)
        {
            for (int i = members.Count - 1; i >= 0; --i)
            {
                if (members[i].Id == member.Id)
                {
                    this.members.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Devuelve el próximo miembro dsiponible según la estrategia indicada
        /// </summary>
        /// <returns></returns>
        public QueueMember NextAvailable()
        {
            return strategy.GetNext();
        }

        /// <summary>
        /// Devuelve el próximo miembro dsiponible según la estrategia indicada teniendo en cuenta el WrapupTime de la cola
        /// </summary>
        /// <returns></returns>
        public QueueMember NextAvailable(int wrapupTime)
        {
            return strategy.GetNext(wrapupTime);
        }
    }
}
