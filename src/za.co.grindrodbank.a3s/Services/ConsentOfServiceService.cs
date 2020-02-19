/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using NLog;
using za.co.grindrodbank.a3s.Exceptions;
using za.co.grindrodbank.a3s.Helpers;
using za.co.grindrodbank.a3s.Models;
using za.co.grindrodbank.a3s.Repositories;

namespace za.co.grindrodbank.a3s.Services
{
    public class ConsentOfServiceService : IConsentOfServiceService
    {
        private readonly IConsentOfServiceRepository consentOfServiceRepository;
        private readonly IMapper mapper;
        private readonly IArchiveHelper archiveHelper;
        private static readonly ILogger currentClassLogger = LogManager.GetCurrentClassLogger();

        public ConsentOfServiceService(IConsentOfServiceRepository consentOfServiceRepository, IArchiveHelper archiveHelper, IMapper mapper)
        {
            this.consentOfServiceRepository = consentOfServiceRepository;
            this.archiveHelper = archiveHelper;
            this.mapper = mapper;
        }

        public async Task<ConsentOfService> GetCurrentConsentAsync()
        {
            var currentConsent = await consentOfServiceRepository.GetCurrentConsentAsync();
            return mapper.Map<ConsentOfService>(currentConsent);
        }

        public async Task<bool> UpdateCurrentConsentAsync(ConsentOfService consentOfService, Guid changedById)
        {
            var consentOfServiceModel = mapper.Map<ConsentOfServiceModel>(consentOfService);
            consentOfServiceModel.ChangedBy = changedById;
            var databaseObj = await consentOfServiceRepository.UpdateCurrentConsentAsync(consentOfServiceModel);
            return databaseObj != null;
        }

        private void ValidateFileCompatibility(byte[] fileContents)
        {
            List<string> archiveFiles; 

            try
            {
                archiveFiles = archiveHelper.ReturnFilesListInTarGz(fileContents, true);
            }
            catch (ArchiveException ex)
            {
                currentClassLogger.Error(ex);
                throw new ItemNotProcessableException("An archive error occurred during the validation of the consent file.");
            }
            catch (Exception ex)
            {
                currentClassLogger.Error(ex);
                throw new InvalidOperationException("A general error occurred during the validation of the consent file.");
            }
            
            if (!archiveFiles.Contains(A3SConstants.CONSENT_OF_SERVICE_CSS_FILE))
                throw new ItemNotProcessableException($"Consent file archive does not contain a '{A3SConstants.CONSENT_OF_SERVICE_CSS_FILE}' file.");
        }

        private string GetFileContents(byte[] content)
        {
            List<InMemoryFile> extractedFiles = archiveHelper.ExtractFilesFromTarGz(content);

            // Extract CSS File
            var cssFile =
                extractedFiles.FirstOrDefault(x => x.FileName.Trim() == A3SConstants.CONSENT_OF_SERVICE_CSS_FILE);

            return cssFile == null ? null : ByteArrayToString(cssFile.FileContents);
        }

        private string ByteArrayToString(byte[] inputBytes)
        {
            StringBuilder builder = new StringBuilder();

            foreach (var t in inputBytes)
                builder.Append(Convert.ToChar(t));

            return builder.ToString();
        }
    }
}
