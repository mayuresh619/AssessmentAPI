using AssessmentAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;

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
        /// <returns>Batch Id</returns>
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
        /// <returns>Batch Details</returns>
        BatchDetailsResponse GetBatchDetails(Guid batchId);

        /// <summary>
        /// Add file details to the batch
        /// </summary>
        /// <param name="batchId">Batch ID</param>
        /// <param name="fileName">Name of file</param>
        /// <param name="mimeType">MIME Type</param>
        /// <param name="fileSize">Size of file</param>
        /// <returns></returns>
        bool AddFileDetails(string batchId, string fileName, string mimeType, double fileSize);

        /// <summary>
        /// Creates Storage container
        /// </summary>
        /// <param name="containerName">Name of storage container</param>
        void CreateStorageContainer(string containerName);

        /// <summary>
        /// Uploads file to container
        /// </summary>
        /// <param name="batchId">Batch ID</param>
        /// <param name="fileName">Name of file</param>
        /// <returns>boolean</returns>
        bool UploadFileToContainer(string batchId, string fileName);

        /// <summary>
        /// Is File Name Valid
        /// </summary>
        /// <param name="fileName">Name of file</param>
        /// <returns>Boolean</returns>
        bool IsValidFilename(string fileName);

    }
}
