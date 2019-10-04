// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ClearBitPersonVocabulary.cs" company="Clued In">
//   Copyright Clued In
// </copyright>
// <summary>
//   Defines the ClearBitPersonVocabulary type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using CluedIn.Core.Data;
using CluedIn.Core.Data.Vocabularies;

namespace CluedIn.ExternalSearch.Providers.Klout.Vocabularies
{
    /// <summary>The clear bit Person vocabulary.</summary>
    /// <seealso cref="CluedIn.Core.Data.Vocabularies.SimpleVocabulary" />
    public class KloutPersonVocabulary : SimpleVocabulary
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GlassDoorPersonVocabulary"/> class.
        /// </summary>
        public KloutPersonVocabulary()
        {
            this.VocabularyName = "Klout Person";
            this.KeyPrefix = "klout.Person";
            this.KeySeparator   = ".";
            this.Grouping       = EntityType.Person;

            this.Score = this.Add(new VocabularyKey("Score"));
        }

        public VocabularyKey Score { get; set; }
    }
}
