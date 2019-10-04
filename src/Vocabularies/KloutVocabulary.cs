// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ClearBitVocabulary.cs" company="Clued In">
//   Copyright Clued In
// </copyright>
// <summary>
//   Defines the ClearBitVocabulary type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CluedIn.ExternalSearch.Providers.Klout.Vocabularies
{
    /// <summary>The clear bit vocabulary.</summary>
    public static class KloutVocabulary
    {
        /// <summary>
        /// Initializes static members of the <see cref="KloutVocabulary" /> class.
        /// </summary>
        static KloutVocabulary()
        {
            Person = new KloutPersonVocabulary();            
        }

        /// <summary>Gets the organization.</summary>
        /// <value>The organization.</value>
        public static KloutPersonVocabulary Person { get; private set; }
    }
}