/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using za.co.grindrodbank.a3s.Exceptions;
using za.co.grindrodbank.a3s.Models;
using za.co.grindrodbank.a3s.Repositories;
using AutoMapper;
using za.co.grindrodbank.a3s.A3SApiResources;
using System.Linq;
using static za.co.grindrodbank.a3s.Models.TransientStateMachineRecord;

namespace za.co.grindrodbank.a3s.Services
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository roleRepository;
        private readonly IUserRepository userRepository;
        private readonly IFunctionRepository functionRepository;
        private readonly ISubRealmRepository subRealmRepository;
        private readonly IRoleTransientRepository roleTransientRepository;
        private readonly IRoleFunctionTransientRepository roleFunctionTransientRepository;
        private readonly IRoleRoleTransientRepository roleRoleTransientRepository;
        private readonly IMapper mapper;

        public RoleService(IRoleRepository roleRepository, IUserRepository userRepository, IFunctionRepository functionRepository, ISubRealmRepository subRealmRepository, IRoleTransientRepository roleTransientRepository, IRoleFunctionTransientRepository roleFunctionTransientRepository, IRoleRoleTransientRepository roleRoleTransientRepository, IMapper mapper)
        {
            this.roleRepository = roleRepository;
            this.userRepository = userRepository;
            this.functionRepository = functionRepository;
            this.subRealmRepository = subRealmRepository;
            this.roleTransientRepository = roleTransientRepository;
            this.roleFunctionTransientRepository = roleFunctionTransientRepository;
            this.roleRoleTransientRepository = roleRoleTransientRepository;
            this.mapper = mapper;
        }

        public async Task<RoleTransient> CreateAsync(RoleSubmit roleSubmit, Guid createdById)
        {
            // Start transactions to allow complete rollback in case of an error
            InitSharedTransaction();

            try
            {
                RoleModel existingRole = await roleRepository.GetByNameAsync(roleSubmit.Name);
                if (existingRole != null)
                    throw new ItemNotProcessableException($"Role with Name '{roleSubmit.Name}' already exist.");

                RoleTransientModel newTransientRole = await CaptureTransientRoleAsync(Guid.Empty, roleSubmit.Name, roleSubmit.Description, roleSubmit.SubRealmId, TransientAction.Create, createdById);
                // Even though we are creating/capturing the role here, it is possible that the configured approval count is 0,
                // which means that we need to check for whether the transient state is released, and process the affected role accrodingly.
                // NOTE: It is possible for an empty role (not persisted) to be returned if the role is not released in the following step.
                RoleModel role = await UpdateRoleBasedOnTransientActionIfTransientRoleStateIsReleased(newTransientRole);

                newTransientRole.LatestTransientRoleFunctions = await CaptureRoleFunctionAssignmentChanges(role, newTransientRole.RoleId, roleSubmit, createdById, roleSubmit.SubRealmId);
                newTransientRole.LatestTransientRoleChildRoles = await CaptureChildRoleAssignmentChanges(role, newTransientRole.RoleId, roleSubmit, createdById, roleSubmit.SubRealmId);

                // It is possible that the assigned functions, roles or sub-realms state has changed. Update the model, but only if it has an ID.
                if(role.Id != Guid.Empty)
                {
                    await roleRepository.UpdateAsync(role);
                }

                // All successful
                CommitTransaction();

                return mapper.Map<RoleTransient>(newTransientRole); 
            }
            catch
            {
                RollbackTransaction();
                throw;
            }
        }

        private async Task<List<RoleRoleTransientModel>> CaptureChildRoleAssignmentChanges(RoleModel role, Guid roleId, RoleSubmit roleSubmit, Guid createdBy, Guid subRealmId)
        {
            await CheckIfThereAreExistingCapturedOrApprovedTransientChildRolesForRoleAndThrowExceptionIfThereAre(roleId);

            List<RoleRoleTransientModel> latestRoleChildRoleTransients = new List<RoleRoleTransientModel>();

            await DetectAndCaptureNewChildRoleAssignments(role, roleId, roleSubmit, createdBy, subRealmId, latestRoleChildRoleTransients);
            await DetectAndCaptureChildRolesRemovedFromRole(role, roleId, roleSubmit, createdBy, subRealmId, latestRoleChildRoleTransients);

            return latestRoleChildRoleTransients;
        }

        private async Task CheckIfThereAreExistingCapturedOrApprovedTransientChildRolesForRoleAndThrowExceptionIfThereAre(Guid roleId)
        {
            List<RoleRoleTransientModel> affectedTransientChildRoles = new List<RoleRoleTransientModel>();
            var allTransientChildRoles = await roleRoleTransientRepository.GetAllTransientChildRoleRelationsForRoleAsync(roleId);

            // Extract a distinct list of child role IDs from this all the child rolee transients records.
            var distinctTransientChildRoleIds = allTransientChildRoles.Select(trf => trf.ChildRoleId).Distinct();

            foreach (var childRoleId in distinctTransientChildRoleIds)
            {
                var latestTransientChildRole = allTransientChildRoles.Where(tcr => tcr.ChildRoleId == childRoleId).LastOrDefault();

                if (latestTransientChildRole.R_State == DatabaseRecordState.Captured || latestTransientChildRole.R_State == DatabaseRecordState.Approved)
                {
                    throw new ItemNotProcessableException($"Unable to capture new state for role with ID '{roleId}' as there is a transient child role with ID '{childRoleId}' that is still in an '{latestTransientChildRole.R_State}' state.");
                }
            }
        }

        private async Task DetectAndCaptureNewChildRoleAssignments(RoleModel role, Guid roleId, RoleSubmit roleSubmit, Guid createdBy, Guid subRealmId, List<RoleRoleTransientModel> latestRoleChildRoleTransients)
        {
            var currentChildRoles = role.ChildRoles ?? new List<RoleRoleModel>();

            foreach(var childRoleId in roleSubmit.RoleIds)
            {
                var existingChildRole = currentChildRoles.Where(cr => cr.ChildRole.Id == childRoleId).FirstOrDefault();
                // If a role is found within the existing child roles, continue, as there is nothing more to do.
                if(existingChildRole != null)
                {
                    continue;
                }

                var transientChildRole = await CaptureChildRoleAssignmentChange(roleId, childRoleId, createdBy, subRealmId, TransientAction.Create);
                CheckForAndProcessReleasedChildRoleTransientRecord(role, transientChildRole);
                latestRoleChildRoleTransients.Add(transientChildRole);
            }
        }

        private async Task DetectAndCaptureChildRolesRemovedFromRole(RoleModel roleModel, Guid roleId, RoleSubmit roleSubmit, Guid capturedBy, Guid subRealmId, List<RoleRoleTransientModel> latestRoleChildRoleTransients)
        {
            var currentReleasedChildRoles = roleModel.ChildRoles ?? new List<RoleRoleModel>();
            // Extract the IDs of the currently assigned child roles, as we want to iterate through this array, as opposed to the actual
            // child role collection, as we are looking to modify the child collection.
            var currentReleasedChildRoleIds = currentReleasedChildRoles.Select(cr => cr.ChildRoleId).ToArray();

            foreach (var assignedChildRoleId in currentReleasedChildRoleIds)
            {
                var childRoleIdFromSubmitList = roleSubmit.RoleIds.Where(r => r == assignedChildRoleId).FirstOrDefault();

                if (childRoleIdFromSubmitList != Guid.Empty)
                {
                    // Continue if the currently assigned function is within the role submit function IDs.
                    continue;
                }

                // If this portion of the execution is reached, we have a child this is currently assigned to the role, but no longer
                // appears within the newly declared associated child roles list within the role submit. Capture a deletion of the currently aassigned child role.
                var removeCapturedTransientChildRole = await CaptureChildRoleAssignmentChange(roleId, assignedChildRoleId, capturedBy, subRealmId, TransientAction.Delete);
                CheckForAndProcessReleasedChildRoleTransientRecord(roleModel, removeCapturedTransientChildRole);
                latestRoleChildRoleTransients.Add(removeCapturedTransientChildRole);
            }
        }

        private void CheckForAndProcessReleasedChildRoleTransientRecord(RoleModel roleModel, RoleRoleTransientModel childRoleTransientModel)
        {
            if(childRoleTransientModel.R_State != DatabaseRecordState.Released)
            {
                return;
            }

            // It is important to ensure that the associated role actually exists by asserting there is an assigned ID.
            if (roleModel.Id == Guid.Empty)
            {
                throw new InvalidStateTransitionException($"Attempting to process a released transient child role with ID '{childRoleTransientModel.ChildRoleId}', but the parent role, with ID '{childRoleTransientModel.ParentRoleId}', does not exist or is not released yet");
            }

            // Ensure there is a role functions relation.
            roleModel.ChildRoles ??= new List<RoleRoleModel>();

            if (childRoleTransientModel.Action == TransientAction.Create)
            {
                roleModel.ChildRoles.Add(new RoleRoleModel
                {
                    ParentRoleId = childRoleTransientModel.ParentRoleId,
                    ChildRoleId = childRoleTransientModel.ChildRoleId
                });

                return;
            }

            // The only remaining action is the removal of the child role from the role.
            var childRoleToRemove = roleModel.ChildRoles.Where(cr => cr.ChildRoleId == childRoleTransientModel.ChildRoleId).FirstOrDefault();
            roleModel.ChildRoles.Remove(childRoleToRemove);
        }


        private async Task<RoleRoleTransientModel> CaptureChildRoleAssignmentChange(Guid roleId, Guid childRoleId, Guid capturedBy, Guid subRealmId, TransientAction action)
        {
            RoleModel childRole = await roleRepository.GetByIdAsync(childRoleId);

            if(childRole == null)
            {
                throw new ItemNotFoundException($"Role with ID '{childRoleId}' not found when attempting to assign it as a child role of role with ID '{roleId}'.");
            }

            ConfirmSubRealmAssociation(subRealmId, childRole);

            var childRoleTransientRecords = await roleRoleTransientRepository.GetTransientChildRoleRelationsForRoleAsync(roleId, childRoleId);
            var latestTransientChildRoleState = childRoleTransientRecords.LastOrDefault();

            var transientChildRole = new RoleRoleTransientModel
            {
                ParentRoleId = roleId,
                ChildRoleId = childRoleId,
                R_State = latestTransientChildRoleState == null ? DatabaseRecordState.Pending : latestTransientChildRoleState.R_State,
                ChangedBy = capturedBy,
                ApprovalCount = latestTransientChildRoleState == null ? 0 : latestTransientChildRoleState.ApprovalCount,
                Action = action
            };

            try // Attempt to transition the state of the transient role function, but be prepared for a possible state transition exception.
            {
                transientChildRole.Capture(capturedBy.ToString());
            }
            catch (Exception e)
            {
                throw new InvalidStateTransitionException($"Cannot capture child role assignment change for Parent Role with ID '{roleId}', ChildRole with ID '{childRoleId}'. Error: {e.Message}");
            }

            await roleRoleTransientRepository.CreateNewTransientStateForRoleChildRoleAsync(transientChildRole);

            return transientChildRole;
        }

        private async Task<List<RoleFunctionTransientModel>> CaptureRoleFunctionAssignmentChanges(RoleModel roleModel, Guid roleId, RoleSubmit roleSubmit, Guid capturedBy, Guid roleSubRealmId)
        {
            await CheckIfThereAreExistingCapturedOrApprovedTransientRoleFunctionsForRoleAndThrowExceptionIfThereAre(roleId);
            List<RoleFunctionTransientModel> affectedRoleFunctionTransientRecords = new List<RoleFunctionTransientModel>();

            await DetectAndCaptureNewRoleFunctionsAssignments(roleModel, roleId, roleSubmit, capturedBy, roleSubRealmId, affectedRoleFunctionTransientRecords);
            await DetectAndCaptureFunctionsRemovedFromRole(roleModel, roleId, roleSubmit, capturedBy, affectedRoleFunctionTransientRecords);

            return affectedRoleFunctionTransientRecords;
        }

        private async Task CheckIfThereAreExistingCapturedOrApprovedTransientRoleFunctionsForRoleAndThrowExceptionIfThereAre(Guid roleId)
        {
            var allTransientRoleFunctions = await roleFunctionTransientRepository.GetAllTransientFunctionRelationsForRoleAsync(roleId);

            // Extract a distinct list of function IDs from this all the role function transients records.
            var distinctFunctionIds = allTransientRoleFunctions.Select(trf => trf.FunctionId).Distinct();

            // Iterate through all the distinc function IDs, find the latest transient record for each function, and process accordingly.
            foreach (var functionId in distinctFunctionIds)
            {
                var latestTransientRoleFunctionRecord = allTransientRoleFunctions.Where(trf => trf.FunctionId == functionId).LastOrDefault();

                if (latestTransientRoleFunctionRecord.R_State == DatabaseRecordState.Captured || latestTransientRoleFunctionRecord.R_State == DatabaseRecordState.Approved)
                {
                    throw new ItemNotProcessableException($"Cannot capture new state for role with ID '{roleId}' as there is a transient role function for function with ID '{functionId}' in a '{latestTransientRoleFunctionRecord.R_State}' state.");
                }
            }
        }
 

        private async Task<List<RoleFunctionTransientModel>> DetectAndCaptureNewRoleFunctionsAssignments(RoleModel roleModel, Guid roleId, RoleSubmit roleSubmit, Guid capturedBy, Guid roleSubRealm, List<RoleFunctionTransientModel> affectedRoleFunctionTransientRecords)
        {
            // Recall, the role might not actually exist at this stage, so safely get access to a role function list.
            var currentReleasedRoleFunctions = roleModel.RoleFunctions ?? new List<RoleFunctionModel>();

            foreach (var functionId in roleSubmit.FunctionIds)
            {
                var existingRoleFunction = currentReleasedRoleFunctions.Where(rf => rf.FunctionId == functionId).FirstOrDefault();

                if(existingRoleFunction == null)
                {
                    var newTransientRoleFunctionRecord = await CaptureRoleFunctionAssignmentChange(roleId, functionId, capturedBy, TransientAction.Create, roleSubRealm);
                    CheckForAndProcessReleasedRoleFunctionTransientRecord(roleModel, newTransientRoleFunctionRecord);
                    affectedRoleFunctionTransientRecords.Add(newTransientRoleFunctionRecord);
                }
            }

            return affectedRoleFunctionTransientRecords;
        }

        private async Task<List<RoleFunctionTransientModel>> DetectAndCaptureFunctionsRemovedFromRole(RoleModel roleModel, Guid roleId, RoleSubmit roleSubmit, Guid capturedBy, List<RoleFunctionTransientModel> affectedRoleFunctionTransientRecords)
        {
            var currentReleasedRoleFunctions = roleModel.RoleFunctions ?? new List<RoleFunctionModel>();
            // Extract the IDs of the currently assigned functions, as we want to iterate through this array, as opposed to the actual
            // role functions collection, as we are looking to modify the role functions collection.
            var currentReleasedRoleFunctionIds = currentReleasedRoleFunctions.Select(rf => rf.FunctionId).ToArray();

            foreach(var assignedFunctionId in currentReleasedRoleFunctionIds)
            {
                var functionIdFromSubmitList = roleSubmit.FunctionIds.Where(f => f == assignedFunctionId).FirstOrDefault();

                if(functionIdFromSubmitList != Guid.Empty)
                {
                    // Continue if the currently assigned function is within the role submit function IDs.
                    continue;
                }

                // If this portion of the execution is reached, we have a function this is currently assigned to the role. but no longer
                // appears within the newly declared associated functions list within the role submit. Capture a deletion of the currently aassigned function.
                var removedTransientRoleFunctionRecord = await CaptureRoleFunctionAssignmentChange(roleId, assignedFunctionId, capturedBy, TransientAction.Delete, roleSubmit.SubRealmId);
                CheckForAndProcessReleasedRoleFunctionTransientRecord(roleModel, removedTransientRoleFunctionRecord);
                affectedRoleFunctionTransientRecords.Add(removedTransientRoleFunctionRecord);
            }

            return affectedRoleFunctionTransientRecords;
        }

        private void CheckForAndProcessReleasedRoleFunctionTransientRecord(RoleModel roleModel, RoleFunctionTransientModel roleFunctionTransientModel)
        {
            if(roleFunctionTransientModel.R_State != DatabaseRecordState.Released)
            {
                return;
            }

            // It is important to check that the associated role actually exists.
            if(roleModel.Id == Guid.Empty)
            {
                throw new InvalidStateTransitionException($"Attempting to process a released transient role function assignment update for function with ID '{roleFunctionTransientModel.FunctionId}' and role with ID '{roleFunctionTransientModel.RoleId}', but the role does not exist or is not released yet");
            }

            // Ensure there is a role functions relation.
            roleModel.RoleFunctions ??= new List<RoleFunctionModel>();

            if(roleFunctionTransientModel.Action == TransientAction.Create)
            {
                roleModel.RoleFunctions.Add(new RoleFunctionModel
                {
                    FunctionId = roleFunctionTransientModel.FunctionId,
                    RoleId = roleFunctionTransientModel.RoleId
                });

                return;
            }

            // The only remaining action is the removal of the function from the role.
            var roleFunctionToRemove = roleModel.RoleFunctions.Where(rf => rf.FunctionId == roleFunctionTransientModel.FunctionId).FirstOrDefault();
            roleModel.RoleFunctions.Remove(roleFunctionToRemove);
        }

        private async Task<RoleFunctionTransientModel> CaptureRoleFunctionAssignmentChange(Guid roleId, Guid functionId, Guid capturedBy, TransientAction action, Guid roleSubRealmId)
        {
            var functionToAdd = await functionRepository.GetByIdAsync(functionId);

            if (functionToAdd == null)
            {
                throw new ItemNotFoundException($"Function with ID '{functionId}' not found when attempting to assign it to a role.");
            }

            ConfirmSubRealmAssociation(roleSubRealmId, functionToAdd);

            var roleFunctionTransientRecords = await roleFunctionTransientRepository.GetTransientFunctionRelationsForRoleAsync(roleId, functionId);
            var latestRoleFunctionTransientState = roleFunctionTransientRecords.LastOrDefault();

            var transientRoleFunction = new RoleFunctionTransientModel
            {
                RoleId = roleId,
                FunctionId = functionId,
                R_State = latestRoleFunctionTransientState == null ? DatabaseRecordState.Pending : latestRoleFunctionTransientState.R_State,
                ChangedBy = capturedBy,
                ApprovalCount = latestRoleFunctionTransientState == null ? 0 : latestRoleFunctionTransientState.ApprovalCount,
                Action = action
            };

            try // Attempt to transition the state of the transient role function, but be prepared for a possible state transition exception.
            {
                transientRoleFunction.Capture(capturedBy.ToString());
            } catch (Exception e)
            {
                throw new InvalidStateTransitionException($"Cannot capture role function assignment change for role with ID '{roleId}', Function with ID '{functionId}'. Assignment Action: '{action}'. Error: {e.Message}");
            }

            await roleFunctionTransientRepository.CreateNewTransientStateForRoleFunctionAsync(transientRoleFunction);

            return transientRoleFunction;
        }

        private async Task<RoleModel> UpdateRoleBasedOnTransientActionIfTransientRoleStateIsReleased(RoleTransientModel roleTransientModel)
        {
            RoleModel roleToUpdate = new RoleModel();

            if (roleTransientModel.R_State != DatabaseRecordState.Released)
            {
                return roleToUpdate;
            }

            roleToUpdate = await roleRepository.GetByIdAsync(roleTransientModel.RoleId);

            if(roleToUpdate == null && roleTransientModel.Action != TransientAction.Create)
            {
                throw new ItemNotFoundException($"Role with ID '{roleTransientModel.RoleId}' not found when attempting to release role.");
            }

            if(roleTransientModel.Action == TransientAction.Modify)
            {
                await UpdateRoleWithCurrentTransientState(roleToUpdate, roleTransientModel);
                return roleToUpdate;
            }

            if(roleTransientModel.Action == TransientAction.Delete)
            {
                await roleRepository.DeleteAsync(roleToUpdate);
                return roleToUpdate;
            }

            // Only attempt to re-create the role if there is no existing role.
            if(roleToUpdate == null)
            {
                return await CreateRoleFromCurrentTransientState(roleTransientModel);
            }

            return roleToUpdate;
        }

        private async Task<RoleModel> CreateRoleFromCurrentTransientState(RoleTransientModel transientRole)
        {
            RoleModel roleToCreate = new RoleModel
            {
                Name = transientRole.Name,
                Description = transientRole.Description,
                Id = transientRole.RoleId
            };

            await AssignSubRealmToRoleFromTransientRoleIfSubRealmNotEmpty(roleToCreate, transientRole);

            return await roleRepository.CreateAsync(roleToCreate);
        }

        private async Task AssignSubRealmToRoleFromTransientRoleIfSubRealmNotEmpty(RoleModel roleModel, RoleTransientModel transientRole)
        {
            if(transientRole.SubRealmId == Guid.Empty)
            {
                return;
            }

            var subRealm = await subRealmRepository.GetByIdAsync(transientRole.SubRealmId, false);

            roleModel.SubRealm = subRealm ?? throw new ItemNotProcessableException($"Sub-Realm with ID '{transientRole.SubRealmId}' not found when attempting to assign it to a role with ID '{roleModel.Id}' from a transient state.");
        }

        private async Task UpdateRoleWithCurrentTransientState(RoleModel roleToRelease, RoleTransientModel transientRole)
        {
            roleToRelease.Name = transientRole.Name;
            roleToRelease.Description = transientRole.Description;

            await roleRepository.UpdateAsync(roleToRelease);
        }

        private async Task<RoleTransientModel> CaptureTransientRoleAsync(Guid roleId, string roleName, string roleDescription, Guid subRealmId, TransientAction action, Guid createdById)
        {
            RoleTransientModel latestTransientRole = null;

            // Recall - there might not be a Guid for the role if we are creating it.
            if(roleId != Guid.Empty)
            {
                var transientRoles = await roleTransientRepository.GetTransientsForRoleAsync(roleId);
                latestTransientRole = transientRoles.LastOrDefault();
            }

            if(subRealmId != Guid.Empty)
            {
                await CheckSubRealmIdIsValid(subRealmId);
            }

            RoleTransientModel newTransientRole = new RoleTransientModel
            {
                Action = action,
                ChangedBy = createdById,
                ApprovalCount = latestTransientRole == null ? 0 : latestTransientRole.ApprovalCount,
                // Pending is the initial state of the state machine for all transient records.
                R_State = latestTransientRole == null ? DatabaseRecordState.Pending : latestTransientRole.R_State,
                Name = roleName,
                Description = roleDescription,
                SubRealmId = subRealmId,
                RoleId = roleId == Guid.Empty ? Guid.NewGuid() : roleId
            };

            try
            {
                newTransientRole.Capture(createdById.ToString());
            }
            catch (Exception e)
            {
                throw new InvalidStateTransitionException($"Cannot capture role with ID '{roleId}'. Error: {e.Message}");
            }

            // Only persist the new captured state of the role if it actually different.
            return IsCapturedRoleDifferentFromLatestTransientRoleState(latestTransientRole, roleName, roleDescription, subRealmId, action) ? await roleTransientRepository.CreateAsync(newTransientRole) : latestTransientRole;
        }

        private bool IsCapturedRoleDifferentFromLatestTransientRoleState(RoleTransientModel latestTransientRoleState, string currentRoleName, string currentRoleDescription, Guid currentRoleSubRealmId, TransientAction action)
        {
            if(latestTransientRoleState == null)
            {
                return true;
            }

            if(latestTransientRoleState.Action != action)
            {
                return true;
            }

            return (latestTransientRoleState.Name != currentRoleName
                   || latestTransientRoleState.Description != currentRoleDescription
                   || latestTransientRoleState.SubRealmId != currentRoleSubRealmId
                   || latestTransientRoleState.R_State == DatabaseRecordState.Declined);
        }

        public async Task<RoleTransient> DeclineRole(Guid roleId, Guid approvedBy)
        {
            InitSharedTransaction();

            try
            {
                var latestTransientRole = await DeclineRoleTransientState(roleId, approvedBy);
                latestTransientRole.LatestTransientRoleFunctions = await FindTransientRoleFunctionsForRoleAndDeclineThem(roleId, approvedBy);
                latestTransientRole.LatestTransientRoleChildRoles = await FindTransientChildRolesForRoleAndDeclineThem(roleId, approvedBy);

                CommitTransaction();
                return mapper.Map<RoleTransient>(latestTransientRole);
            }
            catch (Exception e)
            {
                RollbackTransaction();
                throw e;
            }
        }

        public async Task<RoleTransient> ApproveRole(Guid roleId, Guid approvedBy)
        {
            InitSharedTransaction();

            try
            {
                var latestTransientRole = await ApproveRoleTransientState(roleId, approvedBy);
                RoleModel role = await UpdateRoleBasedOnTransientActionIfTransientRoleStateIsReleased(latestTransientRole);
                latestTransientRole.LatestTransientRoleFunctions = await FindTransientRoleFunctionsForRoleAndApproveThem(role, roleId, approvedBy);
                latestTransientRole.LatestTransientRoleChildRoles = await FindTransientChildRolesForRoleAndApproveThem(role, roleId, approvedBy);

                // It is possible that the assigned functions, roles or sub-realms state has changed. Update the model, but only if it has an ID and the role is not being deleted!
                if (role.Id != Guid.Empty && (latestTransientRole.Action != TransientAction.Delete))
                {
                    await roleRepository.UpdateAsync(role);
                }

                CommitTransaction();

                return mapper.Map<RoleTransient>(latestTransientRole);
            } catch(Exception e)
            {
                RollbackTransaction();
                throw e;
            }
        }

        private async Task<List<RoleRoleTransientModel>> FindTransientChildRolesForRoleAndApproveThem(RoleModel role, Guid roleId, Guid approvedBy)
        {
            List<RoleRoleTransientModel> affectedTransientChildRoles = new List<RoleRoleTransientModel>();
            var allTransientChildRoles = await roleRoleTransientRepository.GetAllTransientChildRoleRelationsForRoleAsync(roleId);

            // Extract a distinct list of child role IDs from this all the child rolee transients records.
            var distinctTransientChildRoleIds = allTransientChildRoles.Select(trf => trf.ChildRoleId).Distinct();

            foreach (var childRoleId in distinctTransientChildRoleIds)
            {
                var latestTransientChildRole = allTransientChildRoles.Where(tcr => tcr.ChildRoleId == childRoleId).LastOrDefault();

                if(latestTransientChildRole.R_State == DatabaseRecordState.Released)
                {
                    // This must be a an old - already released transient child role, so ignore.
                    continue;
                }

                try
                {
                    latestTransientChildRole.Approve(approvedBy.ToString());
                } catch (Exception e)
                {
                    throw new InvalidStateTransitionException($"Error approving transient child role with ID '{latestTransientChildRole.ChildRoleId}' assignment updates for role with ID '{latestTransientChildRole.ParentRoleId}' owing to invalid state transition. Error: {e.Message}");
                }

                // Reset the ID of the now approved child role transition record so we can persist a new record with it's current state.
                latestTransientChildRole.Id = Guid.Empty;
                // Null the created at field so it can be re-created by the DB.
                latestTransientChildRole.CreatedAt = new DateTime();

                await roleRoleTransientRepository.CreateNewTransientStateForRoleChildRoleAsync(latestTransientChildRole);
                CheckForAndProcessReleasedChildRoleTransientRecord(role, latestTransientChildRole);
                affectedTransientChildRoles.Add(latestTransientChildRole);
            }

            return affectedTransientChildRoles;
        }

        private async Task<List<RoleRoleTransientModel>> FindTransientChildRolesForRoleAndDeclineThem(Guid roleId, Guid approvedBy)
        {
            List<RoleRoleTransientModel> affectedTransientChildRoles = new List<RoleRoleTransientModel>();
            var allTransientChildRoles = await roleRoleTransientRepository.GetAllTransientChildRoleRelationsForRoleAsync(roleId);

            // Extract a distinct list of child role IDs from this all the child rolee transients records.
            var distinctTransientChildRoleIds = allTransientChildRoles.Select(trf => trf.ChildRoleId).Distinct();

            foreach (var childRoleId in distinctTransientChildRoleIds)
            {
                var latestTransientChildRole = allTransientChildRoles.Where(tcr => tcr.ChildRoleId == childRoleId).LastOrDefault();

                if (latestTransientChildRole.R_State == DatabaseRecordState.Released)
                {
                    // This must be a an old - already released transient child role, so ignore.
                    continue;
                }

                try
                {
                    latestTransientChildRole.Decline(approvedBy.ToString());
                }
                catch (Exception e)
                {
                    throw new InvalidStateTransitionException($"Error declining transient child role with ID '{latestTransientChildRole.ChildRoleId}' assignment updates for role with ID '{latestTransientChildRole.ParentRoleId}' owing to invalid state transition. Error: {e.Message}");
                }

                // Reset the ID of the now approved child role transition record so we can persist a new record with it's current state.
                latestTransientChildRole.Id = Guid.Empty;
                // Clear the timestamp so a new one is created.
                latestTransientChildRole.CreatedAt = new DateTime();

                await roleRoleTransientRepository.CreateNewTransientStateForRoleChildRoleAsync(latestTransientChildRole);
                affectedTransientChildRoles.Add(latestTransientChildRole);
            }

            return affectedTransientChildRoles;
        }

        private async Task<List<RoleFunctionTransientModel>> FindTransientRoleFunctionsForRoleAndApproveThem(RoleModel role, Guid roleId, Guid approvedBy)
        {
            List<RoleFunctionTransientModel> affectedRoleFunctionTransientRecords = new List<RoleFunctionTransientModel>();
            var allTransientRoleFunctions = await roleFunctionTransientRepository.GetAllTransientFunctionRelationsForRoleAsync(roleId);

            // Extract a distinct list of function IDs from this all the role function transients records.
            var distinctFunctionIds = allTransientRoleFunctions.Select(trf => trf.FunctionId).Distinct();

            // Iterate through all the distinc function IDs, find the latest transient record for each function, and process accordingly.
            foreach(var functionId in distinctFunctionIds)
            {
                var latestTransientRoleFunctionRecord = allTransientRoleFunctions.Where(trf => trf.FunctionId == functionId).LastOrDefault();

                if(latestTransientRoleFunctionRecord.R_State == DatabaseRecordState.Released)
                {
                    // This must be an old - already released transient, so ignore.
                    continue;
                }

                try
                {
                    latestTransientRoleFunctionRecord.Approve(approvedBy.ToString());
                } catch (Exception e)
                {
                    throw new InvalidStateTransitionException($"Error approving transient role function for role '{latestTransientRoleFunctionRecord.RoleId} and function '{latestTransientRoleFunctionRecord.FunctionId} owing to invalid state transition. Error: {e.Message}''");
                }
                
                // reset the ID of the transient role function so a new one can be persisted from it's current state.
                latestTransientRoleFunctionRecord.Id = Guid.Empty;
                // Null the created At date on the object so that it can be recreated.
                latestTransientRoleFunctionRecord.CreatedAt = new DateTime();

                await roleFunctionTransientRepository.CreateNewTransientStateForRoleFunctionAsync(latestTransientRoleFunctionRecord);
                CheckForAndProcessReleasedRoleFunctionTransientRecord(role, latestTransientRoleFunctionRecord);
                affectedRoleFunctionTransientRecords.Add(latestTransientRoleFunctionRecord);
            }

            return affectedRoleFunctionTransientRecords;
        }

        private async Task<List<RoleFunctionTransientModel>> FindTransientRoleFunctionsForRoleAndDeclineThem(Guid roleId, Guid approvedBy)
        {
            List<RoleFunctionTransientModel> affectedRoleFunctionTransientRecords = new List<RoleFunctionTransientModel>();
            var allTransientRoleFunctions = await roleFunctionTransientRepository.GetAllTransientFunctionRelationsForRoleAsync(roleId);

            // Extract a distinct list of function IDs from this all the role function transients records.
            var distinctFunctionIds = allTransientRoleFunctions.Select(trf => trf.FunctionId).Distinct();

            // Iterate through all the distinc function IDs, find the latest transient record for each function, and process accordingly.
            foreach (var functionId in distinctFunctionIds)
            {
                var latestTransientRoleFunctionRecord = allTransientRoleFunctions.Where(trf => trf.FunctionId == functionId).LastOrDefault();

                if (latestTransientRoleFunctionRecord.R_State == DatabaseRecordState.Released)
                {
                    // This must be an old - already released transient, so ignore.
                    continue;
                }

                try
                {
                    latestTransientRoleFunctionRecord.Decline(approvedBy.ToString());
                }
                catch (Exception e)
                {
                    throw new InvalidStateTransitionException($"Error declining transient role function for role '{latestTransientRoleFunctionRecord.RoleId} and function '{latestTransientRoleFunctionRecord.FunctionId} owing to invalid state transition. Error: {e.Message}''");
                }

                // reset the ID of the transient role function so a new one can be persisted from it's current state.
                latestTransientRoleFunctionRecord.Id = Guid.Empty;
                // reset the createAt timestamp so a new one is created.
                latestTransientRoleFunctionRecord.CreatedAt = new DateTime();

                await roleFunctionTransientRepository.CreateNewTransientStateForRoleFunctionAsync(latestTransientRoleFunctionRecord);
                affectedRoleFunctionTransientRecords.Add(latestTransientRoleFunctionRecord);
            }

            return affectedRoleFunctionTransientRecords;
        }

        private async Task <RoleTransientModel> ApproveRoleTransientState(Guid roleId, Guid approvedBy)
        {
            var transientRoles = await roleTransientRepository.GetTransientsForRoleAsync(roleId);
            var latestTransientRole = transientRoles.LastOrDefault();

            if(latestTransientRole == null)
            {
                throw new InvalidStateTransitionException($"Cannot approve role with ID '{roleId}' as it has no previous transient states.");
            }

            // Recall, that there may be no transient record changes when approving a role-function or role-child-role assignment change.
            if(latestTransientRole.R_State == DatabaseRecordState.Released)
            {
                return latestTransientRole;
            }

            EnsureRoleApproverIsDistinct(transientRoles, approvedBy);

            try
            {
                latestTransientRole.Approve(approvedBy.ToString());
            }
            catch (Exception e)
            {
                throw new InvalidStateTransitionException($"Cannot approve a transient role state for role with ID '{roleId}'. Error: {e.Message}");
            }

            // Reset the Transient Role ID to force the creation of a new transient record.
            latestTransientRole.Id = Guid.Empty;
            // Clear the createAt column so that the DB can set it.
            latestTransientRole.CreatedAt = new DateTime();

            return await roleTransientRepository.CreateAsync(latestTransientRole);
        }

        private List<RoleTransientModel> GetLatestActiveTransientRolesSincePreviousReleasedState(List<RoleTransientModel> allTransientRoles)
        {
            List<RoleTransientModel> lastestActiveTransients = new List<RoleTransientModel>();

            // Iterate backwards through the transients to get to the last 'released' or 'declined', or the the beginning of the
            // collection if the role was captured for the first time.
            for (int i = allTransientRoles.Count - 1; i >= 0; i--)
            {
                if(allTransientRoles.ElementAt(i).R_State == DatabaseRecordState.Released || allTransientRoles.ElementAt(i).R_State == DatabaseRecordState.Declined)
                {
                    return lastestActiveTransients;
                }

                lastestActiveTransients.Add(allTransientRoles.ElementAt(i));
            }

            return lastestActiveTransients;
        }

        private void EnsureRoleApproverIsDistinct(List<RoleTransientModel> transientRoles, Guid approverId)
        {
            var latestActiveTransientRoles = GetLatestActiveTransientRolesSincePreviousReleasedState(transientRoles);
            var transientRoleWithApprover = latestActiveTransientRoles.Where(rt => rt.ChangedBy == approverId && rt.R_State == DatabaseRecordState.Approved ).FirstOrDefault();

            if(transientRoleWithApprover != null)
            {
                throw new ItemNotProcessableException($"Cannot approve role as it has already been approved by this approver.");
            }
        }

        private async Task<RoleTransientModel> DeclineRoleTransientState(Guid roleId, Guid approvedBy)
        {
            var transientRoles = await roleTransientRepository.GetTransientsForRoleAsync(roleId);
            var latestTransientRole = transientRoles.LastOrDefault();

            if (latestTransientRole == null)
            {
                throw new InvalidStateTransitionException($"Cannot approve role with ID '{roleId}' as it has no previous transient states.");
            }
            // Recall, that there may be no transient record changes when approving a role-function or role-child-role assignment change.
            if (latestTransientRole.R_State == DatabaseRecordState.Released)
            {
                return latestTransientRole;
            }

            try
            {
                latestTransientRole.Decline(approvedBy.ToString());
            }
            catch (Exception e)
            {
                throw new InvalidStateTransitionException($"Cannot decline a transient role state for role with ID '{roleId}'. Error: '{e.Message}'");
            }

            // Reset the Transient Role ID to force the creation of a new transient record.
            latestTransientRole.Id = Guid.Empty;
            // Reset the created_at time so a newe timestamp is generated.
            latestTransientRole.CreatedAt = new DateTime();

            return await roleTransientRepository.CreateAsync(latestTransientRole);
        }

        private async Task CheckSubRealmIdIsValid(Guid subRealmId)
        {
            var subRealm = await subRealmRepository.GetByIdAsync(subRealmId, false);

            if(subRealm == null)
            {
                throw new ItemNotFoundException($"Sub-realm with ID '{subRealmId}' not found when attempting to assign it to a role.");
            }
        }

        public async Task<Role> GetByIdAsync(Guid roleId)
        {
            return mapper.Map<Role>(await roleRepository.GetByIdAsync(roleId));
        }

        public async Task<List<Role>> GetListAsync()
        {
            return mapper.Map<List<Role>>(await roleRepository.GetListAsync());
        }

        public async Task<RoleTransient> UpdateAsync(RoleSubmit roleSubmit, Guid roleId, Guid updatedById)
        {
            // Start transactions to allow complete rollback in case of an error
            InitSharedTransaction();

            try
            {
                RoleModel existingRole = await roleRepository.GetByIdAsync(roleId);

                if (existingRole == null)
                {
                    throw new ItemNotFoundException($"Role with ID '{roleId}' not found.");
                }

                RoleTransientModel newTransientRole = await CaptureTransientRoleAsync(roleId, roleSubmit.Name, roleSubmit.Description, roleSubmit.SubRealmId, TransientAction.Modify, updatedById);
                // Even though we are creating/capturing the role here, it is possible that the configured approval count is 0,
                // which means that we need to check for whether the transient state is released, and process the affected role accrodingly.
                // NOTE: It is possible for an empty role (not persisted) to be returned if the role is not released in the following step.
                RoleModel role = await UpdateRoleBasedOnTransientActionIfTransientRoleStateIsReleased(newTransientRole);

                if (role.Id == Guid.Empty)
                {
                    role = existingRole;
                }

                newTransientRole.LatestTransientRoleFunctions = await CaptureRoleFunctionAssignmentChanges(role, newTransientRole.RoleId, roleSubmit, updatedById, roleSubmit.SubRealmId);
                newTransientRole.LatestTransientRoleChildRoles = await CaptureChildRoleAssignmentChanges(role, newTransientRole.RoleId, roleSubmit, updatedById, roleSubmit.SubRealmId);

                // It is possible that the assigned functions, roles or sub-realms state has changed. Update the model, but only if it has an ID.
                if (role.Id != Guid.Empty)
                {
                    await roleRepository.UpdateAsync(role);
                }

                // All successful
                CommitTransaction();

                return mapper.Map<RoleTransient>(newTransientRole);
            }
            catch
            {
                RollbackTransaction();
                throw;
            }
        }

        private void ConfirmSubRealmAssociation(Guid roleSubRealmId, FunctionModel function)
        {
            // If there is a Sub-Realm associated with role, we must ensure that the function we are attempting to add to the role is associated with the same sub realm.
            if (roleSubRealmId != null && roleSubRealmId != Guid.Empty) 
            {
                if (function.SubRealm == null || function.SubRealm.Id != roleSubRealmId)
                {
                    throw new ItemNotProcessableException($"Attempting to add a function with ID '{function.Id}' to a role within the sub-realm with ID '{roleSubRealmId}', but the function does not exist within that sub-realm.");
                }
            }
            else
            {
                if (function.SubRealm != null)
                {
                    throw new ItemNotProcessableException($"Attempting to add a function with ID '{function.Id}' to a role within the sub-realm with ID '{roleSubRealmId}', but the function does not exist within that sub-realm.");
                }
            }
        }

        private void ConfirmSubRealmAssociation(Guid roleSubRealmId, RoleModel roleToAddAsChildRole)
        {
            // If there is a Sub-Realm associated with role, we must ensure that the child role we are attempting to add to the role is associated with the same sub realm.
            if (roleSubRealmId != null && roleSubRealmId != Guid.Empty)
            {
                if (roleToAddAsChildRole.SubRealm == null || roleSubRealmId != roleToAddAsChildRole.SubRealm.Id)
                {
                    throw new ItemNotProcessableException($"Attempting to add a role with ID '{roleToAddAsChildRole.Id}' as a child role but the roles are not within the same sub-realm.");
                }
            }
            else
            {
                if (roleToAddAsChildRole.SubRealm != null)
                {
                    throw new ItemNotProcessableException($"Attempting to add a role with ID '{roleToAddAsChildRole.Id}' as a child of a role but the roles are not within the same sub-realm.");
                }
            }
        }

        public void InitSharedTransaction()
        {
            userRepository.InitSharedTransaction();
            roleRepository.InitSharedTransaction();
            functionRepository.InitSharedTransaction();
            subRealmRepository.InitSharedTransaction();
            roleTransientRepository.InitSharedTransaction();
            roleFunctionTransientRepository.InitSharedTransaction();
            roleRoleTransientRepository.InitSharedTransaction();
        }

        public void CommitTransaction()
        {
            userRepository.CommitTransaction();
            roleRepository.CommitTransaction();
            functionRepository.CommitTransaction();
            subRealmRepository.CommitTransaction();
            roleTransientRepository.CommitTransaction();
            roleFunctionTransientRepository.CommitTransaction();
            roleRoleTransientRepository.CommitTransaction();
        }

        public void RollbackTransaction()
        {
            userRepository.RollbackTransaction();
            roleRepository.RollbackTransaction();
            functionRepository.RollbackTransaction();
            subRealmRepository.RollbackTransaction();
            roleTransientRepository.RollbackTransaction();
            roleFunctionTransientRepository.RollbackTransaction();
            roleRoleTransientRepository.RollbackTransaction();
        }

        public async Task<PaginatedResult<RoleModel>> GetPaginatedListAsync(int page, int pageSize, bool includeRelations, string filterName, List<KeyValuePair<string, string>> orderBy)
        {
            return await roleRepository.GetPaginatedListAsync(page, pageSize, includeRelations, filterName, orderBy);
        }

        public async Task<RoleTransient> DeleteAsync(Guid roleId, Guid deletedById)
        {
            // Start transactions to allow complete rollback in case of an error.
            InitSharedTransaction();

            try
            {
                RoleModel existingRole = await roleRepository.GetByIdAsync(roleId);

                if (existingRole == null)
                {
                    throw new ItemNotFoundException($"Role with ID '{roleId}' not found.");
                }

                RoleTransientModel newTransientRole = await CaptureTransientRoleAsync(roleId, existingRole.Name, existingRole.Description, existingRole.SubRealm == null ? Guid.Empty : existingRole.SubRealm.Id, TransientAction.Delete, deletedById);
                // Even though we are creating/capturing the role here, it is possible that the configured approval count is 0,
                // which means that we need to check for whether the transient state is released, and process the affected role accrodingly.
                // NOTE: It is possible for an empty role (not persisted) to be returned if the role is not released in the following step.
                RoleModel role = await UpdateRoleBasedOnTransientActionIfTransientRoleStateIsReleased(newTransientRole);

                // All successful
                CommitTransaction();

                return mapper.Map<RoleTransient>(newTransientRole);
            }
            catch
            {
                RollbackTransaction();
                throw;
            }
        }

        /// <summary>
        /// Gets all the transient records for a role, as well as the function and child role relations, that have been generated for the role since
        /// the last transient termination state (decline or released).
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public async Task<RoleTransients> GetLatestRoleTransientsAsync(Guid roleId)
        {
            LatestActiveTransientsForRoleModel latestActiveTransientsForRoleModel = new LatestActiveTransientsForRoleModel();

            var allTransientsForRole = await roleTransientRepository.GetTransientsForRoleAsync(roleId);
            latestActiveTransientsForRoleModel.LatestActiveRoleTransients = GetLatestActiveTransientRolesSincePreviousReleasedState(allTransientsForRole);
            latestActiveTransientsForRoleModel.LatestActiveRoleFunctionTransients = await GetLatestActiveTransientRoleFunctionsSincePreviousReleasedState(roleId);
            latestActiveTransientsForRoleModel.LatestActiveChildRoleTransients = await GetLatestActiveTransientChildRolesSincePreviousReleasedState(roleId);

            return mapper.Map<RoleTransients>(latestActiveTransientsForRoleModel);
        }

        private async Task<List<RoleFunctionTransientModel>> GetLatestActiveTransientRoleFunctionsSincePreviousReleasedState(Guid roleId)
        {    
            List<RoleFunctionTransientModel> affectedRoleFunctionTransientRecords = new List<RoleFunctionTransientModel>();
            var allTransientRoleFunctions = await roleFunctionTransientRepository.GetAllTransientFunctionRelationsForRoleAsync(roleId);

            // Extract a distinct list of function IDs from this all the role function transients records.
            var distinctFunctionIds = allTransientRoleFunctions.Select(trf => trf.FunctionId).Distinct();

            // Iterate through all the distinc function IDs, find the latest transient record for each function, and process accordingly.
            foreach (var functionId in distinctFunctionIds)
            {
                var allTransientRoleFunctionRecordsForFunction = allTransientRoleFunctions.Where(trf => trf.FunctionId == functionId).ToList();
                var latestActiveTransientRoleFunctionsForFunction = GetAllLatestActiveTransientRoleFunctionsForRoleFunctionAsync(allTransientRoleFunctionRecordsForFunction);
                latestActiveTransientRoleFunctionsForFunction.ForEach(item => affectedRoleFunctionTransientRecords.Add(item));
            }

            return affectedRoleFunctionTransientRecords;
        }

        private List<RoleFunctionTransientModel> GetAllLatestActiveTransientRoleFunctionsForRoleFunctionAsync(List<RoleFunctionTransientModel> allTransientRoleFuntionsForFunction)
        {
            List<RoleFunctionTransientModel> lastestActiveTransients = new List<RoleFunctionTransientModel>();

            // Iterate backwards through the transients to get to the last 'released' or 'declined', or the the beginning of the
            // collection if the role was captured for the first time.
            for (int i = allTransientRoleFuntionsForFunction.Count - 1; i >= 0; i--)
            {
                if (allTransientRoleFuntionsForFunction.ElementAt(i).R_State == DatabaseRecordState.Released || allTransientRoleFuntionsForFunction.ElementAt(i).R_State == DatabaseRecordState.Declined)
                {
                    return lastestActiveTransients;
                }

                lastestActiveTransients.Add(allTransientRoleFuntionsForFunction.ElementAt(i));
            }

            return lastestActiveTransients;
        }

        private async Task<List<RoleRoleTransientModel>> GetLatestActiveTransientChildRolesSincePreviousReleasedState(Guid roleId)
        {
            List<RoleRoleTransientModel> affectedChildRoleTransientRecords = new List<RoleRoleTransientModel>();
            var allTransientChildRoles = await roleRoleTransientRepository.GetAllTransientChildRoleRelationsForRoleAsync(roleId);

            // Extract a distinct list of function IDs from this all the role function transients records.
            var distinctChildRoleIds = allTransientChildRoles.Select(trf => trf.ChildRoleId).Distinct();

            // Iterate through all the distinc function IDs, find the latest transient record for each function, and process accordingly.
            foreach (var childRoleId in distinctChildRoleIds)
            {
                var allTransientChildRoleRecordsForChildRole = allTransientChildRoles.Where(trf => trf.ChildRoleId == childRoleId).ToList();
                var latestActiveTransientChildRolesForChildRole = GetAllLatestActiveTransientChildRolesForChildRoleAsync(allTransientChildRoleRecordsForChildRole);
                latestActiveTransientChildRolesForChildRole.ForEach(item => affectedChildRoleTransientRecords.Add(item));
            }

            return affectedChildRoleTransientRecords;
        }

        private List<RoleRoleTransientModel> GetAllLatestActiveTransientChildRolesForChildRoleAsync(List<RoleRoleTransientModel> allTransientChildRolesForChildRole)
        {
            List<RoleRoleTransientModel> lastestActiveTransients = new List<RoleRoleTransientModel>();

            // Iterate backwards through the transients to get to the last 'released' or 'declined', or the the beginning of the
            // collection if the role was captured for the first time.
            for (int i = allTransientChildRolesForChildRole.Count - 1; i >= 0; i--)
            {
                if (allTransientChildRolesForChildRole.ElementAt(i).R_State == DatabaseRecordState.Released || allTransientChildRolesForChildRole.ElementAt(i).R_State == DatabaseRecordState.Declined)
                {
                    return lastestActiveTransients;
                }

                lastestActiveTransients.Add(allTransientChildRolesForChildRole.ElementAt(i));
            }

            return lastestActiveTransients;
        }
    }
}
