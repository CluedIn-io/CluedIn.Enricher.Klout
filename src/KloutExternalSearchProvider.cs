// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ClearBitExternalSearchProvider.cs" company="Clued In">
//   Copyright Clued In
// </copyright>
// <summary>
//   Defines the ClearBitExternalSearchProvider type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net;
using CluedIn.Core;
using CluedIn.Core.Data;
using CluedIn.Core.Data.Parts;
using CluedIn.Core.FileTypes;
using CluedIn.Core.Utilities;
using CluedIn.Crawling;
using CluedIn.ExternalSearch.Providers.Klout.Model;
using RestSharp;
using CluedIn.ExternalSearch.Providers.Klout.Vocabularies;

namespace CluedIn.ExternalSearch.Providers.Klout
{
    /// <summary>The clear bit external search provider.</summary>
    /// <seealso cref="CluedIn.ExternalSearch.ExternalSearchProviderBase" />
    public class KloutExternalSearchProvider : ExternalSearchProviderBase
    {
        /**********************************************************************************************************
         * CONSTRUCTORS
         **********************************************************************************************************/
        // TODO: Move Magic GUID to constants
        /// <summary>
        /// Initializes a new instance of the <see cref="KloutExternalSearchProvider" /> class.
        /// </summary>
        public KloutExternalSearchProvider()
            : base(new Guid("{3A0BF027-2FFB-45D7-A8CA-0D81514C4913}"), EntityType.Person, EntityType.Infrastructure.User, EntityType.Infrastructure.Contact)
        {
        }

        /**********************************************************************************************************
         * METHODS
         **********************************************************************************************************/

        /// <summary>Builds the queries.</summary>
        /// <param name="context">The context.</param>
        /// <param name="request">The request.</param>
        /// <returns>The search queries.</returns>
        public override IEnumerable<IExternalSearchQuery> BuildQueries(ExecutionContext context, IExternalSearchRequest request)
        {
            if (!this.Accepts(request.EntityMetaData.EntityType))
                yield break;

            var existingResults = request.GetQueryResults<KloutId>(this).ToList();

            Func<string, bool> idFilter = value => existingResults.Any(r => string.Equals(r.Data.id, value, StringComparison.InvariantCultureIgnoreCase));

            // Query Input
            var entityType = request.EntityMetaData.EntityType;
            var twitterUrl = request.QueryParameters.GetValue(CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInUser.SocialTwitter, new HashSet<string>());

            Uri twitter;

            foreach (var possibleUrl in twitterUrl)
            {
                if (Uri.IsWellFormedUriString(possibleUrl, UriKind.Absolute))
                {
                    if (Uri.TryCreate(possibleUrl, UriKind.Absolute, out twitter))
                    {
                        twitterUrl.Add(twitter.Segments.Last());
                    }
                }
            }

            if (twitterUrl != null)
            {
                var values = twitterUrl;

                foreach (var value in values.Where(v => !idFilter(v)))
                    yield return new ExternalSearchQuery(this, entityType, ExternalSearchQueryParameter.Identifier, value);
            }
        }

