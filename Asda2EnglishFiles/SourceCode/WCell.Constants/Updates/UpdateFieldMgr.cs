using System;

namespace WCell.Constants.Updates
{
    public static class UpdateFieldMgr
    {
        public static readonly UpdateFieldCollection[] Collections = new UpdateFieldCollection[9];
        public static readonly ObjectTypeId[] InheritedTypeIds = new ObjectTypeId[9];
        public static int ExplorationZoneFieldSize;

        public static void Init()
        {
            UpdateFieldMgr.InitInheritance();
            UpdateFieldMgr.FixFields();
            for (ObjectTypeId id = ObjectTypeId.Object; id < ObjectTypeId.Count; ++id)
            {
                UpdateField[] fields = (UpdateField[]) UpdateFields.AllFields[(int) id].Clone();
                int offset = int.MaxValue;
                bool hasPrivateFields = false;
                UpdateField updateField1 = (UpdateField) null;
                for (int index = 0; index < fields.Length; ++index)
                {
                    UpdateField updateField2 = fields[index];
                    if (updateField2 != null)
                    {
                        if (offset == int.MaxValue)
                            offset = (int) updateField2.Offset;
                        updateField1 = updateField2;
                        hasPrivateFields = hasPrivateFields ||
                                           (updateField2.Flags & UpdateFieldFlags.Private) != UpdateFieldFlags.None;
                    }
                    else if (updateField1 != null)
                        fields[index] = updateField1;
                }

                ObjectTypeId inheritedTypeId = UpdateFieldMgr.InheritedTypeIds[(int) id];
                UpdateFieldCollection baseCollection;
                if (inheritedTypeId != ObjectTypeId.None)
                {
                    baseCollection = UpdateFieldMgr.Collections[(int) inheritedTypeId];
                    if (baseCollection.Fields.Length >= fields.Length)
                        throw new Exception("BaseCollection of UpdateFields equal or bigger than inherited collection");
                    for (int index = 0; index < baseCollection.Fields.Length; ++index)
                    {
                        UpdateField field = baseCollection.Fields[index];
                        fields[index] = field;
                    }
                }
                else
                    baseCollection = (UpdateFieldCollection) null;

                UpdateFieldMgr.Collections[(int) id] =
                    new UpdateFieldCollection(id, fields, baseCollection, offset, hasPrivateFields);
            }
        }

        /// <summary>Looks a little ugly but sadly is very important</summary>
        private static void FixFields()
        {
            UpdateFieldMgr.ExplorationZoneFieldSize = (int) UpdateFields.AllFields[4][1041].Size;
        }

        private static void InitInheritance()
        {
            UpdateFieldMgr.InheritedTypeIds[0] = ObjectTypeId.None;
            UpdateFieldMgr.InheritedTypeIds[1] = ObjectTypeId.Object;
            UpdateFieldMgr.InheritedTypeIds[2] = ObjectTypeId.Item;
            UpdateFieldMgr.InheritedTypeIds[3] = ObjectTypeId.Object;
            UpdateFieldMgr.InheritedTypeIds[4] = ObjectTypeId.Unit;
            UpdateFieldMgr.InheritedTypeIds[5] = ObjectTypeId.Object;
            UpdateFieldMgr.InheritedTypeIds[6] = ObjectTypeId.Object;
            UpdateFieldMgr.InheritedTypeIds[7] = ObjectTypeId.Object;
            UpdateFieldMgr.InheritedTypeIds[8] = ObjectTypeId.None;
        }

        public static UpdateFieldCollection Get(ObjectTypeId type)
        {
            if (UpdateFieldMgr.Collections[0] == null)
                UpdateFieldMgr.Init();
            return UpdateFieldMgr.Collections[(int) type];
        }
    }
}