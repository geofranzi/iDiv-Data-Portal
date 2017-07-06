﻿using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Subjects;
using System;
using System.Collections.Generic;
using System.Linq;
using Vaiona.Persistence.Api;

namespace BExIS.Security.Services.Authorization
{
    public class EntityPermissionManager
    {
        public EntityPermissionManager()
        {
            var uow = this.GetUnitOfWork();

            EntityPermissionRepository = uow.GetReadOnlyRepository<EntityPermission>();
            EntityRepository = uow.GetReadOnlyRepository<Entity>();
            SubjectRepository = uow.GetReadOnlyRepository<Subject>();
        }

        public IReadOnlyRepository<EntityPermission> EntityPermissionRepository { get; private set; }
        public IQueryable<EntityPermission> EntityPermissions => EntityPermissionRepository.Query();
        public IReadOnlyRepository<Entity> EntityRepository { get; private set; }
        public IReadOnlyRepository<Subject> SubjectRepository { get; private set; }

        public void Create(EntityPermission entityPermission)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityPermissionRepository = uow.GetRepository<EntityPermission>();
                entityPermissionRepository.Put(entityPermission);
                uow.Commit();
            }
        }

        public void Create(Subject subject, Entity entity, long key, short rights)
        {
            var entityPermission = new EntityPermission()
            {
                Subject = subject,
                Entity = entity,
                Key = key,
                Rights = rights
            };

            using (var uow = this.GetUnitOfWork())
            {
                var entityPermissionRepository = uow.GetRepository<EntityPermission>();
                entityPermissionRepository.Put(entityPermission);
                uow.Commit();
            }
        }

        public EntityPermission Create(string subjectName, Type subjectType, string entityName, Type entityType, long key, List<RightType> rights)
        {
            var entityPermission = new EntityPermission()
            {
                Subject = SubjectRepository.Query(s => s.Name.ToUpperInvariant() == subjectName.ToUpperInvariant() && s.GetType() == subjectType).FirstOrDefault(),
                Entity = EntityRepository.Query(e => e.Name.ToUpperInvariant() == entityName.ToUpperInvariant() && e.Name.GetType() == entityType).FirstOrDefault(),
                Key = key,
                // ToDo
                Rights = 0
            };

            using (var uow = this.GetUnitOfWork())
            {
                var entityPermissionRepository = uow.GetRepository<EntityPermission>();
                entityPermissionRepository.Put(entityPermission);
                uow.Commit();
            }

            return entityPermission;
        }

        public void Delete(EntityPermission entityPermission)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityPermissionRepository = uow.GetRepository<EntityPermission>();
                entityPermissionRepository.Delete(entityPermission);
                uow.Commit();
            }
        }

        public void Delete(long entityPermissionId)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityPermissionRepository = uow.GetRepository<EntityPermission>();
                entityPermissionRepository.Delete(EntityPermissionRepository.Get(entityPermissionId));
                uow.Commit();
            }
        }

        public EntityPermission FindById(long entityPermissionId)
        {
            return EntityPermissionRepository.Get(entityPermissionId);
        }

        public short GetRights(Subject subject, Entity entity, long key)
        {
            var entityPermission = EntityPermissionRepository.Get(m => m.Subject.Id == subject.Id && m.Entity.Id == entity.Id).FirstOrDefault();
            return entityPermission?.Rights ?? 0;
        }

        public List<RightType> GetRights(string subjectName, Type subjectType, string entityName, Type entityType, long key)
        {
            var subject = SubjectRepository.Query(s => s.Name.ToUpperInvariant() == subjectName.ToUpperInvariant() && s.GetType() == subjectType).FirstOrDefault();
            var entity = EntityRepository.Query(e => e.Name.ToUpperInvariant() == entityName.ToUpperInvariant() && e.Name.GetType() == entityType).FirstOrDefault();
            var binary = Convert.ToString(GetRights(subject, entity, key), 2);
            return Enum.GetValues(typeof(RightType)).Cast<int>().Where(right => binary.ElementAt((binary.Length - 1) - right) == '1').Cast<RightType>().ToList();
        }

        public List<long> GetKeys(string subjectName, Type subjectType, string entityName, Type entityType, RightType rightType)
        {
            var subject = SubjectRepository.Query(s => s.Name.ToUpperInvariant() == subjectName.ToUpperInvariant() && s.GetType() == subjectType).FirstOrDefault();
            var entity = EntityRepository.Query(e => e.Name.ToUpperInvariant() == entityName.ToUpperInvariant() && e.Name.GetType() == entityType).FirstOrDefault();

            var keys = EntityPermissionRepository.Query(e => e.Subject.Id == subject.Id && e.Entity.Id == entity.Id && e.Rights >=)

            return new List<long>();
        }

        public bool HasRight(string subjectName, Type subjectType, string entityName, Type entityType, long key, RightType rightType)
        {
            var subject = SubjectRepository.Query(s => s.Name.ToUpperInvariant() == subjectName.ToUpperInvariant() && s.GetType() == subjectType).FirstOrDefault();
            var entity = EntityRepository.Query(e => e.Name.ToUpperInvariant() == entityName.ToUpperInvariant() && e.Name.GetType() == entityType).FirstOrDefault();

            var binary = Convert.ToString(GetRights(subject, entity, key), 2);
            return binary.ElementAt((binary.Length - 1) - (int)rightType) == '1';
        }

        public void Update(EntityPermission entityPermission)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityPermissionRepository = uow.GetRepository<EntityPermission>();
                entityPermissionRepository.Put(entityPermission);
                uow.Commit();
            }
        }
    }
}