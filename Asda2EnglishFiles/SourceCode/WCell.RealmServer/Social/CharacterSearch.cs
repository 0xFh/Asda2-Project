using System;
using System.Collections.Generic;
using WCell.Core;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;

namespace WCell.RealmServer.Interaction
{
    /// <summary>
    /// Base class used for <see cref="T:WCell.RealmServer.Entities.Character" /> searches.
    /// <remarks>
    /// This class provides common character searching criterias, if you need to include your own custom criterias
    /// just derive from this class and override the <see cref="M:WCell.RealmServer.Interaction.CharacterSearch.IncludeCharacter(WCell.RealmServer.Entities.Character)" /> method.
    /// By default this class compares the online characters matching the search criterias. If you need to conduct
    /// searches on offline or custom Characters, you can do so overriding the <see cref="M:WCell.RealmServer.Interaction.CharacterSearch.GetCharacters" /> method.
    /// </remarks>
    /// </summary>
    public class CharacterSearch
    {
        private EntityId m_id = EntityId.Zero;
        private byte m_maxLevel = byte.MaxValue;
        private string m_name;
        private byte m_minLevel;
        private uint m_maxResultCount;

        /// <summary>
        /// Character name search filter. If not set this filter is ignored when performing the search.
        /// </summary>
        public string Name
        {
            get { return this.m_name; }
            set { this.m_name = value; }
        }

        /// <summary>
        /// Character <see cref="P:WCell.RealmServer.Interaction.CharacterSearch.EntityId" /> search filter. If not set this filter is ignored when performing the search.
        /// </summary>
        public EntityId EntityId
        {
            get { return this.m_id; }
            set { this.m_id = value; }
        }

        /// <summary>
        /// Character min level search filter. If not set this filter is ignored when performing the search.
        /// </summary>
        public byte MinLevel
        {
            get { return this.m_minLevel; }
            set { this.m_minLevel = value; }
        }

        /// <summary>
        /// Character max level search filter. If not set this filter is ignored when performing the search.
        /// </summary>
        public byte MaxLevel
        {
            get { return this.m_maxLevel; }
            set { this.m_maxLevel = value; }
        }

        /// <summary>
        /// The maximum ammount of Characters matching the search criterias to return
        /// </summary>
        public uint MaxResultCount
        {
            get { return this.m_maxResultCount; }
            set { this.m_maxResultCount = value; }
        }

        /// <summary>
        /// Retrieves a list of the matched characters based on the search criterias.
        /// </summary>
        /// <returns>A list of the matched characters</returns>
        public ICollection<Character> RetrieveMatchedCharacters()
        {
            List<Character> characterList = new List<Character>();
            ICollection<Character> characters = this.GetCharacters();
            uint num = 0;
            foreach (Character character in (IEnumerable<Character>) characters)
            {
                if (this.IncludeCharacter(character))
                {
                    ++num;
                    if (num <= this.m_maxResultCount)
                        characterList.Add(character);
                    else
                        break;
                }
            }

            return (ICollection<Character>) characterList;
        }

        /// <summary>
        /// Used to retrieve the character list used in the search.
        /// By default it retrieves the online characters of the <see cref="T:WCell.RealmServer.Global.World" />.
        /// Override if you need to search offline characters or custom character lists.
        /// </summary>
        /// <returns>The character list to be searched</returns>
        protected virtual ICollection<Character> GetCharacters()
        {
            return (ICollection<Character>) World.GetAllCharacters();
        }

        /// <summary>
        /// Used by inheriters to allow custom search criterias to be performed.
        /// </summary>
        /// <param name="character">The <see cref="T:WCell.RealmServer.Entities.Character" /> to be checked against custom search criterias.</param>
        /// <returns>True if the character pass all custom search criterias. False otherwise.</returns>
        protected virtual bool IncludeCharacter(Character character)
        {
            return (!(this.m_id != EntityId.Zero) || !(character.EntityId != this.m_id)) &&
                   (this.m_name.Length <= 0 ||
                    character.Name.IndexOf(this.m_name, StringComparison.InvariantCultureIgnoreCase) >= 0) &&
                   (character.Level >= (int) this.m_minLevel && character.Level <= (int) this.m_maxLevel);
        }
    }
}