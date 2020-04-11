using WCell.Constants.Updates;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.UpdateFields;

namespace WCell.RealmServer.Entities
{
	public partial class Asda2Item
	{
		public override UpdateFieldHandler.DynamicUpdateFieldHandler[] DynamicUpdateFieldHandlers
		{
			get { return UpdateFieldHandler.DynamicItemFieldHandlers; }
		}

		protected override UpdateType GetCreationUpdateType(UpdateFieldFlags relation)
		{
			return UpdateType.Create;
		}

        // 0x8 in 3.1
		public override UpdateFlags UpdateFlags
		{
			get { return UpdateFlags.Flag_0x10; }
		}

		public override void RequestUpdate()
		{/*
			OwningCharacter.AddItemToUpdate(this);
			m_requiresUpdate = true;*/
		}

		public override UpdateFieldFlags GetUpdateFieldVisibilityFor(Character chr)
		{
			if (chr == m_owner)
			{
				return UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly | UpdateFieldFlags.GroupOnly | UpdateFieldFlags.Public;
			}
			return UpdateFieldFlags.Public;
		}

        protected override void WriteUpdateFlag_0x10(Core.Network.PrimitiveWriter writer, UpdateFieldFlags relation)
        {
            //base.WriteUpdateFlag_0x10(writer, relation);
            writer.Write(2f);
        }

	    public void DecreaseDurability(byte i,bool silent = false)
	    {
            if (Durability < i)
            {
                Durability = 0;
                OnUnEquip();
                }
            else
                Durability-=i;
            if(!silent)
                Asda2CharacterHandler.SendUpdateDurabilityResponse(OwningCharacter.Client,this);
	    }
	}
}