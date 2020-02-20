/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NLog;
using Stateless;

namespace za.co.grindrodbank.a3s.Models
{
    public abstract class TransientStateMachineRecord
    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        public enum DatabaseRecordState
        {
            Pending,
            Captured,
            Approved,
            Released,
            Declined
        }

        public enum DatabaseRecordTrigger
        {
            Pend,
            Capture,
            Approve,
            Decline,
            Release
        }

        private readonly StateMachine<DatabaseRecordState, DatabaseRecordTrigger> stateMachine;
        private readonly StateMachine<DatabaseRecordState, DatabaseRecordTrigger>.TriggerWithParameters<string> captureTrigger;
        private readonly StateMachine<DatabaseRecordState, DatabaseRecordTrigger>.TriggerWithParameters<string> pendTrigger;
        private readonly StateMachine<DatabaseRecordState, DatabaseRecordTrigger>.TriggerWithParameters<string> approveTrigger;
        private readonly StateMachine<DatabaseRecordState, DatabaseRecordTrigger>.TriggerWithParameters<string> declineTrigger;
        private readonly StateMachine<DatabaseRecordState, DatabaseRecordTrigger>.TriggerWithParameters<string> releaseTrigger;

        [Required]
        [Column(TypeName = "text")]
        public DatabaseRecordState R_State { get; set; }

        [Required]
        public string ChangedBy { get; set; }

        public int ApprovalCount { get; set; }

        private int RequiredApprovalCount { get; set; }

        public string Action { get; set; }

        //public abstract void UpdateRelations();

        public TransientStateMachineRecord()
        {
            // Instantiate a new state machine in the loaded state
            stateMachine = new StateMachine<DatabaseRecordState, DatabaseRecordTrigger>(() => R_State, s =>
             R_State = s);

            // Instantiate a new triggers with a parameters. 
            captureTrigger = stateMachine.SetTriggerParameters<string>(DatabaseRecordTrigger.Capture);
            pendTrigger = stateMachine.SetTriggerParameters<string>(DatabaseRecordTrigger.Pend);
            approveTrigger = stateMachine.SetTriggerParameters<string>(DatabaseRecordTrigger.Approve);
            declineTrigger = stateMachine.SetTriggerParameters<string>(DatabaseRecordTrigger.Decline);
            releaseTrigger = stateMachine.SetTriggerParameters<string>(DatabaseRecordTrigger.Release);

            // Configure the Pending state
            stateMachine.Configure(DatabaseRecordState.Pending)
                .OnEntryFrom(pendTrigger, OnPended)
                .Permit(DatabaseRecordTrigger.Capture, DatabaseRecordState.Captured);

            // Configure the Captured state
            stateMachine.Configure(DatabaseRecordState.Captured)
                .OnEntryFrom(captureTrigger, OnCaptured)
                .PermitReentry(DatabaseRecordTrigger.Capture)
                .Permit(DatabaseRecordTrigger.Pend, DatabaseRecordState.Pending)
                .Permit(DatabaseRecordTrigger.Approve, DatabaseRecordState.Approved)
                .Permit(DatabaseRecordTrigger.Decline, DatabaseRecordState.Declined);

            // Configure the Approved state
            stateMachine.Configure(DatabaseRecordState.Approved)
                .OnEntryFrom(approveTrigger, OnApproved)
                .PermitIf(DatabaseRecordTrigger.Release, DatabaseRecordState.Released, () => (ApprovalCount >= RequiredApprovalCount))
                .PermitReentry(DatabaseRecordTrigger.Approve)
                .Permit(DatabaseRecordTrigger.Decline, DatabaseRecordState.Declined)
                .Permit(DatabaseRecordTrigger.Capture, DatabaseRecordState.Captured)
                .Permit(DatabaseRecordTrigger.Pend, DatabaseRecordState.Pending);

            // Configure the Declined state
            stateMachine.Configure(DatabaseRecordState.Declined)
                .OnEntryFrom(declineTrigger, OnDeclined);

            // Configure the Released state
            stateMachine.Configure(DatabaseRecordState.Released)
                .OnEntryFrom(releaseTrigger, OnReleased)
                // Added this configuration to enable going from released to captured.
                .Permit(DatabaseRecordTrigger.Capture, DatabaseRecordState.Captured);

            // Set this from configuration later.
            RequiredApprovalCount = 2;
        }

