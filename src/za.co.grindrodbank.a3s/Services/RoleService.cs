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

                RoleTransientModel newTransientRole = await CaptureTransientRoleAsync(Guid.Empty, roleSubmit.Name, roleSubmit.Description, roleSubmit.SubRealmId, "create", createdById);
                // Even though we are creating/capturing the role here, it is possible that the configured approval count is 0,
                // which means that we need to check for whether the transient state is released, and process the affected role accrodingly.
                // NOTE: It is possible for an empty role (not persisted) to be returned if the role is not released in the following step.
                RoleModel role = await UpdateRoleBasedOnTransientActionIfTransientRoleStateIsReleased(newTransientRole);

                await CaptureRoleFunctionAssignmentChanges(role, newTransientRole.RoleId, roleSubmit, createdById, roleSubmit.SubRealmId);
                await CaptureChildRoleAssignmentChanges(role, newTransientRole.RoleId, roleSubmit, createdById, roleSubmit.SubRealmId);

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

        private async Task CaptureChildRoleAssignmentChanges(RoleModel role, Guid roleId, RoleSubmit roleSubmit, Guid createdBy, Guid subRealmId)
        {
            await DetectAndCaptureNewChildRoleAssignments(role, roleId, roleSubmit, createdBy, subRealmId);
            await DetectAndCaptureChildRolesRemovedFromRole(role, roleId, roleSubmit, createdBy, subRealmId);
        }

        private async Task DetectAndCaptureNewChildRoleAssignments(RoleModel role, Guid roleId, RoleSubmit roleSubmit, Guid createdBy, Guid subRealmId)
        {
            var currentChildRoles = role.ChildRoles ?? new List<RoleRoleModel>();

            foreach(var childRoleId in roleSubmit.RoleIds)
            {
                var existingChildRole = currentChildRoles.Where(cr => cr.ChildRole.Id == childRoleId).FirstOrDefault();
                // If a role is found withing the existing child roles, return, as there is nothing more to do.
                if(existingChildRole != null)
                {
                    return;
                }

                var transientChildRole = await CaptureChildRoleAssignmentChange(roleId, childRoleId, createdBy, subRealmId, "create");
                CheckForAndProcessReleasedChildRoleTransientRecord(role, transientChildRole);
            }
        }

        private async Task DetectAndCaptureChildRolesRemovedFromRole(RoleModel roleModel, Guid roleId, RoleSubmit roleSubmit, Guid capturedBy, Guid subRealmId)
        {
            var currentReleasedChildRoles = roleModel.ChildRoles ?? new List<RoleRoleModel>();
            // Extract the IDs of the currently assigned child roles, as we want to iterate through this array, as opposed to the actual
            // child role collection, as we are looking to modify the child collection.
            var currentReleasedChildRoleIds = currentReleasedChildRoles.Select(cr => cr.ParentRoleId).ToArray();

            foreach (var assignedChildRoleId in currentReleasedChildRoleIds)
            {
                var childRoleIdFromSubmitList = roleSubmit.RoleIds.Where(r => r == assignedChildRoleId).FirstOrDefault();

                if (childRoleIdFromSubmitList != null)
                {
                    // Continue if the currently assigned function is within the role submit function IDs.
                    continue;
                }

                // If this portion of the execution is reached, we have a child this is currently assigned to the role, but no longer
                // appears within the newly declared associated child roles list within the role submit. Capture a deletion of the currently aassigned child role.
                var removeCapturedTransientChildRole = await CaptureChildRoleAssignmentChange(roleId, assignedChildRoleId, capturedBy, subRealmId, "delete");
                CheckForAndProcessReleasedChildRoleTransientRecord(roleModel, removeCapturedTransientChildRole);
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

            if (childRoleTransientModel.Action == "create")
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


        private async Task<RoleRoleTransientModel> CaptureChildRoleAssignmentChange(Guid roleId, Guid childRoleId, Guid capturedBy, Guid subRealmId, string action)
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
                throw new InvalidStateTransitionException(e.Message);
            }

            await roleRoleTransientRepository.CreateNewTransientStateForRoleChildRoleAsync(transientChildRole);

            return transientChildRole;
        }

        private async Task CaptureRoleFunctionAssignmentChanges(RoleModel roleModel, Guid roleId, RoleSubmit roleSubmit, Guid capturedBy, Guid roleSubRealmId)
        {
            await DetectAndCaptureNewRoleFunctionsAssignments(roleModel, roleId, roleSubmit, capturedBy, roleSubRealmId);
            await DetectAndCaptureFunctionsRemovedFromRole(roleModel, roleId, roleSubmit, capturedBy);
        }
 

        private async Task DetectAndCaptureNewRoleFunctionsAssignments(RoleModel roleModel, Guid roleId, RoleSubmit roleSubmit, Guid capturedBy, Guid roleSubRealm)
        {
            // Recall, the role might not actually exist at this stage, so safely get access to a role function list.
            var currentReleasedRoleFunctions = roleModel.RoleFunctions ?? new List<RoleFunctionModel>();

            foreach (var functionId in roleSubmit.FunctionIds)
            {
                var existingRoleFunction = currentReleasedRoleFunctions.Where(rf => rf.FunctionId == functionId).FirstOrDefault();

                if(existingRoleFunction == null)
                {
                    var newTransientRoleFunctionRecord = await CaptureRoleFunctionAssignmentChange(roleId, functionId, capturedBy, "create", roleSubmit.SubRealmId);
                    CheckForAndProcessReleasedRoleFunctionTransientRecord(roleModel, newTransientRoleFunctionRecord);
                }
            }
        }

        private async Task DetectAndCaptureFunctionsRemovedFromRole(RoleModel roleModel, Guid roleId, RoleSubmit roleSubmit, Guid capturedBy)
        {
            var currentReleasedRoleFunctions = roleModel.RoleFunctions ?? new List<RoleFunctionModel>();
            // Extract the IDs of the currently assigned functions, as we want to iterate through this array, as opposed to the actual
            // role functions collection, as we are looking to modify the role functions collection.
            var currentReleasedRoleFunctionIds = currentReleasedRoleFunctions.Select(rf => rf.FunctionId).ToArray();

            foreach(var assignedFunctionId in currentReleasedRoleFunctionIds)
            {
                var functionIdFromSubmitList = roleSubmit.FunctionIds.Where(f => f == assignedFunctionId).FirstOrDefault();

                if(functionIdFromSubmitList != null)
                {
                    // Continue if the currently assigned function is within the role submit function IDs.
                    continue;
                }

                // If this portion of the execution is reached, we have a function this is currently assigned to the role. but no longer
                // appears within the newly declared associated functions list within the role submit. Capture a deletion of the currently aassigned function.
                var removedTransientRoleFunctionRecord = await CaptureRoleFunctionAssignmentChange(roleId, assignedFunctionId, capturedBy, "delete", roleSubmit.SubRealmId);
                CheckForAndProcessReleasedRoleFunctionTransientRecord(roleModel, removedTransientRoleFunctionRecord);
            }
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

            if(roleFunctionTransientModel.Action == "create")
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

        private async Task<RoleFunctionTransientModel> CaptureRoleFunctionAssignmentChange(Guid roleId, Guid functionId, Guid capturedBy, string action, Guid roleSubRealmId)
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
                throw new InvalidStateTransitionException(e.Message);
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

            if(roleToUpdate == null && roleTransientModel.Action != "create")
            {
                throw new ItemNotFoundException($"Role with ID '{roleTransientModel.RoleId}' not found when attempting to release role.");
            }

            if(roleTransientModel.Action == "modify")
            {
                await UpdateRoleWithCurrentTransientState(roleToUpdate, roleTransientModel);
                return roleToUpdate;
            }

            if(roleTransientModel.Action == "delete")
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

        private async Task<RoleTransientModel> CaptureTransientRoleAsync(Guid roleId, string roleName, string roleDescription, Guid subRealmId, string action, Guid createdById)
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
                throw new InvalidStateTransitionException(e.Message);
            }

            return await roleTransientRepository.CreateAsync(newTransientRole);
        }

        public async Task<RoleTransient> DeclineRole(Guid roleId, Guid approvedBy)
        {
            InitSharedTransaction();

            try
            {
                var latestTransientRole = await DeclineRoleTransientState(roleId, approvedBy);
                await FindTransientRoleFunctionsForRoleAndDeclineThem(roleId, approvedBy);
                await FindTransientChildRolesForRoleAndDeclineThem(roleId, approvedBy);

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
                await FindTransientRoleFunctionsForRoleAndApproveThem(role, roleId, approvedBy);
                await FindTransientChildRolesForRoleAndApproveThem(role, roleId, approvedBy);

                // It is possible that the assigned functions, roles or sub-realms state has changed. Update the model, but only if it has an ID.
                if (role.Id != Guid.Empty)
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

            try
            {
                latestTransientRole.Approve(approvedBy.ToString());
            }
            catch (Exception e)
            {
                throw new InvalidStateTransitionException(e.Message);
            }

            // Reset the Transient Role ID to force the creation of a new transient record.
            latestTransientRole.Id = Guid.Empty;

            return await roleTransientRepository.CreateAsync(latestTransientRole);
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
                throw new InvalidStateTransitionException(e.Message);
            }

            // Reset the Transient Role ID to force the creation of a new transient record.
            latestTransientRole.Id = Guid.Empty;

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

        public async Task<Role> UpdateAsync(RoleSubmit roleSubmit, Guid updatedById)
        {
            // Start transactions to allow complete rollback in case of an error
            InitSharedTransaction();

            try
            {
                // Note: The mapper will only map the basic first level members of the RoleSubmit to the Role.
                // The RoleSubmit contains a list of User UUIDs that will need to be found and converted into actual user representations.
                RoleModel role = await roleRepository.GetByIdAsync(roleSubmit.Uuid);

                if (role == null)
                    throw new ItemNotFoundException($"Role with ID '{roleSubmit.Uuid}' not found when attempting to update a role using this ID!");

                if (role.Name != roleSubmit.Name)
                {
                    // Confirm the new name is available
                    var checkExistingNameModel = await roleRepository.GetByNameAsync(roleSubmit.Name);
                    if (checkExistingNameModel != null)
                        throw new ItemNotProcessableException($"Role with name '{roleSubmit.Name}' already exists.");
                }

                role.Name = roleSubmit.Name;
                role.Description = roleSubmit.Description;

                await AssignFunctionsToRoleFromFunctionIdList(role, roleSubmit.FunctionIds);
                // Note: Sub-realm of a role cannot be changed once created. Hence the absence of a call to 'CheckForSubRealmAndAssignToRoleIfExists'.
                await AssignRolesToRoleFromRolesIdList(role, roleSubmit.RoleIds);

                // All successful
                CommitTransaction();

                return mapper.Map<Role>(await roleRepository.UpdateAsync(role));
            }
            catch
            {
                RollbackTransaction();
                throw;
            }
        }

        private async Task AssignFunctionsToRoleFromFunctionIdList(RoleModel role, List<Guid> functionIds)
        {
            // The user associations for this role are going to be created or overwritten, its easier to rebuild it that apply a diff.
            role.RoleFunctions = new List<RoleFunctionModel>();

            if (functionIds != null && functionIds.Count > 0)
            {
                foreach (var functionId in functionIds)
                {
                    var function = await functionRepository.GetByIdAsync(functionId);

                    if (function == null)
                    {
                        throw new ItemNotFoundException("Unable to find a function with ID: " + functionId + "when attempting to assign it to a role.");
                    }

                    ConfirmSubRealmAssociation(role.SubRealm == null ? Guid.Empty : role.SubRealm.Id, function);

                    role.RoleFunctions.Add(new RoleFunctionModel
                    {
                        Role = role,
                        Function = function
                    });
                }
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

        /// <summary>
        /// Assigns child roles to a role from a List of child role IDs. This methid will check that there is a legitimate role associated with each supplied
        /// Role ID within the list.
        /// </summary>
        /// <param name="roleModel"></param>
        /// <param name="roleIds"></param>
        /// <returns></returns>
        private async Task AssignRolesToRoleFromRolesIdList(RoleModel roleModel, List<Guid> roleIds)
        {
            // Child Roles are not mandatory. If the role IDs are null, return without resetting their state
            if (roleIds == null)
            {
                return;
            }

            // If the child roles element is set, reset the association list, even if there are no elements in it.
            roleModel.ChildRoles = new List<RoleRoleModel>();

            if (roleIds.Count == 0)
            {
                return;
            }

            foreach (var roleIdToAddAsChildRole in roleIds)
            {
                var roleToAddAsChildRole = await roleRepository.GetByIdAsync(roleIdToAddAsChildRole);

                if (roleToAddAsChildRole == null)
                {
                    throw new ItemNotFoundException($"Unable to find role with ID: '{roleIdToAddAsChildRole}' when attempting to assign this role as a child of role: '{roleModel.Name}'.");
                }

                // Only non-compound roles can be added to compound roles. Therefore, prior to adding the potential child role to the parent role, it must be
                // asserted that the child row has no child roles attached to it.
                if (roleToAddAsChildRole.ChildRoles.Count > 0)
                {
                    // Note. This function is called by create role and update role functions within this class. Therefore, the 'roleModel' object will not have an ID set if called from the create context. Use it's name.
                    throw new ItemNotProcessableException($"Assigning a compound role as a child of a role is prohibited. Attempting to add Role '{roleToAddAsChildRole.Name} with ID: '{roleToAddAsChildRole.Id}' as a child role of Role: '{roleModel.Name}'. However, it already has '{roleToAddAsChildRole.ChildRoles.Count}' child roles assigned to it! Not adding it.");
                }

                ConfirmSubRealmAssociation(roleModel.Id, roleToAddAsChildRole);

                roleModel.ChildRoles.Add(new RoleRoleModel
                {
                    ParentRole = roleModel,
                    ChildRole = roleToAddAsChildRole
                });
            }
        }

        private async Task CheckForSubRealmAndAssignToRoleIfExists(RoleModel role, RoleSubmit roleSubmit)
        {
            // Recall that submit models with empty GUIDs will not be null but rather Guid.Empty.
            if (roleSubmit.SubRealmId == null || roleSubmit.SubRealmId == Guid.Empty)
            {
                return;
            }

            var existingSubRealm = await subRealmRepository.GetByIdAsync(roleSubmit.SubRealmId, false);
            role.SubRealm = existingSubRealm ?? throw new ItemNotFoundException($"Sub-realm with ID '{roleSubmit.SubRealmId}' does not exist.");
        }

        public void InitSharedTransaction()
        {
            userRepository.InitSharedTransaction();
            roleRepository.InitSharedTransaction();
            functionRepository.InitSharedTransaction();
            subRealmRepository.InitSharedTransaction();
            roleTransientRepository.InitSharedTransaction();
        }

        public void CommitTransaction()
        {
            userRepository.CommitTransaction();
            roleRepository.CommitTransaction();
            functionRepository.CommitTransaction();
            subRealmRepository.CommitTransaction();
            roleTransientRepository.CommitTransaction();
        }

        public void RollbackTransaction()
        {
            userRepository.RollbackTransaction();
            roleRepository.RollbackTransaction();
            functionRepository.RollbackTransaction();
            subRealmRepository.RollbackTransaction();
            roleTransientRepository.RollbackTransaction();
        }

        public async Task<PaginatedResult<RoleModel>> GetPaginatedListAsync(int page, int pageSize, bool includeRelations, string filterName, List<KeyValuePair<string, string>> orderBy)
        {
            return await roleRepository.GetPaginatedListAsync(page, pageSize, includeRelations, filterName, orderBy);
        }
    }
}
