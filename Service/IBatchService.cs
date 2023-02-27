using AssessmentAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace AssessmentAPI.Service
{
    public interface IBatchService
    {
        /// <summary>
        /// Checks for business unit is appropriate
        /// </summary>
        /// <param name="businessUnit">Business Unit Name</param>
        /// <returns>Boolean</returns>
        bool ValidateBusinessUnit(string businessUnit);

        /// <summary>
        /// Create a new batch
        /// </summary>
        /// <param name="batchRequest">Batch Details</param>
        /// <returns></returns>
        Guid CreateBatch(BatchRequest batchRequest);

        /// <summary>
        /// Checks for Batch ID
        /// </summary>
        /// <param name="batchId">Batch ID</param>
        /// <returns>Status Codes</returns>
        int ValidateBatchID(Guid batchId);

        /// <summary>
        /// Gets the Batch Details
        /// </summary>
        /// <param name="batchId">Batch ID</param>
        /// <returns></returns>
        BatchDetailsResponse GetBatchDetails(Guid batchId);
    }
}
