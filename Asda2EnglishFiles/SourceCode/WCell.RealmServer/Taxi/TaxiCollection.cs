using System.Collections.Generic;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Handlers;

namespace WCell.RealmServer.Taxi
{
    /// <summary>
    /// Represents the relationship between the Character and his/her known TaxiNodes
    /// </summary>
    public class TaxiCollection
    {
        private readonly Character m_owner;
        private Dictionary<uint, PathNode> m_nodes;

        public TaxiCollection(Character chr)
        {
            this.m_owner = chr;
            this.m_nodes = new Dictionary<uint, PathNode>();
        }

        public Character Owner
        {
            get { return this.m_owner; }
        }

        public void AddNode(PathNode node)
        {
            this.m_nodes[node.Id] = node;
            TaxiHandler.SendTaxiPathActivated(this.m_owner.Client);
        }
    }
}