        /// <summary>Executes the search.</summary>
        /// <param name="context">The context.</param>
        /// <param name="query">The query.</param>
        /// <returns>The results.</returns>
        public override IEnumerable<IExternalSearchQueryResult> ExecuteSearch(ExecutionContext context, IExternalSearchQuery query)
        {
            var name = query.QueryParameters[ExternalSearchQueryParameter.Identifier].FirstOrDefault();

            if (string.IsNullOrEmpty(name))
                yield break;

            var client = new RestClient("http://api.klout.com/v2");

            var request = new RestRequest(string.Format("identity.json/twitter?screenName={0}&key=w6dgewtjtrmd4vube5a6d9ff", name), Method.GET);

            var response = client.ExecuteTaskAsync<KloutId>(request).Result;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                if (response.Data != null)
                    if (response.Data.id != null)
                    {
                        var fullKloutObject = new KloutObject();
                        {
                            var userRequest = new RestRequest(string.Format("user.json/{0}?key=w6dgewtjtrmd4vube5a6d9ff", response.Data.id), Method.GET);
                            var userResponse = client.ExecuteTaskAsync<KloutUser>(userRequest).Result;
                            fullKloutObject.User = userResponse.Data;
                        }

                        {
                            var userRequest = new RestRequest(string.Format("user.json/{0}/topics?key=w6dgewtjtrmd4vube5a6d9ff", response.Data.id), Method.GET);
                            var userResponse = client.ExecuteTaskAsync<List<KloutTopic>>(userRequest).Result;
                            fullKloutObject.Topics = userResponse.Data;
                        }

                        {
                            var userRequest = new RestRequest(string.Format("user.json/{0}/influence?key=w6dgewtjtrmd4vube5a6d9ff", response.Data.id), Method.GET);
                            var userResponse = client.ExecuteTaskAsync<KloutInfluencers>(userRequest).Result;
                            fullKloutObject.Influencers = userResponse.Data;
                        }

                        fullKloutObject.Twitter = name;

                        yield return new ExternalSearchQueryResult<KloutObject>(query, fullKloutObject);
                    }
            }
            else if (response.StatusCode == HttpStatusCode.NoContent || response.StatusCode == HttpStatusCode.NotFound)
                yield break;
            else if (response.ErrorException != null)
                throw new AggregateException(response.ErrorException.Message, response.ErrorException);
            else
                throw new ApplicationException("Could not execute external search query - StatusCode:" + response.StatusCode);
        }

        /// <summary>Builds the clues.</summary>
        /// <param name="context">The context.</param>
        /// <param name="query">The query.</param>
        /// <param name="result">The result.</param>
        /// <param name="request">The request.</param>
        /// <returns>The clues.</returns>
        public override IEnumerable<Clue> BuildClues(ExecutionContext context, IExternalSearchQuery query, IExternalSearchQueryResult result, IExternalSearchRequest request)
        {
            var resultItem = result.As<KloutObject>();

       

            var code = new EntityCode(EntityType.Person, CodeOrigin.CluedIn.CreateSpecific("klout"), resultItem.Data.User.kloutId);

            var clue = new Clue(code, context.Organization);

            clue.Data.OriginProviderDefinitionId = this.Id;

            this.PopulateMetadata(clue.Data.EntityData, resultItem);

            yield return clue;

            {
                foreach (var influencee in resultItem.Data.Influencers.myInfluencees)
                {
                    var influenceeCode = new EntityCode(EntityType.Person, CodeOrigin.CluedIn.CreateSpecific("klout"), influencee.entity.payload.kloutId);

                    var influenceeClue = new Clue(influenceeCode, context.Organization);

                    influenceeClue.Data.OriginProviderDefinitionId = this.Id;

                    this.PopulateMetadata(influenceeClue.Data.EntityData, influencee, resultItem.Data.User);

                    yield return influenceeClue;
                }

                foreach (var influencer in resultItem.Data.Influencers.myInfluencers)
                {
                    var influencerCode = new EntityCode(EntityType.Person, CodeOrigin.CluedIn.CreateSpecific("klout"), influencer.entity.payload.kloutId);

                    var influencerClue = new Clue(influencerCode, context.Organization);

                    influencerClue.Data.OriginProviderDefinitionId = this.Id;

                    this.PopulateMetadata(influencerClue.Data.EntityData, influencer, resultItem.Data.User);

                    yield return influencerClue;
                }
            }
        }

        /// <summary>Gets the primary entity metadata.</summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        /// <param name="request">The request.</param>
        /// <returns>The primary entity metadata.</returns>
        public override IEntityMetadata GetPrimaryEntityMetadata(ExecutionContext context, IExternalSearchQueryResult result, IExternalSearchRequest request)
        {
            var resultItem = result.As<KloutObject>();
            return this.CreateMetadata(resultItem);
        }

