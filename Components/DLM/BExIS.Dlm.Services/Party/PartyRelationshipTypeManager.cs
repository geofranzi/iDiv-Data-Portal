﻿using BExIS.Dlm.Entities.Party;
using BExIS.Ext.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vaiona.Persistence.Api;

namespace BExIS.Dlm.Services.Party
{
    public class PartyRelationshipTypeManager
    {
        public IReadOnlyRepository<PartyRelationshipType> Repo { get; private set; }
        public IReadOnlyRepository<PartyTypePair> RepoPartyTypePair { get; private set; }
        public PartyRelationshipTypeManager()
        {
            IUnitOfWork uow = this.GetUnitOfWork();
            Repo = uow.GetReadOnlyRepository<PartyRelationshipType>();
            RepoPartyTypePair = uow.GetReadOnlyRepository<PartyTypePair>();
        }
        #region PartyRelationshipType
        /// <summary>
        /// Creating PartyRelationshipType
        /// because PartyRelationshipType should have PartyTypePairs,partyTypePair created in the same time of creating PartyRelationshipType
        /// </summary>
        /// <param name="title"></param>
        /// <param name="description"></param>
        /// <param name="indicatesHierarchy"></param>
        /// <param name="maxCardinality"></param>
        /// <param name="minCardinality"></param>
        /// <param name="partyTypePairAlowedSource"></param>
        /// <param name="partyTypePairAlowedTarget"></param>
        /// <param name="partyTypePairTitle"></param>
        /// <param name="partyTypePairDescription"></param>
        /// <returns></returns>
        public PartyRelationshipType Create(string title,string displayName, string description, bool indicatesHierarchy, int maxCardinality,
            int minCardinality, bool partyRelationShipTypeDefault, PartyType partyTypePairAlowedSource, PartyType partyTypePairAlowedTarget,
            string partyTypePairTitle, string partyTypePairDescription)
        {

            Contract.Requires(!string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(partyTypePairTitle));
            Contract.Requires(partyTypePairAlowedSource != null && partyTypePairAlowedSource.Id > 0);
            Contract.Requires(partyTypePairAlowedTarget != null && partyTypePairAlowedTarget.Id > 0);
            Contract.Ensures((Contract.Result<PartyRelationshipType>() != null && Contract.Result<PartyRelationshipType>().Id >= 0));
            //Contract.Ensures(Contract.Result<PartyTypePair>() != null && Contract.Result<PartyTypePair>().Id >= 0);

            PartyRelationshipType entity = new PartyRelationshipType()
            {
                Description = description,
                IndicatesHierarchy = indicatesHierarchy,
                MaxCardinality = maxCardinality,
                MinCardinality = minCardinality,
                Title = title,
                DisplayName=displayName
            };
            var partyTypeEntity = new PartyTypePair()
            {
                AllowedSource = partyTypePairAlowedSource,
                AllowedTarget = partyTypePairAlowedTarget,
                Description = partyTypePairDescription,
                PartyRelationshipType = entity,
                Title = partyTypePairTitle,
                PartyRelationShipTypeDefault= partyRelationShipTypeDefault
            };

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyRelationshipType> repo = uow.GetRepository<PartyRelationshipType>();
                IRepository<PartyTypePair> repoPTP = uow.GetRepository<PartyTypePair>();
                repo.Put(entity);
                repoPTP.Put(partyTypeEntity);
                uow.Commit();
            }
            return (entity);
        }
        public PartyRelationshipType Update(long id, string title, string description, bool indicatesHierarchy, int maxCardinality,
           int minCardinality)
        {
            Contract.Requires(id > 0);
            Contract.Requires(!string.IsNullOrWhiteSpace(title));
            Contract.Ensures((Contract.Result<PartyRelationshipType>() != null && Contract.Result<PartyRelationshipType>().Id >= 0));
            var entity = new PartyRelationshipType();
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyRelationshipType> repo = uow.GetRepository<PartyRelationshipType>();
                entity = repo.Get(id);
                if (entity == null)
                    BexisException.Throw(null, "PartyRelationshipType not found", BexisException.ExceptionType.Edit);
                entity.Title = title;
                entity.Description = description;
                entity.IndicatesHierarchy = indicatesHierarchy;
                entity.MaxCardinality = maxCardinality;
                entity.MinCardinality = minCardinality;
                repo.Put(entity);
                uow.Commit();
            }
            return entity;
        }

        public bool Delete(PartyRelationshipType partyRelationType)
        {
            Contract.Requires(partyRelationType != null);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyRelationshipType> repoPR = uow.GetRepository<PartyRelationshipType>();
                IRepository<PartyType> repoType = uow.GetRepository<PartyType>();

                var entity = repoPR.Reload(partyRelationType);
                //If there is a relation between entity and a party we couldn't delete it
                if (entity.PartyRelationships.Count() > 0)
                    BexisException.Throw(entity, "There are some relations between this 'PartyRelationshipType' and 'Party'", BexisException.ExceptionType.Delete);
                // remove all associations between the entity and AssociatedPairs
                entity.AssociatedPairs.ToList().ForEach(item => item.PartyRelationshipType = null);
                entity.AssociatedPairs.Clear();

                repoPR.Delete(entity);

                uow.Commit();
            }
            return (true);
        }
        public bool Delete(IEnumerable<PartyRelationshipType> entities)
        {
            Contract.Requires(entities != null);
            Contract.Requires(Contract.ForAll(entities, (PartyRelationshipType e) => e != null));
            Contract.Requires(Contract.ForAll(entities, (PartyRelationshipType e) => e.Id >= 0));
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyRelationshipType> repoPR = uow.GetRepository<PartyRelationshipType>();
                IRepository<PartyType> repoType = uow.GetRepository<PartyType>();
                foreach (var entity in entities)
                {
                    var latest = repoPR.Reload(entity);
                    //If there is a relation between entity and a party we couldn't delete it
                    if (entity.PartyRelationships.Count() > 0)
                        BexisException.Throw(entity, "There are some relations between this 'PartyRelationshipType' and 'Party'", BexisException.ExceptionType.Delete, true);
                    // remove all associations between the entity and AssociatedPairs
                    entity.AssociatedPairs.ToList().ForEach(item => item.PartyRelationshipType = null);
                    entity.AssociatedPairs.Clear();
                    repoPR.Delete(entity);
                }
                uow.Commit();
            }
            return (true);
        }
        public IEnumerable<PartyRelationshipType> GetPartyRelationshipTypeWithAllowedAssociated(int sourcePartyTypeId)
        {
            Contract.Requires(sourcePartyTypeId > 0);
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyType> repoPT = uow.GetRepository<PartyType>();
                IRepository<PartyRelationshipType> repoPRT = uow.GetRepository<PartyRelationshipType>();
                IRepository<PartyTypePair> repoPTP = uow.GetRepository<PartyTypePair>();
                PartyType sourcePartyType = repoPT.Get(sourcePartyTypeId);
                var partyRelationshipTypes = repoPRT.Get().OrderBy(item => item.Title);
                foreach (var partyRelationshipType in partyRelationshipTypes)
                    partyRelationshipType.AssociatedPairs = partyRelationshipType.AssociatedPairs.Where(item => item.AllowedSource.Id == sourcePartyType.Id).ToList();
                return partyRelationshipTypes.ToList();
            }
        }


        #endregion
        #region PartyTypePair
        public PartyTypePair AddPartyTypePair(string title, PartyType allowedSource, PartyType allowedTarget, string description,bool partyRelationShipTypeDefault,
            PartyRelationshipType partyRelationshipType)
        {
            Contract.Requires(!string.IsNullOrEmpty(title));
            Contract.Requires(allowedSource != null && allowedSource.Id > 0);
            Contract.Requires(allowedTarget != null && allowedTarget.Id > 0);
            Contract.Ensures(Contract.Result<PartyTypePair>() != null && Contract.Result<PartyTypePair>().Id >= 0);

            var entity = new PartyTypePair()
            {
                AllowedSource = allowedSource,
                AllowedTarget = allowedTarget,
                Description = description,
                PartyRelationshipType = partyRelationshipType,
                Title = title,
                PartyRelationShipTypeDefault= partyRelationShipTypeDefault
            };
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyTypePair> repo = uow.GetRepository<PartyTypePair>();
                repo.Put(entity);
                uow.Commit();
            }
            return (entity);
        }
        public PartyTypePair UpdatePartyTypePair(long id, string title, PartyType allowedSource, PartyType alowedTarget, string description,bool partyRelationShipTypeDefault,
            PartyRelationshipType partyRelationshipType)
        {
            Contract.Requires(id > 0);
            Contract.Requires(!string.IsNullOrEmpty(title));
            Contract.Requires(allowedSource != null && allowedSource.Id > 0);
            Contract.Requires(alowedTarget != null && alowedTarget.Id > 0);
            Contract.Ensures(Contract.Result<PartyTypePair>() != null && Contract.Result<PartyTypePair>().Id >= 0);
            var entity = new PartyTypePair();
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyTypePair> repo = uow.GetRepository<PartyTypePair>();
                entity = repo.Get(id);
                if (entity == null)
                    BexisException.Throw(null, "PartyTypePair not found", BexisException.ExceptionType.Edit);
                entity.AllowedSource = allowedSource;
                entity.AllowedTarget = alowedTarget;
                entity.Description = description;
                entity.PartyRelationshipType = partyRelationshipType;
                entity.Title = title;
                entity.PartyRelationShipTypeDefault = partyRelationShipTypeDefault;
                repo.Put(entity);
                uow.Commit();
            }
            return (entity);
        }
        public bool RemovePartyTypePair(PartyTypePair partyTypePair)
        {
            Contract.Requires(partyTypePair != null && partyTypePair.Id > 0);
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyTypePair> repoPR = uow.GetRepository<PartyTypePair>();
                IRepository<PartyRelationshipType> repoRel = uow.GetRepository<PartyRelationshipType>();
                var entity = repoPR.Reload(partyTypePair);
                if (repoRel.Get(item => item.AssociatedPairs.Contains(partyTypePair)).Count() > 0)
                    BexisException.Throw(entity, "There are some relations between this entity and 'PartyRelationshipType'.", BexisException.ExceptionType.Delete);

                repoPR.Delete(entity);
                uow.Commit();
            }
            return (true);
        }
        public bool RemovePartyTypePair(IEnumerable<PartyTypePair> entities)
        {
            Contract.Requires(entities != null);
            Contract.Requires(Contract.ForAll(entities, (PartyTypePair e) => e != null));
            Contract.Requires(Contract.ForAll(entities, (PartyTypePair e) => e.Id >= 0));
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyTypePair> repoPR = uow.GetRepository<PartyTypePair>();
                IRepository<PartyRelationshipType> repoRel = uow.GetRepository<PartyRelationshipType>();
                foreach (var entity in entities)
                {
                    if (repoRel.Get(item => item.AssociatedPairs.Contains(entity)).Count() > 0)
                        BexisException.Throw(entity, "There are some relations between this entity and 'PartyRelationshipType'.", BexisException.ExceptionType.Delete, true);

                    var latest = repoPR.Reload(entity);
                    repoPR.Delete(latest);
                }
                uow.Commit();
            }
            return (true);
        }
        public IEnumerable<PartyTypePair> GetPartyTypePairs(int sourcePartyTypeId)
        {
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyTypePair> repoPTP = uow.GetRepository<PartyTypePair>();
                return repoPTP.Get(item => item.AllowedSource.Id == sourcePartyTypeId);
            }
        }
        #endregion
        #region additional_methods
        public IEnumerable<PartyType> GetRootPartyTypes()
        {
            var partyTypes = new List<PartyType>();
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyTypePair> repoPartyTypePair = uow.GetRepository<PartyTypePair>();
                partyTypes.AddRange(repoPartyTypePair.Get(item => item.PartyRelationshipType.IndicatesHierarchy).Select(item => item.AllowedTarget).Distinct());
            }
            return partyTypes;
        }

        /// <summary>
        /// there is an inheritance relationship between the party types which comes from the party relationship type 
        /// if the indicate inheritance is true it means it is a root party types and all the source parties 
        /// </summary>
        /// <param name="targetPartyTypeId"></param>
        /// <returns></returns>
        public IEnumerable<PartyType> GetChildPartyTypes(int targetPartyTypeId)
        {
            var partyTypes = new List<PartyType>();
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PartyTypePair> repoPartyTypePair = uow.GetRepository<PartyTypePair>();
                //Find all the typePair which are IndicatesHierarchy and their target is equal to the input
                var partyTypePairs = RepoPartyTypePair.Get(item => item.AllowedTarget.Id == targetPartyTypeId && item.PartyRelationshipType.IndicatesHierarchy);
                //Add all of their parties
                partyTypes.AddRange(partyTypePairs.Select(item => item.AllowedSource).Distinct());

            }
            return partyTypes;
        }
        #endregion
    }
}