        public void Capture(string capturer)
        {
            logger.Debug($"CAPTURE: with capturer: '{capturer}'");
            stateMachine.Fire(captureTrigger, capturer);
        }

        public void Pend(string pender)
        {
            logger.Debug($"PENDING: with pender: '{pender}'");
            stateMachine.Fire(pendTrigger, pender);
        }

        public void Approve(string approver)
        {
            logger.Debug($"APPROVE: with approver: '{approver}'");
            stateMachine.Fire(approveTrigger, approver);
        }

        public void Decline(string decliner)
        {
            logger.Debug($"DECLINE: with decliner: '{decliner}'");
            stateMachine.Fire(declineTrigger, decliner); ;
        }

        public void Release(string releaser)
        {
            logger.Debug($"RELEASE: with releaser: '{releaser}'");
            stateMachine.Fire(releaseTrigger, releaser);
        }

        public string GetState()
        {
            return $"\nState: {R_State}\nApprovalCount: {ApprovalCount}\n";
        }

        /// <summary>
        /// This method is called automatically when the Approved state is entered, but only when the trigger is approveTrigger.
        /// </summary>
        /// <param name="approver"></param>
        private void OnApproved(string approver)
        {
            logger.Debug($"ON APPROVED: with approver: '{approver}'");
            if (string.IsNullOrWhiteSpace(approver))
                throw new System.Exception("Approver must be specified when approving an application");


            // Perform a check prior to updated the approval count, as it may already be satisfied.
            // Consider the case where the required approval count is 0.
            if (ApprovalCount >= RequiredApprovalCount)
            {
                stateMachine.Fire(releaseTrigger, approver);
                return;
            }

            // Now increased the approval count and re-assess whether to transition to released.
            ChangedBy = approver;
            ApprovalCount++;

            if (ApprovalCount >= RequiredApprovalCount)
            {
                stateMachine.Fire(releaseTrigger, approver);
            }
        }

        /// <summary>
        /// This method is called automatically when the Pending state is entered, but only when the trigger is pendTrigger.
        /// </summary>
        /// <param name="pender"></param>
        private void OnPended(string pender)
        {
            logger.Debug($"ON PENDED: with pender: '{pender}'");
            if (string.IsNullOrWhiteSpace(pender))
                throw new System.Exception("Pender must be specified when pending an application");

            ChangedBy = pender;
            ApprovalCount = 0;
        }

        /// <summary>
        /// This method is called automatically when the Captured state is entered, but only when the trigger is captureTrigger.
        /// </summary>
        private void OnCaptured(string capturer)
        {
            logger.Debug($"ON CAPTURED: with capturerr: '{capturer}'");
            if (string.IsNullOrWhiteSpace(capturer))
                throw new System.Exception("Capturer must be specified when capturing an application");

            ChangedBy = capturer;
            ApprovalCount = 0;

            if (RequiredApprovalCount == 0)
            {
                stateMachine.Fire(approveTrigger, capturer);
            }
        }

        /// <summary>
        /// This method is called automatically when the Declined state is entered, but only when the trigger is declineTrigger.
        /// </summary>
        /// <param name="decliner"></param>
        private void OnDeclined(string decliner)
        {
            logger.Debug($"ON DECLINED: with decliner: '{decliner}'");
            if (string.IsNullOrWhiteSpace(decliner))
                throw new System.Exception("Decliner must be specified when declining an application");

            ChangedBy = decliner;
            ApprovalCount = 0;
        }

        /// <summary>
        /// This method is called automatically when the Released state is entered, but only when the trigger is releaseTrigger.
        /// </summary>
        /// <param name="releaser"></param>
        private void OnReleased(string releaser)
        {
            logger.Debug($"ON RELEASED: with releaser: '{releaser}'");
            if (string.IsNullOrWhiteSpace(releaser))
                throw new System.Exception("Releaser must be specified when releasing an application");

            ChangedBy = releaser;
        }

    }
}
