using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Constants.Spells;
using WCell.Core.Network;
using WCell.RealmServer.Asda2Titles;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Network;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;

namespace WCell.RealmServer.Handlers
{
    internal class Asda2SpellHandler
    {
        [PacketHandler(RealmServerOpCode.UseSkill)] //5256
        public static void UseSkillRequest(IRealmClient client, RealmPacketIn packet)
        {
            client.ActiveCharacter.IsFighting = false;
            //Todo send spell init packet
            client.ActiveCharacter.IsMoving = false;
            var skillId = packet.ReadInt16();//default : 927Len : 2
            packet.Position += 1;//nk1 default : 1Len : 1
            var x = packet.ReadInt16();//default : 100Len : 2
            var y = packet.ReadInt16();//default : 362Len : 2
            var targetType = packet.ReadByte();//default : 1Len : 1
            var targetId = packet.ReadUInt16();//default : 18Len : 4

            Spell spell = client.ActiveCharacter.Spells.GetSpellByRealId(skillId);
            if (spell == null) return;
            if (spell.SoulGuardProffLevel !=0)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Trying to use SoulguardSkill as normal skill.");return;
            }
            ProcessUseSkill(client, targetType, skillId, targetId);
            Asda2TitleChecker.OnSkillUsed(client.ActiveCharacter, skillId);
        }
        [PacketHandler(RealmServerOpCode.CancelSkill)]//6142
        public static void CancelSkillRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position += 1;//nk default : unkLen : 5
            var skillId = packet.ReadInt16();//default : 202Len : 2
            client.ActiveCharacter.Auras.RemoveFirstVisibleAura(a => a.Spell.RealId == skillId && a.IsBeneficial);
        }


        private static void ProcessUseSkill(IRealmClient client, byte targetType, short skillId, ushort targetId)
        {
            Unit target = null;
            if (targetType == 0)
                target = client.ActiveCharacter.Map.GetNpcByUniqMapId(targetId);
            else if (targetType == 1)
                target = World.GetCharacterBySessionId(targetId);
            else
            {
                client.ActiveCharacter.SendSystemMessage(
                    string.Format("Unknown skill target type {0}. SkillId {1}. Please report to developers.", targetType,
                                  skillId));
            }
            if (target == null)
            {
                client.ActiveCharacter.SendInfoMsg("Bad target.");
                return;
            }
            Spell spell = client.ActiveCharacter.Spells.GetSpellByRealId(skillId);
            //if (client.ActiveCharacter.MapId == WCell.Constants.World.MapId.BatleField)
            //{
            //    if (spell.Id == 2007 || spell.Id == 1760 || spell.Id == 1759 || spell.Id == 2759
            //            || spell.Id == 3759 || spell.Id == 4759 || spell.Id == 5759 || spell.Id == 1556
            //            || spell.Id == 1916 || spell.Id == 2916 || spell.Id == 3916 || spell.Id == 4916 ||
            //            spell.Id == 5916 || spell.Id == 6916 || spell.Id == 7916 || spell.Id == 1917 ||
            //            spell.Id == 2917 || spell.Id == 3917 || spell.Id == 4917 || spell.Id == 5917 ||
            //            spell.Id == 6917 || spell.Id == 7917)
            //    {
            //        client.ActiveCharacter.SendInfoMsg(" you can't use it in war");
            //        return;

            //    }
            //}
           

            var targetChar = target as Character;

            if (targetChar != null)
            {
               // client.ActiveCharacter != targetChar && !client.ActiveCharacter.MayAttack(targetChar) &&
                if (client.ActiveCharacter != targetChar && !client.ActiveCharacter.MayAttack(targetChar) && client.ActiveCharacter.MapId == WCell.Constants.World.MapId.BatleField)
                {
                    
                    if (client.ActiveCharacter.Asda2FactionId != targetChar.Asda2FactionId)
                    {
                        if (spell.Id == 1938 || spell.Id == 2938 )
                        {
                            client.ActiveCharacter.SendInfoMsg("stop doing that +)");
                            return;
                        }
                    }
                    
                    if (client.ActiveCharacter.Asda2FactionId == targetChar.Asda2FactionId)
                    {
                        /*  if (spell.RealId != 983 || spell.RealId != 534 || spell.RealId != 1020 || spell.RealId != 1069
                              || spell.RealId != 958 || spell.RealId != 1070 || spell.RealId != 993 || spell.RealId != 978
                              || spell.RealId != 585 || spell.RealId != 615 || spell.RealId != 1045 || spell.RealId != 614
                              || spell.RealId != 981 || spell.RealId != 982)*/

                        //  client.ActiveCharacter.SendInfoMsg("dont attack your friend +)");
                        //  return;
                        if (spell.RealId == 997 || spell.Id == 1820 || spell.Id == 2820 || spell.Id == 3820 || spell.Id == 4820 || spell.Id == 5820 || spell.Id == 6820 || spell.Id == 7820 || spell.Id == 1841 || spell.Id == 2841 || spell.Id == 3841 || spell.Id == 4841 || spell.Id == 5841 || spell.Id == 6841 || spell.Id == 7841 || spell.Id == 1844 || spell.Id == 2844 || spell.Id == 3844 || spell.Id == 4844 || spell.Id == 5844 || spell.Id == 6844 || spell.Id == 7844 || spell.Id == 1632 || spell.Id == 2632 || spell.Id == 3632 || spell.Id == 4632 || spell.Id == 5632 || spell.Id == 6632 || spell.Id == 7632 || spell.Id == 1632 || spell.Id == 1632 || spell.Id == 2632 || spell.Id == 3632 || spell.Id == 4632 || spell.Id == 5632 || spell.Id == 6632 || spell.Id == 7632 || spell.Id == 1635 || spell.Id == 2635 || spell.Id == 3635 || spell.Id == 4635 || spell.Id == 5635 || spell.Id == 6635 || spell.Id == 7635 || spell.Id == 1613 || spell.Id == 2613 || spell.Id == 3613 || spell.Id == 4613 || spell.Id == 5613 || spell.Id == 6613 || spell.Id == 7613 || spell.Id == 1633 || spell.Id == 2633 || spell.Id == 3633 || spell.Id == 4633 || spell.Id == 5633 || spell.Id == 6633 || spell.Id == 7633 || spell.Id == 1636 || spell.Id == 2636 || spell.Id == 3636 || spell.Id == 4636 || spell.Id == 5636 || spell.Id == 6636 || spell.Id == 7636 || spell.Id == 1634 || spell.Id == 2634 || spell.Id == 3634 || spell.Id == 4634 || spell.Id == 5634 || spell.Id == 6634 || spell.Id == 7634 || spell.Id == 1616 || spell.Id == 2616 || spell.Id == 3616 || spell.Id == 4616 || spell.Id == 5616 || spell.Id == 6616 || spell.Id == 7616 || spell.Id == 1637 || spell.Id == 2637 || spell.Id == 3637 || spell.Id == 4637 || spell.Id == 5637 || spell.Id == 6637 || spell.Id == 7637 || spell.Id == 1823 || spell.Id == 2823 || spell.Id == 3823 || spell.Id == 4823 || spell.Id == 5823 || spell.Id == 6823 || spell.Id == 7823 || spell.Id == 1842 || spell.Id == 2842 || spell.Id == 3842 || spell.Id == 4842 || spell.Id == 5842 || spell.Id == 6842 || spell.Id == 7842 || spell.Id == 1845 || spell.Id == 2845 || spell.Id == 3845 || spell.Id == 4845 || spell.Id == 5845 || spell.Id == 6845 || spell.Id == 7845 || spell.Id == 2044 || spell.Id == 3044 || spell.Id == 4044 || spell.Id == 5044 || spell.Id == 6044 || spell.Id == 7044 || spell.Id == 8044 || spell.Id == 2064 || spell.Id == 3064 || spell.Id == 4064 || spell.Id == 5064 || spell.Id == 6064 || spell.Id == 7064 || spell.Id == 8064 || spell.Id == 2065 || spell.Id == 3065 || spell.Id == 4065 || spell.Id == 5065 || spell.Id == 6065 || spell.Id == 7065 || spell.Id == 8065 || spell.Id == 2046 || spell.Id == 3046 || spell.Id == 4046 || spell.Id == 5046 || spell.Id == 6046 || spell.Id == 7046 || spell.Id == 8046 || spell.Id == 2066 || spell.Id == 3066 || spell.Id == 4066 || spell.Id == 5066 || spell.Id == 6066 || spell.Id == 7066 || spell.Id == 8066 || spell.Id == 2042 || spell.Id == 3042 || spell.Id == 4042 || spell.Id == 5042 || spell.Id == 6042 || spell.Id == 7042 || spell.Id == 8042 || spell.Id == 2062 || spell.Id == 3062 || spell.Id == 4062 || spell.Id == 5062 || spell.Id == 6062 || spell.Id == 7062 || spell.Id == 8062 || spell.Id == 2063 || spell.Id == 3063 || spell.Id == 4063 || spell.Id == 5063 || spell.Id == 6063 || spell.Id == 7063 || spell.Id == 8063 || spell.Id == 1704 || spell.Id == 1713 || spell.Id == 1756 || spell.Id == 1849 || spell.Id == 1078 || spell.Id == 2078 || spell.Id == 1535 || spell.Id == 1904 || spell.Id == 1073 || spell.Id == 1903 || spell.Id == 2903 || spell.Id == 3078 || spell.Id == 2904 || spell.Id == 3073 || spell.Id == 2704 || spell.Id == 2713 || spell.Id == 2756 || spell.Id == 2849 || spell.Id == 3903 || spell.Id == 4078 || spell.Id == 3904 || spell.Id == 4073 || spell.Id == 3704 || spell.Id == 3713 || spell.Id == 3756 || spell.Id == 3849 || spell.Id == 4903 || spell.Id == 5078 || spell.RealId == 913 || spell.Id == 4904 || spell.Id == 5073 || spell.Id == 4704 || spell.Id == 4713 || spell.Id == 4756 || spell.Id == 4849 || spell.Id == 5903 || spell.Id == 6078 || spell.Id == 5904 || spell.Id == 6073 || spell.Id == 5704 || spell.Id == 5713 || spell.Id == 5756 || spell.Id == 6849 || spell.Id == 6903 || spell.Id == 7078 || spell.Id == 6904 || spell.Id == 7073 || spell.Id == 6704 || spell.Id == 6713 || spell.Id == 6756 || spell.Id == 7849 || spell.Id == 7903 || spell.Id == 8078 || spell.Id == 7904 || spell.Id == 8073 || spell.Id == 7704 || spell.Id == 7713 || spell.Id == 7756 || spell.Id == 8849 || spell.Id == 1507 || spell.Id == 1508 || spell.Id == 1641 || spell.Id == 2079 || spell.Id == 1606 || spell.Id == 1902 || spell.Id == 1501 || spell.Id == 1502 || spell.Id == 1503 || spell.Id == 1701 || spell.Id == 1702 || spell.Id == 1708 || spell.Id == 1848 || spell.Id == 1504 || spell.Id == 1505 || spell.Id == 1640 || spell.Id == 1908 || spell.Id == 1905 || spell.Id == 1912 || spell.Id == 2507 || spell.Id == 2508 || spell.Id == 2641 || spell.Id == 3079 || spell.Id == 2606 || spell.Id == 2902 || spell.Id == 2501 || spell.Id == 2502 || spell.Id == 2503 || spell.Id == 2701 || spell.Id == 2702 || spell.Id == 2708 || spell.Id == 2848 || spell.Id == 2504 || spell.Id == 2505 || spell.Id == 2640 || spell.Id == 2908 || spell.Id == 2905 || spell.Id == 2912 || spell.Id == 3507 || spell.Id == 3508 || spell.Id == 3641 || spell.Id == 4079 || spell.Id == 3606 || spell.Id == 3902 || spell.Id == 3501 || spell.Id == 3502 || spell.Id == 3503 || spell.Id == 3701 || spell.Id == 3702 || spell.Id == 3708 || spell.Id == 3848 || spell.Id == 3504 || spell.Id == 3505 || spell.Id == 3640 || spell.Id == 3908 || spell.Id == 3905 || spell.Id == 3912 || spell.Id == 4507 || spell.Id == 4508 || spell.Id == 4641 || spell.Id == 5079 || spell.Id == 4606 || spell.Id == 4902 || spell.Id == 4501 || spell.Id == 4502 || spell.Id == 4503 || spell.Id == 4701 || spell.Id == 4702 || spell.Id == 4708 || spell.Id == 4848 || spell.Id == 4504 || spell.Id == 4505 || spell.Id == 4640 || spell.Id == 4908 || spell.Id == 4905 || spell.Id == 4912 || spell.Id == 5507 || spell.Id == 5508 || spell.Id == 5641 || spell.Id == 6079 || spell.Id == 5606 || spell.Id == 5902 || spell.Id == 5501 || spell.Id == 5502 || spell.Id == 5503 || spell.Id == 5701 || spell.Id == 5702 || spell.Id == 5708 || spell.Id == 5848 || spell.Id == 5504 || spell.Id == 5505 || spell.Id == 5640 || spell.Id == 5908 || spell.Id == 5905 || spell.Id == 5912 || spell.Id == 6507 || spell.Id == 6508 || spell.Id == 6641 || spell.Id == 7079 || spell.Id == 6606 || spell.Id == 6902 || spell.Id == 6501 || spell.Id == 6502 || spell.Id == 6503 || spell.Id == 6701 || spell.Id == 6702 || spell.Id == 6708 || spell.Id == 6848 || spell.Id == 6504 || spell.Id == 6505 || spell.Id == 6640 || spell.Id == 6908 || spell.Id == 6905 || spell.Id == 6912 || spell.Id == 7507 || spell.Id == 7508 || spell.Id == 7641 || spell.Id == 8079 || spell.Id == 7606 || spell.Id == 7902 || spell.Id == 7501 || spell.Id == 7502 || spell.Id == 7503 || spell.Id == 7701 || spell.Id == 7702 || spell.Id == 7708 || spell.Id == 7848 || spell.Id == 7504 || spell.Id == 7505 || spell.Id == 7640 || spell.Id == 7908 || spell.Id == 7905 || spell.Id == 7912 || spell.Id == 1509 || spell.Id == 1514 || spell.Id == 1709 || spell.Id == 1712 || spell.Id == 1506 || spell.Id == 1511 || spell.Id == 1901 || spell.Id == 1907 || spell.Id == 1914 || spell.Id == 1935 || spell.Id == 1909 || spell.Id == 1916 || spell.Id == 1706 || spell.Id == 1705 || spell.Id == 1602 || spell.Id == 2509 || spell.Id == 2514 || spell.Id == 2709 || spell.Id == 2712 || spell.Id == 2506 || spell.Id == 2511 || spell.Id == 2901 || spell.Id == 2907 || spell.Id == 2914 || spell.Id == 2935 || spell.Id == 2909 || spell.Id == 2916 || spell.Id == 2706 || spell.Id == 2705 || spell.Id == 2602 || spell.Id == 3509 || spell.Id == 3514 || spell.Id == 3709 || spell.Id == 3712 || spell.Id == 3506 || spell.Id == 3511 || spell.Id == 3901 || spell.Id == 3907 || spell.Id == 3914 || spell.Id == 3935 || spell.Id == 3909 || spell.Id == 3916 || spell.Id == 3706 || spell.Id == 3705 || spell.Id == 3602 || spell.Id == 4509 || spell.Id == 4514 || spell.Id == 4709 || spell.Id == 4712 || spell.Id == 4506 || spell.Id == 4511 || spell.Id == 4901 || spell.Id == 4907 || spell.Id == 4914 || spell.Id == 4935 || spell.Id == 4909 || spell.Id == 4916 || spell.Id == 4706 || spell.Id == 4705 || spell.Id == 4602 || spell.Id == 5509 || spell.Id == 5514 || spell.Id == 5709 || spell.Id == 5712 || spell.Id == 5506 || spell.Id == 5511 || spell.Id == 5901 || spell.Id == 5907 || spell.Id == 5914 || spell.Id == 5935 || spell.Id == 5909 || spell.Id == 5916 || spell.Id == 5706 || spell.Id == 5705 || spell.Id == 5602 || spell.Id == 6509 || spell.Id == 6514 || spell.Id == 6709 || spell.Id == 6712 || spell.Id == 6506 || spell.Id == 6511 || spell.Id == 6901 || spell.Id == 6907 || spell.Id == 6914 || spell.Id == 6935 || spell.Id == 6909 || spell.Id == 6916 || spell.Id == 6706 || spell.Id == 6705 || spell.Id == 6602 || spell.Id == 7509 || spell.Id == 7514 || spell.Id == 7709 || spell.Id == 7712 || spell.Id == 7506 || spell.Id == 7511 || spell.Id == 7901 || spell.Id == 7907 || spell.Id == 7914 || spell.Id == 7935 || spell.Id == 7909 || spell.Id == 7916 || spell.Id == 7706 || spell.Id == 7705 || spell.Id == 7602 || spell.Id == 1561 || spell.Id == 1935 || spell.Id == 1966 || spell.Id == 1910 || spell.Id == 2561 || spell.Id == 2935 || spell.Id == 2966 || spell.Id == 2910 || spell.Id == 3935 || spell.Id == 3966 || spell.Id == 3910 || spell.Id == 4935 || spell.Id == 4966 || spell.Id == 4910 || spell.Id == 5935 || spell.Id == 5966 || spell.Id == 5910 || spell.Id == 6935 || spell.Id == 6966 || spell.Id == 6910 || spell.Id == 7935 || spell.Id == 7966 || spell.Id == 7910)
                        {
                            client.ActiveCharacter.SendInfoMsg("you can't attck your friend +)");
                            return;
                        }

                    }
                    /*if (client.ActiveCharacter.IsInGroup && targetChar.IsInGroup &&
                        targetChar.Group == client.ActiveCharacter.Group)
                    {
                        //is in one party let use skills
                        if (spell.RealId == 997 || spell.Id == 1820 || spell.Id == 2820 || spell.Id == 3820 || spell.Id == 4820 || spell.Id == 5820 || spell.Id == 6820 || spell.Id == 7820 || spell.Id == 1841 || spell.Id == 2841 || spell.Id == 3841 || spell.Id == 4841 || spell.Id == 5841 || spell.Id == 6841 || spell.Id == 7841 || spell.Id == 1844 || spell.Id == 2844 || spell.Id == 3844 || spell.Id == 4844 || spell.Id == 5844 || spell.Id == 6844 || spell.Id == 7844 || spell.Id == 1632 || spell.Id == 2632 || spell.Id == 3632 || spell.Id == 4632 || spell.Id == 5632 || spell.Id == 6632 || spell.Id == 7632 || spell.Id == 1632 || spell.Id == 1632 || spell.Id == 2632 || spell.Id == 3632 || spell.Id == 4632 || spell.Id == 5632 || spell.Id == 6632 || spell.Id == 7632 || spell.Id == 1635 || spell.Id == 2635 || spell.Id == 3635 || spell.Id == 4635 || spell.Id == 5635 || spell.Id == 6635 || spell.Id == 7635 || spell.Id == 1613 || spell.Id == 2613 || spell.Id == 3613 || spell.Id == 4613 || spell.Id == 5613 || spell.Id == 6613 || spell.Id == 7613 || spell.Id == 1633 || spell.Id == 2633 || spell.Id == 3633 || spell.Id == 4633 || spell.Id == 5633 || spell.Id == 6633 || spell.Id == 7633 || spell.Id == 1636 || spell.Id == 2636 || spell.Id == 3636 || spell.Id == 4636 || spell.Id == 5636 || spell.Id == 6636 || spell.Id == 7636 || spell.Id == 1634 || spell.Id == 2634 || spell.Id == 3634 || spell.Id == 4634 || spell.Id == 5634 || spell.Id == 6634 || spell.Id == 7634 || spell.Id == 1616 || spell.Id == 2616 || spell.Id == 3616 || spell.Id == 4616 || spell.Id == 5616 || spell.Id == 6616 || spell.Id == 7616 || spell.Id == 1637 || spell.Id == 2637 || spell.Id == 3637 || spell.Id == 4637 || spell.Id == 5637 || spell.Id == 6637 || spell.Id == 7637 || spell.Id == 1823 || spell.Id == 2823 || spell.Id == 3823 || spell.Id == 4823 || spell.Id == 5823 || spell.Id == 6823 || spell.Id == 7823 || spell.Id == 1842 || spell.Id == 2842 || spell.Id == 3842 || spell.Id == 4842 || spell.Id == 5842 || spell.Id == 6842 || spell.Id == 7842 || spell.Id == 1845 || spell.Id == 2845 || spell.Id == 3845 || spell.Id == 4845 || spell.Id == 5845 || spell.Id == 6845 || spell.Id == 7845 || spell.Id == 2044 || spell.Id == 3044 || spell.Id == 4044 || spell.Id == 5044 || spell.Id == 6044 || spell.Id == 7044 || spell.Id == 8044 || spell.Id == 2064 || spell.Id == 3064 || spell.Id == 4064 || spell.Id == 5064 || spell.Id == 6064 || spell.Id == 7064 || spell.Id == 8064 || spell.Id == 2065 || spell.Id == 3065 || spell.Id == 4065 || spell.Id == 5065 || spell.Id == 6065 || spell.Id == 7065 || spell.Id == 8065 || spell.Id == 2046 || spell.Id == 3046 || spell.Id == 4046 || spell.Id == 5046 || spell.Id == 6046 || spell.Id == 7046 || spell.Id == 8046 || spell.Id == 2066 || spell.Id == 3066 || spell.Id == 4066 || spell.Id == 5066 || spell.Id == 6066 || spell.Id == 7066 || spell.Id == 8066 || spell.Id == 2042 || spell.Id == 3042 || spell.Id == 4042 || spell.Id == 5042 || spell.Id == 6042 || spell.Id == 7042 || spell.Id == 8042 || spell.Id == 2062 || spell.Id == 3062 || spell.Id == 4062 || spell.Id == 5062 || spell.Id == 6062 || spell.Id == 7062 || spell.Id == 8062 || spell.Id == 2063 || spell.Id == 3063 || spell.Id == 4063 || spell.Id == 5063 || spell.Id == 6063 || spell.Id == 7063 || spell.Id == 8063 || spell.Id == 1704 || spell.Id == 1713 || spell.Id == 1756 || spell.Id == 1849 || spell.Id == 1078 || spell.Id == 2078 || spell.Id == 1535 || spell.Id == 1904 || spell.Id == 1073 || spell.Id == 1903 || spell.Id == 2903 || spell.Id == 3078 || spell.Id == 2904 || spell.Id == 3073 || spell.Id == 2704 || spell.Id == 2713 || spell.Id == 2756 || spell.Id == 2849 || spell.Id == 3903 || spell.Id == 4078 || spell.Id == 3904 || spell.Id == 4073 || spell.Id == 3704 || spell.Id == 3713 || spell.Id == 3756 || spell.Id == 3849 || spell.Id == 4903 || spell.Id == 5078 ||spell.RealId == 913 || spell.Id == 4904 || spell.Id == 5073 || spell.Id == 4704 || spell.Id == 4713 || spell.Id == 4756 || spell.Id == 4849 || spell.Id == 5903 || spell.Id == 6078 || spell.Id == 5904 || spell.Id == 6073 || spell.Id == 5704 || spell.Id == 5713 || spell.Id == 5756 || spell.Id == 6849 || spell.Id == 6903 || spell.Id == 7078 || spell.Id == 6904 || spell.Id == 7073 || spell.Id == 6704 || spell.Id == 6713 || spell.Id == 6756 || spell.Id == 7849 || spell.Id == 7903 || spell.Id == 8078 || spell.Id == 7904 || spell.Id == 8073 || spell.Id == 7704 || spell.Id == 7713 || spell.Id == 7756 || spell.Id == 8849 || spell.Id == 1507 || spell.Id == 1508 || spell.Id == 1641 || spell.Id == 2079 || spell.Id == 1606 || spell.Id == 1902 || spell.Id == 1501 || spell.Id == 1502 || spell.Id == 1503 || spell.Id == 1701 || spell.Id == 1702 || spell.Id == 1708 || spell.Id == 1848 || spell.Id == 1504 || spell.Id == 1505 || spell.Id == 1640 || spell.Id == 1908 || spell.Id == 1905 || spell.Id == 1912 || spell.Id == 2507 || spell.Id == 2508 || spell.Id == 2641 || spell.Id == 3079 || spell.Id == 2606 || spell.Id == 2902 || spell.Id == 2501 || spell.Id == 2502 || spell.Id == 2503 || spell.Id == 2701 || spell.Id == 2702 || spell.Id == 2708 || spell.Id == 2848 || spell.Id == 2504 || spell.Id == 2505 || spell.Id == 2640 || spell.Id == 2908 || spell.Id == 2905 || spell.Id == 2912 || spell.Id == 3507 || spell.Id == 3508 || spell.Id == 3641 || spell.Id == 4079 || spell.Id == 3606 || spell.Id == 3902 || spell.Id == 3501 || spell.Id == 3502 || spell.Id == 3503 || spell.Id == 3701 || spell.Id == 3702 || spell.Id == 3708 || spell.Id == 3848 || spell.Id == 3504 || spell.Id == 3505 || spell.Id == 3640 || spell.Id == 3908 || spell.Id == 3905 || spell.Id == 3912 || spell.Id == 4507 || spell.Id == 4508 || spell.Id == 4641 || spell.Id == 5079 || spell.Id == 4606 || spell.Id == 4902 || spell.Id == 4501 || spell.Id == 4502 || spell.Id == 4503 || spell.Id == 4701 || spell.Id == 4702 || spell.Id == 4708 || spell.Id == 4848 || spell.Id == 4504 || spell.Id == 4505 || spell.Id == 4640 || spell.Id == 4908 || spell.Id == 4905 || spell.Id == 4912 || spell.Id == 5507 || spell.Id == 5508 || spell.Id == 5641 || spell.Id == 6079 || spell.Id == 5606 || spell.Id == 5902 || spell.Id == 5501 || spell.Id == 5502 || spell.Id == 5503 || spell.Id == 5701 || spell.Id == 5702 || spell.Id == 5708 || spell.Id == 5848 || spell.Id == 5504 || spell.Id == 5505 || spell.Id == 5640 || spell.Id == 5908 || spell.Id == 5905 || spell.Id == 5912 || spell.Id == 6507 || spell.Id == 6508 || spell.Id == 6641 || spell.Id == 7079 || spell.Id == 6606 || spell.Id == 6902 || spell.Id == 6501 || spell.Id == 6502 || spell.Id == 6503 || spell.Id == 6701 || spell.Id == 6702 || spell.Id == 6708 || spell.Id == 6848 || spell.Id == 6504 || spell.Id == 6505 || spell.Id == 6640 || spell.Id == 6908 || spell.Id == 6905 || spell.Id == 6912 || spell.Id == 7507 || spell.Id == 7508 || spell.Id == 7641 || spell.Id == 8079 || spell.Id == 7606 || spell.Id == 7902 || spell.Id == 7501 || spell.Id == 7502 || spell.Id == 7503 || spell.Id == 7701 || spell.Id == 7702 || spell.Id == 7708 || spell.Id == 7848 || spell.Id == 7504 || spell.Id == 7505 || spell.Id == 7640 || spell.Id == 7908 || spell.Id == 7905 || spell.Id == 7912 || spell.Id == 1509 || spell.Id == 1514 || spell.Id == 1709 || spell.Id == 1712 || spell.Id == 1506 || spell.Id == 1511 || spell.Id == 1901 || spell.Id == 1907 || spell.Id == 1914 || spell.Id == 1935 || spell.Id == 1909 || spell.Id == 1916 || spell.Id == 1706 || spell.Id == 1705 || spell.Id == 1602 || spell.Id == 2509 || spell.Id == 2514 || spell.Id == 2709 || spell.Id == 2712 || spell.Id == 2506 || spell.Id == 2511 || spell.Id == 2901 || spell.Id == 2907 || spell.Id == 2914 || spell.Id == 2935 || spell.Id == 2909 || spell.Id == 2916 || spell.Id == 2706 || spell.Id == 2705 || spell.Id == 2602 || spell.Id == 3509 || spell.Id == 3514 || spell.Id == 3709 || spell.Id == 3712 || spell.Id == 3506 || spell.Id == 3511 || spell.Id == 3901 || spell.Id == 3907 || spell.Id == 3914 || spell.Id == 3935 || spell.Id == 3909 || spell.Id == 3916 || spell.Id == 3706 || spell.Id == 3705 || spell.Id == 3602 || spell.Id == 4509 || spell.Id == 4514 || spell.Id == 4709 || spell.Id == 4712 || spell.Id == 4506 || spell.Id == 4511 || spell.Id == 4901 || spell.Id == 4907 || spell.Id == 4914 || spell.Id == 4935 || spell.Id == 4909 || spell.Id == 4916 || spell.Id == 4706 || spell.Id == 4705 || spell.Id == 4602 || spell.Id == 5509 || spell.Id == 5514 || spell.Id == 5709 || spell.Id == 5712 || spell.Id == 5506 || spell.Id == 5511 || spell.Id == 5901 || spell.Id == 5907 || spell.Id == 5914 || spell.Id == 5935 || spell.Id == 5909 || spell.Id == 5916 || spell.Id == 5706 || spell.Id == 5705 || spell.Id == 5602 || spell.Id == 6509 || spell.Id == 6514 || spell.Id == 6709 || spell.Id == 6712 || spell.Id == 6506 || spell.Id == 6511 || spell.Id == 6901 || spell.Id == 6907 || spell.Id == 6914 || spell.Id == 6935 || spell.Id == 6909 || spell.Id == 6916 || spell.Id == 6706 || spell.Id == 6705 || spell.Id == 6602 || spell.Id == 7509 || spell.Id == 7514 || spell.Id == 7709 || spell.Id == 7712 || spell.Id == 7506 || spell.Id == 7511 || spell.Id == 7901 || spell.Id == 7907 || spell.Id == 7914 || spell.Id == 7935 || spell.Id == 7909 || spell.Id == 7916 || spell.Id == 7706 || spell.Id == 7705 || spell.Id == 7602 || spell.Id == 1561 || spell.Id == 1935 || spell.Id == 1966 || spell.Id == 1910 || spell.Id == 2561 || spell.Id == 2935 || spell.Id == 2966 || spell.Id == 2910 || spell.Id == 3935 || spell.Id == 3966 || spell.Id == 3910 || spell.Id == 4935 || spell.Id == 4966 || spell.Id == 4910 || spell.Id == 5935 || spell.Id == 5966 || spell.Id == 5910 || spell.Id == 6935 || spell.Id == 6966 || spell.Id == 6910 || spell.Id == 7935 || spell.Id == 7966 || spell.Id == 7910)
                        {
                            client.ActiveCharacter.SendInfoMsg("Can't use skills with harmfull on this target.");
                            return;
                        }
                        //if (!spell.Effects.IsAura)
                       // {
                        //    client.ActiveCharacter.SendInfoMsg("Can't use skills with harmfull on this target.");
                       //     return;
                       // }
                    }
                    else
                    {
                        client.ActiveCharacter.SendInfoMsg("Can't use skills on this target.");
                        return;
                    }*/
                }
            }
            if (spell != null)
            {
                //SendSetAtackStateGuiResponse(client.ActiveCharacter);
                SpellCast cast = client.ActiveCharacter.SpellCast;
                if (cast.DelayUntil > DateTime.Now)
                {
                    client.ActiveCharacter.SendInfoMsg("Skill delay in progress.");
                    return;
                }
                if (!client.ActiveCharacter.PlayerSpells.IsReady(spell))
                {
                    client.ActiveCharacter.SendInfoMsg("Skill is on cooldown.");
                    return;
                }
                cast.DelayUntil = DateTime.Now.AddMilliseconds(spell.CastDelay);
                var reason = cast.Start(spell, target);
                if(reason == SpellFailedReason.Ok)
                {
                    if(spell.LearnLevel<10)
                    {
                        
                    }
                     else if(spell.LearnLevel<30)
                    {
                        if(client.ActiveCharacter.GreenCharges<10)
                            client.ActiveCharacter.GreenCharges += 1;
                    }
                    else if (spell.LearnLevel < 50)
                    {
                        if (client.ActiveCharacter.BlueCharges < 10)     
                            client.ActiveCharacter.BlueCharges += 1;
                        if (client.ActiveCharacter.GreenCharges < 10)
                            client.ActiveCharacter.GreenCharges += 1;
                        
                    }
                    else
                    {
                        if (client.ActiveCharacter.RedCharges < 10)                        
                            client.ActiveCharacter.RedCharges += 1;
                        if (client.ActiveCharacter.BlueCharges < 10)
                            client.ActiveCharacter.BlueCharges += 1;
                        if (client.ActiveCharacter.GreenCharges < 10)
                            client.ActiveCharacter.GreenCharges += 1;
                        
                    }
                    SendSetSkiillPowersStatsResponse(client.ActiveCharacter, true, skillId);
                }
                else if (reason == SpellFailedReason.OutOfRange)
                {
                    Asda2MovmentHandler.MoveToSelectedTargetAndAttack(client.ActiveCharacter);
                }
            }
        }

        [PacketHandler(RealmServerOpCode.UseSoulGuardSkill)]//6158
        public static void UseSoulGuardSkillRequest(IRealmClient client, RealmPacketIn packet)
        {
            client.ActiveCharacter.IsFighting = false;
            client.ActiveCharacter.IsMoving = false;
            var skillId = packet.ReadInt16();//default : 927Len : 2
            packet.Position += 1;//nk1 default : 1Len : 1
            var x = packet.ReadInt16();//default : 100Len : 2
            var y = packet.ReadInt16();//default : 362Len : 2
            var targetType = packet.ReadByte();//default : 1Len : 1
            var targetId = packet.ReadUInt16();//default : 18Len : 4
            Spell spell = client.ActiveCharacter.Spells.GetSpellByRealId(skillId);
            if(spell==null)return;
            if(spell.SoulGuardProffLevel<1||spell.SoulGuardProffLevel>3)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Trying to use skill as SoulguardSkill.");return;
            }
            switch (spell.SoulGuardProffLevel)
            {
                case 1:
                    if (client.ActiveCharacter.GreenCharges<5)
                    {
                        client.ActiveCharacter.SendInfoMsg("Not enougt charges.");
                        SendSetSkiillPowersStatsResponse(client.ActiveCharacter, false, 0);
                        return;
                    }
                    client.ActiveCharacter.GreenCharges -= 5;
                    break;
                case 2:
                    if (client.ActiveCharacter.BlueCharges < 5)
                    {
                        client.ActiveCharacter.SendInfoMsg("Not enougt charges.");
                        SendSetSkiillPowersStatsResponse(client.ActiveCharacter, false, 0);
                        return;
                    }
                    client.ActiveCharacter.BlueCharges -= 5;
                    break;
                case 3:
                    if (client.ActiveCharacter.RedCharges < 5)
                    {
                        client.ActiveCharacter.SendInfoMsg("Not enougt charges.");
                        SendSetSkiillPowersStatsResponse(client.ActiveCharacter, false, 0);
                        return;
                    }
                    client.ActiveCharacter.RedCharges -= 5;
                    break;
            }
            ProcessUseSkill(client, targetType, skillId, targetId);
            SendSetSkiillPowersStatsResponse(client.ActiveCharacter,false, 0);
        }
            


        public static void SendSetSkiillPowersStatsResponse(Character chr,bool animate,Int16 skillId)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SetSkiillPowersStats))//6157
            {
                packet.WriteInt32(355335);//value name : unk4 default value : 355335Len : 4
                packet.WriteByte(animate?1:0);//{animate}default value : 0 Len : 1
                packet.WriteByte((byte) chr.Archetype.ClassId);//{casterClass}default value : 7 Len : 1
                packet.WriteInt16(skillId);//{skillId}default value : -1 Len : 2
                packet.WriteByte(chr.GreenCharges);//{green}default value : 1 Len : 1
                packet.WriteByte(chr.BlueCharges);//{blue}default value : 0 Len : 1
                packet.WriteByte(chr.RedCharges);//{red}default value : 0 Len : 1
                chr.Send(packet, addEnd: true);
            }
            
        }


        /// <summary>
        /// Clears a single spell's cooldown
        /// </summary>
        public static void SendClearCoolDown(Character chr, SpellId spellId)
        {
            var spell = SpellHandler.Get(spellId);
            if (spell == null)
            {
                chr.SendSystemMessage(string.Format("Can't clear cooldown for {0} cause skill not exist.", spellId));
                return;
            }
            SendClearCoolDown(chr, spell.RealId);
            
        }
        public static void SendClearCoolDown(Character chr, short realId)
        {
            if(chr == null)
                return;
            using (var packet = new RealmPacketOut(RealmServerOpCode.SkillReady))//5274
            {
                packet.WriteInt16(realId);//{skillId}default value : 586 Len : 2
                chr.Send(packet, addEnd: true);
            }

        }
        public static void SendSetSkillCooldownResponse(Character chr, Spell spell)
        {
            if(chr==null|| spell ==null)
                return;
            using (var packet = new RealmPacketOut(RealmServerOpCode.SetSkillCooldown))//5271
            {
                packet.WriteByte(1);//{status}default value : 1 Len : 1
                packet.WriteInt16(chr.SessionId);//{sessId}default value : 42 Len : 2
                packet.WriteInt16(spell.RealId);//{skillId}default value : 586 Len : 2
                packet.WriteInt16(2);//value name : unk2 default value : 2Len : 2
                chr.Send(packet, addEnd: false);
            }
        }
        static readonly byte[] unk12 = new byte[] { 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        public static void SendBuffEndedResponse(Character chr,short buffId)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.BuffEnded))//5273
            {
                packet.WriteInt16(chr.SessionId);//{sessId}default value : 45 Len : 2
                packet.WriteInt16(buffId);//{buffId}default value : 202 Len : 2
                chr.SendPacketToArea(packet);
            }
        }
        public static void SendUseSkillResultResponse(Character chr,Int16 skillId,Asda2UseSkillResult status)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.UseSkillResult))//5257
            {
                packet.WriteByte((byte)status);//{status}default value : 7 Len : 1
                packet.WriteInt16(chr.SessionId);//{casterSessId}default value : 6 Len : 2
                packet.WriteInt16(skillId);//{skillId}default value : 927 Len : 2
                packet.WriteByte(0);//value name : unk8 default value : 0Len : 1
                packet.WriteInt16(-1);//value name : unk2 default value : -1Len : 2
                chr.Send(packet,addEnd: false);
            }
        }
        public static void SendMonstrUsedSkillResponse(NPC caster, short skillId, Unit initialTarget, DamageAction[] actions)
        {
            if(caster==null)return;
            //todo mass atack from NPC
            using (var packet = new RealmPacketOut(RealmServerOpCode.MonstrUsedSkill))//8012
            {
                var targetChr = initialTarget as Character;
                packet.WriteByte(0);//value name : unk5 default value : 0Len : 1
                packet.WriteInt16(skillId);//{skillId}default value : 71 Len : 2
                packet.WriteInt16(caster.UniqIdOnMap);//{mobId}default value : 243 Len : 2
                packet.WriteByte(0);//value name : unk8 default value : 0Len : 1
                packet.WriteByte(1);//value name : unk9 default value : 1Len : 1

                packet.WriteInt16(targetChr == null ? 0 : targetChr.SessionId);//{targetSessId}default value : 23 Len : 2
                var i = 0;
                if (actions != null)
                {
                    foreach (var damageAction in actions)
                    {
                        if (i > 16 || damageAction == null)
                            break;
                        targetChr = damageAction.Victim as Character;
                        packet.WriteByte(1); //value name : unk1 default value : 1Len : 1
                        packet.WriteInt16(targetChr == null ? 0 : targetChr.SessionId);
                            //{targetSessId0}default value : 23 Len : 2
                        var dmg = damageAction.ActualDamage;
                        if (dmg < 0 || dmg > 200000000)
                            dmg = 0;
                        packet.WriteInt32(actions.Length == 0 ? 0 : dmg);
                            //{damage}default value : 4218 Len : 4
                        packet.WriteByte(actions.Length == 0 ? 3 : 1); //{effectType}default value : 1 Len : 1
                        packet.WriteSkip(unk14); //value name : unk14 default value : unk14Len : 21
                        i++;
                    }
                }
                caster.SendPacketToArea(packet,false,true);
            }
        }
        static readonly byte[] unk14 = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        public static void SendAnimateSkillStrikeResponse(Character caster, short spellRealId, DamageAction[] actions, Unit initialTarget)
        {
            SendSetAtackStateGuiResponse(caster);
            using (var packet = new RealmPacketOut(RealmServerOpCode.AnimateSkillStrike)) //5270
            {
                var targetNpc = initialTarget as NPC;
                var targetChr = initialTarget as Character;
                if(targetChr == null && targetNpc == null)
                {
                    caster.SendSystemMessage(string.Format("Wrong spell target {0}. can't animate cast. SpellId {1}",initialTarget,spellRealId));
                }
                packet.WriteInt16(caster.SessionId);//{sessId}default value : 42 Len : 2
                packet.WriteInt16(spellRealId);//{skillId}default value : 508 Len : 2
                packet.WriteInt16(6);//value name : unk6 default value : 6Len : 2
                packet.WriteByte(1);//value name : unk7 default value : 1Len : 2
                packet.WriteByte((byte) (targetNpc == null?Asda2SkillTargetType.Player:Asda2SkillTargetType.Monstr));//value name : targetType default value : 1Len : 2
                if(targetChr!=null && actions!= null)
                {
                    for (int i = 0; i < actions.Length; i++)
                    {
                        var action = actions[i];
                        if (action == null)
                            continue;
                        var status = SpellHitStatus.Ok;
                        if (action.IsCritical)
                            status = SpellHitStatus.Crit;
                        else if (action.Damage == 0)
                            status = SpellHitStatus.Miss;
                        else if (action.Blocked > 0)
                            status = SpellHitStatus.Bloced;
                        if (i < 16)
                        {
                            packet.WriteUInt16(targetChr.SessionId); //{targetId}default value : 82 Len : 2
                            packet.WriteInt32(action.ActualDamage); //{damage}default value : 571 Len : 4
                            packet.WriteInt32((byte) status); //{hitStatus}default value : 1 Len : 4
                            packet.WriteInt32(797); //value name : unk11 default value : 797Len : 4
                            packet.WriteSkip(unk12); //value name : unk12 default value : unk12Len : 15
                        }
                        action.OnFinished();
                    }
                }
                else if (actions != null)
                {
                    for (int i = 0; i < actions.Length; i++)
                    {
                        var action = actions[i];
                        if (action == null)
                            continue;
                        var status = SpellHitStatus.Ok;
                        if (action.IsCritical)
                            status = SpellHitStatus.Crit;
                        else if (action.Damage == 0)
                            status = SpellHitStatus.Miss;
                        else if (action.Blocked > 0)
                            status = SpellHitStatus.Bloced;
                        ushort targetId = 0;
                        if (initialTarget is NPC)
                        {
                            if (action.Victim == null || !(action.Victim is NPC))
                                targetId = ushort.MaxValue;
                            else
                            {
                                targetId = action.Victim.UniqIdOnMap;
                            }
                        }
                        if (i < 16)
                        {
                            packet.WriteUInt16(targetId); //{targetId}default value : 82 Len : 2
                            packet.WriteInt32(action.ActualDamage); //{damage}default value : 571 Len : 4
                            packet.WriteInt32((byte) status); //{hitStatus}default value : 1 Len : 4
                            packet.WriteInt32(797); //value name : unk11 default value : 797Len : 4
                            packet.WriteSkip(unk12); //value name : unk12 default value : unk12Len : 15
                        }
                        action.OnFinished();
                    }
                }
                else if(targetChr!=null)
                {
                    packet.WriteUInt16(targetChr.SessionId); //{targetId}default value : 82 Len : 2
                    packet.WriteInt32(0); //{damage}default value : 571 Len : 4
                    packet.WriteInt32(3); //{hitStatus}default value : 1 Len : 4
                    packet.WriteInt32(0); //value name : unk11 default value : 797Len : 4
                    packet.WriteSkip(unk12); //value name : unk12 default value : unk12Len : 15
                }
                caster.SendPacketToArea(packet, true, false);
            }
            //SendSetSkillCooldownResponse(caster, spell);
            //Asda2MovmentHandler.SendEndMoveByFastInstantRegularMoveResponse(caster);
        }

        public static void SendSetAtackStateGuiResponse(Character chr)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SetAtackStateGui))//4205
            {
                packet.WriteInt16(chr.SessionId);//{sessId}default value : 7 Len : 2
                packet.WriteInt32( chr.Account.AccountId);//{accId}default value : 340701 Len : 4
                chr.SendPacketToArea(packet,true,true);
            }
        }
        public static void SendMonstrTakesDamageSecondaryResponse(Character chr,Character targetChr,NPC targetNpc,int damage)
        {
            if(targetChr == null && targetNpc ==null)return;
            using (var packet = new RealmPacketOut(RealmServerOpCode.MonstrTakesDamageSecondary)) //4102
            {
                packet.WriteByte(targetNpc != null?0:1); //value name : unk4 default value : 0Len : 1
                packet.WriteInt16(targetNpc != null?(short)targetNpc.UniqIdOnMap:targetChr.SessionId); //{monstrId}default value : 300 Len : 2
                packet.WriteInt16(160); //{effectId}default value : 160 Len : 2
                packet.WriteInt32(damage); //{damage}default value : 73 Len : 4
                packet.WriteInt32(450); //value name : unk60 default value : 450Len : 4
                packet.WriteByte(1); //value name : unk7 default value : 1Len : 1
                packet.WriteInt16(66); //value name : unk8 default value : 66Len : 2
                packet.WriteByte(0); //value name : unk1 default value : 0Len : 1
                if(targetChr!=null)
                    targetChr.SendPacketToArea(packet, true, true);
                else
                    targetNpc.SendPacketToArea(packet, true, true);
            }
        }

        public static void SendCharacterBuffedResponse(Character target ,Aura aura)
        {
            if(aura.Spell == null)
                return;
            using (var packet = new RealmPacketOut(RealmServerOpCode.CharacterBuffed))//5272
            {
                packet.WriteInt16(target.SessionId);//{sessId}default value : 42 Len : 2
                packet.WriteInt16(aura.Spell.RealId);//{effectId}default value : 161 Len : 2
                packet.WriteInt16(0);//{iconId}default value : 161 Len : 2
                packet.WriteInt16(0);//{skillId}default value : 586 Len : 2
                packet.WriteInt16(1);//value name : unk8 default value : 1Len : 2
                packet.WriteByte(2);//value name : unk9 default value : 2Len : 1
                packet.WriteInt16((short)(aura.TimeLeft/1000));//value name : unk10 default value : 240Len : 2
                packet.WriteByte(2);//value name : unk11 default value : 2Len : 1
                packet.WriteSkip(stub14);//{stub14}default value : stub14 Len : 20
                target.SendPacketToArea(packet,true,true);
            }
        }
        static readonly byte[] stub14 = new byte[] { 0x00, 0x00, 0xC6, 0x70, 0xD3, 0x25, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00 };

        [PacketHandler(RealmServerOpCode.LearnSkill)] //5253
        public static void LearnSkillRequest(IRealmClient client, RealmPacketIn packet)
        {
            var skillId = packet.ReadInt16(); //default : 826Len : 2
            var level = packet.ReadByte(); //default : 7Len : 1
            var r = client.ActiveCharacter.PlayerSpells.TryLearnSpell(skillId, level);
            if(r!=SkillLearnStatus.Ok)
                SendSkillLearnedResponse(r,client.ActiveCharacter,0,0);

        }
        public static void SendSkillLearnedResponse(SkillLearnStatus status, Character ownerChar, uint id, int level)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SkillLearned)) //5254
            {
                packet.WriteByte((byte) status); //{status}default value : 1 Len : 1
                packet.WriteInt16(ownerChar.Spells.AvalibleSkillPoints); //value name : skillpoints default value : stab7Len : 2
                packet.WriteInt32(ownerChar.Money); //{money}default value : 33198985 Len : 4
                packet.WriteInt16(id); //{skillId}default value : 830 Len : 2
                packet.WriteByte(level); //{skillLevel}default value : 2 Len : 1
                packet.WriteSkip(stab16); //value name : stab16 default value : stab16Len : 1
                packet.WriteInt16(ownerChar.Asda2Strength); //{str}default value : 166 Len : 2
                packet.WriteInt16(ownerChar.Asda2Agility); //{dex}default value : 74 Len : 2
                packet.WriteInt16(ownerChar.Asda2Stamina); //{stamina}default value : 120 Len : 2
                packet.WriteInt16(ownerChar.Asda2Spirit); //{spirit}default value : 48 Len : 2
                packet.WriteInt16(ownerChar.Asda2Intellect); //{intelect}default value : 74 Len : 2
                packet.WriteInt16(ownerChar.Asda2Luck); //{luck}default value : 48 Len : 2
                packet.WriteInt16(0); //{bonusStr}default value : 274 Len : 2
                packet.WriteInt16(0); //{bonusDex}default value : 33 Len : 2
                packet.WriteInt16(0); //{bonusStamina}default value : 18 Len : 2
                packet.WriteInt16(0); //{bonusSpirit}default value : 7 Len : 2
                packet.WriteInt16(0); //{bonusInt}default value : 0 Len : 2
                packet.WriteInt16(0); //{bonusLuck}default value : 0 Len : 2
                packet.WriteInt16(ownerChar.Asda2Strength); //{str0}default value : 166 Len : 2
                packet.WriteInt16(ownerChar.Asda2Agility); //{dex0}default value : 74 Len : 2
                packet.WriteInt16(ownerChar.Asda2Stamina); //{stamin}default value : 120 Len : 2
                packet.WriteInt16(ownerChar.Asda2Spirit); //{spirit0}default value : 48 Len : 2
                packet.WriteInt16(ownerChar.Asda2Intellect); //{int}default value : 74 Len : 2
                packet.WriteInt16(ownerChar.Asda2Luck); //{lucj}default value : 48 Len : 2
                packet.WriteInt32(ownerChar.MaxHealth); //{maxHealth}default value : 1615 Len : 4
                packet.WriteInt16(ownerChar.MaxPower); //{maxMana}default value : 239 Len : 2
                packet.WriteInt32(ownerChar.Health); //{curHealth}default value : 1615 Len : 4
                packet.WriteInt16(ownerChar.Power); //{curMp}default value : 227 Len : 2
                packet.WriteInt16((short) ownerChar.MinDamage); //{minAtack}default value : 482 Len : 2
                packet.WriteInt16((short) ownerChar.MaxDamage); //{MaxAtack}default value : 542 Len : 2
                packet.WriteInt16(ownerChar.MinMagicDamage); //{minMatack}default value : 45 Len : 2
                packet.WriteInt16(ownerChar.MaxMagicDamage); //{maxMtack}default value : 64 Len : 2
                packet.WriteInt16((short) ownerChar.Asda2MagicDefence); //{MDef}default value : 68 Len : 2
                packet.WriteInt16((short) ownerChar.Asda2Defence); //{minDef}default value : 193 Len : 2
                packet.WriteInt16((short)ownerChar.Asda2Defence); //{maxDef}default value : 207 Len : 2
                packet.WriteFloat(ownerChar.BlockChance); //{minBlock}default value : 0 Len : 4
                packet.WriteFloat(ownerChar.BlockValue); //{maxBlock}default value : 0 Len : 4
                packet.WriteInt16(15); //value name : unk41 default value : 15Len : 2
                packet.WriteInt16(7); //value name : unk42 default value : 7Len : 2
                packet.WriteInt16(4); //value name : unk43 default value : 4Len : 2
                packet.WriteSkip(stub87); //{stub87}default value : stub87 Len : 28*/
                ownerChar.Send(packet, addEnd: false);
            }
        }

        private static readonly byte[] stab7 = new byte[] {0x05, 0x00};
        private static readonly byte[] stab16 = new byte[] {0x01};

        private static readonly byte[] stub87 = new byte[]
                                                    {
                                                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00
                                                    };
        public static void SendSkillLearnedFirstTimeResponse(IRealmClient client, short skillId,int cooldownSecs)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SkillLearnedFirstTime))//6056
            {
                packet.WriteInt16(skillId);//{skillId}default value : 981 Len : 2
                packet.WriteByte(1);//value name : unk5 default value : 1Len : 1
                packet.WriteByte(1);//value name : unk6 default value : 1Len : 1
                packet.WriteInt16(cooldownSecs);//{cooldown}default value : 10000 Len : 2
                packet.WriteSkip(stab12);//value name : stab12 default value : stab12Len : 2
                packet.WriteInt16(271);//value name : unk9 default value : 271Len : 2
                packet.WriteInt32(28);//value name : unk10 default value : 28Len : 4
                packet.WriteByte(100);//value name : unk11 default value : 100Len : 1
                packet.WriteByte(100);//value name : unk12 default value : 100Len : 1
                packet.WriteInt16(8);//value name : unk2 default value : 8Len : 2
                packet.WriteSkip(stab24);//value name : stab24 default value : stab24Len : 16
                client.Send(packet, addEnd: true);
            }
        }
        static readonly byte[] stab12 = new byte[] { 0x00, 0x00 };
        static readonly byte[] stab24 = new byte[] { 0x08, 0x00, 0xE0, 0x93, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        [PacketHandler((RealmServerOpCode)5430)] //5253
        public static void U5330(IRealmClient client, RealmPacketIn packet)
        {
        }
        [PacketHandler((RealmServerOpCode)9915)] //5253
        public static void U9915(IRealmClient client, RealmPacketIn packet)
        {
        }

        [PacketHandler((RealmServerOpCode)5056)] //5253
        public static void U5056(IRealmClient client, RealmPacketIn packet)
        {
        }
        [PacketHandler((RealmServerOpCode)5045)] //5253
        public static void U5045(IRealmClient client, RealmPacketIn packet)
        {
        }
        [PacketHandler((RealmServerOpCode)1010)] //5253
        public static void U1010(IRealmClient client, RealmPacketIn packet)
        {
        }
        [PacketHandler((RealmServerOpCode)6072)] //5253
        public static void U6072(IRealmClient client, RealmPacketIn packet)
        {
        }
        [PacketHandler((RealmServerOpCode)6084)] //5253
        public static void U6084(IRealmClient client, RealmPacketIn packet)
        {
        }
        [PacketHandler((RealmServerOpCode)6059)] //5253
        public static void U6059(IRealmClient client, RealmPacketIn packet)
        {
        }
        [PacketHandler((RealmServerOpCode)6150)] //5253
        public static void U6150(IRealmClient client, RealmPacketIn packet)
        {
        }
        [PacketHandler((RealmServerOpCode)1018)] //5253
        public static void U1018(IRealmClient client, RealmPacketIn packet)
        {
        }
        [PacketHandler((RealmServerOpCode)6749)] //5253
        public static void U6749(IRealmClient client, RealmPacketIn packet)
        {
        }
        [PacketHandler((RealmServerOpCode)6591)] //5253
        public static void U6591(IRealmClient client, RealmPacketIn packet)
        {
        }
        [PacketHandler((RealmServerOpCode)6577)] //5253
        public static void U6577(IRealmClient client, RealmPacketIn packet)
        {
        }
        [PacketHandler((RealmServerOpCode)5474)] //5253
        public static void U5474(IRealmClient client, RealmPacketIn packet)
        {
        }

    }

    internal enum Asda2MobSpellUseType
    {
        Damage =1,
        Buff =2,
        Debuff =3,
    }

    internal enum Asda2UseSkillResult
    {
        CannotApplyThisSkill =0,
        Ok=1,
        LowMp =2,
        WrongJob =3,
        WrongWeapon =4,
        CantBeUsedWhilePowerupingSkill =5,
        ItIsNotAnActiveSkill =6,

    }

    public enum SkillLearnStatus
    {
        Fail = 12,
        Ok = 1,
        BadSpellLevel = 2,
        SpellLevelIsMaximum = 3,
        JoblevelIsNotHighEnought = 4,
        NotEnoghtMoney = 5,
        NotEnoghtSpellPoints = 6,
        YouHaveSpendAllAlowedForThisJobSpellPoints = 7,
        LowLevel =8,
        BadProffession = 4,
        YourInventoryHasBenExpanded = 9,
        CCHasBeedRecharged = 10,
        CannontOpenStatusWindow =11,

    }
    public enum Asda2SkillTargetType
    {
        Player = 1,
        Monstr = 0
    }
}
