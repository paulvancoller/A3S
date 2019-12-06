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
using NLog;
using za.co.grindrodbank.a3s.A3SApiResources;
using System.Linq;

namespace za.co.grindrodbank.a3s.Services
{
    public class TeamService : ITeamService
    {
        private readonly ITeamRepository teamRepository;
        private readonly IApplicationDataPolicyRepository applicationDataPolicyRepository;
        private readonly ITermsOfServiceRepository termsOfServiceRepository;
        private readonly IMapper mapper;
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        public TeamService(ITeamRepository teamRepository, IApplicationDataPolicyRepository applicationDataPolicyRepository, ITermsOfServiceRepository termsOfServiceRepository, IMapper mapper)
        {
            this.teamRepository = teamRepository;
            this.applicationDataPolicyRepository = applicationDataPolicyRepository;
            this.termsOfServiceRepository = termsOfServiceRepository;
            this.mapper = mapper;
        }

        public async Task<Team> CreateAsync(TeamSubmit teamSubmit, Guid createdById)
        {
            // Start transactions to allow complete rollback in case of an error
            BeginAllTransactions();

            try
            {
                TeamModel existingTeam = await teamRepository.GetByNameAsync(teamSubmit.Name, false);
                if (existingTeam != null)
                    throw new ItemNotProcessableException($"Team with Name '{teamSubmit.Name}' already exist.");

                // This will only map the first level of members onto the model. User IDs and Policy IDs will not be.
                var teamModel = mapper.Map<TeamModel>(teamSubmit);
                teamModel.ChangedBy = createdById;

                await AssignTeamsToTeamFromTeamIdList(teamModel, teamSubmit.TeamIds);
                await AssignApplicationDataPoliciesToTeamFromDataPolicyIdList(teamModel, teamSubmit.DataPolicyIds);
                await ValidateTermsOfServiceEntry(teamModel.TermsOfServiceId);

                var createdTeam = mapper.Map<Team>(await teamRepository.CreateAsync(teamModel));

                // All successful
                CommitAllTransactions();

                return createdTeam;
            }
            catch
            {
                RollbackAllTransactions();
                throw;
            }
        }

        private async Task ValidateTermsOfServiceEntry(Guid? termsOfServiceId)
        {
            if (termsOfServiceId == null)
                return;

            TermsOfServiceModel existingTermsOfService = await termsOfServiceRepository.GetByIdAsync((Guid)termsOfServiceId, false);

            if (existingTermsOfService == null)
                throw new ItemNotFoundException($"TermsOfService entry with Id '{termsOfServiceId} not found.");
        }

        public async Task<Team> GetByIdAsync(Guid teamId, bool includeRelations = false)
        {
            return mapper.Map<Team>(await teamRepository.GetByIdAsync(teamId, includeRelations));
        }

        public async Task<List<Team>> GetListAsync()
        {
            return mapper.Map<List<Team>>(await teamRepository.GetListAsync());
        }

        public async Task<Team> UpdateAsync(TeamSubmit teamSubmit, Guid updatedById)
        {
            // Start transactions to allow complete rollback in case of an error
            BeginAllTransactions();

            try
            {
                TeamModel existingTeam = await teamRepository.GetByIdAsync(teamSubmit.Uuid, true);

                if (existingTeam == null)
                    throw new ItemNotFoundException($"Team with ID '{teamSubmit.Uuid}' not found when attempting to update a team using this ID!");

                if (existingTeam.Name != teamSubmit.Name)
                {
                    // Confirm the new name is available
                    var checkExistingNameModel = await teamRepository.GetByNameAsync(teamSubmit.Name, false);
                    if (checkExistingNameModel != null)
                        throw new ItemNotProcessableException($"Team with name '{teamSubmit.Name}' already exists.");
                }

                // Map the first level team submit attributes onto the team model.
                existingTeam.Name = teamSubmit.Name;
                existingTeam.Description = teamSubmit.Description;
                existingTeam.TermsOfServiceId = teamSubmit.TermsOfServiceId;
                existingTeam.ChangedBy = updatedById;

                await AssignTeamsToTeamFromTeamIdList(existingTeam, teamSubmit.TeamIds);
                await AssignApplicationDataPoliciesToTeamFromDataPolicyIdList(existingTeam, teamSubmit.DataPolicyIds);
                await ValidateTermsOfServiceEntry(existingTeam.TermsOfServiceId);

                // All successful
                CommitAllTransactions();

                return mapper.Map<Team>(await teamRepository.UpdateAsync(existingTeam));
            }
            catch
            {
                RollbackAllTransactions();
                throw;
            }
        }

