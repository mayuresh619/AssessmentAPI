using AssessmentAPI.Controllers;
using AssessmentAPI.DataLayer;
using AssessmentAPI.Logger;
using AssessmentAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.NetworkInformation;
using static AssessmentAPI.Models.BatchDetailsResponse;
using static AssessmentAPI.Models.BatchRequest;

namespace AssessmentAPI.Service
{
    public class BatchService : IBatchService
    {

        #region Private Variables
        private readonly AssessmentDBContext _context;
        private readonly IConfiguration _config;
        private readonly ILogger _logger;
        private Guid _guid;
        private Batch _batch;
        private Attributes _attributes;

        #endregion

        #region Constructor

        public BatchService(IConfiguration configuration, ILogger logger)
        {
            _config = configuration;
            _logger = logger;
            _context = new AssessmentDBContext(_config);
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Checks for business unit is appropriate
        /// </summary>
        /// <param name="businessUnit">Business Unit Name</param>
        /// <returns>Boolean</returns>
        public bool ValidateBusinessUnit(string businessUnit)
        {
            try
            {
                _logger.LogInfo($"Execution of {nameof(BatchService)}.{nameof(ValidateBusinessUnit)} method begin",
                    ("Business Unit Name", businessUnit)
                    );
                var _businessUnits = _context.BusinessUnits.Where(auth => auth.BusinessUnitName == businessUnit).ToList();

                if (_businessUnits.Count() <= 0)
                {
                    _logger.LogInfo($"No Business Unit found with name {businessUnit}",
                        ("Method Name",$"{nameof(BatchService)}.{nameof(ValidateBusinessUnit)}"));
                    return false;
                }
                _logger.LogInfo($"Execution of {nameof(BatchService)}.{nameof(ValidateBusinessUnit)} method end",
                    ("Business Unit Response", JsonConvert.SerializeObject(_businessUnits))
                    );
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception occured in {nameof(BatchService)}.{nameof(ValidateBusinessUnit)}",
                    ("Business Unit Name", businessUnit)
                    );
                throw ex;
            }
        }

        /// <summary>
        /// Create a new batch
        /// </summary>
        /// <param name="batchRequest">Batch Details</param>
        /// <returns>Batch Id</returns>
        public Guid CreateBatch(BatchRequest batchRequest)
        {
            try
            {
                _logger.LogInfo($"Execution of {nameof(BatchService)}.{nameof(CreateBatch)} method begin",
                     ("Request object", JsonConvert.SerializeObject(batchRequest))
                     );
                var _businessUnits = _context.BusinessUnits.Where(auth => auth.BusinessUnitName == batchRequest.BusinessUnit).ToList();
                var users = _context.Users.Where(a => batchRequest.Acl1.ReadUsers.Contains(a.UserName)).ToList();
                var groups = _context.Groups.Where(a => batchRequest.Acl1.ReadGroups.Contains(a.GroupName)).ToList();
                _guid = Guid.NewGuid();
                _batch = new Batch();
                _batch.BatchId = _guid.ToString();
                _batch.Status = "Incomplete";
                _batch.BatchPublishedDate = DateTime.Now;
                _batch.ExpiryDate = batchRequest.ExpiryDate;
                _batch.BusinessUnitId = _businessUnits.FirstOrDefault().BusinessUnitId;
                _batch.ReadUsers = String.Join(",", users.Select(x => x.UserId.ToString()).ToArray());
                _batch.ReadGroups = String.Join(",", groups.Select(x => x.GroupId.ToString()).ToArray());
                _context.Batch.Add(_batch);

                if (batchRequest.Attritubes != null)
                {
                    foreach (var attribute in batchRequest.Attritubes)
                    {
                        _attributes = new Attributes();
                        _attributes.Key = attribute.Key;
                        _attributes.Value = attribute.Value;
                        _attributes.BatchId = _batch.BatchId;
                        _context.Attributes.Add(_attributes);
                    } 
                }

                _context.SaveChanges();
                _logger.LogInfo($"Execution of {nameof(BatchService)}.{nameof(CreateBatch)} method end",
                     ("Batch ID", _guid.ToString())
                     );
                return _guid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception occured in {nameof(BatchService)}.{nameof(CreateBatch)}",
                     ("Request object", JsonConvert.SerializeObject(batchRequest))
                    );
                throw ex;
            }
        }