        /// <summary>Gets the preview image.</summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        /// <param name="request">The request.</param>
        /// <returns>The preview image.</returns>
        public override IPreviewImage GetPrimaryEntityPreviewImage(ExecutionContext context, IExternalSearchQueryResult result, IExternalSearchRequest request)
        {
            return null;
        }

        /// <summary>Creates the metadata.</summary>
        /// <param name="resultItem">The result item.</param>
        /// <returns>The metadata.</returns>
        private IEntityMetadata CreateMetadata(IExternalSearchQueryResult<KloutObject> resultItem)
        {
            var metadata = new EntityMetadataPart();

            this.PopulateMetadata(metadata, resultItem);

            return metadata;
        }

        /// <summary>Populates the metadata.</summary>
        /// <param name="metadata">The metadata.</param>
        /// <param name="resultItem">The result item.</param>
        private void PopulateMetadata(IEntityMetadata metadata, IExternalSearchQueryResult<KloutObject> resultItem)
        {
            var code = new EntityCode(EntityType.Person, CodeOrigin.CluedIn.CreateSpecific("klout"), resultItem.Data.User.kloutId);

            metadata.EntityType = EntityType.Person;
            if (resultItem.Data.User.nick != null) metadata.Name = resultItem.Data.User.nick;
            metadata.OriginEntityCode = code;

            metadata.Properties[KloutVocabulary.Person.Score] = resultItem.Data.User.score.score.ToString();
            //metadata.Properties[KloutVocabulary.Person.CompensationAndBenefitsRating] = resultItem.Data.compensationAndBenefitsRating;
            //metadata.Properties[KloutVocabulary.Person.CultureAndValuesRating] = resultItem.Data.cultureAndValuesRating;
            //metadata.Properties[KloutVocabulary.Person.ExactMatch] = resultItem.Data.exactMatch.ToString();

            foreach (var topic in resultItem.Data.Topics)
                metadata.Tags.Add(new Tag(topic.name));

            metadata.Codes.Add(new EntityCode(EntityType.Person, CodeOrigin.CluedIn.CreateSpecific("social.twitter"), resultItem.Data.Twitter));

            metadata.Codes.Add(code);
        }

        private void PopulateMetadata(IEntityMetadata metadata, MyInfluencee myInfluencee, KloutUser user)
        {
            var code = new EntityCode(EntityType.Person, CodeOrigin.CluedIn.CreateSpecific("klout"), myInfluencee.entity.payload.kloutId);

            metadata.EntityType = EntityType.Person;
            if (myInfluencee.entity.payload.nick != null) metadata.Name = myInfluencee.entity.payload.nick;
            metadata.OriginEntityCode = code;

            metadata.Properties[KloutVocabulary.Person.Score] = myInfluencee.entity.payload.score.score.ToString();
            //metadata.Properties[KloutVocabulary.Person.CompensationAndBenefitsRating] = resultItem.Data.compensationAndBenefitsRating;
            //metadata.Properties[KloutVocabulary.Person.CultureAndValuesRating] = resultItem.Data.cultureAndValuesRating;
            //metadata.Properties[KloutVocabulary.Person.ExactMatch] = resultItem.Data.exactMatch.ToString();

            var client = new RestClient("http://api.klout.com/v2");

            var fullKloutObject = new KloutObject();
            //{
            //    var userRequest = new RestRequest(string.Format("user.json/{0}?key=w6dgewtjtrmd4vube5a6d9ff", myInfluencee.entity.payload.kloutId), Method.GET);
            //    var userResponse = client.ExecuteTaskAsync<KloutUser>(userRequest).Result;
            //    fullKloutObject.User = userResponse.Data;
            //}

            {
                var userRequest = new RestRequest(string.Format("user.json/{0}/topics?key=w6dgewtjtrmd4vube5a6d9ff", myInfluencee.entity.payload.kloutId), Method.GET);
                var userResponse = client.ExecuteTaskAsync<List<KloutTopic>>(userRequest).Result;
                fullKloutObject.Topics = userResponse.Data;
            }

            //{
            //    var userRequest = new RestRequest(string.Format("user.json/{0}/influence?key=w6dgewtjtrmd4vube5a6d9ff", myInfluencee.entity.payload.kloutId), Method.GET);
            //    var userResponse = client.ExecuteTaskAsync<KloutInfluencers>(userRequest).Result;
            //    fullKloutObject.Influencers = userResponse.Data;
            //}

            foreach (var topic in fullKloutObject.Topics)
                metadata.Tags.Add(new Tag(topic.name));
            
            var from = new EntityReference(code);
            var to = new EntityReference(new EntityCode(EntityType.Person, CodeOrigin.CluedIn.CreateSpecific("klout"), user.kloutId));
            var edge = new EntityEdge(from, to, EntityEdgeType.SimilarTo);
            metadata.OutgoingEdges.Add(edge);

            metadata.Codes.Add(new EntityCode(EntityType.Person, CodeOrigin.CluedIn.CreateSpecific("social.twitter"), fullKloutObject.Twitter));
            
            metadata.Codes.Add(code);
        }