        /// <summary>
        /// Assigns a list of Team IDs as child teams of a team that is passed to this function. This function will validate that each team exists by
        /// attempting to fetch it from the databse using the ID supplied in the list.
        /// </summary>
        /// <param name="teamModel"></param>
        /// <param name="teamIds"></param>
        /// <returns></returns>
        private async Task AssignTeamsToTeamFromTeamIdList(TeamModel teamModel, List<Guid> teamIds)
        {
            // It is not mandatory to have the teams set, so return here if the list is null.
            if (teamIds == null)
            {
                return;
            }

            teamModel.ChildTeams = new List<TeamTeamModel>();

            // If the list is set, but there are no elements in it, this is intepretted as re-setting the associated teams.
            if (teamIds.Count == 0)
            {
                return;
            }

            // Before adding any child teams to this team, ensure that is does not contain users, as compound teams with users are prohibited.
            if (teamModel.UserTeams != null && teamModel.UserTeams.Any())
            {
                throw new ItemNotProcessableException($"Attempting to assign child teams to team '{teamModel.Name}', but it has users in it! Cannot create a compound team with users!");
            }

            foreach (var childTeamId in teamIds)
            {
                // It is imperative to fetch the child teams with their relations, as potential child teams are assessed for the child team below.
                var teamToAddAsChild = await teamRepository.GetByIdAsync(childTeamId, true);

                if (teamToAddAsChild == null)
                {
                    throw new ItemNotFoundException($"Unable to find existing team by ID: '{childTeamId}', when attempting to assign that team to existing team: '{teamModel.Name}' as a child team.");
                }

                //Teams can only be added to a team as a child if it has no children of it's own. This prevents having compound teams that contain child compound teams.
                if (teamToAddAsChild.ChildTeams.Count > 0)
                {
                    // Note: 'teamModel' may not have an ID as this function is potentially called from the createAsync function prior to persisting the team into the database. Use it's name when referencing it for safety.
                    throw new ItemNotProcessableException($"Adding compound team as child of a team is prohibited. Attempting to add team with name: '{teamToAddAsChild.Name}' and ID: '{teamToAddAsChild.Id}' as a child team of team with name: '{teamModel.Name}'. However it already has '{teamToAddAsChild.ChildTeams.Count}' child teams of its own.");
                }

                teamModel.ChildTeams.Add(new TeamTeamModel
                {
                    ParentTeam = teamModel,
                    ChildTeam = teamToAddAsChild
                });
            }
        }

        /// <summary>
        /// Assigns a list of a application data policies to the team given a list of application data policy IDs. This function will verify that there is a legitimate
        /// application data poliy associated with each ID before adding it.
        /// </summary>
        /// <param name="team"></param>
        /// <param name="applicationDataPolicyIds"></param>
        /// <returns></returns>
        private async Task<TeamModel> AssignApplicationDataPoliciesToTeamFromDataPolicyIdList(TeamModel team, List<Guid> applicationDataPolicyIds)
        {
            if (applicationDataPolicyIds == null)
            {
                return team;
            }

            team.ApplicationDataPolicies = new List<TeamApplicationDataPolicyModel>();

            // If the list is set, but there are no elements in it, this is intepretted as re-setting the associated application data policies.
            if (applicationDataPolicyIds.Count == 0)
            {
                return team;
            }

            foreach (var applicationDataPolicyId in applicationDataPolicyIds)
            {
                var applicationDataPolicyToAdd = await applicationDataPolicyRepository.GetByIdAsync(applicationDataPolicyId);

                if (applicationDataPolicyToAdd == null)
                {
                    throw new ItemNotFoundException($"Unable to find Application Data Policy with ID '{applicationDataPolicyId}' when attempting to assign it to team '{team.Name}'.");
                }

                team.ApplicationDataPolicies.Add(new TeamApplicationDataPolicyModel
                {
                    Team = team,
                    ApplicationDataPolicy = applicationDataPolicyToAdd
                });
            }

            return team;
        }

        public async Task<List<Team>> GetListAsync(Guid teamMemberUserGuid)
        {
            return mapper.Map<List<Team>>(await teamRepository.GetListAsync(teamMemberUserGuid));
        }

        private void BeginAllTransactions()
        {
            teamRepository.InitSharedTransaction();
            applicationDataPolicyRepository.InitSharedTransaction();
        }

        private void CommitAllTransactions()
        {
            teamRepository.CommitTransaction();
            applicationDataPolicyRepository.CommitTransaction();
        }

        private void RollbackAllTransactions()
        {
            teamRepository.RollbackTransaction();
            applicationDataPolicyRepository.RollbackTransaction();
        }
    }
}