        /// <summary>
        /// Checks for Batch ID
        /// </summary>
        /// <param name="batchId">Batch ID</param>
        /// <returns>Boolean</returns>
        public int ValidateBatchID(Guid batchId)
        {
            try
            {
                _logger.LogInfo($"Execution of {nameof(BatchService)}.{nameof(ValidateBatchID)} method begin",
                     ("Batch Id", batchId.ToString())
                     );
                var _batchDetails = _context.Batch.Where(batch => batch.BatchId == batchId.ToString()).ToList();

                if (_batchDetails.Count() <= 0)
                {
                    return StatusCodes.Status404NotFound;
                }
                if(_batchDetails.FirstOrDefault().ExpiryDate < DateTime.Now)
                {
                    _logger.LogWarning($"Batch is expired",
                       ("Batch Id", batchId.ToString()),
                       ("Method Name", $"{nameof(BatchService)}.{nameof(ValidateBatchID)}"),
                       ("Batch Expiry Date", _batchDetails.FirstOrDefault().ExpiryDate.Value.ToLongDateString()),
                       ("Status Code", StatusCodes.Status410Gone.ToString())
                       );
                    return StatusCodes.Status410Gone;
                }

                _logger.LogInfo($"Execution of {nameof(BatchService)}.{nameof(ValidateBatchID)} method end",
                    ("Batch Id", batchId.ToString()),
                    ("Status Code", StatusCodes.Status200OK.ToString())
                    );
                return StatusCodes.Status200OK;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception occured in {nameof(BatchService)}.{nameof(ValidateBatchID)}",
                    ("Batch Id", batchId.ToString())
                    );
                throw ex;
            }

        }

        /// <summary>
        /// Gets the Batch Details
        /// </summary>
        /// <param name="batchId">Batch ID</param>
        /// <returns></returns>
        public BatchDetailsResponse GetBatchDetails(Guid batchId)
        {
            try
            {
                _logger.LogInfo($"Execution of {nameof(BatchService)}.{nameof(GetBatchDetails)} method begin",
                     ("Batch Id", batchId.ToString())
                     );
                var _batchDetailsResponse = new BatchDetailsResponse();
                var _batchAttributes = _context.Attributes.Where(attr => attr.BatchId == batchId.ToString()).ToList();
                var _batchDetails = _context.Batch.Where(batch => batch.BatchId == batchId.ToString()).ToList().FirstOrDefault();
                var _businessUnitDetails = _context.BusinessUnits.Where(budetails => budetails.BusinessUnitId == _batchDetails.BusinessUnitId).ToList().FirstOrDefault();
                var _fileDetails = _context.Files.Where(file => file.BatchId == _batchDetails.BatchId).ToList();
                _batchDetailsResponse.BatchId = _batchDetails.BatchId;
                _batchDetailsResponse.Status = _batchDetails.Status;
                _batchDetailsResponse.Attritube = _batchAttributes.Select(x => new Attritube1() {
                    Key= x.Key,
                    Value= x.Value
                }).ToArray();
                _batchDetailsResponse.BusinessUnit = _businessUnitDetails.BusinessUnitName;
                _batchDetailsResponse.BatchPublishedDate = _batchDetails.BatchPublishedDate.Value;
                _batchDetailsResponse.ExpiryDate = _batchDetails.ExpiryDate.Value;
                _batchDetailsResponse.Files = _fileDetails.Select(x => new File()
                {
                    FileName = x.FileName,
                    Attritube = (_context.Attributes.Where(attr => attr.FileId.Value == x.FileId).ToList()).Select(a=> new Attritube1()
                    {
                        Key = a.Key,
                        Value = a.Value
                    }).ToArray(),
                    FileSize = x.FileSize.Value,
                    Hash = x.Hash,
                    Links= x.Links,
                    MimeType = x.Mimetype
                }
                ).ToArray();

                _logger.LogInfo($"Execution of {nameof(BatchService)}.{nameof(GetBatchDetails)} method end",
                     ("Batch Id", _batchDetails.BatchId)
                     );
                return _batchDetailsResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception occured in {nameof(BatchService)}.{nameof(GetBatchDetails)}",
                    ("Batch Id", batchId.ToString())
                    );
                throw ex;
            }
        }


        #endregion
    }
}