        private void PopulateMetadata(IEntityMetadata metadata, MyInfluencer myInfluencee, KloutUser user)
        {
            var code = new EntityCode(EntityType.Person, CodeOrigin.CluedIn.CreateSpecific("klout"), myInfluencee.entity.payload.kloutId);

            metadata.EntityType = EntityType.Person;
            if (myInfluencee.entity.payload.nick != null) metadata.Name = myInfluencee.entity.payload.nick;
            metadata.OriginEntityCode = code;

            metadata.Properties[KloutVocabulary.Person.Score] = myInfluencee.entity.payload.score.score.ToString();
            //metadata.Properties[KloutVocabulary.Person.CompensationAndBenefitsRating] = resultItem.Data.compensationAndBenefitsRating;
            //metadata.Properties[KloutVocabulary.Person.CultureAndValuesRating] = resultItem.Data.cultureAndValuesRating;

            var client = new RestClient("http://api.klout.com/v2");

            var fullKloutObject = new KloutObject();
            //{
            //    var userRequest = new RestRequest(string.Format("user.json/{0}?key=w6dgewtjtrmd4vube5a6d9ff", myInfluencee.entity.payload.kloutId), Method.GET);
            //    var userResponse = client.ExecuteTaskAsync<KloutUser>(userRequest).Result;
            //    fullKloutObject.User = userResponse.Data;
            //}

            {
                var userRequest = new RestRequest(string.Format("user.json/{0}/topics?key=w6dgewtjtrmd4vube5a6d9ff", myInfluencee.entity.payload.kloutId), Method.GET);
                var userResponse = client.ExecuteTaskAsync<List<KloutTopic>>(userRequest).Result;
                fullKloutObject.Topics = userResponse.Data;
            }

            //{
            //    var userRequest = new RestRequest(string.Format("user.json/{0}/influence?key=w6dgewtjtrmd4vube5a6d9ff", myInfluencee.entity.payload.kloutId), Method.GET);
            //    var userResponse = client.ExecuteTaskAsync<KloutInfluencers>(userRequest).Result;
            //    fullKloutObject.Influencers = userResponse.Data;
            //}

            foreach (var topic in fullKloutObject.Topics)
                metadata.Tags.Add(new Tag(topic.name));

            var from = new EntityReference(code);
            var to = new EntityReference(new EntityCode(EntityType.Person, CodeOrigin.CluedIn.CreateSpecific("klout"), user.kloutId));
            var edge = new EntityEdge(from, to, EntityEdgeType.SimilarTo);
            metadata.OutgoingEdges.Add(edge);

            metadata.Codes.Add(new EntityCode(EntityType.Person, CodeOrigin.CluedIn.CreateSpecific("social.twitter"), fullKloutObject.Twitter));

            metadata.Codes.Add(code);
        }
    }
}
