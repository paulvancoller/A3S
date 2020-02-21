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

namespace za.co.grindrodbank.a3s.Services
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository roleRepository;
        private readonly IUserRepository userRepository;
        private readonly IFunctionRepository functionRepository;
        private readonly ISubRealmRepository subRealmRepository;
        private readonly IRoleTransientRepository roleTransientRepository;
        private readonly IMapper mapper;

        public RoleService(IRoleRepository roleRepository, IUserRepository userRepository, IFunctionRepository functionRepository, ISubRealmRepository subRealmRepository, IRoleTransientRepository roleTransientRepository, IMapper mapper)
        {
            this.roleRepository = roleRepository;
            this.userRepository = userRepository;
            this.functionRepository = functionRepository;
            this.subRealmRepository = subRealmRepository;
            this.roleTransientRepository = roleTransientRepository;
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

                RoleTransientModel newTransientRole = await CaptureNewTransientRoleFromRoleSubmitAsync(roleSubmit, createdById);
                // Even though we are creating/capturing the role here, it is possible that the configured approval count is 0,
                // which means that we need to check for whether the transient state is released, and process the affected role accrodingly.
                await UpdateRoleBasedOnTransientActionIfTransientRoleStateIsReleased(newTransientRole);

                // Note: The mapper will only map the basic first level members of the RoleSubmit to the Role.
                // The RoleSubmit contains a list of User UUIDs that will need to be found and converted into actual user representations.
                //RoleModel newRole = mapper.Map<RoleModel>(roleSubmit);
                ///newRole.ChangedBy = createdById;

                // The potentially assigned sub-realm is used within the 'AssignFunctionsToRoleFromFunctionIdList' function, so perform sub-realm assignmentd first.
                //await CheckForSubRealmAndAssignToRoleIfExists(newRole, roleSubmit);
                //await AssignFunctionsToRoleFromFunctionIdList(newRole, roleSubmit.FunctionIds);
                //await AssignRolesToRoleFromRolesIdList(newRole, roleSubmit.RoleIds);

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

        private async Task UpdateRoleBasedOnTransientActionIfTransientRoleStateIsReleased(RoleTransientModel roleTransientModel)
        {
            if(roleTransientModel.R_State != TransientStateMachineRecord.DatabaseRecordState.Released)
            {
                return;
            }

            RoleModel roleToUpdate = new RoleModel();

            if (roleTransientModel.Action != "create")
            {
                roleToUpdate = await roleRepository.GetByIdAsync(roleTransientModel.RoleId);

                if(roleToUpdate == null)
                {
                    throw new ItemNotFoundException($"Role with ID '{roleTransientModel.RoleId}' not found when attempting to release role.");
                }
            }

            if(roleTransientModel.Action == "modify")
            {
                await UpdateRoleWithCurrentTransientState(roleToUpdate, roleTransientModel);
                return;
            }

            if(roleTransientModel.Action == "delete")
            {
                await roleRepository.DeleteAsync(roleToUpdate);
                return;
            }

            await CreateRoleFromCurrentTransientState(roleTransientModel);
        }

        private async Task<RoleModel> CreateRoleFromCurrentTransientState(RoleTransientModel transientRole)
        {
            RoleModel roleToCreate = new RoleModel
            {
                Name = transientRole.Name,
                Description = transientRole.Description,
                Id = transientRole.RoleId
            };

            return await roleRepository.CreateAsync(roleToCreate);
        }

        private async Task UpdateRoleWithCurrentTransientState(RoleModel roleToRelease, RoleTransientModel transientRole)
        {
            roleToRelease.Name = transientRole.Name;
            roleToRelease.Description = transientRole.Description;

            await roleRepository.UpdateAsync(roleToRelease);
        }

        private async Task<RoleTransientModel> CaptureNewTransientRoleFromRoleSubmitAsync(RoleSubmit roleSubmit, Guid createdById)
        {
            RoleTransientModel newTransientRole = new RoleTransientModel
            {
                Action = "create",
                ChangedBy = createdById,
                ApprovalCount = 0,
                // Pending is the initial state of the state machine for all transient records.
                R_State = TransientStateMachineRecord.DatabaseRecordState.Pending,
                Name = roleSubmit.Name,
                Description = roleSubmit.Description,
                RoleId = Guid.NewGuid()
            };

            newTransientRole.Capture(createdById.ToString());
            return await roleTransientRepository.CreateAsync(newTransientRole);
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

                    ConfirmSubRealmAssociation(role, function);

                    role.RoleFunctions.Add(new RoleFunctionModel
                    {
                        Role = role,
                        Function = function
                    });
                }
            }
        }

        private void ConfirmSubRealmAssociation(RoleModel role, FunctionModel function)
        {
            // If there is a Sub-Realm associated with role, we must ensure that the function we are attempting to add to the role is associated with the same sub realm.
            if (role.SubRealm != null)
            {
                if (function.SubRealm == null || function.SubRealm.Id != role.SubRealm.Id)
                {
                    throw new ItemNotProcessableException($"Attempting to add a function with ID '{function.Id}' to a role within the '{role.SubRealm.Name}' sub-realm but the function does not exist within that sub-realm.");
                }
            }
            else
            {
                if (function.SubRealm != null)
                {
                    throw new ItemNotProcessableException($"Attempting to add a function with ID '{function.Id}' to a role within the '{role.SubRealm.Name}' sub-realm but the function does not exist within that sub-realm.");
                }
            }
        }

        private void ConfirmSubRealmAssociation(RoleModel roleModel, RoleModel roleToAddAsChildRole)
        {
            // If there is a Sub-Realm associated with role, we must ensure that the child role we are attempting to add to the role is associated with the same sub realm.
            if (roleModel.SubRealm != null)
            {
                if (roleToAddAsChildRole.SubRealm == null || roleModel.SubRealm.Id != roleToAddAsChildRole.SubRealm.Id)
                {
                    throw new ItemNotProcessableException($"Attempting to add a role with ID '{roleToAddAsChildRole.Id}' as a child role of role with ID '{roleModel.Id}' but the roles are not within the same sub-realm.");
                }
            }
            else
            {
                if (roleToAddAsChildRole.SubRealm != null)
                {
                    throw new ItemNotProcessableException($"Attempting to add a role with ID '{roleToAddAsChildRole.Id}' as a child role of role with ID '{roleModel.Id}' but the roles are not within the same sub-realm.");
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

                ConfirmSubRealmAssociation(roleModel, roleToAddAsChildRole);

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
