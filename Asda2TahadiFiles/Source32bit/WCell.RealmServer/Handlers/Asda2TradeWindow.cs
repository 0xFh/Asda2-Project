using System;
using System.Collections.Generic;
using System.Linq;
using WCell.RealmServer.Asda2_Items;
using WCell.RealmServer.Entities;
using WCell.Util.NLog;

namespace WCell.RealmServer.Handlers
{
  public class Asda2TradeWindow
  {
    private Asda2ItemTradeRef[] _firstCharacterItems;
    private Asda2ItemTradeRef[] _secondCharacterItems;
    private bool _accepted;
    private bool _cleanuped;

    public Asda2TradeType TradeType { get; set; }

    public bool Accepted
    {
      get { return _accepted; }
      set
      {
        _accepted = value;
        if(!value)
          return;
        ++FisrtChar.Stunned;
        ++SecondChar.Stunned;
        Asda2TradeHandler.SendTradeStartedResponse(FisrtChar.Client, Asda2TradeStartedStatus.Started,
          SecondChar, TradeType == Asda2TradeType.RedularTrade);
        Asda2TradeHandler.SendTradeStartedResponse(SecondChar.Client, Asda2TradeStartedStatus.Started,
          FisrtChar, TradeType == Asda2TradeType.RedularTrade);
      }
    }

    public Asda2TradeWindow()
    {
      Created = DateTime.Now;
    }

    public DateTime Created { get; private set; }

    public Character FisrtChar { get; set; }

    public Character SecondChar { get; set; }

    public Asda2ItemTradeRef[] FirstCharacterItems
    {
      get { return _firstCharacterItems ?? (_firstCharacterItems = new Asda2ItemTradeRef[5]); }
    }

    public Asda2ItemTradeRef[] SecondCharacterItems
    {
      get { return _secondCharacterItems ?? (_secondCharacterItems = new Asda2ItemTradeRef[5]); }
    }

    public bool Expired
    {
      get
      {
        if(!Accepted)
          return Created - DateTime.Now > new TimeSpan(0, 0, 30);
        return false;
      }
    }

    public void CancelTrade()
    {
      if(FisrtChar != null)
      {
        Asda2TradeHandler.SendTradeRejectedResponse(FisrtChar.Client);
        FisrtChar.Asda2TradeWindow = null;
      }

      if(SecondChar != null)
      {
        Asda2TradeHandler.SendTradeRejectedResponse(SecondChar.Client);
        SecondChar.Asda2TradeWindow = null;
      }

      CleanUp();
    }

    private void CleanUp()
    {
      if(_cleanuped)
        return;
      _cleanuped = true;
      --FisrtChar.Stunned;
      --SecondChar.Stunned;
      FisrtChar.Asda2TradeWindow = null;
      SecondChar.Asda2TradeWindow = null;
      FisrtChar = null;
      SecondChar = null;
      _firstCharacterItems = null;
      _secondCharacterItems = null;
    }

    private Asda2PushItemToTradeStatus SetCharacterItemRefs(Character character, Asda2ItemTradeRef itemRef)
    {
      if(itemRef.Item.IsDeleted)
      {
        character.YouAreFuckingCheater("Trying to add to trade deleted item.", 1);
        CancelTrade();
        return Asda2PushItemToTradeStatus.AnErrorWasFoundWithTransferedItem;
      }

      if(itemRef.Item.Record == null)
      {
        character.YouAreFuckingCheater("Trying to add to trade item record null.", 1);
        CancelTrade();
        return Asda2PushItemToTradeStatus.AnErrorWasFoundWithTransferedItem;
      }

      Asda2ItemTradeRef[] asda2ItemTradeRefArray = FisrtChar != character
        ? (SecondChar == character ? SecondCharacterItems : null)
        : FirstCharacterItems;
      if(asda2ItemTradeRefArray == null || itemRef.Item == null ||
         itemRef.Item.Amount < itemRef.Amount && itemRef.Item.ItemId != 20551 || itemRef.Item.ItemId == 20551 &&
         itemRef.Amount >= character.Money)
      {
        character.YouAreFuckingCheater("Trying to cheat while trading (1).", 50);
        CancelTrade();
        return Asda2PushItemToTradeStatus.AnErrorWasFoundWithTransferedItem;
      }

      Asda2ItemTradeRef asda2ItemTradeRef =
        asda2ItemTradeRefArray.FirstOrDefault(
          i =>
          {
            if(i != null)
              return i.Item == itemRef.Item;
            return false;
          });
      if(asda2ItemTradeRef != null)
      {
        if(asda2ItemTradeRef.Amount + itemRef.Amount > itemRef.Item.Amount && itemRef.Item.ItemId != 20551 ||
           asda2ItemTradeRef.Amount + itemRef.Amount >= character.Money &&
           itemRef.Item.ItemId == 20551)
        {
          character.YouAreFuckingCheater("Trying to cheat while trading (2).", 50);
          CancelTrade();
          return Asda2PushItemToTradeStatus.AnErrorWasFoundWithTransferedItem;
        }

        asda2ItemTradeRef.Amount += itemRef.Amount;
        return Asda2PushItemToTradeStatus.Ok;
      }

      int index1 = -1;
      for(int index2 = 0; index2 < 5; ++index2)
      {
        if(asda2ItemTradeRefArray[index2] == null)
        {
          index1 = index2;
          break;
        }
      }

      if(index1 == -1)
      {
        character.YouAreFuckingCheater("Trying to cheat while trading (3).", 50);
        CancelTrade();
        return Asda2PushItemToTradeStatus.AnErrorWasFoundWithTransferedItem;
      }

      if(itemRef.Item.IsSoulbound)
        return Asda2PushItemToTradeStatus.ItemCantBeTraded;
      asda2ItemTradeRefArray[index1] = itemRef;
      return Asda2PushItemToTradeStatus.Ok;
    }

