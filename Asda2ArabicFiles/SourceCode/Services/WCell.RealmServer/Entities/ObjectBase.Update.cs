/*************************************************************************
 *
 *   file		: ObjectBase.Update.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-14 13:00:53 +0100 (to, 14 jan 2010) $
 
 *   revision		: $Rev: 1192 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.Updates;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.UpdateFields;

namespace WCell.RealmServer.Entities
{
	public partial class ObjectBase
	{
		protected abstract UpdateType GetCreationUpdateType(UpdateFieldFlags relation);
		

		protected internal bool m_requiresUpdate;

		/// <summary> 
		/// This is a reference to <see cref="m_privateUpdateMask"/> if there are no private values in this object
		/// else its handled seperately
		/// </summary>
		protected internal UpdateMask m_publicUpdateMask;
		protected UpdateMask m_privateUpdateMask;
		protected internal int m_highestUsedUpdateIndex;

		public bool RequiresUpdate
		{
			get { return m_requiresUpdate; }
		}

		public abstract UpdateFieldHandler.DynamicUpdateFieldHandler[] DynamicUpdateFieldHandlers
		{
			get;
		}

		internal void ResetUpdateInfo()
		{
			m_privateUpdateMask.Clear();
			if (m_privateUpdateMask != m_publicUpdateMask)
			{
				m_publicUpdateMask.Clear();
			}

			//m_highestUsedUpdateIndex = 0;
			m_requiresUpdate = false;
		}

		/// <summary>
		/// Whether the given field is public
		/// </summary>
		/// <param name="fieldIndex"></param>
		/// <returns></returns>
		public bool IsUpdateFieldPublic(int fieldIndex)
		{
		    return _UpdateFieldInfos.FieldFlags[fieldIndex].HasAnyFlag(UpdateFieldFlags.Public | UpdateFieldFlags.Dynamic);
		}

		/// <summary>
		/// Whether this Object has any private Update fields
		/// </summary>
		/// <returns></returns>
		private bool HasPrivateUpdateFields
		{
			get { return _UpdateFieldInfos.HasPrivateFields; }
		}

		public virtual UpdateFieldFlags GetUpdateFieldVisibilityFor(Character chr)
		{
			return UpdateFieldFlags.Public;
		}

		public abstract void RequestUpdate();
        

		#region Write Updates
		protected internal virtual void WriteObjectCreationUpdate(Character chr)
		{
			
		}

        protected virtual void WriteUpdateFlag_0x8(PrimitiveWriter writer, UpdateFieldFlags relation)
        {
            writer.Write(EntityId.LowRaw);
        }

        protected virtual void WriteUpdateFlag_0x10(PrimitiveWriter writer, UpdateFieldFlags relation)
        {
            writer.Write(1);
        }

        protected virtual void WriteTypeSpecificMovementUpdate(PrimitiveWriter writer, UpdateFieldFlags relation, UpdateFlags updateFlags)
        {
        }

        /// <summary>
        /// Writes the major portion of the create block.
        /// This handles flags 0x20, 0x40, and 0x100, they are exclusive to each other
        /// The content depends on the object's type
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="relation"></param>
        protected virtual void WriteMovementUpdate(PrimitiveWriter writer, UpdateFieldFlags relation)
        {
        }



		protected void WriteUpdateValue(UpdatePacket packet, Character receiver, int index)
		{
			if (_UpdateFieldInfos.FieldFlags[index].HasAnyFlag(UpdateFieldFlags.Dynamic))
			{
				DynamicUpdateFieldHandlers[index](this, receiver, packet);
			}
			else
			{
				packet.Write(m_updateValues[index].UInt32);
			}
		}

		public void SendSpontaneousUpdate(Character receiver, params UpdateFieldId[] indices)
		{
			SendSpontaneousUpdate(receiver, true, indices);
		}

		public void SendSpontaneousUpdate(Character receiver, bool visible, params UpdateFieldId[] indices)
		{
			
		}

		protected void WriteSpontaneousUpdate(UpdateMask mask, UpdatePacket packet, Character receiver, UpdateFieldId[] indices, bool visible)
		{
			// create mask
            for (var i = 0; i < indices.Length; i++)
            {
            	var index = indices[i].RawId;
            	var field = UpdateFieldMgr.Get(ObjectTypeId).Fields[index];
				for (var j = 0; j < field.Size; j++)
				{
					mask.SetBit(index + j);
				}
            }

			// write mask
			mask.WriteTo(packet);

			// write values
			for (var i = mask.m_lowestIndex; i <= mask.m_highestIndex; i++)
			{
				if (mask.GetBit(i))
				{
					if (visible)
					{
						WriteUpdateValue(packet, receiver, i);
					}
					else
					{
						packet.Write(0);
					}
				}
			}
		}

		#endregion

		public RealmPacketOut CreateDestroyPacket()
		{
			var packet = new RealmPacketOut(RealmServerOpCode.SMSG_DESTROY_OBJECT, 9);
			packet.Write(EntityId);

			packet.Write((byte)0); // NEW 3.0.2

			return packet;
		}

		public void SendDestroyToPlayer(Character c)
		{
			using (var packet = CreateDestroyPacket())
			{
				c.Client.Send(packet, addEnd: false);
			}
		}

		public void SendOutOfRangeUpdate(Character receiver, HashSet<WorldObject> worldObjects)
		{
			
		}
		
		// TODO: Improve (a lot)

		#region Spontaneous UpdateBlock Creation

		protected UpdatePacket GetFieldUpdatePacket(UpdateFieldId field, uint value)
		{
			var blocks = (field.RawId >> 5) + 1;
			var emptyBlockSize = (blocks - 1) * 4;

			//UpdatePacket packet = new UpdatePacket(BufferManager.Small.CheckOut());
			var packet = new UpdatePacket { Position = 4 };

			packet.Write(1); // Update Count
			packet.Write((byte)UpdateType.Values);

			EntityId.WritePacked(packet);

			packet.Write((byte)blocks);

			//packet.TotalLength += emptyBlockSize;
			packet.Zero(emptyBlockSize);

			packet.Write(1 << (field.RawId & 31));
			packet.Write(value);

			return packet;
		}

		protected UpdatePacket GetFieldUpdatePacket(UpdateFieldId field, byte[] value)
		{
			var blocks = (field.RawId >> 5) + 1;
			var emptyBlockSize = (blocks - 1) * 4;

			//UpdatePacket packet = new UpdatePacket(BufferManager.Small.CheckOut());
			var packet = new UpdatePacket { Position = 4 };

			packet.Write(1); // Update Count
			packet.Write((byte)UpdateType.Values);

			EntityId.WritePacked(packet);

			packet.Write((byte)blocks);

			//packet.TotalLength += emptyBlockSize;
			packet.Zero(emptyBlockSize);

			packet.Write(1 << (field.RawId & 31));
			packet.Write(value);

			return packet;
		}

		

		public void PushFieldUpdateToPlayer(Character character, UpdateFieldId field, int value)
		{
			using (var packet = GetFieldUpdatePacket(field, (uint)value))
			{
				SendUpdatePacket(character, packet);
			}
		}

		public void PushFieldUpdateToPlayer(Character character, UpdateFieldId field, uint value)
		{
			using (var packet = GetFieldUpdatePacket(field, value))
			{
				SendUpdatePacket(character, packet);
			}
		}

		public void PushFieldUpdateToPlayer(Character character, UpdateFieldId field, byte[] value)
		{
			using (var packet = GetFieldUpdatePacket(field, value))
			{
				SendUpdatePacket(character, packet);
			}
		}

	    protected static void SendUpdatePacket(Character character, UpdatePacket packet)
		{
			packet.SendTo(character.Client);
		}

	    #endregion
	}
}