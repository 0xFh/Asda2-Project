using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.Factions;
using WCell.Constants.World;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Interaction
{
    /// <summary>
    /// <see cref="T:WCell.RealmServer.Interaction.CharacterSearch" /> derived class customized to perform the Who List search
    /// </summary>
    public sealed class WhoSearch : CharacterSearch
    {
        /// <summary>
        /// Guild name search filter. If not set this filter is ignored when performing the search.
        /// </summary>
        public string GuildName { get; set; }

        /// <summary>
        /// Zones search filter. If not set this filter is ignored when performing the search.
        /// </summary>
        public List<ZoneId> Zones { get; set; }

        /// <summary>
        /// Names search filter. If not set this filter is ignored when performing the search.
        /// </summary>
        public List<string> Names { get; set; }

        /// <summary>
        /// Faction search filter. If not set this filter is ignored when performing the search.
        /// </summary>
        public FactionGroup Faction { get; set; }

        /// <summary>
        /// Race search filter. If not set this filter is ignored when performing the search.
        /// </summary>
        public RaceMask2 RaceMask { get; set; }

        /// <summary>
        /// Class search filter. If not set this filter is ignored when performing the search.
        /// </summary>
        public ClassMask2 ClassMask { get; set; }

        public WhoSearch()
        {
            this.GuildName = string.Empty;
            this.Zones = new List<ZoneId>();
            this.Names = new List<string>();
            this.Faction = FactionGroup.Invalid;
            this.RaceMask = RaceMask2.All;
            this.ClassMask = ClassMask2.All;
        }

        /// <summary>
        /// Method used to add custom search criterias. Added Who List custom search criterias to the default ones.
        /// </summary>
        /// <param name="character">The <see cref="T:WCell.RealmServer.Entities.Character" /> to be checked against custom search criterias.</param>
        /// <returns>True if the character pass all custom search criterias. False otherwise.</returns>
        protected override bool IncludeCharacter(Character character)
        {
            return base.IncludeCharacter(character) &&
                   (this.Faction == FactionGroup.Invalid || character.Faction.Group == this.Faction) &&
                   (string.IsNullOrEmpty(this.GuildName) && this.RaceMask.HasAnyFlag(character.RaceMask2) &&
                    this.ClassMask.HasAnyFlag(character.ClassMask2)) &&
                   ((this.Zones.Count <= 0 || this.Zones.Contains(character.Zone.Id)) &&
                    (this.Names.Count <= 0 || this.Names.Contains(character.Name.ToLower())));
        }
    }
}