    public Asda2PushItemToTradeStatus PushItemToTrade(Character activeCharacter, short cellNum, int quantity,
      byte invNum, ref Asda2ItemTradeRef item)
    {
      if(cellNum < 0 || cellNum >= activeCharacter.Asda2Inventory.ShopItems.Length ||
         quantity < 0 || (activeCharacter == FisrtChar && FirstCharShowItems ||
                          activeCharacter == SecondChar && SecondCharShowItems))
      {
        activeCharacter.YouAreFuckingCheater("Trying to cheat while pushing item to trade.", 50);
        CancelTrade();
        return Asda2PushItemToTradeStatus.AnErrorWasFoundWithTransferedItem;
      }

      if(invNum == 2 && cellNum == 0)
        return SetCharacterItemRefs(activeCharacter, item = new Asda2ItemTradeRef
        {
          Amount = quantity,
          Item = activeCharacter.Asda2Inventory.RegularItems[cellNum]
        });
      if(TradeType == Asda2TradeType.RedularTrade)
        return SetCharacterItemRefs(activeCharacter, item = new Asda2ItemTradeRef
        {
          Amount = quantity,
          Item = activeCharacter.Asda2Inventory.RegularItems[cellNum]
        });
      return SetCharacterItemRefs(activeCharacter, item = new Asda2ItemTradeRef
      {
        Amount = quantity,
        Item = activeCharacter.Asda2Inventory.ShopItems[cellNum]
      });
    }

    public void PopItem(Character character, byte inv, short cell)
    {
      if(character == FisrtChar && FirstCharShowItems ||
         character == SecondChar && SecondCharShowItems)
      {
        character.YouAreFuckingCheater("Poping item prom trade with prong params", 20);
        CancelTrade();
      }

      if(FisrtChar == character)
      {
        Asda2ItemTradeRef[] firstCharacterItems = FirstCharacterItems;
      }

      Asda2ItemTradeRef[] asda2ItemTradeRefArray =
        SecondChar == character ? SecondCharacterItems : null;
      if(asda2ItemTradeRefArray == null)
      {
        character.YouAreFuckingCheater("Trying to pop items from trade while items not initialized", 20);
        CancelTrade();
      }
      else
      {
        int index1 = -1;
        for(int index2 = 0; index2 < 5; ++index2)
        {
          if(asda2ItemTradeRefArray[index2] != null &&
             asda2ItemTradeRefArray[index2].Item.InventoryType == (Asda2InventoryType) inv &&
             asda2ItemTradeRefArray[index2].Item.Slot == cell)
          {
            index1 = index2;
            break;
          }
        }

        if(index1 == -1)
        {
          character.YouAreFuckingCheater("Trying to pop items from trade from frong slot", 20);
          CancelTrade();
        }
        else
        {
          asda2ItemTradeRefArray[index1].Item = null;
          asda2ItemTradeRefArray[index1] = null;
        }
      }
    }

    public void ShowItemToOtherPlayer(Character activeCharacter)
    {
      if(activeCharacter == FisrtChar && !FirstCharShowItems)
      {
        FirstCharShowItems = true;
        Asda2TradeHandler.SendConfimTradeFromOponentResponse(SecondChar.Client, FirstCharacterItems);
      }
      else
      {
        if(activeCharacter != SecondChar || SecondCharShowItems)
          return;
        SecondCharShowItems = true;
        Asda2TradeHandler.SendConfimTradeFromOponentResponse(FisrtChar.Client, SecondCharacterItems);
      }
    }

    protected bool FirstCharShowItems { get; set; }

    protected bool SecondCharShowItems { get; set; }

    public void Init()
    {
    }

    public void ConfirmTrade(Character activeCharacter)
    {
      if(_cleanuped)
        return;
      if(!FirstCharShowItems || !SecondCharShowItems)
      {
        activeCharacter.YouAreFuckingCheater("Confirm trade while items not shown", 40);
        CancelTrade();
      }
      else
      {
        if(activeCharacter == FisrtChar)
        {
          if(FirstCharConfirmedTrade)
            return;
          FirstCharConfirmedTrade = true;
        }
        else if(activeCharacter == SecondChar)
        {
          if(SecondCharConfirmedTrade)
            return;
          SecondCharConfirmedTrade = true;
        }

        if(!FirstCharConfirmedTrade || !SecondCharConfirmedTrade)
          return;
        TransferItems();
        CleanUp();
      }
    }

    private void TransferItems()
    {
      if(_cleanuped || ItemsTransfered)
        return;
      ItemsTransfered = true;
      int num1 = FirstCharacterItems.Count(
        i =>
        {
          if(i != null && i.Item != null)
            return i.Item.ItemId != 20551;
          return false;
        });
      int num2 = SecondCharacterItems.Count(
        i =>
        {
          if(i != null && i.Item != null)
            return i.Item.ItemId != 20551;
          return false;
        });
      if(TradeType == Asda2TradeType.RedularTrade)
      {
        if(SecondChar.Asda2Inventory.FreeRegularSlotsCount < num1 ||
           FisrtChar.Asda2Inventory.RegularItems.Count(
             i => i == null) < num2)
        {
          FisrtChar.SendSystemMessage("Check free space in inventory!.");
          SecondChar.SendSystemMessage("Check free space in inventory!.");
          CancelTrade();
          return;
        }
      }
      else if(SecondChar.Asda2Inventory.ShopItems.Count(
                i => i == null) < num1 ||
              FisrtChar.Asda2Inventory.ShopItems.Count(
                i => i == null) < num2)
      {
        FisrtChar.SendSystemMessage("Check free space in inventory!.");
        SecondChar.SendSystemMessage("Check free space in inventory!.");
        CancelTrade();
        return;
      }

      Asda2Item[] items1 =
        Transfer(FirstCharacterItems, SecondChar);
      Asda2Item[] items2 =
        Transfer(SecondCharacterItems, FisrtChar);
      if(TradeType == Asda2TradeType.RedularTrade)
      {
        Asda2TradeHandler.SendRegularTradeCompleteResponse(FisrtChar.Client, items2);
        Asda2TradeHandler.SendRegularTradeCompleteResponse(SecondChar.Client, items1);
      }
      else
      {
        Asda2TradeHandler.SendShopTradeCompleteResponse(FisrtChar.Client, items2);
        Asda2TradeHandler.SendShopTradeCompleteResponse(SecondChar.Client, items1);
      }

      FisrtChar.SendMoneyUpdate();
      SecondChar.SendMoneyUpdate();
    }

    private Asda2Item[] Transfer(IEnumerable<Asda2ItemTradeRef> items, Character rcvr)
    {
      Asda2Item[] asda2ItemArray = new Asda2Item[5];
      int index = 0;
      foreach(Asda2ItemTradeRef asda2ItemTradeRef in items)
      {
        if(asda2ItemTradeRef != null && asda2ItemTradeRef.Item != null)
        {
          Asda2Item itemToCopyStats = asda2ItemTradeRef.Item;
          if(itemToCopyStats.IsDeleted)
            LogUtil.WarnException("Trying to add to {0} item {1} which is deleted", (object) rcvr.Name,
              (object) itemToCopyStats);
          else if(itemToCopyStats.Record == null)
            LogUtil.WarnException("Trying to add to {0} item {1} which record is null", (object) rcvr.Name,
              (object) itemToCopyStats);
          else if(itemToCopyStats.ItemId == 20551)
          {
            if(!asda2ItemTradeRef.Item.OwningCharacter.SubtractMoney((uint) asda2ItemTradeRef.Amount))
            {
              asda2ItemTradeRef.Item.OwningCharacter.YouAreFuckingCheater(
                "transfering items while not enoght money", 40);
              asda2ItemTradeRef.Item.OwningCharacter.Money = 1U;
            }
            else
              rcvr.AddMoney((uint) asda2ItemTradeRef.Amount);
          }
          else
          {
            if(itemToCopyStats.Amount < asda2ItemTradeRef.Amount || asda2ItemTradeRef.Amount <= 0)
              asda2ItemTradeRef.Amount = itemToCopyStats.Amount;
            Asda2Item asda2Item = null;
            int num = (int) rcvr.Asda2Inventory.TryAdd((int) asda2ItemTradeRef.Item.Template.ItemId,
              asda2ItemTradeRef.Amount, false, ref asda2Item, new Asda2InventoryType?(), itemToCopyStats);
            itemToCopyStats.Amount -= asda2ItemTradeRef.Amount;
            asda2ItemArray[index] = asda2Item;
            ++index;
          }
        }
      }

      return asda2ItemArray;
    }

    private bool ItemsTransfered { get; set; }

    protected bool FirstCharConfirmedTrade { get; set; }

    protected bool SecondCharConfirmedTrade { get; set; }
  }
}